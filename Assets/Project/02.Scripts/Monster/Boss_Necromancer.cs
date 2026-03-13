using System.Collections;
using UnityEngine;

public class Boss_Necromancer : Monster
{
    [Header("Necromancer Patterns")]
    [SerializeField] private Monster minionPrefab;
    [SerializeField] private AreaEffect magicCirclePrefab;
    [SerializeField] private float patternInterval = 3.0f;
    [SerializeField] private float maintainDistance = 4.0f;

    private bool _isPatternRunning = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        if (PlayerUnit.Instance == null) return;

        // 현재 활성화된 군집이 아니면 행동 중지
        if (!IsActiveSwarm())
        {
            IsMoveStop = true;
            return;
        }

        if (IsMoveStop) return;

        float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);

        // 일정 거리를 유지하려고 시도 (너무 가까우면 멈춤)
        if (dist <= maintainDistance)
        {
            IsMoveStop = true;
        }
        else
        {
            IsMoveStop = false;
        }

        base.Update();

        if (!_isPatternRunning)
        {
            StartCoroutine(PatternCycle());
        }
    }

    private IEnumerator PatternCycle()
    {
        _isPatternRunning = true;
        yield return new WaitForSeconds(patternInterval);

        while (true)
        {
            // 플레이어가 연출 중이면 대기
            if (PlayerUnit.Instance != null && PlayerUnit.Instance.IsTransitioning)
            {
                yield return null;
                continue;
            }

            // 패턴 랜덤 선택
            int rand = Random.Range(0, 2);
            if (rand == 0) yield return Pattern_Summon();
            else yield return Pattern_CastMagic();

            yield return new WaitForSeconds(patternInterval);
        }
    }

    private IEnumerator Pattern_Summon()
    {
        Debug.Log("Boss: Rise, my servants!");
        // 보스 주변에 쫄개 2마리 소환
        for (int i = 0; i < 2; i++)
        {
            if (minionPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0, 0);
                Monster minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                // 소환된 하수인은 보스보다 앞에 서게 함
            }
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Pattern_CastMagic()
    {
        Debug.Log("Boss: Death comes for you!");
        if (magicCirclePrefab != null && PlayerUnit.Instance != null)
        {
            // 플레이어 발밑에 장판 생성
            Vector3 targetPos = PlayerUnit.Instance.transform.position;
            Instantiate(magicCirclePrefab, targetPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(1.5f);
    }
}
