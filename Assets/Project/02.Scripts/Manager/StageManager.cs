using System;
using System.Collections;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public event Action OnStageProgress;

    [Header("Game State")]
    [field: SerializeField] public int StageCount { get; private set; }
    public bool IsWaitingForNext { get; private set; }
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

    private void HandleSwarmCleared() => IsWaitingForNext = true;

    private void Update()
    {
        // 현재 군집 관리 (기술적 역량: 다수 개체 이동 지시 위임)
        if (_currentSwarm != null)
        {
            _currentSwarm.OnTick(PlayerUnit.Instance);
        }

        // 스테이지 전환 대기
        if (IsWaitingForNext && !IsTransitioning)
        {
            if (Input.GetMouseButtonDown(0)) StartNextFloorTransition();
        }
    }

    #region Combat Effects (Juice)

    /// <summary>
    /// 타격 시 역경직(Hit Stop) 효과를 줍니다.
    /// </summary>
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

    #region Transitions

    private void StartNextFloorTransition()
    {
        if (IsTransitioning) return;
        IsTransitioning = true;
        IsWaitingForNext = false;

        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.MoveToNextFloorSequence(() => 
            {
                NextStage();
                IsTransitioning = false;
            });
        }
        else
        {
            NextStage();
            IsTransitioning = false;
        }
    }

    public void NextStage()
    {
        StageCount++;
        OnStageProgress?.Invoke();
    }

    #endregion
}
