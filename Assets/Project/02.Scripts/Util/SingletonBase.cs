using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance; // private -> protected 로 변경
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;
    public static bool IsQuitting => _applicationIsQuitting;

    [Header("Singleton Settings")]
    [SerializeField] protected bool dontDestroy = true;

    public static T Instance
    {
        get
        {
            // 앱 종료 중이거나 플레이 모드가 아니면 절대 접근/생성하지 않음
            if (_applicationIsQuitting || !Application.isPlaying) return null;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                // 씬에서 찾기 시도 (안전한 함수 사용)
                _instance = (T)Object.FindAnyObjectByType(typeof(T));

                if (_instance != null) return _instance;

                // [수정] 자동 생성 금지 목록: 핵심 매니저 및 UI는 직접 배치된 것만 사용
                string typeName = typeof(T).Name;
                if (typeName == "PlayerUnit" || typeName == "InGameUIManager" || 
                    typeName == "StageManager" || typeName == "OutGameManager")
                {
                    return null; 
                }

                // 그 외 일반 매니저만 자동 생성
                GameObject singleton = new GameObject($"(singleton) {typeof(T)}");
                _instance = singleton.AddComponent<T>();
                
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (dontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already exists. Destroying duplicate on {gameObject.name}");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        // If this was the instance, reset it
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
