using UnityEngine;
using UnityEngine.Audio;

namespace KingLucilia.Audio
{
    public class KingLuciliaAudioController : MonoBehaviour
    {
        [Header("Mixer Reference")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private float duckedMusicDb = -16f;
        [SerializeField] private float duckedAmbientDb = -18f;
        [SerializeField] private float fadeSpeed = 8f;

        private float currentMusicDb = -6f;
        private float currentAmbientDb = -8f;

        private void Update()
        {
            if (AudioManager.Instance == null || mainMixer == null) return;

            bool isVoicePlaying = AudioManager.Instance.VoiceSource != null && AudioManager.Instance.VoiceSource.isPlaying;
            bool inBossFight = AudioManager.Instance.IsBossFightActive;

            // Condicion 1: Ducking automatico cuando habla la Reina o Lucilia
            float targetMusic = isVoicePlaying ? duckedMusicDb : (inBossFight ? -2f : -6f);

            // Condicion 2: Ajuste dinamico del lecho ambiental segun voz o combate
            float targetAmbient = isVoicePlaying ? duckedAmbientDb : (inBossFight ? -15f : -8f);

            currentMusicDb = Mathf.Lerp(currentMusicDb, targetMusic, Time.deltaTime * fadeSpeed);
            currentAmbientDb = Mathf.Lerp(currentAmbientDb, targetAmbient, Time.deltaTime * fadeSpeed);

            mainMixer.SetFloat("VolMusica", currentMusicDb);
            mainMixer.SetFloat("VolAmbiente", currentAmbientDb);
        }
    }
}
