using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.05f, pressScale = 0.95f, speed = 12f;
    Vector3 target = Vector3.one;
    void Update() { transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * speed); }
    public void OnPointerEnter(PointerEventData e) { target = Vector3.one * hoverScale; }
    public void OnPointerExit(PointerEventData e) { target = Vector3.one; }
    public void OnPointerDown(PointerEventData e) { target = Vector3.one * pressScale; }
    public void OnPointerUp(PointerEventData e) { target = Vector3.one * hoverScale; }
}
