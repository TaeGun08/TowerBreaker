using UnityEngine;

public class ShieldBearer : Monster
{
    [Header("Shield Settings")]
    [SerializeField] [Range(0, 1)] private float damageReduction = 0.5f; 
    [SerializeField] private float blockChance = 0.2f; 

    
    public override void TakeDamage(int damage, bool isCrit = false, bool isProjectile = false)
    {
        
        if (Random.value < blockChance)
        {
            if (InGameUIManager.Instance != null)
                InGameUIManager.Instance.SpawnDamageText(transform.position, 0, false);
            
            if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.03f, 0.05f);
            return;
        }

        
        int reducedDamage = Mathf.RoundToInt(damage * (1f - damageReduction));
        base.TakeDamage(reducedDamage, isCrit, isProjectile);
    }
}

