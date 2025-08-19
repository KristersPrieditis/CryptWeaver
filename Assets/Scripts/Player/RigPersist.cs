using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class RigPersist : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsByType<RigPersist>(FindObjectsSortMode.None);
        if (all.Length > 1) { Destroy(gameObject); return; }

        // Ensure only one EventSystem
        var evts = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        for (int i = 1; i < evts.Length; i++) if (evts[i]) Destroy(evts[i].gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
