using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    private static System.Action<T> m_OnAwake;

    public static void WhenInstantiated(System.Action<T> action)
    {
        if (Instance != null)
            action(Instance);
        else
            m_OnAwake += action;
    }

    protected virtual void Awake()
    {
        if (!enabled)
            return;

        if (Instance != null)
        {
            Debug.LogWarning($"Another instance of Singleton {typeof(T).Name} is being instantiated, destroying...", this);
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;

        InternalAwake();

        m_OnAwake?.Invoke(Instance);
        m_OnAwake = null;
    }

    protected virtual void OnEnable()
    {
        if (Instance != this)
            Awake();
    }

    protected virtual void InternalAwake() { }
}