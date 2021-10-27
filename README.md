# Godot Random Path Generator
Generates a randomized path that connects to various ends points. The end goal of the project is an efficient random 3D dungeon/maze generator with walls, ceilings and exit zones.

The code uses the "Chiseled Random Paths" algorithm from BorisTheBrave's project: https://github.com/BorisTheBrave/chiseled-random-paths
<br>
This script uses weighted randomization which edits the probability of specific tiles to be randomly selected (for deleting or state change). This allows for a somewhat more natural randomization if you edit the probability values right e.g. a higher probability of isolated tiles to be selected for removal.
<br>

This project is under the MIT license.
<br>
This project uses textures from the Pixar128 set by Pixar Animation Studios: https://renderman.pixar.com/pixar-one-twenty-eight
<br>
The textures are under the Creative Commons 4.0 Attribution License: https://creativecommons.org/licenses/by/4.0/
<br>
<div>
  <img src="https://imgur.com/EBIWFST.jpg" width="200px" height="auto">
  &nbsp
  <img src="https://imgur.com/MgUCGKP.jpg" width="200px" height="auto">
</div>

<img src="https://imgur.com/5XJuvQF.jpg" width="400px" height="auto">

Fog and weighted randomization:
<br>
<img src="https://imgur.com/yTUWto8.jpg" width="400px" height="auto">
<br>
<img src="https://imgur.com/3XbQUbQ.jpg" width="400px" height="auto">

Weighted randomization with 10x scale:
<br>
<img src="https://imgur.com/hUJBGs7.jpg" width="400px" height="auto">

# Controls
WASD - Move 
<br>
Mouse - Look
<br>
P - Generate new maze

# Current Issues and TODOs
  - Requires Mono version of Godot to build/edit C# scripts
  - Stack overflow when chiseling grids larger than 70x70, will crash the engine.
  - Player collision is buggy
