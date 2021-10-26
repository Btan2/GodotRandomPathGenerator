using Godot;
using System;
using System.Collections.Generic;

public class Maze_Generator : Spatial
{
    private SpatialMaterial material_redbrick = ResourceLoader.Load("res://Mat_RedBricks.tres") as SpatialMaterial;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private const int SCALE = 2;
    private enum Type {BLANK, PATH, END};
    private int num = 1;  
    private int gridSize = 20;
    private int[,] low;
    private int[,] dfsNum;
    private bool[,] isArticulation;
    private Type[,] grid;

    /*
    ==================
    _Ready
    ==================
    */
    public override void _Ready()
    {
        rng.Randomize();
        RandomPath();
        SpawnTiles();
        SetFloorRoof();
    }

    public void NewGrid()
    {
        RandomPath();
        SpawnTiles();
        SetFloorRoof();
    }

    /*
    ====================
    SetFloorRoof
    ====================
    */
    private void SetFloorRoof()
    {
        MeshInstance ceiling = GetNode("Ceiling") as MeshInstance;
        MeshInstance floor = GetNode("Floor") as MeshInstance;

        ceiling.Scale = Vector3.One * gridSize;
        Transform c_transform = ceiling.GlobalTransform;
	    c_transform.origin = new Vector3(gridSize*SCALE/2.105f, 2f, gridSize*SCALE/2.105f);
        ceiling.GlobalTransform = c_transform;

	    floor.Scale = Vector3.One * gridSize;
	    Transform f_transform = floor.GlobalTransform;
        f_transform.origin = new Vector3(gridSize*SCALE/2.105f, 0f, gridSize*SCALE/2.105f);
        floor.GlobalTransform = f_transform;
    }

    /*
    ====================
    RandomPath
    ====================
    */
    private void RandomPath()
    {
        bool[,] ends = new bool[gridSize,gridSize];
        grid = new Type[gridSize,gridSize]; 

        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                grid[x,y] = Type.PATH;
                ends[x,y] = false;
            }
        }   

        ends[0,0] = true;
        grid[0,0] = Type.END;

        int count = 0;
        while(count < 250)
        {
            var rx = rng.RandiRange(0,gridSize-1);
            var ry = rng.RandiRange(0,gridSize-1);

            if(grid[rx,ry] != Type.END)
            {
                ends[rx,ry] = true;
                grid[rx,ry] = Type.END;
                count++;
            }
        }

        bool[,] aPoints;
        while(true)
        {
            aPoints = FindArticulationPoints(ends);
            if(!RemovableTiles(aPoints))
            {
                break;
            }
            
            RemoveRandomTile(aPoints);
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
                if(grid[x,y] == Type.PATH && !aPoints[x,y])
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
        int x = rng.RandiRange(0, gridSize-1);
        int y = rng.RandiRange(0, gridSize-1);

        if (grid[x,y] == Type.PATH && !aPoints[x,y])
        {
            grid[x,y] = Type.BLANK;
        }
    }

    /*
    ====================
    SpawnTiles

    Create a multi mesh of wall tiles with static trimesh colliders
    ====================
    */
    private void SpawnTiles()
    {
        PlaneMesh mesh = new PlaneMesh();
        mesh.Size = new Vector2(1,1) * SCALE;
        mesh.SurfaceSetMaterial(0, material_redbrick);
        
        MultiMesh multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
        multiMesh.ColorFormat = MultiMesh.ColorFormatEnum.None;
        multiMesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.None;
        multiMesh.Mesh = mesh;
        multiMesh.InstanceCount = 0;

        List<Transform> transformArray = new List<Transform>();
        int[] r = new int[]{90,0,-90,180};
        int[,] p = {{1,0},{0,1},{-1,0},{0,-1}};

        for(int x= 0; x < gridSize; x++)
        {
            for(int y=0; y < gridSize; y++)
            {
                if(grid[x,y] == Type.BLANK)
                {
                    continue;
                }

                for(int i = 0; i < 4; i++)
                {
                    int vx = x + p[i,0];
                    int vy = y + p[i,1];

                    bool valid = IsValid(vx,vy);
                    if (!valid || (valid && grid[vx,vy]==Type.BLANK))
                    {
                        multiMesh.InstanceCount += 1;

                        Transform t = new Transform();
                        t.basis = Basis.Identity;
                        t.basis = t.basis.Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(-90));
                        t.basis = t.basis.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(r[i]));
                        t.origin = new Vector3(p[i,0], 1, p[i, 1]) + new Vector3(x, 0, y) * SCALE;
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

                if(grid[x,y] == Type.BLANK)
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

            if(grid[vx,vy] == Type.BLANK)
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