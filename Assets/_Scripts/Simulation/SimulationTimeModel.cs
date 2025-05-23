using System;

public class SimulationTimeModel 
{
    private int time;
    public int Time => time;
    private int day;
    public int Day => day;
    private int month;
    public int Month => month;
    private int year;
    public int Year => year;
    public int daysInMonth =  4;

    private int simulationSpeed;
    public int SimulationSpeed => simulationSpeed;

    public static event Action<int> OnTimeChanged;
    public static event Action<int> OnDayChanged;
    public static event Action<int> OnMonthChanged;
    public static event Action<int> OnYearChanged;
    public static event Action<int> OnSimulationSpeedChanged;

    public static SimulationTimeModel Instance { get; private set;}

    private SimulationTimeModel(){
        time = 0;
        day = 0;
        month = 0;
    }

    public static SimulationTimeModel GetInstance(){
        if(Instance == null){
            Instance = new SimulationTimeModel();
        }
        return Instance;
    }

    private void RolloverDay(){
        day++;
        time = 0;
        if(day >= daysInMonth){
            RolloverMonth();
            day = 0;
        }
        OnDayChanged?.Invoke(day);
    }
    private void RolloverMonth()
    {
        month++;
        if(month >= 12){
            RolloverYear();
            month = 0;
        }
        OnMonthChanged?.Invoke(month);
    }

    private void RolloverYear()
    {
        year++;
        OnYearChanged?.Invoke(year);
    }

    public void IncrementTime(){
        time += simulationSpeed;
        OnTimeChanged?.Invoke(time);
        if(time >= 360){
            RolloverDay();
        }
    }
    public void SetSimulationSpeed(int speed){
        simulationSpeed = speed;
        OnSimulationSpeedChanged?.Invoke(simulationSpeed);
    }
}