using Godot;

/* 
    Main

    - Main controller for the active game tree
    - Ensures initialization methods ("_Ready()") are called in correct order 
    - Starts maps
*/

public class Main : Node
{
    private KinematicBody player;
    private Maze_Generator map;
    private Console console;
    private ConsoleCommands commands;

    private Spatial[] maps;

    /*
    ====================
    _Ready
    ====================
    */
    public override void _Ready()
    {
        player = GetNode("Player") as KinematicBody;
        map = GetNode("Level") as Maze_Generator;
        console = GetNode("Console") as Console;
        commands = new ConsoleCommands(console, map, player);
        console.SetCommands(commands);
        console.Run();

        map.Run();
        map.GetRandomMap();
        map.StartMap(map.GetMapName(), true);
    }


    // public void CheckMap()
    // {
    //     Vector3 map = MapPosition(player.GlobalTransform.origin);

    //     if(grid[(int)map[0], (int)map[2]])
    //     {
    //         if(player.GlobalTransform.origin.x > 120)
    //         {
    //             Transform t = player.GlobalTransform;
    //             t.origin.x = -120;
    //             player.GlobalTransform = t;
    //         }
            
    //         if(player.GlobalTransform.origin.x < -120)
    //         {
    //             Transform t = player.GlobalTransform;
    //             t.origin.x = 120;
    //             player.GlobalTransform = t;
    //         }
            
    //         if(player.GlobalTransform.origin.z > 120)
    //         {
    //             Transform t = player.GlobalTransform;
    //             t.origin.z = -120;
    //             player.GlobalTransform = t;
    //         }
            
    //         if(player.GlobalTransform.origin.z < -120)
    //         {
    //             Transform t = player.GlobalTransform;
    //             t.origin.z = 120;
    //             player.GlobalTransform = t;
    //         }
    //     }
    // }

    // public void OriginOffset()
    // {
    //     const int MAXRANGE = 200;        

    //     for(int i = 0; i < 3; i++)
    //     {
    //         if(i == 1)
    //         {
    //             continue;
    //         }

    //         int offset = 0;

    //         if(player.GlobalTransform.origin[i] > MAXRANGE)
    //         {
    //             offset = -MAXRANGE;
    //         }
    //         else if (player.GlobalTransform.origin[i] < -MAXRANGE)
    //         {
    //             offset = MAXRANGE;
    //         }
    //         else
    //         {
    //             continue;
    //         }

    //         Transform t = player.GlobalTransform;
    //         t.origin[i] = offset;
    //         player.GlobalTransform = t;
            
    //         for(int m = 0; m < maps.Length; m++)
    //         {
    //             t = maps[m].GlobalTransform;
    //             t.origin[i] += offset * 2;
    //             maps[m].GlobalTransform = t;
    //         }
    //     }
    // }

    // public Vector3 LocalizedPosition(Vector3 pos)
    // {
    //     Vector3 localPos = new Vector3();

    //     for(int i = 0; i < 3; i++)
    //     {
    //         localPos[i] = (int)(pos[i]/10 + 0.5f);
    //     }

    //     return localPos;
    // }

    // public Vector3 MapPosition(Vector3 pos)
    // {
    //     Vector3 localPos = Vector3.Zero;

    //     for(int i = 0; i < 3; i++)
    //     {
    //         // float c = pos[i] < 0 ? 0.5f : -0.5f;
    //         localPos[i] = (int)(pos[i]/400 + 0.5f);
    //     }

    //     return localPos + new Vector3(1,0,1);
    // }

}
