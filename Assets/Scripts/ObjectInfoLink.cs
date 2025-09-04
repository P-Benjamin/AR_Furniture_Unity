using UnityEngine;

public class ObjectInfoLink : MonoBehaviour
{
    public ObjectInfo info; // Glisse ici l’asset ScriptableObject

    [ContextMenu("TEST ▸ Show Info")]
    void TestShow() { ShowInfo(); }

    // Exemple d’usage avec ton panneau d’info :
    public void ShowInfo()
    {
        if (info) InfoPanelController.I?.Show(info);
    }
}