using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = (T)FindFirstObjectByType(typeof(T));
                if (m_instance == null)
                {
                    SetupInstance();
                }
            }
            return m_instance;
        }

    }

    private static void SetupInstance()
    {
        m_instance = (T)FindFirstObjectByType(typeof(T));
        if (m_instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = typeof(T).Name;
            m_instance = gameObj.AddComponent<T>();
            DontDestroyOnLoad(gameObj);
        }
    }

    public virtual void Awake()
    {
        RemoveDuplicates();
    }

    private void RemoveDuplicates()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
