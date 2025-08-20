using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Transform))]
public class SceneSpawnPlacer : MonoBehaviour
{
    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // No handoff? Then we don't move the player.
        if (string.IsNullOrEmpty(SceneSpawnRouter.NextSpawnId)) return;

        var wanted = SceneSpawnRouter.NextSpawnId;

        // Look for a SpawnPoint in this scene that matches the id we were handed
        var points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in points)
        {
            if (sp.spawnId == wanted)
            {
                // CharacterController hates teleports while enabled, so toggle it off→on
                var cc = GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                transform.SetPositionAndRotation(sp.transform.position, sp.transform.rotation);
                if (cc) cc.enabled = true;
                break;
            }
        }

        // One-shot mailbox — clear after use
        SceneSpawnRouter.Clear();
    }
}
