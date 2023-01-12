using System;
using Generator;
using Rendering;
using UnityEngine;
using Util;
using Random = System.Random;

namespace MonoBehaviour
{
    [Serializable]
    public struct MapGenerationConfig
    {
        // general map config 
        public int maxIterations;
        public int mapHeight;
        public int mapWidth;
        public int seed;
        public Vector2Int[] targetPositions;
        public bool generatePlatforms;
        public bool enableTunnelMode;

        // initial state
        public Vector2Int initPosition;
        public int initKernelSize;
        public float initKernelCircularity;

        // walker config
        public float bestMoveProbability;
        public float kernelSizeChangeProb;
        public float kernelCircularityChangeProb;
        public KernelSizeConfig[] kernelConfig;

        // obstacle config
        public DistanceTransformMethod distanceTransformMethod;
        public float distanceThreshold;
    }

    public class MainMapGeneration : UnityEngine.MonoBehaviour
    {
        public GameObject squarePrefab;
        private MapGenerator _mapGen;
        private GridDisplay _gridDisplay;
        private Random _seedGenerator;

        [Header("Rendering Config")] public Color hookableColor;
        public Color unhookableColor;
        public Color freezeColor;
        public Color emptyColor;
        public Color obstacleColor;
        public int iterationsPerUpdate;

        [Header("Generation Config")] public bool lockSeed;
        public bool autoGenerate;
        public MapGenerationConfig configuration;

        // generation state
        private bool _generating = false;
        private int _currentIteration = 0;

        void Start()
        {
            _gridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
                obstacleColor);
            _gridDisplay.DisplayGrid(new Map(configuration.mapWidth,
                configuration.mapHeight)); // display empty map so tiles are initialized TODO: lol
            _seedGenerator = new Random(42);
            StartGeneration();
        }


        private void Update()
        {
            if ((autoGenerate || Input.GetKeyDown("r")) && !_generating)
                StartGeneration();

            if (Input.GetKeyDown("e") && !_generating)
            {
                Debug.Log($"exporting map {_mapGen.GetSeed()}");
                _mapGen.Map.ExportMap("" + _mapGen.GetSeed());
                Debug.Log("done");
            }

            if (_generating)
            {
                // do n update steps (n = iterationsPerUpdate)
                for (int i = 0; i < iterationsPerUpdate; i++)
                {
                    _mapGen.Step();
                    _currentIteration++;

                    if (_currentIteration > configuration.maxIterations ||
                        _mapGen.WalkerPos.Equals(_mapGen.GetCurrentTargetPos()))
                    {
                        _generating = false;
                        Debug.Log($"finished with {_currentIteration} iterations");
                        _mapGen.OnFinish();
                        _gridDisplay.DisplayGrid(_mapGen.Map);
                        break;
                    }
                }

                // update display
                _gridDisplay.DisplayGrid(_mapGen.Map);
            }
        }

        private void StartGeneration()
        {
            if (!lockSeed)
                configuration.seed = _seedGenerator.Next();

            _mapGen = new MapGenerator(configuration);
            _generating = true;
            _currentIteration = 0;
        }
    }
}