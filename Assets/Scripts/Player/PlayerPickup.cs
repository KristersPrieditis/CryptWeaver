using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float range = 2.5f;
    public LayerMask interactMask;       // include both: Item + Door/Interactable layers
    public PlayerInventory inventory;    // drag from Player
    public PlayerEquipment equipment;    // drag from Player
    public Transform cam;                // drag Main Camera (or it will auto-find)

    void Awake() { if (!cam && Camera.main) cam = Camera.main.transform; }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E) || !cam) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask))
        {
            // 1) Door?
            var door = hit.collider.GetComponentInParent<DoorSceneLoader>();
            if (door) { door.TryOpen(inventory, equipment); return; }

            // 2) Pickup item?
            var pickup = hit.collider.GetComponentInParent<PickupItems>();
            if (pickup)
            {
                // Use the overload your project has:
                // If your PickupItems.PickUp takes inventory:
                pickup.PickUp(inventory);
                // If it takes equipment instead, use:
                // pickup.PickUp(equipment);
                return;
            }
        }
    }
}