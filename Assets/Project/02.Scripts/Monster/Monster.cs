using System;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour, IDamageable
{
    [Header("Monster Status")]
    [SerializeField] protected MonsterType type; 
    [SerializeField] protected int hp = 30;
    [SerializeField] protected float moveSpeed = 1.0f;
    
    [Header("Visual Effects")]
    [SerializeField] private Corpse[] corpse; 
    [SerializeField] private RewardChest chestPrefab; 

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public Swarm MySwarm { get; set; }
    public bool IsMoveStop { get; set; } 
    public float MoveSpeed => moveSpeed;
    public MonsterType Type => type;

    // 모든 몬스터의 개별 정지 거리 (0.7f로 통일)
    public virtual float StopDistance => 0.7f;

    protected int currentFloorCount;
    private bool _isDead;
    protected VisualFeedback feedback;
    protected Animator animator; 

    protected virtual void Awake()
    {
        CurrentHP = hp;
        feedback = GetComponent<VisualFeedback>();
        animator = GetComponentInChildren<Animator>();
    }

    public void SetDifficulty(int floorCount)
    {
        currentFloorCount = floorCount;
        int multiplier = floorCount / 10;
        int bonusHP = (type == MonsterType.Boss) ? multiplier * 10 : multiplier * 1;
        hp += bonusHP;
        CurrentHP = hp;
    }

    protected bool IsInActiveSwarm()
    {
        return StageManager.Instance != null && StageManager.Instance.CurrentSwarm == MySwarm;
    }

    protected virtual void Update()
    {
        if (PlayerUnit.Instance == null || !IsInActiveSwarm() || IsMoveStop || _isDead) return;
        if (PlayerUnit.Instance.IsTransitioning) return;

        if (type == MonsterType.Boss)
        {
            HandleMovement();
        }
        else
        {
            if (MySwarm != null && MySwarm.CanMinionsMove)
            {
                HandleMovement();
            }
        }
    }

    protected virtual void HandleMovement()
    {
        float distance = transform.position.x - PlayerUnit.Instance.transform.position.x;
        if (distance > StopDistance)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        if (_isDead) return;

        CurrentHP -= damage;
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.SpawnDamageText(transform.position, damage, isCrit);

        if (feedback != null) feedback.PlayHitEffect();
        if (CurrentHP <= 0) Die();
    }

    protected void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (type == MonsterType.Boss && chestPrefab != null)
            Instantiate(chestPrefab, transform.position, Quaternion.identity);

        if (corpse != null)
        {
            foreach (var corpPrefab in corpse)
            {
                if (corpPrefab != null)
                {
                    Corpse instance = Instantiate(corpPrefab, transform.position, Quaternion.identity);
                    // 시체에게 원본 몬스터 타입 전달 (보상 판정용)
                    instance.Setup(type);
                }
            }
        }

        OnDie?.Invoke(this);
        Destroy(gameObject);
    }
}
