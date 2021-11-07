extends Node

#onready var mat : ShaderMaterial = preload("res://materials/Material_Laser_Red.tres")
export var start : Vector2
export var end : Vector2

onready var rng = RandomNumberGenerator.new()

onready var map = get_parent()

var lasers = []

func _ready():
	rng.randomize()

func set_lasers():
	cell_selector(start, end)

func _process(delta):
	pass
	#draw_lasers()

func cell_selector(var from, var to):
	lasers = []
	for x in range(from.x, to.x):
		for y in range(from.y, to.y):
			if map.IsValid(x,y) and map.GetTileType(x,y) != "BLANK":
				var n = get_blank_neighbours(x, y)
				
				var dir
				if n[0] == 1 and n[2] == 1:
					dir = Vector3(0.5, 0, 0)
				elif n[1] == 1 and n[3] == 1:
					dir = Vector3(0, 0, 0.5)
				else:
					continue
				
				var a = (Vector3(x, rng.randf_range(0.05, 0.45), y) + dir) * 10 
				var b = (Vector3(x, rng.randf_range(0.05, 0.45), y) - dir) * 10 
				
				lasers.append([a,b])

func get_blank_neighbours(var x, var y):
	var n = [0,0,0,0]
	var adj = [[1,0],[0,1],[-1,0],[0,-1]]
	for i in range(4):
		var vx = x + adj[i][0]
		var vy = y + adj[i][1]
		
		if (!map.IsValid(vx,vy) or map.GetTileType(vx,vy) == "BLANK"):
			n[i] = 1
		
	return n

func draw_lasers():
	pass
	#for i in range(len(lasers)):
		#DebugDraw.draw_line_3d(lasers[i][0], lasers[i][1], Color.aqua);

#func add_laser(x, y):
#	var wall1 = Vector2(x,y) * 10 + Vector2(0.5,0);
#	var wall2 = Vector2(x,y) * 10 - Vector2(0.5,0);
#	lasers.append([wall1, wall2])
