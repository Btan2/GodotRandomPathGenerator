# Godot Random Path Generator
Generates a randomized path that connects to various ends points. The end goal is a random 3D dungeon/maze generator with walls, ceilings and exit zones.
The code uses two functions from BorisTheBrave's "Chiseled Random Path" project converted to GDScript: https://github.com/BorisTheBrave/chiseled-random-paths

The project is under the MIT license.

<br>
<img src="https://imgur.com/EBIWFST.jpg" width="200px" height="auto"></img> 
<img src="https://imgur.com/MgUCGKP.jpg" width="200px" height="auto"></img>

# Controls
Press "P" to generate a random path between the purple end points.

# Current Issues
Grids that have upwards of 100 cells will take several seconds to generate as no optimization has been performed yet.
