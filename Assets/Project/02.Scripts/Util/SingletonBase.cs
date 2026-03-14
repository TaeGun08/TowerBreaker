using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance; 
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;
    public static bool IsQuitting => _applicationIsQuitting;

    [Header("Singleton Settings")]
    [SerializeField] protected bool dontDestroy = true;

    public static T Instance
    {
        get
        {
            
            if (_applicationIsQuitting || !Application.isPlaying) return null;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                
                _instance = (T)Object.FindAnyObjectByType(typeof(T));

                if (_instance != null) return _instance;

                
                string typeName = typeof(T).Name;
                if (typeName == "PlayerUnit" || typeName == "InGameUIManager" || 
                    typeName == "StageManager" || typeName == "OutGameManager")
                {
                    return null; 
                }

                
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
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
