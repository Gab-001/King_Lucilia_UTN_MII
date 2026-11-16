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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterMushroomCollected()
        {
            currentMushrooms++;
            Debug.Log($"[Mision] Hongo recolectado: {currentMushrooms}/{requiredMushrooms}");

            if (currentMushrooms >= requiredMushrooms)
            {
                OpenBossGate();
            }
        }

        private void OpenBossGate()
        {
            Debug.Log("[Porton] Los 3 hongos han disuelto los sellos. El porton se abre.");
            if (gateOpenVFX != null) gateOpenVFX.Play();
            if (gateDoorObject != null) gateDoorObject.SetActive(false);
        }
    }
}
