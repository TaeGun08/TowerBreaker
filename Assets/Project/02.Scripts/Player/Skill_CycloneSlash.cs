using System.Collections;
using UnityEngine;

public class Skill_CycloneSlash : SkillBase
{
    public Skill_CycloneSlash()
    {
        SkillName = "Cyclone Slash";
        Cooldown = 7.0f;
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        if (!IsReady) yield break;
        StartCooldown();

        int hitCount = 3;
        float interval = 0.2f;

        for (int i = 0; i < hitCount; i++)
        {
            // 회전 애니메이션 느낌을 위해 캐릭터를 살짝 좌우로 흔들거나 스케일 조절 가능 (여기선 로직 집중)
            Swarm swarm = StageManager.Instance?.CurrentSwarm;
            if (swarm != null)
            {
                bool hit = swarm.AttackInRange(player.transform.position.x, 2.0f, 15, true);
                if (hit)
                {
                    CameraManager.Instance.Shake(0.1f, 0.1f);
                    // 마지막 타격은 넉백 적용
                    if (i == hitCount - 1)
                    {
                        swarm.Knockback(1.5f, 0.3f);
                    }
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
