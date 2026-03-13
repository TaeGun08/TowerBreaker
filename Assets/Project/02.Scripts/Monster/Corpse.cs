using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Corpse : MonoBehaviour
{
    private bool _isAbsorbed = false;
    private Rigidbody2D _rigid;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        if (_rigid != null)
        {
            _rigid.linearDamping = 1.0f; // 공기 저항 추가로 미끄러짐 방지
            _rigid.angularDamping = 2.0f; // 회전 저항 추가로 타이어 현상 방지
        }
    }

    private void Start()
    {
        Explode();
    }

    private void Explode()
    {
        if (_rigid == null) return;

        // 힘의 크기를 약간 조절하고 회전력(Torque)을 대폭 낮춤
        Vector2 force = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(2.5f, 4.5f));
        _rigid.AddForce(force, ForceMode2D.Impulse);
        
        // 회전력을 아주 작게 주어 자연스럽게 눕도록 유도
        float torque = Random.Range(-2f, 2f); 
        _rigid.AddTorque(torque, ForceMode2D.Impulse);
    }

    public void StartAbsorb()
    {
        if (_isAbsorbed) return;
        _isAbsorbed = true;

        if (_rigid != null)
        {
            _rigid.simulated = false;
        }

        if (UIAbsorber.Instance != null)
        {
            UIAbsorber.Instance.Absorb(gameObject, false, () => {
                // 수집 완료 시 CurrencyManager 업데이트
                if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddGold(1);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
