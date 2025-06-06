using UnityEngine;
using System.Collections.Generic;

public class DwellerManger : MonoBehaviour
{
    private List<Dweller> dwellers = new();
    public void AddDweller(Dweller dweller)
    {

    }

    public void RemoveDweller(string dwellerId)
    {

    }
    public Dweller GetDweller(string dwellerId)
    {
        return null;
    }

    public List<Dweller> GetAllDwellers()
    {
        return dwellers;
    }
    public void CreateDweller(string dwellerId, string name, Vector3 position)
    {
        Dweller newDweller = new Dweller();
        AddDweller(newDweller);
    }

    public void UpdateDweller()
    {
        //probably dont need this but might want to have it? update dweller logic within dwellers themselves? not sure the best way to handle this
        
    }
}