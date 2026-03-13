using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VisualFeedback))]
[RequireComponent(typeof(PlayerStatsController))]
public class PlayerUnit : SingletonBase<PlayerUnit>, IDamageable
{
    [Header("Module References")]
    [SerializeField] private Animator animator;
    private VisualFeedback _feedback;
    private PlayerStatsController _statsController;

    [Header("Effects Prefabs")]
    [SerializeField] private GameObject attackEffectPrefab; 
    [SerializeField] private GameObject dashEffectPrefab;   
    [SerializeField] private GameObject guardEffectPrefab;  
    [SerializeField] private float effectYOffset = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float guardDuration = 0.4f;
    [SerializeField] private float guardPushDistance = 1.2f;
    [SerializeField] private float playerGuardRecoil = 0.3f;

    [Header("Movement Settings")]
    [SerializeField] private float dashDistance = 0.6f;
    [SerializeField] private float dashStopThreshold = 0.35f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private Vector3 startPosition = new Vector3(-1.4f, -0.1f, 0f);

    [Header("Contact Damage (Dot)")]
    [SerializeField] private float contactDamageRange = 0.5f; 
    [SerializeField] private float contactDamageInterval = 1.0f; 
    [SerializeField] private int contactDamage = 5; 
    private float _contactDamageTimer;

    public int CurrentHP => _statsController.CurrentHP;
    public PlayerStats Stats => _statsController.CurrentStats;
    public event Action<int, int> OnHealthChanged;

    private bool _isDashing, _isAttacking, _isGuarding, _isTransitioning, _isUsingSkill, _isDashCooldown;
    public bool IsTransitioning => _isTransitioning;
    public bool IsInvulnerable { get; set; }
    public bool IsActionActive => _isDashing || _isGuarding;

    private readonly List<SkillBase> _skills = new List<SkillBase>();
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int AnimMoveTrigger = Animator.StringToHash("1_Move");

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        _feedback = GetComponent<VisualFeedback>();
        _statsController = GetComponent<PlayerStatsController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        InitializeSkills();
    }

    private void Start()
    {
        _statsController.OnHealthChanged += (cur, max) => OnHealthChanged?.Invoke(cur, max);
        _statsController.OnStatUpgraded += (msg) => InGameUIManager.Instance.ShowUpgradeNotice(msg);
        
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += RespawnAtStart;

        OnHealthChanged?.Invoke(CurrentHP, Stats.maxHp);
    }

    private void Update()
    {
        HandleContactDamage();
    }

    private void LateUpdate()
    {
        if (!_isTransitioning) ClampPosition();
    }

    private void InitializeSkills()
    {
        _skills.Clear();
        _skills.Add(new Skill_LeapStrike());
        _skills.Add(new Skill_CycloneSlash());
        _skills.Add(new Skill_HolyShield());
    }

    #region Interaction & Combat

    public string UpgradeRandomStat() => _statsController.UpgradeRandomStat();

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (IsInvulnerable || _isTransitioning) return;

        if (_isGuarding)
        {
            TriggerCombatJuice(0.05f, 0.05f, 0.05f);
            OnDeflectSuccess(transform.position + Vector3.right * 0.2f);
            
            // 실제로 공격을 막았을 때만 적 군집 넉백 발생 (수정)
            StageManager.Instance?.CurrentSwarm?.Knockback(guardPushDistance, 0.2f);
            
            DeflectProjectilesInRange(attackRange);
            return;
        }

        _statsController.ApplyDamage(damage);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.1f, 0.1f);
        if (_feedback != null) _feedback.PlayHitEffect(canPlayAnimation: CanInput());
        if (CurrentHP <= 0) Die();
    }

    private void Die() => Debug.Log("<color=black>Player Dead...</color>");

    private void HandleContactDamage()
    {
        if (_isTransitioning || IsInvulnerable || CurrentHP <= 0) return;
        
        Monster front = StageManager.Instance?.CurrentSwarm?.GetFrontMonster();
        if (front == null) return;

        float dist = front.transform.position.x - transform.position.x;
        if (dist <= contactDamageRange)
        {
            _contactDamageTimer += Time.deltaTime;
            if (_contactDamageTimer >= contactDamageInterval)
            {
                _contactDamageTimer = 0f;
                TakeDamage(contactDamage);
            }
        }
        else _contactDamageTimer = 0f;
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

    #endregion

    #region Actions

    public void PerformAttack() { if (CanInput()) StartCoroutine(AttackCoroutine()); }
    public void PerformDash() { if (CanInput() && !_isDashCooldown) StartCoroutine(DashCoroutine()); }
    public void PerformGuard() { if (CanInput()) StartCoroutine(GuardCoroutine()); }

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

    private bool CanInput() => !_isDashing && !_isAttacking && !_isGuarding && !_isTransitioning && !_isUsingSkill && (_feedback != null && !_feedback.IsStunned);

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger(AnimAttackTrigger);
            yield return null; 
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.65f)
            {
                if (animator.IsInTransition(0)) yield return null;
                else yield return null;
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.65f) break;
            }
        }

        ProcessAttackLogic();
        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
    }

    private void ProcessAttackLogic()
    {
        DeflectProjectilesInRange(attackRange);
        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm == null) return;

        int damage = Stats.baseDamage;
        bool isCrit = UnityEngine.Random.value < Stats.critChance;
        if (isCrit) damage = Mathf.RoundToInt(damage * Stats.critDamageMultiplier);

        if (swarm.AttackInRange(transform.position.x, attackRange, damage, isCrit))
        {
            SpawnEffect(attackEffectPrefab, transform.position + Vector3.right * 0.8f);
            if (isCrit) TriggerCombatJuice(0.08f, 0.15f, 0.1f);
            else TriggerCombatJuice(0.05f, 0.08f, 0.05f);
        }
    }

    private IEnumerator DashCoroutine()
    {
        _isDashing = true;
        if (animator != null) animator.SetTrigger(AnimMoveTrigger);
        SpawnEffect(dashEffectPrefab, transform.position);

        float elapsed = 0f, duration = 0.12f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * dashDistance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            if (IsMonsterAhead(nextPos)) break; 
            
            if (DeflectProjectilesInRange(0.5f))
            {
                StageManager.Instance?.CurrentSwarm?.Knockback(guardPushDistance * 0.5f, 0.2f);
            }

            transform.position = nextPos;
            yield return null;
        }
        _isDashing = false;
        StartCoroutine(DashCooldownCoroutine());
    }

    private IEnumerator DashCooldownCoroutine()
    {
        _isDashCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        _isDashCooldown = false;
    }

    private IEnumerator GuardCoroutine()
    {
        _isGuarding = true;
        
        // 가드 버튼을 누르자마자 발생하던 넉백 로직 제거 (수정)

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.left * playerGuardRecoil;

        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / guardDuration;
            float ease = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            
            // 가드 도중 투사체를 반사하면 넉백 발생
            if (DeflectProjectilesInRange(attackRange))
            {
                StageManager.Instance?.CurrentSwarm?.Knockback(guardPushDistance, 0.2f);
            }
            
            yield return null;
        }
        _isGuarding = false;
    }

    #endregion

    #region Helpers & Transitions

    private bool IsMonsterAhead(Vector3 pos)
    {
        Monster front = StageManager.Instance?.CurrentSwarm?.GetFrontMonster();
        return front != null && (front.transform.position.x - pos.x) <= dashStopThreshold;
    }

    private bool DeflectProjectilesInRange(float range)
    {
        if (ProjectileManager.Instance == null) return false;
        var projectiles = ProjectileManager.Instance.ActiveProjectiles;
        bool hitAny = false;
        float pX = transform.position.x;

        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            var proj = projectiles[i];
            if (proj != null)
            {
                float projX = proj.transform.position.x;
                if (projX >= pX - 0.2f && projX <= pX + range)
                {
                    proj.Deflect("Deflected");
                    hitAny = true;
                }
            }
        }
        return hitAny;
    }

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

    private void TriggerCombatJuice(float stop, float intensity, float duration)
    {
        if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(stop);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(intensity, duration);
    }

    public void MoveToNextFloorSequence(Action onComplete)
    {
        if (_isTransitioning) return;
        StartCoroutine(MoveRightAndExit(onComplete));
    }

    private IEnumerator MoveRightAndExit(Action onComplete)
    {
        _isTransitioning = true;
        if (animator != null) animator.SetTrigger(AnimMoveTrigger);
        float elapsed = 0f, duration = 0.5f;
        Vector3 startPos = transform.position, targetPos = new Vector3(8.0f, startPos.y, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        onComplete?.Invoke();
    }

    private void RespawnAtStart()
    {
        StopAllCoroutines();
        _isDashing = _isAttacking = _isGuarding = _isUsingSkill = _isDashCooldown = false;
        StartCoroutine(RespawnAtNewFloor());
    }

    private IEnumerator RespawnAtNewFloor()
    {
        transform.position = new Vector3(-8.0f, startPosition.y, 0f);
        if (animator != null) animator.SetTrigger(AnimMoveTrigger);

        float elapsed = 0f, duration = 0.5f;
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
