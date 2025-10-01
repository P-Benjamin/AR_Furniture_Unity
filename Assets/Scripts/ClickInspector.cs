using UnityEngine;

public class ClickInspector : MonoBehaviour
{
    public Camera arCamera;
    public LayerMask clickableLayers = ~0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryShowAt(Input.mousePosition);
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            var t = Input.GetTouch(0);
            TryShowAt(t.position);       
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
}
