using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;

    public int iterationsPerUpdate;
    public int maxIterations;
    public int mapHeight;
    public int mapWidth;
    public float bestMoveProbability;
    public int kernelSize;
    public float kernelCircularity;

    private bool _generating = false;
    private int _currentIteration = 0;

    void Start()
    {
        // generate map
        MapGen = new MapGenerator(42, mapWidth, mapHeight);
        GridDisplay = new GridDisplay(squarePrefab);
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
            kernelSize,
            kernelCircularity);
        _generating = true;
        _currentIteration = 0;
    }
}