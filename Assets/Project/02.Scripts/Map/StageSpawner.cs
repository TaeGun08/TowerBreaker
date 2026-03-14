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
    private int _nextFloorToSpawn;

    private void Start()
    {
        _nextSpawnPos = startOffset + (Vector3.down * spacingY * bottomBufferCount);
        _nextFloorToSpawn = 0 - bottomBufferCount; 

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
        if (_groundQueue.Count == 0) return;

        
        if (stageScroller != null)
        {
            stageScroller.Scroll(spacingY, 0.4f);
        }

        
        GameObject oldGround = _groundQueue.Dequeue();
        oldGround.transform.localPosition = _nextSpawnPos;
        
        
        if (monsterSpawner != null)
        {
            monsterSpawner.SpawnMonsterByFloor(_nextFloorToSpawn, oldGround);
        }
            
        _groundQueue.Enqueue(oldGround);
        _nextSpawnPos.y += spacingY;
        _nextFloorToSpawn++;

        
        UpdateCurrentSwarm();
        
        
        GameObject bottomGround = _groundQueue.Peek();
        if (monsterSpawner != null)
        {
            monsterSpawner.ClearMonsterOnPlatform(bottomGround);
        }
    }

    private void SpawnGround()
    {
        GameObject obj = Instantiate(groundPrefab, stageScroller.transform);
        obj.transform.localPosition = _nextSpawnPos;

        if (_nextFloorToSpawn >= 0 && monsterSpawner != null)
        {
            monsterSpawner.SpawnMonsterByFloor(_nextFloorToSpawn, obj);
        }

        _groundQueue.Enqueue(obj);
        _nextSpawnPos.y += spacingY;
        _nextFloorToSpawn++;
    }

    private void UpdateCurrentSwarm()
    {
        if (StageManager.Instance == null) return;

        var array = _groundQueue.ToArray();
        
        if (array.Length > bottomBufferCount)
        {
            Swarm nextSwarm = array[bottomBufferCount].GetComponentInChildren<Swarm>();
            if (nextSwarm != null)
            {
                StageManager.Instance.CurrentSwarm = nextSwarm;
            }
        }
    }
}

