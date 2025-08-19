using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;

    [Header("New Game target")]
    [SerializeField] private string firstGameSceneName = "FirstRoom";
    [SerializeField] private string firstSpawnId = "StartSpawn"; // <- define it here

    void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(HandleContinue);
        if (newGameButton)  newGameButton.onClick.AddListener(HandleNewGame);
        if (quitButton)     quitButton.onClick.AddListener(HandleQuit);

        bool hasSave = SaveManager.LoadProgress(out _, out _);
        if (continueButton) continueButton.interactable = hasSave;
    }

    void HandleContinue()
    {
        if (!SaveManager.LoadProgress(out var scene, out var spawn))
        {
            HandleNewGame(); // fallback
            return;
        }

        DestroyPersistents();
        if (!string.IsNullOrEmpty(spawn)) SceneSpawnRouter.SetNext(spawn);
        SceneManager.LoadScene(scene);
    }

    void HandleNewGame()
    {
        SaveManager.DeleteSave();
        DestroyPersistents();
        if (!string.IsNullOrEmpty(firstSpawnId)) SceneSpawnRouter.SetNext(firstSpawnId);
        SceneManager.LoadScene(firstGameSceneName);
    }

    void HandleQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void DestroyPersistents()
    {
        var toDestroy = new List<GameObject>();

        var players = Object.FindObjectsByType<PlayerPersist>(FindObjectsSortMode.None);
        foreach (var p in players) if (p) toDestroy.Add(p.gameObject);

        var uis = Object.FindObjectsByType<UIPersist>(FindObjectsSortMode.None);
        foreach (var u in uis) if (u) toDestroy.Add(u.gameObject);

        // Clear next-spawn handoff
        SceneSpawnRouter.Clear();

#if UNITY_EDITOR
        // If the Inspector is selecting one of these, deselect to avoid editor exceptions
        if (UnityEditor.Selection.activeGameObject &&
            toDestroy.Contains(UnityEditor.Selection.activeGameObject))
        {
            UnityEditor.Selection.activeGameObject = null;
        }
        // Also clear any active Object selection just in case
        UnityEditor.Selection.activeObject = null;
#endif

        // Destroy at end of frame (safe in play mode)
        foreach (var go in toDestroy) if (go) Object.Destroy(go);
    }
}
