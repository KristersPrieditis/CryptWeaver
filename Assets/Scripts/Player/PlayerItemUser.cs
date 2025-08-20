using UnityEngine;

[RequireComponent(typeof(PlayerEquipment))]
public class PlayerItemUser : MonoBehaviour
{
    public PlayerEquipment equipment;       // hand mounts / equipped instances
    public PlayerInventory inventory;       // 6-slot hotbar state
    public PlayerStats stats;               // health
    public Transform cameraTransform;       // used for item forward (defaults to Camera.main)
    public UIOverlayToggle overlayToggle;   // if overlay is up (Tab), ignore clicks

    [Header("Shield")]
    [Range(0f, 1f)] public float blockDamageMultiplier = 0.4f; // damage taken while blocking

    void Awake()
    {
        // Auto-wire if I forgot to hook these in the prefab
        if (!equipment)        equipment = GetComponent<PlayerEquipment>();
        if (!inventory)        inventory = GetComponent<PlayerInventory>();
        if (!stats)            stats     = GetComponent<PlayerStats>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Don’t swing/block while the overlay is open
        if (overlayToggle && overlayToggle.IsOpen) return;

        // Input map (intentional):
        // LMB -> RIGHT hand
        if (Input.GetMouseButtonDown(0)) UseStart(HandSide.Right);
        if (Input.GetMouseButtonUp(0))   UseEnd(HandSide.Right);

        // RMB -> LEFT hand
        if (Input.GetMouseButtonDown(1)) UseStart(HandSide.Left);
        if (Input.GetMouseButtonUp(1))   UseEnd(HandSide.Left);
    }

    public void UseStart(HandSide side)
    {
        var inst = equipment.GetInstance(side);
        if (!inst) return;

        // Find IUsableItem even if it lives on a disabled child
        var usable =
            inst.GetComponent<IUsableItem>() ??
            inst.GetComponentInChildren<IUsableItem>(true);

        if (usable == null)
        {
            Debug.LogWarning($"No IUsableItem found on equipped {inst.name} ({side} hand).");
            return;
        }
        usable.OnUseStart(this, side);
    }

    public void UseEnd(HandSide side)
    {
        var inst = equipment.GetInstance(side);
        if (!inst) return;

        var usable =
            inst.GetComponent<IUsableItem>() ??
            inst.GetComponentInChildren<IUsableItem>(true);

        usable?.OnUseEnd(this, side);
    }

    // Helper for items that need a forward vector (sword swing, etc.)
    public Vector3 Forward => cameraTransform ? cameraTransform.forward : transform.forward;
}