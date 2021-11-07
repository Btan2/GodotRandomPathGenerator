tool
extends Node

## @brief How many frames lines remain shown after being drawn.
const LINES_LINGER_FRAMES = 1

var _lines := []
var _line_material_pool := []

var _mat_laser = preload("res://materials/Material_Laser.tres")

func draw_line_3d(a: Vector3, b: Vector3, color: Color):
	var g = ImmediateGeometry.new()
	g.material_override = _get_line_material()
	g.begin(Mesh.PRIMITIVE_LINES)
	g.set_color(color)
	g.add_vertex(a)
	g.add_vertex(b)
	g.add_vertex(b + Vector3(0,0.25,0))
	g.add_vertex(a + Vector3(0,0.25,0))
	g.end()
	add_child(g)
	_lines.append({
		"node": g,
		"frame": Engine.get_frames_drawn() + LINES_LINGER_FRAMES,
	})

func _get_line_material() -> SpatialMaterial:
	var mat : SpatialMaterial
	if len(_line_material_pool) == 0:
		mat = SpatialMaterial.new()
		mat.flags_unshaded = false
		mat.vertex_color_use_as_albedo = true
	else:
		mat = _line_material_pool[-1]
		_line_material_pool.pop_back()
	return mat

func _recycle_line_material(mat: SpatialMaterial):
	_line_material_pool.append(mat)

func _process_3d_lines_delayed_free(items: Array):
	var i := 0
	while i < len(items):
		var d = items[i]
		if d.frame <= Engine.get_frames_drawn():
			_recycle_line_material(d.node.material_override)
			d.node.queue_free()
			items[i] = items[len(items) - 1]
			items.pop_back()
		else:
			i += 1

func _process(delta: float):
	_process_3d_lines_delayed_free(_lines)
