using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoBehaviour;
using Newtonsoft.Json;

namespace IO
{
    public class ConfigSerializer
    {
        private const string ConfigDir = @"Assets\Config";

        public static void ExportConfig(MapGenerationConfig generationConfig)
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText($@"{ConfigDir}\{generationConfig.configName}.json",
                JsonConvert.SerializeObject(generationConfig, Formatting.Indented));
        }

        public static MapGenerationConfig ImportConfig(string configName)
        {
            string configText = File.ReadAllText($@"{ConfigDir}\{configName}.json");
            MapGenerationConfig generationConfig = JsonConvert.DeserializeObject<MapGenerationConfig>(configText);
            return generationConfig;
        }

        public static List<string> GetConfigNames()
        {
            return Directory.GetFiles(ConfigDir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
        }
    }
}