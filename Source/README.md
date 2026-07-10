# Source Layout

`Program` is intentionally split with C# `partial class Program` files. All partial files compile into the same type, so code in one folder can call methods and fields from another folder directly.

- `AI/`: autopilot and grandmaster survival logic.
- `Core/`: entry point, global state, setup, and update loop.
- `Data/`: save/load and catalog construction.
- `Domain/`: shared enums and structs.
- `Gameplay/`: progression, events, spawning, simulation, and upgrade effects.
- `Rendering/`: draw loop, gameplay scene rendering, post-processing, castles, and visual effects.
- `Rendering/MenuCastle/`: main-menu castle renderer split by sky, lighting, masonry, towers, gatehouse, framing, and detail passes.
- `UI/`: menus, HUD, settings, armory/customization, and reusable widgets.

Shared usings live in `GlobalUsings.cs`.
