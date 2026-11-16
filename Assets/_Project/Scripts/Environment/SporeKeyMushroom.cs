using UnityEngine;
using KingLucilia.Core;
using KingLucilia.Player;

namespace KingLucilia.Environment
{
    public class SporeKeyMushroom : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject collectVFX;
        private bool isCollected = false;

        public string GetInteractionPrompt() => "Presiona [E] para extraer el Hongo Primordial";

        public void Interact(KingLuciliaController player)
        {
            if (isCollected) return;
            isCollected = true;

            if (BossGateManager.Instance != null)
            {
                BossGateManager.Instance.RegisterMushroomCollected();
            }

            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, 0.15f);
        }
    }
}
