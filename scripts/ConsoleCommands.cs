using Godot;

public class ConsoleCommands
{
    private Console console;
    private Maze_Generator map;
    private KinematicBody player;
    private RandomNumberGenerator rng;

    /*
    ==================
    New
    ==================
    */
    public ConsoleCommands(Console console, Maze_Generator map, KinematicBody player)
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();

        this.console = console;
        this.map = map;
        this.player = player;
    }

    /*
    ==================
    Read
    ==================
    */
    public void Read(string s)
    {
	    string[] command = s.ToLower().Split(" ");
	    string[] split_string = command[0].Split("_");
	    switch(split_string[0])
        {
             case "console":
                 if (split_string.Length >= 2)
                 {
                     ConsoleOptions(split_string[1],command);
                 }
                 break;

            case "cl":
                QuickOptions(split_string[1], command);
                break;
            
            case "map":
                MapOptions(command);
                break;
            
            case "player":
                if (split_string.Length >= 2)
                {
                    PlayerOptions(split_string[1],command);
                }
                break;                        

            default:
                break;
        }     
    }

    /*
    ==================
    QuickOptions
    ==================
    */
    private void QuickOptions(string s, string[] cm)
    {
        switch(s)
        {
            case "clear":
                console.Clear();
                break;
            
            case "remove":
                if(cm.Length > 1 && cm[1].IsValidInteger())
                {
                    console.RemoveLine(cm[1].ToInt());
                } 
                break;
            case "position":
                console.ShowCurrentPos();
                break;
            
            default:
                break;
        }
    }

    /*
    ==================
    ConsoleOptions
    ==================
    */
    private void ConsoleOptions(string s, string[] cm)
    {
        switch(s)
        {
            case "texture":
                if(cm.Length > 1)
                {
                    if(cm[1].IsValidInteger())
                    {
                        console.SetBKGTexture(cm[1].ToInt());
                    }
                    else if (cm[1].ToLower() == "random")
                    {
                        int r = rng.RandiRange(0,9);
                        console.SetBKGTexture(r);
                    }
                }       
                break;
            
            case "colour":
                    if(cm.Length > 3)
                    {
                        if(cm[1].IsValidFloat() && cm[2].IsValidFloat() && cm[3].IsValidFloat())
                        {
                            console.SetColour(cm[1].ToFloat(), cm[2].ToFloat(), cm[3].ToFloat());
                        }
                    }
                break;

            case "speed":
                if(cm.Length > 1 && cm[1].IsValidFloat())
                {
                    console.SetSpeed(cm[1].ToFloat());
                }
                break;

            case "height":
                if(cm.Length > 1 && cm[1].IsValidFloat())
                {
                    console.SetHeight(cm[1].ToFloat());
                }
                break;   

            case "lines":
                if(cm.Length > 2)
                {
                    if(cm[1] == "max_history" && cm[2].IsValidInteger())
                    {
                        console.SetMaxHistory(cm[2].ToInt());
                    }
                    if(cm[1] == "max_lines" && cm[2].IsValidInteger())
                    {
                        console.SetMaxLines(cm[2].ToInt());
                    }
                }
                break;

            default:
                break;
        }
    }

    /*
    ==================
    PlayerOptions
    ==================
    */
    private void PlayerOptions(string s, string[] command)
    {
        switch(s)
        {
            case "teleport":
                if(command.Length > 1)
                {
                    float[] pos = new float[]{0f,0f,0f};
                    for(int i = 1; i < command.Length; i++)
                    {
                        if(command[i].IsValidFloat())
                        {
                            pos[i-1] = command[i].ToFloat();
                        }                        
                    }

                    MovePlayerTo(pos[0],pos[1],pos[2]);
                }
                break;

            default:
                break;
        }
    }

    /*
    ====================
    MovePlayerTo
    ====================
    */    
    public void MovePlayerTo(float p1, float p2, float p3)
    {
        // int x = (int)Mathf.Clamp(p1, 0, map.GetMapWidth()-1);
        // int y = (int)Mathf.Clamp(p2, 0, 100);
        // int z = (int)Mathf.Clamp(p3, 0, map.GetMapHeight()-1);    

        // if (map.IsBlank(x,z))
        // {
        //     console.Print("!!!!![Grid position is inside solid]!!!!!");
        //     return;
        // }

        var t = player.GlobalTransform;
        // int scale = map.GetMapScale();
        // t.origin = new Vector3(x * scale, y, z * scale);
        t.origin = new Vector3(p1,p2,p3);
        player.GlobalTransform = t;
    }

    /*
    ==================
    MapOptions
    ==================
    */
    private void MapOptions(string[] command)
    {
        if (command.Length >= 3)
        {
            if (command[2] == "closed")
            {
                map.StartMap(command[1], true);
            }
            else if (command[2] == "open")
            {
                map.StartMap(command[1], false);
            }
            else
            {
                map.StartMap(command[1], false);
            }
        }
        else if(command.Length >= 2)
        {
            if (command[1] == "ls" || command[1] == "list")
            {
                PrintMapNames();
            }
            else if(command[1] == "restart")
            {
                map.StartMap(map.GetMapName(), map.IsEnclosed());
            }
            else if (command[1] == "info")
            {
                console.Print("--------------------------");   
                console.Print("- Name: " + map.GetMapName());
                console.Print("- Width: " + map.GetMapWidth());
                console.Print("- Height: " + map.GetMapHeight());
                console.Print("- Enclosed: " + map.IsEnclosed());
            }
            else
            {
                map.StartMap(command[1], true);
            }
        }
        else
        {
            console.Print(map.GetMapName());
        }
    }

    /*
    ====================
    PrintMapNames
    ====================
    */
    public void PrintMapNames()
    {
        console.Print(":::Map List:::");
        string[] names = map.GetMapNames();
        for(int i =0; i < names.Length; i++)
        {
            console.Print(names[i]);
        }
    }
}
