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
    public bool IsDeflected => _isDeflected; 

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
            
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            
            
            if (sqrDist < 0.09f)
            {
                if (PlayerUnit.Instance.IsActionActive)
                {
                    Deflect("Blocked/Dashed");
                }
                else
                {
                    PlayerUnit.Instance.TakeDamage(damage, isProjectile: true);
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

