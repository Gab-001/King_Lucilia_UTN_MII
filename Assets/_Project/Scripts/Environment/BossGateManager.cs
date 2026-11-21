using System;
using UnityEngine;

namespace KingLucilia.Environment
{
    public class BossGateManager : MonoBehaviour
    {
        public static BossGateManager Instance { get; private set; }

        [Header("Gate Settings")]
        [SerializeField] private int requiredMushrooms = 3;
        [SerializeField] private GameObject gateDoorObject;
        [SerializeField] private ParticleSystem gateOpenVFX;

        private int currentMushrooms = 0;
        private bool gateOpened = false;

        public int CurrentMushrooms => currentMushrooms;
        public int RequiredMushrooms => requiredMushrooms;

        public event Action<int, int> OnMushroomCountChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterMushroomCollected()
        {
            if (gateOpened) return;

            currentMushrooms = Mathf.Min(
                currentMushrooms + 1,
                requiredMushrooms
            );

            Debug.Log(
                $"[Mision] Hongo recolectado: {currentMushrooms}/{requiredMushrooms}"
            );

            OnMushroomCountChanged?.Invoke(
                currentMushrooms,
                requiredMushrooms
            );

            if (currentMushrooms >= requiredMushrooms)
            {
                OpenBossGate();
            }
        }

        private void OpenBossGate()
        {
            if (gateOpened) return;
            gateOpened = true;

            Debug.Log(
                "[Porton] Los 3 hongos han disuelto los sellos. El porton se abre."
            );

            if (gateOpenVFX != null)
                gateOpenVFX.Play();

            if (gateDoorObject != null)
                gateDoorObject.SetActive(false);
        }
    }
}
