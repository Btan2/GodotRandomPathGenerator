extends MeshInstance

enum { BLANK, PATH, END }
var type

"""
====================
set_type
====================
"""
func set_type(tstring : String):
	match(tstring):
		"PATH":
			type = PATH
			set_color(Color.green)
		"BLANK":
			type = BLANK
			set_color(Color.burlywood)
		"END":
			type = END
			set_color(Color.blueviolet)
		_:
			type = BLANK
			set_color(Color.burlywood)

"""
====================
set_color
====================
"""
func set_color(color : Color):
	var mat = get_surface_material(0)
	mat.albedo_color = color
	set_surface_material(0, mat)

"""
====================
get_grid_pos

Get position of the tile in grid coordinates
====================
"""
func get_grid_pos(scale : int):
	return [
		int(global_transform.origin[0]/scale), 
		int(global_transform.origin[1]/scale),
		]

#"""
#====================
#set_xy
#====================
#"""
#func set_xy(x : int, y : int):
#	self.x = x
#	self.y = y
#
#func get_pos():
#	return Vector2(_x,_y)
