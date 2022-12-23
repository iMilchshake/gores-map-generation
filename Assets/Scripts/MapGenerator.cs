using System;
using UnityEditor.VersionControl;
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

    public static bool CheckSameDimension(Map map1, Map map2)
    {
        return map1.height == map2.height && map1.width == map2.width;
    }

    public Map Clone()
    {
        return new Map((BlockType[,])grid.Clone());
    }
}

public class MoveProbabilities
{
    private float[,] _probabilities;
    private int size;

    public MoveProbabilities(int size)
    {
        _probabilities = new float[size, size];
        this.size = size;
    }

    public float this[int x, int y]
    {
        get => _probabilities[MoveIndexToArrayIndex(x), MoveIndexToArrayIndex(y)];
        set => _probabilities[MoveIndexToArrayIndex(x), MoveIndexToArrayIndex(y)] = value;
    }

    public float this[Vector2Int vec]
    {
        get => this[vec.x, vec.y];
        set => this[vec.x, vec.y] = value;
    }

    private int MoveIndexToArrayIndex(int x)
    {
        // since _probabilities is squared, this function works for x and y
        int centerX = (size - 1) / 2;
        return centerX + x;
    }

    public Vector2Int[] GetAllValidMoves()
    {
        Vector2Int[] validMoves = new Vector2Int[size * size];
        int index = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                validMoves[index] = new Vector2Int(x - (size - 1) / 2, y - (size - 1) / 2);
                index++;
            }
        }

        return validMoves;
    }
}

public class MapGenerator
{
    public Vector2Int WalkerPos;
    public Vector2Int WalkerTargetPos;

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
        WalkerTargetPos = new Vector2Int(95, _height / 2);

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
        MoveProbabilities probabilities = new MoveProbabilities(3);
        var validMoves = probabilities.GetAllValidMoves();

        foreach (var move in validMoves)
        {
            var prob = probabilities[move];
            Debug.Log(prob);
        }

        WalkerPos += _rndGen.GetRandomDirectionVector();
        Map[WalkerPos.x, WalkerPos.y] = BlockType.Empty;
    }
}