using System;

[Serializable]
public class SimulationTimeModel
{
    private float time;
    public float Time => time;
    private int day;
    public int Day => day;
    private int month;
    public int Month => month;
    private int year;
    public int Year => year;
    public int daysInMonth = 4;
    public enum MonthType
    {
        January = 0,
        February = 1,
        March = 2,
        April = 3,
        May = 4,
        June = 5,
        July = 6,
        August = 7,
        September = 8,
        October = 9,
        November = 10,
        December = 11
    }

    private int simulationSpeed;
    public int SimulationSpeed => simulationSpeed;

    public static event Action<float> OnTimeChanged;
    public static event Action<int> OnDayChanged;
    public static event Action<int> OnMonthChanged;
    public static event Action<int> OnYearChanged;
    public static event Action<int> OnSimulationSpeedChanged;

    private static SimulationTimeModel _instance;
    public static SimulationTimeModel Instance{
        get
        {
            if (_instance == null)
            {
                _instance = new SimulationTimeModel();
            }
            return _instance;
        }
    }

    
    private SimulationTimeModel()
    {
        time = 90; // Initialize to 90 degrees (dawn)
        day = 0;
        month = 0;
        year = System.DateTime.Now.Year; // Initialize to current year
    }


    private void RolloverDay()
    {
        day++;
        time = 0;
        if (day > daysInMonth)
        {
            RolloverMonth();
            day = 0;
        }
        OnDayChanged?.Invoke(day);
    }
    private void RolloverMonth()
    {
        month++;
        if (month > 12)
        {
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

    public void IncrementTime()
    {
        time += simulationSpeed * 0.005f;
        OnTimeChanged?.Invoke(time);
        if (time >= 360)
        {
            RolloverDay();
        }
    }
    public void SetSimulationSpeed(int speed)
    {
        simulationSpeed = speed;
        OnSimulationSpeedChanged?.Invoke(simulationSpeed);
    }

    public void SetTimeAndDate(int newTime, int newDay, int newMonth, int newYear)
    {
        time = newTime;
        day = newDay;
        month = newMonth;
        year = newYear;

        OnTimeChanged?.Invoke(time);
        OnDayChanged?.Invoke(day);
        OnMonthChanged?.Invoke(month);
        OnYearChanged?.Invoke(year);
    }
}