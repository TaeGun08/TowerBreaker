using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameManager : SingletonBase<OutGameManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject gachaPanel;

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        AutoAssignPanels();
    }

    private void AutoAssignPanels()
    {
        // 이름 기반으로 패널 자동 할당
        if (mainPanel == null) mainPanel = transform.parent.FindDeepChild("MainPanel")?.gameObject;
        if (equipmentPanel == null) equipmentPanel = transform.parent.FindDeepChild("EquipmentPanel")?.gameObject;
        if (gachaPanel == null) gachaPanel = transform.parent.FindDeepChild("GachaPanel")?.gameObject;
    }

    private void Start()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        HideAllPanels();
        mainPanel.SetActive(true);
    }

    public void ShowEquipmentPanel()
    {
        HideAllPanels();
        equipmentPanel.SetActive(true);
    }

    public void ShowGachaPanel()
    {
        HideAllPanels();
        gachaPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        if (gachaPanel != null) gachaPanel.SetActive(false);
    }

    public void StartGame()
    {
        // [추가] 새 게임 시작 시 세션 재화 초기화
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.ResetSessionData();

        // 인게임 씬으로 이동
        SceneManager.LoadScene("InGame");
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
