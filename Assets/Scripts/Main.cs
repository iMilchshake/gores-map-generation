using Unity.Profiling;
using UnityEngine;
using Random = System.Random;

public class Main : MonoBehaviour
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

    [Header("Generation Config")] public int iterationsPerUpdate;
    public int maxIterations;
    public int mapHeight;
    public int mapWidth;

    [Header("Random Walker Config")] public float bestMoveProbability;
    public float kernelSizeChangeProb;
    public float kernelCircularityChangeProb;

    private int _kernelSize = 3;
    private float _kernelCircularity = 0.0f;
    private bool _generating = false;
    private int _currentIteration = 0;

    static readonly ProfilerMarker MarkerMapGenStep = new("MapGeneration.Step");
    static readonly ProfilerMarker MarkerMapGenFinishStep = new("MapGeneration.FinishStep");

    void Start()
    {
        // generate map
        GridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
            obstacleColor);
        GridDisplay.DisplayGrid(new Map(mapWidth, mapHeight)); // display empty map so tiles are initialized TODO: lol
        SeedGenerator = new Random(42);
        StartGeneration();

        // var test = new int[11, 11];
        // ArrayUtils.FillArray2D(test, int.MaxValue);
        // test[5, 5] = 0;
        // Debug.Log(ArrayUtils.Array2DToString(test));
        // MathUtil.DistanceTransformCityBlock(test);
        // Debug.Log(ArrayUtils.Array2DToString(test));
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
                    GridDisplay.DisplayGrid(MapGen.Map);
                    Debug.Log($"finished with {_currentIteration} iterations");
                    using (MarkerMapGenFinishStep.Auto())
                    {
                        MapGen.OnFinish();
                    }

                    GridDisplay.DisplayGrid(MapGen.Map);
                    break;
                }
            }

            // update display
            GridDisplay.DisplayGrid(MapGen.Map);
        }
    }

    private void StartGeneration()
    {
        var margin = 50;

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
            seed: SeedGenerator.Next()
        );
        _generating = true;
        _currentIteration = 0;
    }
}