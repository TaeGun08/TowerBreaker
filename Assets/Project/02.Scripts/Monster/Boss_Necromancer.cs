using System.Collections;
using UnityEngine;

public class Boss_Necromancer : Monster
{
    [Header("Necromancer Patterns")]
    [SerializeField] private Monster minionPrefab;
    [SerializeField] private AreaEffect magicCirclePrefab;
    [SerializeField] private float patternInterval = 3.0f;

    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");
    private bool _isPatternRunning;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        // 1. 기본 이동 로직 (부모 Monster.Update 사용)
        base.Update();

        if (PlayerUnit.Instance == null || !IsInActiveSwarm()) return;

        // 2. 패턴 사이클 관리
        if (!_isPatternRunning)
        {
            StartCoroutine(PatternCycle());
        }
    }

    protected override void HandleMovement()
    {
        // 네크로맨서는 멀리서 마법을 써야 하므로 정지 거리를 멀게 설정 (3.5f)
        float stopDist = 3.5f;
        float distance = transform.position.x - PlayerUnit.Instance.transform.position.x;

        if (distance > stopDist)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
        }
    }

    private IEnumerator PatternCycle()
    {
        _isPatternRunning = true;
        yield return new WaitForSeconds(patternInterval);

        while (IsInActiveSwarm() && PlayerUnit.Instance != null)
        {
            if (PlayerUnit.Instance.IsTransitioning)
            {
                yield return null;
                continue;
            }

            int rand = Random.Range(0, 2);
            if (rand == 0) yield return Pattern_Summon();
            else yield return Pattern_CastMagic();

            yield return new WaitForSeconds(patternInterval);
        }
        _isPatternRunning = false;
    }

    private IEnumerator Pattern_Summon()
    {
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        yield return new WaitForSeconds(0.5f);

        // 소환 개수를 2개로 줄여 밸런스 조정
        for (int i = 0; i < 2; i++)
        {
            if (minionPrefab != null && MySwarm != null)
            {
                // 보스 뒤쪽에서 소환
                float spawnX = transform.position.x + 1.0f + Random.Range(0f, 1.0f);
                Vector3 spawnPos = new Vector3(spawnX, transform.position.y, 0);
                
                Monster minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                minion.SetDifficulty(currentFloorCount);
                MySwarm.AddMonster(minion);
            }
        }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Pattern_CastMagic()
    {
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        yield return new WaitForSeconds(0.5f);

        if (magicCirclePrefab != null && PlayerUnit.Instance != null)
        {
            Vector3 targetPos = PlayerUnit.Instance.transform.position;
            // 플레이어 발밑에 마법 생성
            Instantiate(magicCirclePrefab, targetPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(1.0f);
    }
}
