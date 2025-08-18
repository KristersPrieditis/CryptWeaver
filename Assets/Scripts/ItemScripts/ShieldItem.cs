using UnityEngine;

public class ShieldItem : MonoBehaviour, IUsableItem
{
    [Header("Animation")]
    public float   raiseLerp = 12f;
    public Vector3 raisedOffsetEuler = new Vector3(25, 0, 0); // rotation added to rest while holding
    public bool    invertOffset = false; // if it moves the wrong way, tick this

    Transform  _t;
    Quaternion _rest;
    bool _active;

    void Awake()
    {
        _t = transform;
        _rest = _t.localRotation; // whatever the prefab’s current pose is = "lowered"
        _active = false;
        _t.localRotation = _rest;
    }

    public void OnUseStart(PlayerItemUser user, HandSide hand)
    {
        _active = true;
        user.stats?.SetBlocking(true, user.blockDamageMultiplier);
    }

    public void OnUseEnd(PlayerItemUser user, HandSide hand)
    {
        _active = false;
        user.stats?.SetBlocking(false);
    }

    void Update()
    {
        var offset = Quaternion.Euler(invertOffset ? -raisedOffsetEuler : raisedOffsetEuler);
        var target = _active ? _rest * offset : _rest;
        _t.localRotation = Quaternion.Slerp(_t.localRotation, target, Time.deltaTime * raiseLerp);
    }
}
