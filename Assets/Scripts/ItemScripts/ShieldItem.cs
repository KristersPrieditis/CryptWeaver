using UnityEngine;

public class ShieldItem : MonoBehaviour, IUsableItem
{
    [Header("Animation")]
    public float   raiseLerp = 12f;                       // how fast it eases up/down
    public Vector3 raisedOffsetEuler = new Vector3(25, 0, 0); // extra rotation while holding block
    public bool    invertOffset = false;                  // flip if it tilts the wrong way

    Transform  _t;
    Quaternion _rest;
    bool _active;

    void Awake()
    {
        _t = transform;
        _rest = _t.localRotation;   // prefab pose = “lowered” baseline
        _active = false;
        _t.localRotation = _rest;
    }

    public void OnUseStart(PlayerItemUser user, HandSide hand)
    {
        _active = true;                                 // start raising
        user.stats?.SetBlocking(true, user.blockDamageMultiplier); // cut damage while held
    }

    public void OnUseEnd(PlayerItemUser user, HandSide hand)
    {
        _active = false;                                // start lowering
        user.stats?.SetBlocking(false);
    }

    void Update()
    {
        // ease toward raised or rest
        var offset = Quaternion.Euler(invertOffset ? -raisedOffsetEuler : raisedOffsetEuler);
        var target = _active ? _rest * offset : _rest;
        _t.localRotation = Quaternion.Slerp(_t.localRotation, target, Time.deltaTime * raiseLerp);
    }
}