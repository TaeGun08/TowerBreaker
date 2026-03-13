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
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float moveSpeedMultiplier = 1.0f;
    [SerializeField] private float knockbackCooldown = 0.5f; 

    private float _lastKnockbackTime;
    private bool _isMoveStop;
    private bool _isForcedStop; 

    public bool IsCleared => _monsters.Count == 0;

    public void SetMoveStop(bool stop)
    {
        _isForcedStop = stop;
    }

    public void SpawnMixed(Monster[] prefabs, Vector3 offset, float spacingX, int floorCount = 0)
    {
        Clear();
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
        if (_isForcedStop) return;

        PlayerUnit player = PlayerUnit.Instance;
        if (player == null || player.IsTransitioning) return;

        Monster front = GetFrontMonster();
        if (front == null) return;

        float distance = front.transform.position.x - player.transform.position.x;
        _isMoveStop = (distance <= stopDistance);

        if (!_isMoveStop)
        {
            bool anyMonsterActing = _monsters.Exists(m => m != null && m.IsMoveStop);
            if (!anyMonsterActing)
            {
                float moveDelta = moveSpeedMultiplier * Time.deltaTime;
                transform.Translate(Vector3.left * moveDelta);
            }
        }
    }

    public Monster GetFrontMonster()
    {
        if (_cachedFrontMonster != null && _cachedFrontMonster.gameObject.activeInHierarchy) 
            return _cachedFrontMonster;

        float minX = float.MaxValue;
        _cachedFrontMonster = null;

        foreach (var monster in _monsters)
        {
            if (monster == null) continue;
            float localX = monster.transform.localPosition.x;
            if (localX < minX)
            {
                minX = localX;
                _cachedFrontMonster = monster;
            }
        }
        return _cachedFrontMonster;
    }

    public bool AttackInRange(float playerX, float range, int damage, bool isCrit = false)
    {
        bool hitAny = false;
        float threshold = playerX + range;

        Monster targetMonster = null;
        float minTargetX = float.MaxValue;

        foreach (var monster in _monsters)
        {
            if (monster == null) continue;
            float mX = monster.transform.position.x;
            if (mX <= threshold && mX < minTargetX)
            {
                minTargetX = mX;
                targetMonster = monster;
            }
        }

        if (targetMonster != null)
        {
            targetMonster.TakeDamage(damage, isCrit);
            hitAny = true;
        }

        Collider2D[] others = Physics2D.OverlapCircleAll(new Vector2(playerX + range * 0.5f, 0), range);
        foreach (var col in others)
        {
            if (col.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damageable is Monster || damageable is PlayerUnit) continue;
                damageable.TakeDamage(damage, isCrit);
                hitAny = true;
            }
        }

        return hitAny;
    }

    public void Knockback(float distance, float duration)
    {
        if (Time.time < _lastKnockbackTime + knockbackCooldown) return;
        _lastKnockbackTime = Time.time;

        StopAllCoroutines();
        StartCoroutine(KnockbackCoroutine(distance, duration));
    }

    private IEnumerator KnockbackCoroutine(float distance, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + Vector3.right * distance;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = 1f - (1f - t) * (1f - t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, easeOut);
            yield return null;
        }
        transform.localPosition = targetPos;
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
