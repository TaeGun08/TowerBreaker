using UnityEngine;

public class ShieldBearer : Monster
{
    [Header("Shield Settings")]
    [SerializeField] [Range(0, 1)] private float damageReduction = 0.5f; // 데미지 50% 감소
    [SerializeField] private float blockChance = 0.3f; // 30% 확률로 완전 방어

    public override void TakeDamage(int damage, bool isCrit = false)
    {
        // 확률적으로 완전 방어
        if (Random.value < blockChance)
        {
            Debug.Log($"{gameObject.name} blocked the attack!");
            if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.03f, 0.05f);
            return;
        }

        // 데미지 감소 적용
        int reducedDamage = Mathf.RoundToInt(damage * (1f - damageReduction));
        base.TakeDamage(reducedDamage, isCrit);
    }
}
