using UnityEngine;
using UnityEngine.Audio;

namespace KingLucilia.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer & Snapshots (Bloques A y B)")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerSnapshot snapLlanuras;
        [SerializeField] private AudioMixerSnapshot snapArenaColoso;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("SFX con Variacion (Bloque C)")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioClip[] slashClips;

        [Header("SFX Fijos (Bloque C)")]
        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip harvestSporeClip;
        [SerializeField] private AudioClip damageClip;

        [Header("Lineas de Voz (Bloque E)")]
        [SerializeField] private AudioClip[] voiceLines;

        public AudioSource VoiceSource => voiceSource;
        public bool IsBossFightActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            SetExplorationState(0.5f);
            PlayVoiceLine(0); // Linea 1 al iniciar el recorrido
        }

        // Bloque B: Estados musicales con tiempos de transicion deliberados
        public void SetExplorationState(float transitionTime = 2.0f)
        {
            IsBossFightActive = false;
            if (snapLlanuras != null) snapLlanuras.TransitionTo(transitionTime);
        }

        public void SetBossCombatState(float transitionTime = 1.2f)
        {
            IsBossFightActive = true;
            if (snapArenaColoso != null) snapArenaColoso.TransitionTo(transitionTime);
        }

        // Bloque C: SFX con variacion (clips alternados + pitch/volumen aleatorios)
        public void PlayFootstep() => PlayRandomVariation(footstepClips, 0.92f, 1.08f, 0.75f, 0.95f);
        public void PlaySlash() => PlayRandomVariation(slashClips, 0.90f, 1.12f, 0.85f, 1.0f);

        private void PlayRandomVariation(AudioClip[] clips, float minPitch, float maxPitch, float minVol, float maxVol)
        {
            if (clips == null || clips.Length == 0 || sfxSource == null) return;
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            AudioClip selected = clips[Random.Range(0, clips.Length)];
            sfxSource.PlayOneShot(selected, Random.Range(minVol, maxVol));
        }

        // Bloque C: SFX puntuales fijos
        public void PlayDash() => PlayFixedSFX(dashClip, 0.95f);
        public void PlayHarvest() => PlayFixedSFX(harvestSporeClip, 1.0f);
        public void PlayDamage() => PlayFixedSFX(damageClip, 1.0f);

        private void PlayFixedSFX(AudioClip clip, float volume)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, volume);
        }

        // Bloque E: Disparo de voz humana editada
        public void PlayVoiceLine(int index)
        {
            if (voiceSource == null || voiceLines == null || index < 0 || index >= voiceLines.Length) return;
            if (voiceLines[index] == null) return;

            voiceSource.Stop();
            voiceSource.clip = voiceLines[index];
            voiceSource.Play();
        }
    }
}
