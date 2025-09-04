using UnityEngine;

public class FurnitureItem : MonoBehaviour
{
    public string displayName = "Meuble";
    public MaterialCycler cycler;        // ton cycler avec paillettes
    public OrbitPinchTwist manip;        // rotation/pinch
    void Awake()
    {
        if (manip) manip.enabled = false;   // activ� seulement si "courant"
    }

    public void SetSelected(bool on)
    {
        if (manip) manip.enabled = on;      // seul l'objet s�lectionn� re�oit les gestes
    }
}