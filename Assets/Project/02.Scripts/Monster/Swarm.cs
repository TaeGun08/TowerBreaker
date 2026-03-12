using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    private List<Monster> _monsters = new();
    private int _currentTargetIndex = 0;

    public bool IsCleared => _currentTargetIndex >= _monsters.Count;

    public void AddMonster(Monster monster)
    {
        _monsters.Add(monster);
    }

    public void AttackFront()
    {
        if (IsCleared) return;
        
        Monster target = _monsters[_currentTargetIndex];
        if (target != null)
        {
            target.TakeDamage();
            _currentTargetIndex++;
        }
    }

    // 기존 Monster.cs에서 호출하던 함수 호환 유지
    public void SetMonsterCount(int delta)
    {
        // 필요 시 몬스터 개수 추적 로직 추가 가능
    }

    public void OnMonsterDestroyed()
    {
        // 몬스터 사망 시 처리 로직
    }
}
