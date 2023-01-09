using System;
using UnityEngine;

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

    public void ExportMap(string name)
    {
        IO.MapSerializer.ExportMap(this, name);
    }

    public void SetBlocks(int xPos, int yPos, bool[,] kernel, BlockType type)
    {
        var kernelOffset = (kernel.GetLength(0) - 1) / 2;
        var kernelSize = kernel.GetLength(0);

        for (var xKernel = 0; xKernel < kernelSize; xKernel++)
        {
            for (var yKernel = 0; yKernel < kernelSize; yKernel++)
            {
                if (kernel[xKernel, yKernel])
                    grid[xPos + (xKernel - kernelOffset), yPos + (yKernel - kernelOffset)] = type;
            }
        }
    }


    public Map Clone()
    {
        return new Map((BlockType[,])grid.Clone());
    }

    public int[,] GetDistanceMap(DistanceTransformMethod distanceTransformMethod)
    {
        // setup distance array 
        int[,] distance = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                distance[x, y] = grid[x, y] switch
                {
                    BlockType.Hookable => 0,
                    _ => int.MaxValue
                };
            }
        }

        // calculate distance transform
        MathUtil.DistanceTransform(distance, distanceTransformMethod);
        return distance;
    }

    public bool CheckTypeInArea(int x1, int y1, int x2, int y2, BlockType type)
    {
        // returns True if type is at least present once in the area
        for (var x = x1; x <= x2; x++)
        {
            for (var y = y1; y <= y2; y++)
            {
                if (grid[x, y] == type)
                    return true;
            }
        }

        return false;
    }

    public BlockType[,] GetCellNeighbors(int xPos, int yPos)
    {
        var neighbors = new BlockType[3, 3];
        for (var xOffset = 0; xOffset <= 2; xOffset++)
        {
            for (var yOffset = 0; yOffset <= 2; yOffset++)
            {
                neighbors[xOffset, yOffset] = grid[xPos - xOffset - 1, yPos - yOffset - 1];
            }
        }

        return neighbors;
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
    public Vector2Int[] WalkerTargetPositions;
    public int WalkerTargetPosIndex = 0;

    private float _bestMoveProbability;
    private float _kernelSizeChangeProb;
    private float _kernelCircularityChangeProb;

    public KernelGenerator kernelGenerator;
    private bool[,] _kernel;


    public MapGenerator(int width, int height, Vector2Int startPos, Vector2Int[] targetPositions,
        float bestMoveProbability,
        int kernelSize, float kernelCircularity, float kernelSizeChangeProb, float kernelCircularityChangeProb,
        KernelSizeConfig[] kernelConfig, int seed)
    {
        WalkerPos = startPos;
        WalkerTargetPositions = targetPositions;
        Map = new Map(width, height);

        _bestMoveProbability = bestMoveProbability;
        _kernelSizeChangeProb = kernelSizeChangeProb;
        _kernelCircularityChangeProb = kernelCircularityChangeProb;
        _width = width;
        _height = height;
        _rndGen = new RandomGenerator(seed);

        kernelGenerator = new KernelGenerator(kernelConfig, kernelSize, kernelCircularity);
        _kernel = kernelGenerator.GetCurrentKernel();
    }

    public void Step()
    {
        // pick a random move based on the distance towards the current target position 
        var distanceProbabilities = GetDistanceProbabilities(3);

        // hotfix: dont allow diagonal moves TODO: MoveArray requires a proper rework since diagonal moves seem to add no value
        distanceProbabilities[-1, -1] = 0.0f;
        distanceProbabilities[1, -1] = 0.0f;
        distanceProbabilities[-1, 1] = 0.0f;
        distanceProbabilities[1, 1] = 0.0f;
        distanceProbabilities[0, 0] = 0.0f;
        distanceProbabilities.Normalize();

        // pick a move based on the probabilities
        var pickedMove = _rndGen.PickRandomMove(distanceProbabilities);

        // move walker by picked move and remove tiles using a given kernel
        WalkerPos += pickedMove;
        kernelGenerator.Mutate(_kernelSizeChangeProb, _kernelCircularityChangeProb, _rndGen);
        Map.SetBlocks(WalkerPos.x, WalkerPos.y, kernelGenerator.GetCurrentKernel(), BlockType.Empty);

        // test if current target was reached
        if (WalkerPos.Equals(GetCurrentTargetPos()) && WalkerTargetPosIndex < WalkerTargetPositions.Length - 1)
        {
            Debug.Log($"reached targetPos index={WalkerTargetPosIndex}");
            WalkerTargetPosIndex++;
        }
    }

    public void OnFinish(DistanceTransformMethod distanceTransformMethod, int distanceThreshold)
    {
        FillSpaceWithObstacles(distanceTransformMethod, distanceThreshold);
        GenerateFreeze();
    }

    public Vector2Int GetCurrentTargetPos()
    {
        return WalkerTargetPositions[WalkerTargetPosIndex];
    }

    private MoveArray GetDistanceProbabilities(int moveSize)
    {
        var moveCount = moveSize * moveSize;
        var probabilities = new MoveArray(moveSize);
        var validMoves = probabilities.GetAllValidMoves();

        // calculate distances for each possible move
        var moveDistances = new float[moveCount];
        for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
            moveDistances[moveIndex] = Vector2Int.Distance(WalkerTargetPositions[WalkerTargetPosIndex],
                WalkerPos + validMoves[moveIndex]);

        // sort moves by their respective distance to the goal
        Array.Sort(moveDistances, validMoves);

        // assign each move a probability based on their index in the sorted order
        for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
            probabilities[validMoves[moveIndex]] = MathUtil.GeometricDistribution(moveIndex + 1, _bestMoveProbability);
        probabilities.Normalize(); // normalize the probabilities so that they sum up to 1

        return probabilities;
    }


    private void FillSpaceWithObstacles(DistanceTransformMethod distanceTransformMethod, int distanceThreshold)
    {
        var distances = Map.GetDistanceMap(distanceTransformMethod);
        var width = distances.GetLength(0);
        var height = distances.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (distances[x, y] >= distanceThreshold)
                {
                    Map[x, y] = BlockType.Hookable;
                }
            }
        }
    }

    private void GenerateFreeze()
    {
        // iterate over every cell of the map
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                // if a hookable tile is nearby -> set freeze
                if (Map[x, y] == BlockType.Empty &&
                    Map.CheckTypeInArea(x - 1, y - 1, x + 1, y + 1, BlockType.Hookable))
                    Map[x, y] = BlockType.Freeze;
            }
        }
    }
}