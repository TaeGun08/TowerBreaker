using System.Collections;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{
    [Header("AOE Settings")]
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private float damageRadius = 1.2f;
    [SerializeField] private int damage = 15;

    private void Start()
    {
        StartCoroutine(EffectSequence());
    }

    private IEnumerator EffectSequence()
    {
        // 시각적 피드백 (스케일이 커지거나 색이 변하는 등의 연출 추가 지점)
        transform.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / warningDuration;
            transform.localScale = Vector3.one * (t * damageRadius);
            yield return null;
        }

        // 실제 타격 판정
        if (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning)
        {
            float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            if (dist <= damageRadius)
            {
                Debug.Log("<color=red>Player caught in explosion!</color>");
                // TakeDamage에서 가드 여부를 체크하므로, 가드 중이면 데미지를 입지 않습니다.
                PlayerUnit.Instance.TakeDamage(damage); 
                
                // 플레이어가 가드 중이 아닐 때만 강한 흔들림과 히트스탑 적용
                if (!PlayerUnit.Instance.IsActionActive) // 가드나 대쉬 중이 아닐 때
                {
                    CameraManager.Instance.Shake(0.2f, 0.15f);
                    StageManager.Instance.TriggerHitStop(0.05f);
                }
            }
        }

        // 이펙트 후 사라짐
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
