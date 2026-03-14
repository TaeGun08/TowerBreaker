using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Berserk", menuName = "SkillsSO/Berserk")]
public class SkillData_Berserk : SkillData
{
    public override SkillTier Tier => SkillTier.Legendary;
    public float duration = 10.0f;

    public override IEnumerator Execute(PlayerUnit player)
    {
        // 버프 효과 시작
        player.StartCoroutine(BerserkEffect(player));
        yield return null;
    }

    private IEnumerator BerserkEffect(PlayerUnit player)
    {
        // [Todo] 버프 시각적 연출 추가
        Debug.Log("<color=red>Berserk Activated!</color>");
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 로직상 처리는 PlayerUnit 등의 데미지 계산식에서 체크
            yield return null;
        }

        Debug.Log("<color=white>Berserk Ended.</color>");
    }
}
