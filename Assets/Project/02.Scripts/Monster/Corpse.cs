using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Corpse : MonoBehaviour
{
    [SerializeField] private float lifetime = 2.0f; 
    private Rigidbody2D _rigid;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
        
        // 회전 및 이동 저항을 높여서 미끄러지듯 멈추게 설정
        _rigid.linearDamping = 1.0f;          // 선형 저항 (이동 멈춤)
        _rigid.angularDamping = 5.0f;   // 회전 저항 (회전 멈춤)
        
        Explode();
        
        Destroy(gameObject, lifetime);
    }

    private void Explode()
    {
        if (_rigid == null) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        // 위쪽 방향으로 힘을 더 실어주어 튀어오르는 느낌 강조
        randomDir += Vector2.up * 0.5f;

        float force = Random.Range(4f, 8f); 
        float torque = Random.Range(-2f, 2f); // 회전력을 대폭 낮춤

        _rigid.AddForce(randomDir * force, ForceMode2D.Impulse);
        _rigid.AddTorque(torque, ForceMode2D.Impulse);
    }
}
