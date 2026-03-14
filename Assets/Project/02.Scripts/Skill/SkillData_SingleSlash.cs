using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_SingleSlash", menuName = "SkillsSO/SingleSlash")]
public class SkillData_SingleSlash : SkillData
{
    public override SkillTier Tier => SkillTier.Common;
    public float damageMultiplier = 2.5f;

    private void OnEnable()
    {
        description = "Deals 250% damage to a single enemy with a rapid strike.";
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.IsInvulnerable = true;
        player.PlaySkillAnimation(); 

        
        yield return new WaitForSeconds(0.2f);
        player.SpawnSkillEffect(effectPrefab, Vector3.right * 0.8f); 

        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm != null)
        {
            Monster target = swarm.GetFrontMonster();
            if (target != null)
            {
                int damage = Mathf.RoundToInt(player.Stats.baseDamage * damageMultiplier);
                target.TakeDamage(damage, isCrit: true);
                
                
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.1f);
                if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.1f);
            }
        }

        player.IsInvulnerable = false;
    }
}

