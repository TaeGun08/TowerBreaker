using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class OutGameManager : SingletonBase<OutGameManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject gachaPanel;

    protected override void Awake()
    {
        
        Time.timeScale = 1.0f;
        PurgeAndRestoreEventSystem();

        dontDestroy = false; 
        base.Awake();
        
        if (Instance == this)
        {
            AutoAssignPanels();
        }
    }

    private void PurgeAndRestoreEventSystem()
    {
        
        EventSystem[] allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var es in allEventSystems)
        {
            DestroyImmediate(es.gameObject);
        }

        
        GameObject newES = new GameObject("EventSystem_Restored");
        newES.AddComponent<EventSystem>();
        newES.AddComponent<StandaloneInputModule>();
        Debug.Log("<color=green>[OutGameManager] EventSystem has been Purged and Restored.</color>");
    }

    private void AutoAssignPanels()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            if (mainPanel == null) mainPanel = canvas.transform.FindDeepChild("MainPanel")?.gameObject;
            if (equipmentPanel == null) equipmentPanel = canvas.transform.FindDeepChild("EquipmentPanel")?.gameObject;
            if (gachaPanel == null) gachaPanel = canvas.transform.FindDeepChild("GachaPanel")?.gameObject;
        }
    }

    private void Start()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        HideAllPanels();
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void ShowEquipmentPanel()
    {
        HideAllPanels();
        if (equipmentPanel != null) equipmentPanel.SetActive(true);
    }

    public void ShowGachaPanel()
    {
        HideAllPanels();
        if (gachaPanel != null) gachaPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        if (gachaPanel != null) gachaPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.ResetSessionData();
        SceneManager.LoadScene("InGame");
    }

    public void ResetAllGameData()
    {
        if (EquipmentManager.Instance != null) EquipmentManager.Instance.ClearAllEquipmentData();
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.ClearAllCurrencyData();
        ShowMainPanel();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("OutGame");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

