using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

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

    [SerializeField] string galleryAlbum = "AR_Furniture";
    [SerializeField] bool saveInDCIMCamera = true; 

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

        CanvasGroup cg = null;
        if (flash)
        {
            flash.transform.SetAsLastSibling();
            flash.gameObject.SetActive(true);
            var img = flash.GetComponent<Image>(); if (img) img.raycastTarget = false;

            cg = flash.GetComponent<CanvasGroup>() ?? flash.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.alpha = 0f;

            float tIn = 0f;
            while (tIn < flashInDuration)
            {
                tIn += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, flashMaxAlpha, Mathf.Clamp01(tIn / flashInDuration));
                yield return null;
            }
        }
        if (cg && flash)
        {
            float tOut = 0f;
            while (tOut < flashOutDuration)
            {
                tOut += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(flashMaxAlpha, 0f, Mathf.Clamp01(tOut / flashOutDuration));
                yield return null;
            }
            cg.alpha = 0f;
            flash.gameObject.SetActive(false);
        }

        var saved = new List<(GameObject go, bool wasActive)>();
        foreach (var go in hideWhileCapturing) { if (!go) continue; saved.Add((go, go.activeSelf)); go.SetActive(false); }

        try
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            Texture2D tex = null;
            try
            {
                var prevActive = RenderTexture.active;
                RenderTexture.active = null; // backbuffer
                tex = ScreenCapture.CaptureScreenshotAsTexture();
                RenderTexture.active = prevActive;
            }
            catch {  }

            if (tex == null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = null;
                int w = Screen.width, h = Screen.height;
                var rt = RenderTexture.active; int rw = rt ? rt.width : w; int rh = rt ? rt.height : h;
                var rect = new Rect(0, 0, Mathf.Min(w, rw), Mathf.Min(h, rh));
                tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
                tex.ReadPixels(rect, 0, 0, false);
                tex.Apply();
                RenderTexture.active = prev;
            }
            if (tex == null) { Debug.LogError("[Screenshot] Texture nulle"); yield break; }

            byte[] png = tex.EncodeToPNG();
            Destroy(tex);
            if (png == null || png.Length == 0) { Debug.LogError("[Screenshot] PNG vide"); yield break; }
            Debug.Log($"[Screenshot] OK bytes={png.Length}");

            string filename = $"Screen_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

#if UNITY_ANDROID && !UNITY_EDITOR
        // 👉 met à true pour tester : DCIM/Camera (très fiable sur Samsung)
        bool ok = SaveToGalleryAndroid(png, filename, "AR_Furniture", useDCIMCamera:true);
        Debug.Log("[Screenshot] SaveToGalleryAndroid ok=" + ok + " (dest=" + (true ? "DCIM/Camera" : "Pictures/AR_Furniture") + ")");
        if (!ok)
        {
            // Fallback local (toujours présent)
            string folder = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Screenshots");
            System.IO.Directory.CreateDirectory(folder);
            string path = System.IO.Path.Combine(folder, filename);
            System.IO.File.WriteAllBytes(path, png);
            Debug.LogWarning("[Screenshot] Fallback: " + path);
        }
#else
            string folder = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Screenshots");
            System.IO.Directory.CreateDirectory(folder);
            string path = System.IO.Path.Combine(folder, filename);
            System.IO.File.WriteAllBytes(path, png);
            Debug.Log("[Screenshot] Saved: " + path);
#endif
        }
        finally
        {
            foreach (var e in saved) if (e.go) e.go.SetActive(e.wasActive);
            isCapturing = false;
        }
    }



#if UNITY_ANDROID && !UNITY_EDITOR
static bool SaveToGalleryAndroid(byte[] data, string fileName, string album, bool useDCIMCamera)
{
    try
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
        using (var values = new AndroidJavaObject("android.content.ContentValues"))
        using (var images = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
        {
            // DCIM/Camera = quasi toujours visible immédiatement dans la Galerie Samsung
            string relativePath = useDCIMCamera ? "DCIM/Camera/" : ("Pictures/" + album + "/");

            values.Call<AndroidJavaObject>("put", "mime_type", "image/png");
            values.Call<AndroidJavaObject>("put", "relative_path", relativePath);
            values.Call<AndroidJavaObject>("put", "_display_name", fileName);
            values.Call<AndroidJavaObject>("put", "is_pending", 1);

            // (facultatif) améliorer l’indexation:
            long now = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            values.Call<AndroidJavaObject>("put", "datetaken", now);
            values.Call<AndroidJavaObject>("put", "date_added", now/1000);
            values.Call<AndroidJavaObject>("put", "date_modified", now/1000);

            var uri = resolver.Call<AndroidJavaObject>("insert",
                images.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);
            if (uri == null) throw new System.Exception("resolver.insert null");

            using (var stream = resolver.Call<AndroidJavaObject>("openOutputStream", uri, "w"))
            {
                if (stream == null) throw new System.Exception("openOutputStream null");
                stream.Call("write", data);
                stream.Call("flush");
                stream.Call("close");
            }

            var cv = new AndroidJavaObject("android.content.ContentValues");
            cv.Call<AndroidJavaObject>("put", "is_pending", 0);
            resolver.Call<int>("update", uri, cv, null, null);

            Debug.Log("[Gallery] Saved: " + relativePath + fileName + " / uri=" + uri.ToString());
            return true;
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError("[SaveToGalleryAndroid] " + ex.Message);
        return false;
    }
}
#endif


}
