using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class KingLuciliaPauseMenuStyler
{
    static readonly Color Ivory = new Color32(231, 220, 198, 255);
    static readonly Color Gold = new Color32(189, 165, 116, 255);
    static readonly Color Dark = new Color32(21, 27, 28, 248);
    static readonly Color Secondary = new Color32(41, 50, 50, 255);

    [MenuItem("Tools/King Lucilia/Estilizar Menu de Pausa")]
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

        Transform overlay = canvas.transform.Find("Panel_Pause");
        Transform card = overlay != null
            ? overlay.Find("Panel_Pause")
            : null;
        Transform content = card != null
            ? card.Find("Menu_Content")
            : null;

        if (content == null)
        {
            Debug.LogError(
                "No se encontró Panel_Pause/Panel_Pause/Menu_Content."
            );
            return;
        }

        // Fondo general.
        Stretch(overlay.GetComponent<RectTransform>());

        Image overlayImage = GetOrAddImage(overlay.gameObject);
        overlayImage.color = new Color(0.025f, 0.035f, 0.04f, 0.88f);
        overlayImage.raycastTarget = true;

        // Tarjeta central.
        RectTransform cardRect = card.GetComponent<RectTransform>();
        Center(cardRect, 400, 440, 0, 0);

        Image cardImage = GetOrAddImage(card.gameObject);
        cardImage.color = Dark;
        cardImage.raycastTarget = true;

        // Contenedor interno.
        RectTransform contentRect = content.GetComponent<RectTransform>();
        Stretch(contentRect);

        // Evitar que layouts automáticos antiguos
        // sobrescriban las posiciones visuales.
        DisableLayouts(card.gameObject);
        DisableLayouts(content.gameObject);

        // Título original.
        Transform title = content.Find("Pause");
        if (title != null)
        {
            Center(title.GetComponent<RectTransform>(),
                340, 55, 0, 122);

            StyleText(title.gameObject, "PAUSA", 33, Ivory);
        }

        // Subtítulo decorativo.
        CreateLabel(
            content,
            "KL_PauseSubtitle",
            "KING LUCILIA",
            19,
            Gold,
            178
        );

        CreateLabel(
            content,
            "KL_PauseLore",
            "EL REINO EN SILENCIO",
            11,
            Gold,
            91
        );

        // Botones existentes: no reemplazamos componentes.
        StyleButton(content, "Btn_Resume",
            "CONTINUAR", 28, true);

        StyleButton(content, "Btn_Restart",
            "REINICIAR", -40, false);

        StyleButton(content, "Btn_Exit",
            "MENÚ PRINCIPAL", -108, false);

        CreateLabel(
            content,
            "KL_PauseFooter",
            "Un reino detenido entre la vida y la muerte",
            12,
            new Color32(151, 156, 145, 255),
            -178
        );

        EditorUtility.SetDirty(overlay.gameObject);
        EditorUtility.SetDirty(card.gameObject);
        EditorUtility.SetDirty(content.gameObject);

        EditorSceneManager.MarkSceneDirty(
            overlay.gameObject.scene
        );

        Selection.activeGameObject = card.gameObject;

        Debug.Log(
            "[King Lucilia] Menú de pausa estilizado. " +
            "Se conservaron los botones existentes."
        );
    }

    static void StyleButton(
        Transform parent,
        string name,
        string label,
        float y,
        bool primary)
    {
        Transform t = parent.Find(name);

        if (t == null)
        {
            Debug.LogWarning("No se encontró " + name);
            return;
        }

        var rect = t.GetComponent<RectTransform>();
        Center(rect, 310, 49, 0, y);

        var image = GetOrAddImage(t.gameObject);
        image.color = primary ? new Color32(116, 99, 68, 255) : Secondary;

        var button = t.GetComponent<Button>();

        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = primary
                ? new Color32(235, 217, 180, 255)
                : new Color32(180, 193, 180, 255);
            colors.pressedColor = new Color32(160, 145, 115, 255);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            EditorUtility.SetDirty(button);
        }

        // Estiliza el texto existente sin reemplazarlo.
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

    static void StyleText(
        GameObject obj,
        string value,
        int size,
        Color color)
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

    static void CreateLabel(
        Transform parent,
        string name,
        string value,
        int fontSize,
        Color color,
        float y)
    {
        Transform existing = parent.Find(name);

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
                obj, "Crear decoración de pausa"
            );
        }

        Center(obj.GetComponent<RectTransform>(),
            350, 35, 0, y);

        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = value;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }

    static Image GetOrAddImage(GameObject obj)
    {
        var image = obj.GetComponent<Image>();
        if (image == null)
            image = Undo.AddComponent<Image>(obj);
        return image;
    }

    static void DisableLayouts(GameObject obj)
    {
        var layouts = obj.GetComponents<LayoutGroup>();
        foreach (var layout in layouts)
        {
            Undo.RecordObject(layout, "Desactivar layout de pausa");
            layout.enabled = false;
        }

        var fitter = obj.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            Undo.RecordObject(fitter, "Desactivar ContentSizeFitter");
            fitter.enabled = false;
        }
    }

    static void Center(
        RectTransform rect,
        float width,
        float height,
        float x,
        float y)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estilizar menú de pausa");

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rect)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estirar panel de pausa");

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
