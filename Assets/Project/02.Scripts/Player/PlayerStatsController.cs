using System;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private PlayerStats baseStats;
    private PlayerStats _currentStats;
    public PlayerStats CurrentStats => _currentStats ?? baseStats;

    public int CurrentHP { get; private set; }
    public event Action<int, int> OnHealthChanged;
    public event Action<string> OnStatUpgraded;

    private void Awake()
    {
        _currentStats = baseStats.Clone();
        CurrentHP = _currentStats.maxHp;
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
        string result = _currentStats.LevelUpRandomStat();
        OnHealthChanged?.Invoke(CurrentHP, _currentStats.maxHp);
        OnStatUpgraded?.Invoke(result);
        return result;
    }
}
