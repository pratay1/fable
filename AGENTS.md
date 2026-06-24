# AGENTS.md

## Cursor Cloud specific instructions

### Product
This repo is a single product: **Fable**, an offline 2D top-down survival/roguelite shooter
written in **C# / .NET 9** using **Raylib-cs** (`fable.csproj`). The entire game lives in one
large file, `Program.cs`. There is **no backend, database, web server, or network layer** — it
is a self-contained desktop game. Local state is persisted in `fable_save.txt`.

### Toolchain (baked into the VM snapshot)
- The **.NET 9 SDK** is installed at `~/.dotnet` and is on `PATH` via `~/.bashrc`
  (`DOTNET_ROOT=$HOME/.dotnet`). New interactive shells have `dotnet` available; if a
  non-interactive shell does not, invoke it as `~/.dotnet/dotnet`.
- The update script runs `dotnet restore` (pulls `Raylib-cs` 8.0.0 from NuGet).

### Build / lint / test / run
- Build: `dotnet build -c Debug` (run from repo root). There is **no separate linter** — the
  compiler + analyzers surface warnings during build (the build is expected to succeed with one
  pre-existing unused-field warning, `CS0414`).
- Tests: **there are no automated tests** in this repo (no test project).
- Run: `dotnet run -c Debug`.

### Running headless (important)
Raylib opens an OpenGL window, so a display is required. This VM is headless, so:
- Start a virtual display first, e.g. `Xvfb :99 -screen 0 900x900x24 &` then `export DISPLAY=:99`.
- Use software GL: `export LIBGL_ALWAYS_SOFTWARE=1` (no GPU on the VM; Mesa `llvmpipe` is used).
- Capture frames/video with `ffmpeg -f x11grab` against `:99`.

### Input injection gotchas (headless)
There is **no window manager** on `:99`, so GLFW input is finicky:
- First give the game window input focus: `xdotool windowfocus --sync <winid>` (find it with
  `xdotool search --name Fable`). A `xdotool mousemove <x> <y> click 1` into the window also
  helps establish focus.
- Send keys with an explicit **hold**: `xdotool keydown <key>; sleep 0.18; xdotool keyup <key>`.
  Fast `xdotool key <key>` events are often dropped (treated as auto-repeat) and do **not**
  register. Do **not** use `xdotool key --window` (GLFW ignores synthetic `XSendEvent` keys).

### Gameplay notes useful for demos
- From the main menu, **Enter/Space** starts a run; **C** = Armory, **S** = Settings.
- Standing still is fatal fast: tiles crumble ("The floor collapsed beneath you"). Keep moving
  onto fresh tiles to survive.
- Press **`\`** (backslash) to toggle the **Grandmaster Pilot** AI, which auto-plays, dodges
  crumbling tiles, and fights — the easiest way to produce a self-playing gameplay demo headless.
- After game over: **R** rebirths (new run), **Esc** returns to the main menu.
