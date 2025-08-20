using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float range = 2.5f;                 // how far the E-ray reaches
    public LayerMask interactMask;             // Items + Doors/Interactables layers
    public PlayerInventory inventory;          // hooked to the rig's inventory
    public PlayerEquipment equipment;          // hooked to the rig's equipment
    public Transform cam;                      // camera used for the ray (defaults to Camera.main)

    void Awake()
    {
        // If not wired in the prefab, grab the main camera at runtime
        if (!cam && Camera.main) cam = Camera.main.transform;
    }

    void Update()
    {
        // Only act on E press, and only if we have a camera to ray from
        if (!Input.GetKeyDown(KeyCode.E) || !cam) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask))
        {
            // Door gets priority: if we hit a door, try to open and we're done
            var door = hit.collider.GetComponentInParent<DoorSceneLoader>();
            if (door)
            {
                door.TryOpen(inventory, equipment);
                return;
            }

            // Otherwise, see if it's a pickable item and grab it
            var pickup = hit.collider.GetComponentInParent<PickupItems>();
            if (pickup)
            {
                pickup.PickUp(inventory);
                return;
            }
        }
    }
}
