using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Berserk", menuName = "SkillsSO/Berserk")]
public class SkillData_Berserk : SkillData
{
    public override SkillTier Tier => SkillTier.Legendary;
    public float duration = 10.0f;

    private void OnEnable()
    {
        description = "Doubles attack damage but doubles damage received for 10 seconds.";
    }

    public override IEnumerator Execute(PlayerUnit player)
    {
        player.PlaySkillAnimation();
        player.SpawnSkillEffect(effectPrefab, Vector3.up * 0.5f);

        
        player.StartCoroutine(BerserkEffect(player));
        yield return null;
    }

    private IEnumerator BerserkEffect(PlayerUnit player)
    {
        
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            yield return null;
        }

    }
}
