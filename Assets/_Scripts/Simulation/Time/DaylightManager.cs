using System.Collections;
using UnityEngine;

public class DaylightManager : MonoBehaviour
{
    public GameObject SunDirectionLight;
    //private SimulationTimeModel timeModel;
    private ISimulation simulationManager;

    private bool isDimming = false;
    private bool isBrightening = false;

    void Start()
    {
        SunDirectionLight.transform.rotation = Quaternion.Euler(0, GetMonthRotation(), 0);
        simulationManager = SimulationManagerService.GetInstance();
        SimulationTimeModel.OnTimeChanged += HandleSunMovement;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
    }

    private void HandleSunMovement(float time)
    {


        if (simulationManager == null) return;

        if (time >= 45f && time <= 330f)
        {
            if (!isBrightening)
            {
                StartCoroutine(BrightenSunlight()); // Brighten the sunlight when in daytime
            }
            
            float normalizedTime = (time - 90f) / (300f - 90f); // Normalize time to a 0-1 range for day
            float sunlightRotation = normalizedTime * 180f;
            SunDirectionLight.transform.rotation = Quaternion.Euler(sunlightRotation, GetMonthRotation(), 0); // time is in degrees already

        }
        else
        {
            if (!isDimming)
            {
                StartCoroutine(DimSunlight()); // Dim the sunlight when not in daytime
            }
            SunDirectionLight.transform.rotation = Quaternion.Euler(-90f, GetMonthRotation(), 0); // time is in degrees already
        }
    }

    private float GetMonthRotation()
    {
        // may want to update this later to determine day length based on the month as well as the rotation of the sun
        int month = SimulationTimeModel.Instance.Month; // get the current month
        float monthRotation;
        switch (month)
        {
            case 0: // January
                monthRotation = 85f;
                break;
            case 1: // February
                monthRotation = 75f;
                break;
            case 2: // March
                monthRotation = 65f;
                break;
            case 3: // April
                monthRotation = 55f;
                break;
            case 4: // May
                monthRotation = 45f;
                break;
            case 5: // June
                monthRotation = 40f;
                break;
            case 6: // July
                monthRotation = 45f;
                break;
            case 7: // August
                monthRotation = 55f;
                break;
            case 8: // September
                monthRotation = 65f;
                break;
            case 9: // October
                monthRotation = 75f;
                break;
            case 10: // November
                monthRotation = 85f;
                break;
            case 11: // December
                monthRotation = 90f;
                break;
            default:
                monthRotation = 40f; // Default rotation for unknown months
                break;
        }
        return monthRotation; // Return the calculated rotation for the current month
    }

    private IEnumerator DimSunlight()
    {
        if (isDimming) yield break; // Prevent multiple coroutines from running simultaneously

        isDimming = true; // Set the flag to prevent re-entry

        float targetIntensity = 0f;
        float currentIntensity = SunDirectionLight.GetComponent<Light>().intensity;
        float duration = 1f; // Duration to dim the sunlight
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            SunDirectionLight.GetComponent<Light>().intensity = Mathf.Lerp(currentIntensity, targetIntensity, t);
            yield return null;
        }

        isDimming = false; // Reset the flag after dimming
    }

    private IEnumerator BrightenSunlight()
    {
        if (isBrightening) yield break; // Prevent multiple coroutines from running simultaneously

        isBrightening = true; // Set the flag to prevent re-entry

        float targetIntensity = 2f; // Assuming the light's intensity is 1 when fully bright
        float currentIntensity = SunDirectionLight.GetComponent<Light>().intensity;
        float duration = 1f; // Duration to brighten the sunlight
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            SunDirectionLight.GetComponent<Light>().intensity = Mathf.Lerp(currentIntensity, targetIntensity, t);
            yield return null;
        }

        isBrightening = false; // Reset the flag after brightening
    }
}