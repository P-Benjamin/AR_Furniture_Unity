#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class OpenSceneOnLoad
{
    const string PreferredMenuScenePath = "Assets/Scenes/MainMenu.unity";

    static OpenSceneOnLoad()
    {
        EditorApplication.update += OnEditorLoadedOnce;
    }

    static void OnEditorLoadedOnce()
    {
        EditorApplication.update -= OnEditorLoadedOnce;

        var active = EditorSceneManager.GetActiveScene();
        bool invalid = !active.IsValid() || string.IsNullOrEmpty(active.path) || active.name == "Untitled";
        if (!invalid) return;

        string targetPath = null;
        if (EditorBuildSettings.scenes != null && EditorBuildSettings.scenes.Length > 0)
        {
            var s0 = EditorBuildSettings.scenes[0];
            if (s0.enabled) targetPath = s0.path;
        }

        if (string.IsNullOrEmpty(targetPath) && System.IO.File.Exists(PreferredMenuScenePath))
            targetPath = PreferredMenuScenePath;

        if (string.IsNullOrEmpty(targetPath))
        {
            var guids = AssetDatabase.FindAssets("MainMenu t:scene");
            if (guids.Length > 0) targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        if (!string.IsNullOrEmpty(targetPath))
        {
            EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
            Debug.Log("[AutoOpenMenuOnLoad] Ouverture automatique : " + targetPath);
        }
        else
        {
            Debug.LogWarning("[AutoOpenMenuOnLoad] Aucune scène menu trouvée (vérifie le chemin/Build Settings).");
        }
    }
}
#endif
