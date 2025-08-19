using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplySaveOnLoadOnce : MonoBehaviour
{
    public bool useSavedPosition = false; // usually false if you use SpawnPoints

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var inv   = Object.FindFirstObjectByType<PlayerInventory>();
        if (!inv) { Destroy(gameObject); return; }

        var stats = inv.GetComponent<PlayerStats>();
        var ok = SaveManager.TryLoadIntoPlayer(inv, stats, inv.transform, useSavedPosition);

        // Re-equip visuals to match equipped indices
        var equip = inv.GetComponent<PlayerEquipment>();
        if (ok && equip)
        {
            if (inv.equippedLeft  >= 0) inv.EquipSlot(inv.equippedLeft,  equip);
            if (inv.equippedRight >= 0) inv.EquipSlot(inv.equippedRight, equip);
        }

        Destroy(gameObject);
    }
}
