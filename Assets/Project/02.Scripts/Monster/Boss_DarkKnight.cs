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

    protected virtual void Update()
    {
        // 현재 활성화된 군집이 아니면 행동 중지
        if (!IsInActiveSwarm())
        {
            IsMoveStop = true;
            return;
        }

        // Monster.Update()는 이제 존재하지 않으므로 호출하지 않음 (Swarm에서 처리)

        if (!_isPatternRunning && PlayerUnit.Instance != null)
        {
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            if (sqrDist < 36.0f)
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
            yield return new WaitForSeconds(3.0f);

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
        
        Vector3 startPos = transform.position;
        Vector3 backPos = startPos + Vector3.right * 0.5f;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            transform.position = Vector3.Lerp(startPos, backPos, t);
            yield return null;
        }

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
        yield return new WaitForSeconds(0.8f);

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

        yield return new WaitForSeconds(0.5f);
        IsMoveStop = false;
    }
}
