using UnityEngine;

public class Archer : Monster
{
    [Header("Archer Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float attackRange = 5.5f; 
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private Transform firePoint;

    private float _lastAttackTime;
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    
    public override float StopDistance => 0.7f;

    protected override void Update()
    {
        
        base.Update();

        if (PlayerUnit.Instance == null || !IsInActiveSwarm()) return;
        if (PlayerUnit.Instance.IsTransitioning) return;

        
        float distanceToPlayer = transform.position.x - PlayerUnit.Instance.transform.position.x;
        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        if (projectilePrefab == null) return;

        Projectile proj = Instantiate(projectilePrefab, firePoint != null ? firePoint.position : transform.position, Quaternion.identity);
        proj.SetDirection(Vector2.left);
    }
}

