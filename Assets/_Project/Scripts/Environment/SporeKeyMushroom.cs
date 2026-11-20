using UnityEngine;
using KingLucilia.Core;
using KingLucilia.Player;
using KingLucilia.Audio;

namespace KingLucilia.Environment
{
    public class SporeKeyMushroom : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject collectVFX;
        private bool isCollected = false;

        public string GetInteractionPrompt() => isCollected ? "" : "Presiona [E] para extraer el Hongo Primordial";

        public void Interact(KingLuciliaController player)
        {
            if (isCollected) return;
            isCollected = true;

            // 1. Apagar de inmediato cualquier AudioSource 3D del hongo
            foreach (var src in GetComponentsInChildren<AudioSource>(true))
            {
                src.Stop();
                src.loop = false;
                src.enabled = false;
            }

            // 2. Ocultar visualmente el hongo y desactivar colisionadores al instante
            foreach (var rend in GetComponentsInChildren<Renderer>(true))
            {
                rend.enabled = false;
            }
            foreach (var col in GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }

            // 3. Reproducir su sonido exclusivo de recoleccion de hongo
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCollectMushroom();
            }

            if (BossGateManager.Instance != null)
            {
                BossGateManager.Instance.RegisterMushroomCollected();
            }

            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, 0.1f);
        }
    }
}
