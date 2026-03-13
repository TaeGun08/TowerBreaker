using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float lifetime = 3f;

    private Vector2 _direction = Vector2.left;
    private Rigidbody2D _rigid;
    private bool _isDeflected = false;

    public void SetDirection(Vector2 dir) => _direction = dir.normalized;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _rigid.gravityScale = 0; 
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (_isDeflected) return; 

        transform.Translate(_direction * (speed * Time.deltaTime));
        
        if (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning)
        {
            float dist = Vector2.Distance(transform.position, PlayerUnit.Instance.transform.position);
            
            // 피격 판정 축소 (0.6f -> 0.3f)
            if (dist < 0.3f)
            {
                if (PlayerUnit.Instance.IsActionActive())
                {
                    Deflect("Blocked/Dashed");
                }
                else
                {
                    PlayerUnit.Instance.TakeDamage(damage);
                    DestroyProjectile("Hit Player");
                }
            }
        }
    }

    public void Deflect(string reason = "")
    {
        if (_isDeflected) return;
        _isDeflected = true;

        // 튕겨나갈 때 플레이어에게 알림 (이펙트 생성 등을 위해)
        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.OnDeflectSuccess(transform.position);
        }

        _rigid.gravityScale = 2.0f;
        Vector2 reflectDir = new Vector2(Random.Range(0.5f, 1.5f), Random.Range(1.0f, 2.0f)).normalized;
        _rigid.AddForce(reflectDir * Random.Range(5f, 8f), ForceMode2D.Impulse);
        _rigid.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);

        Destroy(gameObject, 1.5f);
    }

    public void DestroyProjectile(string reason = "")
    {
        Destroy(gameObject);
    }
}
