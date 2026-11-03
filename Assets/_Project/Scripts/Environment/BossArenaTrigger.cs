using UnityEngine;
using Unity.Cinemachine; // Cinemachine 3.x
using KingLucilia.Enemies;

namespace KingLucilia.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class BossArenaTrigger : MonoBehaviour
    {
        [Header("Obstacles & Fog Wall")]
        [SerializeField] private GameObject fogWallCollider; // Pared física que bloquea la salida

        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera bossArenaCamera; // Cámara cenital/amplia de la arena
        [SerializeField] private int activePriority = 20;

        [Header("Boss Reference")]
        [SerializeField] private ColossusBoss colossusBoss;

        private bool hasTriggered = false;

        private void Awake()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;

            if (fogWallCollider != null)
            {
                fogWallCollider.SetActive(false);
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

            // 1. Sellar la salida trasera
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
        }
    }
}
