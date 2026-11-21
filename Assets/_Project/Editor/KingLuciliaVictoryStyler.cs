using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class KingLuciliaVictoryStyler
{
    static readonly Color Ivory = new Color32(233, 223, 201, 255);
    static readonly Color Gold = new Color32(197, 166, 108, 255);
    static readonly Color Muted = new Color32(157, 167, 155, 255);
    static readonly Color Dark = new Color32(20, 26, 27, 250);
    static readonly Color Secondary = new Color32(41, 50, 50, 255);

    [MenuItem("Tools/King Lucilia/Estilizar Panel Victory")]
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

        var overlay = canvas.transform.Find("Panel_Victory");
        var card = overlay != null
            ? overlay.Find("Panel_Victory") : null;
        var content = card != null
            ? card.Find("Menu_Content") : null;

        if (content == null)
        {
            Debug.LogError("No se encontró la estructura del panel Victory.");
            return;
        }

        Stretch(overlay.GetComponent<RectTransform>());

        var overlayImage = GetImage(overlay.gameObject);
        overlayImage.color = new Color(0.025f, 0.035f, 0.04f, 0.9f);
        overlayImage.raycastTarget = true;

        Center(card.GetComponent<RectTransform>(), 420, 490, 0, 0);

        var cardImage = GetImage(card.gameObject);
        cardImage.color = Dark;
        cardImage.raycastTarget = true;

        Stretch(content.GetComponent<RectTransform>());

        DisableLayouts(card.gameObject);
        DisableLayouts(content.gameObject);

        // Título original.
        var title = content.Find("Title");
        if (title != null)
        {
            Center(title.GetComponent<RectTransform>(), 370, 60, 0, 105);
            SetText(title.gameObject, "VICTORIA", 36, Ivory);
        }

        // Decoración y narrativa.
        CreateLabel(content, "KL_VictoryBrand",
            "KING LUCILIA", 15, Gold, 197);

        CreateLabel(content, "KL_VictorySubtitle",
            "EL DESPERTAR DE LAS ALAS", 13, Gold, 65);

        CreateLabel(content, "KL_VictoryMessage",
            "El coloso ha caído.", 19, Ivory, 7);

        CreateLabel(content, "KL_VictoryDescription",
            "La bendición de la Reina Mariposa finalmente despierta.",
            14, Muted, -36);

        StyleButton(content, "Btn_PlayAgain",
            "VOLVER A JUGAR", -113, true);

        StyleButton(content, "Btn_Exit",
            "MENÚ PRINCIPAL", -176, false);

        CreateLabel(content, "KL_VictoryFooter",
            "El final de una batalla. El comienzo de un nuevo vuelo.",
            11, Muted, -222);

        EditorUtility.SetDirty(overlay.gameObject);
        EditorUtility.SetDirty(card.gameObject);
        EditorSceneManager.MarkSceneDirty(overlay.gameObject.scene);

        Selection.activeGameObject = card.gameObject;

        Debug.Log("[King Lucilia] Panel Victory estilizado.");
    }

    static void StyleButton(
        Transform parent, string name, string label,
        float y, bool primary)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            Debug.LogWarning("No se encontró " + name);
            return;
        }

        Center(t.GetComponent<RectTransform>(), 320, 49, 0, y);

        var image = GetImage(t.gameObject);
        image.color = primary
            ? new Color32(116, 99, 68, 255)
            : Secondary;

        var button = t.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(225, 210, 175, 255);
            colors.pressedColor = new Color32(160, 145, 115, 255);
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            button.colors = colors;

            EditorUtility.SetDirty(button);
        }

        var tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.fontSize = 17;
            tmp.color = Ivory;
            tmp.alignment = TextAlignmentOptions.Center;
            EditorUtility.SetDirty(tmp);
        }
        else
        {
            var text = t.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
                text.fontSize = 17;
                text.color = Ivory;
                text.alignment = TextAnchor.MiddleCenter;
                EditorUtility.SetDirty(text);
            }
        }
    }

    static void CreateLabel(
        Transform parent, string name, string value,
        int size, Color color, float y)
    {
        var existing = parent.Find(name);
        GameObject obj;

        if (existing != null)
        {
            obj = existing.gameObject;
        }
        else
        {
            obj = new GameObject(name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

            obj.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(
                obj, "Crear decoración Victory");
        }

        Center(obj.GetComponent<RectTransform>(), 370, 45, 0, y);

        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = value;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }

    static void SetText(
        GameObject obj, string value, int size, Color color)
    {
        var tmp = obj.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = value;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            EditorUtility.SetDirty(tmp);
            return;
        }

        var text = obj.GetComponent<Text>();
        if (text != null)
        {
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            EditorUtility.SetDirty(text);
        }
    }

    static Image GetImage(GameObject obj)
    {
        var image = obj.GetComponent<Image>();
        if (image == null)
            image = Undo.AddComponent<Image>(obj);
        return image;
    }

    static void DisableLayouts(GameObject obj)
    {
        foreach (var layout in obj.GetComponents<LayoutGroup>())
        {
            Undo.RecordObject(layout, "Desactivar layout Victory");
            layout.enabled = false;
        }

        var fitter = obj.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            Undo.RecordObject(fitter, "Desactivar fitter Victory");
            fitter.enabled = false;
        }
    }

    static void Center(
        RectTransform rect, float w, float h, float x, float y)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estilizar Victory");
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        rect.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rect)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estirar Victory");
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
