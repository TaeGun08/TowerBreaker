using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    private const string KEY_CHEST = "TotalChests";

    
    public int TotalChests { get; private set; }

    
    public int SessionCorpse { get; private set; }
    public int SessionChests { get; private set; }

    public System.Action<int> OnSessionCorpseChanged;
    public System.Action<int> OnSessionChestChanged;

    protected override void Awake()
    {
        
        if (_instance != null && _instance != this)
        {
            Debug.Log("[CurrencyManager] Duplicate found, destroying.");
            Destroy(gameObject);
            return;
        }

        dontDestroy = true;
        base.Awake();
        
        LoadTotalData();
        Debug.Log($"[CurrencyManager] Awake. TotalChests: {TotalChests}");
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
        Debug.Log("[CurrencyManager] Session Data Reset.");
    }

    public void AddCorpse(int amount)
    {
        SessionCorpse += amount;
        Debug.Log($"[CurrencyManager] Adding Corpse: {amount}. Current Session: {SessionCorpse}");
        
        
        OnSessionCorpseChanged?.Invoke(SessionCorpse);
        
        
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.UpdateCorpseUI(SessionCorpse);
        }
    }

    public void AddChest(int count)
    {
        SessionChests += count;
        TotalChests += count;
        
        PlayerPrefs.SetInt(KEY_CHEST, TotalChests);
        PlayerPrefs.Save();

        Debug.Log($"[CurrencyManager] Adding Chest: {count}. Total: {TotalChests}");

        OnSessionChestChanged?.Invoke(SessionChests);
        
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.UpdateChestUI(SessionChests);
        }
    }

    public void UseCorpse(int amount)
    {
        if (SessionCorpse >= amount)
        {
            SessionCorpse -= amount;
            OnSessionCorpseChanged?.Invoke(SessionCorpse);
            if (InGameUIManager.Instance != null) InGameUIManager.Instance.UpdateCorpseUI(SessionCorpse);
        }
    }

    public void ClearAllCurrencyData()
    {
        TotalChests = 0;
        SessionChests = 0;
        SessionCorpse = 0;
        PlayerPrefs.DeleteKey(KEY_CHEST);
        PlayerPrefs.Save();
        
        OnSessionChestChanged?.Invoke(0);
        OnSessionCorpseChanged?.Invoke(0);
        Debug.Log("<color=red>Currency Data Cleared!</color>");
    }
}

