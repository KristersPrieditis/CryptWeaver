using UnityEngine;

public enum ItemType { Sword, Shield, Potion, Other }

[CreateAssetMenu(menuName = "MORTIS/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;      // e.g. "key.firstfloor.01"  (UNIQUE, stable)
    public string itemName;    // display name, e.g. "Dungeon Key"

    [Header("Visuals")]
    public Sprite icon;
    public GameObject prefab;

    [Header("Type")]
    public ItemType itemType = ItemType.Other;
}