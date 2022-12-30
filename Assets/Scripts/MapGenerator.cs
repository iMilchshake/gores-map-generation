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

    // checks if the entire area is filled with the given BlockType
    public bool CheckArea(int x1, int y1, int x2, int y2, BlockType type)
    {
        for (var x = x1; x <= x2; x++)
        {
            for (var y = y1; y <= y2; y++)
            {
                if (grid[x, y] != type)
                    return false;
            }
        }

        return true;
    }

    public void FillArea(int x1, int y1, int x2, int y2, BlockType type)
    {
        for (var x = x1; x <= x2; x++)
        {
            for (var y = y1; y <= y2; y++)
            {
                grid[x, y] = type;
            }
        }
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
            _kernelSize = _rndGen.RandomChoice(new[] { 3, 3, 3, 3, 3, 5, 5, 5, 9 });

        if (updateCircularity)
            _kernelCircularity = _rndGen.RandomChoice(new[] { 0.0f, 0.3f, 0.7f });

        if (updateSize || updateCircularity)
        {
            if (_kernelSize == 3)
                _kernelCircularity = 0.0f; // circularity doesnt really make sense for a 3x3 kernel

            _kernel = KernelGenerator.GetCircularKernel(_kernelSize, _kernelCircularity);
        }
    }


    // expands a rectangle starting from a starting position until it cant be further expanded
    public (int, int, int, int) GetFloodFillArea(int xPos, int yPos)
    {
        // bottom left point of rectangle
        var x1 = xPos;
        var y1 = yPos;

        // top right point of rectangle
        var x2 = xPos;
        var y2 = yPos;

        var upLocked = false;
        var rightLocked = false;
        var downLocked = false;
        var leftLocked = false;

        while (!(leftLocked && upLocked && rightLocked && downLocked))
        {
            // expand left
            if (Map.CheckArea(x1 - 1, y1, x2, y2, BlockType.Empty))
                x1--;
            else
                leftLocked = true;

            // expand up
            if (Map.CheckArea(x1, y1, x2, y2 + 1, BlockType.Empty))
                y2++;
            else
                upLocked = true;

            // expand right
            if (Map.CheckArea(x1, y1, x2 + 1, y2, BlockType.Empty))
                x2++;
            else
                rightLocked = true;

            // expand down 
            if (Map.CheckArea(x1, y1 - 1, x2, y2, BlockType.Empty))
                y1--;
            else
                downLocked = true;

            Debug.Log($"{x1},{y1} - {x2},{y2} ");
        }

        return (x1, y1, x2, y2);
    }


    public void PlaceObstacle()
    {
        // get random empty point
        var (xPos, yPos) = _rndGen.GetRandomPositionWithType(Map, BlockType.Empty);
        Debug.Log($"found valid position at {xPos},{yPos}");

        // fill area around point
        var (x1, y1, x2, y2) = GetFloodFillArea(xPos, yPos);

        x1 += 2;
        y1 += 2;
        x2 -= 2;
        y2 -= 2;

        // check if area fulfills certain criteria for obstacles
        var areaWidth = x2 - x1;
        var areaHeight = y2 - y1;
        var area = areaWidth * areaHeight;
        Debug.Log($"{areaWidth}, {areaHeight}, {area}");
        if (areaWidth >= 2 && areaHeight >= 2 && area > 30)
        {
            Debug.Log("valid!");
            var maxRectSize = 5;
            var width = _rndGen.Rnd.Next(1, Math.Min(areaWidth + 1, maxRectSize));
            var height = _rndGen.Rnd.Next(1, Math.Min(areaHeight + 1, maxRectSize));
            Debug.Log(width);
            Debug.Log(height);

            var x = _rndGen.Rnd.Next(areaWidth - width);
            var y = _rndGen.Rnd.Next(areaHeight - height);
            Debug.Log($"{x},{y} - width:{width}, height:{height}");

            Map.FillArea(x1 + x, y1 + y, x1 + x + width, y1 + y + height, BlockType.Obstacle);
        }

        // Map.FillArea(x1, y1, x2, y2, BlockType.Freeze);
        // Map[xPos, yPos] = BlockType.Obstacle;
    }
}