using System.Collections;
using UnityEngine;

public class CameraManager : SingletonBase<CameraManager>
{
    [Header("Settings")]
    [SerializeField] private Transform targetCamera;

    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;

    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private int _lastCameraCount;

    protected override void Awake()
    {
        base.Awake();
        
        if (targetCamera == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null) targetCamera = mainCam.transform;
        }

        if (targetCamera != null)
        {
            _originalPos = targetCamera.position;
        }
        
        CreateBackgroundCamera();
        UpdateAspectRatio();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateAspectRatio();
    }

    private void Update()
    {
        int currentCameraCount = Camera.allCamerasCount;
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight || currentCameraCount != _lastCameraCount)
        {
            UpdateAspectRatio();
        }
    }

    public void UpdateAspectRatio()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _lastCameraCount = Camera.allCamerasCount;

        float targetAspect = 1440f / 2560f;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect;
        if (scaleHeight < 1.0f)
        {
            rect = new Rect(0, (1.0f - scaleHeight) / 2.0f, 1.0f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect = new Rect((1.0f - scaleWidth) / 2.0f, 0, scaleWidth, 1.0f);
        }

        ApplyRectToAllCameras(rect);
    }

    private void CreateBackgroundCamera()
    {
        GameObject bgCamObj = new GameObject("BackgroundCamera");
        bgCamObj.transform.SetParent(transform);
        Camera bgCam = bgCamObj.AddComponent<Camera>();
        bgCam.depth = -100;
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0; 
        bgCam.rect = new Rect(0, 0, 1, 1);
    }

    private void ApplyRectToAllCameras(Rect rect)
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cam in cameras)
        {
            if (cam.name == "BackgroundCamera") continue;
            cam.rect = rect;
        }
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

