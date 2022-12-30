using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;

    [Header("Rendering Config")] 
    public Color hookableColor;
    public Color unhookableColor;
    public Color freezeColor;
    public Color emptyColor;
    public Color obstacleColor;
    
    [Header("Generation Config")] 
    public int iterationsPerUpdate;
    public int maxIterations;
    public int mapHeight;
    public int mapWidth;
    
    [Header("Random Walker Config")] 
    public float bestMoveProbability;
    public float kernelSizeChangeProb;
    public float kernelCircularityChangeProb;

    private int _kernelSize = 3;
    private float _kernelCircularity = 0.0f;
    private bool _generating = false;
    private int _currentIteration = 0;

    void Start()
    {
        // generate map
        MapGen = new MapGenerator(42, mapWidth, mapHeight);
        GridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
            obstacleColor);
        GridDisplay.DisplayGrid(MapGen.Map);

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
        MapGen.Setup(new Vector2Int(25, mapHeight / 2),
            new Vector2Int(mapWidth - 25, mapHeight / 2),
            bestMoveProbability,
            _kernelSize,
            _kernelCircularity,
            kernelSizeChangeProb,
            kernelCircularityChangeProb);
        _generating = true;
        _currentIteration = 0;
    }
}