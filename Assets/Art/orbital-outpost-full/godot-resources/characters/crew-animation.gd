extends Node2D
## A paired visual only. Supply movement and role-state logic in your game.
## Body is the only playback clock; Shadow follows its animation and frame.

signal animation_finished(clip: StringName)

@onready var body: AnimatedSprite2D = $Body
@onready var shadow: AnimatedSprite2D = $Shadow

func _ready() -> void:
	body.animation_changed.connect(_sync_shadow)
	body.frame_changed.connect(_sync_shadow)
	body.animation_finished.connect(_on_animation_finished)
	_sync_shadow()

func play_animation(clip: StringName, restart: bool = false) -> bool:
	if not body.sprite_frames.has_animation(clip):
		return false
	if restart:
		body.stop()
	body.play(clip)
	_sync_shadow()
	return true

func pause_animation() -> void:
	body.pause()
	_sync_shadow()

func resume_animation() -> void:
	body.play()
	_sync_shadow()

func _sync_shadow() -> void:
	shadow.pause()
	shadow.animation = body.animation
	shadow.set_frame_and_progress(body.frame, body.frame_progress)

func _on_animation_finished() -> void:
	_sync_shadow()
	animation_finished.emit(body.animation)
