using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    [Header("Singleton Settings")]
    [SerializeField] protected bool dontDestroy = true;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = (T)FindFirstObjectByType(typeof(T));

                if (_instance != null) return _instance;

                GameObject singleton = new GameObject();
                _instance = singleton.AddComponent<T>();
                singleton.name = "(singleton) " + typeof(T).ToString();

                // If it's the first creation and it needs to be persistent
                // Awake will handle this, but if Awake isn't called yet (unlikely)
                // we set it here if we want immediate persistence.
                // However, since we use AddComponent, Awake will be called.
                
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
