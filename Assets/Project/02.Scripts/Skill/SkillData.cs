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
    public Sprite icon; // 아이콘 복구
    public float cooldown;

    // 실제 스킬 로직 (상속받아 구현)
    public abstract IEnumerator Execute(PlayerUnit player);
}

// 실시간 쿨다운 상태를 관리하기 위한 런타임 클래스
public class SkillInstance
{
    public SkillData Data { get; private set; }
    public float LastUsedTime { get; private set; }

    public SkillInstance(SkillData data)
    {
        Data = data;
        LastUsedTime = -999f; // 바로 사용 가능하게 초기화
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
