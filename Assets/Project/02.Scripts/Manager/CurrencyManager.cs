using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    private const string KEY_GOLD = "TotalGold";
    private const string KEY_CHEST = "TotalChests";

    // 영구 저장 데이터
    public int TotalGold { get; private set; }
    public int TotalChests { get; private set; }

    // 현재 세션(인게임) 데이터 - UI에 표시할 용도
    public int SessionGold { get; private set; }
    public int SessionChests { get; private set; }

    public System.Action<int> OnSessionGoldChanged;
    public System.Action<int> OnSessionChestChanged;

    protected override void Awake()
    {
        dontDestroy = true;
        base.Awake();
        LoadTotalData();
        ResetSessionData();
    }

    private void LoadTotalData()
    {
        TotalGold = PlayerPrefs.GetInt(KEY_GOLD, 0);
        TotalChests = PlayerPrefs.GetInt(KEY_CHEST, 0);
    }

    public void ResetSessionData()
    {
        SessionGold = 0;
        SessionChests = 0;
        OnSessionGoldChanged?.Invoke(SessionGold);
        OnSessionChestChanged?.Invoke(SessionChests);
    }

    public void AddGold(int amount)
    {
        // 세션 데이터 추가 및 UI 알림
        SessionGold += amount;
        OnSessionGoldChanged?.Invoke(SessionGold);

        // 영구 저장 데이터 업데이트
        TotalGold += amount;
        PlayerPrefs.SetInt(KEY_GOLD, TotalGold);
        PlayerPrefs.Save();
    }

    public void AddChest(int count)
    {
        // 세션 데이터 추가 및 UI 알림
        SessionChests += count;
        OnSessionChestChanged?.Invoke(SessionChests);

        // 영구 저장 데이터 업데이트
        TotalChests += count;
        PlayerPrefs.SetInt(KEY_CHEST, TotalChests);
        PlayerPrefs.Save();
    }
}
