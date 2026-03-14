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
            _rigid.linearDamping = 1.0f;
            _rigid.angularDamping = 2.0f;
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

        
        int reward = _originType == MonsterType.Boss ? 150 : 20;
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCorpse(reward);
        }

        if (UIAbsorber.Instance != null)
        {
            UIAbsorber.Instance.Absorb(gameObject, false, null);
        }
        else
        {
            Destroy(gameObject, 0.5f);
        }
    }
}

