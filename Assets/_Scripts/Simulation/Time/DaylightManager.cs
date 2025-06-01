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
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        HandleSunMovement();
    }

    private void HandleSunMovement()
    {

    }

}