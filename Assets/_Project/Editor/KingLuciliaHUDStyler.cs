using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class KingLuciliaHUDStyler
{
    [MenuItem("Tools/King Lucilia/Estilizar HUD")]
    public static void StyleHUD()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Salí de Play antes de modificar el HUD.");
            return;
        }

        var canvas = GameObject.Find("UI_HUD_Canvas");
        if (!canvas)
        {
            Debug.LogError("No se encontró UI_HUD_Canvas.");
            return;
        }

        var panel = canvas.transform.Find("Panel_PlayerBars");
        if (!panel)
        {
            Debug.LogError("No se encontró Panel_PlayerBars.");
            return;
        }

        Undo.SetCurrentGroupName("Estilizar HUD King Lucilia");
        int group = Undo.GetCurrentGroup();

        SetRect(panel, new Vector2(0, 1),
            new Vector2(30, -30), new Vector2(280, 95));

        StyleSlider(panel, "Slider_Health",
            new Vector2(0, -5), new Vector2(250, 20),
            new Color32(168, 59, 73, 255));

        StyleSlider(panel, "Slider_Stamina",
            new Vector2(0, -38), new Vector2(230, 14),
            new Color32(90, 154, 112, 255));

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);

        Debug.Log("HUD actualizado: barras y áreas internas ajustadas.");
    }

    private static void SetRect(
        Transform target,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        var rect = target as RectTransform;
        if (!rect) return;

        Undo.RecordObject(rect, "Ajustar HUD");

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
    }

    private static void Stretch(RectTransform rect)
    {
        if (!rect) return;

        Undo.RecordObject(rect, "Expandir área HUD");

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
    }

    private static void StyleSlider(
        Transform panel,
        string name,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        var target = panel.Find(name);
        if (!target) return;

        SetRect(target, new Vector2(0, 1), position, size);

        var slider = target.GetComponent<Slider>();
        if (!slider) return;

        var background = target.Find("Background") as RectTransform;

        if (background)
        {
            Stretch(background);
            SetColor(background.GetComponent<Image>(),
                new Color32(37, 42, 45, 255));
        }

        var fillArea = target.Find("Fill Area") as RectTransform;

        if (fillArea)
        {
            Stretch(fillArea);
        }

        if (slider.fillRect)
        {
            Stretch(slider.fillRect);
            SetColor(slider.fillRect.GetComponent<Image>(), color);
        }

        // No modificamos los valores ni eventos del Slider.
    }

    private static void SetColor(Image image, Color color)
    {
        if (!image) return;

        Undo.RecordObject(image, "Color HUD");
        image.color = color;

        PrefabUtility.RecordPrefabInstancePropertyModifications(image);
    }
}
