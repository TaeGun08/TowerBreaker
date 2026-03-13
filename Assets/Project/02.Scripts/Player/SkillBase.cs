using System.Collections;
using UnityEngine;

public abstract class SkillBase
{
    public string SkillName { get; protected set; }
    public float Cooldown { get; protected set; }
    public float LastUsedTime { get; protected set; }

    public bool IsReady => Time.time >= LastUsedTime + Cooldown;

    public abstract IEnumerator Execute(PlayerUnit player);

    protected void StartCooldown()
    {
        LastUsedTime = Time.time;
    }
}
