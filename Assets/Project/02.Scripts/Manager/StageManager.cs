using System;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public event Action OnStageProgress;

    [field: SerializeField] public int StageCount { get; private set; }
    
    // 현재 진행 중인 발판의 몬스터 군집
    private Swarm currentSwarm;
    public Swarm CurrentSwarm 
    { 
        get => currentSwarm;
        set 
        {
            // 이전 군집 처리
            if (currentSwarm != null)
            {
                currentSwarm.SetMoveStop(true);
                currentSwarm.OnCleared -= NextStage; // 이벤트 해제
            }

            currentSwarm = value;

            // 새로운 현재 군집 처리
            if (currentSwarm == null) return;
            currentSwarm.SetMoveStop(false);
            currentSwarm.OnCleared += NextStage; // 다 죽으면 다음 스테이지로
        }
    }

    private void Update()
    {
        // Space키: 현재 군집의 맨 앞 몬스터 공격
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        if (CurrentSwarm != null && !CurrentSwarm.IsCleared)
        {
            // 임시로 데미지 10 부여
            CurrentSwarm.AttackFront(10);
        }
    }

    public void NextStage()
    {
        StageCount++;
        OnStageProgress?.Invoke();
    }
}
