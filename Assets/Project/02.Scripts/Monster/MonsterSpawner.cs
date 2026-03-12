using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Swarm swarmPrefab;
    [SerializeField] private Monster[] monsterPrefabs;
    [SerializeField] private int monsterCount = 3;

    public Swarm SpawnSwarm()
    {
        // 발판 위치에 Swarm 생성
        Swarm newSwarm = Instantiate(swarmPrefab, transform);
        newSwarm.transform.localPosition = Vector3.zero;

        Monster lastMonster = null;
        for (int i = 0; i < monsterCount; i++)
        {
            if (monsterPrefabs == null || monsterPrefabs.Length == 0) continue;

            Monster monster = Instantiate(monsterPrefabs[Random.Range(0, monsterPrefabs.Length)], newSwarm.transform);
            // 발판 위에서 일렬로 배치
            monster.transform.localPosition = new Vector3(0, 0, i * 1.5f); 
            
            monster.Initialize(newSwarm, lastMonster);
            newSwarm.AddMonster(monster);
            
            lastMonster = monster;
        }

        return newSwarm;
    }
}
