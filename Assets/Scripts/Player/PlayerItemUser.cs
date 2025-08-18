using UnityEngine;

[RequireComponent(typeof(PlayerEquipment))]
public class PlayerItemUser : MonoBehaviour
{
    public PlayerEquipment equipment;
    public PlayerInventory inventory;
    public PlayerStats stats;
    public Transform cameraTransform;     // assign Main Camera
    public UIOverlayToggle overlayToggle; // optional: so clicks don't fire when Tab overlay is open

    [Header("Shield")]
    [Range(0f, 1f)] public float blockDamageMultiplier = 0.4f;

    void Awake()
    {
        if (!equipment) equipment = GetComponent<PlayerEquipment>();
        if (!inventory) inventory = GetComponent<PlayerInventory>();
        if (!stats)     stats     = GetComponent<PlayerStats>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Skip input while overlay (Tab) is open
        if (overlayToggle && overlayToggle.IsOpen) return;

        // SWAPPED mapping:
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

        var usable =
            inst.GetComponent<IUsableItem>() ??
            inst.GetComponentInChildren<IUsableItem>(true);  // <-- includeInactive

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
            inst.GetComponentInChildren<IUsableItem>(true);  // <-- includeInactive

        usable?.OnUseEnd(this, side);
    }

    public Vector3 Forward => cameraTransform ? cameraTransform.forward : transform.forward;
}