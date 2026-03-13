using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VisualFeedback))]
public class PlayerUnit : SingletonBase<PlayerUnit>, IDamageable
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private VisualFeedback _feedback;

    [Header("Effects")]
    [SerializeField] private GameObject attackEffectPrefab; 
    [SerializeField] private GameObject dashEffectPrefab;   
    [SerializeField] private GameObject guardEffectPrefab;  
    [SerializeField] private float effectYOffset = 0.5f;

    [Header("Stats & Status")]
    [SerializeField] private PlayerStats baseStats; 
    public int CurrentHP { get; private set; }
    public PlayerStats Stats => baseStats; 

    [Header("Movement Settings")]
    [SerializeField] private float dashDistance = 0.5f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashStopThreshold = 0.3f; 
    [SerializeField] private Vector3 startPosition = new Vector3(-1.4f, -0.1f, 0f);

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackCooldown = 0.3f; 
    [SerializeField] private float guardRange = 1.0f; 
    [SerializeField] private float guardDuration = 0.2f;
    [SerializeField] private float guardPushDistance = 1.0f; 
    [SerializeField] private float playerKnockbackDistance = 0.3f; 

    private bool _isDashing, _isAttacking, _isGuarding, _isTransitioning, _isUsingSkill;
    
    public bool IsInvulnerable { get; set; } 
    public bool IsTransitioning => _isTransitioning;

    private readonly List<SkillBase> _skills = new List<SkillBase>();
    private static readonly int AnimMoveTrigger = Animator.StringToHash("1_Move");
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        CurrentHP = baseStats.maxHp;
        _feedback = GetComponent<VisualFeedback>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        _skills.Clear();
        _skills.Add(new Skill_LeapStrike());
        _skills.Add(new Skill_CycloneSlash());
        _skills.Add(new Skill_HolyShield());
    }

    private void Start()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += RespawnAtStart;
    }

    protected override void OnDestroy()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress -= RespawnAtStart;
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

    #region Input & Actions
    
    public void PerformAttack() { if (CanInput()) StartCoroutine(AttackCoroutine()); }
    public void PerformDash() { if (CanInput()) StartCoroutine(DashCoroutine()); }
    public void PerformGuard() { if (CanInput() && CanPerformGuard()) StartCoroutine(GuardCoroutine()); }

    private bool CanPerformGuard()
    {
        float pX = transform.position.x;
        Projectile[] projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        foreach (var proj in projectiles)
        {
            if (proj != null && proj.transform.position.x >= pX - 0.2f && proj.transform.position.x <= pX + guardRange) return true;
        }
        Monster front = GetCurrentSwarm()?.GetFrontMonster();
        return front != null && (front.transform.position.x - pX) <= guardRange;
    }

    public void UseSkill(int index)
    {
        if (!CanInput() || index < 0 || index >= _skills.Count) return;
        SkillBase skill = _skills[index];
        if (skill.IsReady) StartCoroutine(SkillSequence(skill));
    }

    private IEnumerator SkillSequence(SkillBase skill)
    {
        _isUsingSkill = true;
        yield return StartCoroutine(skill.Execute(this));
        _isUsingSkill = false;
    }

    public bool CanInput() => !_isDashing && !_isAttacking && !_isGuarding && !_isTransitioning && !_isUsingSkill && (_feedback != null && !_feedback.IsStunned);
    public bool IsActionActive() => _isDashing || _isGuarding;
    private Swarm GetCurrentSwarm() => StageManager.Instance?.CurrentSwarm;

    #endregion

    #region Combat Logic
    
    public void OnDeflectSuccess(Vector3 position)
    {
        GameObject prefab = _isGuarding ? guardEffectPrefab : (_isAttacking ? attackEffectPrefab : dashEffectPrefab);
        SpawnEffect(prefab, position);
    }

    private void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            position.y += effectYOffset;
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger(AnimAttackTrigger);
            yield return null; 
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
            {
                if (animator.IsInTransition(0)) yield return null;
                else yield return null;
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f) break;
            }
        }
        SpawnEffect(attackEffectPrefab, transform.position + Vector3.right * 0.5f);
        ProcessAttackLogic();
        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
    }

    private void ProcessAttackLogic()
    {
        DeflectProjectilesInRange(attackRange);
        Swarm swarm = GetCurrentSwarm();
        if (swarm == null) return;

        int damage = baseStats.baseDamage;
        bool isCrit = UnityEngine.Random.value < baseStats.critChance;
        if (isCrit) damage = Mathf.RoundToInt(damage * baseStats.critDamageMultiplier);

        if (swarm.AttackInRange(transform.position.x, attackRange, damage))
        {
            if (isCrit) TriggerCombatJuice(0.08f, 0.15f, 0.1f);
            else TriggerCombatJuice(0.05f, 0.08f, 0.05f);

            if (UnityEngine.Random.value < baseStats.doubleHitChance)
            {
                swarm.AttackInRange(transform.position.x, attackRange, Mathf.RoundToInt(damage * baseStats.secondHitDamageRatio));
            }
        }
    }

    private void DeflectProjectilesInRange(float range)
    {
        Projectile[] projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        float pX = transform.position.x;
        foreach (var proj in projectiles)
        {
            if (proj != null)
            {
                float projX = proj.transform.position.x;
                if (projX >= pX - 0.2f && projX <= pX + range) proj.Deflect("Deflected");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsInvulnerable || _isTransitioning) return;
        CurrentHP -= Mathf.Max(1, damage - baseStats.defense);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.1f, 0.1f);
        if (_feedback != null) _feedback.PlayHitEffect(canPlayAnimation: CanInput());
        if (CurrentHP <= 0) Die();
    }

    private void Die() => Debug.Log("<color=black>Player Dead...</color>");

    #endregion

    #region Other Actions (Dash, Guard, Transitions)

    private IEnumerator DashCoroutine()
    {
        _isDashing = true;
        if (animator != null) animator.SetTrigger(AnimMoveTrigger);
        SpawnEffect(dashEffectPrefab, transform.position);
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * dashDistance;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            DeflectProjectilesInRange(0.4f);
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            if (IsMonsterAhead(nextPos)) break; 
            transform.position = nextPos;
            yield return null;
        }
        _isDashing = false;
    }

    private IEnumerator GuardCoroutine()
    {
        _isGuarding = true;
        DeflectProjectilesInRange(guardRange);
        Swarm swarm = GetCurrentSwarm();
        if (swarm != null)
        {
            Monster front = swarm.GetFrontMonster();
            if (front != null && (front.transform.position.x - transform.position.x) <= guardRange)
            {
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.05f, 0.05f);
                OnDeflectSuccess(front.transform.position);
                swarm.Knockback(guardPushDistance, guardDuration);
            }
        }
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.left * playerKnockbackDistance;
        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            DeflectProjectilesInRange(guardRange);
            float ease = 1f - (1f - (elapsed / guardDuration)) * (1f - (elapsed / guardDuration));
            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }
        transform.position = targetPos;
        _isGuarding = false;
    }

    private void TriggerCombatJuice(float stop, float intensity, float duration)
    {
        if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(stop);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(intensity, duration);
    }

    private bool IsMonsterAhead(Vector3 pos)
    {
        Monster front = GetCurrentSwarm()?.GetFrontMonster();
        return front != null && (front.transform.position.x - pos.x) <= dashStopThreshold;
    }

    public void MoveToNextFloorSequence(Action onComplete)
    {
        if (_isTransitioning) return;
        StartCoroutine(MoveRightAndExit(onComplete));
    }

    private IEnumerator MoveRightAndExit(Action onComplete)
    {
        _isTransitioning = true;
        float elapsed = 0f, duration = 0.5f;
        Vector3 startPos = transform.position, targetPos = new Vector3(8.0f, startPos.y, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        transform.position = targetPos;
        onComplete?.Invoke();
    }

    private void RespawnAtStart()
    {
        StopAllCoroutines();
        _isDashing = _isAttacking = _isGuarding = _isUsingSkill = false;
        StartCoroutine(RespawnAtNewFloor());
    }

    private IEnumerator RespawnAtNewFloor()
    {
        transform.position = new Vector3(-8.0f, startPosition.y, 0f);
        float elapsed = 0f, duration = 0.4f;
        Vector3 startPos = transform.position, targetPos = startPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        transform.position = targetPos;
        _isTransitioning = false;
    }

    #endregion
}
