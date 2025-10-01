using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

public class ARDisplayManager : MonoBehaviour
{
    public static ARDisplayManager Instance;

    public Button btnColor;
    public Button btnScreenshot;

    private FurnitureItem current;
    private readonly List<FurnitureItem> visibles = new List<FurnitureItem>();

    public Image flash;                
    [Range(0f, 1f)] public float flashMaxAlpha = 0.6f;
    public float flashInDuration = 0.08f;
    public float flashOutDuration = 0.12f;


    public List<GameObject> hideWhileCapturing = new List<GameObject>();

    bool isCapturing;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void OnTargetVisible(FurnitureItem item)
    {
        if (item == null) return;
        if (!visibles.Contains(item)) visibles.Add(item);

        SetCurrent(item); 
    }

    public void OnTargetHidden(FurnitureItem item)
    {
        if (item == null) return;
        visibles.Remove(item);

        if (current == item)
        {
            current.SetSelected(false);
            current = null;
            if (visibles.Count > 0) SetCurrent(visibles[visibles.Count - 1]);
        }
        UpdateUI();
    }

    void SetCurrent(FurnitureItem item)
    {
        if (current == item) return;

        if (current) current.SetSelected(false);
        current = item;
        if (current) current.SetSelected(true);

        UpdateUI();
    }

    void UpdateUI()
    {
        bool on = current != null;
        if (btnColor) btnColor.interactable = on;
        if (btnScreenshot) btnScreenshot.interactable = true; 
    }

    public void ChangeColor()
    {
        if (current && current.cycler) current.cycler.Cycle(); 
    }

    public void Capture()
    {
        if (!isCapturing) StartCoroutine(CaptureRoutine());
    }

    System.Collections.IEnumerator CaptureRoutine()
    {
        if (isCapturing) yield break;
        isCapturing = true;

        UnityEngine.CanvasGroup cg = null;
        if (flash)
        {
            flash.transform.SetAsLastSibling();           
            flash.gameObject.SetActive(true);

            cg = flash.GetComponent<UnityEngine.CanvasGroup>();
            if (!cg) cg = flash.gameObject.AddComponent<UnityEngine.CanvasGroup>();
            cg.alpha = 0f;

            float t = 0f;
            while (t < flashInDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, flashMaxAlpha, Mathf.Clamp01(t / flashInDuration));
                yield return null;                          
            }
        }

        if (cg)
        {
            float t = 0f;
            while (t < flashOutDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(flashMaxAlpha, 0f, Mathf.Clamp01(t / flashOutDuration));
                yield return null;
            }
            cg.alpha = 0f;
            flash.gameObject.SetActive(false);
        }

        var saved = new List<(GameObject go, bool wasActive)>();
        foreach (var go in hideWhileCapturing) { if (!go) continue; saved.Add((go, go.activeSelf)); go.SetActive(false); }

        yield return null;                       
        yield return new WaitForEndOfFrame();    

        Texture2D tex = null;

        try
        {
            var prevActive = RenderTexture.active;
            RenderTexture.active = null; 
            tex = ScreenCapture.CaptureScreenshotAsTexture();
            RenderTexture.active = prevActive;
        }
        catch { }

        if (tex == null)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = null;

            int w = Screen.width, h = Screen.height;
            var rt = RenderTexture.active;                   
            int rw = rt ? rt.width : w;
            int rh = rt ? rt.height : h;
            var rect = new Rect(0, 0, Mathf.Min(w, rw), Mathf.Min(h, rh));

            tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            tex.ReadPixels(rect, 0, 0, false);
            tex.Apply();

            RenderTexture.active = prev;
        }

        byte[] png = tex.EncodeToPNG();
        Object.Destroy(tex);

        string folder = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Screenshots");
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
        string path = System.IO.Path.Combine(folder, $"Screen_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        System.IO.File.WriteAllBytes(path, png);

#if UNITY_ANDROID && !UNITY_EDITOR
    try {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
        {
            mediaScanner.CallStatic("scanFile", activity, new string[]{ path }, null, null);
        }
    } catch {}
#endif

        foreach (var e in saved) { if (e.go) e.go.SetActive(e.wasActive); }

        isCapturing = false;
    }

}
