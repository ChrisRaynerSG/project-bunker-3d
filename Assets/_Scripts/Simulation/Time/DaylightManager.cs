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

        if (time >= 45f && time <= 330f)
        {
            float normalizedTime = (time - 90f) / (300f - 90f); // Normalize time to a 0-1 range for day
            float sunlightRotation = normalizedTime * 180f;
            SunDirectionLight.transform.rotation = Quaternion.Euler(sunlightRotation, 40, 0); // time is in degrees already
        }
        else
        {
            // Nighttime
            SunDirectionLight.transform.rotation = Quaternion.Euler(-90f, 40, 0); // time is in degrees already
        }
    }
}