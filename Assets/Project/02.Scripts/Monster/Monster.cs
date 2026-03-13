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
    
    [Header("Visual Effects")]
    [SerializeField] private Corpse[] corpse; 
    [SerializeField] private RewardChest chestPrefab; // 보상 상자 복구

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public Swarm MySwarm { get; set; }
    public bool IsMoveStop { get; set; } 

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

        // 보스일 경우 상자 생성 기능 복구
        if (type == MonsterType.Boss && chestPrefab != null)
        {
            Instantiate(chestPrefab, transform.position, Quaternion.identity);
        }

        if (corpse != null)
        {
            foreach (var corp in corpse)
                if (corp != null) Instantiate(corp, transform.position, Quaternion.identity);
        }

        OnDie?.Invoke(this);
        Destroy(gameObject);
    }
}
