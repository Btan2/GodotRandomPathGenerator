extends Control

#onready var mat : ShaderMaterial = preload("res://materials/Material_Laser_Red.tres")
var start
var end

func set_laser_positions(_start, _end):
	start = _start
	end = _end

func _ready():
	set_laser_positions(Vector3.ZERO, Vector3.ONE * 7)

func _physics_process(delta):
	DebugDraw.draw_line_3d(start, end, Color.red);
