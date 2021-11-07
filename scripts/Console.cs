using Godot;

public class Console : Control
{
    private ColorRect rectFooter;
    private Sprite imageBKG;
    private Label versionLabel;
    private Label lineOUT;
    private LineEdit lineIN;
    private Vector2 configResolution;
    private ConsoleCommands commands;
    private Texture[] cTex;
    private PStack lines;
    private PStack history;
    
    private bool showConsole = false;
    private int historyPos = 0;
    private int linePos = 0;
    private int MAXLINES = 20;
    private int MAXHISTORY = 5;
    private float windowHeight = 0.5f;
    private float windowSpeed = 15.0f;

    private bool loaded = false;

    /*
    ==================
    RUN
    ==================
    */
    public void Run()
    {
        PauseMode = PauseModeEnum.Process;
        lineOUT = GetNode("Text_Output") as Label;
        versionLabel = GetNode("VersionControl") as Label;
        lineIN = GetNode("Text_Input") as LineEdit;
        rectFooter = GetNode("Rect_Footer") as ColorRect;
        imageBKG = GetNode("Texture_BKG") as Sprite;
        
        ResizeElements();
        Clear();
        LoadConsoleTextures();

        loaded = true;
    }

    public void SetCommands(ConsoleCommands cc)
    {
        commands = cc;
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
            if(showConsole)
            {
                showConsole = false;
                PauseGame(false);	
                lineIN.ReleaseFocus();
            }
            else
            {
                showConsole = true;
                PauseGame(true);
                lineIN.GrabFocus();
            }

            GetTree().SetInputAsHandled();
        }

        if(Input.IsActionPressed("console"))
        {
            GetTree().SetInputAsHandled();
        }
        
        if (showConsole)
        {
            // Scroll "up/down" through input history
            if(Input.IsActionJustPressed("ui_up"))
            {
                HistoryDisplay(-1);
            }
            if (Input.IsActionJustPressed("ui_down"))
            {
                HistoryDisplay(1);
            }

            // Scroll "up/down" through console output
            if(Input.IsActionPressed("ui_page_up"))
            {
                LineDisplay(-1);
            }
            if(Input.IsActionPressed("ui_page_down"))
            {
                LineDisplay(1);
            }

            // ENTER BUTTON
            if(Input.IsActionJustPressed("ui_accept"))
            {
                // Default output string if nothing is entered
                if(lineIN.Text.StripEdges(true,true) == "")
                {
                    lineIN.Text = "]";
                }         

                // Add input string to history array
                if (history.Count() >= MAXHISTORY)
                {
                    history.PushTop(lineIN.Text);
                }  
                else
                {
                    history.Add(lineIN.Text);
                }

                historyPos = history.Count();                  
                
                Print(lineIN.Text);

                if(commands != null)
                {
                    commands.Read(lineIN.Text);     
                }    
                           
                lineIN.Clear();
            }
        }
    }

    /*
    ==================
    _Process
    ==================
    */
    public override void _PhysicsProcess(float delta)
    {
        if(!loaded)
        {
            return;
        }

        Vector2 rP = RectPosition;
        float y = RectPosition.y;
        y += showConsole ? windowSpeed : -windowSpeed;
        y = Mathf.Clamp(y, 0, configResolution.y * windowHeight);
	    if(y <= 0)
        {
            rectFooter.Hide();
        }
	    else
        {
            rectFooter.Show();
        }  

        RectPosition = new Vector2(rP[0], y);

        // Set version control position
        Vector2 t = versionLabel.RectPosition;
        t.y = -RectPosition.y/2 - 84;
        t.x = configResolution.x - 320;
        versionLabel.RectPosition = t;        
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

        Vector2 versionPos = versionLabel.RectPosition;
        versionPos[0] = configResolution.x - versionLabel.RectSize.x;
        versionLabel.RectPosition = versionPos;

        Vector2 inScale = lineIN.RectSize;
        inScale.x = configResolution.x;
        inScale.y = 26.0f;
        lineIN.RectSize =  inScale;

        Vector2 inPos = new Vector2(5,-26);
        lineIN.RectPosition = inPos;
        
        lineOUT.RectSize = configResolution;

        Vector2 outPos = new Vector2();
        outPos.x = 5;
        outPos.y = -(configResolution.y + 26.0f);
        lineOUT.RectPosition = outPos;
        
        Vector2 rSize = rectFooter.RectSize;
	    rSize.x = configResolution.x;
        rectFooter.RectSize = rSize;
    }

    /*
    ==================
    LoadConsoleTextures
    ==================
    */
    private void LoadConsoleTextures()
    {
        Print("loading console textures..");
        Print("--------------------------");

        cTex = new Texture[10];
        
        string path = "res://textures/console/";
        Directory dir = new Directory();

        if(dir.Open(path) != Error.Ok)
        {
            return;
        }
        
        dir.Open(path);
        dir.ListDirBegin();

        int index = 0;

        while(index < cTex.Length)
        {
            string fileName = dir.GetNext();
            if (fileName == "")
            {
                break;
            }

            if(fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
            {
                Print(fileName);
                cTex[index] = ResourceLoader.Load("res://textures/console/" + fileName) as Texture;
                index++;
            }
        }
        
        dir.ListDirEnd();
        
        imageBKG.Texture = ResourceLoader.Load("res://textures/console/" + "Tiles072.jpg") as Texture;
    }

    /*
    ==================
    Print
    ==================
    */
    public void Print(string s)
    {      
        PushToConsole(s);
    }
    public void Print(bool b)
    {
        string s = b.ToString();
        PushToConsole(s);
    }
    public void Print(float f)
    {
        string s = f.ToString();
        PushToConsole(s);
    }
    public void Print(int n)
    {
        string s = n.ToString();
        PushToConsole(s);
    }
    public void Print(Vector2 v)
    {
        PushToConsole("[" + v[0].ToString() +  "," + v[1].ToString() +"]");
    }

    public void Print(Vector3 v)
    {
        PushToConsole("[" + v[0].ToString() +  "," + v[1].ToString() + "," + v[2].ToString() +"]");
    }

    /*
    ==================
    Mute Audio
    ==================
    */
    public void Mute()
    {

    }

    /*
    ==================
    UnMute Audio
    ==================
    */
    public void UnMute()
    {

    }

    /*
    ==================
    PushToConsole
    ==================
    */
    private void PushToConsole(string s)
    {
        if(lines.Count() >= MAXLINES)
        {
            lines.PushTop(s);
        }
        else
        {
            lines.Add(s);
        }
        
        LineDisplay(1);
    }

    /*
    ==================
    CycleHistory

    Scroll through input history
    ==================
    */
    private void HistoryDisplay(int p)
    {
	    historyPos = Mathf.Clamp(historyPos + p, 0, history.Count());
	    lineIN.Text = history.GetString(historyPos);
    }  

    /*
    ==================
    CycleDisplay

    Scroll through output text
    ==================
    */
    private void LineDisplay(int p)
    {        
        const int DISPLAY_SIZE = 20;

        linePos = (int)Mathf.Clamp(linePos + p, 0, lines.Count());

        int from = 0;
        if(linePos >= DISPLAY_SIZE)
        {
            from = (int)Mathf.Clamp(linePos - DISPLAY_SIZE, 0, DISPLAY_SIZE);
        }

        lineOUT.Text = "";
        for(int i = from; i < linePos; i++)
        {
            lineOUT.Text += lines.GetString(i) + (i >= linePos-1 ? "" : "\n");
        }
    }

    /*
    ==================
    Clear
    ==================
    */
    public void Clear()
    {
        lines = new PStack(MAXLINES);
        history = new PStack(MAXHISTORY);

        linePos = 0;
        historyPos = 0;   

        lineIN.Text = "";
        lineOUT.Text = "";
    }

    /*
    ==================
    Pause Game
    ==================
    */
    private void PauseGame(bool b)
    {
	    GetTree().Paused = b;
    }

    public void ShowCurrentPos()
    {
        Print(linePos);
    }

    /*
    ==================
    SetSpeed
    ==================
    */
    public void SetSpeed(float speed)
    {
        windowSpeed = Mathf.Clamp(speed, 0, 100);
    }

    /*
    ==================
    SetHeight
    ==================
    */
    public void SetHeight(float height)
    {
        windowHeight = Mathf.Clamp(height, 0.2f, 1.0f);
    }

    /*
    ==================
    SetColour
    ==================
    */
    public void SetColour(float r, float g, float b)
    {
        Color c = new Color();
        c.r = r;
        c.g = g;
        c.b = b;
        c.a = 1.0f;

        (imageBKG.Material as ShaderMaterial).SetShaderParam("color", c);
    }

    /*
    ==================
    SetBKGTexture
    ==================
    */
    public void SetBKGTexture(int i)
    {
        imageBKG.Texture = cTex[(int)Mathf.Clamp(i, 0, cTex.Length-1)];
    }

    /*
    ==================
    RemoveLine
    ==================
    */
    public void RemoveLine(int p)
    {
        lines.Remove(p);
        LineDisplay(-1);
    }

    /*
    ==================
    SetMaxHistory
    ==================
    */
    public void SetMaxHistory(int c)
    {
        MAXHISTORY = (int)Mathf.Clamp(c, 2, 1000);
        history.Resize(MAXHISTORY);
        historyPos = (int)Mathf.Clamp(historyPos, 0, MAXHISTORY);
        LineDisplay(0);
    }

    /*
    ==================
    SetMaxLines
    ==================
    */
    public void SetMaxLines(int c)
    {
        MAXLINES = (int)Mathf.Clamp(c, 2, 1000);
        lines.Resize(MAXLINES);
        LineDisplay(0);
    }
}
