using System;
using System.Collections;
using UnityEngine;

public class PlayerUnit : SingletonBase<PlayerUnit>
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Combat Config")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.3f; 

    [Header("Dash Config")]
    [SerializeField] private float dashDistance = 0.5f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashStopThreshold = 0.3f; 
    
    [Header("Guard Config")]
    [SerializeField] private float guardPushDistance = 1.0f; 
    [SerializeField] private float playerKnockbackDistance = 0.3f; 
    [SerializeField] private float guardDuration = 0.2f;

    [Header("Visual Config")]
    [SerializeField] private Vector3 startPosition = new Vector3(-1.4f, -0.1f, 0f);

    private bool _isDashing;
    private bool _isAttacking;
    private bool _isGuarding;
    private bool _isTransitioning;
    
    public bool IsTransitioning => _isTransitioning;

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
    }

    private void Start()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageProgress += RespawnAtStart;
        }
    }

    protected override void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageProgress -= RespawnAtStart;
        }
        base.OnDestroy();
    }

    private void LateUpdate()
    {
        if (!_isTransitioning) ClampPosition();
    }

    private void ClampPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float screenHalfWidth = cam.orthographicSize * cam.aspect;
        float limitX = screenHalfWidth * 0.8f;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;
    }

    #region Input Actions
    
    public void PerformAttack()
    {
        if (CanInput() == false) return;
        StartCoroutine(AttackCoroutine());
    }

    public void PerformDash()
    {
        if (CanInput() == false) return;
        StartCoroutine(DashCoroutine());
    }

    public void PerformGuard()
    {
        if (CanInput() == false) return;
        StartCoroutine(GuardCoroutine());
    }

    private bool CanInput() => !_isDashing && !_isAttacking && !_isGuarding && !_isTransitioning;

    private Swarm GetCurrentSwarm() => StageManager.Instance?.CurrentSwarm;

    #endregion

    #region Combat Coroutines
    
    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("2_Attack");
            yield return null;

            // 애니메이션 진행도 체크 (70% 시점에서 타격 판정)
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
            {
                if (animator.IsInTransition(0)) yield return null;
                else yield return null;
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f) break;
            }
        }

        // 실제 타격 판정 및 연출 연동
        Swarm swarm = GetCurrentSwarm();
        if (swarm != null)
        {
            bool hitSuccess = swarm.AttackInRange(transform.position.x, attackRange, attackDamage);
            if (hitSuccess) TriggerCombatJuice(0.05f, 0.08f, 0.05f); // 타격 성공 피드백
        }

        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
    }

    private IEnumerator DashCoroutine()
    {
        _isDashing = true;
        if (animator != null) animator.SetTrigger("1_Move");

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * dashDistance;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, t);

            if (IsMonsterAhead(nextPos)) break; 

            transform.position = nextPos;
            yield return null;
        }

        _isDashing = false;
    }

    private IEnumerator GuardCoroutine()
    {
        Swarm swarm = GetCurrentSwarm();
        if (swarm == null) yield break;

        Monster frontMonster = swarm.GetFrontMonster();
        if (frontMonster == null) yield break;

        float distance = frontMonster.transform.position.x - transform.position.x;
        if (distance > attackRange + 0.2f) yield break;

        _isGuarding = true;

        // 가드 피드백 (화면 흔들림 분리 호출)
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.05f, 0.05f);
        swarm.Knockback(guardPushDistance, guardDuration);

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.left * playerKnockbackDistance;

        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / guardDuration;
            float ease = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }
        transform.position = targetPos;

        _isGuarding = false;
    }

    /// <summary>
    /// 타격 시 발생하는 시청각 피드백(Juice)을 트리거합니다.
    /// </summary>
    private void TriggerCombatJuice(float stopDuration, float shakeIntensity, float shakeDuration)
    {
        if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(stopDuration);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(shakeIntensity, shakeDuration);
    }

    private bool IsMonsterAhead(Vector3 checkPos)
    {
        Swarm swarm = GetCurrentSwarm();
        if (swarm == null) return false;

        Monster frontMonster = swarm.GetFrontMonster();
        if (frontMonster == null) return false;

        return (frontMonster.transform.position.x - checkPos.x) <= dashStopThreshold;
    }

    #endregion

    #region Floor Transitions
    
    public void MoveToNextFloorSequence(Action onComplete)
    {
        if (_isTransitioning) return;
        StartCoroutine(MoveRightAndExit(onComplete));
    }

    private IEnumerator MoveRightAndExit(Action onComplete)
    {
        _isTransitioning = true;
        float duration = 0.5f; 
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(8.0f, transform.position.y, transform.position.z); 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        onComplete?.Invoke();
    }

    private void RespawnAtStart() => StartCoroutine(RespawnAtNewFloor());

    private IEnumerator RespawnAtNewFloor()
    {
        transform.position = new Vector3(-8.0f, startPosition.y, startPosition.z);
        
        float duration = 0.4f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        _isTransitioning = false;
    }

    #endregion
}
