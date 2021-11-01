using Godot;
using System.Collections.Generic;

public class DrawLine3D : Node2D
{
    class Line
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float time;

        public Line(Vector3 start, Vector3 end, Color c, float time)
        {
            this.start = start;
            this.end = end;
            this.color = c;
            this.time = time;
        }
    }

    private List<Line> Lines = new List<Line>();
    private bool RemovedLine = false;

    public override void _Process(float delta)
    {
        // Code to execute in editor.        
        for(int i = 0; i < Lines.Count; i++)       
        {
            Lines[i].time -= delta;
        }

        if(Lines.Count > 0 || RemovedLine)
        {
            Update();
            RemovedLine = false;
        }
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        Lines.Add(new Line(start, end, color, 0.0f));
    }

    public override void _Draw()
    {
        Camera cam = GetViewport().GetCamera();
        for(int i = 0; i < Lines.Count; i++)
        {
            Vector2 screenPointStart = cam.UnprojectPosition(Lines[i].start);
            Vector2 screenPointEnd = cam.UnprojectPosition(Lines[i].end);

            if(cam.IsPositionBehind(Lines[i].start) || cam.IsPositionBehind(Lines[i].end))
            {
                continue;
            }

            DrawLine(screenPointStart, screenPointEnd, Lines[i].color);
        }

        int l = Lines.Count-1;

        while(l >=0)
        {
            if(Lines[l].time<0.0f)
            {
                Lines.RemoveAt(l);
                RemovedLine = true;
            }

            l--;
        }
    }
}