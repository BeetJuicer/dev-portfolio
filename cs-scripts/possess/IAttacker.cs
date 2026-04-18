namespace StateMachineCore
{
using System;
public interface IAttacker
{
    AttackData attackData { get; }
    void SetAttackData(AttackData attackData);
    void StartAttack();
    void StopAttack();
    void InflictDamage();
    event Action OnAttackComplete;
}

}