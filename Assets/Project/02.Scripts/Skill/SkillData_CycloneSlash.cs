using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_CycloneSlash", menuName = "SkillsSO/CycloneSlash")]
public class SkillData_CycloneSlash : SkillData
{
    public override SkillTier Tier => SkillTier.Epic;
    public int hitCount = 3;
    public float interval = 0.2f;
    public float range = 2.0f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        for (int i = 0; i < hitCount; i++)
        {
            Swarm swarm = StageManager.Instance?.CurrentSwarm;
            if (swarm != null)
            {
                int damage = Mathf.RoundToInt(player.Stats.baseDamage * 0.8f);
                swarm.AttackInRange(player.transform.position.x, range, damage, false);
                
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.05f, 0.05f);
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
