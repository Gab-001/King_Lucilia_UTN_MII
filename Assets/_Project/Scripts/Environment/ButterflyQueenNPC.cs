using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using KingLucilia.Core;
using KingLucilia.Player;
using KingLucilia.Audio;

namespace KingLucilia.Environment
{
    public class ButterflyQueenNPC : MonoBehaviour, IInteractable
    {
        public enum Speaker
        {
            QueenB,
            Lucilia
        }

        [System.Serializable]
        public class DialogueLine
        {
            public Speaker speaker;

            [TextArea(3, 5)]
            public string text;
        }

        [Header("Cinematic Settings")]
        [SerializeField] private CinemachineCamera dialogueCamera;
        [SerializeField] private float textSpeed = 0.035f;

        [Header("UI Canvas Reference")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI speakerNameText;

        [Header("Narrative Lines")]
        [SerializeField] private DialogueLine[] conversation =
        {
            new DialogueLine
            {
                speaker = Speaker.QueenB,
                text = "Te reconozco, Lucilia... el último que aún recuerda cómo devolver la carne a la tierra."
            },
            new DialogueLine
            {
                speaker = Speaker.Lucilia,
                text = "He venido a detener al Coloso. Pero necesito comprender qué ocurrió en este reino."
            },
            new DialogueLine
            {
                speaker = Speaker.QueenB,
                text = "Los humanos nos arrebataron el vuelo antes de que el falso dios silenciara la muerte. Cortaron mis alas creyendo que así apagarían nuestra danza."
            },
            new DialogueLine
            {
                speaker = Speaker.Lucilia,
                text = "Entonces llevaré tu bendición conmigo. No permitiré que el Coloso siga dominando este lugar."
            },
            new DialogueLine
            {
                speaker = Speaker.QueenB,
                text = "Llévalas contigo. En tu espalda están selladas y mudas... pero cuando rompas la carne hinchada del Coloso y su núcleo estalle, despertarán."
            }
        };

        private bool giftGranted;
        private bool isInteracting;
        private KingLuciliaController activePlayer;

        public string GetInteractionPrompt()
        {
            return giftGranted
                ? ""
                : "Presiona [E] para hablar con la Reina";
        }

        public void Interact(KingLuciliaController player)
        {
            if (giftGranted || isInteracting || player == null)
                return;

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

                    if (anim != null)
                        anim.SetFloat("Speed", 0f);

                    activePlayer.enabled = false;
                }

                if (dialogueCamera != null)
                    dialogueCamera.Priority = 20;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayQueenInteract();
                    AudioManager.Instance.PlayVoiceLine(1);
                }

                if (dialoguePanel != null)
                    dialoguePanel.SetActive(true);

                yield return new WaitForSecondsRealtime(1f);

                if (conversation != null)
                {
                    foreach (DialogueLine line in conversation)
                    {
                        if (line == null) continue;

                        if (speakerNameText != null)
                        {
                            speakerNameText.text =
                                line.speaker == Speaker.QueenB
                                ? "REINA MARIPOSA"
                                : "LUCILIA";

                            speakerNameText.color =
                                line.speaker == Speaker.QueenB
                                ? new Color32(201, 166, 110, 255)
                                : new Color32(136, 184, 151, 255);
                        }

                        if (dialogueText != null)
                        {
                            dialogueText.text = "";

                            string phrase = line.text ?? "";

                            foreach (char letter in phrase)
                            {
                                dialogueText.text += letter;
                                yield return new WaitForSecondsRealtime(
                                    Mathf.Max(0.001f, textSpeed)
                                );
                            }
                        }
                        else
                        {
                            yield return new WaitForSecondsRealtime(2.5f);
                        }

                        yield return new WaitForSecondsRealtime(1.8f);
                    }
                }

                // La bendición se entrega al terminar
                // toda la conversación.
                if (activePlayer != null &&
                    activePlayer.TryGetComponent<LuciliaWingsVFX>(
                        out var wingsComp))
                {
                    wingsComp.GrantDormantWings();
                    giftGranted = true;
                }
                else
                {
                    Debug.LogWarning(
                        "[QueenB] No se encontró LuciliaWingsVFX. " +
                        "La bendición no fue entregada."
                    );
                }
            }
            finally
            {
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(false);

                if (dialogueCamera != null)
                    dialogueCamera.Priority = 0;

                if (activePlayer != null)
                    activePlayer.enabled = true;

                isInteracting = false;
            }
        }
    }
}
