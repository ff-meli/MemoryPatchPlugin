using Newtonsoft.Json;
using System;

namespace MemoryPatchPlugin.json
{
    class ValidationDefinition
    {
        // optional - the string inside ffxivgame.ver
        // used to restrict a patch to a particular exe version
        public string ExeVersion { get; set; }

        // optional - the bytes expected to be at the patch location before modifiction
        // This supports ?? or ** wildcards
        private string requiredBytesString;
        [JsonProperty("requiredBytes")]
        public string RequiredBytesString
        {
            get
            {
                return requiredBytesString;
            }
            set
            {
                requiredBytesString = value;
                RequiredBytes = PatchDefinition.PatternToBytes(requiredBytesString);
            }
        }

        internal byte?[] RequiredBytes { get; private set; }
    }
}
