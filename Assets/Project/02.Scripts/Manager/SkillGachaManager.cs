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
        
        Time.timeScale = 0f;
        
        
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

