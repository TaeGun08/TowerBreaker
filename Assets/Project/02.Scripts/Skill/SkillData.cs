using System.Collections;
using UnityEngine;

public enum SkillTier
{
    Common, Rare, Epic, Legendary
}

public abstract class SkillData : ScriptableObject
{
    [Header("Skill Settings")]
    public abstract SkillTier Tier { get; } 
    public Sprite icon;
    public float cooldown;
    [TextArea] public string description; 
    public GameObject effectPrefab; 

    
    public abstract IEnumerator Execute(PlayerUnit player);
}


public class SkillInstance
{
    public SkillData Data { get; private set; }
    public float LastUsedTime { get; private set; }

    public SkillInstance(SkillData data)
    {
        Data = data;
        LastUsedTime = -999f; 
    }

    public bool IsReady => Time.time >= LastUsedTime + Data.cooldown;

    public void Use(PlayerUnit player)
    {
        if (IsReady)
        {
            LastUsedTime = Time.time;
            player.StartCoroutine(Data.Execute(player));
        }
    }
}

