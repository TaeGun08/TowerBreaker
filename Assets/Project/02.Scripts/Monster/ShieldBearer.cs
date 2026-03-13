using UnityEngine;

public class ShieldBearer : Monster
{
    [Header("Shield Settings")]
    [SerializeField] [Range(0, 1)] private float damageReduction = 0.5f; // 데미지 50% 감소
    [SerializeField] private float blockChance = 0.3f; // 30% 확률로 완전 방어

    public override void TakeDamage(int damage)
    {
        // 확률적으로 완전 방어
        if (Random.value < blockChance)
        {
            Debug.Log($"{gameObject.name} blocked the attack!");
            // 가드 이펙트나 사운드 추가 지점
            CameraManager.Instance.Shake(0.03f, 0.05f); // 작은 진동
            return;
        }

        // 데미지 감소 적용
        int reducedDamage = Mathf.RoundToInt(damage * (1f - damageReduction));
        base.TakeDamage(reducedDamage);
    }
    
    // ShieldBearer는 넉백에 더 강하게 설정할 수 있도록 
    // 나중에 Swarm의 Knockback 로직에서 Monster의 질량(Mass) 등을 체크하게 확장 가능
}
