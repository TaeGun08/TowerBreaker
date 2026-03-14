using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_SingleSlash", menuName = "SkillsSO/SingleSlash")]
public class SkillData_SingleSlash : SkillData
{
    public override SkillTier Tier => SkillTier.Common;
    public float damageMultiplier = 2.5f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.IsInvulnerable = true;

        // 약간의 선딜레이
        yield return new WaitForSeconds(0.2f);

        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm != null)
        {
            Monster target = swarm.GetFrontMonster();
            if (target != null)
            {
                int damage = Mathf.RoundToInt(player.Stats.baseDamage * damageMultiplier);
                target.TakeDamage(damage, isCrit: true);
                
                // 타격 연출
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.1f);
                if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.1f);
            }
        }

        player.IsInvulnerable = false;
    }
}
