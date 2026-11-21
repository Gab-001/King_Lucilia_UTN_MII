using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KingLuciliaPlayFromHere
{
    private const string MenuPath =
        "Tools/King Lucilia/Play From Here";

    private static Vector3 selectedPosition;
    private static bool hasPosition;

    private static GameObject player;
    private static Vector3 originalPosition;
    private static Quaternion originalRotation;
    private static bool pendingRestore;

    static KingLuciliaPlayFromHere()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (e.type != EventType.ContextClick)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(
            e.mousePosition
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            10000f,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Vector3 clickedPosition = hit.point;

        GenericMenu menu = new GenericMenu();

        menu.AddItem(
            new GUIContent("Play From Here"),
            false,
            () =>
            {
                selectedPosition = clickedPosition;
                hasPosition = true;
                StartPlayFromHere();
            }
        );

        menu.ShowAsContext();
        e.Use();
    }

    [MenuItem(MenuPath)]
    private static void StartPlayFromHere()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (!hasPosition)
        {
            Debug.LogWarning(
                "Primero elegí un punto del terreno con clic derecho en Scene."
            );
            return;
        }

        var controller =
            Object.FindFirstObjectByType<
                KingLucilia.Player.KingLuciliaController
            >();

        if (controller == null)
        {
            Debug.LogError(
                "No se encontró KingLuciliaController en la escena."
            );
            return;
        }

        player = controller.gameObject;

        originalPosition = player.transform.position;
        originalRotation = player.transform.rotation;

        // Separación pequeña para evitar que aparezca
        // parcialmente dentro del terreno.
        Vector3 spawnPosition =
            selectedPosition + Vector3.up * 0.5f;

        Undo.RecordObject(
            player.transform,
            "Play From Here"
        );

        player.transform.position = spawnPosition;

        pendingRestore = true;

        Debug.Log(
            "[Play From Here] Iniciando desde " +
            spawnPosition
        );

        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(
        PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode &&
            pendingRestore)
        {
            if (player != null)
            {
                player.transform.position = originalPosition;
                player.transform.rotation = originalRotation;
            }

            pendingRestore = false;
            player = null;
            hasPosition = false;

            Debug.Log(
                "[Play From Here] Posición original restaurada."
            );
        }
    }
}
