using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitPinchTwist : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 0.25f;
    public float twistSpeed = 0.8f;
    public bool clampPitch = true;
    public float minPitch = -89f, maxPitch = 89f;

    float yaw, pitch, roll;

    // pinch/twist (incrémental, pas de borne)
    float prevPinchDist = 0f;
    float startTwistAngle = 0f, startRoll = 0f;

    void Awake()
    {
        var e = transform.rotation.eulerAngles;
        pitch = Normalize(e.x); yaw = Normalize(e.y); roll = Normalize(e.z);
    }

    void Update()
    {
#if UNITY_EDITOR
        MouseControls();
#endif
        TouchControls();
    }

    void TouchControls()
    {
        int tc = Input.touchCount;
        if (tc == 1)
        {
            var t = Input.GetTouch(0);
            if (IsOverUI(t.fingerId)) return;

            if (t.phase == TouchPhase.Moved)
            {
                yaw += -t.deltaPosition.x * rotateSpeed;
                pitch += t.deltaPosition.y * rotateSpeed;
                if (clampPitch) pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                ApplyRot();
            }
        }
        else if (tc >= 2)
        {
            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);
            if (IsOverUI(t0.fingerId) || IsOverUI(t1.fingerId)) return;

            float curDist = Vector2.Distance(t0.position, t1.position);

            // début du geste 2 doigts
            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began || prevPinchDist <= 0f)
            {
                prevPinchDist = Mathf.Max(0.0001f, curDist);
                startTwistAngle = Mathf.Atan2(t1.position.y - t0.position.y, t1.position.x - t0.position.x) * Mathf.Rad2Deg;
                startRoll = roll;
            }

            // PINCH : échelle incrémentale (pas de plafond)
            float factor = curDist / Mathf.Max(0.0001f, prevPinchDist);
            if (!float.IsNaN(factor) && !float.IsInfinity(factor))
            {
                float s = transform.localScale.x * factor;
                transform.localScale = new Vector3(s, s, s);
            }
            prevPinchDist = curDist;

            // TWIST : roll
            float curAngle = Mathf.Atan2(t1.position.y - t0.position.y, t1.position.x - t0.position.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(startTwistAngle, curAngle);
            roll = startRoll + deltaAngle * twistSpeed;

            if (clampPitch) pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            ApplyRot();
        }
        else
        {
            // pas de touche : reset l’historique pinch
            prevPinchDist = 0f;
        }
    }

    void MouseControls()
    {
        if (Input.GetMouseButton(0) && !IsOverUI(-1))
        {
            yaw += -Input.GetAxis("Mouse X") * 8f * rotateSpeed;
            pitch += Input.GetAxis("Mouse Y") * 8f * rotateSpeed;
            if (clampPitch) pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            ApplyRot();
        }
        // molette: pas de limite non plus
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            float s = transform.localScale.x * Mathf.Pow(1.1f, scroll); // facteur exponentiel doux
            transform.localScale = new Vector3(s, s, s);
        }
    }

    void ApplyRot() => transform.rotation = Quaternion.Euler(pitch, yaw, roll);

    static float Normalize(float a) { a %= 360f; if (a > 180f) a -= 360f; return a; }

    bool IsOverUI(int pointerId)
    {
        if (EventSystem.current == null) return false;
        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
