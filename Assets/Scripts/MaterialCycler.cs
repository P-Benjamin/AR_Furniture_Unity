using UnityEngine;

public class MaterialCycler : MonoBehaviour
{
    public AudioClip sound;
    public AudioSource audioSrc;

    public Renderer[] targets;

    public Material[] variants;

    public bool autoFindInChildren = true;
    public bool instancePerObject = true; 
    public int materialSlot = -1;        
    public bool verboseLogs = true;

    public ParticleSystem glitterPrefab;
    public bool glitterPerRenderer = false;
    public Vector3 glitterOffset = Vector3.zero;
    public Transform glitterParent;
    public float glitterAutoDestroy = 2f;

    int i = -1;

    void Start()
    {
        if ((targets == null || targets.Length == 0) && autoFindInChildren)
            targets = GetComponentsInChildren<Renderer>(true);

        if (verboseLogs)
            Debug.Log($"[MaterialCycler] Init. targets={targets?.Length ?? 0}, variants={variants?.Length ?? 0}");
    }

    public void Cycle()
    {
        if (variants == null || variants.Length == 0) { if (verboseLogs) Debug.LogWarning("[MaterialCycler] Pas de variants"); return; }
        if (targets == null || targets.Length == 0) { if (verboseLogs) Debug.LogWarning("[MaterialCycler] Pas de targets"); return; }

        i = (i + 1) % variants.Length;
        var nextMat = variants[i];

        foreach (var r in targets)
        {
            if (!r) continue;

            if (materialSlot < 0)
            {
                if (r.sharedMaterials != null && r.sharedMaterials.Length > 1)
                {
                    var mats = instancePerObject ? r.materials : r.sharedMaterials;
                    for (int k = 0; k < mats.Length; k++) mats[k] = nextMat;
                    if (instancePerObject) r.materials = mats; else r.sharedMaterials = mats;
                }
                else
                {
                    if (instancePerObject) r.material = nextMat; else r.sharedMaterial = nextMat;
                }
            }
            else
            {
                var mats = instancePerObject ? r.materials : r.sharedMaterials;
                if (materialSlot >= 0 && materialSlot < mats.Length)
                {
                    mats[materialSlot] = nextMat;
                    if (instancePerObject) r.materials = mats; else r.sharedMaterials = mats;
                }
            }

            if (sound != null && audioSrc != null)
            {
                audioSrc.PlayOneShot(sound);
            }

            if (glitterPrefab && glitterPerRenderer) SpawnGlitterAt(r.bounds.center);
        }

        if (glitterPrefab && !glitterPerRenderer) SpawnGlitterAt(GetTargetsCenter());

        if (verboseLogs) Debug.Log($"[MaterialCycler] Cycle -> {nextMat.name}");
    }

    Vector3 GetTargetsCenter()
    {
        Bounds b = new Bounds(transform.position, Vector3.zero);
        bool inited = false;
        foreach (var r in targets)
        {
            if (!r) continue;
            if (!inited) { b = r.bounds; inited = true; }
            else b.Encapsulate(r.bounds);
        }
        return b.center;
    }

    void SpawnGlitterAt(Vector3 worldPos)
    {
        var vfx = Instantiate(glitterPrefab, worldPos + glitterOffset, Quaternion.identity, glitterParent);
        if (glitterAutoDestroy > 0f) Destroy(vfx.gameObject, glitterAutoDestroy);
        vfx.Play(true);
    }
}
