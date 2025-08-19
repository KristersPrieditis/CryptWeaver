using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPersist : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsByType<PlayerPersist>(FindObjectsSortMode.None);
        if (all.Length > 1) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }
}