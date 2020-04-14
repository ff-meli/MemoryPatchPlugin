using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace MemoryPatchPlugin.Configuration
{
    [Serializable]
    class PluginConfiguration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<PatchMetaData> Patches { get; set; } = new List<PatchMetaData>();
    }
}
