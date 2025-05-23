using System;
using UnityEngine;
public class SimulationManagerService : ISimulation

{
    public static SimulationManagerService Instance { get; private set; } // Singleton instance
    private bool isPaused;
    public bool IsPaused => isPaused;

    private SimulationTimeModel timeModel;
    public static event Action<bool> OnSimulationPaused;

    private SimulationManagerService(){
        timeModel = SimulationTimeModel.GetInstance();
        timeModel.SetSimulationSpeed(0); // Initialize simulation speed to 0 (paused)
        Time.timeScale = 0; // Set Unity's time scale to 0 (paused)
        isPaused = false;
    }
    public static SimulationManagerService GetInstance()
    {
        if (Instance == null)
        {
            Instance = new SimulationManagerService();
        }
        return Instance;
    }

    public void PauseSimulation()
    {
        isPaused = true;
        Time.timeScale = 0;
        OnSimulationPaused?.Invoke(isPaused);
    }

    public void PlaySimulation()
    {
        if(timeModel.SimulationSpeed == 0)
        {
            timeModel.SetSimulationSpeed(1); // Set to normal speed
        }

        isPaused = false;
        Time.timeScale = timeModel.SimulationSpeed;
        OnSimulationPaused?.Invoke(isPaused);
    }

    public void SetSimulationSpeed(int speed)
    {
        timeModel.SetSimulationSpeed(speed);
        Time.timeScale = timeModel.SimulationSpeed;
    }

    public void UpdateSimulationTime()
    {
        if (!isPaused)
        {
            timeModel.IncrementTime();
        }
    }
}