using UnityEngine;
using Random = System.Random;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;

    void Start()
    {
        MapGen = new MapGenerator(42, 100, 100);
        GridDisplay = new GridDisplay(squarePrefab);

        var map = MapGen.GenerateMap(500);
        GridDisplay.DisplayGrid(map);
    }


    private void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            var map = MapGen.GenerateMap(500);
            GridDisplay.DisplayGrid(map);
        }
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
    // define some colors TODO: move this somewhere else
    public Color Hookable = new Color(1f, 0.86f, 0.27f);
    public Color Unhookable = new Color(0.29f, 0.45f, 0.5f);
    public Color Freeze = new Color(0.01f, 0f, 0.02f);
    public Color Empty = new Color(1.0f, 1.0f, 1.0f, 0.1f);

    private readonly GameObject _squarePrefab; // this is also really stupid

    public GridTile[,] gridDisplayTiles; // keeps track of initiated tiles

    public GridDisplay(GameObject squarePrefab)
    {
        _squarePrefab = squarePrefab;
    }

    public void ClearDisplay()
    {
        // no tiles are being displayed -> skip
        if (gridDisplayTiles == null)
            return;

        // remove all existing tiles
        for (int x = 0; x < gridDisplayTiles.GetLength(0); x++)
        {
            for (int y = 0; y < gridDisplayTiles.GetLength(1); y++)
            {
                Object.Destroy(gridDisplayTiles[x, y].Obj);
            }
        }

        gridDisplayTiles = null;
    }

    public void DisplayGrid(BlockType[,] grid)
    {
        ClearDisplay(); // removes all existing tiles

        gridDisplayTiles = new GridTile[grid.GetLength(0), grid.GetLength(1)];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                GridTile tile = InitializeSquare(new Vector2(x, y), grid[x, y]);
                gridDisplayTiles[x, y] = tile;
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