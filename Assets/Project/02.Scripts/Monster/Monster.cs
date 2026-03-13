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

        // 군집 전체가 이동 가능한 상태일 때만 개별 이동 수행 (복구 및 디벨롭)
        if (MySwarm != null && MySwarm.CanMove)
        {
            HandleMovement();
        }
    }

    protected virtual void HandleMovement()
    {
        // 개별 정지 거리 체크 (군집이 움직이더라도 보스는 자신의 정지 거리를 지킴)
        float stopDist = (type == MonsterType.Boss) ? 3.0f : 0.7f;
        float distance = transform.position.x - PlayerUnit.Instance.transform.position.x;

        if (distance > stopDist)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit = false)
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
            foreach (var corp in corpse)
                if (corp != null) Instantiate(corp, transform.position, Quaternion.identity);
        }

        OnDie?.Invoke(this);
        Destroy(gameObject);
    }
}
