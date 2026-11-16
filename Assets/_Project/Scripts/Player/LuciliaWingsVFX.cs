using System.Collections;
using UnityEngine;

namespace KingLucilia.Player
{
    public class LuciliaWingsVFX : MonoBehaviour
    {
        [Header("Wing VFX Elements")]
        [SerializeField] private GameObject wingsObject;
        [SerializeField] private ParticleSystem wingsBurstParticles;

        private bool hasDormantWings = false;

        private void Awake()
        {
            if (wingsObject != null) wingsObject.SetActive(false);
        }

        public void GrantDormantWings()
        {
            hasDormantWings = true;
            Debug.Log("[Don Recibido] Alas selladas en la espalda de Lucilia.");
        }

        public void ManifestWings()
        {
            if (!hasDormantWings) return;
            StartCoroutine(ManifestWingsRoutine());
        }

        private IEnumerator ManifestWingsRoutine()
        {
            yield return new WaitForSeconds(1.8f);

            if (wingsBurstParticles != null) wingsBurstParticles.Play();
            if (wingsObject != null) wingsObject.SetActive(true);

            Debug.Log("[Continuara...] La energia liberada restaura las alas en el Rey Lucilia.");
        }
    }
}
