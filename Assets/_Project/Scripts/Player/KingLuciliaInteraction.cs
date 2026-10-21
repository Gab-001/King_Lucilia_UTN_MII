using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;

namespace KingLucilia.Player
{
    public class KingLuciliaInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float interactionRange = 3.5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform rayOrigin;

        private KingLuciliaController playerController;

        private void Awake()
        {
            playerController = GetComponent<KingLuciliaController>();
            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            Ray ray = new Ray(rayOrigin.position + Vector3.up * 0.5f, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    interactable.Interact(playerController);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 origin = (rayOrigin != null ? rayOrigin.position : transform.position) + Vector3.up * 0.5f;
            Gizmos.DrawRay(origin, transform.forward * interactionRange);
        }
    }
}
