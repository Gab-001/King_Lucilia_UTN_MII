using UnityEngine;
using Unity.Cinemachine;
using KingLucilia.Enemies;
using KingLucilia.UI;
using KingLucilia.Audio;

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

        [Header("Audio Transition")]
        [SerializeField] private float musicTransitionTime = 1.2f;

        private bool hasTriggered = false;

        private void Awake()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;

            if (fogWallCollider != null) fogWallCollider.SetActive(false);
            if (playerHUD == null) playerHUD = FindFirstObjectByType<PlayerHUD>();
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
            if (fogWallCollider != null) fogWallCollider.SetActive(true);
            if (bossArenaCamera != null) bossArenaCamera.Priority = activePriority;
            if (colossusBoss != null) colossusBoss.WakeUp();

            if (playerHUD != null && colossusBoss != null)
            {
                playerHUD.ActivateBossBar(colossusBoss, "El Coloso Hinchado");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBossCombatState(musicTransitionTime);
                AudioManager.Instance.PlayVoiceLine(2);
            }
        }
    }
}
