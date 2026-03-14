using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    public event Action OnCleared;
    private readonly List<Monster> _monsters = new();
    
    [Header("Settings")]
    [SerializeField] private float knockbackCooldown = 0.5f; 

    private float _lastKnockbackTime;
    private bool _isForcedStop; 
    
    
    public bool CanMinionsMove { get; private set; }
    public bool IsCleared => _monsters.Count == 0;

    public void SetMoveStop(bool stop)
    {
        _isForcedStop = stop;
        foreach (var m in _monsters) if (m != null) m.IsMoveStop = stop;
    }

    public void SpawnMixed(Monster[] prefabs, Vector3 offset, float spacingX, int floorCount = 0)
    {
        Clear();
        _isForcedStop = false;
        
        if (prefabs == null || prefabs.Length == 0) return;

        float startX = -(prefabs.Length - 1) * spacingX * 0.5f;

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] == null) continue;
            Monster monster = Instantiate(prefabs[i], transform);
            Vector3 spawnPos = offset;
            spawnPos.x += startX + (i * spacingX);
            monster.transform.localPosition = spawnPos;
            
            monster.MySwarm = this;
            monster.OnDie += HandleMonsterDie;
            monster.SetDifficulty(floorCount);
            _monsters.Add(monster);
        }
    }

    public void AddMonster(Monster monster)
    {
        if (monster == null) return;
        monster.transform.SetParent(transform);
        monster.MySwarm = this;
        monster.OnDie += HandleMonsterDie;
        _monsters.Add(monster);
    }

    private void HandleMonsterDie(Monster monster)
    {
        _monsters.Remove(monster);
        if (IsCleared) OnCleared?.Invoke();
    }

    private void Update()
    {
        if (StageManager.Instance == null || StageManager.Instance.CurrentSwarm != this) return;
        if (_isForcedStop || PlayerUnit.Instance == null || PlayerUnit.Instance.IsTransitioning) 
        {
            CanMinionsMove = false;
            return;
        }

        
        Monster frontMinion = GetFrontMinion();
        if (frontMinion == null) 
        {
            CanMinionsMove = true; 
            return;
        }

        
        float distance = frontMinion.transform.position.x - PlayerUnit.Instance.transform.position.x;
        CanMinionsMove = (distance > 0.7f);
    }

    private Monster GetFrontMinion()
    {
        float minX = float.MaxValue;
        Monster front = null;

        foreach (var m in _monsters)
        {
            if (m == null || m.Type == MonsterType.Boss) continue; 
            float worldX = m.transform.position.x;
            if (worldX < minX)
            {
                minX = worldX;
                front = m;
            }
        }
        return front;
    }

    public Monster GetFrontMonster()
    {
        float minX = float.MaxValue;
        Monster front = null;
        foreach (var m in _monsters)
        {
            if (m == null) continue;
            float worldX = m.transform.position.x;
            if (worldX < minX) { minX = worldX; front = m; }
        }
        return front;
    }

    public bool AttackInRange(float playerX, float range, int damage, bool isCrit = false)
    {
        float threshold = playerX + range;
        Monster target = null;
        float minTargetX = float.MaxValue;

        foreach (var m in _monsters)
        {
            if (m == null) continue;
            float mX = m.transform.position.x;
            if (mX <= threshold && mX < minTargetX)
            {
                minTargetX = mX;
                target = m;
            }
        }

        if (target != null)
        {
            target.TakeDamage(damage, isCrit);
            return true;
        }

        Collider2D[] others = Physics2D.OverlapCircleAll(new Vector2(playerX + range * 0.5f, 0), range);
        foreach (var col in others)
        {
            if (col.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damageable is Monster || damageable is PlayerUnit) continue;
                damageable.TakeDamage(damage, isCrit);
                return true;
            }
        }
        return false;
    }

    public void Knockback(float distance, float duration)
    {
        if (Time.time < _lastKnockbackTime + knockbackCooldown) return;
        _lastKnockbackTime = Time.time;

        foreach (var m in _monsters)
        {
            if (m != null && m.Type != MonsterType.Boss)
            {
                m.Knockback(distance, duration);
            }
        }
    }

    public void Clear()
    {
        foreach (var monster in _monsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        _monsters.Clear();
    }
}

