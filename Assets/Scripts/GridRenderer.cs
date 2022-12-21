using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject squarePrefab;
    public MapGenerator MapGen;
    public GridDisplay GridDisplay;

    void Start()
    {
        MapGen = new MapGenerator(42, 100, 100);
        GridDisplay = new GridDisplay(squarePrefab);

        var map = MapGen.GenerateMap(500);
        GridDisplay.DisplayGrid(map);
    }


    private void Update()
    {
        if (Input.GetKey("r"))
        {
            var map = MapGen.GenerateMap(500);
            GridDisplay.DisplayGrid(map);
        }
    }
}