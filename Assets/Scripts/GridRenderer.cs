using System;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
using Random = System.Random;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public GridDisplay GridDisplay;

    void Start()
    {
        var mapGen = new MapGenerator(42, 100, 100);
        for (int iteration = 0; iteration < 500; iteration++)
        {
            mapGen.Step();
        }

        var grid = mapGen.Grid;

        GridDisplay = new GridDisplay(squarePrefab, grid);
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
    public GameObject obj;
    public SpriteRenderer spriteRenderer;

    public GridTile(GameObject obj, SpriteRenderer spriteRenderer)
    {
        this.obj = obj;
        this.spriteRenderer = spriteRenderer;
    }
}

public class MapGenerator
{
    private readonly int _height;
    private readonly int _width;
    public BlockType[,] Grid;
    private int WalkerPosX;
    private int WalkerPosY;

    private Random _rand;

    public MapGenerator(int seed, int width, int height)
    {
        _width = width;
        _height = height;
        Grid = new BlockType[width, height];
        WalkerPosX = _width / 2;
        WalkerPosY = _height / 2;
        _rand = new Random(seed);
    }

    public void Step()
    {
        int moveX = _rand.Next(-1, 2);
        int moveY = _rand.Next(-1, 2);
        WalkerPosX += moveX;
        WalkerPosY += moveY;
        Debug.Log($"{WalkerPosX}, {WalkerPosY}");
        Grid[WalkerPosX, WalkerPosY] = BlockType.Empty;
    }
}