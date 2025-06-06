using UnityEngine;
public class Dweller : MonoBehaviour
{
    private DwellerData dwellerData;

    // not sure we need a reference to HungerManager and other Managers here, we add them as components to the Dweller GameObject?
    void Awake()
    {
        // Initialize DwellerData and HungerManager
        dwellerData = new DwellerData();
        // Set up the Dweller's initial state
    }
}