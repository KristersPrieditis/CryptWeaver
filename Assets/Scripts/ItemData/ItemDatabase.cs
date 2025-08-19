using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "MORTIS/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new();

    Dictionary<string, ItemData> _byId;
    Dictionary<string, ItemData> _byName;

    void OnEnable()
    {
        _byId = new Dictionary<string, ItemData>(System.StringComparer.Ordinal);
        _byName = new Dictionary<string, ItemData>(System.StringComparer.Ordinal);

        foreach (var item in items)
        {
            if (!item) continue;
            if (!string.IsNullOrEmpty(item.itemId))   _byId[item.itemId] = item;
            if (!string.IsNullOrEmpty(item.itemName)) _byName[item.itemName] = item;
        }
    }

    public ItemData GetById(string id) =>
        (id != null && _byId.TryGetValue(id, out var d)) ? d : null;

    public ItemData GetByName(string name) =>
        (name != null && _byName.TryGetValue(name, out var d)) ? d : null;

    // Convenience: loads from Resources/ItemDatabase.asset
    public static ItemDatabase LoadDefault() => Resources.Load<ItemDatabase>("ItemDatabase");
}
