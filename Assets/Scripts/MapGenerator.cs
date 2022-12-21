using UnityEngine;
using Random = System.Random;

public class MapGenerator
{
    public readonly int Height;
    public readonly int Width;
    public BlockType[,] Grid;
    public Vector2Int WalkerPos;
    private Random _rand;

    public MapGenerator(int seed, int width, int height)
    {
        _rand = new Random(seed);
        Width = width;
        Height = height;
        WalkerPos = new Vector2Int(width / 2, height / 2);

        Grid = new BlockType[width, height];
    }

    public BlockType[,] GenerateMap(int iterationCount)
    {
        Grid = new BlockType[Width, Height];
        WalkerPos = new Vector2Int(Width / 2, Height / 2);

        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            Step();
        }

        return Grid;
    }

    public void Step()
    {
        WalkerPos += new Vector2Int(_rand.Next(-1, 2), _rand.Next(-1, 2));
        Debug.Log($"{WalkerPos.x}, {WalkerPos.y}");
        Grid[WalkerPos.x, WalkerPos.y] = BlockType.Empty;
    }
}