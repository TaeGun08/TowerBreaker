using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillGachaManager : SingletonBase<SkillGachaManager>
{
    [Header("Skill Database (Assign in Inspector)")]
    [SerializeField] private List<SkillData> _allSkills = new List<SkillData>();

    public void OpenGachaUI()
    {
        // 게임 일시 정지
        Time.timeScale = 0f;
        
        // UI 오픈 (InGameUIManager를 통해 패널 활성화)
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.ShowSkillGachaPanel();
        }
    }

    public SkillData DrawRandomSkill()
    {
        if (_allSkills == null || _allSkills.Count == 0) return null;
        
        int rand = UnityEngine.Random.Range(0, _allSkills.Count);
        return _allSkills[rand];
    }

    public void CloseGachaUI()
    {
        Time.timeScale = 1.0f;
    }
}
