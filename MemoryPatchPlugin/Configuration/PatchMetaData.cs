using System;

namespace MemoryPatchPlugin.Configuration
{
    class PatchMetaData
    {
        public string Name { get; set; }
        public bool EnableOnStartup { get; set; }
        public bool DisableOnUnload { get; set; } = true;
    }
}
