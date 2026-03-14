using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_HolyHeal", menuName = "SkillsSO/HolyHeal")]
public class SkillData_HolyHeal : SkillData
{
    public override SkillTier Tier => SkillTier.Rare;
    public int healAmount = 20;

    private void OnEnable()
    {
        description = "Instantly restores 20 HP using sacred light.";
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.PlaySkillAnimation();
        player.SpawnSkillEffect(effectPrefab, Vector3.up * 0.5f);
        player.Heal(healAmount);
        
        yield return new WaitForSeconds(0.3f);
    }
}
