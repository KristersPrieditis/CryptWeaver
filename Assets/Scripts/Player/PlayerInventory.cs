using UnityEngine;

public enum HandSide { Left, Right }

public class PlayerInventory : MonoBehaviour
{
    // slots 0–2 = LEFT hand, 3–5 = RIGHT hand
    [Header("Inventory")]
    public ItemData[] slots = new ItemData[6];

    [Header("Equip State (slot indices)")]
    public int equippedLeft = -1;   // which left slot is in hand, -1 = none
    public int equippedRight = -1;  // which right slot is in hand, -1 = none

    [Header("Drop Settings")]
    public Transform dropOrigin;          // usually the camera
    public float dropForwardOffset = 0.9f;
    public float dropUpOffset = 0.1f;
    public float initialThrowSpeed = 0f;  // set ~3–6 for a small toss

    public HandSide lastActiveHand { get; private set; } = HandSide.Left;

    public static bool IsLeftSlot(int index)  => index >= 0 && index <= 2;
    public static bool IsRightSlot(int index) => index >= 3 && index <= 5;

    public bool TryAdd(ItemData item)
    {
        // first empty slot wins
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                return true;
            }
        }
        return false;
    }

    public void MarkHandUsed(HandSide side) => lastActiveHand = side;

    // Equip a specific slot into the correct hand (doesn't remove from inventory)
    public void EquipSlot(int index, PlayerEquipment equipment)
    {
        if (index < 0 || index >= slots.Length) return;
        var data = slots[index];
        if (data == null) return;

        bool toLeft = IsLeftSlot(index);
        var side = toLeft ? HandSide.Left : HandSide.Right;

        // clear whatever was in that hand
        if (toLeft && equippedLeft != -1)   equipment.Unequip(HandSide.Left);
        if (!toLeft && equippedRight != -1) equipment.Unequip(HandSide.Right);

        // spawn visuals under the hand mount
        equipment.Equip(data, side);

        if (toLeft) equippedLeft = index; else equippedRight = index;
        MarkHandUsed(side);
    }

    // Drop whatever is currently equipped from a given hand
    public void DropFromHand(PlayerEquipment equipment, HandSide side)
    {
        int slotIndex = side == HandSide.Left ? equippedLeft : equippedRight;
        if (slotIndex == -1) return;
        var data = slots[slotIndex];
        if (data == null) return;

        // remove visuals from the hand
        equipment.Unequip(side);

        // free the slot
        slots[slotIndex] = null;
        if (side == HandSide.Left) equippedLeft = -1; else equippedRight = -1;

        // spawn a world copy in front of the camera (or player)
        var origin = dropOrigin ? dropOrigin : equipment.transform;
        Vector3 pos = origin.position + origin.forward * dropForwardOffset + Vector3.up * dropUpOffset;

        var go = Instantiate(data.prefab, pos, Quaternion.identity);

        // make sure it’s on the Item layer (if you have one)
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer != -1) SetLayerRecursive(go, itemLayer);

        // colliders back on for world physics
        foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = true;

        // re-enable physics for all rigidbodies in the prefab
        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother fall
        }

        // give the root (or first) rigidbody a small shove
        if (rbs.Length > 0)
        {
            rbs[0].linearVelocity = origin.forward * initialThrowSpeed;
            rbs[0].angularVelocity = Random.insideUnitSphere * 2f;
        }

        // belt and suspenders: make sure colliders are enabled
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            c.enabled = true;
    }

    public void DropLastUsed(PlayerEquipment equipment)
    {
        DropFromHand(equipment, lastActiveHand);
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    // For consumables: remove from hand + clear the slot
    public void ConsumeFromHand(PlayerEquipment equipment, HandSide side)
    {
        int slotIndex = side == HandSide.Left ? equippedLeft : equippedRight;
        if (slotIndex == -1) return;
        equipment.Unequip(side);
        slots[slotIndex] = null;
        if (side == HandSide.Left) equippedLeft = -1; else equippedRight = -1;
    }
}