using UnityEngine;

public abstract class PersistentSingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnInit();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// Dipanggil hanya sekali sepanjang game
    protected virtual void OnInit() { }
}