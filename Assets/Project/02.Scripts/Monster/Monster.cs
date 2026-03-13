using System;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

[RequireComponent(typeof(VisualFeedback))]
public abstract class Monster : MonoBehaviour, IDamageable
{
    [Header("Monster Status")]
    [SerializeField] protected MonsterType type; // 몬스터 종류 (Normal, Boss)
    [SerializeField] protected int hp = 30;
    [SerializeField] private float moveSpeed = 1.0f;
    
    [Header("Effects")]
    [SerializeField] private Corpse[] corpse; 

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public bool IsMoveStop { get; set; }
    public Swarm MySwarm { get; set; }

    private bool _isDead;
    protected VisualFeedback feedback;
    protected Animator animator; 

    protected virtual void Awake()
    {
        CurrentHP = hp;
        feedback = GetComponent<VisualFeedback>();
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 층수에 따른 난이도 보정치를 적용합니다.
    /// </summary>
    public void SetDifficulty(int floorCount)
    {
        int bonusHP = 0;
        int interval = 10; // 10층마다 상승

        if (type == MonsterType.Boss)
        {
            // 보스는 10층마다 10씩 상승
            bonusHP = (floorCount / interval) * 10;
        }
        else
        {
            // 일반 몬스터는 10층마다 1씩 상승
            bonusHP = (floorCount / interval) * 1;
        }

        hp += bonusHP;
        CurrentHP = hp; // 현재 체력 동기화
        
        if (bonusHP > 0)
        {
            Debug.Log($"<color=yellow>{gameObject.name} Scaled! Floor: {floorCount}, Bonus HP: {bonusHP}</color>");
        }
    }

    protected bool IsActiveSwarm()
    {
        if (MySwarm == null || StageManager.Instance == null) return false;
        return StageManager.Instance.CurrentSwarm == MySwarm;
    }

    protected virtual void Update()
    {
        if (!IsActiveSwarm() || IsMoveStop || _isDead || (feedback != null && feedback.IsStunned)) return;
        transform.Translate(Vector2.left * (moveSpeed * Time.deltaTime));
    }

    public virtual void TakeDamage(int damage)
    {
        if (_isDead) return;
        CurrentHP -= damage;
        if (feedback != null) feedback.PlayHitEffect();
        if (CurrentHP <= 0) Die();
    }

    protected void Die()
    {
        if (_isDead) return;
        _isDead = true;
        OnDie?.Invoke(this);
        if (corpse != null)
        {
            foreach (var corp in corpse)
            {
                if (corp != null) Instantiate(corp, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }
}
