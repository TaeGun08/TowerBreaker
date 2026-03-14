using System;
using System.Collections;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour, IDamageable
{
    #region Serialized Fields

    [Header("Monster Status")]
    [SerializeField] protected MonsterType type; 
    [SerializeField] protected int hp = 30;
    [SerializeField] protected float moveSpeed = 1.0f;
    
    [Header("Visual Effects")]
    [SerializeField] private Corpse[] corpse; 
    [SerializeField] private RewardChest chestPrefab; 

    #endregion

    #region Properties & Events

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public Swarm MySwarm { get; set; }
    public bool IsMoveStop { get; set; } 
    public float MoveSpeed => moveSpeed;
    public MonsterType Type => type;

    
    public virtual float StopDistance => 0.7f;

    #endregion

    #region Private Fields

    protected int currentFloorCount;
    private bool _isDead;
    protected VisualFeedback feedback;
    protected Animator animator;
    private Coroutine _knockbackCoroutine; 

    #endregion

    #region Lifecycle

    protected virtual void Awake()
    {
        CurrentHP = hp;
        feedback = GetComponent<VisualFeedback>();
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        if (!CanMove()) return;
        HandleMovement();
    }

    #endregion

    #region Movement & Logic

    public void SetDifficulty(int floorCount)
    {
        currentFloorCount = floorCount;
        int multiplier = floorCount / 10;
        int bonusHP = (type == MonsterType.Boss) ? multiplier * 10 : multiplier * 1;
        hp += bonusHP;
        CurrentHP = hp;
    }

    protected virtual bool CanMove()
    {
        if (PlayerUnit.Instance == null || !IsInActiveSwarm() || IsMoveStop || _isDead) return false;
        if (PlayerUnit.Instance.IsTransitioning) return false;

        if (type != MonsterType.Boss)
        {
            if (MySwarm == null || !MySwarm.CanMinionsMove) return false;
        }

        return true;
    }

    protected virtual void HandleMovement()
    {
        float distance = transform.position.x - PlayerUnit.Instance.transform.position.x;
        if (distance > StopDistance)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
        }
    }

    protected bool IsInActiveSwarm()
    {
        return StageManager.Instance != null && StageManager.Instance.CurrentSwarm == MySwarm;
    }

    #endregion

    #region Damage & Health

    public virtual void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        if (_isDead) return;

        CurrentHP -= damage;
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.SpawnDamageText(transform.position, damage, isCrit);

        if (feedback != null) feedback.PlayHitEffect();
        if (CurrentHP <= 0) Die();
    }

    public void Knockback(float distance, float duration)
    {
        if (_isDead) return;
        
        if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(distance, duration));
    }

    private IEnumerator KnockbackCoroutine(float distance, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * distance;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetPos, easeOut);
            yield return null;
        }
        _knockbackCoroutine = null;
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        SpawnRewards();

        OnDie?.Invoke(this);
        Destroy(gameObject);
    }

    private void SpawnRewards()
    {
        if (type == MonsterType.Boss && chestPrefab != null)
            Instantiate(chestPrefab, transform.position, Quaternion.identity);

        if (corpse != null)
        {
            foreach (var corpPrefab in corpse)
            {
                if (corpPrefab == null) continue;
                Corpse instance = Instantiate(corpPrefab, transform.position, Quaternion.identity);
                instance.Setup(type);
            }
        }
    }

    #endregion
}

