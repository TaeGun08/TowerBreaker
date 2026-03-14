using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillGacha : MonoBehaviour
{
    [Header("Gacha Main")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button drawButton;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Result Info")]
    [SerializeField] private GameObject resultArea;
    [SerializeField] private Image drawnSkillIcon; // 뽑힌 스킬 아이콘
    [SerializeField] private TextMeshProUGUI skillTierText;

    [Header("Selection Buttons")]
    [SerializeField] private GameObject selectionArea;
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Image slot1Icon; // 슬롯 1 현재 스킬 아이콘
    [SerializeField] private Image slot2Icon; // 슬롯 2 현재 스킬 아이콘
    [SerializeField] private Button skipButton;

    private SkillData _drawnSkill;
    private const int GACHA_COST = 100;

    private void Awake()
    {
        // 버튼 자동 연결 (에디터 설정 실수 방지)
        if (drawButton != null) drawButton.onClick.AddListener(OnClickDraw);
        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnClickSlot(0));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnClickSlot(1));
        if (skipButton != null) skipButton.onClick.AddListener(OnClickSkip);
    }

    private void OnEnable()
    {
        ResetUI();
    }

    private void ResetUI()
    {
        if (costText != null) costText.text = $"{GACHA_COST} Corpse";
        if (drawButton != null) drawButton.gameObject.SetActive(true);
        if (resultArea != null) resultArea.SetActive(false);
        if (selectionArea != null) selectionArea.SetActive(false);
        if (messageText != null) messageText.text = "Spend Corpse to find a new skill!";
    }

    public void OnClickDraw()
    {
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.SessionCorpse < GACHA_COST)
        {
            if (messageText != null) messageText.text = "<color=red>Not enough Corpse!</color>";
            return;
        }

        CurrencyManager.Instance.UseCorpse(GACHA_COST);
        _drawnSkill = SkillGachaManager.Instance.DrawRandomSkill();
        ShowResult(_drawnSkill);
    }

    private void ShowResult(SkillData skill)
    {
        if (skill == null) return;
        
        if (drawButton != null) drawButton.gameObject.SetActive(false);
        if (resultArea != null) resultArea.SetActive(true);
        if (selectionArea != null) selectionArea.SetActive(true);

        if (drawnSkillIcon != null)
        {
            drawnSkillIcon.sprite = skill.icon;
            drawnSkillIcon.gameObject.SetActive(true);
        }

        if (skillTierText != null) skillTierText.text = $"[{skill.Tier.ToString().ToUpper()}]";

        // 등급별 색상 적용
        Color tierColor = Color.white;
        switch (skill.Tier)
        {
            case SkillTier.Rare: tierColor = Color.cyan; break;
            case SkillTier.Epic: tierColor = Color.magenta; break;
            case SkillTier.Legendary: tierColor = Color.red; break;
        }
        if (skillTierText != null) skillTierText.color = tierColor;

        UpdateSlotButtons();
    }

    private void UpdateSlotButtons()
    {
        if (PlayerUnit.Instance == null) return;
        var currentSkills = PlayerUnit.Instance.CurrentSkills;

        // 슬롯 1 설정
        if (slot1Button != null)
        {
            bool hasSkill = currentSkills.Count > 0;
            var tmp = slot1Button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = hasSkill ? "Replace Slot 1" : "Equip to Slot 1";
            
            if (slot1Icon != null)
            {
                slot1Icon.gameObject.SetActive(hasSkill);
                if (hasSkill && currentSkills[0] != null && currentSkills[0].Data != null) 
                    slot1Icon.sprite = currentSkills[0].Data.icon;
            }
        }

        // 슬롯 2 설정
        if (slot2Button != null)
        {
            bool hasSkill = currentSkills.Count > 1;
            var tmp = slot2Button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = hasSkill ? "Replace Slot 2" : "Equip to Slot 2";
            
            if (slot2Icon != null)
            {
                slot2Icon.gameObject.SetActive(hasSkill);
                if (hasSkill && currentSkills[1] != null && currentSkills[1].Data != null) 
                    slot2Icon.sprite = currentSkills[1].Data.icon;
            }
        }
    }

    public void OnClickSlot(int index)
    {
        if (PlayerUnit.Instance == null || _drawnSkill == null) return;

        if (index == 0)
        {
            if (PlayerUnit.Instance.CurrentSkills.Count < 1) PlayerUnit.Instance.AddSkill(_drawnSkill);
            else PlayerUnit.Instance.ReplaceSkill(0, _drawnSkill);
        }
        else
        {
            if (PlayerUnit.Instance.CurrentSkills.Count < 2) PlayerUnit.Instance.AddSkill(_drawnSkill);
            else PlayerUnit.Instance.ReplaceSkill(1, _drawnSkill);
        }

        Close();
    }

    public void OnClickSkip()
    {
        Close();
    }

    private void Close()
    {
        _drawnSkill = null;
        if (SkillGachaManager.Instance != null) SkillGachaManager.Instance.CloseGachaUI();
        gameObject.SetActive(false);
    }
}
