using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string arSceneName = "ARScene";  
    public GameObject loadingOverlay;        

    public void PlayAR()
    {
        if (loadingOverlay)
        {
            loadingOverlay.transform.SetAsLastSibling();
            loadingOverlay.SetActive(true);
        }
        StartCoroutine(LoadAsync(arSceneName));
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    System.Collections.IEnumerator LoadAsync(string scene)
    {
        yield return new WaitForSecondsRealtime(3f); 
        var op = SceneManager.LoadSceneAsync(scene);
        while (!op.isDone) yield return null;
    }
}