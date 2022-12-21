using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class GridDisplay
{
    // define some colors TODO: move this somewhere else
    public Color HookableColor = new(1f, 0.86f, 0.27f);
    public Color UnhookableColor = new(0.29f, 0.45f, 0.5f);
    public Color FreezeColor = new(0.01f, 0f, 0.02f);
    public Color EmptyColor = new(1.0f, 1.0f, 1.0f, 0.1f);

    private readonly GameObject _squarePrefab; // this is also really stupid
    private GridTile[,] _gridDisplayTiles; // keeps track of initiated tiles
    private BlockType[,] _currentGrid; // grid that is currently displayed

    public GridDisplay(GameObject squarePrefab)
    {
        _squarePrefab = squarePrefab;
    }

    public void ClearDisplay()
    {
        // no tiles are being displayed -> skip
        if (_gridDisplayTiles == null)
            return;

        // remove all existing tiles
        for (int x = 0; x < _gridDisplayTiles.GetLength(0); x++)
        {
            for (int y = 0; y < _gridDisplayTiles.GetLength(1); y++)
            {
                Object.Destroy(_gridDisplayTiles[x, y].Obj);
            }
        }

        _gridDisplayTiles = null;
    }

    public void DisplayGrid(BlockType[,] grid)
    {
        if (_currentGrid == null) // initialize tiles if display function is called the first time
        {
            InitializeDisplayTiles(grid);
            return; // rest can be skipped since initialize function already sets the correct color for tiles 
        }

        // check dimensions of new grid TODO: long if statement, maybe wrap inside a function?
        if (_currentGrid.GetLength(0) != grid.GetLength(0) ||
            _currentGrid.GetLength(1) != grid.GetLength(1))
        {
            throw new IndexOutOfRangeException("grids have different dimension");
        }

        // update display using new grid
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                // check if type of current tile changed, if yes update display
                if (_currentGrid[x, y] != grid[x, y])
                {
                    Debug.Log($"[GridDisplay] Update: ({x},{y}) {_currentGrid[x, y]} -> {grid[x, y]}");
                    UpdateTileColor(_gridDisplayTiles[x, y], grid[x, y]);
                }
            }
        }

        _currentGrid = grid;
    }

    private void InitializeDisplayTiles(BlockType[,] grid)
    {
        if (_gridDisplayTiles != null)
            throw new InvalidOperationException("tiles have already been initialized");

        _gridDisplayTiles = new GridTile[grid.GetLength(0), grid.GetLength(1)];
        _currentGrid = grid;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                GridTile tile = InitializeSquare(new Vector2(x, y), grid[x, y]);
                _gridDisplayTiles[x, y] = tile;
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