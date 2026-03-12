using System;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    public event Action OnStageProgress;

    [field: SerializeField] public int StageCount { get; private set; }
    
    // 현재 진행 중인 발판의 몬스터 군집
    // public Swarm CurrentSwarm { get; set; }

    private void Update()
    {
        // Z키: 현재 군집의 맨 앞 몬스터 공격
        if (Input.GetKeyDown(KeyCode.Z))
        {
            /*
            if (CurrentSwarm != null && !CurrentSwarm.IsCleared)
            {
                CurrentSwarm.AttackFront();
            }
            */
        }

        // Space키: 군집 처치 완료 시 다음 스테이지로 진행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 임시로 무조건 다음 스테이지로 진행하도록 변경
            NextStage();
            /*
            if (CurrentSwarm == null || CurrentSwarm.IsCleared)
            {
                NextStage();
            }
            else
            {
                Debug.Log("[StageManager] 모든 몬스터를 처치해야 진행할 수 있습니다.");
            }
            */
        }
    }

    public void NextStage()
    {
        StageCount++;
        OnStageProgress?.Invoke();
    }
}
