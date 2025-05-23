using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{

    public string fileName;
    public int autosaveIndex = 0;
    
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void SetFileName(String saveName)
    {
        // make the save name unique? date/time/savename?
        // Do Like Rimworld does and have the name of the colony/vault be the save name passed in, and then differentiate the saves by date/time?

        fileName = Path.Combine("saves", $"{saveName}_UndergroundSave_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");
    }
    
    public void SaveGameState(GameState gameState, bool autosave = false)
    {
        string data = JsonUtility.ToJson(gameState);
        SaveGame(data, autosave);
    }

    public GameState LoadGameState(string fileName)
    {
        string data = LoadGame(fileName);
        GameState gameState = JsonUtility.FromJson<GameState>(data);
        return gameState;
    }

    private void SaveGame(string data, bool autosave = false)
    {
        string path;

        if (autosave)
        {
            path = Path.Combine("saves", $"Autosave_{autosaveIndex + 1}.json");
            autosaveIndex = (autosaveIndex + 1) % 5;
        }
        else
        {
            path = fileName;
        }

        if (WriteToFile(path, data) == true)
        {
            Debug.Log("Game saved successfully.");
        }
        else
        {
            Debug.Log("Failed to save game.");
        }
    }

    private string LoadGame(string fileName)
    {
        string data = "";
        if (ReadFromFile(out string dataFromFile, fileName) == true)
        {
            Debug.Log("Game loaded successfully.");
            data = dataFromFile;
        }
        return data;
    }

    private bool WriteToFile(string fileName, string data)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(fullPath, data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
            return false;
        }
    }
    private bool ReadFromFile(out string data, string fileName){
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            data = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from file: {e.Message}");
            data = null;
            return false;
        }
    }
}
