using UnityEngine;
[ExecuteAlways]
public class SafeArea : MonoBehaviour
{
    Rect last; RectTransform rt;
    void OnEnable() { rt = GetComponent<RectTransform>(); Apply(); }
    void Update() { Apply(); }
    void Apply()
    {
        var sa = Screen.safeArea; if (sa == last || rt == null) return; last = sa;
        var min = sa.position; var max = sa.position + sa.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min; rt.anchorMax = max;
    }
}