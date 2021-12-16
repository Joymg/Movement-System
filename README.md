# Movement-System

A movement system implemented in Unity.

Implemented features:
- Basic Physics
- Surface Contact: How the player interacts with the surfaces, allowing wall jumps, "realistic" jumps from steeps, getting out of crevasses etc.
- Orbital Camera: Orbits around the player with manual or automatic rotation. Make movement relative to the camera. Prevent camera form intersecting with the world
- Custom Gravity: Allows arbitary gravity. Apply custom gravity to arbitrary bodies.
- Complex Gravity: Supports multiple gravity sources. Created planar, spherical, and box-shaped gravity sources. Adding gravity falloff.
- Moving environment: Animated platforms. Player follows motion of the platform is connected to.
- Climbing: Climbable and unclimbable surfaces. Stick to walls, even if they're moving. Use wall-relative controls for climbing. Climb around corners and overhangs. Prevent sliding while standing on a slope
- Reactive environement: Launch pads and levitation zones. Multi-purpose detection zone. Reactively swap materials and activate or deactivate objects. Move objects via simple interpolation triggered by events.

Coming Next:
- Rolling
- Support for tactile devices
- Swimming(?)
