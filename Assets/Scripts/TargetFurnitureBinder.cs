using UnityEngine;
using Vuforia;
using System.Collections;

public class TargetVisibilityController : MonoBehaviour
{
    public GameObject contentRoot;      // parent du modèle à montrer/cacher
    public FurnitureItem furnitureItem; // ton script du modèle (gestes, etc.)
    public float hideDelay = 0.05f;     // petit délai pour éviter le clignotement

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
        bool shouldShow = s.Status == Status.TRACKED; // STRICT: seulement TRACKED

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
