/*
MIT License

Copyright (c) 2018 Adam Newgas

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Source: https://github.com/BorisTheBrave/chiseled-random-paths
*/

using Godot;
using System;
using System.Collections.Generic;

public class Maze_Generator : Node
{
    private RandomNumberGenerator rng;
    private SpatialMaterial material_redbrick;
    private const int SCALE = 10;
    private const int WALLHEIGHT = 2;
    private int num = 1;
    private int width;
    private int height;
    private float totalWeight = 0;
    private float[,] weights;
    private int[,] low;
    private int[,] dfsNum;
    private bool[,] isArticulation;

    private enum Tile {BLANK, PATH, END, START, EXIT, TRAP};

    private Tile[,] grid;
    private bool[,] ends;
    private int[,] adjacent = {{1,0},{0,1},{-1,0},{0,-1}};
    private Texture[] maps;
    private string[] mapNames;
    private string mapName = "";

    private int[] start;
    private int[] exit;

    private bool enclosed = false;
    private Vector3 mazeOrigin;

    /*
    ==================
    _Ready
    ==================
    */
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();

        mazeOrigin = Vector3.Zero;
        material_redbrick = ResourceLoader.Load("res://materials/Material_RedBricks.tres") as SpatialMaterial;

        LoadMaps();   
    }

    /*
    ==================
    LoadMaps
    ==================
    */
    private void LoadMaps()
    {
        List<Texture> mImgs = new List<Texture>();
        List<String> mNames = new List<string>();

        var dir = new Directory();
        dir.Open("res://maps/");
		dir.ListDirBegin();
		string file = dir.GetNext();
        
		while(file != "")
        {
            if (file.EndsWith(".png"))
            {
                mImgs.Add(ResourceLoader.Load("res://maps/" + file) as Texture);
                mNames.Add(file.ToLower());
            }

			file = dir.GetNext();
        }

        maps = mImgs.ToArray();
        mImgs.Clear();

        mapNames = mNames.ToArray();   
        mNames.Clear();
    }

    /*
    ===============
    IsValid
    ===============
    */
    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /*
    ====================
    GetMapName
    ====================
    */
    public string GetMapName()
    {
        return mapName;
    }

    public int[] GetMapSize()
    {
        return new int[]{width, height};
    }

    public int GetMapWidth()
    {
        return width;
    }

    public int GetMapHeight()
    {
        return height;
    }

    public int GetMapScale()
    {
        return SCALE;
    }

    public bool IsEnclosed()
    {
        return enclosed;
    }

    /*
    ====================
    AddToConsole
    ====================
    */      
    private void AddToConsole(string s)
    {
        (GetParent() as console).AddToConsole(s);
    }

    /*
    ====================
    PrintMapNames
    ====================
    */
    public void PrintMapNames()
    {
        AddToConsole(":::Map List:::");
        for(int i =0; i < mapNames.Length; i++)
        {
            AddToConsole(mapNames[i]);
        }
    }

    /*
    ====================
    StartMap

    Starts a new map
    ====================
    */
    public void StartMap(string s)
    {
        enclosed = true;

        Texture map = GetMap(s);

        if(map == null)
        {
            AddToConsole(":::Cannot load map (does it exist?):::");
            return;
        }

        AddToConsole("");
        AddToConsole("Generating new map........");
        AddToConsole("--------------------------");        
        AddToConsole("MAP: " + mapName);
       
        GenerateMap(map);

        //CellSelector(0,5,0,5);

        CreateMultiMesh();      
                
        // Set player position
        KinematicBody player;
        player = GetParent().GetNode("Player") as KinematicBody;
        Transform t = player.GlobalTransform;
        t.origin = Vector3.Up * 4;
        player.GlobalTransform = t;

        // Set floor scale and position
        MeshInstance floor;
        floor = GetParent().GetNode("Floor") as MeshInstance;
        floor.Scale = new Vector3(width+1, Math.Max(width+1, height+1), height+1) * SCALE;
	    Transform f_transform = floor.GlobalTransform;
        f_transform.origin = new Vector3(width*SCALE/2f, 0f, height*SCALE/2f);
        floor.GlobalTransform = f_transform;
    }

    public void StartMap(string s, bool b)
    {
        enclosed = b;

        Texture map = GetMap(s);

        if(map == null)
        {
            AddToConsole(":::Cannot load map (does it exist?):::");
            return;
        }

        AddToConsole("");
        AddToConsole("Generating new map........");
        AddToConsole("--------------------------");        
        AddToConsole("MAP: " + mapName);
       
        GenerateMap(map);

        //CellSelector(0,5,0,5);

        CreateMultiMesh();      
                
        // Set player position
        KinematicBody player;
        player = GetParent().GetNode("Player") as KinematicBody;
        Transform t = player.GlobalTransform;
        t.origin = Vector3.Up * 4;
        player.GlobalTransform = t;

        // Set floor scale and position
        MeshInstance floor;
        floor = GetParent().GetNode("Floor") as MeshInstance;
        floor.Scale = new Vector3(width+1, Math.Max(width+1, height+1), height+1) * SCALE;
	    Transform f_transform = floor.GlobalTransform;
        f_transform.origin = new Vector3(width*SCALE/2f, 0f, height*SCALE/2f);
        floor.GlobalTransform = f_transform;
    }

   /*
    ====================
    GetMap
    ====================
    */
    public Texture GetMap(string s)
    {   
        int mapIndex = -1; 
        string title = s.Split("_")[0];

        for(int i = 0; i < mapNames.Length; i++)
        {
            if(mapNames[i].BeginsWith(title))
            {
                mapIndex = i;
            }
        }                    

        if(mapIndex >= 0)
        {
            mapName = s;
            return maps[mapIndex];
        }
        else if(s == "random")
        {
            int rand = rng.RandiRange(0,maps.Length-1);
            mapName = mapNames[rand];
            return maps[rand];
        }
        else
        {
            return null;
        }
    }

    /*
    ====================
    GenerateMap
    ====================
    */
    private void GenerateMap(Texture map)
    {        
        Image img = map.GetData();
        ReadImageToGridMap(img);
        
        // Check for cut count in filename
        
        string[] nameSplit = mapName.Split("_");

        if(nameSplit.Length <= 1)
        {
            AddToConsole("MAP: 0 tiles removed" + "\n" + "MAP: No chisel paths.." + "\n" + "MAP: " + (enclosed ? "Closed" : "Open"));
            GeneratePath(false, 0);  
            return;          
        }

        int cuts = -1;
        string cutStr = nameSplit[1];       
        if(cutStr.Contains("."))
        {
            cutStr = cutStr.Substring(0, cutStr.IndexOf("."));
        }

        if(cutStr.IsValidInteger())
        {
            cuts = cutStr.ToInt();
        }

        //AddToConsole("CUT HUS NUCH");
        //AddToConsole(cuts.ToString());

        if(cuts <= -1)
        {
            AddToConsole("MAP: 0 tiles removed" + "\n" + "MAP: No chisel paths.." + "\n" + "MAP: " + (enclosed ? "Closed" : "Open"));
            GeneratePath(false, 0);
        }
        else
        {
            AddToConsole("MAP: Chiseled path with ~" + cuts.ToString() + " tiles removed.." + "\n" + "MAP: " + (enclosed ? "Closed" : "Open"));
            GeneratePath(true, cuts);  
        }  
    }

    /*
    ====================
    ReadImageMap

    Setup grid from image pixel array
    ====================
    */
    public void ReadImageToGridMap(Image img)
    {
        img.Lock();

        width = (int)img.GetWidth();
        height = (int)img.GetHeight();
        grid = new Tile[width,height];
        ends = new bool[width,height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = img.GetPixel(i,j);
                if (pixel == Colors.White)
                {
                    // PATH
                    ends[i,j] = false;
                    grid[i,j] = Tile.PATH;
                }
                if (pixel == Colors.Black)
                {
                    // END
                    ends[i,j] = true;
                    grid[i,j] = Tile.END;
                }
            }
        }

        img.Unlock();
    }

    /*
    ====================
    GeneratePath
    ====================
    */
    private void GeneratePath(bool chisel, int maxCount)
    {
        start = new int[2]{0,0};
        exit = new int[2]{width-1, height-1};
        
        ends[0,0] = true;
        grid[0,0] = Tile.START;
        ends[exit[0], exit[1]] = true;               
        grid[exit[0], exit[1]] = Tile.EXIT;        

        if (!chisel)
        {
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < width; y++)
                {
                    if (grid[x,y] == Tile.PATH)
                    {
                        grid[x,y] = Tile.BLANK;
                    }
                }
            }

            return;
        }

        // Chisel the grid and add random pathways
        int count = 0;
        bool[,] aPoints;
        while(true)
        {
            aPoints = FindArticulationPoints(ends);
            if(!RemovableTiles(aPoints))
            {
                break;
            }

            InitializeWeights();
            RemoveRandomTile(aPoints);

            if (maxCount > 0)
            {
                if (count > maxCount)
                {
                    break;
                }

                count++;
            }            
        }
    }

    /*
    ==================
    RemovableTiles
    ==================
    */
    private bool RemovableTiles(bool[,] aPoints)
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(grid[x,y] == Tile.PATH && !aPoints[x,y])
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*
    ==================
    RemoveRandomTile
    ==================
    */
    private void RemoveRandomTile(bool[,] aPoints)
    {
        int[] r = GetRandomTile();
        int rx = r[0];
        int ry = r[1];

        if (grid[rx,ry] == Tile.PATH && !aPoints[rx,ry])
        {
            grid[rx,ry] = Tile.BLANK;
        }
    }

    /*
    ==================
    GetRandomTile

    Get random tile using weights
    ==================
    */
    private int[] GetRandomTile()
    {
        float rand = rng.RandfRange(0f, totalWeight);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(rand < weights[x,y])
                {
                    return new int[]{x,y};
                }

                rand -= weights[x,y];
            }
        }

        return new int[0];
    }

    /*
    ==================
    InitializeWeights
    ==================
    */
    private void InitializeWeights()
    {
        totalWeight = 0f;

        weights = new float[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                float roll = GetRollWeight(x,y);
                weights[x,y] = roll;
                totalWeight += roll;
            }
        }
    }

    /*
    ==================
    CellSelector
    ==================
    */
    public void CellSelector(int xMin, int xMax, int yMin, int yMax)
    {
        int xSize = Math.Abs(xMax - xMin);
        int ySize = Math.Abs(yMax - yMin);

        //int[,,] selectedCells = new int[xSize, ySize,2];
        //List<Vector2> selectedCells = new List<Vector2>();
        for( int x = 0; x < xSize; x++)
        {
            for(int y = 0; y < ySize; y++)
            {
                //selectedCells[x,y,0] = xMin+x;
                //selectedCells[x,y,1] = yMin+y;                

                grid[xMin+x,yMin+y] = Tile.PATH;
            }
        }
    }

    /*
    ====================
    GetRollWeight

    Returns probability of a tile being
    randomly selected based on its neighbour count
    ====================
    */
    private float GetRollWeight(int x, int y)
    {
        int n = 0;
        for(int i = 0; i < 4; i++)
        {
            int vx = x + adjacent[i,0];
            int vy = y + adjacent[i,1];

            if(!IsValid(vx,vy) || grid[vx,vy] == Tile.BLANK)
            {
                continue;
            }

            n++;
        }

        // TODO: Higher probability of chosing tiles with more blank neighbours
        // Should increase uiniform

        switch(n)
        {
            case 0:
                return 0.9f;

            case 1:
                return 0.8f;

            case 2:
                return 0.4f;
            
            case 3:
                return 0.3f;
            
            case 4:
                return 0.9f;
            
            default:
                return 0.2f;
        }
    }

    /*
    ====================
    CreateMultiMesh

    Create a multi mesh of wall tiles with static trimesh colliders
    ====================
    */
    private void CreateMultiMesh()
    {
        // Remove every multi-mesh instance before creating a new one
        for(int i = 0; i < GetChildCount(); i ++)
        {
            Node c = GetChild(i);

            if(c is MultiMeshInstance)
            {
                RemoveChild(c);
                c.QueueFree();

            }
        }

        PlaneMesh mesh = new PlaneMesh();
        mesh.Size = new Vector2(1,WALLHEIGHT) * SCALE;
        mesh.SurfaceSetMaterial(0, material_redbrick);

        MultiMesh multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
        multiMesh.ColorFormat = MultiMesh.ColorFormatEnum.None;
        multiMesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.None;
        multiMesh.Mesh = mesh;
        multiMesh.InstanceCount = 0;

        // Initialize rotations
        int[] r = new int[]{90,0,-90,180};
        List<Transform> transformArray = new List<Transform>();

        for(int x= 0; x < width; x++)
        {
            for(int y=0; y < height; y++)
            {       
                if(grid[x,y] == Tile.BLANK && enclosed)
                {
                    continue;
                }  

                // if(grid[x,y] == Tile.START || grid[x,y] == Tile.EXIT)
                // {
                //     continue;
                // }

                // Rotate and position transforms for plane meshes
                for(int i = 0; i < 4; i++)
                {
                    int vx = x + adjacent[i,0];
                    int vy = y + adjacent[i,1];
                    int vr = 0;
                    
                    bool valid = IsValid(vx,vy);

                    if (enclosed)
                    {
                        if (!valid || (valid && grid[vx,vy] == Tile.BLANK))
                        {
                            multiMesh.InstanceCount += 1;

                            Transform t = new Transform();
                            t.basis = Basis.Identity;
                            t.basis = t.basis.Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(-90));
                            t.basis = t.basis.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(r[i]));
                            t.origin = new Vector3(adjacent[i,0], WALLHEIGHT, adjacent[i, 1]) * SCALE/2  + new Vector3(x, 0, y) * SCALE;
                            transformArray.Add(t);
                        }
                    }
                    else
                    {
                        if ((!valid && grid[x,y] == Tile.BLANK) || (valid && grid[x,y] != Tile.BLANK && grid[vx,vy] == Tile.BLANK))
                        {
                            if(!valid)
                            {
                                vr = 180;
                            }

                            multiMesh.InstanceCount += 1;

                            Transform t = new Transform();
                            t.basis = Basis.Identity;
                            t.basis = t.basis.Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(-90));
                            t.basis = t.basis.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(r[i] + vr));
                            t.origin = new Vector3(adjacent[i,0], WALLHEIGHT, adjacent[i, 1]) * SCALE/2  + new Vector3(x, 0, y) * SCALE;
                            transformArray.Add(t);                            
                        }
                    }
                }
            }
        }

        MultiMeshInstance mmi = new MultiMeshInstance();
        StaticBody staticBody = new StaticBody();
        Shape shape = multiMesh.Mesh.CreateTrimeshShape();
        mmi.Multimesh = multiMesh;
        mmi.AddChild(staticBody);

        // Apply plane mesh transforms and trimesh collision to multi-mesh
        for(int i = 0; i < multiMesh.InstanceCount; i++)
        {
            multiMesh.SetInstanceTransform(i, transformArray[i]);
            CollisionShape collisionShape = new CollisionShape();
            collisionShape.Shape = shape;
            collisionShape.Transform = transformArray[i];
            staticBody.AddChild(collisionShape);
        }

        AddChild(mmi);
    }

    /*
    ===============
    FindArticulationPoints
    ===============
    */
    private bool[,] FindArticulationPoints(bool[,] relevant)
    {
        num = 1;
        low = new int[width, height];
        dfsNum = new int[width, height];
        isArticulation = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y= 0; y < height; y++)
            {
                isArticulation[x,y] = false;

                if(grid[x,y] == Tile.BLANK)
                {
                    continue;
                }
                if (relevant.Length > 0 && !relevant[x,y])
                {
                    continue;
                }

                int childCount = CutVertex(x,y, relevant).Item1;

                if(childCount > 1)
                {
                    isArticulation[x,y] = true;
                }
                else if(childCount == 0)
                {
                    isArticulation[x,y] = false;
                }

                return isArticulation;
            }
        }

        return isArticulation;
    }

    /*
    ===============
    CutVertex
    ===============
    */
    private (int, bool) CutVertex(int ux, int uy, bool[,] relevant)
    {
        int childCount = 0;
        bool isRelevant = relevant[ux,uy];
        if(isRelevant)
        {
            isArticulation[ux,uy] = true;
        }

        bool isRelevantSubtree = isRelevant;

        num++;
        low[ux,uy] = num;
        dfsNum[ux,uy] = num;


        for(int i = 0; i < 4; i++)
        {
            int vx = ux + adjacent[i,0];
            int vy = uy + adjacent[i,1];

            if (!IsValid(vx,vy))
            {
                continue;
            }

            if(grid[vx,vy] == Tile.BLANK)
            {
                continue;
            }

            if(dfsNum[vx,vy] == 0)
            {
                var cv = CutVertex(vx, vy, relevant);
                bool childRelevantSubtree = cv.Item2;
                childCount++;
                if(childRelevantSubtree)
                {
                    isRelevantSubtree = true;
                }
                if(low[vx,vy] >= dfsNum[ux,uy])
                {
                    if(relevant.Length == 0 || childRelevantSubtree)
                    {
                        isArticulation[ux,uy] = true;
                    }
                }
                low[ux,uy] = Math.Min(low[ux, uy], low[vx,vy]);
            }
            else
            {
                low[ux,uy] = Math.Min(low[ux,uy], dfsNum[vx,vy]);
            }
        }

        return (childCount, isRelevantSubtree);
    }
}

// /*
// ====================
// GetNeighbours
// ====================
// */
// private (Tile[],Vector2[]) GetNeighbours(int x, int y)
// {
//     List<Tile> neighbours = new List<Tile>();
//     List<Vector2> pos = new List<Vector2>();

//     for(int i = 0; i < 4; i++)
//     {
//         int vx = x + adjacent[i,0];
//         int vy = y + adjacent[i,1];

//         if(!IsValid(vx,vy))
//         {
//             continue;
//         }

//         neighbours.Add(grid[vx,vy]);
//         pos.Add(new Vector2(vx,vy));
//     }

//     return (neighbours.ToArray(), pos.ToArray());
// }