using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Corpse : MonoBehaviour
{
    private bool _isAbsorbed = false;
    private Rigidbody2D _rigid;
    private MonsterType _originType = MonsterType.Normal;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        if (_rigid != null)
        {
            _rigid.drag = 1.0f;
            _rigid.angularDrag = 2.0f;
        }
    }

    public void Setup(MonsterType type)
    {
        _originType = type;
    }

    private void Start()
    {
        Explode();
    }

    private void Explode()
    {
        if (_rigid == null) return;
        Vector2 force = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(2.5f, 4.5f));
        _rigid.AddForce(force, ForceMode2D.Impulse);
        float torque = Random.Range(-2f, 2f); 
        _rigid.AddTorque(torque, ForceMode2D.Impulse);
    }

    public void StartAbsorb()
    {
        if (_isAbsorbed) return;
        _isAbsorbed = true;

        if (_rigid != null) _rigid.simulated = false;

        if (UIAbsorber.Instance != null)
        {
            UIAbsorber.Instance.Absorb(gameObject, false, () => {
                // [수정]: 보스였을 때만 골드 추가
                if (_originType == MonsterType.Boss && CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.AddGold(100); // 보스 골드 보상
                }
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
