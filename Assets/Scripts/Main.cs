using UnityEngine;
using Random = System.Random;

public class GridRenderer : MonoBehaviour
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

    void Start()
    {
        // generate map
        GridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
            obstacleColor);
        GridDisplay.DisplayGrid(new Map(mapWidth, mapHeight)); // display empty map so tiles are initialized TODO: lol
        SeedGenerator = new Random(42);
        StartGeneration();

        var test = new int[11, 11];
        ArrayUtils.FillArray2D(test, int.MaxValue);
        test[5, 5] = 0;
        Debug.Log(ArrayUtils.Array2DToString(test));
        var testOut = MathUtil.DistanceTransformCityBlock(test);
        Debug.Log(ArrayUtils.Array2DToString(testOut));
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
                MapGen.Step();
                _currentIteration++;

                if (_currentIteration > maxIterations || MapGen.WalkerPos.Equals(MapGen.WalkerTargetPos))
                {
                    _generating = false;
                    GridDisplay.DisplayGrid(MapGen.Map);
                    Debug.Log($"finished with {_currentIteration} iterations");
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
            new Vector2Int(25, mapHeight / 2),
            new Vector2Int(mapWidth - 25, mapHeight / 2),
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