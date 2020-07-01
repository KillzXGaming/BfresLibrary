# BfresLibrary
A reworked version of NintenTools.Bfres with Wii U and Switch support and many various improvements/features.

## Features
- Support for nearly all bfres versions for Wii U and Switch platforms.
- Merged library code for WiiU and Switch platforms for easy access and conversion.
- Support for accessing shader parameters.
- In library swizzle methods to access deswizzled textures. (Note these are not decoded and will not output RGBA8 data, must be done by external code).
- Many various improvements and bug fixes to the previous NintenTools library.

## Planned
- Instant conversion between Wii U and Switch binaries. This is already mostly functional, textures need to be done. 
Note for proper conversion in games, the materials must be from the platform being converted to due to shader changes.
- Improve visibily animation support. Boolean bits have issues on some animations.
- Add text conversion for all sections.
- Use an updated binary data version.
- Cleanup the read/writing code. It's quite a mess due to using various platforms and versions. Some things are adjusted to improve speed and performances and also to mantain accuarcy with the ordering and handling of some structures. 

## Credits

[Syroot/Ray Koopa for the original library used.](https://gitlab.com/Syroot/NintenTools.Bfres/tree/master/src/Syroot.NintenTools.Bfres)

## LICENSE

https://gitlab.com/Syroot/NintenTools.Bfres/-/blob/master/LICENS
