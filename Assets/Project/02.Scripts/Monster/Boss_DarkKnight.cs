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

    protected override void Update()
    {
        // 현재 활성화된 군집이 아니면 행동 중지
        if (!IsActiveSwarm())
        {
            IsMoveStop = true;
            return;
        }

        base.Update();

        if (!_isPatternRunning && PlayerUnit.Instance != null)
        {
            float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            // 일정 거리 안으로 들어오면 패턴 시작
            if (dist < 6.0f)
            {
                StartCoroutine(PatternCycle());
            }
        }
    }

    private IEnumerator PatternCycle()
    {
        _isPatternRunning = true;

        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            // 플레이어가 연출 중이면 대기
            if (PlayerUnit.Instance != null && PlayerUnit.Instance.IsTransitioning)
            {
                continue;
            }

            int rand = Random.Range(0, 2);
            if (rand == 0) yield return Pattern_DashAttack();
            else yield return Pattern_WideSlash();
        }
    }

    private IEnumerator Pattern_DashAttack()
    {
        Debug.Log("Dark Knight: Charge!");
        IsMoveStop = true; // 일반 이동 중단
        
        // 돌진 준비 연출 (살짝 뒤로 갔다가)
        Vector3 startPos = transform.position;
        Vector3 backPos = startPos + Vector3.right * 0.5f;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            transform.position = Vector3.Lerp(startPos, backPos, t);
            yield return null;
        }

        // 앞으로 돌진!
        Vector3 dashTarget = transform.position + Vector3.left * dashDistance;
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * dashSpeed;
            transform.position = Vector3.Lerp(backPos, dashTarget, t);
            
            // 돌진 중 플레이어 타격 체크
            if (PlayerUnit.Instance != null)
            {
                float d = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
                if (d < 0.8f)
                {
                    Debug.Log("Dark Knight: Crushed you!");
                    CameraManager.Instance.Shake(0.3f, 0.2f);
                    // 플레이어 넉백 연출은 나중에 PlayerUnit에 추가 가능
                    break;
                }
            }
            yield return null;
        }

        IsMoveStop = false;
    }

    private IEnumerator Pattern_WideSlash()
    {
        Debug.Log("Dark Knight: Perish!");
        IsMoveStop = true;

        // 베기 예고 연출 (기 모으기)
        yield return new WaitForSeconds(0.8f);

        // 넓은 범위 타격
        if (PlayerUnit.Instance != null)
        {
            float d = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            if (d <= slashRange)
            {
                Debug.Log("Player hit by wide slash!");
                CameraManager.Instance.Shake(0.2f, 0.1f);
                StageManager.Instance.TriggerHitStop(0.1f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        IsMoveStop = false;
    }
}
