using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIGameOverPanel : MonoBehaviour
{
    [Header("Result Summary")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI chestText;
    [SerializeField] private TextMeshProUGUI corpseText;

    [Header("Buttons")]
    [SerializeField] private Button menuButton;

    private void Awake()
    {
        if (menuButton != null) menuButton.onClick.AddListener(OnClickMenu);
    }

    private void OnEnable()
    {
        RefreshResults();
    }

    private void RefreshResults()
    {
        // 1. 도달한 층수 표시 (복구)
        if (StageManager.Instance != null && floorText != null)
        {
            floorText.text = $"REACHED FLOOR: {StageManager.Instance.StageCount + 1}";
        }

        // 2. 이번 세션 획득 보상 표시
        if (CurrencyManager.Instance != null)
        {
            if (chestText != null) chestText.text = $"CHESTS: {CurrencyManager.Instance.SessionChests}";
            if (corpseText != null) corpseText.text = $"CORPSE: {CurrencyManager.Instance.SessionCorpse}";
        }
    }

    private void OnClickMenu()
    {
        // 시간 정상화 후 메인 메뉴로 복귀
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("OutGame");
    }
}
