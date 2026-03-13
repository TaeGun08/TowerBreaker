using UnityEngine;

public class Corpse : MonoBehaviour
{
    private Rigidbody2D rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        Explode();
    }

    private void Explode()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        
        float force = Random.Range(5f, 7f); 

        rigid.AddForce(randomDir * force, ForceMode2D.Impulse);
    }
}
