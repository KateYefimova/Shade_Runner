ORBITAL OUTPOST
32 x 32 sci-fi interiors and crew by DadddyDarko
Version 0.14.3 | Published by HappyHustle

CONTENTS
- 24 props, eight floor tiles and sixteen cardinal wall connector states.
- Four equipment loops with sixteen timed frames.
- Six crew, four directions, 100 clips and 584 timed frames including holds.
- PNG atlases, individual images, separate shadows and animation JSON.
- Ten editable Aseprite sources and a 12-colour palette.
- Twelve Godot SpriteFrames libraries and six paired character scenes.
- A furnished Godot sample room, crew showcase and reusable 24-cell TileSet.

QUICK START
1. Extract the full ZIP before opening files.
2. Use art/ for PNGs, atlas rectangles, clip timings and the palette.
3. Slice atlases on a 32 x 32 grid with zero margin and zero spacing.
4. Use nearest-neighbour filtering and whole-pixel scaling.
5. Open source/ files in Aseprite to edit the artwork.
6. Read IMPORT-GUIDE.txt for wall masks, layering and role-action details.

GODOT PROJECT
Import project.godot, let image imports finish, then run the project.
Keep manifest.json: it supplies the sample room layout and equipment data.
To reuse characters, copy art/characters/ and godot-resources/ into your own
project root. See godot-resources/README.txt for playback calls and setup.

The demo fits the 1152 x 768 room, with no outer bars or text overlays. Controls:
C              Switch playable room / six-crew showcase
WASD or Arrows Move the selected crew member
Tab            Select the next crew member
E              Perform a role action; Security draws/holsters
F              Fire once while Security is armed
Space          Pause / resume
G              Show / hide the grid
H              Show / hide crew
R              Reset the scene

Explorer scans; Engineer repairs; Medic treats; Scientist examines.
Android lifts nearby cargo or lowers it. Return to placed cargo to lift it.
Movement and facing lock during one-shot actions. The showcase lasts twelve
seconds and repeats. The sample provides basic wall and furniture blocking.

COMPATIBILITY AND SCOPE
Tested with Godot 4.3 and 4.7.2 on Windows, Standard/GDScript builds, using the
Compatibility renderer. Other versions, platforms and renderers are unverified.
No editor cache is included; allow a fresh import after extraction.

The artwork uses 32 x 32 cells, except the assembled room source. Props have
one viewing direction. Walls use a four-neighbour, sixteen-state connector
set. Add your own collision shapes and terrain rules to the reusable TileSet.
The role actions demonstrate animations. Combat, health, research progression,
quests, general inventory systems, door animations, UI and audio are absent.

WINDOWS DEMO
The separate free Windows x86-64 demo is available on the project page.
Extract its ZIP and open OrbitalOutpost.exe; Godot installation is not needed.
It starts with the showcase. The controls above apply; Esc closes the demo.
Exported with Godot 4.7.2 Compatibility. The executable is unsigned. Keep its
included license and engine notices with it. Other hardware is unverified.

FILES AND RIGHTS
ASSET-LIST.txt lists the props, tiles and character clips.
LICENSE.txt explains commercial/non-commercial project use and redistribution.
Credit is optional; standalone asset resale and redistribution are prohibited.

Project: https://happyhustle.itch.io/orbital-outpost
Creator: https://dadddydarko.itch.io/
