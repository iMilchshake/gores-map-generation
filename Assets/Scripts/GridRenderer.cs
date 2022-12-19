using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public GridDisplay GridDisplay;

    private const int Width = 10;
    private const int Height = 10;

    private BlockType[,] _grid = new BlockType[Width, Height];

    void Start()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if ((x + y) % 5 == 0)
                {
                    _grid[x, y] = BlockType.Unhookable;
                }
                else
                {
                    _grid[x, y] = BlockType.Hookable;
                }
            }
        }

        GridDisplay = new GridDisplay(squarePrefab, _grid);
    }
}

public enum BlockType
{
    Hookable,
    Unhookable,
    Freeze
}


public class GridDisplay
{
    public Color Hookable = new Color(1f, 0.86f, 0.27f);
    public Color Unhookable = new Color(0.29f, 0.45f, 0.5f);
    public Color Freeze = new Color(0.01f, 0f, 0.02f);
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
        var square = Object.Instantiate(_squarePrefab, position, Quaternion.identity);
        var render = square.GetComponent<SpriteRenderer>();

        switch (type)
        {
            case BlockType.Hookable:
                render.color = Hookable;
                break;
            case BlockType.Unhookable:
                render.color = Unhookable;
                break;
            case BlockType.Freeze:
                render.color = Freeze;
                break;
        }


        return new GridTile(square, render);
    }
}

public class GridTile
{
    public GameObject obj;
    public SpriteRenderer spriteRenderer;

    public GridTile(GameObject obj, SpriteRenderer spriteRenderer)
    {
        this.obj = obj;
        this.spriteRenderer = spriteRenderer;
    }
}