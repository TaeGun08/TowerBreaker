using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Regular Monsters")]
    [SerializeField] private Monster commonMonsterPrefab;
    [SerializeField] private Monster[] eliteMonsterPrefabs;
    [SerializeField] [Range(0, 1)] private float eliteSpawnChance = 0.25f;

    [Header("Boss Monsters")]
    [SerializeField] private Monster[] bossPrefabs;
    [SerializeField] private int bossFloorInterval = 5; 

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float spacingX = 0.6f;
    [SerializeField] private int baseMinCount = 1;
    [SerializeField] private int baseMaxCount = 3;

    public void SpawnMonsterByFloor(int floorCount, GameObject platform)
    {
        Swarm swarm = GetOrCreateSwarm(platform);
        
        bool isBossFloor = ((floorCount + 1) % bossFloorInterval == 0);

        if (isBossFloor) SpawnBossSwarm(swarm, floorCount);
        else SpawnMixedSwarm(swarm, floorCount);
    }

    private void SpawnBossSwarm(Swarm swarm, int floorCount)
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;
        Monster selectedBoss = bossPrefabs[Random.Range(0, bossPrefabs.Length)];
        swarm.SpawnMixed(new[] { selectedBoss }, spawnOffset, 0, floorCount);
    }

    private void SpawnMixedSwarm(Swarm swarm, int floorCount)
    {
        if (commonMonsterPrefab == null) return;

        
        int bonusCount = floorCount / 10;
        int totalCount = Random.Range(baseMinCount, baseMaxCount + bonusCount + 1);

        Monster[] prefabsToSpawn = new Monster[totalCount];
        for (int i = 0; i < totalCount; i++)
        {
            if (eliteMonsterPrefabs != null && eliteMonsterPrefabs.Length > 0 && Random.value < eliteSpawnChance)
                prefabsToSpawn[i] = eliteMonsterPrefabs[Random.Range(0, eliteMonsterPrefabs.Length)];
            else
                prefabsToSpawn[i] = commonMonsterPrefab;
        }

        swarm.SpawnMixed(prefabsToSpawn, spawnOffset, spacingX, floorCount);
    }

    private Swarm GetOrCreateSwarm(GameObject platform)
    {
        Transform swarmTransform = platform.transform.Find("SwarmContainer");
        if (swarmTransform == null)
        {
            GameObject swarmObj = new GameObject("SwarmContainer");
            swarmObj.transform.SetParent(platform.transform);
            swarmObj.transform.localPosition = Vector3.zero;
            return swarmObj.AddComponent<Swarm>();
        }
        
        
        swarmTransform.localPosition = Vector3.zero;
        return swarmTransform.GetComponent<Swarm>();
    }

    public void ClearMonsterOnPlatform(GameObject platform)
    {
        Swarm swarm = platform.GetComponentInChildren<Swarm>();
        if (swarm != null) swarm.Clear();
    }
}

