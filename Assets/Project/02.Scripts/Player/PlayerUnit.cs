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
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float guardDuration = 0.45f; 
    [SerializeField] private float guardCooldown = 0.5f; 
    [SerializeField] private float guardPushDistance = 1.3f;
    [SerializeField] private float playerGuardRecoil = 0.25f;

    [Header("Movement Settings")]
    [SerializeField] private float dashDistance = 0.7f;
    [SerializeField] private float dashStopThreshold = 0.4f;
    [SerializeField] private float dashCooldown = 0.4f;
    [SerializeField] private Vector3 startPosition = new Vector3(-1.4f, -0.1f, 0f);

    [Header("Contact Damage (Dot)")]
    [SerializeField] private float contactDamageRange = 0.5f; 
    [SerializeField] private float contactDamageInterval = 1.0f; 
    [SerializeField] private int contactDamage = 5; 
    private float _contactDamageTimer;

    public int CurrentHP => _statsController.CurrentHP;
    public PlayerStats Stats => _statsController.CurrentStats;
    public event Action<int, int> OnHealthChanged;

    private bool _isDashing, _isAttacking, _isGuarding, _isTransitioning, _isUsingSkill, _isDashCooldown, _isGuardCooldown;
    public bool IsTransitioning => _isTransitioning;
    public bool IsInvulnerable { get; set; }
    public bool IsActionActive => _isDashing || _isGuarding;

    private readonly List<SkillBase> _skills = new List<SkillBase>();
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int AnimMoveTrigger = Animator.StringToHash("1_Move");
    private static readonly int AnimGuardTrigger = Animator.StringToHash("4_Guard");

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

    public void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        if (IsInvulnerable || _isTransitioning) return;

        if (IsActionActive)
        {
            // 방어 피드백 (투사체/마법 여부 전달)
            ApplyBlockFeedback(isProjectile: isProjectile, isDashing: _isDashing);
            return;
        }

        _statsController.ApplyDamage(damage);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.1f, 0.1f);
        if (_feedback != null) _feedback.PlayHitEffect(canPlayAnimation: !IsActionActive);
        if (CurrentHP <= 0) Die();
    }

    private void ApplyBlockFeedback(bool isProjectile, bool isDashing = false)
    {
        TriggerCombatJuice(0.05f, 0.05f, 0.05f);
        OnDeflectSuccess(transform.position + Vector3.right * 0.2f);

        if (!isDashing)
        {
            StartCoroutine(PlayerRecoilCoroutine());
        }

        // [중요] 화살이나 마법(isProjectile)을 막았을 때는 적 보스나 군집을 밀어내지 않음
        if (!isProjectile)
        {
            StageManager.Instance?.CurrentSwarm?.Knockback(guardPushDistance, 0.2f);
        }
    }

    private IEnumerator PlayerRecoilCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.left * playerGuardRecoil;
        float elapsed = 0f, duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
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
            if (_isGuarding)
            {
                // 접촉 데미지는 근접 공격이므로 isProjectile = false
                ApplyBlockFeedback(isProjectile: false, isDashing: false);
                return; 
            }

            _contactDamageTimer += Time.deltaTime;
            if (_contactDamageTimer >= contactDamageInterval)
            {
                _contactDamageTimer = 0f;
                TakeDamage(contactDamage, isProjectile: false);
            }
        }
        else _contactDamageTimer = 0f;
    }

    private void ClampPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float screenHalfWidth = cam.orthographicSize * cam.aspect;
        float limitX = screenHalfWidth * 0.85f;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;
    }

    #endregion

    #region Actions

    public void PerformAttack() { if (CanInput()) StartCoroutine(AttackCoroutine()); }
    public void PerformDash() { if (CanInput() && !_isDashCooldown) StartCoroutine(DashCoroutine()); }
    public void PerformGuard() { if (CanInput() && !_isGuardCooldown) StartCoroutine(GuardCoroutine()); }

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

    private bool CanInput() => !_isDashing && !_isAttacking && !_isGuarding && !_isTransitioning && !_isUsingSkill;

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger(AnimAttackTrigger);
            
            float timeout = 0.5f;
            float elapsed = 0f;
            yield return null; 
            while (elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Attack") || state.IsName("2_Attack")) 
                {
                    if (state.normalizedTime >= 0.3f) break;
                }
                yield return null;
            }
        }

        ProcessAttackLogic();
        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
    }

    private void ProcessAttackLogic()
    {
        if (DeflectProjectilesInRange(attackRange))
        {
            ApplyBlockFeedback(isProjectile: true, isDashing: false);
        }

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
                ApplyBlockFeedback(isProjectile: true, isDashing: true);
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
        if (animator != null) animator.SetTrigger(AnimGuardTrigger);

        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm != null)
        {
            Monster front = swarm.GetFrontMonster();
            if (front != null && (front.transform.position.x - transform.position.x) <= contactDamageRange + 0.3f)
            {
                ApplyBlockFeedback(isProjectile: false, isDashing: false);
            }
        }

        float elapsed = 0f;
        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            if (DeflectProjectilesInRange(attackRange))
            {
                ApplyBlockFeedback(isProjectile: true, isDashing: false);
            }
            yield return null;
        }
        _isGuarding = false;
        StartCoroutine(GuardCooldownCoroutine());
    }

    private IEnumerator GuardCooldownCoroutine()
    {
        _isGuardCooldown = true;
        yield return new WaitForSeconds(guardCooldown);
        _isGuardCooldown = false;
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
            if (proj != null && !proj.IsDeflected)
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
        if (_isTransitioning)
        {
            onComplete?.Invoke();
            return;
        }
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
        _isDashing = _isAttacking = _isGuarding = _isUsingSkill = _isDashCooldown = _isGuardCooldown = false;
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
