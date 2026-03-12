using System;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    public event Action OnCleared;
    private List<Monster> monsters = new();

    public bool IsCleared => monsters.Count == 0;

    /// <summary>
    /// 지정된 설정으로 몬스터 군집을 생성합니다.
    /// </summary>
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
            monster.IsMoveStop = true; // 생성 시에는 정지 상태
            monster.OnDie += HandleMonsterDie;
            monsters.Add(monster);
        }
    }

    private void HandleMonsterDie(Monster monster)
    {
        monsters.Remove(monster);
        if (IsCleared)
        {
            OnCleared?.Invoke();
        }
    }

    /// <summary>
    /// 군집의 맨 앞에 있는 몬스터를 공격합니다.
    /// </summary>
    public void AttackFront(int damage)
    {
        if (monsters.Count == 0) return;

        // X 좌표가 가장 작은(왼쪽으로 가장 많이 간) 몬스터가 맨 앞
        Monster frontMonster = null;
        float minX = float.MaxValue;

        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] == null) continue;
            
            if (monsters[i].transform.localPosition.x < minX)
            {
                minX = monsters[i].transform.localPosition.x;
                frontMonster = monsters[i];
            }
        }

        if (frontMonster != null)
        {
            frontMonster.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 군집 내 모든 몬스터의 이동 여부를 설정합니다.
    /// </summary>
    public void SetMoveStop(bool stop)
    {
        foreach (var monster in monsters)
        {
            if (monster != null)
            {
                monster.IsMoveStop = stop;
            }
        }
    }

    public void Clear()
    {
        foreach (var monster in monsters)
        {
            if (monster != null)
            {
                monster.OnDie -= HandleMonsterDie;
                Destroy(monster.gameObject);
            }
        }
        monsters.Clear();
    }
}
