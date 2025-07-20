
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// NameData holds the version and categories of names.
/// It contains arrays of first and last names for each gender.
/// It can be extended to include more categories or formats in the future.
/// </summary>
/// <remarks>
/// This class is used to store names for each category.
/// It can be loaded from a JSON file and accessed statically.
/// 
/// The JSON file should be structured to match the NameData class.
/// </remarks>
[Serializable]
public class NameData
{
    public string version;
    public Dictionary<string, CategoryData> nameCategories;
    public string[] nameFormats;
    public string[] nicknames;
    public string[] jobs;

}

/// <summary>
/// CategoryData holds the names for each category.
/// It contains arrays of first and last names for each gender.
/// It can be extended to include more categories or formats in the future.
/// </summary>
/// <remarks>
/// This class is used to store names for each category.
/// </remarks>

[Serializable]
public class CategoryData
{
    public GenderNames male;
    public GenderNames female;
    public GenderNames neutral;
}

/// <summary>
/// GenderNames holds arrays of first and last names for a specific gender.
/// </summary>
/// <remarks>
/// This class is used to store names for male, female, and neutral categories.
/// It can be extended to include more categories or formats in the future.
/// </remarks>
/// <example>
/// var maleNames = new GenderNames { first = new[] { "John", "Alex" }, last = new[] { "Smith", "Doe" } };
/// </example>
[Serializable]
public class GenderNames
{
    public string[] first;
    public string[] last;

}

/// <summary>
/// NameDatabase is responsible for loading and providing access to the names used in the game.
/// It loads names from a JSON file located in the StreamingAssets folder.
/// The names can be used for generating random dweller names, nicknames, and jobs.
/// </summary>
/// <remarks>
/// The database is loaded once and can be accessed statically.
/// It currently supports male, female, and neutral names.
/// It can be extended to include more categories or formats in the future.
/// </remarks>
public static class NameDatabase
{

    private static NameData _nameData;
    private static bool _isLoaded = false;

    public static void LoadNameDatabase()
    {
        if (_isLoaded) return;

        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Data/DwellerNames.json");

            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                _nameData = JsonUtility.FromJson<NameData>(content);
                _isLoaded = true;
                Debug.Log($"Loaded name database version: {_nameData.version}");
                LogNameStats();
            }
            else
            {
                Debug.LogError($"Name database file not found at: {filePath}");
                CreateFallbackDatabase();
            }
        }

        catch (Exception ex)
        {
            Debug.LogError($"Failed to load name database: {ex.Message}");
            CreateFallbackDatabase();
        }
    }

    public static string GenerateRandomName(string category = "human", bool? isMale = null, string format = null)
    {
        if (!_isLoaded) LoadNameDatabase();

        GenderNames genderData;
        if (isMale == null)
        {
            if (_nameData.nameCategories[category].neutral?.first?.Length > 0)
            {
                genderData = _nameData.nameCategories[category].neutral;
            }
            else
            {
                Debug.LogWarning($"No neutral names found for category: {category}");
                return null;
            }
        }
    }

    private static void LogNameStats()
    {
        foreach (var category in _nameData.nameCategories)
        {
            int maleCount = category.Value.male?.first?.Length ?? 0;
            int femaleCount = category.Value.female?.first?.Length ?? 0;
            int neutralCount = category.Value.neutral?.first?.Length ?? 0;

            Debug.Log($"Category: {category.Key}, Male Names: {maleCount}, Female Names: {femaleCount}, Neutral Names: {neutralCount}");
        }
    }

    private static void CreateFallbackDatabase()
    {
        _nameData = new NameData
        {
            version = "fallback",
            nameCategories = new Dictionary<string, CategoryData>
            {
                ["human"] = new CategoryData
                {
                    male = new GenderNames
                    {
                        first = new[] { "John", "Alex", "Chris", "David", "Michael" },
                        last = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones" }
                    },
                    female = new GenderNames
                    {
                        first = new[] { "Jane", "Emily", "Katie", "Sarah", "Lisa" },
                        last = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones" }
                    }
                }
            },
            nameFormats = new[] { "{first} {last}" },
            nicknames = new[] { "Ace", "Chief", "Doc" },
            jobs = new[] { "Builder", "Miner", "Trader" }
        };
        _isLoaded = true;
    }

}