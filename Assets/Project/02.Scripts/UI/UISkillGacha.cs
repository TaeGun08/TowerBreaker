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
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillTierText;
    [SerializeField] private TextMeshProUGUI skillDescText;

    [Header("Selection Buttons")]
    [SerializeField] private GameObject selectionArea;
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button skipButton;

    private SkillData _drawnSkill;
    private const int GACHA_COST = 100;

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

        // [수정] 이름과 설명 텍스트 표시 로직 제거 (효과와 아이콘에만 집중)
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
        var skills = PlayerUnit.Instance.CurrentSkills;

        // [수정] 스킬 이름을 포함하지 않고 단순한 슬롯 번호만 표기
        if (slot1Button != null)
        {
            slot1Button.GetComponentInChildren<TextMeshProUGUI>().text = "Slot 1";
        }

        if (slot2Button != null)
        {
            slot2Button.GetComponentInChildren<TextMeshProUGUI>().text = "Slot 2";
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
