using UnityEngine;

public interface IDamageable
{
    int CurrentHP { get; }
    void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false);
}

public interface IAttacker
{
    void Attack();
}
