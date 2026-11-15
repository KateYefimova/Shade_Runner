extends Node2D

const CELL := 32
const DRAW_SCALE := 2.0
const MAP_OFFSET := Vector2.ZERO
const SPEED := 24.0
const MARKER_START := Vector2(6.5 * CELL, 6.5 * CELL)
const CREW_ORDER := ["explorer", "engineer", "medic", "security", "scientist", "android"]
var manifest: Dictionary
var animation_atlas: Dictionary
var floor_tex: Texture2D
var prop_tex: Texture2D
var shadow_tex: Texture2D
var equipment_tex: Texture2D
var actor_tex: Texture2D
var actor_shadow_tex: Texture2D
var actor_data: Dictionary
var crew: Dictionary
var actor_kind := "security"
var carrying := false
var cargo_transition := ""
var cargo_floor_exists := false
var cargo_floor_position := Vector2.ZERO
var cargo_floor_facing := "south"
var cargo_floor_textures: Dictionary
var repairing := false
var treating := false
var examining := false
var scanning := false
var aiming := false
var firing := false
var weapon_transition := ""
var facing := "south"
var walking := false
var actor_seconds := 0.0
var solid_bases: Array[Rect2] = []
var marker := MARKER_START
var clock_seconds := 0.0
var paused := false
var show_grid := false
var show_marker := true
var props_by_name: Dictionary = {}
var tile_resource: TileSet

func _ready() -> void:
	get_window().content_scale_size = Vector2i(1152,768)
	get_window().size = Vector2i(1152,768)
	texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	manifest = JSON.parse_string(FileAccess.get_file_as_string("res://manifest.json"))
	animation_atlas = JSON.parse_string(FileAccess.get_file_as_string("res://art/equipment-animations.json"))
	floor_tex = load("res://art/tiles.png")
	prop_tex = load("res://art/props.png")
	shadow_tex = load("res://art/shadows.png")
	equipment_tex = load("res://art/equipment-animations.png")
	crew = {
		"explorer": {"body":load("res://art/characters/outpost-explorer.png"), "shadow":load("res://art/characters/outpost-explorer-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-explorer-clips.json"))},
		"engineer": {"body":load("res://art/characters/outpost-engineer.png"), "shadow":load("res://art/characters/outpost-engineer-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-engineer-clips.json"))},
		"medic": {"body":load("res://art/characters/outpost-medic.png"), "shadow":load("res://art/characters/outpost-medic-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-medic-clips.json"))},
		"security": {"body":load("res://art/characters/outpost-security.png"), "shadow":load("res://art/characters/outpost-security-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-security-clips.json"))},
		"scientist": {"body":load("res://art/characters/outpost-scientist.png"), "shadow":load("res://art/characters/outpost-scientist-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-scientist-clips.json"))},
		"android": {"body":load("res://art/characters/outpost-android.png"), "shadow":load("res://art/characters/outpost-android-shadows.png"), "data":JSON.parse_string(FileAccess.get_file_as_string("res://art/characters/outpost-android-clips.json"))}
	}
	_select_character(actor_kind)
	for direction in ["south","east","north","west"]:
		cargo_floor_textures[direction] = load("res://art/characters/android-cargo-floor-"+direction+".png")
	for item in manifest.props:
		props_by_name[item.name] = item
	_build_solid_bases()
	_build_tile_resource()
	_build_controls()
	queue_redraw()

func _build_solid_bases() -> void:
	# Simple sample footprints; raised art can overlap the actor above its feet.
	for item in manifest.demo.placements:
		if item[0] in ["floor_vent", "maintenance_drone", "fire_extinguisher"]: continue
		var origin := Vector2(float(item[1]), float(item[2])) * CELL
		solid_bases.append(Rect2(origin + Vector2(8,22), Vector2(16,7)))

func _select_character(kind: String) -> void:
	cargo_transition = ""
	cargo_floor_exists = kind == "android"
	cargo_floor_position = marker
	cargo_floor_facing = facing
	weapon_transition = ""
	carrying = false
	actor_kind = kind
	actor_tex = crew[kind].body
	actor_shadow_tex = crew[kind].shadow
	actor_data = crew[kind].data
	repairing = false
	treating = false
	examining = false
	scanning = false
	aiming = false
	firing = false
	walking = false
	actor_seconds = 0.0

func _start_repair() -> void:
	if actor_kind != "engineer" or paused or repairing or treating or examining or scanning: return
	repairing = true
	walking = false
	actor_seconds = 0.0

func _start_treatment() -> void:
	if actor_kind != "medic" or paused or repairing or treating or examining or scanning: return
	treating = true
	walking = false
	actor_seconds = 0.0

func _start_examination() -> void:
	if actor_kind != "scientist" or paused or repairing or treating or examining or scanning: return
	examining = true
	walking = false
	actor_seconds = 0.0

func _start_scan() -> void:
	if actor_kind != "explorer" or paused or repairing or treating or examining or scanning: return
	scanning = true
	walking = false
	actor_seconds = 0.0

func _start_role_action() -> void:
	if actor_kind == "explorer": _start_scan()
	elif actor_kind == "security": _toggle_aim()
	elif actor_kind == "engineer": _start_repair()
	elif actor_kind == "medic": _start_treatment()
	elif actor_kind == "scientist": _start_examination()
	elif actor_kind == "android": _start_cargo_action()

func _start_cargo_action() -> void:
	if actor_kind != "android" or paused or cargo_transition != "": return
	if not carrying:
		if not cargo_floor_exists or marker.distance_to(cargo_floor_position) > 2.0:
			_update_label()
			return
		marker = cargo_floor_position
		facing = cargo_floor_facing
	cargo_transition = "lower" if carrying else "lift"
	cargo_floor_exists = false
	walking = false
	actor_seconds = 0.0

func _get_floor_cargo() -> Array[Dictionary]:
	if actor_kind == "android" and cargo_floor_exists and cargo_transition == "":
		return [{"position":cargo_floor_position,"facing":cargo_floor_facing}]
	return []

func _toggle_aim() -> void:
	if actor_kind != "security" or paused or firing or weapon_transition != "": return
	weapon_transition = "holster" if aiming else "draw"
	walking = false
	actor_seconds = 0.0

func _start_fire() -> void:
	if actor_kind != "security" or paused or not aiming or firing or weapon_transition != "": return
	firing = true
	walking = false
	actor_seconds = 0.0

func _build_tile_resource() -> void:
	tile_resource = TileSet.new()
	tile_resource.tile_size = Vector2i(CELL, CELL)
	var atlas := TileSetAtlasSource.new()
	atlas.texture = floor_tex
	atlas.texture_region_size = Vector2i(CELL, CELL)
	for i in range(24):
		atlas.create_tile(Vector2i(i % 8, i / 8))
	tile_resource.add_source(atlas, 0)

func _build_controls() -> void:
	pass

func _update_label() -> void:
	pass

func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_C:
				get_tree().change_scene_to_file("res://godot-demo/crew-showcase.tscn")
			KEY_G:
				show_grid = not show_grid
			KEY_SPACE:
				paused = not paused
			KEY_H:
				show_marker = not show_marker
			KEY_TAB:
				_select_character(CREW_ORDER[(CREW_ORDER.find(actor_kind)+1)%CREW_ORDER.size()])
			KEY_E:
				_start_role_action()
			KEY_F:
				_start_fire()
			KEY_R:
				cargo_transition = ""
				cargo_floor_exists = actor_kind == "android"
				cargo_floor_position = MARKER_START
				cargo_floor_facing = "south"
				weapon_transition = ""
				carrying = false
				marker = MARKER_START
				facing = "south"
				walking = false
				actor_seconds = 0.0
				repairing = false
				treating = false
				examining = false
				scanning = false
				aiming = false
				firing = false
		_update_label()
		queue_redraw()

func _process(delta: float) -> void:
	if not paused:
		clock_seconds += delta
		var motion := Vector2.ZERO
		if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT): motion.x -= 1
		if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT): motion.x += 1
		if Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP): motion.y -= 1
		if Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN): motion.y += 1
		_update_actor(motion, delta)
		queue_redraw()

func _update_actor(motion: Vector2, delta: float) -> void:
	if cargo_transition != "":
		actor_seconds += delta
		if actor_seconds * 1000.0 >= float(actor_data.clips[cargo_transition+"_"+facing].total_ms):
			carrying = cargo_transition == "lift"
			cargo_floor_exists = not carrying
			cargo_floor_position = marker
			cargo_floor_facing = facing
			cargo_transition = ""
			actor_seconds = 0.0
			_update_label()
		return
	if weapon_transition != "":
		actor_seconds += delta
		if actor_seconds * 1000.0 >= float(actor_data.clips[weapon_transition+"_"+facing].total_ms):
			aiming = weapon_transition == "draw"
			weapon_transition = ""
			actor_seconds = 0.0
			_update_label()
		return
	if firing:
		actor_seconds += delta
		if actor_seconds * 1000.0 >= float(actor_data.clips["fire_"+facing].total_ms):
			firing = false
			actor_seconds = 0.0
			_update_label()
		return
	if repairing or treating or examining or scanning:
		actor_seconds += delta
		var role_action := "examine_" if examining else ("treat_" if treating else "repair_")
		if scanning: role_action = "scan_"
		if actor_seconds * 1000.0 >= float(actor_data.clips[role_action+facing].total_ms):
			repairing = false
			treating = false
			examining = false
			scanning = false
			actor_seconds = 0.0
		return
	if motion != Vector2.ZERO:
		if absf(motion.x) > absf(motion.y): facing = "east" if motion.x > 0 else "west"
		else: facing = "south" if motion.y > 0 else "north"
	var before := marker
	var displacement := motion.normalized() * SPEED * delta
	var steps := maxi(1, ceili(displacement.length() / 2.0))
	for step in range(steps): _move_marker(displacement / steps)
	var moved := marker.distance_to(before) > 0.001
	if moved != walking: actor_seconds = 0.0
	walking = moved
	actor_seconds += delta

func _move_marker(motion: Vector2) -> void:
	var candidate := marker + Vector2(motion.x, 0)
	if _walkable(candidate): marker.x = candidate.x
	candidate = marker + Vector2(0, motion.y)
	if _walkable(candidate): marker.y = candidate.y

func _walkable(point: Vector2) -> bool:
	for corner in [Vector2(-4,-4), Vector2(4,-4), Vector2(-4,4), Vector2(4,4)]:
		var q: Vector2 = point + corner
		var x := floori(q.x / CELL)
		var y := floori(q.y / CELL)
		if x < 0 or y < 0 or x >= int(manifest.demo.width) or y >= int(manifest.demo.height): return false
		if manifest.demo.map[y][x] is String: return false
	var feet := Rect2(point + Vector2(-3,-2), Vector2(6,4))
	for base in solid_bases:
		if feet.intersects(base): return false
	return true

func _actor_frame() -> int:
	var action := "examine_" if examining else ("treat_" if treating else ("repair_" if repairing else ("walk_" if walking else "idle_")))
	if actor_kind == "android" and carrying: action = "carry_" + ("walk_" if walking else "idle_")
	if scanning: action = "scan_"
	if aiming: action = "armed_walk_" if walking else "aim_"
	if firing: action = "fire_"
	if weapon_transition != "": action = weapon_transition + "_"
	if cargo_transition != "": action = cargo_transition + "_"
	return _frame_at(actor_data, action + facing, actor_seconds)

func _frame_at(data: Dictionary, clip_name: String, seconds: float) -> int:
	var clip: Dictionary = data.clips[clip_name]
	var time_ms := seconds * 1000.0
	if clip.loop: time_ms = fmod(time_ms, float(clip.total_ms))
	else: time_ms = minf(time_ms, float(clip.total_ms)-0.001)
	var elapsed := 0.0
	for i in range(clip.frames.size()):
		elapsed += float(clip.durations_ms[i])
		if time_ms < elapsed: return int(clip.frames[i])
	return int(clip.frames[-1])

func _get_draw_actors() -> Array[Dictionary]:
	return [{"body":actor_tex,"shadow":actor_shadow_tex,"data":actor_data,"frame":_actor_frame(),"position":marker}]

func _draw_actor(shadow: bool, actor: Dictionary) -> void:
	var rect: Array = actor.data.frames[actor.frame].rect
	var origin: Vector2 = (actor.position.round() - Vector2(16,28)) / CELL
	_draw_asset(actor.shadow if shadow else actor.body, Rect2(rect[0],rect[1],rect[2],rect[3]), origin)

func _wall_mask(x: int, y: int) -> int:
	var grid: Array = manifest.demo.map
	var result := 0
	if y > 0 and grid[y-1][x] is String: result |= 1
	if x < int(manifest.demo.width)-1 and grid[y][x+1] is String: result |= 2
	if y < int(manifest.demo.height)-1 and grid[y+1][x] is String: result |= 4
	if x > 0 and grid[y][x-1] is String: result |= 8
	return result

func _atlas_rect(index: int, columns: int = 8) -> Rect2:
	return Rect2((index % columns) * CELL, (index / columns) * CELL, CELL, CELL)

func _clip_frame(clip_index: int, seconds: float) -> int:
	var time_ms := fmod(seconds * 1000.0, 600.0)
	var duration_sum := 0.0
	for frame_index in range(4):
		duration_sum += float(manifest.animations[clip_index].durations_ms[frame_index])
		if time_ms < duration_sum: return frame_index
	return 3

func _draw_asset(tex: Texture2D, rect: Rect2, cell: Vector2) -> void:
	draw_texture_rect_region(tex, Rect2(MAP_OFFSET + cell * CELL * DRAW_SCALE, Vector2(CELL,CELL) * DRAW_SCALE), rect)

func _draw() -> void:
	if manifest.is_empty(): return
	var width := int(manifest.demo.width)
	var height := int(manifest.demo.height)
	for y in range(height):
		for x in range(width):
			var index := int(manifest.demo.floors[y][x])
			if manifest.demo.map[y][x] is String:
				index = 8 + _wall_mask(x,y)
			_draw_asset(floor_tex, _atlas_rect(index), Vector2(x,y))
	for item in manifest.demo.placements:
		var region: Array = props_by_name[item[0]].rect
		var rect := Rect2(region[0], region[1], region[2], region[3])
		_draw_asset(shadow_tex, rect, Vector2(item[1], item[2]))
	var actors: Array[Dictionary] = []
	if show_marker: actors = _get_draw_actors()
	for actor in actors: _draw_actor(true, actor)
	var objects: Array[Dictionary] = []
	for item in manifest.demo.placements:
		objects.append({"kind":"prop", "item":item, "ground_y":float(item[2])*CELL+28})
	for item in manifest.demo.fxplacements:
		objects.append({"kind":"equipment", "item":item, "ground_y":float(item[2])*CELL+28})
	for actor in actors: objects.append({"kind":"actor", "ground_y":actor.position.y,"actor":actor})
	if show_marker:
		for cargo in _get_floor_cargo():
			objects.append({"kind":"floor_cargo","ground_y":cargo.position.y+(-3 if cargo.facing == "north" else 1),"cargo":cargo})
	objects.sort_custom(func(a: Dictionary, b: Dictionary) -> bool: return a.ground_y < b.ground_y)
	for object in objects:
		if object.kind == "floor_cargo":
			_draw_asset(cargo_floor_textures[object.cargo.facing],Rect2(0,0,32,32),(object.cargo.position.round()-Vector2(16,28))/CELL)
			continue
		if object.kind == "actor":
			_draw_actor(false, object.actor)
			continue
		var item: Array = object.item
		if object.kind == "prop":
			var region: Array = props_by_name[item[0]].rect
			_draw_asset(prop_tex, Rect2(region[0],region[1],region[2],region[3]), Vector2(item[1],item[2]))
		else:
			var clip_index := -1
			for i in range(manifest.animations.size()):
				if manifest.animations[i].name == item[0]: clip_index = i
			var frame_index := clip_index * 4 + _clip_frame(clip_index, clock_seconds)
			var frame: Dictionary = animation_atlas.frames[frame_index].frame
			_draw_asset(equipment_tex, Rect2(frame.x,frame.y,frame.w,frame.h), Vector2(item[1],item[2]))
	if show_grid:
		for x in range(width+1):
			draw_line(MAP_OFFSET+Vector2(x*CELL*DRAW_SCALE,0),MAP_OFFSET+Vector2(x*CELL*DRAW_SCALE,height*CELL*DRAW_SCALE),Color(0.6,0.93,0.88,0.25))
		for y in range(height+1):
			draw_line(MAP_OFFSET+Vector2(0,y*CELL*DRAW_SCALE),MAP_OFFSET+Vector2(width*CELL*DRAW_SCALE,y*CELL*DRAW_SCALE),Color(0.6,0.93,0.88,0.25))

func qa_check() -> Dictionary:
	var failures: Array[String] = []
	if floor_tex.get_size() != Vector2(256,96): failures.append("Tile atlas dimensions")
	if prop_tex.get_size() != Vector2(256,96): failures.append("Prop atlas dimensions")
	if animation_atlas.frames.size() != 16: failures.append("Animation frame count")
	if tile_resource.get_source(0).get_tiles_count() != 24: failures.append("Godot TileSet tile count")
	if crew.explorer.data.frames.size() != 72 or crew.explorer.data.clips.size() != 12: failures.append("Explorer frame and clip counts")
	if crew.engineer.data.frames.size() != 72 or crew.engineer.data.clips.size() != 12: failures.append("Engineer frame and clip counts")
	if crew.medic.data.frames.size() != 72 or crew.medic.data.clips.size() != 12: failures.append("Medic frame and clip counts")
	if crew.security.data.frames.size() != 152 or crew.security.data.clips.size() != 28: failures.append("Security frame and clip counts")
	if crew.scientist.data.frames.size() != 72 or crew.scientist.data.clips.size() != 12: failures.append("Scientist frame and clip counts")
	if crew.android.data.frames.size() != 144 or crew.android.data.clips.size() != 24: failures.append("Android frame and clip counts")
	for kind in crew:
		if crew[kind].body.get_size() != crew[kind].shadow.get_size(): failures.append(kind+" shadow atlas alignment")
	if not _walkable(MARKER_START): failures.append("Crew spawn clearance")
	for base in solid_bases:
		if _walkable(base.get_center()): failures.append("Furniture footprint blocking")
	if manifest.demo.floors.size() != int(manifest.demo.height): failures.append("Floor layout height")
	for row in manifest.demo.floors:
		if row.size() != int(manifest.demo.width): failures.append("Floor layout width")
	for i in range(16):
		var frame: Dictionary = animation_atlas.frames[i].frame
		if frame.w != 32 or frame.h != 32: failures.append("Animation frame dimensions " + str(i))
		if animation_atlas.frames[i].duration != [140,100,140,220][i%4]: failures.append("Animation timing " + str(i))
	for cell in [Vector2(1,1),Vector2(7,6),Vector2(9,6),Vector2(13,5),Vector2(16,10)]:
		if not _walkable((cell+Vector2(0.5,0.5))*CELL): failures.append("Expected open floor " + str(cell))
	for cell in [Vector2(0,0),Vector2(8,3),Vector2(17,11)]:
		if _walkable((cell+Vector2(0.5,0.5))*CELL): failures.append("Expected wall collision " + str(cell))
	for i in range(4):
		for j in range(4):
			if _clip_frame(i,[0.0,0.14,0.24,0.38][j]+0.001) != j: failures.append("Frame scheduling")
	var original := marker
	marker = MARKER_START
	_move_marker(Vector2(12,0))
	if marker.x <= MARKER_START.x: failures.append("Movement")
	marker = Vector2(1*CELL+4,2.5*CELL)
	_move_marker(Vector2(-2,0))
	if marker.x != 1*CELL+4: failures.append("Movement wall blocking")
	marker = original
	return {"passed":failures.is_empty(),"failures":failures,"godot_version":Engine.get_version_info().string,"props":manifest.props.size(),"tiles":24,"equipment_clips":4,"equipment_frames":16,"characters":6,"character_clips":100,"character_frames":584,"tile_resource_tiles":tile_resource.get_source(0).get_tiles_count(),"furniture_bases":solid_bases.size()}

