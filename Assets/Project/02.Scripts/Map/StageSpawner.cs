using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageScroller stageScroller;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private MonsterSpawner monsterSpawner;
    
    [Header("Platform Settings")]
    [SerializeField] private int initCount;
    [SerializeField] private int bottomBufferCount;
    [SerializeField] private Vector3 startOffset;
    [SerializeField] private float spacingY;

    private Queue<GameObject> groundQueue = new();
    private Vector3 nextSpawnPos;

    private void Start()
    {
        nextSpawnPos = startOffset + (Vector3.down * spacingY * bottomBufferCount);
        
        for (int i = 0; i < initCount; i++)
        {
            SpawnGround(i >= bottomBufferCount);
        }

        UpdateCurrentSwarm();

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageProgress += HandleStageProgress;
        }
    }

    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageProgress -= HandleStageProgress;
        }
    }

    private void HandleStageProgress()
    {
        if (stageScroller == null || stageScroller.IsMoving) return;
        
        // 1. 발판 재배치
        if (groundQueue.Count > 0)
        {
            // 가장 아래에 있던 발판을 꺼내서 위로 보냄
            GameObject oldGround = groundQueue.Dequeue();
            oldGround.transform.localPosition = nextSpawnPos;
            
            // 위로 올라간 발판에 몬스터 생성 위임
            if (monsterSpawner != null)
            {
                monsterSpawner.SpawnMonsterOnPlatform(oldGround);
            }
                
            groundQueue.Enqueue(oldGround);
            nextSpawnPos.y += spacingY;

            // 맨 아래로 내려간 발판의 몬스터 제거 위임
            GameObject newBottomGround = groundQueue.Peek();
            if (monsterSpawner != null)
            {
                monsterSpawner.ClearMonsterOnPlatform(newBottomGround);
            }

            // 새로운 '현재 발판' 군집 업데이트
            UpdateCurrentSwarm();
        }

        // 3. 스크롤 업데이트
        stageScroller.Scroll(spacingY, 0.3f);
    }

    private void SpawnGround(bool spawnMonster)
    {
        GameObject obj = Instantiate(groundPrefab, stageScroller.transform);
        obj.transform.localPosition = nextSpawnPos;

        if (spawnMonster && monsterSpawner != null)
        {
            monsterSpawner.SpawnMonsterOnPlatform(obj);
        }

        groundQueue.Enqueue(obj);
        nextSpawnPos.y += spacingY;
    }

    private void UpdateCurrentSwarm()
    {
        if (StageManager.Instance == null) return;

        var array = groundQueue.ToArray();
        if (array.Length > bottomBufferCount)
        {
            StageManager.Instance.CurrentSwarm = array[bottomBufferCount].GetComponent<Swarm>();
        }
    }
}
