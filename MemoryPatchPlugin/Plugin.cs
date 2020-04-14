using Dalamud.Game.Command;
using Dalamud.Plugin;
using MemoryPatchPlugin.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MemoryPatchPlugin
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Memory Patch Plugin";

        private const string command = "/ppatch";

        private DalamudPluginInterface pi;
        private PluginConfiguration configuration;
        private Patcher patcher;
        private PatcherUI ui;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            this.configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            pluginInterface.CommandManager.AddHandler(command, new CommandInfo(OnDisplayCommand)
            {
                HelpMessage = "Displays the memory patcher window.  Do not use this if you don't know what it is for.",
                ShowInHelp = false
            });

            // Bit of a hack, but I want these persistent outside of the install directory, but also not in the config itself
            var patchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XivLauncher", "pluginConfigs", "memoryPatcher");
            if (!Directory.Exists(patchPath))
            {
                Directory.CreateDirectory(patchPath);
                // this may go away, but for now install the base patch(es) for people to use and test with
                InstallDefaultPatches(patchPath);
            }

            this.patcher = new Patcher(patchPath, pluginInterface.TargetModuleScanner);

            EnableAutoLoadPatches();

            // let's just pass the entire world in here... :(
            this.ui = new PatcherUI(this.patcher, this.configuration, this.pi, this);

            this.pi.UiBuilder.OnBuildUi += Display;
            this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => OnDisplayCommand(command, "");
        }

        public void Dispose()
        {
            this.pi.UiBuilder.OnBuildUi -= Display;
            this.pi.CommandManager.RemoveHandler(command);

            this.patcher.Dispose();

            this.pi.Dispose();
        }

        public void ReloadPatches()
        {
            this.patcher.ReloadPatches();
            EnableAutoLoadPatches();
        }

        private void InstallDefaultPatches(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var patch in assembly.GetManifestResourceNames())
            {
                if (!patch.EndsWith(".json"))
                {
                    continue;
                }

                using (var stream = assembly.GetManifestResourceStream(patch))
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();

                    // strip off the namespace
                    var filename = patch.Substring(patch.IndexOf('.') + 1);
                    File.WriteAllText(Path.Combine(path, filename), text);
                }
            }
        }

        private void EnableAutoLoadPatches()
        {
            foreach (var patch in this.configuration.Patches)
            {
                if (patch.EnableOnStartup)
                {
                    patcher.Patches.FirstOrDefault(p => p.Definition.Name == patch.Name)?.Enable();
                }
            }
        }

        private void OnDisplayCommand(string command, string args)
        {
            this.ui.Visible = true;
        }

        private void Display()
        {
            this.ui.Draw();
        }
    }
}
