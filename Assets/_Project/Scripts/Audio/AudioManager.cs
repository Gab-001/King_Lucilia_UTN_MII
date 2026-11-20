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
        [SerializeField] private AudioSource explorationMusicSource;
        [SerializeField] private AudioSource bossMusicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("SFX con Variacion (Bloque C)")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioClip[] slashClips;

        [Header("SFX Fijos (Bloque C)")]
        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip harvestSporeClip;
        [SerializeField] private AudioClip damageClip;
        [SerializeField] private AudioClip sporeShotClip;
        [SerializeField] private AudioClip collectMushroomClip;
        [SerializeField] private AudioClip queenInteractClip;

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
            PlayVoiceLine(0);
        }

        public void SetExplorationState(float transitionTime = 2.0f)
        {
            IsBossFightActive = false;
            if (bossMusicSource != null && bossMusicSource.isPlaying) bossMusicSource.Stop();
            if (explorationMusicSource != null && !explorationMusicSource.isPlaying) explorationMusicSource.Play();
            if (snapLlanuras != null) snapLlanuras.TransitionTo(transitionTime);
        }

        public void SetBossCombatState(float transitionTime = 1.2f)
        {
            IsBossFightActive = true;
            if (explorationMusicSource != null && explorationMusicSource.isPlaying) explorationMusicSource.Stop();
            if (bossMusicSource != null && !bossMusicSource.isPlaying) bossMusicSource.Play();
            if (snapArenaColoso != null) snapArenaColoso.TransitionTo(transitionTime);
        }

        public void PlayFootstep() => PlayRandomVariation(footstepClips, 0.97f, 1.03f, 0.82f, 0.96f);
        public void PlaySlash() => PlayRandomVariation(slashClips, 0.96f, 1.04f, 0.88f, 1.0f);

        private void PlayRandomVariation(AudioClip[] clips, float minPitch, float maxPitch, float minVol, float maxVol)
        {
            if (clips == null || clips.Length == 0 || sfxSource == null) return;
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            AudioClip selected = clips[Random.Range(0, clips.Length)];
            sfxSource.PlayOneShot(selected, Random.Range(minVol, maxVol));
        }

        public void PlayDash() => PlayFixedSFX(dashClip, 0.95f);
        public void PlayHarvest() => PlayFixedSFX(harvestSporeClip, 1.0f);
        public void PlayDamage() => PlayFixedSFX(damageClip, 1.0f);
        public void PlaySporeShot() => PlayFixedSFX(sporeShotClip, 0.95f);
        public void PlayCollectMushroom() => PlayFixedSFX(collectMushroomClip, 1.0f);
        public void PlayQueenInteract() => PlayFixedSFX(queenInteractClip, 0.95f);

        private void PlayFixedSFX(AudioClip clip, float volume)
        {
            if (clip == null || sfxSource == null) return;
            // Mantiene el pitch limpio sin cortar la cola del tajo
            sfxSource.PlayOneShot(clip, volume);
        }

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
