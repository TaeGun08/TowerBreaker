using UnityEngine;

public class ShieldBearer : Monster
{
    [Header("Shield Settings")]
    [SerializeField] [Range(0, 1)] private float damageReduction = 0.5f; 
    [SerializeField] private float blockChance = 0.2f; 

    // 부모 Monster의 변경된 TakeDamage 시그니처와 일치하도록 수정
    public override void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        // 1. 방패 막기 확률 체크 (완전 무시)
        if (Random.value < blockChance)
        {
            if (InGameUIManager.Instance != null)
                InGameUIManager.Instance.SpawnDamageText(transform.position, 0, false);
            
            if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.03f, 0.05f);
            return;
        }

        // 2. 데미지 감소 적용
        int reducedDamage = Mathf.RoundToInt(damage * (1f - damageReduction));
        base.TakeDamage(reducedDamage, isCrit, isProjectile);
    }
}
