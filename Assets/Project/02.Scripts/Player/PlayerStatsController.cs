using System;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private PlayerStats baseStats; 
    private PlayerStats _runtimeBaseStats;          
    private PlayerStats _finalStats;                

    public PlayerStats CurrentStats => _finalStats ?? _runtimeBaseStats ?? baseStats;

    public int CurrentHP { get; private set; }
    public event Action<int, int> OnHealthChanged;
    public event Action<string> OnStatUpgraded;

    private void Awake()
    {
        
        InitializeStats();
    }

    private void Start()
    {
        
        RefreshFinalStats();
        CurrentHP = CurrentStats.maxHp;
        OnHealthChanged?.Invoke(CurrentHP, CurrentStats.maxHp);
    }

    private void InitializeStats()
    {
        if (baseStats == null)
        {
            Debug.LogWarning($"[PlayerStatsController] BaseStats is missing on {gameObject.name}. Initialization skipped.");
            return;
        }

        _runtimeBaseStats = baseStats.Clone();
        RefreshFinalStats();
    }

    
    public void RefreshFinalStats()
    {
        if (_runtimeBaseStats == null) return;
        
        _finalStats = _runtimeBaseStats.Clone();

        if (EquipmentManager.Instance != null)
        {
            var (atk, def, hp, crit, dbl) = EquipmentManager.Instance.GetTotalBonuses();
            
            _finalStats.baseDamage += atk;
            _finalStats.defense += def;
            _finalStats.maxHp += hp;
            _finalStats.critChance += crit;
            _finalStats.doubleHitChance += dbl;

            Debug.Log($"[PlayerStatsController] Equipment Applied: ATK+{atk}, DEF+{def}, HP+{hp}, CRIT+{crit*100}%");
        }

        
        if (CurrentHP > _finalStats.maxHp) CurrentHP = _finalStats.maxHp;
        if (CurrentHP <= 0 && _finalStats.maxHp > 0) CurrentHP = _finalStats.maxHp; 
        
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
        RefreshFinalStats(); 
        return result;
    }
}

