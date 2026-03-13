using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    public event Action OnCleared;
    private readonly List<Monster> _monsters = new();
    private Monster _cachedFrontMonster;

    [Header("Settings")]
    [SerializeField] private float stopDistance = 0.7f; 
    [SerializeField] private float knockbackCooldown = 0.5f; 

    private float _lastKnockbackTime;
    private bool _isForcedStop; 
    
    // 군집 전체 이동 가능 여부
    public bool CanMove { get; private set; }
    public bool IsCleared => _monsters.Count == 0;

    public void SetMoveStop(bool stop)
    {
        _isForcedStop = stop;
        // 개별 몬스터들에게도 상태 전달
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
        _cachedFrontMonster = null;
    }

    public void AddMonster(Monster monster)
    {
        if (monster == null) return;
        monster.transform.SetParent(transform);
        monster.MySwarm = this;
        monster.OnDie += HandleMonsterDie;
        _monsters.Add(monster);
        _cachedFrontMonster = null;
    }

    private void HandleMonsterDie(Monster monster)
    {
        if (_cachedFrontMonster == monster) _cachedFrontMonster = null;
        _monsters.Remove(monster);
        if (IsCleared) OnCleared?.Invoke();
    }

    private void Update()
    {
        if (StageManager.Instance == null || StageManager.Instance.CurrentSwarm != this) return;
        if (_isForcedStop) 
        {
            CanMove = false;
            return;
        }

        PlayerUnit player = PlayerUnit.Instance;
        if (player == null || player.IsTransitioning) 
        {
            CanMove = false;
            return;
        }

        Monster front = GetFrontMonster();
        if (front == null) 
        {
            CanMove = false;
            return;
        }

        // 1. 맨 앞 개체와의 거리 체크
        float distance = front.transform.position.x - player.transform.position.x;
        
        // 2. 전체 이동 가능 여부 결정 (맨 앞이 멈추면 다 멈춤)
        CanMove = (distance > stopDistance);

        // 3. 특정 몬스터가 패턴 수행 중(IsMoveStop)이면 군집 전체 정지
        if (CanMove)
        {
            if (_monsters.Exists(m => m != null && m.IsMoveStop))
            {
                CanMove = false;
            }
        }
    }

    public Monster GetFrontMonster()
    {
        if (_cachedFrontMonster == null || !_cachedFrontMonster.gameObject.activeInHierarchy)
        {
            float minX = float.MaxValue;
            _cachedFrontMonster = null;

            foreach (var monster in _monsters)
            {
                if (monster == null) continue;
                float worldX = monster.transform.position.x;
                if (worldX < minX)
                {
                    minX = worldX;
                    _cachedFrontMonster = monster;
                }
            }
        }
        return _cachedFrontMonster;
    }

    public bool AttackInRange(float playerX, float range, int damage, bool isCrit = false)
    {
        float threshold = playerX + range;
        Monster target = null;
        float minTargetX = float.MaxValue;

        foreach (var monster in _monsters)
        {
            if (monster == null) continue;
            float mX = monster.transform.position.x;
            if (mX <= threshold && mX < minTargetX)
            {
                minTargetX = mX;
                target = monster;
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
            if (m != null) StartCoroutine(IndividualKnockback(m, distance, duration));
        }
    }

    private IEnumerator IndividualKnockback(Monster m, float distance, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = m.transform.position;
        Vector3 targetPos = startPos + Vector3.right * distance;
        while (elapsed < duration)
        {
            if (m == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = 1f - (1f - t) * (1f - t);
            m.transform.position = Vector3.Lerp(startPos, targetPos, easeOut);
            yield return null;
        }
    }

    public void Clear()
    {
        foreach (var monster in _monsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        _monsters.Clear();
        _cachedFrontMonster = null;
    }
}
