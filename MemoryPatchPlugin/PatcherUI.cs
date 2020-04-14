using Dalamud.Plugin;
using ImGuiNET;
using MemoryPatchPlugin.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace MemoryPatchPlugin
{
    class PatcherUI
    {
        private bool visible = false;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        // not great to need all these passed in
        private Patcher patcher;
        private PluginConfiguration configuration;
        private DalamudPluginInterface pluginInterface;
        private Plugin plugin;

        public PatcherUI(Patcher patcher, PluginConfiguration configuration, DalamudPluginInterface pluginInterface, Plugin plugin)
        {
            this.patcher = patcher;
            this.configuration = configuration;
            this.pluginInterface = pluginInterface;
            this.plugin = plugin;
        }

        public void Draw()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(430, 478), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Memory Patcher", ref this.visible, ImGuiWindowFlags.NoResize))
            {
                ImGui.BeginChild("##wrapper", new Vector2(0, 405));

                foreach (var patch in this.patcher.Patches)
                {
                    if (ImGui.CollapsingHeader(patch.Definition.Name))
                    {
                        ImGui.Indent();

                        if (!string.IsNullOrEmpty(patch.Definition.Description))
                        {
                            ImGui.TextWrapped(patch.Definition.Description);
                        }

                        bool isActive = patch.IsActive;
                        if (ImGui.Checkbox("Enabled##" + patch.Definition.Name, ref isActive))
                        {
                            if (isActive)
                            {
                                patch.Enable();
                            }
                            else
                            {
                                patch.Disable();
                            }
                        }

                        var configData = this.configuration.Patches.FirstOrDefault(p => p.Name == patch.Definition.Name);

                        var enabledAtStartup = configData?.EnableOnStartup ?? false;
                        if (ImGui.Checkbox("Enabled at startup##" + patch.Definition.Name, ref enabledAtStartup))
                        {
                            // TODO: may just want to create entries for every loaded patch regardless
                            // This is already a bit weird, and could get messy if we add more settings
                            if (configData == null)
                            {
                                configData = new PatchMetaData()
                                {
                                    Name = patch.Definition.Name,
                                };
                                this.configuration.Patches.Add(configData);
                            }

                            configData.EnableOnStartup = enabledAtStartup;
                            this.pluginInterface.SavePluginConfig(this.configuration);
                        }

                        ImGui.Unindent();
                    }
                }
                ImGui.EndChild();

                ImGui.Separator();

                ImGui.SetCursorPos(new Vector2(203, 447));
                if (ImGui.Button("Open patch folder"))
                {
                    Process.Start(this.patcher.PatchDirectory);
                }
                ImGui.SameLine();

                if (ImGui.Button("Reload patches"))
                {
                    if (this.patcher.Patches.Any(p => p.IsActive))
                    {
                        ImGui.OpenPopup("Reload patches?");
                    }
                    else
                    {
                        // TODO: might want to do this in a task
                        this.plugin.ReloadPatches();
                    }
                }

                bool dummy = true;
                if (ImGui.BeginPopupModal("Reload patches?", ref dummy, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("You have active patches.\nReloading will disable them all (and re-enable according to Enable At Startup).");
                    ImGui.Spacing();
                    ImGui.Text("Are you sure?");
                    ImGui.Spacing();
                    ImGui.SetCursorPosX(200);
                    if (ImGui.Button("Cancel", new Vector2(120, 40)))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("OK", new Vector2(120, 40)))
                    {
                        // TODO: might want to do this in a task
                        this.plugin.ReloadPatches();
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.End();
        }
    }
}
