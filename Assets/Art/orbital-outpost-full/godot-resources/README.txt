ORBITAL OUTPOST — GODOT CHARACTER RESOURCES
Version 0.14.3

WHAT IS INCLUDED
Six body SpriteFrames libraries, six matching shadow libraries, six paired
character scenes, and one small visual playback script. All 100 character
clips and 584 timed cells are represented in each body/shadow set.

QUICK START
1. Copy art/characters/ and godot-resources/ into your Godot project root.
   Keep those two folder names and their relative paths. Wait for PNG import.
2. Drag godot-resources/characters/outpost-<role>.tscn into your scene.
   Roles: explorer, engineer, medic, security, scientist, android.
3. The root sits at the feet pivot (16,28). Move and scale that root. The
   children use a top-left offset of (-16,-28), with nearest filtering.
4. The scene starts idle_south. Call play_animation with an exact clip name.
   Clip names match art/characters/outpost-<role>-clips.json.

Example, from a script with an instanced crew child named Engineer:
    $Engineer.play_animation(&"walk_east")
    $Engineer.play_animation(&"repair_east", true)
    $Engineer.pause_animation()
    $Engineer.resume_animation()

play_animation returns false for an unknown clip, leaving playback unchanged.
Repeated play of the current clip continues its clock. Pass true to restart.
After a finished one-shot, use true to replay it from the start. Pause/resume
retain the current frame and progress. Connect the root's animation_finished
signal (clip: StringName) to choose the next action. One-shots hold their last
pose until your code chooses another clip. Loops do not emit that signal.

TIMING AND SHADOWS
SpriteFrames uses 10 fps as its timing base, with a separate relative weight
for every frame. A 70 ms frame has weight 0.7, a 280 ms hold has weight 2.8.
Actual duration = relative weight / fps at normal playback speed. Do not
replace the weighted durations with uniform frame timings.
Source: https://docs.godotengine.org/en/stable/classes/class_spriteframes.html

Body is the only running AnimatedSprite2D clock. The script copies animation
and frame to a paused shadow, preventing drift through switches and pauses.
Shadow draws at z_index -1; body draws at 0. Configure your room's floor,
ground shadows and Y-sorted actors accordingly. Parent/root transforms move
both layers. Keep positions and display scale on whole native pixels.

USING JUST THE RESOURCES
Assign outpost-<role>.tres to an AnimatedSprite2D's Sprite Frames property.
Use the corresponding -shadows.tres on a shadow node, or omit it if your game
uses another shadow system. Set nearest filtering and the correct feet pivot.
No manifest, clip JSON, Aseprite installation, plugin or custom importer is
needed at runtime by these resources or paired scenes. PNG atlases are needed.
Do not edit shared SpriteFrames in a running character unless all instances
should change; make the resource unique first for per-instance edits.

ACTION STATE LOGIC
These are visual resources, not a movement, inventory or combat controller.
The original playable demo still demonstrates action timing and cargo state.
Your controller decides when movement/facing lock and which clip follows.
Security: draw -> aim/armed_walk; fire -> aim; holster -> idle/walk.
Android: lift -> carry_idle/carry_walk; lower -> empty idle/walk.
Explorer scan, Engineer repair, Medic treat, Scientist examine -> idle/walk.
Use the facing suffix south/east/north/west throughout each transition.

For Android floor cargo, hide the separate floor component before lift starts:
the transition already contains it. At lower completion, change to empty idle
and show android-cargo-floor-<facing>.png at the same interaction position.
Keep that floor object there when the Android walks away. North floor cargo
draws behind the body/legs; other directions draw in front of the body. The
floor PNG includes its contact shadow. Do not overlay it on a transition.
Security's sidearm is already included in its body frames; do not add it twice.

Tested on Windows with Godot 4.3 and 4.7.2 Compatibility.
See ../README.txt for compatibility and scope.
