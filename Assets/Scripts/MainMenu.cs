using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string arSceneName = "ARScene"; // mets ici le nom EXACT de ta scène AR

    // Appelé par le bouton "Lancer l'AR"
    public void StartAR()
    {
        // Optionnel : petit son de clic, transition, etc.
        SceneManager.LoadScene(arSceneName);
    }

    // Appelé par le bouton "Quitter"
    public void QuitApp()
    {
        // En build
        Application.Quit();

        // Dans l’éditeur (pour tester)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}