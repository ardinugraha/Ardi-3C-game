using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            OnInit();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// Dipanggil hanya sekali saat instance pertama dibuat
    protected virtual void OnInit() { }
}