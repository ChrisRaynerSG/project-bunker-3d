public interface ISimulation
{
    void PauseSimulation();
    void PlaySimulation();
    void SetSimulationSpeed(int speed);
    void UpdateSimulationTime();
    bool IsPaused { get; }
}