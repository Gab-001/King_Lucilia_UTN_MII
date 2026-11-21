using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class KingLuciliaHUDReadabilityFix
{
    [MenuItem("Tools/King Lucilia/Corregir Legibilidad HUD")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Salí de Play antes de aplicar.");
            return;
        }

        GameObject canvasObject = GameObject.Find("UI_HUD_Canvas");

        if (canvasObject == null)
        {
            Debug.LogError("No se encontró UI_HUD_Canvas.");
            return;
        }

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();

        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Ajustar Canvas Scaler");

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EditorUtility.SetDirty(scaler);
        }

        int updated = 0;

        foreach (TextMeshProUGUI tmp in
                 canvasObject.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            string path = GetPath(tmp.transform).ToLowerInvariant();

            bool mushroom =
                path.Contains("mushroom") ||
                path.Contains("hongo");

            bool queen =
                path.Contains("queen") ||
                path.Contains("blessing");

            bool health =
                path.Contains("health") ||
                path.Contains("stamina");

            if (!mushroom && !queen && !health)
                continue;

            Undo.RecordObject(tmp, "Mejorar legibilidad HUD");

            tmp.enableAutoSizing = false;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = mushroom ? 24 : queen ? 22 : 20;
            tmp.color = new Color32(242, 232, 211, 255);
            tmp.raycastTarget = false;

            // Evita compresión horizontal de caracteres.
            tmp.characterSpacing = 0;
            tmp.wordSpacing = 0;

            RectTransform rect = tmp.rectTransform;

            Undo.RecordObject(rect, "Ajustar caja de texto HUD");

            // Conservamos las anclas y posiciones originales.
            // Solo garantizamos una altura mínima.
            if (rect.rect.height < 30f)
            {
                Vector2 size = rect.sizeDelta;
                size.y += 30f - rect.rect.height;
                rect.sizeDelta = size;
            }

            EditorUtility.SetDirty(tmp);
            updated++;
        }

        EditorSceneManager.MarkSceneDirty(canvasObject.scene);

        Debug.Log(
            $"[King Lucilia] Legibilidad mejorada en {updated} textos."
        );
    }

    private static string GetPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
