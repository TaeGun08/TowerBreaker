using System.Collections;
using UnityEngine;

public class Boss_DarkKnight : Monster
{
    [Header("Dark Knight Patterns")]
    [SerializeField] private float dashSpeed = 10.0f;
    [SerializeField] private float dashDistance = 3.0f;
    [SerializeField] private float slashRange = 2.5f;
    [SerializeField] private int slashDamage = 25;

    private bool _isPatternRunning = false;
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    protected override void Update()
    {
        base.Update();

        // 현재 활성화된 군집이 아니면 행동 중지
        if (!IsInActiveSwarm())
        {
            IsMoveStop = true;
            return;
        }

        if (!_isPatternRunning && PlayerUnit.Instance != null)
        {
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            // 6.0f -> 8.0f (거리에 상관없이 더 적극적으로 패턴 시작)
            if (sqrDist < 64.0f) 
            {
                StartCoroutine(PatternCycle());
            }
        }
    }

    private IEnumerator PatternCycle()
    {
        _isPatternRunning = true;

        while (IsInActiveSwarm())
        {
            // 패턴 사이 대기 시간 조정 (2.0초 ~ 3.5초 랜덤)
            yield return new WaitForSeconds(Random.Range(2.0f, 3.5f));

            if (PlayerUnit.Instance != null && PlayerUnit.Instance.IsTransitioning)
            {
                continue;
            }

            int rand = Random.Range(0, 2);
            if (rand == 0) yield return Pattern_DashAttack();
            else yield return Pattern_WideSlash();
        }
        _isPatternRunning = false;
    }

    private IEnumerator Pattern_DashAttack()
    {
        IsMoveStop = true; 
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        
        // 돌진 전 예비 동작 (뒤로 살짝 물러남)
        Vector3 startPos = transform.position;
        Vector3 backPos = startPos + Vector3.right * 0.5f;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f; // 예비 동작 속도 약간 증가
            transform.position = Vector3.Lerp(startPos, backPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // 돌진 직전 멈춤 (긴장감)

        Vector3 dashTarget = transform.position + Vector3.left * dashDistance;
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * dashSpeed;
            transform.position = Vector3.Lerp(backPos, dashTarget, t);
            
            if (PlayerUnit.Instance != null)
            {
                float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
                if (sqrDist < 0.64f)
                {
                    PlayerUnit.Instance.TakeDamage(slashDamage);
                    if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.3f, 0.2f);
                    break;
                }
            }
            yield return null;
        }

        IsMoveStop = false;
    }

    private IEnumerator Pattern_WideSlash()
    {
        IsMoveStop = true;
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        
        // [수정] 1.1f -> 0.7f: 플레이어와 동일한 타이밍으로 조정
        yield return new WaitForSeconds(0.7f); 

        if (PlayerUnit.Instance != null)
        {
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            if (sqrDist <= slashRange * slashRange)
            {
                PlayerUnit.Instance.TakeDamage(slashDamage);
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.1f);
                if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.1f);
            }
        }

        yield return new WaitForSeconds(0.7f); // 후딜레이
        IsMoveStop = false;
    }
}
