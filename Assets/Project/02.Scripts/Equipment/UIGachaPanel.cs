using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGachaPanel : MonoBehaviour
{
    [Header("Currency View")]
    [SerializeField] private TextMeshProUGUI totalChestText;

    [Header("Gacha Controls")]
    [SerializeField] private Button drawButton;
    [SerializeField] private Button backButton;

    [Header("Result View Area")]
    [SerializeField] private GameObject resultArea;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image frameImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private TextMeshProUGUI tierText;

    private void Awake()
    {
        
        if (drawButton != null) drawButton.onClick.AddListener(OnClickDraw);
        if (backButton != null) backButton.onClick.AddListener(OnClickBack);
    }

    private void OnEnable()
    {
        RefreshCurrency();
        if (resultArea != null) resultArea.SetActive(false);
        if (drawButton != null) 
        {
            drawButton.gameObject.SetActive(true);
            drawButton.interactable = true;
        }
    }

    private void RefreshCurrency()
    {
        if (CurrencyManager.Instance != null && totalChestText != null)
        {
            totalChestText.text = $"MY CHESTS: {CurrencyManager.Instance.TotalChests}";
        }
    }

    private bool _isDrawing = false; 

    public void OnClickDraw()
    {
        if (_isDrawing) return; 

        
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.TotalChests < 1)
        {
            if (totalChestText != null) totalChestText.text = "<color=red>NOT ENOUGH CHESTS!</color>";
            return;
        }

        StartCoroutine(DrawSequence());
    }

    private IEnumerator DrawSequence()
    {
        _isDrawing = true;
        if (drawButton != null) drawButton.interactable = false;
        
        if (resultArea != null) resultArea.SetActive(false);
        if (itemNameText != null) 
        {
            itemNameText.gameObject.SetActive(true);
            itemNameText.text = "Opening Chest...";
        }
        
        yield return new WaitForSeconds(1.0f);

        if (EquipmentGachaManager.Instance != null)
        {
            EquipmentData result = EquipmentGachaManager.Instance.DrawEquipment();
            if (result != null)
            {
                ShowResult(result);
                RefreshCurrency();
            }
        }
        
        if (drawButton != null) drawButton.interactable = true;
        _isDrawing = false;
    }

    private void ShowResult(EquipmentData data)
    {
        if (data == null) return;
        if (resultArea != null) resultArea.SetActive(true);
        
        if (itemIcon != null) itemIcon.sprite = data.icon;
        if (frameImage != null) frameImage.color = data.GetTierColor();
        
        if (itemNameText != null) 
        {
            itemNameText.gameObject.SetActive(true);
            itemNameText.text = data.equipmentName;
        }

        if (tierText != null) 
        {
            tierText.text = $"[{data.tier.ToString().ToUpper()}]";
            tierText.color = data.GetTierColor();
        }

        string stats = "";
        if (data.atkBonus != 0) stats += $"ATK +{data.atkBonus} ";
        if (data.defBonus != 0) stats += $"DEF +{data.defBonus} ";
        if (data.hpBonus != 0) stats += $"HP +{data.hpBonus} ";
        if (data.critBonus > 0) stats += $"CRIT +{data.critBonus * 100}% ";
        if (data.doubleHitBonus > 0) stats += $"DBL +{data.doubleHitBonus * 100}% ";
        
        if (itemStatsText != null) itemStatsText.text = stats;
    }

    public void OnClickBack()
    {
        OutGameManager.Instance.ShowMainPanel();
    }
}

