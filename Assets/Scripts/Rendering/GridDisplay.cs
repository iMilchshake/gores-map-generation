using System;
using Generator;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;

namespace Rendering
{
    public class GridDisplay
    {
        private readonly GameObject _squarePrefab; // this is also really stupid
        private GridTile[,] _gridDisplayTiles; // keeps track of initiated tiles
        private Map _currentMap; // map that is currently displayed
        private readonly GameObject _tileParentObject; // parent object to group all displayed tiles together

        private readonly Color _hookableColor;
        private readonly Color _unhookableColor;
        private readonly Color _freezeColor;
        private readonly Color _emptyColor;
        private readonly Color _obstacleColor;

        public GridDisplay(GameObject squarePrefab, Color hookableColor, Color unhookableColor, Color freezeColor,
            Color emptyColor, Color obstacleColor)
        {
            _squarePrefab = squarePrefab;
            _tileParentObject = new GameObject("Tiles");

            _hookableColor = hookableColor;
            _unhookableColor = unhookableColor;
            _freezeColor = freezeColor;
            _emptyColor = emptyColor;
            _obstacleColor = obstacleColor;
        }

        public void ClearDisplay()
        {
            // no tiles are being displayed -> skip
            if (_gridDisplayTiles == null)
                return;

            // remove all existing tiles
            for (int x = 0; x < _gridDisplayTiles.GetLength(0); x++)
            {
                for (int y = 0; y < _gridDisplayTiles.GetLength(1); y++)
                {
                    Object.Destroy(_gridDisplayTiles[x, y].Obj);
                }
            }

            _gridDisplayTiles = null;
        }

        public void DisplayGrid(Map map)
        {
            if (_currentMap == null) // initialize tiles if display function is called the first time
            {
                InitializeDisplayTiles(map);
                return; // rest can be skipped since initialize function already sets the correct color for tiles 
            }

            // check dimensions of new map
            if (!Map.CheckSameDimension(_currentMap, map))
                throw new IndexOutOfRangeException("grids have different dimension");

            // update display using new grid
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    // check if type of current tile changed, if yes update display
                    if (_currentMap[x, y] != map[x, y])
                    {
                        UpdateTileColor(_gridDisplayTiles[x, y], map[x, y]);
                    }
                }
            }

            _currentMap = map.Clone(); // save new map for next update
        }

        private void InitializeDisplayTiles(Map map)
        {
            if (_gridDisplayTiles != null)
                throw new InvalidOperationException("tiles have already been initialized");

            _gridDisplayTiles = new GridTile[map.Width, map.Height];
            _currentMap = map;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    GridTile tile = InitializeSquare(new Vector2(x, y), map[x, y]);
                    _gridDisplayTiles[x, y] = tile;
                }
            }
        }


        private GridTile InitializeSquare(Vector2 position, BlockType type)
        {
            // initialize Unity components
            var square = Object.Instantiate(_squarePrefab, new Vector3(position.x, position.y, 1.0f),
                Quaternion.identity);
            square.transform.SetParent(_tileParentObject.transform);
            var render = square.GetComponent<SpriteRenderer>();

            // initialize GridTile
            var tile = new GridTile(square, render);
            UpdateTileColor(tile, type);

            return tile;
        }

        private void UpdateTileColor(GridTile tile, BlockType type)
        {
            tile.SpriteRenderer.color = type switch
            {
                BlockType.Freeze => _freezeColor,
                BlockType.Unhookable => _unhookableColor,
                BlockType.Hookable => _hookableColor,
                BlockType.Empty => _emptyColor,
                BlockType.Obstacle => _obstacleColor,
                _ => _emptyColor
            };
        }
    }

    public class GridTile
    {
        public GameObject Obj;
        public SpriteRenderer SpriteRenderer;

        public GridTile(GameObject obj, SpriteRenderer spriteRenderer)
        {
            Obj = obj;
            SpriteRenderer = spriteRenderer;
        }
    }
}