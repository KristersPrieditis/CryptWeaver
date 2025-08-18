using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(int amount, Vector3 point, Vector3 normal);
}
