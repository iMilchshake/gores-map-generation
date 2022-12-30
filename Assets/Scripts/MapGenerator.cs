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

    public static bool CheckSameDimension(Map map1, Map map2)
    {
        return map1.height == map2.height && map1.width == map2.width;
    }

    public Map Clone()
    {
        return new Map((BlockType[,])grid.Clone());
    }
}

public class MoveArray
{
    private float[,] _values;
    private readonly int _size;

    public MoveArray(int size)
    {
        _values = new float[size, size];
        this._size = size;
    }

    public float this[int x, int y]
    {
        get => _values[MoveIndexToArrayIndex(x), MoveIndexToArrayIndex(y)];
        set => _values[MoveIndexToArrayIndex(x), MoveIndexToArrayIndex(y)] = value;
    }

    public float this[Vector2Int vec]
    {
        get => this[vec.x, vec.y];
        set => this[vec.x, vec.y] = value;
    }

    private int MoveIndexToArrayIndex(int x)
    {
        // since _probabilities is squared, this function works for x and y
        int centerX = (_size - 1) / 2;
        return centerX + x;
    }

    public Vector2Int[] GetAllValidMoves()
    {
        Vector2Int[] validMoves = new Vector2Int[_size * _size];
        int index = 0;
        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                validMoves[index] = new Vector2Int(x - (_size - 1) / 2, y - (_size - 1) / 2);
                index++;
            }
        }

        return validMoves;
    }

    public void Normalize()
    {
        float sum = Sum();
        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                _values[x, y] /= sum;
            }
        }
    }

    public float MaxValue()
    {
        float maxValue = float.MinValue;
        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                if (_values[x, y] > maxValue)
                    maxValue = _values[x, y];
            }
        }

        return maxValue;
    }

    public float Sum()
    {
        float sum = 0;
        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                sum += _values[x, y];
            }
        }

        return sum;
    }

    public override String ToString()
    {
        var strOut = "";

        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size - 1; x++)
            {
                strOut += _values[x, y].ToString("0.00") + ",";
            }

            strOut += _values[_size - 1, y].ToString("0.00") + "\n";
        }

        return strOut;
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
        WalkerPos = new Vector2Int(25, _height / 2);
        WalkerTargetPos = new Vector2Int(175, _height / 2);

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
        int moveSize = 3;
        MoveArray probabilities = new MoveArray(moveSize);
        var validMoves = probabilities.GetAllValidMoves();
        float[] moveDistances = new float[moveSize * moveSize];

        // calculate distances for each possbible move
        for (int moveIndex = 0; moveIndex < moveSize * moveSize; moveIndex++)
        {
            float dist = Vector2Int.Distance(WalkerTargetPos, WalkerPos + validMoves[moveIndex]);
            moveDistances[moveIndex] = dist;
        }

        // sort moves by their respective distance to the goal
        // Debug.Log(String.Join(",", validMoves));
        Array.Sort(moveDistances, validMoves);
        // Debug.Log(String.Join(",", validMoves));

        // Debug.Log(probabilities);

        // assign each move a probability based on their index in the sorted order
        for (int moveIndex = 0; moveIndex < moveSize * moveSize; moveIndex++)
        {
            var move = validMoves[moveIndex];
            probabilities[move] = (float)MathUtil.GeometricDistribution(moveIndex + 1, 0.075f);
        }

        // Debug.Log(probabilities);
        probabilities.Normalize();
        // Debug.Log(probabilities);

        var pickedMove = _rndGen.PickRandomMove(probabilities);
        // Debug.Log(pickedMove);
        WalkerPos += pickedMove;
        // Debug.Log(WalkerPos);

        // TODO: this is dirty af
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Map[WalkerPos.x + x, WalkerPos.y + y] = BlockType.Empty;
            }
        }
    }
}