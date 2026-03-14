using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    private const string KEY_CHEST = "TotalChests";

    // 영구 저장 데이터 (체스트만 유지)
    public int TotalChests { get; private set; }

    // 현재 세션(인게임) 재화: Corpse (스킬 뽑기용)
    public int SessionCorpse { get; private set; }
    public int SessionChests { get; private set; }

    public System.Action<int> OnSessionCorpseChanged;
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
        TotalChests = PlayerPrefs.GetInt(KEY_CHEST, 0);
    }

    public void ResetSessionData()
    {
        SessionCorpse = 0;
        SessionChests = 0;
        OnSessionCorpseChanged?.Invoke(SessionCorpse);
        OnSessionChestChanged?.Invoke(SessionChests);
    }

    public void AddCorpse(int amount)
    {
        // 세션 데이터 추가 및 UI 알림 (인게임 전용)
        SessionCorpse += amount;
        OnSessionCorpseChanged?.Invoke(SessionCorpse);
    }

    public void UseCorpse(int amount)
    {
        if (SessionCorpse >= amount)
        {
            SessionCorpse -= amount;
            OnSessionCorpseChanged?.Invoke(SessionCorpse);
        }
    }

    public void AddChest(int count)
    {
        // 세션 데이터 추가 및 UI 알림
        SessionChests += count;
        OnSessionChestChanged?.Invoke(SessionChests);

        // 영구 저장 데이터 업데이트 (아웃게임 용)
        TotalChests += count;
        PlayerPrefs.SetInt(KEY_CHEST, TotalChests);
        PlayerPrefs.Save();
    }
}
