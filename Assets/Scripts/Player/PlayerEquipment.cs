using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public Transform leftHandMount;
    public Transform rightHandMount;

    private GameObject leftHandItem;
    private GameObject rightHandItem;

    public void TryEquip(ItemData itemData)
    {
        if (itemData == null) return;

        Transform targetMount = itemData.itemType == ItemType.Shield || itemData.itemType == ItemType.Potion
            ? leftHandMount
            : rightHandMount;

        // Remove existing item
        foreach (Transform child in targetMount)
            Destroy(child.gameObject);

        // Equip new item
        GameObject item = Instantiate(itemData.prefab, targetMount);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        if (targetMount == leftHandMount)
            leftHandItem = item;
        else
            rightHandItem = item;
    }
}
