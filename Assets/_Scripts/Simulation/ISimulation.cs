public interface ISimulation
{
    void PauseSimulation();
    void PlaySimulation();
    void SetSimulationSpeed(int speed);
    void UpdateSimulationTime();
    bool IsPaused { get; }

    void LoadTimeFromSave(string saveData);
    // Need methods to get current time, day, month, year?

}