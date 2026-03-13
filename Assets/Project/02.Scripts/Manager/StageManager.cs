using System;
using System.Collections;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public event Action OnStageProgress;
    public event Action OnGameClear; 

    [Header("Game State")]
    [field: SerializeField] public int StageCount { get; private set; } = 0; 
    [SerializeField] private int maxStageCount = 100;
    [SerializeField] private float autoProceedDelay = 0.8f; 
    
    public bool IsTransitioning { get; private set; }

    private Swarm _currentSwarm;

    public Swarm CurrentSwarm 
    { 
        get => _currentSwarm;
        set 
        {
            if (_currentSwarm != null)
            {
                _currentSwarm.SetMoveStop(true);
                _currentSwarm.OnCleared -= HandleSwarmCleared;
            }

            _currentSwarm = value;
            if (_currentSwarm == null) return;
            
            _currentSwarm.SetMoveStop(false);
            _currentSwarm.OnCleared += HandleSwarmCleared;
        }
    }

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
    }

    private void HandleSwarmCleared()
    {
        if (StageCount >= maxStageCount - 1)
        {
            OnGameClear?.Invoke();
            return;
        }
        
        if (!IsTransitioning)
        {
            StartCoroutine(AutoProceedToNextFloor());
        }
    }

    private IEnumerator AutoProceedToNextFloor()
    {
        IsTransitioning = true;

        // 적 전멸 후 모든 보상(시체, 상자) 수집
        CollectAllRewards();
        yield return new WaitForSeconds(autoProceedDelay);

        if (PlayerUnit.Instance != null)
        {
            bool playerMoved = false;
            float waitTimeout = 2.0f;
            float waitElapsed = 0f;

            PlayerUnit.Instance.MoveToNextFloorSequence(() => { playerMoved = true; });
            
            while (!playerMoved && waitElapsed < waitTimeout)
            {
                waitElapsed += Time.deltaTime;
                yield return null;
            }
        }

        bool wasBoss = ((StageCount + 1) % 5 == 0); 
        NextStage(wasBoss);
        
        if (!wasBoss && PlayerUnit.Instance != null)
        {
            string msg = PlayerUnit.Instance.UpgradeRandomStat();
            if (InGameUIManager.Instance != null) InGameUIManager.Instance.ShowUpgradeNotice(msg);
        }
        
        yield return new WaitForSeconds(0.5f); 
        IsTransitioning = false;
    }

    private void CollectAllRewards()
    {
        // 1. 모든 시체 수집
        Corpse[] corpses = FindObjectsByType<Corpse>(FindObjectsSortMode.None);
        foreach (var corpse in corpses)
        {
            if (corpse != null) corpse.StartAbsorb();
        }

        // 2. 미획득 보상 상자 수집 추가
        RewardChest[] chests = FindObjectsByType<RewardChest>(FindObjectsSortMode.None);
        foreach (var chest in chests)
        {
            if (chest != null) chest.StartAbsorb();
        }
    }

    public void NextStage(bool wasBoss = false)
    {
        if (StageCount < maxStageCount - 1)
        {
            StageCount++;
            OnStageProgress?.Invoke();
            Debug.Log($"<color=white>Entered Floor {StageCount + 1}</color>");
        }
    }

    public void TriggerHitStop(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StopCoroutine("HitStopCoroutine");
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f; 
    }
}
