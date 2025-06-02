using UnityEngine;

public class DaylightManager : MonoBehaviour
{
    public GameObject SunDirectionLight;
    //private SimulationTimeModel timeModel;
    private ISimulation simulationManager;

    void Start()
    {
        SunDirectionLight.transform.rotation = Quaternion.Euler(0, 0, 0);
        //timeModel = SimulationTimeModel.GetInstance();
        simulationManager = SimulationManagerService.GetInstance();
        SimulationTimeModel.OnTimeChanged += HandleSunMovement;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        // HandleSunMovement();
    }

    private void HandleSunMovement(float time)
    {
        if (simulationManager == null) return;

        float sunlightRotation = time - 90f;
        // Calculate the sun's rotation based on the time of day
        SunDirectionLight.transform.rotation = Quaternion.Euler(sunlightRotation, 40, 0); // time is in degrees already

    }
}