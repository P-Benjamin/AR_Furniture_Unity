using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.PlayerSettings;

public class ClickInspector : MonoBehaviour
{
    public Camera arCamera;
    public LayerMask clickableLayers = ~0;

    void Update()
    {
        // Souris (éditeur)
        if (Input.GetMouseButtonDown(0))
        {
            TryShowAt(Input.mousePosition);
        }
        // Mobile (touch)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            var t = Input.GetTouch(0);
            TryShowAt(t.position); // TEMP: on ignore l'UI pour tester        
        }
        }

    void TryShowAt(Vector3 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayers))
        {
            var link = hit.collider.GetComponentInParent<ObjectInfoLink>();
            if (link) link.ShowInfo();
        }
    }

    bool IsOverUI(int pointerId)
    {
        if (EventSystem.current == null) return false;
        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
