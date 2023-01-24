using System;
using Generator;
using IO;
using Rendering;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Util;
using Random = System.Random;
using Tile = UnityEngine.Tilemaps.Tile;

namespace MonoBehaviour
{
    [Serializable]
    public struct MapLayoutConfig
    {
        public string layoutName;
        public Vector2Int initPosition;
        public Vector2Int[] waypoints;
        public int mapHeight;
        public int mapWidth;
    }

    [Serializable]
    public struct MapGenerationConfig
    {
        public string configName;

        // general map config 
        public int maxIterations;
        public int seed;
        public bool generatePlatforms;
        public bool enableTunnelMode;

        // initial state
        public int initKernelSize;
        public float initKernelCircularity;

        // walker config
        public float bestMoveProbability;
        public float kernelSizeChangeProb;
        public float kernelCircularityChangeProb;
        public float kernelOuterSizeMarginProb;
        public float kernelOuterCircularityProb;
        public int waypointReachedDistance;
        public KernelSizeConfig[] kernelConfig;

        // tunnel config TODO: lengths should depend on width
        public float tunnelProbability;
        public int[] tunnelLengths;
        public int[] tunnelWidths;

        // obstacle config
        public DistanceTransformMethod distanceTransformMethod;
        public float distanceThreshold;
        public float preDistanceNoise;
        public int gridDistance;
    }

    public class MainMapGeneration : UnityEngine.MonoBehaviour
    {
        public static MainMapGeneration instance;
        private MapGenerator _mapGen;
        private Random _seedGenerator;
        private MapRenderer _mapRenderer;

        [Header("Rendering Config")] public MapColorPalette mapColorPalette;
        public int iterationsPerUpdate;
        public Tile testTile;
        public Tilemap tilemap;

        [Header("Generation Config")] public bool lockSeed;
        public bool autoGenerate;
        public MapGenerationConfig generationConfig;
        public MapLayoutConfig layoutConfig;

        // generation state
        private bool _generating = false;
        private int _currentIteration = 0;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            _seedGenerator = new Random(42);
            _mapRenderer = new MapRenderer(testTile, tilemap, layoutConfig.mapWidth, layoutConfig.mapHeight,
                mapColorPalette);

            Debug.Log(string.Join(",", ConfigSerializer.GetMapGenerationConfigs()));

            // ConfigSerializer.ExportConfig(generationConfig);
            // generationConfig = ConfigSerializer.ImportConfig("main");
            StartGeneration();
        }

        private void Update()
        {
            if ((autoGenerate || Input.GetKeyDown("r")) && !_generating)
                StartGeneration();

            if (Input.GetKeyDown("e") && !_generating)
            {
                string mapName = $"{generationConfig.configName}_{layoutConfig.layoutName}_{generationConfig.seed}";
                Debug.Log($"exporting map {mapName}");
                _mapGen.Map.ExportMap(mapName);
                Debug.Log("done");
            }

            if (_generating)
            {
                // do n update steps (n = iterationsPerUpdate)
                for (int i = 0; i < iterationsPerUpdate; i++)
                {
                    _mapGen.Step();
                    _currentIteration++;

                    if (_currentIteration > generationConfig.maxIterations || _mapGen.finished)
                    {
                        _generating = false;
                        _mapGen.OnFinish();
                        UIHandler.instance.SetEnabled(true);
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
                generationConfig.seed = _seedGenerator.Next();

            _mapGen = new MapGenerator(generationConfig, layoutConfig);
            _mapRenderer.UpdateColorMap(mapColorPalette);
            Debug.Log("" + mapColorPalette.marginFreezeColor);
            _generating = true;
            _currentIteration = 0;

            UIHandler.instance.SetEnabled(false); // disable UI
        }
    }
}