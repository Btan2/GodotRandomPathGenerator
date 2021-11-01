using Godot;

public class LaserLine : MeshInstance
{
    //
    //1. Draw a line between two points
    //2. Change line texture to noise texture
    //3. Control points from public move method
    //4. Check if shader works on line
    //
    public override void _Ready()
    {
        DrawLine3D draw = new DrawLine3D();
        draw.DrawLine(GlobalTransform.origin + Vector3.Right, GlobalTransform.origin + Vector3.Down, Color.ColorN("Red", 1.0f));    
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
     public override void _Process(float delta)
    {
        DrawLine3D draw = new DrawLine3D();
        draw.DrawLine(GlobalTransform.origin + Vector3.Right * 10, GlobalTransform.origin + Vector3.Down * 10, Color.ColorN("Red", 1.0f));      
    }
}
