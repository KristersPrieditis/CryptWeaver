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
    [SerializeField] private string firstSpawnId = "StartSpawn";

    [Header("Persistent Rig (Player+UI prefab)")]
    [SerializeField] private GameObject persistentRigPrefab; // root has RigPersist; child Player has SceneSpawnPlacer

    void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(HandleContinue);
        if (newGameButton)  newGameButton.onClick.AddListener(HandleNewGame);
        if (quitButton)     quitButton.onClick.AddListener(HandleQuit);

        bool hasSave = SaveManager.LoadProgress(out _, out _);
        if (continueButton) continueButton.interactable = hasSave;
    }

    // ===== Buttons =====

    void HandleContinue()
    {
        if (!SaveManager.LoadProgress(out var scene, out var spawn))
        {
            HandleNewGame();
            return;
        }

        EnsureRigExists();                           // make sure Player+UI rig exists
        if (!string.IsNullOrEmpty(spawn))
            SceneSpawnRouter.SetNext(spawn);

        // Apply save to the rig after the scene loads
        var applier = new GameObject("ApplySaveOnce").AddComponent<ApplySaveOnLoadOnce>();
        applier.useSavedPosition = false;            // true = use exact saved coords; false = use SpawnPoint

#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
        SceneManager.LoadScene(scene);
    }

    void HandleNewGame()
    {
        SaveManager.DeleteSave();                    // wipe old progress
        CreateRigFresh();                            // destroy old rig (if any) + spawn a fresh one

        if (!string.IsNullOrEmpty(firstSpawnId))
            SceneSpawnRouter.SetNext(firstSpawnId);

#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
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

    // ===== Rig helpers =====

    static bool HasRig() =>
        Object.FindObjectsByType<RigPersist>(FindObjectsSortMode.None).Length > 0;

    void EnsureRigExists()
    {
        if (HasRig()) return;
        if (!persistentRigPrefab)
        {
            Debug.LogError("MainMenuController: persistentRigPrefab not assigned.");
            return;
        }
        var go = Instantiate(persistentRigPrefab);
        go.name = "MortisPersistentRig";
    }

    void CreateRigFresh()
    {
        DestroyRigIfAny();
        EnsureRigExists();
    }

    static void DestroyRigIfAny()
    {
        var toDestroy = new List<GameObject>();
        foreach (var r in Object.FindObjectsByType<RigPersist>(FindObjectsSortMode.None))
            if (r) toDestroy.Add(r.gameObject);

        SceneSpawnRouter.Clear();

#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject &&
            toDestroy.Contains(UnityEditor.Selection.activeGameObject))
            UnityEditor.Selection.activeGameObject = null;
        UnityEditor.Selection.activeObject = null;
#endif
        foreach (var go in toDestroy) if (go) Object.Destroy(go);
    }
}
