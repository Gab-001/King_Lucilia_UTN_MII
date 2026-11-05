using UnityEngine;
using KingLucilia.UI;

namespace KingLucilia.Environment
{
    [RequireComponent(typeof(Collider))]
    public class TutorialTrigger : MonoBehaviour
    {
        [Header("Tutorial Message")]
        [TextArea(2, 4)]
        [SerializeField] private string tutorialMessage = "Presiona [Espacio] para saltar";

        [Header("Display Settings")]
        [SerializeField] private bool showOnlyOnce = true;
        [SerializeField] private float autoHideDelay = 0f; // 0 significa que se oculta al salir del trigger

        private PlayerHUD playerHUD;
        private bool hasShown = false;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            playerHUD = FindFirstObjectByType<PlayerHUD>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (showOnlyOnce && hasShown) return;

            if (other.CompareTag("Player") || other.GetComponent<KingLucilia.Player.KingLuciliaController>() != null)
            {
                if (playerHUD == null)
                {
                    playerHUD = FindFirstObjectByType<PlayerHUD>();
                }

                if (playerHUD != null)
                {
                    playerHUD.SetInteractionPrompt(true, tutorialMessage);
                    hasShown = true;

                    if (autoHideDelay > 0f)
                    {
                        Invoke(nameof(HidePrompt), autoHideDelay);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (autoHideDelay <= 0f)
            {
                if (other.CompareTag("Player") || other.GetComponent<KingLucilia.Player.KingLuciliaController>() != null)
                {
                    HidePrompt();

                    if (showOnlyOnce)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        private void HidePrompt()
        {
            if (playerHUD != null)
            {
                playerHUD.SetInteractionPrompt(false);
            }
        }
    }
}
