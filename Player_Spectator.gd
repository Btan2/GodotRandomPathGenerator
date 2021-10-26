extends KinematicBody

onready var collider : CollisionShape = $CollisionShape
onready var head : Spatial = $Head
onready var camera : Camera = $Head/Camera

const MAXSPEED : float = 4.0        # default: 32.0
const WALKSPEED : float = 4.0       # default: 16.0
const ACCELERATE : float = 10.0     # default: 10.0
const MOVEFRICTION : float = 6.0    # default: 6.0

var movespeed : float = 32.0
var fmove : float = 0.0
var smove : float = 0.0
var umove : float = 0.0
var velocity : Vector3 = Vector3.ZERO
var mouse_rotation_x : float
var mouse_sensitivity : float = 0.1

var noclip = false

"""
===============
_ready
===============
"""
func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

"""
===============
_input
===============
"""
func _input(event):
	if event is InputEventMouseMotion:
		mouse_rotation_x -= event.relative.y * mouse_sensitivity
		mouse_rotation_x = clamp(mouse_rotation_x, -90, 90)
		rotate_y(deg2rad(-event.relative.x * mouse_sensitivity))
	
	fmove = Input.get_action_strength("move_forward") - Input.get_action_strength("move_backward")
	smove = Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	umove = Input.get_action_strength("jump") - Input.get_action_strength("crouch")
	
	movespeed = WALKSPEED if Input.is_action_pressed("shift") else MAXSPEED

"""
===============
_physics_process
===============
"""
func _physics_process(delta):
	var wishdir : Vector3
	
	head.transform.origin = Vector3(0, 0.25, 0)
	camera.rotation_degrees = Vector3(mouse_rotation_x, 0, 0)
	
	if noclip:
		wishdir = (global_transform.basis.x * smove + global_transform.basis.y * umove + -camera.global_transform.basis.z * fmove).normalized()
	else:
		wishdir = (global_transform.basis.x * smove + -head.global_transform.basis.z * fmove).normalized()
	
	if wishdir != Vector3.ZERO:
		velocity = velocity.linear_interpolate(wishdir * movespeed, ACCELERATE * delta) 
	else:
		velocity = velocity.linear_interpolate(Vector3.ZERO, MOVEFRICTION * delta) 
	
	velocity = move_and_slide(velocity)

