using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Config")]
    [SerializeField] private Monster monsterPrefab;
    [SerializeField] private int minCount = 1;
    [SerializeField] private int maxCount = 3;

    [Header("Spawn Positioning")]
    [Tooltip("발판(Ground)의 중심에서 얼마나 떨어진 위치에 생성할지 결정합니다.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("여러 마리 생성 시 몬스터들 간의 X축 간격을 조절합니다.")]
    [SerializeField] private float spacingX = 0.5f;

    /// <summary>
    /// 플랫폼에 몬스터 군집을 생성합니다.
    /// 발판 자체를 밀지 않도록 자식 오브젝트(SwarmContainer)를 생성하여 관리합니다.
    /// </summary>
    public void SpawnMonsterOnPlatform(GameObject platform)
    {
        if (monsterPrefab == null) return;

        // 발판 아래에 Swarm 전용 컨테이너가 있는지 확인
        Transform swarmTransform = platform.transform.Find("SwarmContainer");
        Swarm swarm;

        if (swarmTransform == null)
        {
            GameObject swarmObj = new GameObject("SwarmContainer");
            swarmObj.transform.SetParent(platform.transform);
            swarmObj.transform.localPosition = Vector3.zero;
            swarm = swarmObj.AddComponent<Swarm>();
        }
        else
        {
            swarm = swarmTransform.GetComponent<Swarm>();
        }

        int count = Random.Range(minCount, maxCount + 1);
        
        // MonsterSpawner에 설정된 위치(spawnOffset)와 간격(spacingX)을 전달하여 생성
        swarm.Spawn(monsterPrefab, count, spawnOffset, spacingX);
    }

    public void ClearMonsterOnPlatform(GameObject platform)
    {
        // 자식 오브젝트에서 Swarm을 찾아 제거
        Swarm swarm = platform.GetComponentInChildren<Swarm>();
        if (swarm != null)
        {
            swarm.Clear();
        }
    }
}
