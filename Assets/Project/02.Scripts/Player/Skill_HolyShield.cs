using System.Collections;
using UnityEngine;

public class Skill_HolyShield : SkillBase
{
    public Skill_HolyShield()
    {
        SkillName = "Holy Shield";
        Cooldown = 10.0f;
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        if (!IsReady) yield break;
        StartCooldown();

        // 2초간 완전 방어 및 무적
        float duration = 2.0f;
        player.IsInvulnerable = true;
        
        Debug.Log("<color=cyan>Holy Shield Active!</color>");
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            Swarm swarm = StageManager.Instance?.CurrentSwarm;
            if (swarm != null)
            {
                Monster front = swarm.GetFrontMonster();
                if (front != null)
                {
                    float dist = front.transform.position.x - player.transform.position.x;
                    // 적이 아주 가까이 오면 자동으로 강력하게 밀쳐냄 (반격 연출)
                    if (dist < 1.5f)
                    {
                        swarm.Knockback(2.5f, 0.2f);
                        CameraManager.Instance.Shake(0.08f, 0.05f);
                    }
                }
            }
            yield return null;
        }
        
        player.IsInvulnerable = false;
        Debug.Log("<color=cyan>Holy Shield Deactivated</color>");
    }
}
