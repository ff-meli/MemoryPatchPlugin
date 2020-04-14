using System;

namespace MemoryPatchPlugin.json
{
    // This class used to be much more complicated, to actually handle address/sig+offset/datasig+offset separately
    // For now I've abandoned data sigs, since they probably don't make much sense for patching, and just simplified
    // all this to a single class for ease of use
    class LocationDefinition
    {
        // All fields are optional, but at least one must be specified

        // optional - a byte signature to scan for.  Supports ?? and ** wildcards
        public string Signature { get; set; }
        
        // optional
        // When used with Signature, an offset added to the found signature location
        // When used alone, the offset from the module base of the executable
        public string Offset { get; set; }
    }
}
