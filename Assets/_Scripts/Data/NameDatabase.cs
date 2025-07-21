
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

    /// <summary>
    /// Generates a random name based on the specified category and gender.
    /// </summary>
    /// <param name="category">The name category (e.g., "human").</param>
    /// <param name="isMale">Indicates if the generated name should be male or female.</param>
    /// <returns>A tuple containing the first and last name.</returns>
    /// <remarks>
    /// This method currently uses a simple random selection from predefined lists.
    /// </remarks>
    /// <example>
    /// var (firstName, lastName) = NameDatabase.GenerateRandomName("human", true);
    /// Debug.Log($"Generated name: {firstName} {lastName}");
    /// </example>

    public static (string firstName, string lastName) GenerateRandomName(string category = "human", bool? isMale = null)
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
                genderData = UnityEngine.Random.value > 0.5f
                ? _nameData.nameCategories[category].male
                : _nameData.nameCategories[category].female;
            }
        }
        else
        {
            genderData = isMale.Value
            ? _nameData.nameCategories[category].male
            : _nameData.nameCategories[category].female;
        }

        string firstName = GetRandomFromArray(genderData.first);
        string lastName = GetRandomFromArray(genderData.last);

        return (firstName, lastName);
    }


    /// <summary>
    /// Formats a name based on the specified format string.
    /// </summary>
    /// <param name="format">The format string containing placeholders for first, last, nickname, and job.</param>
    /// <param name="firstName">The first name to insert into the format.</param>
    /// <param name="lastName">The last name to insert into the format.</param>
    /// <returns>The formatted name string.</returns>
    /// <remarks>
    /// This method currently uses a simple string replacement for formatting.
    /// </remarks>
    /// <example>
    /// var formattedName = NameDatabase.FormatName("{first} {last}", "John", "Doe");
    /// Debug.Log($"Formatted name: {formattedName}");
    /// </example>
    private static string FormatName(string format, string firstName, string lastName)
    {
        string result = format;
        result = result.Replace("{first}", firstName);
        result = result.Replace("{last}", lastName);

        if (result.Contains("{nickname}"))
        {
            string nickname = GetRandomFromArray(_nameData.nicknames);
            result = result.Replace("{nickname}", nickname);
        }

        if (result.Contains("{job}"))
        {
            string job = GetRandomFromArray(_nameData.jobs);
            result = result.Replace("{job}", job);
        }
        return result;
    }


    /// <summary>
    /// Gets a random name from the specified array.
    /// </summary>
    /// <param name="array">The array of names to choose from.</param>
    /// <returns>A random name from the array.</returns>
    /// <remarks>
    /// This method currently uses Unity's Random.Range to select a random index.
    /// </remarks>
    /// <example>
    /// var randomName = NameDatabase.GetRandomFromArray(new[] { "Alice", "Bob", "Charlie" });
    /// Debug.Log($"Random name: {randomName}");
    /// </example>
    private static string GetRandomFromArray(string[] array)
    {
        if (array == null || array.Length == 0) return "Unknown";
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
    /// <summary>
    /// Logs the statistics of names in each category.
    /// </summary>
    /// <remarks>
    /// This method iterates through each category and logs the name counts.
    /// </remarks>
    /// <example>
    /// NameDatabase.LogNameStats();
    /// </example>
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
    
    /// <summary>
    /// Creates a fallback name database with default names.
    /// This is used when the main database fails to load.
    /// </summary>
    /// <remarks>
    /// This method initializes a simple set of names to ensure the game can still function.
    /// It can be extended with more names or categories as needed.
    /// </remarks>
    /// <example>
    /// NameDatabase.CreateFallbackDatabase();
    /// </example>
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