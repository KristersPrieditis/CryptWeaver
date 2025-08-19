using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Transform))]
public class SceneSpawnPlacer : MonoBehaviour
{
    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(SceneSpawnRouter.NextSpawnId)) return;

        var wanted = SceneSpawnRouter.NextSpawnId;
        var points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in points)
        {
            if (sp.spawnId == wanted)
            {
                var cc = GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                transform.SetPositionAndRotation(sp.transform.position, sp.transform.rotation);
                if (cc) cc.enabled = true;
                break;
            }
        }
        SceneSpawnRouter.Clear();
    }
}
