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
        // 초기화는 여기서 진행하되, 최종 스탯 갱신은 Start에서 한 번 더 수행
        InitializeStats();
    }

    private void Start()
    {
        // [중요] 씬 로드 후 매니저들이 준비된 시점에 장비 보너스 다시 계산
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

    // 장비 보너스를 합산하여 최종 스탯을 갱신합니다.
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

        // 최대 체력이 변경되었을 때 현재 체력 비율 유지 혹은 조정
        if (CurrentHP > _finalStats.maxHp) CurrentHP = _finalStats.maxHp;
        if (CurrentHP <= 0 && _finalStats.maxHp > 0) CurrentHP = _finalStats.maxHp; // 초기 로드 시 체력 설정
        
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
