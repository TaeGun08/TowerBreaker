using System.Collections;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{
    [Header("AOE Settings")]
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private float damageRadius = 1.5f;
    [SerializeField] private int damage = 20;
    
    [Header("Visuals")]
    [SerializeField] private GameObject warningVisual; // 예고 연출 (빨간 원 등)
    [SerializeField] private GameObject explosionVisual; // 실제 폭발 이펙트

    private void Start()
    {
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        // 1. 경고 연출
        if (warningVisual != null) warningVisual.SetActive(true);
        if (explosionVisual != null) explosionVisual.SetActive(false);

        yield return new WaitForSeconds(warningDuration);

        // 2. 폭발 연출
        if (warningVisual != null) warningVisual.SetActive(false);
        if (explosionVisual != null) explosionVisual.SetActive(true);

        // 3. 실제 타격 판정
        if (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning)
        {
            float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            if (dist <= damageRadius)
            {
                // [수정] TakeDamage 호출. 플레이어가 가드 중이면 내부에서 완전 방어됨.
                PlayerUnit.Instance.TakeDamage(damage); 
                
                // 플레이어가 가드 중이 아닐 때만 강한 피드백
                if (!PlayerUnit.Instance.IsActionActive)
                {
                    if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.15f);
                    if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.05f);
                }
            }
        }

        // 이펙트 후 사라짐
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
