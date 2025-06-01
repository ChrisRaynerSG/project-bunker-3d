public interface ISimulation
{
    // Simulation time control methods
    void PauseSimulation();
    void PlaySimulation();
    void SetSimulationSpeed(int speed);
    void UpdateSimulationTime();
    bool IsPaused { get; }

    // Save and Load methods
    void LoadTimeFromSave(string saveData);

    //Accessor methods for simulation speed
    int GetSimulationSpeed();

    // Accessor methods for current time, day, month, year
    float GetCurrentTime();
    int GetCurrentDay();
    int GetCurrentMonth();
    int GetCurrentYear();

}