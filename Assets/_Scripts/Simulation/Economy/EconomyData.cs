using UnityEngine;

[System.Serializable]
public class EconomyData
{
    /// <summary>
    /// Represents the amount of money in the economy.
    /// </summary>
    private float money;

    /// <summary>
    /// Gets or sets the amount of money in the economy.
    /// </summary>
    /// <remarks>
    /// This property allows you to get or set the current amount of money.
    /// </remarks>
    public float Money
    {
        get => money;
        private set => money = value; // Ensure money does not go below zero
    }
    /// <summary>
    /// Represents the total expense in the economy.
    /// </summary>
    private float expense;

    /// <summary>
    /// Gets or sets the total expense in the economy.
    /// </summary>
    public float Expense
    {
        get => expense;
        private set => expense = value; // Ensure expense does not go below zero
    }

    /// <summary>
    /// Represents the total income in the economy.
    /// </summary>
    private float income;

    /// <summary>
    /// Gets or sets the total income in the economy.
    /// </summary>
    /// <remarks>
    /// This property allows you to get or set the current amount of income.
    /// </remarks>
    public float Income
    {
        get => income;
        private set => income = value; // Ensure income does not go below zero
    }

    /// <summary>
    /// Gets the change in money per tick.
    /// </summary>
    public float MoneyChangePerTick => Income - Expense;

    /// <summary>
    /// Event that is triggered when the money amount changes.
    /// </summary>
    /// <remarks>
    /// This event can be used to update UI elements or trigger other actions in the game.
    /// </remarks>
    public static event System.Action<float> OnMoneyChanged;

    /// <summary>
    /// Singleton instance of EconomyData.
    /// </summary>
    private static EconomyData _instance;
    /// <summary>
    /// Gets the singleton instance of EconomyData.
    /// </summary>
    public static EconomyData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EconomyData();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Private constructor to prevent instantiation from outside.
    /// This ensures that EconomyData is a singleton.
    /// </summary>
    private EconomyData()
    {
        Money = 1000f; // Initial amount of money
    }
    
    /// <summary>
    /// Adds money to the economy.
    /// </summary>
    /// <param name="amount">The amount of money to add.</param>
    /// <remarks>
    /// This method adds the specified amount of money to the economy.
    /// If the amount is negative, it does nothing.
    /// </remarks>
    public bool AddMoney(float amount)
    {
        if (amount < 0) return false; // Prevent adding negative money
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
        return true; // Return true to indicate success
    }

    /// <summary>
    /// Removes money from the economy.
    /// </summary>
    /// <param name="amount">The amount of money to remove.</param>
    /// <remarks>
    /// This method removes the specified amount of money from the economy.
    /// If the amount is negative, it does nothing.
    /// </remarks>
    public bool RemoveMoney(float amount)
    {
        if (amount < 0) return false; // Prevent removing negative money
        if (Money < amount)
        {
            Debug.LogWarning("Not enough money to remove.");
            return false; // Prevent removing more money than available
        }
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        return true;
    }
}