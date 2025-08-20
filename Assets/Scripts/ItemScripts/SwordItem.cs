using System.Collections;
using UnityEngine;

public class SwordItem : MonoBehaviour, IUsableItem
{
    public enum SwingAxis { LocalUp, LocalRight, LocalForward, CustomLocal }

    [Header("Damage")]
    public int damage = 25;                      // how hard this thing hits

    [Header("Timing")]
    public float swingDuration = 0.28f;          // time from back → forward
    public float cooldown      = 0.25f;          // small lockout after a swing

    [Header("Transforms")]
    public Transform pivot;                      // what rotates (usually a hilt pivot). Defaults to self
    public Collider  hitbox;                     // trigger along the blade, only on while swinging

    [Header("Swing Shape")]
    public SwingAxis swingAxis = SwingAxis.LocalRight; // LocalRight = “downward chop” feel
    public Vector3   customAxisLocal = new Vector3(0.7f, 0.3f, 0f); // used if SwingAxis=CustomLocal
    public float     backswingAngle = -40f;      // degrees from rest toward “back”
    public float     forwardAngle   =  80f;      // degrees from rest toward “through the target”
    public bool      mirrorOnLeftHand = true;    // flip for left-hand use
    public bool      invertDirection  = false;   // final flip if it still feels wrong

    // runtime
    Quaternion _rest;
    bool _swinging, _cooldowning;
    HandSide _currentHand;

    void Awake()
    {
        // sane defaults if I forgot to wire anything
        if (!pivot) pivot = transform;
        if (hitbox) { hitbox.isTrigger = true; hitbox.enabled = false; }
    }

    public void OnUseStart(PlayerItemUser user, HandSide hand)
    {
        // no mashing: either mid-swing or cooling down
        if (_swinging || _cooldowning) return;

        _currentHand = hand;
        _rest = pivot.localRotation;   // capture current rest rotation at click time
        StartCoroutine(SwingRoutine());
    }

    public void OnUseEnd(PlayerItemUser user, HandSide hand) { /* no hold action for sword */ }

    IEnumerator SwingRoutine()
    {
        _swinging = true; _cooldowning = true;
        if (hitbox) hitbox.enabled = true;

        // figure out which way we’re rotating
        float sign = 1f;
        if (_currentHand == HandSide.Left && mirrorOnLeftHand) sign = -1f;
        if (invertDirection) sign *= -1f;

        // build start/end rotations around the chosen local axis
        Vector3 axisLocal = GetAxisLocal().normalized;
        Quaternion a = _rest * Quaternion.AngleAxis(backswingAngle * sign, axisLocal);
        Quaternion b = _rest * Quaternion.AngleAxis(forwardAngle   * sign, axisLocal);

        // ease from back → forward
        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / swingDuration);
            pivot.localRotation = Quaternion.Slerp(a, b, k);
            yield return null;
        }

        // done: reset pose, turn off blade hits, start cooldown
        pivot.localRotation = _rest;
        if (hitbox) hitbox.enabled = false;

        _swinging = false;
        yield return new WaitForSeconds(cooldown);
        _cooldowning = false;
    }

    Vector3 GetAxisLocal()
    {
        // choose which local axis we rotate around
        switch (swingAxis)
        {
            case SwingAxis.LocalUp:       return Vector3.up;      // side slash (left↔right)
            case SwingAxis.LocalRight:    return Vector3.right;   // vertical/downward chop
            case SwingAxis.LocalForward:  return Vector3.forward; // twist/roll slash
            case SwingAxis.CustomLocal:   return customAxisLocal;
        }
        return Vector3.right;
    }

    void OnTriggerEnter(Collider other)
    {
        // only land hits during the window
        if (!hitbox || !hitbox.enabled) return;

        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            var cp = other.ClosestPoint(hitbox.bounds.center);
            dmg.ApplyDamage(damage, cp, -transform.forward);
        }
    }
}
