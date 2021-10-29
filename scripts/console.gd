extends Node2D

var console_height : float = 0.5
var console_speed : float = 15.0
onready var config_resolution : Vector2 = get_viewport().size
onready var console = $Console_Text
onready var console_input = $Console_Input
onready var foot = $Console_Footer
#onready var tunnel = $Tunnel
onready var squares = $Squares
onready var solid_color = $Color
onready var config = $"../Config"
#onready var target_window = $"../target_loading_window/Target_window"

var console_textures = []
var c_lines : PoolStringArray
var input_history : PoolStringArray
var display_console : bool = false
var quitting : bool = false
var pause_menu : bool = false
var history_pos = 0
var display_size = 0

const MAX_LINE_COUNT = 200
const maxres : Vector2 = Vector2(1920, 1080)
const line_width : float = 2.0

"""
==================
_ready
==================
"""
func _ready():
	resize_elements()
	c_lines = PoolStringArray(console.text.split("\n"))
	display_size = c_lines.size()-1
	console_input.text = ""
	
	load_console_textures()
	randomize()
	#tunnel.texture = console_textures[rand_range(0, console_textures.size()-1)]
	squares.get_material().set_shader_param("color", Color(0.007843, 0.470588, 0.631373))

"""
==================
_input
==================
"""
func _input(_event):
	#if quitting or pause_menu: return
	if quitting: 
		return
	
	if !config.console:
		return
	
	if Input.is_action_just_pressed("console"):
		if display_console:
			if !get_parent().pause_game:
				get_parent().get_parent().pause_game(false)
			display_console = false
			console_input.release_focus()
		else:
			get_parent().get_parent().pause_game(true)
			display_console = true
			console_input.grab_focus()
		# Don't add tilde to line edit
		get_tree().set_input_as_handled()
	
	if Input.is_action_pressed("console"):
		get_tree().set_input_as_handled()
	
	if display_console:
		
		# Cycle input history
		if input_history.size() > 0:
			if Input.is_action_just_pressed("ui_up"):
				display_previous(-1)
				get_tree().set_input_as_handled()
			if Input.is_action_just_pressed("ui_down"):
				display_previous(1)
				get_tree().set_input_as_handled()
		
		# Scroll console up down
		if Input.is_action_pressed("ui_page_up"):
			display_size = clamp(display_size - 1, 1, c_lines.size()-1)
			update_console_view()
		if Input.is_action_pressed("ui_page_down"):
			display_size = clamp(display_size + 1, 1, c_lines.size()-1)
			update_console_view()
		
		# Input command
		if Input.is_action_just_pressed("ui_accept"):
			if console_input.text.strip_edges(true, true) == "":
				console_input.text = "]"
			else:
				input_history.append(console_input.text)
				if input_history.size() > MAX_LINE_COUNT:
					input_history.remove(input_history.size()-1)
				history_pos = input_history.size()
			
			add_to_console(console_input.text)
			input_reader(console_input.text)
			console_input.clear()

"""
==================
_process
==================
"""
func _process(_delta):
	transform.origin.y += console_speed if display_console else -console_speed
	transform.origin.y = clamp(transform.origin.y, 0, config_resolution.y * console_height)
	
	if transform.origin.y <= 0:
		foot.hide()
	else:
		foot.show()

"""
==================
add_to_console
==================
"""
func add_to_console(s):
	c_lines.append(s)
	display_size += 1
	update_console_view()
"""
==================
terminate (exit game)
==================
"""
func terminate():
	add_to_console("Quitting...")
	console_input.release_focus()
	quitting = true
	quit_wait(2.0)

"""
==================
quit_wait
==================
"""
func quit_wait(time) -> void:
	yield(get_tree().create_timer(time), "timeout")
	get_tree().notification(MainLoop.NOTIFICATION_WM_QUIT_REQUEST)

"""
==================
display_previous
==================
"""
func display_previous(p):
	history_pos = clamp(history_pos + p, 0, input_history.size()-1)
	console_input.text = input_history[history_pos]
	console_input.caret_position = console_input.text.length()

"""
==================
update_console_view
==================
"""
func update_console_view():
	console.text = ""
	for i in range(display_size + 1):
		console.text += c_lines[i] + ("\n" if i != display_size else "")

"""
==================
load_images
==================
"""
func load_console_textures():
	add_to_console("--------------------------")
	add_to_console("loading console textures..")
	add_to_console("--------------------------")
	
	var dir = Directory.new()
	if dir.open("res://textures/console/") == OK:
		dir.list_dir_begin()
		var file = dir.get_next()
		var count = 0
		while(file != ""):
			if file.ends_with(".jpg") or file.ends_with(".png"):
				add_to_console(file + " : " + str(count))
				count += 1
				console_textures.append(load("res://textures/console/" + file))
			file = dir.get_next()

"""
==================
input_reader
==================
"""
func input_reader(s):
	var cmd = s.to_lower().split(" ")
	var split_string = cmd[0].split("_")
	match split_string[0]:
		"console":
			if split_string.size() >= 2:
				console_settings(split_string[1], cmd)
#		"loading":
#			if split_string.size() >= 2:
#				loading_screen(split_string[1], cmd)
		"quit":
			terminate()
		"exit":
			terminate()

#func loading_screen(s, cmd):
#	match s:
#		"exit":
#			if cmd[1] == "true" or cmd[1] == "1":
#				target_window.exit_sequence = true
#			elif cmd[1] == "false" or cmd[1] == "0":
#				target_window.exit_sequence = false

"""
==================
console_settings
==================
"""
func console_settings(s, cmd):
	#var cl = cmd[0].split("_")
	match s:
		"height":
			if cmd.size() > 1 and cmd[1].is_valid_float():
				console_height = clamp(float(cmd[1]), 0.1, 1.0) 
		"speed":
			if cmd.size() > 1 :
				if cmd[1].is_valid_float():
					console_speed = clamp(float(cmd[1]), 5, 100.0)
		"colour":
			if cmd.size() > 3: 
				if cmd[1].is_valid_float() and cmd[2].is_valid_float() and cmd[3].is_valid_float():
					var r = clamp(float(cmd[1]), 0.0, 1.0)
					var g = clamp(float(cmd[2]), 0.0, 1.0)
					var b = clamp(float(cmd[3]), 0.0, 1.0)
					#tunnel.get_material().set_shader_param("color", Color(r, g, b))
					squares.get_material().set_shader_param("color", Color(r, g, b))
			elif cmd.size() > 1:
				if cmd[1].is_valid_float():
					#tunnel.get_material().set_shader_param("color", Color(cmd[1]))
					squares.get_material().set_shader_param("color", Color(cmd[1]))
		"texture":
			if cmd.size() > 1 :
				if cmd[1].is_valid_float():
					var i = clamp(int(cmd[1]), 0, console_textures.size()-1)
					#tunnel.texture = console_textures[i]
				elif cmd[1].to_lower() == "random":
					pass
					#tunnel.texture = console_textures[rand_range(0, console_textures.size()-1)]
		"text":
			if cmd.size() > 3: 
				if cmd[1].is_valid_float() and cmd[2].is_valid_float() and cmd[3].is_valid_float():
						var r = clamp(float(cmd[1]), 0.0, 1.0)
						var g = clamp(float(cmd[2]), 0.0, 1.0)
						var b = clamp(float(cmd[3]), 0.0, 1.0)
						console.set("custom_colors/font_color", Color(r,g,b))
						console_input.set("custom_colors/font_color", Color(r,g,b))
			elif cmd.size() > 1:
				if cmd[1].is_valid_float():
					console.set("custom_colors/font_color", Color(cmd[1]))
					console_input.set("custom_colors/font_color", Color(cmd[1]))
		"brightness":
			if cmd.size() > 1 and cmd[1].is_valid_float():
				squares.get_material().set_shader_param("brightness", clamp(float(cmd[1]), 0.0, 5.0))
				#tunnel.get_material().set_shader_param("brightness", clamp(float(cmd[1]), 0.0, 5.0))
		"contrast":
				if cmd.size() > 1 and cmd[1].is_valid_float():
					squares.get_material().set_shader_param("contrast", clamp(float(cmd[1]), 0.0, 5.0))
					#tunnel.get_material().set_shader_param("contrast", clamp(float(cmd[1]), 0.0, 5.0))
		"saturation":
				if cmd.size() > 1 and cmd[1].is_valid_float():
					squares.get_material().set_shader_param("saturation", clamp(float(cmd[1]), -10.0, 100.0))
					#tunnel.get_material().set_shader_param("saturation", clamp(float(cmd[1]), -10.0, 100.0))
		"bkg":
			if cmd.size() > 1:
#				if cmd[1] == "tunnel":
#					tunnel.show()
#					squares.hide()
				if cmd[1] == "squares":
					#tunnel.hide()
					squares.show()
				else:
					solid_color.show()

"""
==================
resize_elements
==================
"""
func resize_elements():
	#var x_scale = config_resolution.x/maxres.x
	#var y_scale = config_resolution.y/maxres.y
	#tunnel.scale = Vector2(x_scale, -x_scale)
	#tunnel.region_rect.size = config_resolution
	config_resolution = get_viewport().size
	foot.rect_size.x = config_resolution.x
