using Newtonsoft.Json;
using System;

namespace MemoryPatchPlugin.json
{
    class CodeDefinition
    {
        // optional - where to put the new code
        // For now the only supported option is "overwrite", which will
        // write the new bytes directly on top of existing memory (so it
        // is necessary to match lengths)
        public string Location { get; set; } = "overwrite";

        // the new assembly to write when this patch is enabled
        private string byteString;
        [JsonProperty("bytes", Required = Required.Always)]
        public string ByteString
        {
            get
            {
                return byteString;
            }
            set
            {
                byteString = value;
                Bytes = PatchDefinition.StringToBytes(byteString);
            }
        }

        internal byte[] Bytes { get; private set; }
    }
}
