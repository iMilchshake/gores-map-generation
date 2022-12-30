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
        _size = size;
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
        var centerX = (_size - 1) / 2;
        return centerX + x;
    }

    public Vector2Int[] GetAllValidMoves()
    {
        var validMoves = new Vector2Int[_size * _size];
        var index = 0;
        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _size; y++)
            {
                validMoves[index] = new Vector2Int(x - (_size - 1) / 2, y - (_size - 1) / 2);
                index++;
            }
        }

        return validMoves;
    }

    public void Normalize()
    {
        var sum = Sum();
        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _size; y++)
            {
                _values[x, y] /= sum;
            }
        }
    }

    public float MaxValue()
    {
        var maxValue = float.MinValue;
        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _size; y++)
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
        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _size; y++)
            {
                sum += _values[x, y];
            }
        }

        return sum;
    }

    public override String ToString()
    {
        var strOut = "";

        for (var y = 0; y < _size; y++)
        {
            for (var x = 0; x < _size - 1; x++)
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
    public Map Map;
    private readonly int _width;
    private readonly int _height;
    private readonly RandomGenerator _rndGen;

    public Vector2Int WalkerPos;
    public Vector2Int WalkerTargetPos;
    private float _bestMoveProbability;
    private bool[,] _kernel;

    public MapGenerator(int seed, int width, int height)
    {
        _rndGen = new RandomGenerator(seed);
        _width = width;
        _height = height;
        Map = new Map(width, height);
    }

    public void Setup(Vector2Int startPos, Vector2Int targetPos, float bestMoveProbability, int kernelSize,
        float kernelCircularity)
    {
        WalkerPos = startPos;
        WalkerTargetPos = targetPos;
        _bestMoveProbability = bestMoveProbability;
        Map = new Map(_width, _height);
        _kernel = KernelGenerator.GetCircularKernel(kernelSize, kernelCircularity);
    }


    public void Step()
    {
        // pick a random move based on the distance towards the current target position 
        var distanceProbabilities = GetDistanceProbabilities(3);
        var pickedMove = _rndGen.PickRandomMove(distanceProbabilities);

        // move walker by picked move and remove tiles using a given kernel
        WalkerPos += pickedMove;
        SetBlocks(_kernel, BlockType.Empty);
    }

    private MoveArray GetDistanceProbabilities(int moveSize)
    {
        var moveCount = moveSize * moveSize;
        var probabilities = new MoveArray(moveSize);
        var validMoves = probabilities.GetAllValidMoves();

        // calculate distances for each possible move
        var moveDistances = new float[moveCount];
        for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
            moveDistances[moveIndex] = Vector2Int.Distance(WalkerTargetPos, WalkerPos + validMoves[moveIndex]);

        // sort moves by their respective distance to the goal
        Array.Sort(moveDistances, validMoves);

        // assign each move a probability based on their index in the sorted order
        for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
            probabilities[validMoves[moveIndex]] = MathUtil.GeometricDistribution(moveIndex + 1, _bestMoveProbability);
        probabilities.Normalize(); // normalize the probabilities so that they sum up to 1

        return probabilities;
    }

    private void SetBlocks(bool[,] kernel, BlockType type)
    {
        var kernelOffset = (kernel.GetLength(0) - 1) / 2;
        var kernelSize = kernel.GetLength(0);

        for (var x = 0; x < kernelSize; x++)
        {
            for (var y = 0; y < kernelSize; y++)
            {
                if (kernel[x, y])
                    Map[WalkerPos.x + (x - kernelOffset), WalkerPos.y + (y - kernelOffset)] = type;
            }
        }
    }
}