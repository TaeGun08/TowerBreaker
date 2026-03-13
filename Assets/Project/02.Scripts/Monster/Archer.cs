using UnityEngine;

public class Archer : Monster
{
    [Header("Archer Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private Transform firePoint;

    private float _lastAttackTime;
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    protected override void Update()
    {
        // Monster의 기본 Update(이동 로직) 호출
        // base.Update()는 MySwarm의 상태를 체크하여 이동을 결정합니다.
        base.Update();

        if (PlayerUnit.Instance == null) return;

        // 현재 활성화된 군집이 아니거나 연출 중이면 공격 안 함
        if (!IsActiveSwarm() || PlayerUnit.Instance.IsTransitioning) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);

        // 사거리 내에 플레이어가 있으면 이동 여부와 관계없이 공격 시도
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
        
        Debug.Log($"{gameObject.name} attacks from the swarm!");
    }
}
