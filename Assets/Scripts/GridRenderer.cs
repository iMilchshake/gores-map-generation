using System;
using UnityEngine;
using Object = UnityEngine.Object;
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
        if (Input.GetKey("r"))
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
    public Color HookableColor = new(1f, 0.86f, 0.27f);
    public Color UnhookableColor = new(0.29f, 0.45f, 0.5f);
    public Color FreezeColor = new(0.01f, 0f, 0.02f);
    public Color EmptyColor = new(1.0f, 1.0f, 1.0f, 0.1f);

    private readonly GameObject _squarePrefab; // this is also really stupid
    private GridTile[,] gridDisplayTiles; // keeps track of initiated tiles
    private BlockType[,] currentGrid; // grid that is currently displayed

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
        if (currentGrid == null) // initialize tiles if display function is called the first time
        {
            InitializeDisplayTiles(grid);
            return; // rest can be skipped since initialize function already sets the correct color for tiles 
        }

        // check dimensions of new grid TODO: long if statement, maybe wrap inside a function?
        if (currentGrid.GetLength(0) != grid.GetLength(0) ||
            currentGrid.GetLength(1) != grid.GetLength(1))
        {
            throw new IndexOutOfRangeException("grids have different dimension");
        }

        // update display using new grid
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                // check if type of current tile changed, if yes update display
                if (currentGrid[x, y] != grid[x, y])
                {
                    Debug.Log($"[GridDisplay] Update: ({x},{y}) {currentGrid[x, y]} -> {grid[x, y]}");
                    UpdateTileColor(gridDisplayTiles[x, y], grid[x, y]);
                }
            }
        }

        currentGrid = grid;
    }

    private void InitializeDisplayTiles(BlockType[,] grid)
    {
        if (gridDisplayTiles != null)
            throw new InvalidOperationException("tiles have already been initialized");

        gridDisplayTiles = new GridTile[grid.GetLength(0), grid.GetLength(1)];
        currentGrid = grid;
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
        // initialize Unity components
        var square = Object.Instantiate(_squarePrefab, new Vector3(position.x, position.y, 1.0f), Quaternion.identity);
        var render = square.GetComponent<SpriteRenderer>();

        // initialize GridTile
        var tile = new GridTile(square, render);
        UpdateTileColor(tile, type);

        return tile;
    }

    private void UpdateTileColor(GridTile tile, BlockType type)
    {
        tile.SpriteRenderer.color = type switch
        {
            BlockType.Freeze => FreezeColor,
            BlockType.Unhookable => UnhookableColor,
            BlockType.Hookable => HookableColor,
            BlockType.Empty => EmptyColor,
            _ => EmptyColor
        };
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