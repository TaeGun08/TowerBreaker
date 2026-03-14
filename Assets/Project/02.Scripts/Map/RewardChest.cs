using UnityEngine;

public class RewardChest : MonoBehaviour, IDamageable
{
    private bool _isAbsorbed = false;

    // IDamageable 인터페이스 구현
    public int CurrentHP => _isAbsorbed ? 0 : 1;

    // 인터페이스의 변경된 시그니처와 일치하도록 수정
    public void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        if (_isAbsorbed) return;
        StartAbsorb();
    }

    public void StartAbsorb()
    {
        if (_isAbsorbed) return;
        _isAbsorbed = true;

        // 1. 데이터 즉시 반영 (가장 확실함)
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddChest(1);
        }

        // 2. 타격 연출
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.15f, 0.15f);

        // 3. UI 연출 시도
        if (UIAbsorber.Instance != null)
        {
            UIAbsorber.Instance.Absorb(gameObject, true, null);
        }
        else
        {
            Destroy(gameObject, 0.5f);
        }
    }
}
