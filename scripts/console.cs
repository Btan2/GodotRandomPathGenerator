using Godot;
using System;
using System.Collections.Generic;

public class console : Node
{
    private Node2D window;
    private float consoleHeight = 0.5f;
    private float consoleSpeed = 15.0f;
    private RandomNumberGenerator rng;
    private Vector2 configResolution;
    private Label consoleOut;
    private LineEdit consoleIn;
    private ColorRect foot;
    private Sprite consoleBKG;
    private Texture[] consoleTextures;
    private List<string> c_lines;
    private List<string> history;
    private bool displayConsole = false;
    private bool quitting = false;
    private int historyPos = 0;
    private int displaySize = 0;
    private const int MAX_CLINE_COUNT = 200;
    private Maze_Generator level;

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
        consoleOut = GetNode("Console/Console_Text") as Label;
        consoleIn = GetNode("Console/Console_Input") as LineEdit;
        foot = GetNode("Console/Console_Footer") as ColorRect;
        consoleBKG = GetNode("Console/Squares") as Sprite;
        configResolution = GetViewport().Size;

        c_lines = new List<string>();
        history = new List<string>();

	    displaySize = 0;
	    consoleIn.Text = "";
        consoleOut.Text = "";

        LoadConsoleTextures();

        Color c = new Color();
        c.r = 0.0F;
        c.g  = 0.78f;
        c.b = 0.1f;
        c.a = 1.0f;

        (consoleBKG.Material as ShaderMaterial).SetShaderParam("color", c);

        level.StartMap("random");
    }

    /*
    ==================
    _Input
    ==================
    */
    public override void _Input(InputEvent @event)
    {
        if (quitting) 
            return;
        
        // Show/hide console window
        if (Input.IsActionJustPressed("console"))
        {
            if(displayConsole)
            {
                displayConsole = false;
                pauseGame(false);	
                consoleIn.ReleaseFocus();
            }
            else
            {
                displayConsole = true;
                pauseGame(true);
                consoleIn.GrabFocus();
            }

            GetTree().SetInputAsHandled();
        }

        if(Input.IsActionPressed("console"))
        {
            GetTree().SetInputAsHandled();
        }
        
        if (displayConsole)
        {
            // Go "up/down" through input history
            if(history.Count > 0)
            {
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
                displaySize = (int)Mathf.Clamp(displaySize -1, 1, c_lines.Count);
                UpdateConsole();
            }
            if(Input.IsActionPressed("ui_page_down"))
            {
                displaySize = (int)Mathf.Clamp(displaySize + 1, 1, c_lines.Count);
                UpdateConsole();
            }

            // Console input text
            if(Input.IsActionJustPressed("ui_accept"))
            {
                if(consoleIn.Text.StripEdges(true,true) == "")
                {
                    consoleIn.Text = "]";
                }
                else
                {
                    history.Add(consoleIn.Text);
                    if (history.Count > MAX_CLINE_COUNT)
                    {
                        history.RemoveAt(0);
                    }

                    historyPos = history.Count;
                }

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
        t.origin.y += displayConsole ? consoleSpeed : -consoleSpeed;
        t.origin.y = Mathf.Clamp(t.origin.y, 0, configResolution.y * consoleHeight);
	    if(t.origin.y <= 0)
        {
            foot.Hide();
        }
	    else
        {
            foot.Show();
        }  

        window.Transform = t;
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
        AddToConsole("loading console textures..");
        AddToConsole("--------------------------");
        List<Texture> textures = new List<Texture>();

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
                textures.Add(ResourceLoader.Load("res://textures/console/" + fileName) as Texture);
            }
        }
        
        consoleTextures = textures.ToArray();
        textures.Clear();

        dir.ListDirEnd();
        
        int r = rng.RandiRange(0,consoleTextures.Length-1);
        consoleBKG.Texture = consoleTextures[r];
    }

    /*
    ==================
    AddToConsole
    ==================
    */
    public void AddToConsole(string s)
    {        
	    c_lines.Add(s);
	    displaySize++;

	    UpdateConsole();
    }

    /*
    ==================
    UpdateConsole
    ==================
    */
    private void UpdateConsole()
    {
        consoleOut.Text = "";
	    for(int i = 0; i < displaySize; i++)
        {
            consoleOut.Text += c_lines[i] + (i != displaySize-1 ? "\n" : "");
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
                     ConsoleSettings(split_string[1],command);
                 }
                 break;
            
            case "map":
                    MapOptions(command);
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

        if(command.Length >= 2)
        {    
            if (command[1] == "ls" || command[1] == "list")
            {
                level.PrintMapNames();
            }
            else if(command[1] == "restart")
            {
                level.StartMap(level.GetMapName());
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

    /*
    ==================
    ConsoleSettings
    ==================
    */
    private void ConsoleSettings(string s, string[] command)
    {
        switch(s)
        {
            case "texture":
                if(command.Length > 1)
                {
                    if(command[1].IsValidInteger())
                    {
                        int i = (int)Mathf.Clamp(command[1].ToInt(), 0, consoleTextures.Length-1);
                        consoleBKG.Texture = consoleTextures[i];
                    }
                    else if (command[1].ToLower() == "random")
                    {
                        int r = rng.RandiRange(0,consoleTextures.Length-1);
                        consoleBKG.Texture = consoleTextures[r];
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

            default:
                break;
        }
    }

    /*
    ==================
    SetConsoleSpeed
    ==================
    */
    private void SetConsoleSpeed(float speed)
    {
        consoleSpeed = speed;
    }

    /*
    ==================
    SetConsoleHeight
    ==================
    */
    private void SetConsoleHeight(float height)
    {
        consoleHeight = height;
    }

    /*
    ==================
    DisplayPrevious
    ==================
    */
    private void DisplayPrevious(int p)
    {
	    historyPos = Mathf.Clamp(historyPos + p, 0, history.Count-1);
	    consoleIn.Text = history[historyPos];
	    consoleIn.CaretPosition = consoleIn.Text.Length-1;
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
        
        Vector2 rSize = foot.RectSize;
	    rSize.x = configResolution.x;
        foot.RectSize = rSize;
    }
}
