# Godot Random Path Generator
Generates a randomized path that connects to various ends points. The end goal of the project is an efficient random 3D dungeon/maze generator with walls, ceilings and exit zones.

The code uses the "Chiseled Random Paths" algorithm from BorisTheBrave's project (converted to GDScript): https://github.com/BorisTheBrave/chiseled-random-paths

This project is under the MIT license.
This project uses textures from the Pixar128 set by Pixar Animation Studios: https://renderman.pixar.com/pixar-one-twenty-eight
<br>
The textures are under the Creative Commons 4.0 Attribution License: https://creativecommons.org/licenses/by/4.0/

<br>
<img src="https://imgur.com/EBIWFST.jpg" width="200px" height="auto">
<img src="https://imgur.com/MgUCGKP.jpg" width="200px" height="auto">
<img src="https://imgur.com/5XJuvQF.jpg" width="400px" height="auto">

With fog and weighted randomization:
<br>
<img src="https://imgur.com/yTUWto8.jpg" width="400px" height="auto">

# Controls
WASD - Move 
<br>
Mouse - Look
<br>
P - Generate new maze

# Current Issues and TODOs
  - Stack overflow when chiseling grids larger than 70x70, will crash the engine.
  - Player collision is buggy
