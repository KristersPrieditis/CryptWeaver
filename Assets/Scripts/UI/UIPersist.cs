using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIPersist : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsOfType<UIPersist>(true);
        if (all.Length > 1) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // ensure exactly one EventSystem exists
        var evts = FindObjectsOfType<EventSystem>(true);
        for (int i = 1; i < evts.Length; i++) Destroy(evts[i].gameObject);
    }
}
