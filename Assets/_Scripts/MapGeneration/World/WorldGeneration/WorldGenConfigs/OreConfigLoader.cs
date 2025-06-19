using System.Collections.Generic;
using UnityEngine;

public static class OreConfigLoader
{
    private static OreConfigLoaderImpl _instance;

    public static List<OreConfig> LoadAllOreConfigs()
    {
        if (_instance == null)
        {
            _instance = new OreConfigLoaderImpl();
        }

        return _instance.LoadAllConfigs();
    }

    private class OreConfigLoaderImpl : ConfigLoader<OreConfig, OreConfigCollection>
    {
         protected override string BaseConfigResourcePath => "Data/OreConfigs";
        protected override string ModConfigFilePattern => "*ore*.json";
        
        protected override List<OreConfig> ExtractConfigsFromCollection(OreConfigCollection collection)
        {
            return collection?.ores;
        }
        
        protected override string GetConfigId(OreConfig config)
        {
            return config?.oreBlockId ?? string.Empty;
        }
        
        protected override bool ValidateConfig(OreConfig config)
        {
            if (!base.ValidateConfig(config))
                return false;
            
            if (string.IsNullOrEmpty(config.oreBlockId))
            {
                Debug.LogWarning("OreConfig has empty oreBlockId, skipping...");
                return false;
            }
            
            if (config.threshold < 0f || config.threshold > 1f)
            {
                Debug.LogWarning($"OreConfig '{config.oreBlockId}' has invalid threshold {config.threshold}, clamping to 0-1 range");
                config.threshold = Mathf.Clamp01(config.threshold);
            }
            
            return true;
        }
        
        protected override List<OreConfig> RemoveDuplicates(List<OreConfig> configs)
        {
            // Apply validation during duplicate removal
            var validConfigs = new List<OreConfig>();
            
            foreach (var config in configs)
            {
                if (ValidateConfig(config))
                {
                    validConfigs.Add(config);
                }
            }
            return base.RemoveDuplicates(validConfigs);
        }
    }
}