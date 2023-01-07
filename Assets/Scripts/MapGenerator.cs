using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Map
{
    private BlockType[,] grid;
    public int Width;
    public int Height;

    public Map(int width, int height)
    {
        grid = new BlockType[width, height];
        this.Width = width;
        this.Height = height;
    }

    public Map(BlockType[,] grid)
    {
        this.grid = grid;
        Width = grid.GetLength(0);
        Height = grid.GetLength(1);
    }

    public BlockType this[int x, int y]
    {
        get => grid[x, y];
        set => grid[x, y] = value;
    }

    public static bool CheckSameDimension(Map map1, Map map2)
    {
        return map1.Height == map2.Height && map1.Width == map2.Width;
    }

    public Map Clone()
    {
        return new Map((BlockType[,])grid.Clone());
    }

    public int[,] GetDistanceMap()
    {
        // setup array
        int[,] distance = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // if (grid[x, y] != BlockType.Hookable)
                // {
                //     distance[x, y] = int.MaxValue;
                // }
                distance[x, y] = grid[x, y] switch
                {
                    BlockType.Hookable => 0,
                    _ => int.MaxValue
                };
            }
        }

        // calculate distance transform
        MathUtil.DistanceTransformCityBlock(distance);
        return distance;
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
        return ArrayUtils.Array2DToString(_values);
    }
}

public class MapGenerator
{
    public Map Map;
    private readonly int _width;
    private readonly int _height;
    private RandomGenerator _rndGen = new(0);

    public Vector2Int WalkerPos;
    public Vector2Int WalkerTargetPos;
    private float _bestMoveProbability;
    private float _kernelSizeChangeProb;
    private float _kernelCircularityChangeProb;

    private int _kernelSize;
    private float _kernelCircularity;
    private bool[,] _kernel;


    public MapGenerator(int width, int height, Vector2Int startPos, Vector2Int targetPos, float bestMoveProbability,
        int kernelSize, float kernelCircularity, float kernelSizeChangeProb, float kernelCircularityChangeProb,
        int seed)
    {
        WalkerPos = startPos;
        WalkerTargetPos = targetPos;
        Map = new Map(width, height);

        _bestMoveProbability = bestMoveProbability;
        _kernelSizeChangeProb = kernelSizeChangeProb;
        _kernelCircularityChangeProb = kernelCircularityChangeProb;
        _width = width;
        _height = height;
        _rndGen = new RandomGenerator(seed);
        _kernelSize = kernelSize;
        _kernelCircularity = kernelCircularity;
        _kernel = KernelGenerator.GetCircularKernel(_kernelSize, _kernelCircularity);
    }

    public void Step()
    {
        // pick a random move based on the distance towards the current target position 
        var distanceProbabilities = GetDistanceProbabilities(3);
        var pickedMove = _rndGen.PickRandomMove(distanceProbabilities);

        // move walker by picked move and remove tiles using a given kernel
        WalkerPos += pickedMove;
        UpdateKernel();
        SetBlocks(_kernel, BlockType.Empty);
    }

    public void OnFinish()
    {
        FillSpace();
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

    private void UpdateKernel()
    {
        var updateSize = _rndGen.RandomBool(_kernelSizeChangeProb);
        var updateCircularity = _rndGen.RandomBool(_kernelCircularityChangeProb);

        if (updateSize)
            _kernelSize = _rndGen.RandomChoice(new[] { 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 5, 9 });

        if (updateCircularity)
            _kernelCircularity = _rndGen.RandomChoice(new[] { 0.0f, 0.3f, 0.7f });

        if (updateSize || updateCircularity)
        {
            if (_kernelSize == 3)
                _kernelCircularity = 0.0f; // circularity doesnt really make sense for a 3x3 kernel

            _kernel = KernelGenerator.GetCircularKernel(_kernelSize, _kernelCircularity);
        }
    }

    private void FillSpace()
    {
        var distances = Map.GetDistanceMap();
        var width = distances.GetLength(0);
        var height = distances.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (distances[x, y] >= 7)
                {
                    Map[x, y] = BlockType.Obstacle;
                }
            }
        }
    }
}