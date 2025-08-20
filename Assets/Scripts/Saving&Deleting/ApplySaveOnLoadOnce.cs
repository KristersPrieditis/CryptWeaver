using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplySaveOnLoadOnce : MonoBehaviour
{
    public bool useSavedPosition = false; // usually false — SpawnPoints handle placement

    void Awake()
    {
        // run once after the next scene load, then self-destruct
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // find the live rig's inventory (player lives in the persistent rig)
        var inv = Object.FindFirstObjectByType<PlayerInventory>();
        if (!inv) { Destroy(gameObject); return; }

        var stats = inv.GetComponent<PlayerStats>();

        // pull data from SQLite into this rig
        var ok = SaveManager.TryLoadIntoPlayer(inv, stats, inv.transform, useSavedPosition);

        // make the visuals match the saved equipped indices (both hands)
        var equip = inv.GetComponent<PlayerEquipment>();
        if (ok && equip)
        {
            if (inv.equippedLeft  >= 0) inv.EquipSlot(inv.equippedLeft,  equip);
            if (inv.equippedRight >= 0) inv.EquipSlot(inv.equippedRight, equip);
        }

        Destroy(gameObject);
    }
}
