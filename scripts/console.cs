using Godot;
using System.Collections.Generic;

public class console : Node
{
    private RandomNumberGenerator rng;
    private Maze_Generator level;
    private KinematicBody player;
    private ColorRect windowLine;
    private Node2D window;
    private float windowHeight = 0.5f;
    private float windowSpeed = 15.0f;

    private Vector2 configResolution;
    private Label consoleOut;
    private LineEdit consoleIn;
    private Sprite cBKG;
    private Texture[] cTextures;
    private string[] cLines;
    private string[] history;
    private bool displayWindow = false;

    private int historyCount = 0;
    private int historyPos = 0;
    private int displaySize = 0;
    private int lineCount = 0;

    private int MAX_CLINE = 500;
    private int MAX_HISTORY = 10;
    
    /*
    ==================
    _Ready
    ==================
    */
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();

        window = GetNode("Console") as Node2D;
        level = GetNode("Map") as Maze_Generator;
        player = GetNode("Player") as KinematicBody;     
        consoleOut = GetNode("Console/Console_Text") as Label;
        consoleIn = GetNode("Console/Console_Input") as LineEdit;
        windowLine = GetNode("Console/Console_Footer") as ColorRect;
        cBKG = GetNode("Console/Squares") as Sprite;
        
        configResolution = GetViewport().Size;

        cLines = new string[MAX_CLINE];
        history = new string[MAX_HISTORY];

	    lineCount = 0;
        historyCount = 0;
        historyPos = 0;
        displaySize = 0;
	    consoleIn.Text = "";
        consoleOut.Text = "";

        LoadConsoleTextures();

        Color c = new Color();
        c.r = 0.0F;
        c.g  = 0.78f;
        c.b = 0.1f;
        c.a = 1.0f;

        (cBKG.Material as ShaderMaterial).SetShaderParam("color", c);

        level.StartMap("random");
    }

    /*
    ==================
    _Input
    ==================
    */
    public override void _Input(InputEvent @event)
    {
        // if (quitting) 
        //     return;
        
        // Show/hide console window
        if (Input.IsActionJustPressed("console"))
        {
            if(displayWindow)
            {
                displayWindow = false;
                pauseGame(false);	
                consoleIn.ReleaseFocus();
            }
            else
            {
                displayWindow = true;
                pauseGame(true);
                consoleIn.GrabFocus();
            }

            GetTree().SetInputAsHandled();
        }

        if(Input.IsActionPressed("console"))
        {
            GetTree().SetInputAsHandled();
        }
        
        if (displayWindow)
        {
            if(historyCount > 0)
            {
                // Go "up/down" through input history
                if(Input.IsActionJustPressed("ui_up"))
                {
                    DisplayPrevious(-1);
                }
                if (Input.IsActionJustPressed("ui_down"))
                {
                    DisplayPrevious(1);
                }
            }

            // Scroll console page up/down one line
            if(Input.IsActionPressed("ui_page_up"))
            {
                displaySize = (int)Mathf.Clamp(displaySize -1, 1, lineCount);
                UpdateView();
            }
            if(Input.IsActionPressed("ui_page_down"))
            {
                displaySize = (int)Mathf.Clamp(displaySize + 1, 1, lineCount);
                UpdateView();
            }

            // ENTER
            if(Input.IsActionJustPressed("ui_accept"))
            {
                if(consoleIn.Text.StripEdges(true,true) == "")
                {
                    consoleIn.Text = "]";
                }           

                if (historyCount >= MAX_HISTORY)
                {
                    history = PushTop(consoleIn.Text, history);                        
                    historyCount = MAX_HISTORY-1;
                }  
                else
                {
                    history[historyCount] = consoleIn.Text;                    
                }

                historyCount++;
                historyPos = historyCount;

                AddToConsole(consoleIn.Text);
                ReadInput(consoleIn.Text);
                consoleIn.Clear();
            }
        }
    }

    /*
    ==================
    _Process
    ==================
    */
    public override void _Process(float delta)
    {
        Transform2D t = window.Transform;
        t.origin.y += displayWindow ? windowSpeed : -windowSpeed;
        t.origin.y = Mathf.Clamp(t.origin.y, 0, configResolution.y * windowHeight);
	    if(t.origin.y <= 0)
        {
            windowLine.Hide();
        }
	    else
        {
            windowLine.Show();
        }  

        window.Transform = t;
    }

    /*
    ==================
    DisplayPrevious
    ==================
    */
    private void DisplayPrevious(int p)
    {
	    historyPos = Mathf.Clamp(historyPos + p, 0, historyCount-1);
	    consoleIn.Text = history[historyPos];
    }  

    /*
    ==================
    Pause Game
    ==================
    */
    private void pauseGame(bool b)
    {
	    GetTree().Paused = b;
    }

    /*
    ==================
    LoadConsoleTextures
    ==================
    */
    private void LoadConsoleTextures()
    {
        AddToConsole("loading console textures.." + "\n" + "--------------------------");
        List<Texture> temp = new List<Texture>();

        string path = "res://textures/console/";
        Directory dir = new Directory();
        dir.Open(path);
        dir.ListDirBegin();
        while (true)
        {
            string fileName = dir.GetNext();
            if (fileName == "")
            {
                break;
            }

            if(fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
            {
                AddToConsole(fileName);
                temp.Add(ResourceLoader.Load("res://textures/console/" + fileName) as Texture);
            }
        }
        
        cTextures = temp.ToArray();
        temp.Clear();

        dir.ListDirEnd();
        
        //int r = rng.RandiRange(0,cTextures.Length-1);
        cBKG.Texture = cTextures[4];
    }

    /*
    ==================
    AddToConsole
    ==================
    */
    public void AddToConsole(string s)
    {      
        if(lineCount >= MAX_CLINE)
        {
            cLines = PushTop(s, cLines);            
            lineCount = MAX_CLINE-1;
            displaySize = (int)Mathf.Clamp(displaySize + 1, 1, lineCount);
        }
        else
        {
	        cLines[lineCount] = s;
        }

        lineCount++;
        displaySize = (int)Mathf.Clamp(displaySize + 1, 1, lineCount);

        UpdateView();

    }

    /*
    ==================
    UpdateView
    ==================
    */
    private void UpdateView()
    {
        consoleOut.Text = "";
	    for(int i = 0; i < displaySize; i++)
        {
             consoleOut.Text += cLines[i] + (i != displaySize-1 ? "\n" : "");
        }
    }

    /*
    ==================
    ReadInput
    ==================
    */
    private void ReadInput(string s)
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
    ClearConsole
    ==================
    */
    private void ClearConsole()
    {
        cLines = new string[MAX_CLINE];
        history = new string[MAX_HISTORY];

	    lineCount = 0;
        historyCount = 0;
        historyPos = 0;
        displaySize = 0;
	    consoleIn.Text = "";
        consoleOut.Text = "";
    }

    /*
    ==================
    RefreshConsole
    ==================
    */
    private void RefreshConsole()
    {
        cLines = ResizeArray(cLines, MAX_CLINE);
        history = ResizeArray(history, MAX_HISTORY);

	    lineCount = (int)Mathf.Clamp(lineCount, 0, MAX_CLINE);
        historyCount = (int)Mathf.Clamp(historyCount, 0, MAX_HISTORY);
        historyPos = historyCount;
        displaySize = (int)Mathf.Clamp(displaySize, 0, lineCount);	    
        consoleIn.Text = "";
    }

    /*
    ==================
    ResizeArray
    ==================
    */
    private string[] ResizeArray(string[] ar, int newSize)
    {
        string[] res = new string[newSize];

        for(int i = 0; i < newSize; i++)
        {
            if (i > ar.Length-1)
            {
                break;
            }                
            
            string s = ar[i];
            res[i] = s;
        }

        return res;
    }

    /*
    ==================
    PushTop (string)

    Removes element at 0 and shifts following elements back one
    Adds new element at length-1.
    ==================
    */
    private string[] PushTop(string addText, string[] current)
    {
        string[] shifted = new string[current.Length];
        for(int i = 1; i < current.Length; i++)
        {
            shifted[i-1] = current[i];
        }

        shifted[shifted.Length-1] = addText;        


        return shifted;
    }

    /*
    ==================
    PushBottom (string)

    Removes element at length-1 and shifts following elements forward
    Adds new element at 0;
    ==================
    */
    private string[] PushBottom(string addText, string[] current)
    {
        string[] shifted = new string[current.Length];
        for(int i = current.Length-1; i > 0; i--)
        {
            shifted[i] = current[i-1];
        }

        shifted[0] = addText;
        displaySize = (int)Mathf.Clamp(displaySize - 1, 1, lineCount);

        return shifted;
    }

    /*
    ==================
    SetConsoleSpeed
    ==================
    */
    private void SetConsoleSpeed(float speed)
    {
        windowSpeed = speed;
    }

    /*
    ==================
    SetConsoleHeight
    ==================
    */
    private void SetConsoleHeight(float height)
    {
        windowHeight = height;
    }

    /*
    ==================
    SetMaxHistory
    ==================
    */
    private void SetMaxHistory(int c)
    {
        MAX_HISTORY = (int)Mathf.Clamp(c, 0, 1000);
        RefreshConsole();
    }

    /*
    ==================
    SetMaxCLine
    ==================
    */
    private void SetMaxCLine(int c)
    {
        MAX_CLINE = (int)Mathf.Clamp(c, 0, 1000);
        RefreshConsole();
    }

    /*
    ==================
    ConsoleOptions
    ==================
    */
    private void ConsoleOptions(string s, string[] command)
    {
        switch(s)
        {
            case "texture":
                if(command.Length > 1)
                {
                    if(command[1].IsValidInteger())
                    {
                        int i = (int)Mathf.Clamp(command[1].ToInt(), 0, cTextures.Length-1);
                        cBKG.Texture = cTextures[i];
                    }
                    else if (command[1].ToLower() == "random")
                    {
                        int r = rng.RandiRange(0,cTextures.Length-1);
                        cBKG.Texture = cTextures[r];
                    }
                }       
                break;

            case "speed":
                if(command.Length > 1 && command[1].IsValidFloat())
                {
                    SetConsoleSpeed(command[1].ToFloat());
                }
                break;

            case "height":
                if(command.Length > 1 && command[1].IsValidFloat())
                {
                    SetConsoleHeight(command[1].ToFloat());
                }
                break;                
            case "lines":
                if(command.Length > 2)
                {
                    if(command[1] == "max_history" && command[2].IsValidInteger())
                    {
                        SetMaxHistory(command[2].ToInt());
                    }
                    if(command[1] == "max_cline" && command[2].IsValidInteger())
                    {
                        SetMaxCLine(command[2].ToInt());
                    }
                }
                break;
            case "clear":
                if(command.Length == 2)
                {
                    ClearConsole();
                }
                break;
            default:
                break;
        }
    }

    /*
    ==================
    ResizeElements
    ==================
    */
    private void ResizeElements()
    {
	    //var x_scale = config_resolution.x/maxres.x
	    //var y_scale = config_resolution.y/maxres.y
	    configResolution = GetViewport().Size;
        
        Vector2 rSize = windowLine.RectSize;
	    rSize.x = configResolution.x;
        windowLine.RectSize = rSize;
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
                            //AddToConsole(command[i].ToString());
                            pos[i-1] = command[i].ToFloat();
                        }                        
                    }

                    int x = (int)Mathf.Clamp(pos[0], 0, level.GetMapWidth());
                    int y = (int)Mathf.Clamp(pos[1], 0, 100);
                    int z = (int)Mathf.Clamp(pos[2], 0, level.GetMapHeight());                        
                    
                    var t = player.GlobalTransform;
                    int scale = level.GetMapScale();
                    t.origin = new Vector3(x * scale, y, z * scale);
                    player.GlobalTransform = t;
                }
                break;

            default:
                break;
        }
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
                level.StartMap(command[1], true);
            }
            else if (command[2] == "open")
            {
                level.StartMap(command[1], false);
            }
            else
            {
                level.StartMap(command[1], false);
            }
        }
        else if(command.Length >= 2)
        {
            if (command[1] == "ls" || command[1] == "list")
            {
                level.PrintMapNames();
            }
            else if(command[1] == "restart")
            {
                level.StartMap(level.GetMapName());
            }
            else if (command[1] == "info")
            {
                AddToConsole("Map Name: " + level.GetMapName());
                AddToConsole("Map Width: " + level.GetMapWidth());
                AddToConsole("Map Height: " + level.GetMapHeight());
                AddToConsole("Map Enclosed: " + level.IsEnclosed());
            }
            else
            {
                level.StartMap(command[1]);
            }
        }
        else
        {
            AddToConsole(level.GetMapName());
        }
    }
}
