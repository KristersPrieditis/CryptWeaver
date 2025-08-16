using UnityEngine;

public enum HandSide { Left, Right }

public class PlayerInventory : MonoBehaviour
{
    // 0–2 = LEFT, 3–5 = RIGHT
    [Header("Inventory")]
    public ItemData[] slots = new ItemData[6];

    [Header("Equip State (slot indices)")]
    public int equippedLeft = -1;
    public int equippedRight = -1;

    [Header("Drop Settings")]
    public Transform dropOrigin;          // usually the camera
    public float dropForwardOffset = 0.9f;
    public float dropUpOffset = 0.1f;
    public float initialThrowSpeed = 0f;  // set 3–6 for a small toss

    public HandSide lastActiveHand { get; private set; } = HandSide.Left;

    public static bool IsLeftSlot(int index) => index >= 0 && index <= 2;
    public static bool IsRightSlot(int index) => index >= 3 && index <= 5;

    public bool TryAdd(ItemData item)
    {
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

    // Called by input to equip a specific slot into the proper hand
    public void EquipSlot(int index, PlayerEquipment equipment)
    {
        if (index < 0 || index >= slots.Length) return;
        var data = slots[index];
        if (data == null) return;

        bool toLeft = IsLeftSlot(index);
        var side = toLeft ? HandSide.Left : HandSide.Right;

        // Unequip current from that hand (does NOT remove from inventory)
        if (toLeft && equippedLeft != -1) equipment.Unequip(HandSide.Left);
        if (!toLeft && equippedRight != -1) equipment.Unequip(HandSide.Right);

        // Equip visual instance for this slot
        equipment.Equip(data, side);

        if (toLeft) equippedLeft = index; else equippedRight = index;
        MarkHandUsed(side);
    }

    // Drop currently equipped item from a hand
    public void DropFromHand(PlayerEquipment equipment, HandSide side)
    {
        int slotIndex = side == HandSide.Left ? equippedLeft : equippedRight;
        if (slotIndex == -1) return;
        var data = slots[slotIndex];
        if (data == null) return;

        // Remove from equipment visuals
        equipment.Unequip(side);

        // Remove from inventory slot
        slots[slotIndex] = null;
        if (side == HandSide.Left) equippedLeft = -1; else equippedRight = -1;

        // Spawn world drop
        var origin = dropOrigin ? dropOrigin : equipment.transform;
        Vector3 pos = origin.position + origin.forward * dropForwardOffset + Vector3.up * dropUpOffset;

        var go = Instantiate(data.prefab, pos, Quaternion.identity);

        // Layer + colliders as you already had...
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer != -1) SetLayerRecursive(go, itemLayer);
        foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = true;

        // Turn real physics back on for ALL rigidbodies in the prefab
        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // nice smooth fall
        }

        // Give the root (or first) rigidbody the toss impulse
        if (rbs.Length > 0)
        {
            rbs[0].linearVelocity = origin.forward * initialThrowSpeed;
            rbs[0].angularVelocity = Random.insideUnitSphere * 2f;
        }

        // Ensure colliders are enabled
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
}
