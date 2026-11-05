using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingLucilia.Player;
using KingLucilia.Combat;
using KingLucilia.Enemies;

namespace KingLucilia.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Player Tracking References")]
        [SerializeField] private KingLuciliaController playerController;
        [SerializeField] private KingLuciliaCombat playerCombat;
        [SerializeField] private KingLuciliaInteraction playerInteraction;

        [Header("Player Bars")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider sporeSlider;

        [Header("Context Prompt")]
        [SerializeField] private GameObject interactionPromptPanel;
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("Boss Health Bar")]
        [SerializeField] private GameObject bossBarContainer;
        [SerializeField] private Slider bossHealthSlider;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private ColossusBoss colossusBoss;

        private void Awake()
        {
            if (playerController == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<KingLuciliaController>();
                    playerCombat = playerObj.GetComponent<KingLuciliaCombat>();
                    playerInteraction = playerObj.GetComponent<KingLuciliaInteraction>();
                }
            }

            if (bossBarContainer != null)
            {
                bossBarContainer.SetActive(false);
            }

            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(false);
            }
        }

        private void Update()
        {
            UpdatePlayerBars();
            UpdateBossBar();
        }

        private void UpdatePlayerBars()
        {
            if (playerController != null && healthSlider != null)
            {
                healthSlider.maxValue = 100f;
                healthSlider.value = playerController.CurrentHealth;
            }

            if (playerCombat != null && staminaSlider != null)
            {
                staminaSlider.maxValue = 100f;
                staminaSlider.value = playerCombat.CurrentStamina;
            }
        }

        public void SetInteractionPrompt(bool isVisible, string promptMessage = "")
        {
            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(isVisible);
                if (interactionText != null && isVisible)
                {
                    interactionText.text = promptMessage;
                }
            }
        }

        public void ActivateBossBar(ColossusBoss boss, string bossDisplayName = "El Coloso Hinchado")
        {
            colossusBoss = boss;
            if (bossBarContainer != null)
            {
                bossBarContainer.SetActive(true);
            }
            if (bossNameText != null)
            {
                bossNameText.text = bossDisplayName;
            }
            if (bossHealthSlider != null && colossusBoss != null)
            {
                bossHealthSlider.maxValue = colossusBoss.MaxHealth;
                bossHealthSlider.value = colossusBoss.CurrentHealth;
            }
        }

        private void UpdateBossBar()
        {
            if (colossusBoss == null || bossBarContainer == null || !bossBarContainer.activeSelf) return;

            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = colossusBoss.CurrentHealth;
            }

            if (colossusBoss.IsDead)
            {
                bossBarContainer.SetActive(false);
            }
        }
    }
}
