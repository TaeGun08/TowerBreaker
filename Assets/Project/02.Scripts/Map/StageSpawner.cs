using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageScroller stageScroller;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private MonsterSpawner monsterSpawner;
    
    [Header("Platform Settings")]
    [SerializeField] private int initCount = 10;
    [SerializeField] private int bottomBufferCount = 2;
    [SerializeField] private Vector3 startOffset;
    [SerializeField] private float spacingY = 4.0f;

    private Queue<GameObject> _groundQueue = new Queue<GameObject>();
    private Vector3 _nextSpawnPos;
    private int _totalSpawnCount = 0; // 지금까지 생성된 총 발판 수

    private void Start()
    {
        _nextSpawnPos = startOffset + (Vector3.down * spacingY * bottomBufferCount);
        
        for (int i = 0; i < initCount; i++)
        {
            // 초기 생성 시에도 층 번호를 부여하여 스폰
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
        
        if (_groundQueue.Count > 0)
        {
            // 1. 가장 아래 발판을 위로 재배치
            GameObject oldGround = _groundQueue.Dequeue();
            oldGround.transform.localPosition = _nextSpawnPos;
            
            // 2. 새로운 층 번호로 몬스터 스폰 지시
            if (monsterSpawner != null)
            {
                // 플레이어가 도달할 층은 현재 스테이지 번호보다 앞서서 생성됨
                monsterSpawner.SpawnMonsterByFloor(_totalSpawnCount, oldGround);
            }
                
            _groundQueue.Enqueue(oldGround);
            _nextSpawnPos.y += spacingY;
            _totalSpawnCount++;

            // 3. 맨 아래로 내려간 발판 청소
            GameObject newBottomGround = _groundQueue.Peek();
            if (monsterSpawner != null)
            {
                monsterSpawner.ClearMonsterOnPlatform(newBottomGround);
            }

            UpdateCurrentSwarm();
        }

        // 맵 스크롤 연출
        stageScroller.Scroll(spacingY, 0.3f);
    }

    private void SpawnGround(bool spawnMonster)
    {
        GameObject obj = Instantiate(groundPrefab, stageScroller.transform);
        obj.transform.localPosition = _nextSpawnPos;

        if (spawnMonster && monsterSpawner != null)
        {
            monsterSpawner.SpawnMonsterByFloor(_totalSpawnCount, obj);
        }

        _groundQueue.Enqueue(obj);
        _nextSpawnPos.y += spacingY;
        _totalSpawnCount++;
    }

    private void UpdateCurrentSwarm()
    {
        if (StageManager.Instance == null) return;

        var array = _groundQueue.ToArray();
        if (array.Length > bottomBufferCount)
        {
            StageManager.Instance.CurrentSwarm = array[bottomBufferCount].GetComponentInChildren<Swarm>();
        }
    }
}
