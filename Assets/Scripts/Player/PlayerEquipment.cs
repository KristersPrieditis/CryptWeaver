using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Hand Mounts")]
    public Transform leftHandMount;
    public Transform rightHandMount;

    private GameObject _leftInstance;
    private GameObject _rightInstance;

    public GameObject GetInstance(HandSide side) =>
        side == HandSide.Left ? _leftInstance : _rightInstance;

    public void Equip(ItemData itemData, HandSide side)
    {
        if (itemData == null || itemData.prefab == null)
        {
            Debug.LogWarning("Equip called with null ItemData or prefab.");
            return;
        }

        Transform mount = side == HandSide.Left ? leftHandMount : rightHandMount;
        if (!mount)
        {
            Debug.LogError("Hand mount not assigned on PlayerEquipment.");
            return;
        }

        // Clear current item on that hand
        foreach (Transform child in mount)
            Destroy(child.gameObject);

        // Spawn and snap to mount
        var item = Instantiate(itemData.prefab, mount);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // While held: disable physics & colliders so it follows perfectly.
        ConfigureEquipped(item);

        if (side == HandSide.Left) _leftInstance = item; else _rightInstance = item;
    }

    public void Unequip(HandSide side)
    {
        Transform mount = side == HandSide.Left ? leftHandMount : rightHandMount;
        if (!mount) return;

        foreach (Transform child in mount)
            Destroy(child.gameObject);

        if (side == HandSide.Left) _leftInstance = null; else _rightInstance = null;
    }

    /// <summary>
    /// Disable physics on an equipped item so it stays glued to the hand.
    /// Individual item scripts (e.g., SwordItem) can enable their own hitboxes temporarily.
    /// </summary>
    private void ConfigureEquipped(GameObject go)
    {
        if (!go) return;

        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;        // <-- correct property
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        var cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
            col.enabled = false; // weapon scripts will enable their specific hitbox when needed
    }
}
