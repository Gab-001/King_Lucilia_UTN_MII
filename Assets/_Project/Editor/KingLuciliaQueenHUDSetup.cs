using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using KingLucilia.UI;

public static class KingLuciliaQueenHUDSetup
{
    [MenuItem("Tools/King Lucilia/Crear Objetivo QueenB")]
    public static void Create()
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

        var existing = canvas.transform.Find("Panel_QueenObjective");

        if (existing != null)
        {
            Debug.Log("El panel de QueenB ya existe.");
            return;
        }

        var panel = new GameObject(
            "Panel_QueenObjective",
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(canvas.transform, false);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(30, -230);
        rect.sizeDelta = new Vector2(320, 48);

        panel.GetComponent<Image>().color =
            new Color32(23, 28, 32, 225);

        var textObject = new GameObject(
            "Text_QueenBlessing",
            typeof(RectTransform),
            typeof(Text)
        );

        textObject.transform.SetParent(panel.transform, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12, 5);
        textRect.offsetMax = new Vector2(-12, -5);

        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;

        panel.AddComponent<QueenBlessingHUD>();

        Undo.RegisterCreatedObjectUndo(
            panel, "Crear objetivo QueenB"
        );

        EditorSceneManager.MarkSceneDirty(panel.scene);
        Selection.activeGameObject = panel;

        Debug.Log("Objetivo de QueenB creado correctamente.");
    }
}
