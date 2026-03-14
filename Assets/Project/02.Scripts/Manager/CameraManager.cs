using System.Collections;
using UnityEngine;

public class CameraManager : SingletonBase<CameraManager>
{
    [Header("Settings")]
    [SerializeField] private Transform targetCamera;

    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;

    protected override void Awake()
    {
        dontDestroy = false; 
        base.Awake();
        
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
        _originalPos = targetCamera.position;
    }

    
    
    
    public void Shake(float intensity, float duration)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Vector3 randomPoint = Random.insideUnitSphere * intensity;
            targetCamera.position = _originalPos + new Vector3(randomPoint.x, randomPoint.y, 0);
            yield return null;
        }
        targetCamera.position = _originalPos;
        _shakeCoroutine = null;
    }

    
    
    
    public void UpdateOriginalPosition(Vector3 newPos)
    {
        _originalPos = newPos;
    }
}

