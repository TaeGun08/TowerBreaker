using System.Collections;
using UnityEngine;

public class Skill_LeapStrike : SkillBase
{
    public Skill_LeapStrike()
    {
        SkillName = "Leap Strike";
        Cooldown = 5.0f;
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        if (!IsReady) yield break;
        StartCooldown();

        // 도약 연출 (간단하게 위로 갔다가 앞으로 떨어지는 보간)
        Vector3 startPos = player.transform.position;
        Vector3 peakPos = startPos + Vector3.up * 2f + Vector3.right * 1f;
        Vector3 targetPos = startPos + Vector3.right * 2f;

        // 도약 중
        float elapsed = 0f;
        float duration = 0.4f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 베지어 곡선 느낌으로 도약
            Vector3 m1 = Vector3.Lerp(startPos, peakPos, t);
            Vector3 m2 = Vector3.Lerp(peakPos, targetPos, t);
            player.transform.position = Vector3.Lerp(m1, m2, t);
            yield return null;
        }

        // 착지 타격
        player.transform.position = targetPos;
        
        // 범위 공격 실행 (더 넓은 범위와 강한 데미지)
        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm != null)
        {
            bool hit = swarm.AttackInRange(player.transform.position.x, 2.5f, 30, true);
            if (hit)
            {
                // 강력한 연출 피드백
                StageManager.Instance.TriggerHitStop(0.15f);
                CameraManager.Instance.Shake(0.25f, 0.2f);
            }
        }
    }
}
