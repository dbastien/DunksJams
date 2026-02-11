/// <summary>Simple editor singleton for non-MonoBehaviour types.</summary>
public class Singleton<T> where T : class, new()
{
    static T instance;
    static readonly object lockObj = new();

    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                return instance ??= new T();
            }
        }
    }

    public virtual void Shutdown()
    {
        lock (lockObj)
        {
            instance = null;
        }
    }
}
