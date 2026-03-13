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
        base.Update();

        if (PlayerUnit.Instance == null || !IsInActiveSwarm()) return;

        if (!_isPatternRunning)
        {
            StartCoroutine(PatternCycle());
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

        for (int i = 0; i < 2; i++)
        {
            if (minionPrefab != null && MySwarm != null)
            {
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
            Instantiate(magicCirclePrefab, targetPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(1.0f);
    }
}
