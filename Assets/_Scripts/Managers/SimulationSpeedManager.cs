using UnityEngine;
using UnityEngine.UI;

public class SimulationSpeedManager : MonoBehaviour
{

    private static SimulationSpeedManager Instance { get; set;} // do I need to get this anywhere?
    private ISimulation simulationManager;

    [SerializeField]
    private Button playButton, pauseButton, speed1Button, speed2Button, speed3Button;
    
    // Initializes the singleton instance of the SimulationManager and creates a new instance of SimulationManagerService.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            simulationManager = SimulationManagerService.GetInstance();
        }
        SetUpUiInput();
    }

    void Start()
    {
        
    }

    void Update()
    {
        HandleKeyboardInput();
        simulationManager.UpdateSimulationTime();
    }

    private void HandleKeyboardInput(){
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
            else
            {
                simulationManager.PauseSimulation();
            }
        }
        if(Input.GetKeyUp(KeyCode.Alpha1)){
            simulationManager.SetSimulationSpeed(1); //normal speed
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        }
        if(Input.GetKeyUp(KeyCode.Alpha2)){
            simulationManager.SetSimulationSpeed(2); //2x speed
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        }
        if(Input.GetKeyUp(KeyCode.Alpha3)){
            simulationManager.SetSimulationSpeed(5); //5x speed
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        }
    }
    private void SetUpUiInput(){
        playButton.onClick.AddListener(() =>
        {
            simulationManager.PlaySimulation();
        });

        pauseButton.onClick.AddListener(() =>
        {
            simulationManager.PauseSimulation();
        });

        speed1Button.onClick.AddListener(() =>
        {
            simulationManager.SetSimulationSpeed(1);
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        });

        speed2Button.onClick.AddListener(() =>
        {
            simulationManager.SetSimulationSpeed(2);
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        });

        speed3Button.onClick.AddListener(() =>
        {
            simulationManager.SetSimulationSpeed(5);
            if(simulationManager.IsPaused){
                simulationManager.PlaySimulation();
            }
        });
    }
}
