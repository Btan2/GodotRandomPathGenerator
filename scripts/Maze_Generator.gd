"""
MIT License

Copyright (c) 2018 Adam Newgas

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Source: https://github.com/BorisTheBrave/chiseled-random-paths
"""

"""

!!!!!!!    WARNING: REDUNDANT CODE, USE C# SCRIPT INSTEAD    !!!!!!!

"""

extends Node

onready var material_redbrick = preload("res://Mat_RedBricks.tres")
onready var t_floor = $Map/Floor
onready var player = $Player

const SCALE = 2
const WALLHEIGHT = 2
const MAXEMPTY = 1.0

var rng = RandomNumberGenerator.new()
var num = 1
var grid_size = 40;
var total_weight = 0;
var weights = []
var low = []
var dfsNum = []
var isArticulation = []
var grid = []

enum { BLANK, PATH, END }

"""
====================
_ready
====================
"""
func _ready():
	rng.randomize()
	new_grid()

"""
====================
_input
====================
"""
func _input(event):
	if Input.is_key_pressed(KEY_P):
		new_grid()

"""
====================
new_grid
====================
"""
func new_grid():
	set_player_pos(Vector3.UP)
	set_floor()
	generate_path()
	create_multimesh()

"""
====================
set_player_pos
====================
"""
func set_player_pos(pos : Vector3):
	player.global_transform.origin = pos

"""
====================
set_floor
====================
"""
func set_floor():
	t_floor.scale = Vector3.ONE * (grid_size+1)
	t_floor.global_transform.origin = Vector3(grid_size*SCALE/2, 0, grid_size*SCALE/2)

"""
====================
create_multimesh

Creates a multi mesh of wall tiles with static trimesh colliders
====================
"""
func create_multimesh():
	var mesh = PlaneMesh.new()
	mesh.size = Vector2(1,WALLHEIGHT) * SCALE
	mesh.surface_set_material(0, material_redbrick)
	
	var multiMesh = MultiMesh.new()
	multiMesh.transform_format = MultiMesh.TRANSFORM_3D
	multiMesh.color_format = MultiMesh.COLOR_FLOAT
	multiMesh.mesh = mesh
	multiMesh.instance_count = 0
	
	var transform_arr = []
	var r = [90,0,-90,180]
	var p = [[1,0],[0,1],[-1,0],[0,-1]]
	
	for x in grid_size:
		for y in grid_size:
			if grid[x][y] == BLANK:
				continue
			
			for i in range(4):
				var vx = x + p[i][0]
				var vy = y + p[i][1]
				
				var valid = is_valid(vx,vy)
				if !valid or (valid and grid[vx][vy] == BLANK):
					multiMesh.instance_count += 1
					
					var t = Transform()
					t.basis = Basis()
					t.basis = t.basis.rotated(Vector3(1,0,0), deg2rad(-90))
					t.basis = t.basis.rotated(Vector3(0,1,0), deg2rad(r[i]))
					t.origin = Vector3(p[i][0], WALLHEIGHT, p[i][1]) + Vector3(x, 0, y) * SCALE
					transform_arr.append(t)
	
	var mmi = MultiMeshInstance.new()
	mmi.multimesh = multiMesh
	var shape = multiMesh.mesh.create_trimesh_shape()
	var collisionNode = StaticBody.new()
	mmi.add_child(collisionNode)
	
	for i in multiMesh.instance_count:
		multiMesh.set_instance_transform(i, transform_arr[i])
		var collisionShape = CollisionShape.new()
		collisionShape.shape = shape
		collisionShape.transform = transform_arr[i]
		collisionNode.add_child(collisionShape)
	
	add_child(mmi)

"""
====================
generate_path
====================
"""
func generate_path():
	var ends = []
	grid = []
	for x in range(grid_size):
		ends.append([])
		grid.append([])
		for y in range(grid_size):
			grid[x].append(PATH)
			ends[x].append(false)
	
	ends[0][0] = true
	grid[0][0] = END
	
	var map = $Map
	for child in map.get_children():
		if child is Spatial:
			var x = int(child.global_transform.origin.x/2)
			var y = int(child.global_transform.origin.z/2)
			ends[x][y] = true
			grid[x][y] = END
	
	initialize_weights()
	
	var a_points
	var count = 0
	#var max_removed = int(((grid_size * grid_size) / 2) * MAXEMPTY)
	var max_removed = 250
	while count < max_removed:
		a_points = find_articulation_points(ends)
		if !removable_tiles(a_points):
			break
		remove_random_tile(a_points)
		count += 1

"""
====================
removable_tiles

Check if path array contains any points that are not articulation points
====================
"""
func removable_tiles(a_points):
	for i in range(grid_size):
		for j in range(grid_size):
			if grid[i][j] == PATH and !a_points[i][j]:
				return true
	return false

"""
====================
remove_random_tile

Removes a random tile from the grid
Only removes path tiles that are not articulation points
====================
"""
func remove_random_tile(ap):
	var r = get_random_tile()
	var rx = r[0]
	var ry = r[1]
	
	if grid[rx][ry] == PATH and !ap[rx][ry]:
		grid[rx][ry] = BLANK

"""
====================
get_random_tile
====================
"""
func get_random_tile():
	var roll = rng.randf_range(0, total_weight)
	for x in range(grid_size):
		for y in range(grid_size):
			if weights[x][y] > roll:
				return [x,y]
	return []

"""
====================
initialize_weights
====================
"""
func initialize_weights():
	total_weight = 0
	weights = []
	
	for x in range(grid_size):
		weights.append([])
		for y in range(grid_size):
			total_weight += get_roll_weight(x,y)
			weights[x].append(total_weight)

"""
====================
get_roll_weight
====================
"""
func get_roll_weight(x : int, y : int):
	var d = [[1,0],[0,1],[-1,0],[0,-1]]
	var n = 0
	for i in range(4):
		var vx = x + d[i][0]
		var vy = y + d[i][1]
		
		if !is_valid(vx,vy):
			continue
		
		if grid[vx][vy] == PATH or grid[vx][vy] == END:
			n += 1
	
	match n:
		0:
			return 0.0
		1:
			return 1.0
		2:
			return 0.4
		3:
			return 0.5
		4:
			return 0.3
		_:
			return 0.3

"""
====================
is_valid
====================
"""
func is_valid(row, col):
	if row >= 0 and row < grid_size and col >= 0 and col < grid_size:
		return true
	return false

"""
====================
find_articulation_points
====================
"""
func find_articulation_points(relevant):
	num = 1
	low = []
	dfsNum = []
	isArticulation = []
	
	for x in range(grid_size):
		low.append([])
		dfsNum.append([])
		isArticulation.append([])
		for y in range(grid_size):
			low[x].append([])
			dfsNum[x].append([])
			isArticulation[x].append(false)
	
	# Find starting point
	for x in range(grid_size):
		for y in range(grid_size):
			if grid[x][y] == BLANK:
				continue
			if len(relevant) > 0 and !relevant[x][y]:
				continue
			var childCount = cut_vertex(x,y, relevant)[0]
			#var childRelevantSubtree = cut_vertex(x,y, relevant)[1]
			
			if childCount > 1:
				isArticulation[x][y] = true
			elif len(relevant) == 0:
				isArticulation[x][y] = false
			return isArticulation;
	
	# No relevant points, or no walkable points
	return isArticulation

"""
====================
cut_vertex
====================
"""
func cut_vertex(ux : int, uy : int, relevant):
	var childCount = 0;
	var isRelevant = relevant[ux][uy]
	if isRelevant:
		isArticulation[ux][uy] = true
	
	var isRelevantSubtree = isRelevant
	num += 1
	low[ux][uy] = num
	dfsNum[ux][uy] = num
	
	var neighbours = [[1, 0],[0, 1],[-1, 0],[0, -1]]
	for d in neighbours:
		var vx = ux + d[0]
		var vy = uy + d[1]
		if vx < 0 or vx >= grid_size or vy < 0 or vy >= grid_size:
			 continue
		if grid[vx][vy] == BLANK:
			continue
		 
		# v is a neighbour of u
		var unvisited = !dfsNum[vx][vy];
		if unvisited:
			var childRelevantSubtree = cut_vertex(vx, vy, relevant)[1]
			childCount += 1
			if childRelevantSubtree:
				isRelevantSubtree = true;
			if low[vx][vy] >= dfsNum[ux][uy]:
				if len(relevant) == 0 or childRelevantSubtree:
					isArticulation[ux][uy] = true
			low[ux][uy] = min(low[ux][uy], low[vx][vy])
		else:
			low[ux][uy] = min(low[ux][uy], dfsNum[vx][vy])
	
	return [childCount, isRelevantSubtree]

#"""
#====================
#set_ends
#====================
#"""
#func set_ends(ends):
#	for i in range(grid_size):
#		for j in range(grid_size):
#			if ends[i][j]:
#				grid[i][j] = END
