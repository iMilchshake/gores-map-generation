using System;
using Unity.Profiling;
using UnityEngine;
using Random = System.Random;

public class MainMapGeneration : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;
    public Random SeedGenerator;

    [Header("Rendering Config")] public Color hookableColor;
    public Color unhookableColor;
    public Color freezeColor;
    public Color emptyColor;
    public Color obstacleColor;

    [Header("Initialization Config")] public int iterationsPerUpdate;
    public int maxIterations;
    public int mapHeight;
    public int mapWidth;
    public int margin;
    public bool forceSeed;

    [Header("Random Walker Config")] public float bestMoveProbability;
    public float kernelSizeChangeProb;
    public float kernelCircularityChangeProb;
    public KernelSizeConfig[] kernelConfig;

    [Header("Obstacle Config")] public DistanceTransformMethod distanceTransformMethod;
    public int distanceThreshold;

    private int _kernelSize = 3;
    private float _kernelCircularity = 0.0f;
    private bool _generating = false;
    private int _currentIteration = 0;

    static readonly ProfilerMarker MarkerMapGenStep = new("MapGeneration.Step");
    static readonly ProfilerMarker MarkerMapGenFinishStep = new("MapGeneration.FinishStep");

    void Start()
    {
        GridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
            obstacleColor);
        GridDisplay.DisplayGrid(new Map(mapWidth, mapHeight)); // display empty map so tiles are initialized TODO: lol
        SeedGenerator = new Random(42);
        StartGeneration();
    }


    private void Update()
    {
        if (Input.GetKeyDown("r") && !_generating)
            StartGeneration();

        if (_generating)
        {
            // do n update steps (n = iterationsPerUpdate)
            for (int i = 0; i < iterationsPerUpdate; i++)
            {
                using (MarkerMapGenStep.Auto())
                {
                    MapGen.Step();
                }

                _currentIteration++;

                if (_currentIteration > maxIterations || MapGen.WalkerPos.Equals(MapGen.GetCurrentTargetPos()))
                {
                    _generating = false;
                    Debug.Log($"finished with {_currentIteration} iterations");
                    MapGen.OnFinish(distanceTransformMethod, distanceThreshold);
                    GridDisplay.DisplayGrid(MapGen.Map);
                    Debug.Log("exporting map");
                    MapGen.Map.ExportMap("testMap");
                    Debug.Log("done");
                    break;
                }
            }

            // update display
            GridDisplay.DisplayGrid(MapGen.Map);
        }
    }

    private void StartGeneration()
    {
        MapGen = new MapGenerator(mapWidth, mapHeight,
            new Vector2Int(margin, margin),
            new[]
            {
                new Vector2Int(mapWidth - margin, margin),
                new Vector2Int(mapWidth - margin, mapHeight - margin),
                new Vector2Int(margin, mapHeight - margin)
            },
            bestMoveProbability,
            _kernelSize,
            _kernelCircularity,
            kernelSizeChangeProb,
            kernelCircularityChangeProb,
            kernelConfig,
            seed: forceSeed ? 0 : SeedGenerator.Next()
        );
        _generating = true;
        _currentIteration = 0;
    }
}