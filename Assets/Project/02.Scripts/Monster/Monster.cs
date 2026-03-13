using System;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour
{
    [Header("Monster Status")]
    [SerializeField] private MonsterType type;
    [SerializeField] protected int hp = 30;
    [SerializeField] private float moveSpeed = 1.0f;
    
    [Header("Effects")]
    [SerializeField] private Corpse[] corpse; // 변수명을 원래대로 복구하여 에디터 할당 값을 유지합니다.

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public bool IsMoveStop { get; set; }

    private bool _isDead = false;

    protected virtual void Awake()
    {
        CurrentHP = hp;
    }

    protected virtual void Update()
    {
        if (IsMoveStop || _isDead) return;
        
        // 왼쪽으로 이동
        transform.Translate(Vector2.left * (moveSpeed * Time.deltaTime));
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        CurrentHP -= damage;
        
        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        OnDie?.Invoke(this);

        // 시체 파편 이펙트 생성 (할당된 파편이 있다면 모두 생성)
        if (corpse != null)
        {
            foreach (Corpse corp in corpse)
            {
                if (corp != null)
                {
                    Instantiate(corp, transform.position, Quaternion.identity);
                }
            }
        }

        // 오브젝트 파괴
        Destroy(gameObject);
    }
}
