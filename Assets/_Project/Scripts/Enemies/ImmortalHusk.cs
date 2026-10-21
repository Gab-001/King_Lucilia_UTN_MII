using UnityEngine;
using KingLucilia.Core;
using KingLucilia.Player;

namespace KingLucilia.Enemies
{
    public class ImmortalHusk : MonoBehaviour, IInteractable
    {
        [Header("Harvest Feedback")]
        [SerializeField] private GameObject sporeDissolveVFXPrefab;
        [SerializeField] private float destroyDelay = 0.2f;

        private bool isHarvested = false;

        public string GetInteractionPrompt()
        {
            return "Presiona [E] para liberar con esporas";
        }

        public void Interact(KingLuciliaController player)
        {
            if (isHarvested) return;

            isHarvested = true;

            if (sporeDissolveVFXPrefab != null)
            {
                Instantiate(sporeDissolveVFXPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, destroyDelay);
        }
    }
}
