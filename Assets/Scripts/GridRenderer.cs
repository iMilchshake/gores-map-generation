using UnityEngine;
using Random = System.Random;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public GridDisplay GridDisplay;

    void Start()
    {
        // Generate map 
        var mapGen = new MapGenerator(42, 100, 100);
        var map = mapGen.GenerateMap(500);

        // Display map
        GridDisplay = new GridDisplay(squarePrefab, map);
    }
}

public enum BlockType
{
    Hookable,
    Unhookable,
    Freeze,
    Empty
}

public class GridDisplay
{
    public Color Hookable = new Color(1f, 0.86f, 0.27f);
    public Color Unhookable = new Color(0.29f, 0.45f, 0.5f);
    public Color Freeze = new Color(0.01f, 0f, 0.02f);
    public Color Empty = new Color(1.0f, 1.0f, 1.0f, 0.1f);
    private readonly GameObject _squarePrefab;

    public GridDisplay(GameObject squarePrefab, BlockType[,] grid)
    {
        _squarePrefab = squarePrefab;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                InitializeSquare(new Vector2(x, y), grid[x, y]);
            }
        }
    }

    private GridTile InitializeSquare(Vector2 position, BlockType type)
    {
        var square = Object.Instantiate(_squarePrefab, new Vector3(position.x, position.y, 1.0f), Quaternion.identity);
        var render = square.GetComponent<SpriteRenderer>();
        render.color = type switch
        {
            BlockType.Freeze => Freeze,
            BlockType.Unhookable => Unhookable,
            BlockType.Hookable => Hookable,
            BlockType.Empty => Empty,
            _ => Empty
        };

        return new GridTile(square, render);
    }
}

public class GridTile
{
    public GameObject Obj;
    public SpriteRenderer SpriteRenderer;

    public GridTile(GameObject obj, SpriteRenderer spriteRenderer)
    {
        Obj = obj;
        SpriteRenderer = spriteRenderer;
    }
}

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