using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MemoryPatchPlugin.json
{
    class PatchDefinition
    {
        // The *unique* name of this patch; for display and configuration
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        // optional - displayed in the UI if present
        public string Description { get; set; }

        // Where this code patch should be applied
        [JsonProperty(Required = Required.Always)]
        public LocationDefinition Location { get; set; }

        // optional - Additional checks to ensure this code patch is able to
        // apply in the correct location
        public ValidationDefinition Validation { get; set; }

        // The code patch itself
        [JsonProperty(Required = Required.Always)]
        public CodeDefinition Patch { get; set; }


        // Stolen from dalamud's sigscanner module
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte?[] PatternToBytes(string pattern)
        {
            pattern = pattern.Replace(" ", "");

            if (pattern.Length % 2 != 0)
                throw new ArgumentException("Byte pattern length without whitespace must be divisible by two.",
                                            nameof(pattern));

            var byteLength = pattern.Length / 2;
            var bytes = new byte?[byteLength];

            for (var i = 0; i < byteLength; i++)
            {
                var hexString = pattern.Substring(i * 2, 2);
                if (hexString == "??" || hexString == "**")
                {
                    bytes[i] = null;
                    continue;
                }

                bytes[i] = byte.Parse(hexString, NumberStyles.AllowHexSpecifier);
            }

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] StringToBytes(string byteString)
        {
            byteString = byteString.Replace(" ", "");

            if (byteString.Length % 2 != 0)
                throw new ArgumentException("Byte pattern length without whitespace must be divisible by two.",
                                            nameof(byteString));

            var byteLength = byteString.Length / 2;
            var bytes = new byte[byteLength];

            for (var i = 0; i < byteLength; i++)
            {
                var hexString = byteString.Substring(i * 2, 2);
                bytes[i] = byte.Parse(hexString, NumberStyles.AllowHexSpecifier);
            }

            return bytes;
        }
    }
}
