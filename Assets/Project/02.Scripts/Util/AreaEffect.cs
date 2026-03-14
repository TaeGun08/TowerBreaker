using System.Collections;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{
    [Header("AOE Settings")]
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private float damageRadius = 1.5f;
    [SerializeField] private int damage = 20;
    
    [Header("Visuals")]
    [SerializeField] private GameObject warningVisual; 
    [SerializeField] private GameObject explosionVisual; 

    private void Start()
    {
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        
        if (warningVisual != null) warningVisual.SetActive(true);
        if (explosionVisual != null) explosionVisual.SetActive(false);

        yield return new WaitForSeconds(warningDuration);

        
        if (warningVisual != null) warningVisual.SetActive(false);
        if (explosionVisual != null) explosionVisual.SetActive(true);

        
        if (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning)
        {
            float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            if (dist <= damageRadius)
            {
                
                PlayerUnit.Instance.TakeDamage(damage, isProjectile: true); 
                
                
                if (!PlayerUnit.Instance.IsActionActive)
                {
                    if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.15f);
                    if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.05f);
                }
            }
        }

        
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

