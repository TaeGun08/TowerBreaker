using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_HolyShield", menuName = "SkillsSO/HolyShield")]
public class SkillData_HolyShield : SkillData
{
    public override SkillTier Tier => SkillTier.Rare;
    public float duration = 2.0f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.IsInvulnerable = true;
        // 시각적 효과 (예: 실드 이펙트) 활성화 로직 추가 가능
        
        yield return new WaitForSeconds(duration);
        
        player.IsInvulnerable = false;
    }
}
