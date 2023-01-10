using System;
using System.Collections.Generic;
using IO;
using UnityEngine;
using Util;

namespace Generator
{
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
            MapSerializer.ExportMap(this, name);
        }

        public void SetBlocks(int xPos, int yPos, bool[,] kernel, BlockType type)
        {
            var kernelOffset = (kernel.GetLength(0) - 1) / 2;
            var kernelSize = kernel.GetLength(0);

            for (var xKernel = 0; xKernel < kernelSize; xKernel++)
            {
                for (var yKernel = 0; yKernel < kernelSize; yKernel++)
                {
                    int x = xPos + (xKernel - kernelOffset);
                    int y = yPos + (yKernel - kernelOffset);
                    if (kernel[xKernel, yKernel] && x > 0 && x < Width && y > 0 && y < Height)
                        grid[x, y] = type;
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
        public Map Map { get; }
        public int Seed { get; }
        public Vector2Int WalkerPos;

        private readonly int _width;
        private readonly int _height;
        private RandomGenerator _rndGen;
        private float _bestMoveProbability;
        private float _kernelSizeChangeProb;
        private float _kernelCircularityChangeProb;
        private List<Vector2Int> _positions;
        private KernelGenerator _kernelGenerator;
        private Vector2Int[] _walkerTargetPositions;

        private bool[,] _kernel;
        private int _walkerTargetPosIndex = 0;
        private MapGeneratorMode _walkerMode;

        private int _tunnelRemainingSteps = 0;
        private Vector2Int _tunnelLastDir;

        public MapGenerator(int width, int height, Vector2Int startPos, Vector2Int[] targetPositions,
            float bestMoveProbability,
            int kernelSize, float kernelCircularity, float kernelSizeChangeProb, float kernelCircularityChangeProb,
            KernelSizeConfig[] kernelConfig, int seed)
        {
            Map = new Map(width, height);
            Seed = seed;
            WalkerPos = startPos;

            _width = width;
            _height = height;
            _rndGen = new RandomGenerator(seed);
            _bestMoveProbability = bestMoveProbability;
            _kernelSizeChangeProb = kernelSizeChangeProb;
            _kernelCircularityChangeProb = kernelCircularityChangeProb;
            _positions = new List<Vector2Int>();
            _kernelGenerator = new KernelGenerator(kernelConfig, kernelSize, kernelCircularity);
            _walkerTargetPositions = targetPositions;

            _walkerMode = MapGeneratorMode.DistanceProbability; // default mode 
            _kernel = _kernelGenerator.GetCurrentKernel();
            _positions.Add(new Vector2Int(WalkerPos.x, WalkerPos.y));
        }

        public void Step()
        {
            Vector2Int pickedMove;
            switch (_walkerMode)
            {
                case MapGeneratorMode.DistanceProbability:
                    var distanceProbabilities = GetDistanceProbabilities(3);
                    pickedMove = _rndGen.PickRandomMove(distanceProbabilities);
                    _kernelGenerator.Mutate(_kernelSizeChangeProb, _kernelCircularityChangeProb, _rndGen);
                    if (_rndGen.RandomBool(0.005f))
                    {
                        _walkerMode = MapGeneratorMode.Tunnel;
                        _tunnelRemainingSteps = 15;
                        _tunnelLastDir = pickedMove;
                    }

                    break;
                case MapGeneratorMode.Tunnel:
                    if (_tunnelRemainingSteps <= 0)
                        _walkerMode = MapGeneratorMode.DistanceProbability;
                    _tunnelRemainingSteps--;


                    // update direction
                    if (_rndGen.RandomBool(0.00f))
                    {
                        _tunnelLastDir = _rndGen.PickRandomMove(GetDistanceProbabilities(3));
                    }

                    _kernel = KernelGenerator.GetKernel(4, 0.0f);
                    pickedMove = _tunnelLastDir;
                    break;
                default:
                    throw new Exception("invalid mode");
            }


            // move walker by picked move and remove tiles using a given kernel
            WalkerPos += pickedMove;
            _positions.Add(new Vector2Int(WalkerPos.x, WalkerPos.y));
            Map.SetBlocks(WalkerPos.x, WalkerPos.y, _kernelGenerator.GetCurrentKernel(), BlockType.Empty);

            // test if current target was reached
            if (WalkerPos.Equals(GetCurrentTargetPos()) && _walkerTargetPosIndex < _walkerTargetPositions.Length - 1)
            {
                Debug.Log($"reached targetPos index={_walkerTargetPosIndex}");
                _walkerTargetPosIndex++;
            }
        }

        public void OnFinish(DistanceTransformMethod distanceTransformMethod, int distanceThreshold)
        {
            FillSpaceWithObstacles(distanceTransformMethod, distanceThreshold);
            GenerateFreeze();
            SetPlatforms();
        }

        public Vector2Int GetCurrentTargetPos()
        {
            return _walkerTargetPositions[_walkerTargetPosIndex];
        }

        private MoveArray GetDistanceProbabilities(int moveSize)
        {
            var moveCount = moveSize * moveSize;
            var probabilities = new MoveArray(moveSize);
            var validMoves = probabilities.GetAllValidMoves();

            // calculate distances for each possible move
            var moveDistances = new float[moveCount];
            for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
                moveDistances[moveIndex] = Vector2Int.Distance(_walkerTargetPositions[_walkerTargetPosIndex],
                    WalkerPos + validMoves[moveIndex]);

            // sort moves by their respective distance to the goal
            Array.Sort(moveDistances, validMoves);

            // assign each move a probability based on their index in the sorted order
            for (var moveIndex = 0; moveIndex < moveCount; moveIndex++)
                probabilities[validMoves[moveIndex]] =
                    MathUtil.GeometricDistribution(moveIndex + 1, _bestMoveProbability);

            // hotfix: dont allow diagonal moves TODO: MoveArray requires a proper rework since diagonal moves seem to add no value
            probabilities[-1, -1] = 0.0f;
            probabilities[1, -1] = 0.0f;
            probabilities[-1, 1] = 0.0f;
            probabilities[1, 1] = 0.0f;
            probabilities[0, 0] = 0.0f;
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

        private void SetPlatforms()
        {
            // very WIP, but kinda works?
            int minPlatformDistance = 200; // an average distance might allow for better platform placement
            int safeDistanceX = 3;
            int safeDistanceY = 4;

            int lastPlatformIndex = 0;
            int currentPositionIndex = 0;
            int positionsCount = _positions.Count;

            while (currentPositionIndex < positionsCount)
            {
                if (currentPositionIndex > lastPlatformIndex + minPlatformDistance)
                {
                    int x = _positions[currentPositionIndex].x;
                    int y = _positions[currentPositionIndex].y;
                    if (!Map.CheckTypeInArea(x - safeDistanceX, y - safeDistanceY, x + safeDistanceX, y + safeDistanceY,
                            BlockType.Hookable) && !Map.CheckTypeInArea(x - safeDistanceX, y - safeDistanceY,
                            x + safeDistanceX, y + safeDistanceY,
                            BlockType.Freeze))
                    {
                        // safe area, place platform
                        Map[x, y - 3] = BlockType.Hookable;
                        Map[x - 1, y - 3] = BlockType.Hookable;
                        Map[x - 2, y - 3] = BlockType.Hookable;
                        Map[x + 1, y - 3] = BlockType.Hookable;
                        Map[x + 2, y - 3] = BlockType.Hookable;
                        // Map[x - safeDistanceX, y - safeDistanceY] = BlockType.Unhookable;
                        // Map[x + safeDistanceX, y + safeDistanceY] = BlockType.Unhookable;

                        lastPlatformIndex = currentPositionIndex;
                    }
                }

                currentPositionIndex++;
            }
        }
    }
}