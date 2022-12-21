using UnityEngine;
using Random = System.Random;

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
}

public class MapGenerator
{
    public Vector2Int WalkerPos;

    private Map _map;
    private Random _rand;

    public MapGenerator(int seed, int width, int height)
    {
        WalkerPos = new Vector2Int(width / 2, height / 2);
        _rand = new Random(seed);
        _map = new Map(width, height);
    }

    public Map GenerateMap(int iterationCount)
    {
        _map = new Map(_map.width, _map.height);
        WalkerPos = new Vector2Int(_map.width / 2, _map.height / 2);

        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            Step();
        }

        return _map;
    }

    public void Step()
    {
        WalkerPos += new Vector2Int(_rand.Next(-1, 2), _rand.Next(-1, 2));
        Debug.Log($"{WalkerPos.x}, {WalkerPos.y}");
        _map[WalkerPos.x, WalkerPos.y] = BlockType.Empty;
    }
}