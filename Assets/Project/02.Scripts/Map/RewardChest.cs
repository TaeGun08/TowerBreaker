using UnityEngine;

public class RewardChest : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject openEffectPrefab;
    private bool _isOpened = false;

    // IDamageable 인터페이스 구현
    public int CurrentHP => _isOpened ? 0 : 1;

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (_isOpened) return;
        Open();
    }

    private void Open()
    {
        _isOpened = true;
        
        // 1. 랜덤 능력치 상승
        string upgradeMsg = PlayerUnit.Instance.UpgradeRandomStat();
        
        // 2. UI 알림
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.ShowUpgradeNotice(upgradeMsg);
        }

        // 3. 연출
        if (openEffectPrefab != null) Instantiate(openEffectPrefab, transform.position, Quaternion.identity);
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.2f);

        // 4. 삭제
        Destroy(gameObject, 0.5f);
    }
}
