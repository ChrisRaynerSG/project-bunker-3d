using UnityEngine;
using System;
using TMPro;


public class TimeManager : MonoBehaviour
{
    // reference to simulation manager
    private ISimulation simulationManager;

    // UI elements for displaying time
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI MonthText;
    public TextMeshProUGUI YearText;

    // buttons for changing time speed

    void Awake()
    {
        simulationManager = SimulationManagerService.GetInstance();
        Debug.Log("TimeManager initialized with simulation manager: " + simulationManager);
        Debug.Log("Simulation is paused: " + simulationManager.IsPaused);
        Debug.Log("Current time: " + simulationManager.GetCurrentTime());
    }

    void Start()
    {
        simulationManager.PauseSimulation();
    }

    void Update()
    {
        HandleInput();
        HandleTimeUpdate();
        HandleUiChanges();
    }

    void HandleTimeUpdate()
    {
        if(simulationManager.IsPaused) return;
        simulationManager.UpdateSimulationTime();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (simulationManager.IsPaused)
            {
                simulationManager.PlaySimulation();
            }
            else
            {
                simulationManager.PauseSimulation();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            simulationManager.SetSimulationSpeed(1); // Normal speed
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            simulationManager.SetSimulationSpeed(2); // Double speed
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            simulationManager.SetSimulationSpeed(4); // Quadruple speed
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            simulationManager.SetSimulationSpeed(10); // 10x speed
        }
    }

    void HandleUiChanges()
    {
        int hour = (int)simulationManager.GetCurrentTime() / 15; // 360 degrees / 24 hours = 15 degrees per hour
        int minute = (int)(simulationManager.GetCurrentTime() % 15 * (60f / 15f)); // or just *4.0f
        int month = simulationManager.GetCurrentMonth(); // integer between 0 and 11                                                
        string monthName = Enum.GetName(typeof(SimulationTimeModel.MonthType), month); // Get the name of the month from the enum

        TimeText.text = hour.ToString("00") + ":" + minute.ToString("00");
        DayText.text = (simulationManager.GetCurrentDay() +1 ).ToString(); // get the current day from the simulation manager + 1 because days start at 0
        MonthText.text = monthName; // display the name of the month
        // currently 4 days in a month 12 months in a year so 48 days in a year - may want to change this later depending on balance?
        YearText.text = simulationManager.GetCurrentYear().ToString();
    }
}