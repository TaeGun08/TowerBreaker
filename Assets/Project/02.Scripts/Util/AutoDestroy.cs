using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
