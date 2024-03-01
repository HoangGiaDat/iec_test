using Sirenix.OdinInspector;
using UnityEngine;

public abstract class SingletonMono<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour
{
    private static object syncRoot = new object();
    protected static volatile T instance;
    //private static bool applicationIsQuitting = false;
    public static T Instance
    {
        get
        {
            lock (syncRoot)
            {
                if (instance != null) return instance;
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance != null) return instance;
                instance = new GameObject().AddComponent<T>();
                instance.gameObject.name = instance.GetType().Name;
            }
            return instance;
        }
    }



    public static bool IsNull => instance == null;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed,
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    protected virtual void Awake()
    {
        // check if there's another instance already exist in scene
        if (instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            // Destroy this instances because already exist the singleton of EventsDispatcer
            Debug.Log($"An instance of T already exist : {instance.name}, So destroy this instance : {name}!!");
            Destroy(gameObject);
        }
        else
        {
            // set instance
            instance = this as T;
        }
    }
}