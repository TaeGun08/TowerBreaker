using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_HolyShield", menuName = "SkillsSO/HolyShield")]
public class SkillData_HolyShield : SkillData
{
    public override SkillTier Tier => SkillTier.Rare;
    public float duration = 2.0f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.PlaySkillAnimation();
        player.SpawnSkillEffect(effectPrefab, Vector3.up * 0.5f);
        player.IsInvulnerable = true;
        
        yield return new WaitForSeconds(duration);
        
        player.IsInvulnerable = false;
    }
}
