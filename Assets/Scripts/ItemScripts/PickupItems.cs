using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    public void PickUp(PlayerEquipment playerEquipment)
    {
        playerEquipment.TryEquip(itemData);
        Destroy(gameObject); // Remove from scene after pickup
    }
}
