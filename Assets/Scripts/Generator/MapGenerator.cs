using System;
using System.Collections.Generic;
using System.Linq;
using IO;
using MonoBehaviour;
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

        public void SetBlocks(int xPos, int yPos, bool[,] kernel, BlockType type, bool updateBlocksOnly)
        {
            var kernelOffset = (kernel.GetLength(0) - 1) / 2;
            var kernelSize = kernel.GetLength(0);

            for (var xKernel = 0; xKernel < kernelSize; xKernel++)
            {
                for (var yKernel = 0; yKernel < kernelSize; yKernel++)
                {
                    int x = xPos + (xKernel - kernelOffset);
                    int y = yPos + (yKernel - kernelOffset);
                    if (kernel[xKernel, yKernel] && // only update if the kernel is true
                        x > 0 && x < Width && y > 0 && y < Height && // Check if in bounds
                        (!updateBlocksOnly || grid[x, y] == BlockType.Hookable)) // Only update hookable blocks
                    {
                        grid[x, y] = type; // TODO: save setter?
                    }
                }
            }
        }

        public Map Clone()
        {
            return new Map((BlockType[,])grid.Clone());
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

        public bool CheckAnyInArea(int x1, int y1, int x2, int y2)
        {
            // returns True if type is at least present once in the area
            for (var x = x1; x <= x2; x++)
            {
                for (var y = y1; y <= y2; y++)
                {
                    if (grid[x, y].IsAny())
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
        public Vector2Int[] moves;
        public float[] probabilities;
        public readonly int size;

        public MoveArray(bool allowDiagonal)
        {
            size = allowDiagonal ? 8 : 4;
            moves = new Vector2Int[size];
            probabilities = new float[size];

            int index = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) // skip (0,0) move
                        continue;

                    if (!allowDiagonal && x != 0 && y != 0) // skip diagonal moves, if disallowed
                        continue;

                    moves[index++] = new Vector2Int(x, y);
                }
            }
        }

        public void Normalize()
        {
            float sum = Sum();
            for (int i = 0; i < size; i++)
            {
                probabilities[i] /= sum;
            }
        }

        public float Sum()
        {
            return probabilities.Sum();
        }

        public override String ToString()
        {
            return $"{moves}, {probabilities}";
        }
    }

    public class MapGenerator
    {
        // config
        public MapGenerationConfig generationConfig;
        public MapLayoutConfig layoutConfig;

        // data structures
        public Map Map { get; }
        private RandomGenerator _rndGen;
        private List<Vector2Int> _positions;
        private KernelGenerator _kernelGenerator;

        // walker state
        public Vector2Int WalkerPos;
        private bool[,] _kernel;
        private int _walkerTargetPosIndex = 0;
        private MapGeneratorState _walkerState;
        private int stepCount;
        public bool finished;

        // tunnel mode state
        private int _tunnelRemainingSteps = 0;
        private Vector2Int _tunnelDir;

        public MapGenerator(MapGenerationConfig generationConfig, MapLayoutConfig layoutConfig)
        {
            this.generationConfig = generationConfig;
            this.layoutConfig = layoutConfig;

            Map = new Map(generationConfig.mapWidth, generationConfig.mapHeight);
            _rndGen = new RandomGenerator(generationConfig.seed);
            _positions = new List<Vector2Int>();
            _positions.Add(new Vector2Int(WalkerPos.x, WalkerPos.y));
            _kernelGenerator =
                new KernelGenerator(generationConfig.kernelConfig, generationConfig.initKernelSize, generationConfig.initKernelCircularity, _rndGen);

            WalkerPos = generationConfig.initPosition;
            _kernel = _kernelGenerator.GetCurrentKernel();
            _walkerState = MapGeneratorState.DistanceProbability; // start default mode 
        }

        public int GetSeed()
        {
            return generationConfig.seed;
        }

        private Vector2Int StepTunnel()
        {
            if (_tunnelRemainingSteps <= 0)
            {
                _walkerState = MapGeneratorState.DistanceProbability;
                _kernelGenerator.Mutate(generationConfig);
            }

            _tunnelRemainingSteps--;
            return _tunnelDir;
        }

        private Vector2Int StepDistanceProbabilities()
        {
            var distanceProbabilities = GetDistanceProbabilities();
            var pickedMove = _rndGen.PickRandomMove(distanceProbabilities);
            _kernelGenerator.Mutate(generationConfig);

            // switch to tunnel mode with a certain probability TODO: state pattern?
            if (generationConfig.enableTunnelMode && _rndGen.RandomBool(generationConfig.tunnelProbability))
            {
                _walkerState = MapGeneratorState.Tunnel;
                _tunnelRemainingSteps = _rndGen.RandomChoice(generationConfig.tunnelLengths);
                int tunnelWidth = _rndGen.RandomChoice(generationConfig.tunnelWidths);
                _kernelGenerator.ForceKernelConfig(tunnelWidth, 0.0f, tunnelWidth, 0.0f);
                _tunnelDir = GetBestMove();
            }

            return pickedMove;
        }

        public void Step()
        {
            if (finished)
                throw new Exception("Map generation already finished");

            // calculate next move depending on current _walkerMode
            Vector2Int pickedMove = _walkerState switch
            {
                MapGeneratorState.DistanceProbability => StepDistanceProbabilities(),
                MapGeneratorState.Tunnel => StepTunnel(),
                _ => throw new ArgumentOutOfRangeException()
            };

            // move walker by picked move and remove tiles using a given kernel
            WalkerPos += pickedMove;
            _positions.Add(new Vector2Int(WalkerPos.x, WalkerPos.y));

            // apply outer kernel (update freeze, only override walls)
            Map.SetBlocks(WalkerPos.x, WalkerPos.y, _kernelGenerator.GetCurrentOuterKernel(), BlockType.MarginFreeze,
                updateBlocksOnly: true);

            // apply inner kernel (set as empty)
            Map.SetBlocks(WalkerPos.x, WalkerPos.y, _kernelGenerator.GetCurrentKernel(), BlockType.Empty,
                updateBlocksOnly: false);

            // update targetPosition if current one was reached
            Vector2Int currentTarget = GetCurrentTargetPos();
            bool targetReached = Math.Abs(WalkerPos.x - currentTarget.x) + Math.Abs(WalkerPos.y - currentTarget.y) <=
                                 generationConfig.waypointReachedDistance;

            bool targetsLeft = _walkerTargetPosIndex < layoutConfig.waypoints.Length - 1;
            if (targetsLeft && targetReached)
            {
                _walkerTargetPosIndex++;
            }
            else if (!targetsLeft && targetReached)
            {
                finished = true;
            }

            stepCount++;
        }

        public void OnFinish()
        {
            FillSpaceWithObstacles(generationConfig.distanceTransformMethod, generationConfig.distanceThreshold, generationConfig.preDistanceNoise,
                generationConfig.gridDistance);

            GenerateFreeze();

            PlaceRoom(WalkerPos.x, WalkerPos.y, 10, 10, 0, 0, BlockType.Finish);
            PlaceRoom(generationConfig.initPosition.x, generationConfig.initPosition.y, 10, 10, 0, 0, BlockType.Start);
            PlacePlatform(WalkerPos.x, WalkerPos.y);
            PlacePlatform(generationConfig.initPosition.x, generationConfig.initPosition.y);
            Map[generationConfig.initPosition.x, generationConfig.initPosition.y + 1] = BlockType.Spawn;

            PlaceRoomBorder(WalkerPos.x, WalkerPos.y, 10, 10, 1, 1, BlockType.Finish);
            PlaceRoomBorder(generationConfig.initPosition.x, generationConfig.initPosition.y, 10, 10, 1, 1, BlockType.Start);

            if (generationConfig.generatePlatforms)
            {
                GeneratePlatforms(700, 4, 4, 0, 4);
            }
        }

        public Vector2Int GetCurrentTargetPos()
        {
            return layoutConfig.waypoints[_walkerTargetPosIndex];
        }

        private MoveArray GetDistanceProbabilities()
        {
            var moveArray = new MoveArray(allowDiagonal: false);

            // calculate distances for each possible move
            var moveDistances = new float[moveArray.size];
            for (var moveIndex = 0; moveIndex < moveArray.size; moveIndex++)
                moveDistances[moveIndex] =
                    Vector2Int.Distance(GetCurrentTargetPos(), WalkerPos + moveArray.moves[moveIndex]);

            // sort moves by their respective distance to the goal
            Array.Sort(moveDistances, moveArray.moves);

            // assign each move a probability based on their index in the sorted order
            for (var i = 0; i < moveArray.size; i++)
                moveArray.probabilities[i] = MathUtil.GeometricDistribution(i + 1, generationConfig.bestMoveProbability);

            moveArray.Normalize(); // normalize the probabilities so that they sum up to 1

            return moveArray;
        }

        private Vector2Int GetBestMove()
        {
            var moveArray = new MoveArray(allowDiagonal: false);

            // calculate distances for each possible move
            Vector2Int bestMove = Vector2Int.zero;
            float bestDistance = float.MaxValue;
            for (var moveIndex = 0; moveIndex < moveArray.size; moveIndex++)
            {
                float distance = Vector2Int.Distance(GetCurrentTargetPos(), WalkerPos + moveArray.moves[moveIndex]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMove = moveArray.moves[moveIndex];
                }
            }

            return bestMove;
        }


        private void FillSpaceWithObstacles(DistanceTransformMethod distanceTransformMethod, float distanceThreshold,
            float preDistanceNoise, int gridDistance)
        {
            float[,] distances =
                MathUtil.DistanceTransform(Map, distanceTransformMethod, _rndGen, preDistanceNoise, gridDistance);
            int width = distances.GetLength(0);
            int height = distances.GetLength(1);
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
            for (var x = 0; x < generationConfig.mapWidth; x++)
            {
                for (var y = 0; y < generationConfig.mapHeight; y++)
                {
                    if (Map[x, y] == BlockType.Empty &&
                        (Map.CheckTypeInArea(x - 1, y - 1, x + 1, y + 1, BlockType.Hookable) ||
                         Map.CheckTypeInArea(x - 1, y - 1, x + 1, y + 1, BlockType.MarginFreeze)))
                    {
                        Map[x, y] = BlockType.Freeze;
                    }
                }
            }
        }

        private void GeneratePlatforms(int minPlatformDistance, int safeTop, int safeRight, int safeDown, int safeLeft)
        {
            int lastPlatformIndex = 0;
            int currentPositionIndex = 0;
            int positionsCount = _positions.Count;

            while (currentPositionIndex < positionsCount)
            {
                if (currentPositionIndex > lastPlatformIndex + minPlatformDistance)
                {
                    int x = _positions[currentPositionIndex].x;
                    int y = _positions[currentPositionIndex].y;
                    if (!CheckPlatformArea(x, y, safeLeft, safeTop, safeRight, safeDown))
                    {
                        currentPositionIndex++;
                        continue;
                    }

                    // move platform area down until it hits a wall
                    bool movedDown = false;
                    while (CheckPlatformArea(x, y, safeLeft, safeTop, safeRight, safeDown))
                    {
                        y--;
                        movedDown = true;
                    }

                    // place platform at last safe position
                    PlacePlatform(x, movedDown ? y + 1 : y);
                    lastPlatformIndex = currentPositionIndex;
                }

                currentPositionIndex++;
            }
        }

        private bool CheckPlatformArea(int x, int y, int safeLeft, int safeTop, int safeRight, int safeDown)
        {
            return !Map.CheckAnyInArea(x - safeLeft, y - safeDown, x + safeRight, y + safeTop);
        }

        private void PlacePlatform(int x, int y)
        {
            Map[x, y] = BlockType.Platform;
            Map[x - 1, y] = BlockType.Platform;
            Map[x - 2, y] = BlockType.Platform;
            Map[x + 1, y] = BlockType.Platform;
            Map[x + 2, y] = BlockType.Platform;
        }

        private void PlaceRoom(int xCenter, int yCenter, int width, int height, int xMargin, int yMargin,
            BlockType type)
        {
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int xPos = xCenter + x - (width / 2);
                    int yPos = yCenter + y - (height / 2);
                    Map[xPos, yPos] = BlockType.Empty;
                }
            }
        }

        private void PlaceRoomBorder(int xCenter, int yCenter, int width, int height, int xMargin, int yMargin,
            BlockType type)
        {
            int marginWidth = width + 2 * xMargin;
            int marginHeight = height + 2 * yMargin;

            for (int x = 0; x <= marginWidth; x++)
            {
                for (int y = 0; y <= marginHeight; y++)
                {
                    int xPos = xCenter + x - marginWidth / 2;
                    int yPos = yCenter + y - marginHeight / 2;
                    if (x == 0 || x == marginWidth || y == 0 || y == marginHeight)
                    {
                        if (Map[xPos, yPos].IsFreezeOrEmpty())
                        {
                            Map[xPos, yPos] = type;
                        }
                    }
                }
            }
        }
    }
}