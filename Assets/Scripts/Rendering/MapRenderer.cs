using System;
using Generator;
using UnityEngine;
using UnityEngine.Tilemaps;
using Util;

namespace Rendering
{
    [Serializable]
    public struct MapColorPalette
    {
        public Color hookableColor;
        public Color unhookableColor;
        public Color freezeColor;
        public Color emptyColor;
        public Color obstacleColor;
        public Color platformColor;
        public Color debugColor;
    }

    public class MapRenderer
    {
        private Tilemap _tilemap;
        private Map _currentMap; // map that is currently displayed
        private MapColorPalette _mapColorPalette;

        public MapRenderer(Tile tile, Tilemap tilemap, int width, int height, MapColorPalette mapColorPalette)
        {
            _tilemap = tilemap;
            _mapColorPalette = mapColorPalette;

            // initialize tiles
            tile.flags = TileFlags.None;
            var area = new BoundsInt { size = new Vector3Int(width, height, 1) };
            TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
            for (int index = 0; index < tileArray.Length; index++)
                tileArray[index] = tile;
            tilemap.SetTilesBlock(area, tileArray);
        }


        public void DisplayMap(Map map)
        {
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                if (_currentMap != null && _currentMap[x, y] == map[x, y])
                    continue; // skip update if tile didnt change

                _tilemap.SetColor(new Vector3Int(x, y), map[x, y] switch
                {
                    BlockType.Empty => _mapColorPalette.emptyColor,
                    BlockType.Freeze => _mapColorPalette.freezeColor,
                    BlockType.Hookable => _mapColorPalette.hookableColor,
                    BlockType.Obstacle => _mapColorPalette.obstacleColor,
                    BlockType.Unhookable => _mapColorPalette.unhookableColor,
                    BlockType.Platform => _mapColorPalette.platformColor,
                    BlockType.Debug => _mapColorPalette.debugColor,
                    _ => Color.red
                });
            }

            _currentMap = map.Clone(); // save map for next render update
        }
    }
}