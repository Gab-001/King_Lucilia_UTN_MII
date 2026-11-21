using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using KingLucilia.UI;

public static class KingLuciliaMushroomHUDConnector
{
    [MenuItem("Tools/King Lucilia/Conectar HUD de Hongos")]
    public static void Connect()
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

        var panel = canvas.transform.Find("Panel_MushroomCounter");

        if (panel == null)
        {
            Debug.LogError("No se encontró Panel_MushroomCounter.");
            return;
        }

        var controller =
            panel.GetComponent<MushroomHUDController>();

        if (controller == null)
        {
            Undo.AddComponent<MushroomHUDController>(
                panel.gameObject
            );
        }

        EditorSceneManager.MarkSceneDirty(
            panel.gameObject.scene
        );

        Selection.activeGameObject = panel.gameObject;

        Debug.Log("HUD de hongos conectado correctamente.");
    }
}
