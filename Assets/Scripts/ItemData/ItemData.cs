using UnityEngine;

public enum ItemType { Weapon, Shield, Potion }

[CreateAssetMenu(fileName = "NewItemData", menuName = "Cryptweaver/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public GameObject prefab;     // 3D model to spawn in hand
    public Sprite icon;           // Hotbar UI icon (optional)
}
