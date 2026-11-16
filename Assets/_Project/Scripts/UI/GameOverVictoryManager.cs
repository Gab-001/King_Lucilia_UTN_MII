using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using KingLucilia.Player;
using KingLucilia.Enemies;

namespace KingLucilia.UI
{
    public class GameOverVictoryManager : MonoBehaviour
    {
        [Header("Entity References")]
        [SerializeField] private KingLuciliaController player;
        [SerializeField] private ColossusBoss colossus;

        [Header("UI Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Scene Navigation")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool sessionEnded = false;
        private bool isPaused = false;

        private void Awake()
        {
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponent<KingLuciliaController>();
                }
            }

            if (colossus == null)
            {
                colossus = FindFirstObjectByType<ColossusBoss>();
            }

            // Asegurar que los paneles inicien apagados y el tiempo activo
            if (pausePanel != null) pausePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            Time.timeScale = 1f;
            LockCursor(true);
        }

        private void Update()
        {
            if (sessionEnded) return;

            // Manejo de Pausa con Escape
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }

            if (isPaused) return;

            // Condición de Derrota: Lucilia cae
            if (player != null && player.IsDead)
            {
                TriggerDefeat();
            }

            // Condición de Victoria: El Coloso es derrotado
            if (colossus != null && colossus.IsDead)
            {
                TriggerVictory();
            }
        }

        public void TogglePause()
        {
            if (sessionEnded) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            LockCursor(!isPaused);
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            TogglePause();
        }

        private void TriggerDefeat()
        {
            sessionEnded = true;
            Time.timeScale = 0f;

            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
            }

            LockCursor(false);
        }

        private void TriggerVictory()
        {
            sessionEnded = true;
            Time.timeScale = 0f;

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            LockCursor(false);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
