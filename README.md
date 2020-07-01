# BfresLibrary
A reworked version of NintenTools.Bfres with Wii U and Switch support and many various improvements/features.

## Features
- Support for nearly all bfres versions for Wii U and Switch platforms.
- Merged library code for WiiU and Switch platforms for easy access and conversion.
- Support for accessing shader parameters.
- In tool swizzle methods to access deswizzled textures. (Note these are not decoded and will not output RGBA8 data, must be done by external code).
- Many various improvements and bug fixes to the previous NintenTools library.

## Planned
- Instant conversion between Wii U and Switch binaries. This is already mostly functional, textures need to be done. 
Note for proper conversion in games, the materials must be from the platform being converted to due to shader changes.
- Improve visibily animation support. Boolean bits have issues on some animations.
- Add text conversion for all sections.
