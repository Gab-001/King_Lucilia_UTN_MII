using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class KingLuciliaMushroomHUD
{
    [MenuItem("Tools/King Lucilia/Crear HUD de Hongos")]
    public static void CreateHUD()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Salí de Play antes de crear el HUD.");
            return;
        }

        var canvas = GameObject.Find("UI_HUD_Canvas");
        if (!canvas)
        {
            Debug.LogError("No se encontró UI_HUD_Canvas.");
            return;
        }

        var existing = canvas.transform.Find("Panel_MushroomCounter");
        if (existing)
        {
            Debug.LogWarning("El contador ya existe. No se duplicó.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var panel = NewUI("Panel_MushroomCounter", canvas.transform);
        SetRect(panel, new Vector2(0, 1),
            new Vector2(30, -140), new Vector2(260, 78));

        var background = panel.gameObject.AddComponent<Image>();
        background.color = new Color32(23, 28, 32, 225);

        var title = NewText("Label_Mushrooms", panel,
            "HONGOS EXTRAÑOS", 15,
            new Vector2(12, -8), new Vector2(180, 24));
        title.color = new Color32(217, 198, 155, 255);

        var counter = NewText("Text_MushroomCount", panel,
            "0 / 3", 17,
            new Vector2(198, -8), new Vector2(50, 24));
        counter.alignment = TextAnchor.MiddleRight;
        counter.color = new Color32(217, 198, 155, 255);

        for (int i = 0; i < 3; i++)
        {
            var slot = NewUI("MushroomSlot_" + (i + 1), panel);

            SetRect(slot, new Vector2(0, 1),
                new Vector2(12 + i * 42, -40),
                new Vector2(32, 28));

            var image = slot.gameObject.AddComponent<Image>();
            image.color = new Color32(52, 59, 59, 255);
        }

        Undo.RegisterCreatedObjectUndo(
            panel.gameObject, "Crear HUD de hongos");

        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        Selection.activeGameObject = panel.gameObject;

        Debug.Log("HUD de hongos creado. Falta conectar el contador al sistema de recolección.");
    }

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void SetRect(
        RectTransform rect, Vector2 anchor,
        Vector2 pos, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }

    private static Text NewText(
        string name, Transform parent,
        string value, int size,
        Vector2 pos, Vector2 dimensions)
    {
        var rect = NewUI(name, parent);
        SetRect(rect, new Vector2(0, 1), pos, dimensions);

        var text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;

        return text;
    }
}
