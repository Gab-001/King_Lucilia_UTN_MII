using UnityEngine;
using UnityEngine.UI;
using KingLucilia.Player;
using KingLucilia.Enemies;

namespace KingLucilia.UI
{
    public class GameOverVictoryManager : MonoBehaviour
    {
        [Header("Player Tracking")]
        [SerializeField] private KingLuciliaController player;

        [Header("Boss Tracking")]
        [SerializeField] private ColossusBoss colossus;

        [Header("Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        private bool sessionEnded = false;

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

            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        private void Update()
        {
            if (sessionEnded) return;

            // Condición de derrota: Lucilia cae
            if (player != null && player.IsDead)
            {
                TriggerDefeat();
            }

            // Condición de victoria: El Coloso es derrotado
            if (colossus != null && colossus.IsDead)
            {
                TriggerVictory();
            }
        }

        private void TriggerDefeat()
        {
            sessionEnded = true;
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void TriggerVictory()
        {
            sessionEnded = true;
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
