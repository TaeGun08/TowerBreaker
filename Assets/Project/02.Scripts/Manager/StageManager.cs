using System;
using System.Collections;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    #region Events

    public event Action OnStageProgress;
    public event Action OnGameClear; 

    #endregion

    #region Serialized Fields

    [Header("Game State")]
    [field: SerializeField] public int StageCount { get; private set; } = 0; 
    [SerializeField] private int maxStageCount = 100;
    [SerializeField] private float autoProceedDelay = 0.8f; 
    
    #endregion

    #region Private Fields

    public bool IsTransitioning { get; private set; }
    private Swarm _currentSwarm;

    #endregion

    #region Properties

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

    #endregion

    #region Lifecycle

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
    }

    #endregion

    #region Stage Progression

    private void HandleSwarmCleared()
    {
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

        // 1. 보상 수집
        CollectAllRewards();
        yield return new WaitForSeconds(autoProceedDelay);

        // 2. 플레이어 이동 연출
        yield return StartCoroutine(MovePlayerToExit());

        // 3. 클리어한 층 판정 및 보상 처리
        int clearedFloor = StageCount + 1;
        bool wasBoss = (clearedFloor % 5 == 0); 
        
        ProcessFloorClearRewards(wasBoss);
        
        // 4. 다음 스테이지 시작
        NextStage(wasBoss);
        yield return new WaitForSeconds(0.5f); 
        IsTransitioning = false;
    }

    private IEnumerator MovePlayerToExit()
    {
        if (PlayerUnit.Instance == null) yield break;

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

    private void ProcessFloorClearRewards(bool wasBoss)
    {
        if (wasBoss)
        {
            if (SkillGachaManager.Instance != null)
                SkillGachaManager.Instance.OpenGachaUI();
        }
        else if (PlayerUnit.Instance != null)
        {
            string msg = PlayerUnit.Instance.UpgradeRandomStat();
            if (InGameUIManager.Instance != null) 
                InGameUIManager.Instance.ShowUpgradeNotice(msg);
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

    #endregion

    #region Reward Handling

    private void CollectAllRewards()
    {
        // 모든 시체 수집
        Corpse[] corpses = FindObjectsByType<Corpse>(FindObjectsSortMode.None);
        foreach (var corpse in corpses)
        {
            if (corpse != null) corpse.StartAbsorb();
        }

        // 미획득 보상 상자 수집
        RewardChest[] chests = FindObjectsByType<RewardChest>(FindObjectsSortMode.None);
        foreach (var chest in chests)
        {
            if (chest != null) chest.StartAbsorb();
        }
    }

    #endregion

    #region Visual Effects & HitStop

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

    #endregion
}
