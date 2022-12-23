using System;
using UnityEngine;

public class Map
{
    private BlockType[,] grid;
    public int width;
    public int height;

    public Map(int width, int height)
    {
        grid = new BlockType[width, height];
        this.width = width;
        this.height = height;
    }

    public Map(BlockType[,] grid)
    {
        this.grid = grid;
        width = grid.GetLength(0);
        height = grid.GetLength(1);
    }

    public BlockType this[int x, int y]
    {
        get => grid[x, y];
        set => grid[x, y] = value;
    }

    public string GetDebugString()
    {
        int emptyCount = 0;
        int hookCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (this[x, y] == BlockType.Empty)
                {
                    emptyCount++;
                }
                else if (this[x, y] == BlockType.Hookable)
                {
                    hookCount++;
                }
            }
        }

        return $"hook={hookCount}, empty={emptyCount}";
    }

    public static bool CheckSameDimension(Map map1, Map map2)
    {
        return map1.height == map2.height && map1.width == map2.width;
    }

    public Map Clone()
    {
        return new Map((BlockType[,])grid.Clone());
    }
}

public class MapGenerator
{
    public Vector2Int WalkerPos;

    public Map Map;
    private int _width;
    private int _height;
    private RandomGenerator _rndGen;

    public MapGenerator(int seed, int width, int height)
    {
        _rndGen = new RandomGenerator(seed);
        _width = width;
        _height = height;
        Initialize();
    }

    public void Initialize()
    {
        WalkerPos = new Vector2Int(_width / 2, _height / 2);
        Map = new Map(_width, _height);
    }

    public Map GenerateMap(int iterationCount)
    {
        Initialize();
        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            Step();
        }

        return Map;
    }

    public void Step()
    {
        WalkerPos += _rndGen.GetRandomDirectionVector();
        // Debug.Log($"{WalkerPos.x}, {WalkerPos.y}");
        Map[WalkerPos.x, WalkerPos.y] = BlockType.Empty;
    }
}