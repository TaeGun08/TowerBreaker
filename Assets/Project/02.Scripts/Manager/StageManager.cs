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
    [SerializeField] private float autoProceedDelay = 0.8f; // 대기 시간 단축
    
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
        
        // 1. 적 처치 후 아주 짧게 대기
        yield return new WaitForSeconds(autoProceedDelay);

        // 2. 플레이어 퇴장 연출
        if (PlayerUnit.Instance != null)
        {
            bool playerMoved = false;
            PlayerUnit.Instance.MoveToNextFloorSequence(() => { playerMoved = true; });
            yield return new WaitUntil(() => playerMoved);
        }

        // 3. 내부 데이터 갱신 및 스크롤 시작
        bool wasBoss = ((StageCount + 1) % 5 == 0); 
        
        // NextStage() 내부에서 OnStageProgress가 발생하고, StageSpawner가 이를 받아 스크롤을 시작함
        NextStage(wasBoss);
        
        // 4. 스탯 보상 (보스 아닐 때만)
        if (!wasBoss && PlayerUnit.Instance != null)
        {
            string msg = PlayerUnit.Instance.UpgradeRandomStat();
            if (InGameUIManager.Instance != null) InGameUIManager.Instance.ShowUpgradeNotice(msg);
        }
        
        // 5. 스크롤 연출 완료 대기 (StageScroller와 시간 맞춤)
        yield return new WaitForSeconds(0.5f); 
        
        IsTransitioning = false;
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
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }
}
