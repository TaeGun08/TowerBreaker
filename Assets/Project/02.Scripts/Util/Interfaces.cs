using UnityEngine;

public interface IDamageable
{
    int CurrentHP { get; }
    void TakeDamage(int damage, bool isCrit = false);
}

public interface IAttacker
{
    void Attack();
}
