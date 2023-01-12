using System;
using Generator;
using Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;
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
        private MapGenerator _mapGen;
        private Random _seedGenerator;
        private MapRenderer _mapRenderer;

        [Header("Rendering Config")] 
        public MapColorPalette mapColorPalette;
        public int iterationsPerUpdate;
        public Tile testTile;
        public Tilemap tilemap;

        [Header("Generation Config")] public bool lockSeed;
        public bool autoGenerate;
        public MapGenerationConfig configuration;

        // generation state
        private bool _generating = false;
        private int _currentIteration = 0;

        void Start()
        {
            _seedGenerator = new Random(42);
            StartGeneration();

            _mapRenderer = new MapRenderer(testTile, tilemap, configuration.mapWidth, configuration.mapHeight,
                mapColorPalette);
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
                        _mapGen.OnFinish();
                        _mapRenderer.DisplayMap(_mapGen.Map);
                        Debug.Log($"finished with {_currentIteration} iterations");
                        break;
                    }
                }

                // update display
                _mapRenderer.DisplayMap(_mapGen.Map);
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