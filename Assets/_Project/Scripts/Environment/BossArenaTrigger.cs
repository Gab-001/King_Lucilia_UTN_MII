using UnityEngine;
using Unity.Cinemachine;
using KingLucilia.Enemies;
using KingLucilia.UI;

namespace KingLucilia.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class BossArenaTrigger : MonoBehaviour
    {
        [Header("Obstacles & Fog Wall")]
        [SerializeField] private GameObject fogWallCollider;

        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera bossArenaCamera;
        [SerializeField] private int activePriority = 20;

        [Header("Boss Reference")]
        [SerializeField] private ColossusBoss colossusBoss;

        [Header("UI Reference")]
        [SerializeField] private PlayerHUD playerHUD;

        private bool hasTriggered = false;

        private void Awake()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;

            if (fogWallCollider != null)
            {
                fogWallCollider.SetActive(false);
            }

            if (playerHUD == null)
            {
                playerHUD = FindFirstObjectByType<PlayerHUD>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<KingLucilia.Player.KingLuciliaController>() != null)
            {
                hasTriggered = true;
                ActivateBossEncounter();
            }
        }

        private void ActivateBossEncounter()
        {
            Debug.Log("<color=yellow>[Boss Arena]</color> Lucilia cruzó el umbral. ¡Despertando al Coloso Hinchado!");

            // 1. Sellar salida
            if (fogWallCollider != null)
            {
                fogWallCollider.SetActive(true);
            }

            // 2. Conmutar cámara Cinemachine
            if (bossArenaCamera != null)
            {
                bossArenaCamera.Priority = activePriority;
            }

            // 3. Despertar la IA del Coloso
            if (colossusBoss != null)
            {
                colossusBoss.WakeUp();
            }

            // 4. Encender barra de vida superior en el HUD
            if (playerHUD != null && colossusBoss != null)
            {
                playerHUD.ActivateBossBar(colossusBoss, "El Coloso Hinchado");
            }
        }
    }
}
