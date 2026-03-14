using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_HolyHeal", menuName = "SkillsSO/HolyHeal")]
public class SkillData_HolyHeal : SkillData
{
    public override SkillTier Tier => SkillTier.Rare;
    public int healAmount = 20;

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.Heal(healAmount);
        
        // 힐 이펙트가 있다면 여기서 소환
        // if (healEffectPrefab != null) Instantiate(healEffectPrefab, player.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);
    }
}
