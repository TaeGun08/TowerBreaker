using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_LeapStrike", menuName = "SkillsSO/LeapStrike")]
public class SkillData_LeapStrike : SkillData
{
    public override SkillTier Tier => SkillTier.Epic;

    private void OnEnable()
    {
        description = "Jump high and smash the ground, dealing AoE damage upon landing.";
    }
    public float jumpHeight = 2.0f;
    public float jumpDistance = 1.0f;
    public float damageMultiplier = 2.0f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.IsInvulnerable = true;
        player.PlaySkillAnimation(); 

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = startPos + Vector3.right * jumpDistance;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight + Vector3.right * (jumpDistance * 0.5f);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f;
            Vector3 m1 = Vector3.Lerp(startPos, peakPos, t);
            Vector3 m2 = Vector3.Lerp(peakPos, targetPos, t);
            Vector3 nextPos = Vector3.Lerp(m1, m2, t);

            
            if (player.IsMonsterAhead(nextPos, 0.4f))
            {
                targetPos = nextPos; 
                break;
            }

            player.transform.position = nextPos;
            yield return null;
        }

        
        player.transform.position = targetPos;
        player.SpawnSkillEffect(effectPrefab, Vector3.zero); 

        Swarm swarm = StageManager.Instance?.CurrentSwarm;
        if (swarm != null)
        {
            int damage = Mathf.RoundToInt(player.Stats.baseDamage * damageMultiplier);
            swarm.AttackInRange(targetPos.x, 1.5f, damage, true);
            if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.15f, 0.1f);
        }

        player.IsInvulnerable = false;
    }
}

