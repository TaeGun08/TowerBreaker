using System;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private PlayerStats baseStats; // 에디터 설정용 기본 스탯
    private PlayerStats _runtimeBaseStats;          // 인게임 레벨업이 반영된 순수 스탯
    private PlayerStats _finalStats;                // 장비 보너스까지 합산된 최종 스탯

    public PlayerStats CurrentStats => _finalStats ?? _runtimeBaseStats ?? baseStats;

    public int CurrentHP { get; private set; }
    public event Action<int, int> OnHealthChanged;
    public event Action<string> OnStatUpgraded;

    private void Awake()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        _runtimeBaseStats = baseStats.Clone();
        RefreshFinalStats();
        CurrentHP = CurrentStats.maxHp;
    }

    // 장비 보너스를 합산하여 최종 스탯을 갱신합니다.
    public void RefreshFinalStats()
    {
        _finalStats = _runtimeBaseStats.Clone();

        if (EquipmentManager.Instance != null)
        {
            var (atk, def, hp, crit, dbl) = EquipmentManager.Instance.GetTotalBonuses();
            
            _finalStats.baseDamage += atk;
            _finalStats.defense += def;
            _finalStats.maxHp += hp;
            _finalStats.critChance += crit;
            _finalStats.doubleHitChance += dbl;
        }

        // 최대 체력이 변경되었을 때 현재 체력 비율 유지 혹은 조정
        if (CurrentHP > _finalStats.maxHp) CurrentHP = _finalStats.maxHp;
        
        OnHealthChanged?.Invoke(CurrentHP, _finalStats.maxHp);
    }

    public void ApplyDamage(int damage)
    {
        CurrentHP -= Mathf.Max(1, damage - CurrentStats.defense);
        OnHealthChanged?.Invoke(CurrentHP, CurrentStats.maxHp);
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentStats.maxHp, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, CurrentStats.maxHp);
    }

    public string UpgradeRandomStat()
    {
        string result = _runtimeBaseStats.LevelUpRandomStat();
        RefreshFinalStats(); // 베이스 스탯이 변했으므로 최종 스탯 다시 계산
        return result;
    }
}
