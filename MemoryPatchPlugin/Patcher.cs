using Dalamud.Game;
using Dalamud.Plugin;
using MemoryPatchPlugin.json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MemoryPatchPlugin
{
    class Patcher : IDisposable
    {
        public List<Patch> Patches { get; } = new List<Patch>();

        public string PatchDirectory { get; private set; }

        private SigScanner scanner;

        public Patcher(string patchDir, SigScanner scanner)
        {
            PatchDirectory = patchDir;
            this.scanner = scanner;

            LoadPatches();
        }

        public void Dispose()
        {
            foreach (var patch in Patches)
            {
                patch.Disable();    // this could also be done with Dispose
            }
            Patches.Clear();
        }

        public void ReloadPatches()
        {
            foreach (var patch in Patches)
            {
                patch.Disable();
            }
            Patches.Clear();

            LoadPatches();
        }

        private void LoadPatches()
        {
            foreach (var patchFile in Directory.GetFiles(PatchDirectory, "*.json"))
            {
                try
                {
                    var patchDef = JsonConvert.DeserializeObject<PatchDefinition>(File.ReadAllText(patchFile));
                    var patch = LoadPatch(patchDef);
                    Patches.Add(patch);
                }
                catch (Exception e)
                {
                    PluginLog.LogError(e, $"Failed to load patch from {patchFile}");
                }
            }
        }

        private Patch LoadPatch(PatchDefinition patchDef)
        {
            PluginLog.Log($"Loading patch {patchDef.Name}");

            var address = ResolveAddress(patchDef);
            ValidatePatch(patchDef, address);

            return new Patch()
            {
                Address = address,
                Bytes = patchDef.Patch.Bytes,
                Definition = patchDef
            };
        }

        private IntPtr ResolveAddress(PatchDefinition patchDef)
        {
            var address = scanner.Module.BaseAddress;

            // if we have a signature, use that as the base instead of the module base
            if (!string.IsNullOrEmpty(patchDef.Location.Signature))
            {
                // TODO: do we need to handle .data too?
                address = scanner.ScanText(patchDef.Location.Signature);

                // ensure that there is only one instance found
                try
                {
                    var offset = (int)(address.ToInt64() - scanner.TextSectionBase.ToInt64());
                    var dupeAddr = scanner.Scan(address + 1, scanner.TextSectionSize - offset, patchDef.Location.Signature);
                    // if we get here, there is a duplicate
                    throw new ArgumentException("Signatures specified in Location.Signature must resolve to a unique address");
                }
                catch (KeyNotFoundException)
                {
                    // Scan() throws this if not found, so getting here means we are ok
                }
            }

            // if there is an offset, add it to the current base
            if (!string.IsNullOrEmpty(patchDef.Location.Offset))
            {
                address += int.Parse(patchDef.Location.Offset, NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier);
            }

            if (address == scanner.Module.BaseAddress)
            {
                throw new ArgumentException("At least one of Location.Signature and Location.Offset must be specified");
            }

            return address;
        }

        private void ValidatePatch(PatchDefinition patchDef, IntPtr address)
        {
            // validate that we can at least read the specified location
            // we could (should) virtualquery here instead to check read/write, but this is lazier
            Memory.Read(address, 1);

            // TODO: exe version, once exposed

            if (patchDef.Validation?.RequiredBytes != null)
            {
                var actualBytes = Memory.Read(address, patchDef.Validation.RequiredBytes.Length);
                for (var i = 0; i < actualBytes.Length; i++)
                {
                    // null bytes are wildcards, so just skip over them
                    if (patchDef.Validation.RequiredBytes[i] != null && patchDef.Validation.RequiredBytes[i] != actualBytes[i])
                    {
                        throw new InvalidOperationException(string.Format("Expected bytes at patch address {0} do not match.  Expected [{1}], found [{2}]",
                            address,
                            patchDef.Validation.RequiredBytesString,
                            BitConverter.ToString(actualBytes).Replace("-", " ")));
                    }
                }
            }

            // TODO: this should probably go somewhere else, but it's not really useful at the moment anyway
            if (patchDef.Patch.Location.ToLowerInvariant() != "overwrite")
            {
                PluginLog.Log("[WARN] Patch.Location only supports \"overwrite\" for now, defaulting to that behavior");
            }

            if (this.Patches.Any(p => p.Definition.Name == patchDef.Name))
            {
                throw new ArgumentException($"A patch with name {patchDef.Name} already exists");
            }
        }
    }
}
