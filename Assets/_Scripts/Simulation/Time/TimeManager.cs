using UnityEngine;
using System;
using NUnit.Framework;

public class TimeManager : MonoBehaviour
{
    private ISimulation simulationManager;

    void Awake()
    {
        simulationManager = SimulationManagerService.GetInstance();
    }

    void Start()
    {
        simulationManager.PauseSimulation();
    }

    void Update()
    {
        HandleInput();
        HandleTimeUpdate();
    }

    void HandleTimeUpdate()
    {
        if (simulationManager.IsPaused) return;
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
    }

}