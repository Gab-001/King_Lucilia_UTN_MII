using UnityEngine;

namespace KingLucilia.Audio
{
    [RequireComponent(typeof(Collider))]
    public class AudioEventTrigger : MonoBehaviour
    {
        [Header("Trigger Actions")]
        [SerializeField] private bool triggerBossMusic = false;
        [SerializeField] private float transitionDuration = 1.2f;
        [SerializeField] private int voiceLineIndex = -1;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered = false;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;

            hasTriggered = true;

            if (AudioManager.Instance != null)
            {
                if (triggerBossMusic)
                {
                    AudioManager.Instance.SetBossCombatState(transitionDuration);
                }

                if (voiceLineIndex >= 0)
                {
                    AudioManager.Instance.PlayVoiceLine(voiceLineIndex);
                }
            }
        }
    }
}
