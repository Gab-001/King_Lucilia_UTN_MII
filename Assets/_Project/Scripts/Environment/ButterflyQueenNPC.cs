using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using KingLucilia.Core;
using KingLucilia.Player;
using KingLucilia.Audio;

namespace KingLucilia.Environment
{
    public class ButterflyQueenNPC : MonoBehaviour, IInteractable
    {
        [Header("Cinematic Settings")]
        [SerializeField] private CinemachineCamera dialogueCamera;
        [SerializeField] private float textSpeed = 0.035f;

        [Header("UI Canvas Reference (Optional)")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
        [SerializeField] private TMPro.TextMeshProUGUI speakerNameText;

        [Header("Narrative Lines")]
        [TextArea(3, 5)]
        [SerializeField] private string[] dialogueLines = new string[]
        {
            "Te reconozco, Lucilia... el último que aún recuerda cómo devolver la carne a la tierra.",
            "Los humanos nos arrebataron el vuelo antes de que el falso dios silenciara la muerte. Cortaron mis alas creyendo que así apagarían nuestra danza.",
            "Llévalas contigo. En tu espalda están selladas y mudas... pero cuando rompas la carne hinchada del Coloso y su núcleo estalle, despertarán."
        };

        private bool giftGranted = false;
        private bool isInteracting = false;
        private KingLuciliaController activePlayer;

        public string GetInteractionPrompt() => giftGranted ? "" : "Presiona [E] para hablar con la Reina";

        public void Interact(KingLuciliaController player)
        {
            if (giftGranted || isInteracting) return;
            activePlayer = player;
            StartCoroutine(CinematicDialogueRoutine());
        }

        private IEnumerator CinematicDialogueRoutine()
        {
            isInteracting = true;

            try
            {
                if (activePlayer != null)
                {
                    var anim = activePlayer.GetComponentInChildren<Animator>();
                    if (anim != null) anim.SetFloat("Speed", 0f);
                    activePlayer.enabled = false;
                }

                if (dialogueCamera != null) dialogueCamera.Priority = 20;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayQueenInteract();
                    AudioManager.Instance.PlayVoiceLine(1);
                }

                if (dialoguePanel != null) dialoguePanel.SetActive(true);
                if (speakerNameText != null) speakerNameText.text = "Reina Mariposa";

                yield return new WaitForSeconds(1.0f);

                foreach (string line in dialogueLines)
                {
                    if (dialogueText != null)
                    {
                        dialogueText.text = "";
                        foreach (char letter in line.ToCharArray())
                        {
                            dialogueText.text += letter;
                            yield return new WaitForSeconds(textSpeed);
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(2.5f);
                    }

                    yield return new WaitForSeconds(1.8f);
                }

                if (activePlayer != null && activePlayer.TryGetComponent<LuciliaWingsVFX>(out var wingsComp))
                {
                    wingsComp.GrantDormantWings();
                }

                giftGranted = true;
            }
            finally
            {
                if (dialoguePanel != null) dialoguePanel.SetActive(false);
                if (dialogueCamera != null) dialogueCamera.Priority = 0;

                if (activePlayer != null) activePlayer.enabled = true;
                isInteracting = false;
            }
        }
    }
}
