using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    public event Action OnCleared;
    private readonly List<Monster> _monsters = new();

    [Header("Settings")]
    [SerializeField] private float stopDistance = 0.5f;

    public bool IsCleared => _monsters.Count == 0;

    public void Spawn(Monster prefab, int count, Vector3 offset, float spacingX)
    {
        Clear();
        
        float startX = -(count - 1) * spacingX * 0.5f;

        for (int i = 0; i < count; i++)
        {
            if (prefab == null) continue;

            Monster monster = Instantiate(prefab, transform);
            Vector3 spawnPos = offset;
            spawnPos.x += startX + (i * spacingX);
            
            monster.transform.localPosition = spawnPos;
            monster.IsMoveStop = true; 
            monster.OnDie += HandleMonsterDie;
            _monsters.Add(monster);
        }
    }

    private void HandleMonsterDie(Monster monster)
    {
        _monsters.Remove(monster);
        if (IsCleared) OnCleared?.Invoke();
    }

    /// <summary>
    /// StageManager에 의해 매 프레임 호출되어 이동 로직을 처리합니다.
    /// </summary>
    public void OnTick(PlayerUnit player)
    {
        if (IsCleared || player == null || player.IsTransitioning)
        {
            SetMoveStop(true);
            return;
        }

        Monster frontMonster = GetFrontMonster();
        if (frontMonster == null) return;

        float distance = frontMonster.transform.position.x - player.transform.position.x;
        SetMoveStop(distance <= stopDistance);
    }

    public Monster GetFrontMonster()
    {
        Monster frontMonster = null;
        float minX = float.MaxValue;

        foreach (var monster in _monsters)
        {
            if (monster == null) continue;
            if (monster.transform.localPosition.x < minX)
            {
                minX = monster.transform.localPosition.x;
                frontMonster = monster;
            }
        }
        return frontMonster;
    }

    /// <summary>
    /// 플레이어의 X 좌표와 공격 범위를 기준으로 몬스터들을 공격합니다.
    /// 타격 성공 여부를 반환합니다.
    /// </summary>
    public bool AttackInRange(float playerX, float range, int damage, bool damageAll = false)
    {
        if (IsCleared) return false;

        float attackThreshold = playerX + range;
        List<Monster> targets = _monsters.FindAll(m => m != null && m.transform.position.x <= attackThreshold);

        if (targets.Count == 0) return false;

        targets.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        if (damageAll)
        {
            foreach (var target in targets) target.TakeDamage(damage);
        }
        else
        {
            targets[0].TakeDamage(damage);
        }
        
        return true;
    }

    public void SetMoveStop(bool stop)
    {
        foreach (var monster in _monsters)
        {
            if (monster != null) monster.IsMoveStop = stop;
        }
    }

    public void Clear()
    {
        foreach (var monster in _monsters)
        {
            if (monster != null)
            {
                monster.OnDie -= HandleMonsterDie;
                Destroy(monster.gameObject);
            }
        }
        _monsters.Clear();
    }

    public void Knockback(float distance, float duration)
    {
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
}
