using UnityEngine;

public class PickupItems : MonoBehaviour
{
    public ItemData itemData;

    public void PickUp(PlayerInventory inventory)
    {
        if (inventory != null && itemData != null)
        {
            bool ok = inventory.TryAdd(itemData);
            if (ok)
            {
                Destroy(gameObject); // picked up -> remove world instance
            }
            // else: inventory full -> keep in world (optionally show a message)
        }
    }
}
