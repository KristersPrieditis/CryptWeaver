using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Hand Mounts")]
    public Transform leftHandMount;
    public Transform rightHandMount;

    GameObject _leftInstance;
    GameObject _rightInstance;

    public void Equip(ItemData itemData, HandSide side)
    {
        if (itemData == null) return;

        Transform mount = side == HandSide.Left ? leftHandMount : rightHandMount;

        // Clear existing visual
        foreach (Transform child in mount)
            Destroy(child.gameObject);

        // Spawn visual instance
        var item = Instantiate(itemData.prefab, mount);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // While held: disable physics & colliders so it doesn't float or spam warnings
        // While held: disable physics & colliders so it follows the hand perfectly
        var rbs = item.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None; // avoid visual trailing while parented
        }
        foreach (var col in item.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
        if (side == HandSide.Left) _leftInstance = item; else _rightInstance = item;
    }

    public void Unequip(HandSide side)
    {
        Transform mount = side == HandSide.Left ? leftHandMount : rightHandMount;
        foreach (Transform child in mount)
            Destroy(child.gameObject);

        if (side == HandSide.Left) _leftInstance = null; else _rightInstance = null;
    }
}
