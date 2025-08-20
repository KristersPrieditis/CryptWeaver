using UnityEngine;

public class PickupItems : MonoBehaviour
{
    public ItemData itemData; // what this pickup represents

    public void PickUp(PlayerInventory inventory)
    {
        if (inventory != null && itemData != null)
        {
            // try to slot it; first empty wins
            bool ok = inventory.TryAdd(itemData);
            if (ok)
            {
                // success → kill the world instance
                Destroy(gameObject);
            }
            else
            {
                // inventory full → leave it in the world
                // (optional: flash UI / play a deny sfx / show "inventory full")
                Debug.Log("Inventory full.");
            }
        }
    }
}