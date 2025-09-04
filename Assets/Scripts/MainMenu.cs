using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string arSceneName = "ARScene"; // mets ici le nom EXACT de ta sc�ne AR

    // Appel� par le bouton "Lancer l'AR"
    public void StartAR()
    {
        // Optionnel : petit son de clic, transition, etc.
        SceneManager.LoadScene(arSceneName);
    }

    // Appel� par le bouton "Quitter"
    public void QuitApp()
    {
        // En build
        Application.Quit();

        // Dans l��diteur (pour tester)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}