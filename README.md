# MemoryPatchPlugin
Simple runtime memory patcher plugin for Dalamud.

Patches can be enabled and disabled freely at runtime through the plugin ui.  The list of available patches can be refreshed at any time as well.

Patches can be flagged to load immediately on plugin startup if desired.

## Sample patch
```
{
  "name": "A Globally Unique Name",
  "description": "An optional description that will be displayed in the UI if present.",
  
  "location": {
    // you can specify a signature, plus an optional offset, as is shown here
    // you can also specify just a signature (implied offset of 0)
    // or just an offset (offset is then added to the module base)

    // signatures are currently only for the text segment
    // and support wildcard bytes with ?? or **
    "signature": "40 55 53 56 57 41 54 48 8D AC 24 ?? ?? ?? ?? 48 81 EC"
    "offset": "3F"
  },
  
  // the entire validation block is optional
  "validation": {
    // the version of the xiv executable that this patch is for
    // if the version does not match, the patch will not be loaded
    "exeVersion": "2020.03.27.0000.0000",
    
    // the byte pattern we expect to see at the address for this patch
    // if it does not match, the patch will not be loaded
    // this supports wildcard bytes with ?? or **
    "requiredBytes": "E8 ?? ?? ?? ?? 44 8D 66 01 84 C0"
  },
  
  "patch": {
    // where the new code bytes will be written
    // currently this is the only working option (and can also be omitted)
    // overwrite means the bytes at the target address are directly overwritten
    "location": "overwrite",
    
    // the byte pattern to apply as the actual code patch
    // be sure to pad instructions with nops or similar as necessary (as with the 0x90 bytes here)
    "bytes": "31 C0 90 90 90"
  }
}
```
