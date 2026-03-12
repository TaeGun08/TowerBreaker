using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageScroller stageScroller;
    [SerializeField] private GameObject groundPrefab;
    
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
            SpawnGround();
        }

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
            GameObject oldGround = groundQueue.Dequeue();
            oldGround.transform.localPosition = nextSpawnPos;
                
            groundQueue.Enqueue(oldGround);
            nextSpawnPos.y += spacingY;
        }

        // 3. 스크롤 및 현재 군집 업데이트
        stageScroller.Scroll(spacingY, 0.3f);
    }

    private void SpawnGround()
    {
        GameObject obj = Instantiate(groundPrefab, stageScroller.transform);
        obj.transform.localPosition = nextSpawnPos;

        groundQueue.Enqueue(obj);
        nextSpawnPos.y += spacingY;
    }
}
