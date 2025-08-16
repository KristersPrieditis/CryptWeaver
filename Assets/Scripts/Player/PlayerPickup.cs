using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float pickupRange = 2f;
    public LayerMask itemLayer;
    public PlayerInventory inventory; // <-- changed

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                PickupItems item = hit.collider.GetComponent<PickupItems>();
                if (item != null)
                {
                    item.PickUp(inventory); // <-- changed
                }
            }
        }
    }
}