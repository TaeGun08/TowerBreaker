using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageScroller stageScroller;
    [SerializeField] private GameObject groundPrefab;
    
    [Header("Platform Settings")]
    [SerializeField] private int initCount = 10;
    [SerializeField] private int bottomBufferCount = 3;
    [SerializeField] private Vector3 startOffset;
    [SerializeField] private float spacingY = 5f;

    private Queue<GameObject> groundQueue = new();
    private Vector3 nextSpawnPos;

    private void Start()
    {
        nextSpawnPos = startOffset + (Vector3.down * spacingY * bottomBufferCount);
        
        for (int i = 0; i < initCount; i++)
        {
            SpawnGround();
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
        if (stageScroller != null && !stageScroller.IsMoving)
        {
            // 1. 발판 재배치
            if (groundQueue.Count > 0)
            {
                GameObject oldGround = groundQueue.Dequeue();
                oldGround.transform.localPosition = nextSpawnPos;
                
                // 2. 기존 군집 제거 및 새 군집 생성 (MonsterSpawner 활용)
                MonsterSpawner spawner = oldGround.GetComponentInChildren<MonsterSpawner>();
                if (spawner != null)
                {
                    Swarm oldSwarm = oldGround.GetComponentInChildren<Swarm>();
                    if (oldSwarm != null) Destroy(oldSwarm.gameObject);
                    
                    spawner.SpawnSwarm();
                }
                
                groundQueue.Enqueue(oldGround);
                nextSpawnPos.y += spacingY;
            }

            // 3. 스크롤 및 현재 군집 업데이트
            stageScroller.Scroll(spacingY, 0.3f);
            Invoke(nameof(UpdateCurrentSwarm), 0.35f);
        }
    }

    private void UpdateCurrentSwarm()
    {
        GameObject[] grounds = groundQueue.ToArray();
        if (grounds.Length > bottomBufferCount)
        {
            GameObject currentGround = grounds[bottomBufferCount]; 
            StageManager.Instance.CurrentSwarm = currentGround.GetComponentInChildren<Swarm>();
        }
    }

    private void SpawnGround()
    {
        GameObject obj = Instantiate(groundPrefab, stageScroller.transform);
        obj.transform.localPosition = nextSpawnPos;
        
        MonsterSpawner spawner = obj.GetComponentInChildren<MonsterSpawner>();
        if (spawner != null)
        {
            spawner.SpawnSwarm();
        }

        groundQueue.Enqueue(obj);
        nextSpawnPos.y += spacingY;
    }
}
