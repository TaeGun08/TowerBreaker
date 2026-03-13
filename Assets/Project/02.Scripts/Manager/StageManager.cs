using System;
using System.Collections;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public event Action OnStageProgress;
    public event Action OnGameClear; 

    [Header("Game State")]
    [field: SerializeField] public int StageCount { get; private set; } = 1; 
    [SerializeField] private int maxStageCount = 100;
    [SerializeField] private float autoProceedDelay = 1.5f; // 클리어 후 자동 진행 대기 시간
    
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
        if (StageCount >= maxStageCount)
        {
            Debug.Log("<color=green>Congratulations! All Stages Cleared!</color>");
            OnGameClear?.Invoke();
            return;
        }
        
        // 터치를 기다리지 않고 자동으로 다음 층으로 이동하는 코루틴 시작
        if (!IsTransitioning)
        {
            StartCoroutine(AutoProceedToNextFloor());
        }
    }

    private IEnumerator AutoProceedToNextFloor()
    {
        IsTransitioning = true;

        // 1. 적 전멸 후 잠시 대기 (승리의 여운, 아이템 드롭 확인 등)
        yield return new WaitForSeconds(autoProceedDelay);

        // 2. 플레이어 자동 퇴장 연출
        if (PlayerUnit.Instance != null)
        {
            // 플레이어 퇴장이 완료될 때까지 대기하기 위해 콜백 사용
            bool playerMoved = false;
            PlayerUnit.Instance.MoveToNextFloorSequence(() => 
            {
                playerMoved = true;
            });

            yield return new WaitUntil(() => playerMoved);
        }

        // 3. 스테이지 스크롤 및 새로운 층 세팅
        NextStage();
        
        // 플레이어의 리스폰 연출은 StageSpawner/PlayerUnit 쪽 이벤트로 자연스럽게 이어지므로 
        // 여기서 Transitioning을 바로 풀지 않고 PlayerUnit 쪽에서 완료 시 풀도록 위임하거나, 
        // 맵 스크롤 시간만큼 대기 후 해제합니다.
        
        // 스테이지 스크롤 연출 시간 대기 (StageSpawner의 scroll duration과 맞춤)
        yield return new WaitForSeconds(0.4f); 
        
        IsTransitioning = false;
    }

    public void NextStage()
    {
        if (StageCount < maxStageCount)
        {
            StageCount++;
            OnStageProgress?.Invoke();
            Debug.Log($"<color=white>Entered Floor {StageCount}</color>");
        }
    }

    #region Combat Effects (Juice)

    public void TriggerHitStop(float duration)
    {
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }

    #endregion
}
