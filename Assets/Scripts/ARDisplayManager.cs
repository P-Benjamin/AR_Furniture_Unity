using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ARDisplayManager : MonoBehaviour
{
    public static ARDisplayManager Instance;

    [Header("UI (optionnel)")]
    public Button btnColor;
    public Button btnScreenshot;
    public TMPro.TextMeshProUGUI label;   // nom du modèle courant

    private FurnitureItem current;
    private readonly List<FurnitureItem> visibles = new List<FurnitureItem>(); // tags visibles

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    // Appelé par TargetFurnitureBinder quand un tag devient visible
    public void OnTargetVisible(FurnitureItem item)
    {
        if (item == null) return;
        if (!visibles.Contains(item)) visibles.Add(item);

        SetCurrent(item);        // "dernier vu" prioritaire
    }

    // Appelé quand le tag n'est plus suivi
    public void OnTargetHidden(FurnitureItem item)
    {
        if (item == null) return;
        visibles.Remove(item);

        if (current == item)
        {
            current.SetSelected(false);
            current = null;
            // Fallback: s'il reste d'autres tags visibles, on prend le dernier
            if (visibles.Count > 0) SetCurrent(visibles[visibles.Count - 1]);
        }
        UpdateUI();
    }

    void SetCurrent(FurnitureItem item)
    {
        if (current == item) return;

        if (current) current.SetSelected(false);
        current = item;
        if (current) current.SetSelected(true);

        UpdateUI();
    }

    void UpdateUI()
    {
        bool on = current != null;
        if (btnColor) btnColor.interactable = on;
        if (btnScreenshot) btnScreenshot.interactable = true; // screenshot toujours OK
        if (label) label.text = on ? current.displayName : "—";
    }

    // ----- Boutons UI -----
    public void ChangeColor()
    {
        if (current && current.cycler) current.cycler.Cycle(); // -> change la couleur + paillettes UNIQUEMENT sur le modèle courant
    }

    public void TakeScreenshot()
    {
        string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        ScreenCapture.CaptureScreenshot($"AR_{ts}.png");
    }
}
