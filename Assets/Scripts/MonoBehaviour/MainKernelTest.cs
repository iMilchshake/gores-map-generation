using Generator;
using Rendering;
using UnityEngine;
using Util;

namespace MonoBehaviour
{
    public class MainKernelTest : UnityEngine.MonoBehaviour
    {
        public GameObject squarePrefab;

        public MapGenerator MapGen;
        // public GridDisplay GridDisplay;

        [Header("Rendering Config")] public Color hookableColor;
        public Color unhookableColor;
        public Color freezeColor;
        public Color emptyColor;
        public Color obstacleColor;

        [Header("Initialization Config")] public int mapHeight;
        public int mapWidth;

        [Header("Initialization Config")] public int kernelSize;

        [Range(0.0f, 1.0f)] public float circularity;

        void Start()
        {
            // GridDisplay = new GridDisplay(squarePrefab, hookableColor, unhookableColor, freezeColor, emptyColor,
            //     obstacleColor);
            // GridDisplay.DisplayGrid(new Map(mapWidth, mapHeight)); // display empty map so tiles are initialized TODO: lol
        }

        // Update is called once per frame
        void Update()
        {
            var map = new Map(mapWidth, mapHeight);
            var kernel = KernelGenerator.GetKernel(kernelSize, circularity);
            // map.SetBlocks(mapWidth / 2, mapHeight / 2, kernel, BlockType.Empty);
            // GridDisplay.DisplayGrid(map);
        }
    }
}