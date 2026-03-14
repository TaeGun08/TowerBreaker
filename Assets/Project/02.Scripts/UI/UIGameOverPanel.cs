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
        
        if (StageManager.Instance != null && floorText != null)
        {
            floorText.text = $"REACHED FLOOR: {StageManager.Instance.StageCount + 1}";
        }

        
        if (CurrencyManager.Instance != null)
        {
            if (chestText != null) chestText.text = $"CHESTS: {CurrencyManager.Instance.SessionChests}";
            if (corpseText != null) corpseText.text = $"CORPSE: {CurrencyManager.Instance.SessionCorpse}";
        }
    }

    private void OnClickMenu()
    {
        
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("OutGame");
    }
}

