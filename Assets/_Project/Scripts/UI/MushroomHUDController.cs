using UnityEngine;
using UnityEngine.UI;
using KingLucilia.Environment;

namespace KingLucilia.UI
{
    public class MushroomHUDController : MonoBehaviour
    {
        private Text counterText;
        private Image[] slots = new Image[3];
        private BossGateManager manager;

        private readonly Color inactiveColor =
            new Color32(52, 59, 59, 255);

        private readonly Color activeColor =
            new Color32(171, 142, 83, 255);

        private void Awake()
        {
            Transform counter = transform.Find("Text_MushroomCount");

            if (counter != null)
                counterText = counter.GetComponent<Text>();

            for (int i = 0; i < slots.Length; i++)
            {
                Transform slot = transform.Find(
                    "MushroomSlot_" + (i + 1)
                );

                if (slot != null)
                    slots[i] = slot.GetComponent<Image>();
            }
        }

        private void Start()
        {
            TryConnect();
        }

        private void OnEnable()
        {
            TryConnect();
        }

        private void TryConnect()
        {
            if (manager != null) return;

            manager = BossGateManager.Instance;

            if (manager == null) return;

            manager.OnMushroomCountChanged += UpdateHUD;

            UpdateHUD(
                manager.CurrentMushrooms,
                manager.RequiredMushrooms
            );
        }

        private void UpdateHUD(int current, int required)
        {
            if (counterText != null)
                counterText.text = $"{current} / {required}";

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].color =
                        i < current ? activeColor : inactiveColor;
                }
            }
        }

        private void OnDisable()
        {
            if (manager != null)
            {
                manager.OnMushroomCountChanged -= UpdateHUD;
                manager = null;
            }
        }
    }
}
