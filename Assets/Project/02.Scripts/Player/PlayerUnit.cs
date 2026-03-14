using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VisualFeedback))]
[RequireComponent(typeof(PlayerStatsController))]
public class PlayerUnit : SingletonBase<PlayerUnit>, IDamageable
{
    #region Serialized Fields

    [Header("Module References")]
    [SerializeField] private Animator animator;
    
    [Header("Effects Prefabs")]
    [SerializeField] private GameObject attackEffectPrefab; 
    [SerializeField] private GameObject dashEffectPrefab;   
    [SerializeField] private GameObject guardEffectPrefab;  
    [SerializeField] private float effectYOffset = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float attackCooldown = 0.7f; 
    [SerializeField] private float guardDuration = 1.0f; 
    [SerializeField] private float guardCooldown = 0.2f; 
    [SerializeField] private float guardPushDistance = 1.6f; 
    [SerializeField] private float playerGuardRecoil = 0.25f;

    [Header("Movement Settings")]
    [SerializeField] private float dashDistance = 0.7f;
    [SerializeField] private float dashStopThreshold = 0.4f;
    [SerializeField] private float dashCooldown = 0.4f;
    [SerializeField] private Vector3 startPosition = new Vector3(-1.4f, -0.1f, 0f);

    [Header("Contact Damage")]
    [SerializeField] private float contactDamageRange = 0.5f; 
    [SerializeField] private float contactDamageInterval = 1.0f; 
    [SerializeField] private int contactDamage = 5; 

    #endregion

    #region Private Fields

    private VisualFeedback _feedback;
    private PlayerStatsController _statsController;
    private float _contactDamageTimer;
    private bool _isDashing, _isAttacking, _isGuarding, _isTransitioning, _isDashCooldown, _isGuardCooldown;
    
    private readonly List<SkillInstance> _skills = new List<SkillInstance>();
    
    
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int AnimMoveTrigger = Animator.StringToHash("1_Move");
    private static readonly int AnimGuardTrigger = Animator.StringToHash("4_Guard");

    #endregion

    #region Properties & Events

    public int CurrentHP => _statsController.CurrentHP;
    public PlayerStats Stats => _statsController.CurrentStats;
    public bool IsTransitioning => _isTransitioning;
    public bool IsInvulnerable { get; set; }
    public bool IsActionActive => _isDashing || _isGuarding;
    public IReadOnlyList<SkillInstance> CurrentSkills => _skills;

    public event Action<int, int> OnHealthChanged;
    public event Action OnSkillsUpdated;

    #endregion

    #region Lifecycle

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        _feedback = GetComponent<VisualFeedback>();
        _statsController = GetComponent<PlayerStatsController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (_statsController != null)
        {
            _statsController.OnHealthChanged += (cur, max) => OnHealthChanged?.Invoke(cur, max);
            _statsController.OnStatUpgraded += (msg) => { if (InGameUIManager.Instance != null) InGameUIManager.Instance.ShowUpgradeNotice(msg); };
        }
        
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += RespawnAtStart;

        OnHealthChanged?.Invoke(CurrentHP, Stats != null ? Stats.maxHp : 100);
    }

    private void Update()
    {
        HandleContactDamage();
    }

    private void LateUpdate()
    {
        if (!_isTransitioning) ClampPosition();
    }

    #endregion

    #region Skill Management

    public bool AddSkill(SkillData data)
    {
        if (_skills.Count < 2)
        {
            _skills.Add(new SkillInstance(data));
            OnSkillsUpdated?.Invoke();
            return true;
        }
        return false;
    }

    public void ReplaceSkill(int index, SkillData newData)
    {
        if (index >= 0 && index < _skills.Count)
        {
            _skills[index] = new SkillInstance(newData);
            OnSkillsUpdated?.Invoke();
        }
    }

    public void UseSkill(int index)
    {
        if (!CanInput() || index < 0 || index >= _skills.Count) return;
        SkillInstance instance = _skills[index];
        if (instance.IsReady) instance.Use(this);
    }

    #endregion

    #region Combat & Damage

    public string UpgradeRandomStat() => _statsController.UpgradeRandomStat();

    public void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        
        if (!gameObject.activeInHierarchy || IsInvulnerable || _isTransitioning) return;

        if (IsActionActive)
        {
            Monster target = StageManager.Instance?.CurrentSwarm?.GetFrontMonster();
            ApplyBlockFeedback(target, isProjectile: isProjectile, isDashing: _isDashing);
            return;
        }

        _statsController.ApplyDamage(damage);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.1f, 0.1f);
        
        
        if (_feedback != null) _feedback.PlayHitEffect(canPlayAnimation: !IsActionActive);
        
        if (CurrentHP <= 0) Die();
    }

    public void Heal(int amount) => _statsController.Heal(amount);

    private void ApplyBlockFeedback(Monster target, bool isProjectile, bool isDashing = false)
    {
        TriggerCombatJuice(0.05f, 0.05f, 0.05f);
        OnDeflectSuccess(transform.position + Vector3.right * 0.2f);

        if (!isDashing)
        {
            StartCoroutine(PlayerRecoilCoroutine());
        }

        if (!isProjectile)
        {
            if (target != null && target.Type == MonsterType.Boss)
                target.Knockback(guardPushDistance, 0.2f);
            else
                StageManager.Instance?.CurrentSwarm?.Knockback(guardPushDistance, 0.2f);
        }
    }

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
                ApplyBlockFeedback(front, isProjectile: false, isDashing: false);
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

    [Header("Death Effects")]
    [SerializeField] private Corpse[] deathFragments; 
    [SerializeField] private float deathSlowdown = 0.2f; 

    private void Die()
    {
        if (IsInvulnerable) return; 

        Debug.Log("<color=red>Player Unit Died!</color>");
        
        
        Time.timeScale = deathSlowdown;
        
        
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.5f, 0.5f);

        
        if (deathFragments != null)
        {
            foreach (var frag in deathFragments)
            {
                if (frag == null) continue;
                Instantiate(frag, transform.position, Quaternion.identity);
            }
        }

        
        if (StageManager.Instance != null)
        {
            
            
        }

        gameObject.SetActive(false);
    }

    #endregion

    #region Actions

    public void PerformAttack() { if (CanInput()) StartCoroutine(AttackCoroutine()); }
    public void PerformDash() { if (CanInput() && !_isDashCooldown) StartCoroutine(DashCoroutine()); }
    public void PerformGuard() { if (CanInput() && !_isGuardCooldown) StartCoroutine(GuardCoroutine()); }

    private bool CanInput() => !_isDashing && !_isAttacking && !_isGuarding && !_isTransitioning;

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);

        yield return new WaitForSeconds(attackCooldown);
        ProcessAttackLogic();

        yield return new WaitForSeconds(0.1f);
        _isAttacking = false;
    }

    private void ProcessAttackLogic()
    {
        if (DeflectProjectilesInRange(attackRange))
            ApplyBlockFeedback(null, isProjectile: true, isDashing: false);

        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm == null) return;

        int damage = Stats.baseDamage;
        bool isCrit = UnityEngine.Random.value < Stats.critChance;
        if (isCrit) damage = Mathf.RoundToInt(damage * Stats.critDamageMultiplier);

        if (swarm.AttackInRange(transform.position.x, attackRange, damage, isCrit))
        {
            SpawnEffect(attackEffectPrefab, transform.position + Vector3.right * 0.8f);
            TriggerCombatJuice(isCrit ? 0.08f : 0.05f, isCrit ? 0.15f : 0.08f, isCrit ? 0.1f : 0.05f);
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
                ApplyBlockFeedback(null, isProjectile: true, isDashing: true);

            transform.position = nextPos;
            yield return null;
        }
        _isDashing = false;
        StartCoroutine(DashCooldownCoroutine());
    }

    private IEnumerator GuardCoroutine()
    {
        _isGuarding = true;
        if (animator != null) animator.SetTrigger(AnimGuardTrigger);

        
        Monster front = StageManager.Instance?.CurrentSwarm?.GetFrontMonster();
        if (front != null && (front.transform.position.x - transform.position.x) <= contactDamageRange + 0.3f)
            ApplyBlockFeedback(front, isProjectile: false, isDashing: false);

        float elapsed = 0f;
        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            if (DeflectProjectilesInRange(attackRange))
                ApplyBlockFeedback(null, isProjectile: true, isDashing: false);
            yield return null;
        }
        _isGuarding = false;
        StartCoroutine(GuardCooldownCoroutine());
    }

    #endregion

    #region Helpers & Feedback

    public void PlaySkillAnimation() { if (animator != null) animator.SetTrigger(AnimAttackTrigger); }

    public void SpawnSkillEffect(GameObject prefab, Vector3 positionOffset = default)
    {
        if (prefab != null) Instantiate(prefab, transform.position + positionOffset, Quaternion.identity);
    }

    public bool IsMonsterAhead(Vector3 pos, float threshold = -1f)
    {
        float actualThreshold = (threshold < 0) ? dashStopThreshold : threshold;
        Monster front = StageManager.Instance?.CurrentSwarm?.GetFrontMonster();
        return front != null && (front.transform.position.x - pos.x) <= actualThreshold;
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

    public void OnDeflectSuccess(Vector3 position) => SpawnEffect(guardEffectPrefab, position);

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

    private IEnumerator DashCooldownCoroutine()
    {
        _isDashCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        _isDashCooldown = false;
    }

    private IEnumerator GuardCooldownCoroutine()
    {
        _isGuardCooldown = true;
        yield return new WaitForSeconds(guardCooldown);
        _isGuardCooldown = false;
    }

    #endregion

    #region Transitions

    public void MoveToNextFloorSequence(Action onComplete)
    {
        if (_isTransitioning) { onComplete?.Invoke(); return; }
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
        _isDashing = _isAttacking = _isGuarding = _isDashCooldown = _isGuardCooldown = false;
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

