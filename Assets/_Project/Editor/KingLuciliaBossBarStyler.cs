using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class KingLuciliaBossBarStyler
{
    static readonly Color Ivory = new Color32(231, 220, 198, 255);
    static readonly Color Gold = new Color32(189, 165, 116, 255);
    static readonly Color Blood = new Color32(158, 57, 73, 255);
    static readonly Color Track = new Color32(53, 48, 45, 255);

    [MenuItem("Tools/King Lucilia/Estilizar Boss Bar")]
    public static void Style()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Salí de Play primero.");
            return;
        }

        var canvas = GameObject.Find("UI_HUD_Canvas");

        if (canvas == null)
        {
            Debug.LogError("No se encontró UI_HUD_Canvas.");
            return;
        }

        var panel = canvas.transform.Find("Panel_BossBar");

        if (panel == null)
        {
            Debug.LogError("No se encontró Panel_BossBar.");
            return;
        }

        var panelRect = panel.GetComponent<RectTransform>();

        Undo.RecordObject(panelRect, "Estilizar Boss Bar");

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 42);
        panelRect.sizeDelta = new Vector2(650, 110);
        panelRect.localScale = Vector3.one;

        // Fondo del panel, si ya existe.
        var panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            Undo.RecordObject(panelImage, "Fondo Boss Bar");
            panelImage.color = new Color32(16, 21, 22, 205);
            panelImage.raycastTarget = false;
        }

        // Usar el Slider real para conservar el sistema de vida.
        var slider = panel.GetComponentInChildren<Slider>(true);

        if (slider == null)
        {
            Debug.LogWarning(
                "No se encontró Slider dentro de Panel_BossBar. " +
                "Se ajustó el panel, pero no la barra."
            );
        }
        else
        {
            var sliderRect = slider.GetComponent<RectTransform>();

            Undo.RecordObject(sliderRect, "Redimensionar vida Boss");

            sliderRect.anchorMin = new Vector2(0.5f, 0f);
            sliderRect.anchorMax = new Vector2(0.5f, 0f);
            sliderRect.pivot = new Vector2(0.5f, 0f);
            sliderRect.anchoredPosition = new Vector2(0, 24);
            sliderRect.sizeDelta = new Vector2(590, 24);
            sliderRect.localScale = Vector3.one;

            // Fondo.
            var background = slider.transform.Find("Background");

            if (background != null)
            {
                Stretch(background.GetComponent<RectTransform>());

                var image = background.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Color fondo Boss");
                    image.color = Track;
                }
            }

            // Fill Area y Fill: importante para que
            // la barra no quede demasiado fina.
            var fillArea = slider.transform.Find("Fill Area");

            if (fillArea != null)
            {
                Stretch(fillArea.GetComponent<RectTransform>());

                var fill = fillArea.Find("Fill");

                if (fill != null)
                {
                    var fillRect = fill.GetComponent<RectTransform>();

                    Undo.RecordObject(fillRect, "Fill Boss");

                    fillRect.anchorMin = new Vector2(0, 0);
                    fillRect.anchorMax = new Vector2(1, 1);
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;

                    var fillImage = fill.GetComponent<Image>();

                    if (fillImage != null)
                    {
                        Undo.RecordObject(fillImage, "Color vida Boss");
                        fillImage.color = Blood;
                    }
                }
            }

            // Ocultar el control visual de arrastre,
            // sin eliminar el Slider ni sus referencias.
            var handle = slider.transform.Find("Handle Slide Area");

            if (handle != null)
            {
                var graphics = handle.GetComponentsInChildren<Graphic>(true);
                foreach (var graphic in graphics)
                {
                    Undo.RecordObject(graphic, "Ocultar handle Boss");
                    graphic.enabled = false;
                }
            }

            slider.interactable = false;
            EditorUtility.SetDirty(slider);
        }

        CreateLabel(
            panel,
            "KL_BossName",
            "EL COLOSO",
            25,
            Ivory,
            68
        );

        CreateLabel(
            panel,
            "KL_BossSubtitle",
            "GUARDIÁN DEL REINO CORROMPIDO",
            12,
            Gold,
            94
        );

        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);

        Selection.activeGameObject = panel.gameObject;

        Debug.Log("[King Lucilia] Boss Bar estilizada.");
    }

    static void CreateLabel(
        Transform parent,
        string name,
        string value,
        int size,
        Color color,
        float y)
    {
        var existing = parent.Find(name);
        GameObject obj;

        if (existing != null)
        {
            obj = existing.gameObject;
        }
        else
        {
            obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(parent, false);

            Undo.RegisterCreatedObjectUndo(
                obj, "Crear texto Boss Bar"
            );
        }

        var rect = obj.GetComponent<RectTransform>();

        Undo.RecordObject(rect, "Posicionar texto Boss");

        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(610, 30);

        var tmp = obj.GetComponent<TextMeshProUGUI>();

        if (tmp != null)
        {
            tmp.text = value;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }

    static void Stretch(RectTransform rect)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estirar Boss Bar");

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
