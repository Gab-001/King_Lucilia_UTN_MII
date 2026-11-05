using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingLucilia.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string gameplaySceneName = "Parcial2_VerticalSlice";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Panels")]
        [SerializeField] private GameObject controlsPanel;

        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void ToggleControlsPanel(bool show)
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(show);
            }
        }

        public void QuitGame()
        {
            Debug.Log("[Menu] Saliendo de la aplicacion...");
            Application.Quit();
        }
    }
}
