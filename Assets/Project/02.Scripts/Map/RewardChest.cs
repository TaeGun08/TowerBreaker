using UnityEngine;

public class RewardChest : MonoBehaviour, IDamageable
{
    private bool _isAbsorbed = false;

    // IDamageable 인터페이스 구현
    public int CurrentHP => _isAbsorbed ? 0 : 1;

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (_isAbsorbed) return;
        StartAbsorb();
    }

    public void StartAbsorb()
    {
        if (_isAbsorbed) return;
        _isAbsorbed = true;

        // 타격 시 가벼운 흔들림 연출
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.15f, 0.15f);

        if (UIAbsorber.Instance != null)
        {
            // 상자 타겟(isChest = true)으로 날려보냄
            UIAbsorber.Instance.Absorb(gameObject, true, () => {
                // UI 도달 시 데이터 저장
                if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddChest(1);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
