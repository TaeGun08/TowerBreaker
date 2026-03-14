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
            if (_applicationIsQuitting) return null;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = (T)FindFirstObjectByType(typeof(T));

                if (_instance != null) return _instance;

                // 인스턴스가 없으면 새로 생성 (이게 없어서 먹통이었음)
                GameObject singleton = new GameObject();
                _instance = singleton.AddComponent<T>();
                singleton.name = "(singleton) " + typeof(T).ToString();
                
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
