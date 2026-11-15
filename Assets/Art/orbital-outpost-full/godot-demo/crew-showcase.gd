extends "res://godot-demo/main.gd"

# A deterministic presentation of existing clips, without new character AI.
const SHOWCASE_LENGTH := 12.0
var showcase_seconds := 0.0

func _ready() -> void:
	super._ready()
	get_window().content_scale_size = Vector2i(1152,768)
	get_window().size = Vector2i(1152,768)

func _update_label() -> void:
	pass

func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_C: get_tree().change_scene_to_file("res://godot-demo/main.tscn")
			KEY_SPACE: paused = not paused
			KEY_R:
				showcase_seconds = 0.0
				clock_seconds = 0.0
			KEY_G: show_grid = not show_grid
			KEY_H: show_marker = not show_marker
		_update_label()
		_refresh_status()
		queue_redraw()

func _process(delta: float) -> void:
	if not paused:
		showcase_seconds = fposmod(showcase_seconds + delta, SHOWCASE_LENGTH)
		clock_seconds = showcase_seconds
		_refresh_status()
		queue_redraw()

func _refresh_status() -> void:
	pass

func _get_floor_cargo() -> Array[Dictionary]:
	var t := fposmod(showcase_seconds, SHOWCASE_LENGTH)
	if t >= 4.0 and t < 5.14: return [{"position":Vector2(368,208),"facing":"west"}]
	if t >= 10.0 and t < 11.14: return [{"position":Vector2(440,208),"facing":"east"}]
	return []

func _staged_actor(kind: String, position: Vector2, clip_name: String, seconds: float) -> Dictionary:
	return {"kind":kind,"body":crew[kind].body,"shadow":crew[kind].shadow,"data":crew[kind].data,
		"frame":_frame_at(crew[kind].data,clip_name,seconds),"position":position,
		"clip":clip_name,"action":crew[kind].data.clips[clip_name].action,
		"facing":crew[kind].data.clips[clip_name].facing,"clip_seconds":seconds}

func _get_draw_actors() -> Array[Dictionary]:
	var t := fposmod(showcase_seconds, SHOWCASE_LENGTH)
	var explorer_position := Vector2(112,112)
	var explorer_clip := "idle_east"
	var explorer_time := t
	if t < 4.0:
		explorer_position.x += t * SPEED
		explorer_clip = "walk_east"
	elif t < 5.0:
		explorer_position.x = 208
		explorer_clip = "scan_east"
		explorer_time = t - 4.0
	elif t < 9.0:
		explorer_position.x = 208 - (t-5.0)*SPEED
		explorer_clip = "walk_west"
		explorer_time = t - 5.0
	elif t < 10.0:
		explorer_clip = "scan_west"
		explorer_time = t - 9.0
	elif t < 11.0:
		explorer_clip = "idle_west"
		explorer_time = t - 10.0
	else:
		explorer_time = t - 11.0
	var engineer_phase := fposmod(t-1.4,4.0)
	var medic_phase := fposmod(t-2.6,4.0)
	var scientist_phase := fposmod(t-0.8,4.0)
	var security_position := Vector2(432,144)
	var security_clip := "idle_north"
	var security_time := t - 9.0
	if t < 3.0:
		security_position.y += t * SPEED
		security_clip = "armed_walk_south"
		security_time = t
	elif t < 6.0:
		security_position.y = 216
		security_clip = "aim_south"
		security_time = t - 3.0
		if t >= 3.4 and t < 4.0:
			security_clip = "fire_south"
			security_time = t - 3.4
		elif t >= 4.0: security_time = t - 4.0
	elif t < 9.0:
		security_position.y = 216 - (t-6.0)*SPEED
		security_clip = "armed_walk_north"
		security_time = t - 6.0
	else:
		security_clip = "aim_north"
		if t >= 9.4 and t < 10.0:
			security_clip = "fire_north"
			security_time = t - 9.4
		elif t >= 10.0: security_time = t - 10.0
	# Use the stationary watch intervals for one-shot weapon transitions.
	# Routes, speed and firing intervals stay the same.
	var watch_phase := t - 4.6 if t < 6.0 else t - 10.6
	if (t >= 4.6 and t < 6.0) or t >= 10.6:
		var watch_facing := "south" if t < 6.0 else "north"
		if watch_phase < 0.48:
			security_clip = "holster_" + watch_facing
			security_time = watch_phase
		elif watch_phase < 0.96:
			security_clip = "idle_" + watch_facing
			security_time = watch_phase - 0.48
		else:
			security_clip = "draw_" + watch_facing
			security_time = watch_phase - 0.96
	# The loaded route is phased against Security's patrol to keep them apart.
	var android_position := Vector2(440,208)
	var android_clip := "carry_idle_east"
	var android_time := t - 9.0
	if t < 3.0:
		android_position.x -= t * SPEED
		android_clip = "carry_walk_west"
		android_time = t
	elif t < 6.0:
		android_position.x = 368
		android_clip = "carry_idle_west"
		android_time = t - 3.0
	elif t < 9.0:
		android_position.x = 368 + (t-6.0)*SPEED
		android_clip = "carry_walk_east"
		android_time = t - 6.0
	# Cargo stays on the floor between each lowering and lifting action.
	if (t >= 3.1 and t < 6.0) or t >= 9.1:
		var cargo_phase := t-3.1 if t < 6.0 else t-9.1
		var cargo_facing := "west" if t < 6.0 else "east"
		if t < (4.0 if t < 6.0 else 10.0):
			android_clip = "lower_"+cargo_facing
			android_time = cargo_phase
		elif t < (5.14 if t < 6.0 else 11.14):
			android_clip = "idle_"+cargo_facing
			android_time = cargo_phase-0.9
		else:
			android_clip = "lift_"+cargo_facing
			android_time = cargo_phase-2.04
	return [
		_staged_actor("explorer",explorer_position,explorer_clip,explorer_time),
		_staged_actor("engineer",Vector2(426,287),"repair_east" if engineer_phase < 1.0 else "idle_east",engineer_phase if engineer_phase < 1.0 else engineer_phase-1.0),
		_staged_actor("medic",Vector2(336,138),"treat_north" if medic_phase < 1.0 else "idle_north",medic_phase if medic_phase < 1.0 else medic_phase-1.0),
		_staged_actor("security",security_position,security_clip,security_time),
		_staged_actor("scientist",Vector2(400,92),"examine_east" if scientist_phase < 1.0 else "idle_east",scientist_phase if scientist_phase < 1.0 else scientist_phase-1.0),
		_staged_actor("android",android_position,android_clip,android_time)
	]
