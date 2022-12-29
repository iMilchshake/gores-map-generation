using System;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;
    public int iterationsPerUpdate;
    public int iterations;

    private bool _generating = false;
    private int _currentIteration = 0;

    void Start()
    {
        // generate map
        MapGen = new MapGenerator(42, 100, 100);
        MapGen.Step();
        //var map = MapGen.GenerateMap(iterations);

        // display map
        // GridDisplay = new GridDisplay(squarePrefab);
        // GridDisplay.DisplayGrid(map);
    }


    private void Update()
    {
        if (Input.GetKeyDown("r") && !_generating)
        {
            MapGen.Initialize();
            _generating = true;
            _currentIteration = 0;
        }

        if (_generating)
        {
            if (_currentIteration > iterations)
                _generating = false;

            // do n update steps (n = iterationsPerUpdate)
            for (int i = 0; i < iterationsPerUpdate; i++)
            {
                MapGen.Step();
                _currentIteration++;
            }

            // update display
            GridDisplay.DisplayGrid(MapGen.Map);
        }
    }
}