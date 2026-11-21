using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class KingLuciliaDialogueStyler
{
    static readonly Color Ivory = new Color32(232, 222, 204, 255);
    static readonly Color Gold = new Color32(201, 166, 110, 255);
    static readonly Color Dark = new Color32(13, 18, 20, 240);

    [MenuItem("Tools/King Lucilia/Estilizar Dialogo Cinematografico")]
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

        var panel = canvas.transform.Find("Dialogue_Cinematic_Panel");

        if (panel == null)
        {
            Debug.LogError(
                "No se encontró Dialogue_Cinematic_Panel."
            );
            return;
        }

        var top = panel.Find("Top_Letterbox");
        var bottom = panel.Find("Bottom_Letterbox");

        if (top == null || bottom == null)
        {
            Debug.LogError("Faltan las bandas cinematográficas.");
            return;
        }

        // Banda superior.
        SetBand(
            top.GetComponent<RectTransform>(),
            true,
            75
        );

        SetBackground(top.gameObject, Dark);

        // Banda inferior.
        SetBand(
            bottom.GetComponent<RectTransform>(),
            false,
            185
        );

        SetBackground(bottom.gameObject, Dark);

        // Nombre del hablante.
        var speaker = bottom.Find("SpeakerName_Text");

        if (speaker != null)
        {
            SetRect(
                speaker.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.65f),
                new Vector2(0.95f, 0.90f)
            );

            var tmp = speaker.GetComponent<TextMeshProUGUI>();

            if (tmp != null)
            {
                Undo.RecordObject(tmp, "Estilizar nombre");

                tmp.fontSize = 24;
                tmp.color = Gold;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.raycastTarget = false;
                EditorUtility.SetDirty(tmp);
            }
        }

        // Texto del diálogo.
        var dialogue = bottom.Find("Dialogue_Text");

        if (dialogue != null)
        {
            SetRect(
                dialogue.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.18f),
                new Vector2(0.95f, 0.66f)
            );

            var tmp = dialogue.GetComponent<TextMeshProUGUI>();

            if (tmp != null)
            {
                Undo.RecordObject(tmp, "Estilizar diálogo");

                tmp.fontSize = 22;
                tmp.color = Ivory;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.enableWordWrapping = true;
                tmp.raycastTarget = false;
                EditorUtility.SetDirty(tmp);
            }
        }

        // Decoración inferior.
        CreateLabel(
            bottom,
            "KL_DialogueHint",
            "EL REINO EN SILENCIO",
            12,
            Gold
        );

        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);

        Selection.activeGameObject = panel.gameObject;

        Debug.Log("[King Lucilia] Diálogo cinematográfico estilizado.");
    }

    static void SetBand(
        RectTransform rect,
        bool top,
        float height)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Estilizar banda cinematográfica");

        rect.anchorMin = top
            ? new Vector2(0, 1)
            : new Vector2(0, 0);

        rect.anchorMax = top
            ? new Vector2(1, 1)
            : new Vector2(1, 0);

        rect.pivot = top
            ? new Vector2(0.5f, 1)
            : new Vector2(0.5f, 0);

        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, height);
        rect.localScale = Vector3.one;
    }

    static void SetBackground(GameObject obj, Color color)
    {
        var image = obj.GetComponent<Image>();

        if (image == null)
            image = Undo.AddComponent<Image>(obj);

        Undo.RecordObject(image, "Color banda cinematográfica");
        image.color = color;
        image.raycastTarget = false;
    }

    static void SetRect(
        RectTransform rect,
        Vector2 min,
        Vector2 max)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Posicionar texto cinematográfico");

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    static void CreateLabel(
        Transform parent,
        string name,
        string value,
        int size,
        Color color)
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
                obj, "Crear decoración diálogo"
            );
        }

        var rect = obj.GetComponent<RectTransform>();

        Undo.RecordObject(rect, "Posicionar decoración");

        rect.anchorMin = new Vector2(0.05f, 0.02f);
        rect.anchorMax = new Vector2(0.95f, 0.16f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var tmp = obj.GetComponent<TextMeshProUGUI>();

        if (tmp != null)
        {
            tmp.text = value;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }
}
