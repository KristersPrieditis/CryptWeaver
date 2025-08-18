using System.Collections;
using UnityEngine;

public class SwordItem : MonoBehaviour, IUsableItem
{
    public enum SwingAxis { LocalUp, LocalRight, LocalForward, CustomLocal }

    [Header("Damage")]
    public int damage = 25;

    [Header("Timing")]
    public float swingDuration = 0.28f;
    public float cooldown      = 0.25f;

    [Header("Transforms")]
    public Transform pivot;            // rotate this (e.g., HiltPivot). Defaults to this.transform
    public Collider  hitbox;           // trigger collider along the blade (enabled only during swing)

    [Header("Swing Shape")]
    public SwingAxis swingAxis = SwingAxis.LocalRight; // LocalRight = pitch (downward chop)
    public Vector3   customAxisLocal = new Vector3(0.7f, 0.3f, 0f); // used when SwingAxis=CustomLocal
    public float     backswingAngle = -40f;  // degrees from rest toward "back"
    public float     forwardAngle   =  80f;  // degrees from rest toward "forward"
    public bool      mirrorOnLeftHand = true; // flip direction for left hand
    public bool      invertDirection  = false; // final flip if it still feels wrong

    // internals
    Quaternion _rest;
    bool _swinging, _cooldowning;
    HandSide _currentHand;

    void Awake()
    {
        if (!pivot) pivot = transform;
        if (hitbox) { hitbox.isTrigger = true; hitbox.enabled = false; }
    }

    public void OnUseStart(PlayerItemUser user, HandSide hand)
    {
        if (_swinging || _cooldowning) return;
        _currentHand = hand;

        // capture rest at click-time (after equip snapped to hand)
        _rest = pivot.localRotation;

        StartCoroutine(SwingRoutine());
    }

    public void OnUseEnd(PlayerItemUser user, HandSide hand) { }

    IEnumerator SwingRoutine()
    {
        _swinging = true; _cooldowning = true;
        if (hitbox) hitbox.enabled = true;

        float sign = 1f;
        if (_currentHand == HandSide.Left && mirrorOnLeftHand) sign = -1f;
        if (invertDirection) sign *= -1f;

        Vector3 axisLocal = GetAxisLocal().normalized;
        Quaternion a = _rest * Quaternion.AngleAxis(backswingAngle * sign, axisLocal);
        Quaternion b = _rest * Quaternion.AngleAxis(forwardAngle   * sign, axisLocal);

        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / swingDuration);
            pivot.localRotation = Quaternion.Slerp(a, b, k);
            yield return null;
        }

        pivot.localRotation = _rest;
        if (hitbox) hitbox.enabled = false;

        _swinging = false;
        yield return new WaitForSeconds(cooldown);
        _cooldowning = false;
    }

    Vector3 GetAxisLocal()
    {
        switch (swingAxis)
        {
            case SwingAxis.LocalUp:       return Vector3.up;      // horizontal slash (left↔right)
            case SwingAxis.LocalRight:    return Vector3.right;   // vertical/downward chop (forward feel)
            case SwingAxis.LocalForward:  return Vector3.forward; // roll/twist slash
            case SwingAxis.CustomLocal:   return customAxisLocal;
        }
        return Vector3.right;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hitbox || !hitbox.enabled) return;
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            var cp = other.ClosestPoint(hitbox.bounds.center);
            dmg.ApplyDamage(damage, cp, -transform.forward);
        }
    }
}
