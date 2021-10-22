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

extends Spatial

onready var tile_cell = preload("res://Tile_Cell.tscn")

onready var tile_ceiling = $Ceiling
onready var tile_floor = $Floor

const SCALE = 2

var rng = RandomNumberGenerator.new()
var grid = []
export var grid_size : int = 20
var num = 1
var low = []
var dfsNum = []
var isArticulation = []
var neighbours = [[1, 0],[0, 1],[-1, 0],[0, -1]] #[RIGHT, UP, LEFT, DOWN]

var exit = Vector2.ZERO

enum { BLANK, PATH, END }
var type

"""
====================
_ready
====================
"""
func _ready():
	rng.randomize()
	initialize_grid(grid_size)
	generate_path()
	spawn_tiles()
	
	# Set floor/ceiling transforms
	tile_ceiling.scale = Vector3.ONE * grid_size
	tile_ceiling.global_transform.origin = Vector3(grid_size*SCALE/2.105, 2, grid_size*SCALE/2.105)
	tile_floor.scale = Vector3.ONE * grid_size
	tile_floor.global_transform.origin = Vector3(grid_size*SCALE/2.105, 0, grid_size*SCALE/2.105)

"""
====================
_input
====================
"""
func _input(event):
	if Input.is_key_pressed(KEY_P):
		reset_tiles()
		generate_path()

"""
====================
initialize_grid
====================
"""
func initialize_grid(count):
	for i in count:
		grid.append([])
		for j in count:
			grid[i].append(PATH)
	
	grid_size = len(grid)

"""
====================
spawn_tiles
====================
"""
func spawn_tiles():
	for x in grid_size:
		for y in grid_size:
			if grid[x][y] == BLANK:
				continue
			
			var tile = tile_cell.instance()
			add_child(tile)
			
			var count = 0
			for d in neighbours:
				var vx = x + d[0]
				var vy = y + d[1]
				var valid = is_valid(vx,vy)
				
				if !valid or (valid and grid[vx][vy] == BLANK):
					var child = tile.get_child(count)
					child.visible = true
					child.get_node("StaticBody/CollisionShape").disabled = false
				count += 1
			
			tile.global_transform.origin = Vector3(x, 0, y) * SCALE

"""
====================
reset_tiles
====================
"""
func reset_tiles():
	for i in range(grid_size):
		for j in range(grid_size):
			grid[i][j] = PATH

"""
====================
generate_path
====================
"""
func generate_path():
	# Initialize end points
	var ends = []
	for x in range(grid_size):
		ends.append([])
		for y in range(grid_size):
			ends[x].append(false)
	
#	var a0 = rng.randi_range(0, 3)
#	var a1 = rng.randi_range(0, 9)
#	ends[a0][a1] = true
#
#	var b0 = rng.randi_range(5, 9)
#	var b1 = rng.randi_range(0, 9)
#	ends[b0][b1] = true
	
	ends[0][0] = true
	ends[0][grid_size-1] = true
	ends[grid_size-1][0] = true
	ends[grid_size-1][grid_size-1] = true
	var h = int(grid_size/2)
	ends[h][h] = true
	var t = int(grid_size/3)
	ends[0][t] = true
	
	set_ends(ends)
	
	# Remove tiles in random order until only end and 
	# articulation points remain
	var a_points
	while true:
		a_points = find_articulation_points(ends)
		if !removable_paths(a_points):
			break
		remove_random_tile(a_points)

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
set_ends
====================
"""
func set_ends(ends):
	for i in range(grid_size):
		for j in range(grid_size):
			if ends[i][j]:
				grid[i][j] = END

"""
====================
removable_paths

Check if path array contains any points that are not articulation points
====================
"""
func removable_paths(a_points):
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
	var i = rng.randi_range(0,grid_size-1)
	var j = rng.randi_range(0,grid_size-1)
	
	if grid[i][j] == PATH and !ap[i][j]:
		grid[i][j] = BLANK
		#print("removed cell: [" + str(ri) + "][" + str(rj) + "]")

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
