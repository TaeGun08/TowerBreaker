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
        // [추가] 스테이지 클리어 시 체력 5 회복
        if (PlayerUnit.Instance != null) PlayerUnit.Instance.Heal(5);

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
        
        // 보스 클리어 시 스킬 뽑기 오픈 (게임 일시 정지 및 UI 팝업)
        if (wasBoss && SkillGachaManager.Instance != null)
        {
            SkillGachaManager.Instance.OpenGachaUI();
        }
        else if (!wasBoss && PlayerUnit.Instance != null)
        {
            // 일반 스테이지는 기존처럼 랜덤 스탯 강화
            string msg = PlayerUnit.Instance.UpgradeRandomStat();
            if (InGameUIManager.Instance != null) InGameUIManager.Instance.ShowUpgradeNotice(msg);
        }
        
        NextStage(wasBoss);
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
