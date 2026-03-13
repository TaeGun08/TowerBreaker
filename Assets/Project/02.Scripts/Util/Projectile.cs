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
    public bool IsDeflected => _isDeflected; // 외부 확인용 프로퍼티

    public void SetDirection(Vector2 dir) => _direction = dir.normalized;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _rigid.gravityScale = 0; 
    }

    private void Start()
    {
        if (ProjectileManager.Instance != null) ProjectileManager.Instance.Register(this);
        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        if (ProjectileManager.Instance != null) ProjectileManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if (_isDeflected) return; 

        transform.Translate(_direction * (speed * Time.deltaTime));
        
        if (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning)
        {
            // 최적화: Vector2.Distance(제곱근 포함) 대신 sqrMagnitude 사용
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            
            // 0.3f 의 제곱은 0.09f
            if (sqrDist < 0.09f)
            {
                if (PlayerUnit.Instance.IsActionActive)
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
