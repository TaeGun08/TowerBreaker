using UnityEngine;

public class Archer : Monster
{
    [Header("Archer Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float attackRange = 5.5f; // 아처는 멀리서 쏘도록 범위 설정
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private Transform firePoint;

    private float _lastAttackTime;
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    protected override void Update()
    {
        // 1. 부모 Monster의 기본 이동 로직 수행 (군집 CanMove 상태에 따라 이동)
        base.Update();

        if (PlayerUnit.Instance == null || !IsInActiveSwarm()) return;
        if (PlayerUnit.Instance.IsTransitioning) return;

        // 2. 사거리 내에 플레이어가 있으면 공격 시도
        float distanceToPlayer = transform.position.x - PlayerUnit.Instance.transform.position.x;

        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }
    }

    protected override void HandleMovement()
    {
        // 아처도 근접 몹들과 동일한 거리에서 멈춤
        float stopDist = 0.7f; 
        float distance = transform.position.x - PlayerUnit.Instance.transform.position.x;

        if (distance > stopDist)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
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

        // 투사체 생성 및 방향 설정
        Projectile proj = Instantiate(projectilePrefab, firePoint != null ? firePoint.position : transform.position, Quaternion.identity);
        proj.SetDirection(Vector2.left);
    }
}
