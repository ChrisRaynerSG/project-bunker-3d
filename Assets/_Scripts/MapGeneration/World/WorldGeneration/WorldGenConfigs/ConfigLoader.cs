using System.Collections.Generic;
using UnityEngine;
using System.IO;

public abstract class ConfigLoader<TConfig, TCollection> where TConfig : class, new()
{
    protected const string MOD_FOLDER_NAME = "Mods"; // Folder where mod configs are stored
    protected const string CONFIG_FILE_EXTENSION = ".json"; // File extension for config files

    protected abstract string BaseConfigResourcePath { get; }

    protected abstract string ModConfigFilePattern { get; }

    protected abstract List<TConfig> ExtractConfigsFromCollection(TCollection collection);

    protected abstract string GetConfigId(TConfig config);

    public virtual List<TConfig> LoadAllConfigs()
    {
        var allConfigs = new List<TConfig>();

        var baseConfigs = LoadBaseGameConfigs();
        allConfigs.AddRange(baseConfigs);
        Debug.Log($"Loaded {baseConfigs.Count} base game {typeof(TConfig).Name} configurations");

        var modConfigs = LoadModdedConfigs();
        allConfigs.AddRange(modConfigs);
        Debug.Log($"Loaded {modConfigs.Count} mod {typeof(TConfig).Name} configurations");

        return RemoveDuplicates(allConfigs);
    }

    protected virtual List<TConfig> LoadBaseGameConfigs()
    {
        var configs = new List<TConfig>();

        TextAsset configAsset = Resources.Load<TextAsset>(BaseConfigResourcePath);
        if (configAsset != null)
        {
            try
            {
                var collection = JsonUtility.FromJson<TCollection>(configAsset.text);
                if (collection != null)
                {
                    var extractedConfigs = ExtractConfigsFromCollection(collection);
                    if (extractedConfigs != null)
                    {
                        configs.AddRange(extractedConfigs);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse base {typeof(TConfig).Name} config: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Base {typeof(TConfig).Name} config not found at Resources/{BaseConfigResourcePath}");
        }
        return configs;
    }

    protected virtual List<TConfig> LoadModdedConfigs()
    {
        var configs = new List<TConfig>();
        string modsPath = Path.Combine(Application.streamingAssetsPath, MOD_FOLDER_NAME);

        if (!Directory.Exists(modsPath))
        {
            Debug.Log($"Mods folder not found at: {modsPath}");
            return configs;
        }

        string[] modDirectories = Directory.GetDirectories(modsPath);

        foreach (string modDir in modDirectories)
        {
            var modConfigs = LoadModConfigs(modDir);
            configs.AddRange(modConfigs);
        }

        return configs;
    }

    protected virtual List<TConfig> LoadModConfigs(string modDirectory)
    {
        var configs = new List<TConfig>();
        string modName = Path.GetFileName(modDirectory);

        string[] configFiles = Directory.GetFiles(modDirectory, ModConfigFilePattern, SearchOption.AllDirectories);

        foreach (string configFile in configFiles)
        {
            try
            {
                string jsonContent = File.ReadAllText(configFile);
                var collection = JsonUtility.FromJson<TCollection>(jsonContent);

                if (collection != null)
                {
                    var extractedConfigs = ExtractConfigsFromCollection(collection);
                    if (extractedConfigs != null)
                    {
                        configs.AddRange(extractedConfigs);
                        Debug.Log($"Loaded {extractedConfigs.Count} {typeof(TConfig).Name} configs from {modName}/{Path.GetFileName(configFile)}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load {typeof(TConfig).Name} config from {configFile}: {e.Message}");
            }
        }

        return configs;
    }
    protected virtual List<TConfig> RemoveDuplicates(List<TConfig> configs)
    {
        var uniqueConfigs = new List<TConfig>();
        var seenIds = new HashSet<string>();
        
        foreach (var config in configs)
        {
            string configId = GetConfigId(config);
            if (!seenIds.Contains(configId))
            {
                uniqueConfigs.Add(config);
                seenIds.Add(configId);
            }
            else
            {
                Debug.LogWarning($"Duplicate {typeof(TConfig).Name} config found for '{configId}', skipping...");
            }
        }
        
        return uniqueConfigs;
    }
    
    protected virtual bool ValidateConfig(TConfig config)
    {
        return config != null;
    }
}