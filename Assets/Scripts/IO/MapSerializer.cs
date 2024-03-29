﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Generator;
using Newtonsoft.Json;
using UnityEngine;
using Util;

namespace IO
{
    public struct InfoObject
    {
        public string author;
        public string version;
        public string credits;
        public string license;
        public string[] settings;
    }

    public struct VersionObject
    {
        public string type;
        public string created_by;
    }

    public struct StringPosition
    {
        public string x;
        public string y;
    }

    public struct IntPosition
    {
        public int x;
        public int y;
    }

    public struct Group
    {
        public string name;
        public StringPosition offset;
        public IntPosition parallax;
        public bool clipping;
        public StringPosition clip;
        public StringPosition clip_size;
    }

    public struct Tile
    {
        public int x;
        public int y;
        public int id;
        public bool mirrored;
        public int rotation;
    }

    public struct Layer
    {
        public string type;
        public int width;
        public int height;
        public Tile[] tiles;
    }

    public class MapSerializer
    {
        private const string MapDir = "Assets/GeneratedMaps/";
        // This class can export a given map in the "MapDir" format
        // Requires a tool like https://gitlab.com/Patiga/twmap to convert to a playable .map

        public static void ExportMap(Map map, string mapName)
        {
            // create required dictionaries
            Directory.CreateDirectory($@"{MapDir}\{mapName}\groups\0_Game\layers\");

            // create json files using above defined structs
            CreateVersionJson(mapName);
            CreateInfoJson(mapName);
            CreateGroupsJson(mapName);
            CreateGameLayer(mapName, map);

            ConvertDirMap(mapName);

            // remove mapDir
            Directory.Delete($@"{MapDir}\{mapName}", true);
        }

        public static void ConvertDirMap(string mapName)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsPlayer:
                    ConvertDirMapWindows(mapName);
                    break;
                case RuntimePlatform.LinuxEditor or RuntimePlatform.LinuxPlayer:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentException($"platform={Application.platform} is not supported");
            }
        }

        private static void ConvertDirMapWindows(string mapName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe"; // TODO: currently only works on windows
            process.StartInfo.Arguments =
                @$"/C python Assets\Scripts\IO\converter.py -i {MapDir}/{mapName} -o {MapDir}/{mapName}.map";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
        }

        private static void CreateInfoJson(string mapName)
        {
            File.WriteAllText($@"{MapDir}\{mapName}\info.json",
                JsonConvert.SerializeObject(new InfoObject
                {
                    author = "iMilchshake",
                    version = "",
                    credits = "",
                    license = "",
                    settings = Array.Empty<string>()
                }, Formatting.Indented));
        }

        private static void CreateVersionJson(string mapName)
        {
            File.WriteAllText($@"{MapDir}\{mapName}\version.json",
                JsonConvert.SerializeObject(new VersionObject
                {
                    type = "ddnet06",
                    created_by = ""
                }, Formatting.Indented));
        }

        private static void CreateGroupsJson(string mapName)
        {
            File.WriteAllText($@"{MapDir}\{mapName}\groups\0_Game\group.json",
                JsonConvert.SerializeObject(new Group
                {
                    name = "Game",
                    offset = { x = "0", y = "0" },
                    parallax = { x = 100, y = 100 },
                    clipping = false,
                    clip = { x = "0", y = "0" },
                    clip_size = { x = "0", y = "0" }
                }, Formatting.Indented));
        }


        private static void CreateGameLayer(string mapName, Map map)
        {
            // define tw-tiles for given map
            List<Tile> tiles = new List<Tile>();
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var flippedY = map.Height - y - 1; // tw map has flipped y orientation
                    if (map[x, y] != BlockType.Empty)
                        tiles.Add(new Tile
                        {
                            x = x,
                            y = flippedY,
                            id = map[x, y] switch
                            {
                                BlockType.Hookable => 1,
                                BlockType.Platform => 1,
                                BlockType.Freeze => 9,
                                BlockType.MarginFreeze => 9,
                                BlockType.Finish => 34,
                                BlockType.Start => 33,
                                BlockType.Spawn => 192,
                                _ => 0 // empty?
                            },
                            mirrored = false,
                            rotation = 0
                        });
                }
            }

            // define game layer
            Layer game = new Layer
            {
                type = "game",
                width = map.Width,
                height = map.Height,
                tiles = tiles.ToArray()
            };

            File.WriteAllText($@"{MapDir}\{mapName}\groups\0_Game\layers\0_Game.json",
                JsonConvert.SerializeObject(game, Formatting.Indented));
        }
    }
}