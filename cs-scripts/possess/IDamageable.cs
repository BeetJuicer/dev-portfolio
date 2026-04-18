namespace StateMachineCore
{
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    void RecoverHealth(float amount);
    void Die();

    float Health { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }
}
}