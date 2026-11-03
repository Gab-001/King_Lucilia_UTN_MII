using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;

namespace KingLucilia.Player
{
    public class KingLuciliaInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3.8f;
        [SerializeField] private float detectionRadius = 0.7f;
        [SerializeField] private LayerMask interactableLayer = ~0;
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

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
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
            Vector3 originPos = rayOrigin != null ? rayOrigin.position : transform.position;
            LayerMask maskToUse = (interactableLayer.value == 0) ? ~0 : interactableLayer;

            // 1. CHEQUEO A QUEMARROPA (OverlapSphere): detecta si ya estás parada encima o pegada
            Collider[] closeColliders = Physics.OverlapSphere(originPos + Vector3.up * 0.4f, detectionRadius + 0.5f, maskToUse);
            foreach (var col in closeColliders)
            {
                var interactable = col.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"<color=green>[Cosecha Cercana]</color> Detectado a quemarropa: '{col.name}'");
                    interactable.Interact(playerController);
                    return;
                }
            }

            // 2. CHEQUEO A DISTANCIA (SphereCast hacia el frente/abajo)
            Vector3 startPos = originPos + Vector3.up * 0.6f;
            Vector3 castDirection = (transform.forward - Vector3.up * 0.25f).normalized;

            if (Physics.SphereCast(startPos, detectionRadius, castDirection, out RaycastHit hit, interactionRange, maskToUse))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"<color=green>[Cosecha Frontal]</color> Detectado en rango: '{hit.collider.name}'");
                    interactable.Interact(playerController);
                    return;
                }
            }

            Debug.LogWarning("[Interacción] Pulsaste E pero no hay carcasas ni al lado ni al frente.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 originPos = rayOrigin != null ? rayOrigin.position : transform.position;
            Gizmos.DrawWireSphere(originPos + Vector3.up * 0.4f, detectionRadius + 0.5f);
        }
    }
}
