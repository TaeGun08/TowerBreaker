using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour
{
    [SerializeField] private MonsterType type;
    [SerializeField] private int hp = 1;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float minDistance = 1.2f;

    public Swarm Swarm { get; set; }
    public Monster FrontMonster { get; set; } // 내 앞의 몬스터
    public bool IsDead { get; private set; }

    public virtual void Initialize(Swarm swarm, Monster front)
    {
        Swarm = swarm;
        FrontMonster = front;
        IsDead = false;
        gameObject.SetActive(true);
    }

    public virtual void TakeDamage()
    {
        if (IsDead) return;
        
        hp--;
        if (hp <= 0)
        {
            IsDead = true;
            gameObject.SetActive(false);
            if (Swarm != null)
            {
                Swarm.OnMonsterDestroyed();
            }
        }
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        // 앞 몬스터와의 거리 유지 (밀림 전파)
        if (FrontMonster != null && !FrontMonster.IsDead)
        {
            float dist = Vector3.Distance(transform.position, FrontMonster.transform.position);
            if (dist < minDistance)
            {
                // 앞 몬스터가 너무 가까우면 뒤로 밀림
                Vector3 targetPos = FrontMonster.transform.position + (transform.position - FrontMonster.transform.position).normalized * minDistance;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);
            }
        }
    }
}
