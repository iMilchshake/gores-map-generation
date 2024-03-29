﻿using System;
using Generator;
using UnityEngine;
using UnityEngine.Serialization;
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
        public Color startColor;
        public Color finishColor;
        public Color spawnColor;
        public Color marginFreezeColor;
    }

    public class MapRenderer
    {
        private Tilemap _tilemap;
        private Tile _tile;
        private Map _currentMap; // map that is currently displayed
        private MapColorPalette _mapColorPalette;

        public MapRenderer(Tile tile, Tilemap tilemap, int width, int height, MapColorPalette mapColorPalette)
        {
            _tilemap = tilemap;
            _tile = tile;
            _mapColorPalette = mapColorPalette;
            InitializeTiles(tile, width, height);
        }

        private void InitializeTiles(Tile tile, int width, int height)
        {
            tile.flags = TileFlags.None;
            var area = new BoundsInt { size = new Vector3Int(width, height, 1) };
            TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
            for (int index = 0; index < tileArray.Length; index++)
                tileArray[index] = tile;
            _tilemap.SetTilesBlock(area, tileArray);
        }

        public void DisplayMap(Map map)
        {
            if (_currentMap != null && !Map.CheckSameDimension(map, _currentMap))
            {
                _tilemap.ClearAllTiles();
                InitializeTiles(_tile, map.Width, map.Height);
                _currentMap = null;
            }

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
                    BlockType.Start => _mapColorPalette.startColor,
                    BlockType.Finish => _mapColorPalette.finishColor,
                    BlockType.Spawn => _mapColorPalette.spawnColor,
                    BlockType.MarginFreeze => _mapColorPalette.marginFreezeColor,
                    _ => throw new ArgumentOutOfRangeException()
                });
            }

            _currentMap = map.Clone(); // save map for next render update
        }

        public void UpdateColorMap(MapColorPalette mapColorPalette)
        {
            _mapColorPalette = mapColorPalette;
        }
    }
}