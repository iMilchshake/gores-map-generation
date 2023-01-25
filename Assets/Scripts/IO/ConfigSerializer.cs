using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoBehaviour;
using Newtonsoft.Json;
using UnityEngine;


namespace IO
{
    public class ConfigSerializer
    {
        private static string generationConfigDir = Path.Combine("Assets", "Config", "mapGeneration");
        private static string layoutConfigDir = Path.Combine("Assets", "Config", "layouts");

        public static void ExportConfig(MapGenerationConfig generationConfig)
        {
            ValidateConfigDirs();
            var path = Path.Combine(generationConfigDir, generationConfig.configName + ".json");
            File.WriteAllText(path, JsonConvert.SerializeObject(generationConfig, Formatting.Indented));
        }

        public static void ExportConfig(MapLayoutConfig layoutConfig)
        {
            ValidateConfigDirs();
            var path = Path.Combine(layoutConfigDir, layoutConfig.layoutName + ".json");
            File.WriteAllText(path, JsonConvert.SerializeObject(layoutConfig, Formatting.Indented));
        }

        public static MapGenerationConfig ImportMapGenerationConfig(string configName)
        {
            string configText = File.ReadAllText(Path.Combine(generationConfigDir, configName + ".json"));
            MapGenerationConfig generationConfig = JsonConvert.DeserializeObject<MapGenerationConfig>(configText);
            return generationConfig;
        }

        public static MapLayoutConfig ImportLayoutConfig(string configName)
        {
            string configText = File.ReadAllText(Path.Combine(layoutConfigDir, configName + ".json"));
            MapLayoutConfig layoutConfig = JsonConvert.DeserializeObject<MapLayoutConfig>(configText);
            return layoutConfig;
        }

        public static List<string> GetMapGenerationConfigs()
        {
            ValidateConfigDirs();
            return Directory.GetFiles(generationConfigDir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
        }

        public static List<string> GetLayoutConfigs()
        {
            ValidateConfigDirs();
            return Directory.GetFiles(layoutConfigDir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
        }

        private static void ValidateConfigDirs()
        {
            if (!Directory.Exists(generationConfigDir))
                Directory.CreateDirectory(generationConfigDir);

            if (!Directory.Exists(layoutConfigDir))
                Directory.CreateDirectory(layoutConfigDir);
        }
    }
}