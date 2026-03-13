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
        dontDestroy = false; // 씬 전환 시 새로 세팅되도록 함
        base.Awake();
        
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
        _originalPos = targetCamera.position;
    }

    /// <summary>
    /// 화면을 흔드는 연출을 실행합니다.
    /// </summary>
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

    /// <summary>
    /// 카메라의 원본 위치를 업데이트합니다 (층 전환 등으로 위치가 바뀔 경우 대비).
    /// </summary>
    public void UpdateOriginalPosition(Vector3 newPos)
    {
        _originalPos = newPos;
    }
}
