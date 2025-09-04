using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InfoPanelController : MonoBehaviour
{
    public static InfoPanelController I;

    [Header("Réfs UI")]
    public GameObject root;            // Panel parent (désactivé par défaut)
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text dimensionsText;
    public TMP_Text priceText;

    [ContextMenu("DEBUG ▸ Validate UI wiring")]
    void ValidateUI()
    {
        Debug.Log($"[InfoPanel] root={(root ? root.name : "NULL")}, " +
                  $"title={(titleText ? titleText.name : "NULL")}, desc={(descriptionText ? descriptionText.name : "NULL")}, " +
                  $"dim={(dimensionsText ? dimensionsText.name : "NULL")}, " +
                  $"price={(priceText ? priceText.name : "NULL")}");
    }

    void Awake()
    {
        I = this;
        Debug.Log($"[InfoPanel] Awake on {gameObject.name} (instanceID={GetInstanceID()})");
        if (root) root.SetActive(false);
    }

    public void Show(ObjectInfo info)
    {
        Debug.Log($"[InfoPanel] Show on {gameObject.name} (instanceID={GetInstanceID()}) with: {(info ? info.title : "NULL")}");


        if (!info || !root) return;

        root.SetActive(true);


        Debug.Log($"[InfoPanel] Show: {(info ? info.title : "NULL")} | " +
         $"TitleRef={(titleText ? "OK" : "NULL")} ");

        if (titleText) titleText.text = info.title ?? "";
        if (descriptionText) descriptionText.text = info.description ?? "";
        if (dimensionsText) dimensionsText.text = string.IsNullOrEmpty(info.dimensions) ? "" : $"Dimensions : {info.dimensions}";
        if (priceText) priceText.text = string.IsNullOrEmpty(info.price) ? "" : $"Prix : {info.price}";

        Canvas.ForceUpdateCanvases();
        var rt = root.GetComponent<RectTransform>();
        if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }
}
