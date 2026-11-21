using UnityEngine;
using UnityEngine.UI;
using KingLucilia.Player;

namespace KingLucilia.UI
{
    public class QueenBlessingHUD : MonoBehaviour
    {
        private Text statusText;
        private LuciliaWingsVFX wings;
        private bool completed;

        private void Awake()
        {
            var status = transform.Find("Text_QueenBlessing");

            if (status != null)
                statusText = status.GetComponent<Text>();
        }

        private void Update()
        {
            if (completed) return;

            if (wings == null)
                wings = FindFirstObjectByType<LuciliaWingsVFX>();

            if (wings != null && wings.HasDormantWings)
            {
                completed = true;
                Refresh();
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (statusText == null) return;

            statusText.text = completed
                ? "✓ Bendicion de QueenB recibida"
                : "○ Habla con QueenB para recibir su bendicion";

            statusText.color = completed
                ? new Color32(136, 184, 151, 255)
                : new Color32(217, 198, 155, 255);
        }
    }
}
