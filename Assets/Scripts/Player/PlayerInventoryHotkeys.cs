using UnityEngine;

public class PlayerInventoryHotkeys : MonoBehaviour
{
    public PlayerInventory inventory;
    public PlayerEquipment equipment;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.EquipSlot(0, equipment);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.EquipSlot(1, equipment);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.EquipSlot(2, equipment);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.EquipSlot(3, equipment);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.EquipSlot(4, equipment);
        if (Input.GetKeyDown(KeyCode.Alpha6)) inventory.EquipSlot(5, equipment);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventory.DropLastUsed(equipment);
        }
    }
}
