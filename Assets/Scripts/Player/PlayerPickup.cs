using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float pickupRange = 2f;
    public LayerMask itemLayer;
    public PlayerEquipment equipment;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
                PickupItem item = hit.collider.GetComponent<PickupItem>();
                if (item != null)
                {
                    item.PickUp(equipment);
                }
            }
        }
    }
}
