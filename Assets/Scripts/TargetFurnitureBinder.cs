using UnityEngine;
using Vuforia;
using System.Collections;

public class TargetVisibilityController : MonoBehaviour
{
    public GameObject contentRoot;      
    public FurnitureItem furnitureItem; 
    public float hideDelay = 0.05f;     

    ObserverBehaviour obs;
    Coroutine hideCo;

    void Awake()
    {
        obs = GetComponent<ObserverBehaviour>();
        if (obs) obs.OnTargetStatusChanged += OnStatusChanged;
        if (contentRoot) contentRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (obs) obs.OnTargetStatusChanged -= OnStatusChanged;
    }

    void OnStatusChanged(ObserverBehaviour b, TargetStatus s)
    {
        bool shouldShow = s.Status == Status.TRACKED; 

        if (shouldShow)
        {
            if (hideCo != null) { StopCoroutine(hideCo); hideCo = null; }
            if (contentRoot && !contentRoot.activeSelf) contentRoot.SetActive(true);
            ARDisplayManager.Instance?.OnTargetVisible(furnitureItem);
        }
        else
        {
            if (hideCo != null) StopCoroutine(hideCo);
            hideCo = StartCoroutine(HideAfterDelay());
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (contentRoot && contentRoot.activeSelf) contentRoot.SetActive(false);
        ARDisplayManager.Instance?.OnTargetHidden(furnitureItem);
        hideCo = null;
    }
}
