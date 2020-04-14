using Dalamud.Plugin;
using MemoryPatchPlugin.json;
using System;

namespace MemoryPatchPlugin
{
    // Low-level memory patch
    // This class does no validation and assumes it is being used in a valid context
    class Patch
    {
        public IntPtr Address { get; set; }
        public byte[] Bytes { get; set; }
        public byte[] OriginalBytes { get; private set; }
        public bool IsActive { get; private set; } = false;
        // This isn't the best place for this, but it's simplest to couple these explicity
        // and we ultimately want to work on Patch objects, so we include this here to
        // keep metadata around
        public PatchDefinition Definition { get; set; }

        public void Enable()
        {
            if (!IsActive)
            {
                OriginalBytes = Memory.Read(Address, Bytes.Length);
                Memory.Write(Address, Bytes);

                IsActive = true;

                PluginLog.Log($"Enabled patch {Definition.Name}");
            }
        }

        public void Disable()
        {
            if (IsActive)
            {
                Memory.Write(Address, OriginalBytes);
                OriginalBytes = null;

                IsActive = false;

                PluginLog.Log($"Disabled patch {Definition.Name}");
            }
        }
    }
}
