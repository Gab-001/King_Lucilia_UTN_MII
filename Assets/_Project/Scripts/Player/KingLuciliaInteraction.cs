using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;

namespace KingLucilia.Player
{
    public class KingLuciliaInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float interactionRange = 4.0f;
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
                Debug.Log("<color=cyan>[Input OK]</color> ¡Tecla E recibida por el Input System!");
                TryInteract();
            }
        }

        private void TryInteract()
        {
            Vector3 startPos = (rayOrigin != null ? rayOrigin.position : transform.position) + Vector3.up * 0.8f;
            Ray ray = new Ray(startPos, transform.forward);

            // Si la LayerMask está vacía en el Inspector, forzamos detección general para no trabar la prueba
            LayerMask maskToUse = interactableLayer.value == 0 ? ~0 : interactableLayer;

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, maskToUse))
            {
                Debug.Log($"<color=yellow>[Raycast Hit]</color> El rayo impactó contra: '{hit.collider.gameObject.name}' en la Layer: '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'");

                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    Debug.Log("<color=green>[Éxito]</color> Objeto interactuable detectado. ¡Ejecutando cosecha!");
                    interactable.Interact(playerController);
                }
                else
                {
                    Debug.LogWarning($"[Aviso] Golpeaste a '{hit.collider.gameObject.name}', pero le falta el script 'ImmortalHusk'.");
                }
            }
            else
            {
                Debug.LogWarning("[Aviso] Pulsaste E pero el rayo no chocó con ningún objeto dentro de 4 metros. Acércate más o mira de frente.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 origin = (rayOrigin != null ? rayOrigin.position : transform.position) + Vector3.up * 0.8f;
            Gizmos.DrawRay(origin, transform.forward * interactionRange);
        }
    }
}
