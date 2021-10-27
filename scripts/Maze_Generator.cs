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
    private KinematicBody player;
    private MeshInstance floor;
    private SpatialMaterial material_redbrick;
    
    private const int SCALE = 10;
    private const int WALLHEIGHT = 2;
    private const float MAXEMPTY = 1.0f;

    private int num = 1;  
    private int gridSize = 40;
    private float totalWeight = 0;
    private float[,] weights;
    private int[,] low;
    private int[,] dfsNum;
    private bool[,] isArticulation;

    private enum Tile {BLANK, PATH, END};
    
    private Tile[,] grid;   

    /*
    ==================
    _Ready
    ==================
    */
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();       
        player = GetNode("Player") as KinematicBody;
        floor = GetNode("Map/Floor") as MeshInstance;
        material_redbrick = ResourceLoader.Load("res://Mat_RedBricks.tres") as SpatialMaterial;
        
        NewGrid();
    }

    /*
    ==================
    _Input
    ==================
    */
    public override void _Input(InputEvent @event)
    {
        if(Input.IsKeyPressed((int)KeyList.P))
        {
            NewGrid();
        }
    }

    /*
    ====================
    NewGrid
    ====================
    */
    public void NewGrid()
    {
        SetPlayerPos(Vector3.Up * 2);
        SetFloor();
        GeneratePath();
        CreateMultiMesh();
    }

    /*
    ====================
    SetPlayerPos
    ====================
    */
    public void SetPlayerPos(Vector3 pos)
    {
        Transform t = player.GlobalTransform;
        t.origin = pos;
        player.GlobalTransform = t;
    }

    /*
    ====================
    SetFloor
    ====================
    */
    private void SetFloor()
    {

	    floor.Scale = Vector3.One * (gridSize+1) * SCALE;
	    Transform f_transform = floor.GlobalTransform;
        f_transform.origin = new Vector3(gridSize*SCALE/2f, 0f, gridSize*SCALE/2f);
        floor.GlobalTransform = f_transform;
    }

    /*
    ====================
    GeneratePath
    ====================
    */
    private void GeneratePath()
    {
        bool[,] ends = new bool[gridSize,gridSize];
        grid = new Tile[gridSize,gridSize]; 

        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                grid[x,y] = Tile.PATH;
                ends[x,y] = false;
            }
        }

        ends[0,0] = true;
        grid[0,0] = Tile.END;

        Node map = GetNode("Map");
        for(int i = 0; i < map.GetChildCount(); i++)
        {            
		    if(map.GetChild(i) is Spatial)
            {
                var c = map.GetChild(i) as Spatial;
			    int x = (int)(c.GlobalTransform.origin.x);
			    int y = (int)(c.GlobalTransform.origin.z);
                x = Mathf.Clamp(x,0,gridSize-1);
                y = Mathf.Clamp(y,0,gridSize-1);
                
			    ends[x,y] = true;
			    grid[x,y] = Tile.END;
            }
        }

                
        int count = 0;

        // var max_ends = (int)(gridSize*gridSize)/2;
        // while(count < max_ends)
        // {
        //     InitializeWeights();

        //     int[] r = GetRandomTile();
        //     if (r.Length <= 0)
        //     {
        //         continue;
        //     }
        //     // Pick random weighted ends
        //     if(grid[r[0],r[1]] != Tile.END)
        //     {
        //         ends[r[0],r[1]] = true;
        //         grid[r[0],r[1]] = Tile.END;
        //         count++;
        //     }
        // }

        bool[,] aPoints;        
        while(count < (gridSize*gridSize/2))
        {
            InitializeWeights();

            aPoints = FindArticulationPoints(ends);
            if(!RemovableTiles(aPoints))
            {
                break;
            }
            
            RemoveRandomTile(aPoints);

            count++;
        }
    }

    /*
    ==================
    RemovableTiles
    ==================
    */
    private bool RemovableTiles(bool[,] aPoints)
    {
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
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
        //int rx = rng.RandiRange(0,gridSize-1);
        //int ry = rng.RandiRange(0,gridSize-1);

        if (grid[rx,ry] == Tile.PATH && !aPoints[rx,ry])
        {
            grid[rx,ry] = Tile.BLANK;
        }
    }

    /*
    ==================
    GetRandomTile
    ==================
    */
    private int[] GetRandomTile()
    {
        float roll = rng.RandfRange(0f, totalWeight);
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                if(weights[x,y] > roll)
                {
                    return new int[]{x,y};
                }
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
        weights = new float[gridSize, gridSize];

        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                totalWeight += GetRollWeight(x,y);
                weights[x,y] = totalWeight;
            }
        }
    }

    /*
    ====================
    GetRollWeight

    Calculates and returns probability of a tile being
    randomly selected based on its neighbour count
    ====================
    */
    private float GetRollWeight(int x, int y)
    {
        int[,] d = new int[,]{{1,0},{0,1},{-1,0},{0,-1}};
        int n = 0;
        for(int i = 0; i < 4; i++)
        {
            int vx = x + d[i,0];
            int vy = y + d[i,1];

            if(!IsValid(vx,vy))
            {
                continue;
            }
            
            // Add to neighbour count
            if(grid[vx,vy] != Tile.BLANK)
            {
                n++;
            }
        }

        switch(n)
        {
            case 0:
                return 0.0f;
            case 1:
                return 1.0f;
            case 2:
                return 0.4f;
            case 3:
                return 0.5f;
            case 4:
                return 0.3f;
            default:
                return 0.3f;
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
        foreach(Node c in GetChildren())
        {
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

        // Initialize rotations and neighbour direction arrays
        int[] r = new int[]{90,0,-90,180};
        int[,] p = {{1,0},{0,1},{-1,0},{0,-1}};
        List<Transform> transformArray = new List<Transform>();

        for(int x= 0; x < gridSize; x++)
        {
            for(int y=0; y < gridSize; y++)
            {
                if(grid[x,y] == Tile.BLANK)
                {
                    continue;
                }

                // Rotate and position transforms for plane meshes

                for(int i = 0; i < 4; i++)
                {
                    int vx = x + p[i,0];
                    int vy = y + p[i,1];

                    bool valid = IsValid(vx,vy);
                    if (!valid || (valid && grid[vx,vy] == Tile.BLANK))
                    {
                        multiMesh.InstanceCount += 1;

                        Transform t = new Transform();
                        t.basis = Basis.Identity;
                        t.basis = t.basis.Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(-90));
                        t.basis = t.basis.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(r[i]));
                        t.origin = new Vector3(p[i,0], WALLHEIGHT, p[i, 1]) * SCALE/2  + new Vector3(x, 0, y) * SCALE;
                        transformArray.Add(t);
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
    IsValid
    ===============
    */
    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }

    /*
    ===============
    FindArticulationPoints
    ===============
    */
    private bool[,] FindArticulationPoints(bool[,] relevant)
    {
        num = 1;
        low = new int[gridSize, gridSize];
        dfsNum = new int[gridSize, gridSize];
        isArticulation = new bool[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y= 0; y < gridSize; y++)
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

        int[,] p = {{1,0},{0,1},{-1,0},{0,-1}};
        for(int i = 0; i < 4; i++)
        {
            int vx = ux + p[i,0];
            int vy = uy + p[i,1];

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