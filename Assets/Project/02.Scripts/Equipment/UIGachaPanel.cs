using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGachaPanel : MonoBehaviour
{
    // 내부에서 자동으로 찾을 요소들
    private TextMeshProUGUI totalChestText;
    private TextMeshProUGUI itemNameText;
    private TextMeshProUGUI itemStatsText;
    private TextMeshProUGUI tierText;
    private Button drawButton;
    private Button backButton;
    private GameObject resultArea;
    private Image itemIcon;
    private Image frameImage;

    private void Awake()
    {
        AutoAssignUI();
        
        if (drawButton != null) drawButton.onClick.AddListener(OnClickDraw);
        if (backButton != null) backButton.onClick.AddListener(OnClickBack);
    }

    private void AutoAssignUI()
    {
        totalChestText = FindChild<TextMeshProUGUI>("Text_TotalChest");
        itemNameText = FindChild<TextMeshProUGUI>("Text_ItemName");
        itemStatsText = FindChild<TextMeshProUGUI>("Text_ItemStats");
        tierText = FindChild<TextMeshProUGUI>("Text_Tier");
        
        drawButton = FindChild<Button>("Btn_Draw");
        backButton = FindChild<Button>("Btn_Back");
        
        resultArea = transform.FindDeepChild("ResultArea")?.gameObject;
        itemIcon = FindChild<Image>("Img_ItemIcon");
        frameImage = FindChild<Image>("Img_Frame");
    }

    private T FindChild<T>(string name) where T : Component
    {
        Transform target = transform.FindDeepChild(name);
        return target != null ? target.GetComponent<T>() : null;
    }

    private void OnEnable()
    {
        RefreshCurrency();
        if (resultArea != null) resultArea.SetActive(false);
        if (drawButton != null) drawButton.gameObject.SetActive(true);
    }

    private void RefreshCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            totalChestText.text = $"My Chests: {CurrencyManager.Instance.TotalChests}";
        }
    }

    public void OnClickDraw()
    {
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.TotalChests < 1)
        {
            if (totalChestText != null) totalChestText.text = "<color=red>Not enough Chests!</color>";
            return;
        }

        StartCoroutine(DrawSequence());
    }

    private IEnumerator DrawSequence()
    {
        drawButton.interactable = false;
        resultArea.SetActive(false);
        
        if (itemNameText != null) itemNameText.text = "Opening Chest...";
        
        // 짧은 대기 (연출)
        yield return new WaitForSeconds(1.0f);

        EquipmentData result = EquipmentGachaManager.Instance.DrawEquipment();
        if (result != null)
        {
            ShowResult(result);
            RefreshCurrency();
        }
        
        drawButton.interactable = true;
    }

    private void ShowResult(EquipmentData data)
    {
        resultArea.SetActive(true);
        
        if (itemIcon != null) itemIcon.sprite = data.icon;
        if (frameImage != null) frameImage.color = data.GetTierColor();
        
        itemNameText.text = data.equipmentName;
        tierText.text = $"[{data.tier.ToString().ToUpper()}]";
        tierText.color = data.GetTierColor();

        string stats = "";
        if (data.atkBonus != 0) stats += $"ATK +{data.atkBonus} ";
        if (data.defBonus != 0) stats += $"DEF +{data.defBonus} ";
        if (data.hpBonus != 0) stats += $"HP +{data.hpBonus} ";
        if (data.critBonus > 0) stats += $"CRIT +{data.critBonus * 100}% ";
        if (data.doubleHitBonus > 0) stats += $"DBL +{data.doubleHitBonus * 100}% ";
        
        itemStatsText.text = stats;
    }

    public void OnClickBack()
    {
        OutGameManager.Instance.ShowMainPanel();
    }
}
