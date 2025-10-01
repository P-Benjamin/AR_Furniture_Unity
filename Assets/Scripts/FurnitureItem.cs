using UnityEngine;

public class FurnitureItem : MonoBehaviour
{
    public string displayName = "Meuble";
    public MaterialCycler cycler;  
    public OrbitPinchTwist manip;        
    void Awake()
    {
        if (manip) manip.enabled = false;   
    }

    public void SetSelected(bool on)
    {
        if (manip) manip.enabled = on;     
    }
}