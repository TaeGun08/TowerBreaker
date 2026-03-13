using System;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour
{
    [SerializeField] private MonsterType type;
    [SerializeField] protected int hp;
    [SerializeField] private float moveSpeed;
    
    [SerializeField] private Corpse[] corpse;

    public event Action<Monster> OnDie;
    public int CurrentHP { get; private set; }
    public bool IsMoveStop { get; set; }

    protected virtual void Start()
    {
        CurrentHP = hp;
    }

    protected void Update()
    {
        if (IsMoveStop) return;
        transform.Translate(Vector2.left * (moveSpeed * Time.deltaTime));
    }

    public void TakeDamage(int damage)
    {
        if (CurrentHP <= 0) return;

        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDie?.Invoke(this);
        foreach (Corpse corp in corpse)
        {
            Instantiate(corp, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
