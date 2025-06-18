using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the EconomyManager.
    /// </summary>
    public static EconomyManager Instance { get; private set; }

    private EconomyData economyData;

    /// <summary>
    /// Initializes the EconomyManager instance.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            economyData = EconomyData.Instance;
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        economyData.AddMoney(amount);
    }

    public void RemoveMoney(int amount)
    {
        if (economyData.Money >= amount) economyData.RemoveMoney(amount);
        else Debug.LogWarning("Not enough money to remove.");
    }

    // Add methods and properties for managing the economy here.
}