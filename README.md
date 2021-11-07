# Godot Random Path Generator
<div>
  <img src="https://imgur.com/EBIWFST.jpg" width="200px" height="auto">
  &nbsp
  <img src="https://imgur.com/MgUCGKP.jpg" width="200px" height="auto">
</div>
Generates a randomized path that connects to various ends points. The end goal of the project is an efficient random 3D dungeon/maze generator with walls, ceilings and exit zones.
<br>
<br>
Path generation is heavily based on the "Chiseled Random Paths" idea from BorisTheBrave's project: https://github.com/BorisTheBrave/chiseled-random-paths
<br>
<br>
This script incorporates weighted randomization which sets the probability of specific tiles to be randomly selected (for deleting or state change). This allows for a somewhat more natural randomization if you edit the probability values right e.g. a higher probability of isolated tiles to be selected for removal.
<br>
<br>
Image reading functionality has been added. Map images can be used to create semi-random environments. Images must only contain white and black values and have full alpha; any given pixel colour value must be either rgba(255,255,255,255) or rgba(0,0,0,255). Images must be in .png format. You can use MS Paint or any other drawing program to create maps. Image size must be relatively small as the dimensions reflect the grid size e.g. 40px = 40 rows/columns = 800 cells. Large images will probably crash.
<br>
Maps can be chiseled to custom amount. The maps contained in this project follow the conventions of "name -> cuts -> format" e.g. "map03_350.png"
Cuts refers to the maximum number of times to set a random tile from the grid to "BLANK" during path randomization. Less cuts means more open spaces, more cuts means more enclosed spaces.
<br>
<br>
This project is under the MIT license.
<br>
This project uses textures from the Pixar128 set by Pixar Animation Studios: https://renderman.pixar.com/pixar-one-twenty-eight
<br>
The textures are under the Creative Commons 4.0 Attribution License: https://creativecommons.org/licenses/by/4.0/
<br>
<br>

<img src="https://imgur.com/5XJuvQF.jpg" width="400px" height="auto">
<br>
<br>
Weighted randomization:
<br>
<div>
  <img src="https://imgur.com/yTUWto8.jpg" width="400px" height="auto">
  &nbsp
  <img src="https://imgur.com/3XbQUbQ.jpg" width="400px" height="auto">
</div>
<br>
Weighted randomization with 10x scale:
<br>
<img src="https://imgur.com/hUJBGs7.jpg" width="400px" height="auto">

# Controls
  - WASD - Move 
  - Mouse - Look
  - N - Noclip
  - ~ - Open console
    - "map restart" - Restart map
    - "map list" - Show maps available to load
    - "map map03_150 closed" - Loads a map with 150 chisels applied and generates boundary walls to confine the player inside the maze.
    - "map map03_650 open" - Loads a map with a custom number of chisels applied (650) but <strong>does not</strong> generate boundary walls to keep the player inside the maze,
    - "console_speed x" - Console window open/close speed ==> 0.0 - 100.0
    - "console_height x" - Console window screen height ==> 0.0 - 1.0
    - "console_texture random" - Randomize console background texture

# Current Issues and TODOs
  - CAUTION: Stack overflow when chiseling large grids. Try to stick to under 1000 cuts when generating maps.
  - Very long load times when generating over 1000 cuts.
  - CONSOLE MAY BE BUGGY
  - Code is still a bit of a mess and needs further cleaning up.
  - Need to reduce coupling.
  - I am experiencing laggy inputs on my machine, although this happens to me with every project with Godot, so hopefully it's just a problem on my end only.
  
# Notes  
  - Requires Mono version of Godot to build/edit C# scripts
  - The scale const in 'Maze_Generator' can be used to inflate small grids.
  - Using edited version of Q_Move: https://github.com/Btan2/Q_Move/blob/main/scripts/pmove.gd
  
