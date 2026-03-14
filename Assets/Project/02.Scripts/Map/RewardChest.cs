using UnityEngine;

public class RewardChest : MonoBehaviour, IDamageable
{
    private bool _isAbsorbed = false;

    
    public int CurrentHP => _isAbsorbed ? 0 : 1;

    
    public void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        if (_isAbsorbed) return;
        StartAbsorb();
    }

    public void StartAbsorb()
    {
        if (_isAbsorbed) return;
        _isAbsorbed = true;

        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddChest(1);
        }

        
        if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.15f, 0.15f);

        
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

