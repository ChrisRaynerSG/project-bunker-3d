public interface ISimulation
{
    void PauseSimulation();
    void PlaySimulation();
    void SetSimulationSpeed(int speed);
    void UpdateSimulationTime();
    bool IsPaused { get; }

    // Need methods to get current time, day, month, year?

}