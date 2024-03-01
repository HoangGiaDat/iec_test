using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------------------------------
//----Smart pool script use singleton pattern to cache prefabs with pooling mechanism.
//--------------------------------------------------------------------------------------


/// <summary>
/// -------------------Hose use:
/// SmartPool.Instance.Spawm()      <=>     Instantite()
/// SmartPool.Instance.Despawn()    <=>     Destroy()
/// SmartPool.Instance.Preload()    <=>     Preload some object in game
/// </summary>


public class Pool
{
    int nextId;

    Stack<GameObject> inactive;                 // Stack hold gameobject belong this pool in state inactive
    GameObject prefabContrainer;                // Gameobject contain pools gameobject
    GameObject prefab;                          // Prefabs belong pool

    /// <summary>
    /// Inital pool
    /// </summary>
    /// <param name="prefabs">Prefab belong to pool</param>
    /// <param name="initQuantify">Number gameobject initial</param>
    public Pool(GameObject prefabs, int initQuantify)
    {
        this.prefab = prefabs;
        this.prefabContrainer = new GameObject($"Pool::{prefabs.name}");

        inactive = new Stack<GameObject>(initQuantify);
    }

    /// <summary>
    /// Instantiate gameobject to scene
    /// If stack don't have any gameobject in state deactive,
    /// we will instantiate new gameobject
    /// Otherwise, we remove one elemnet in stack and active it in game
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="localScale"></param>
    /// <returns></returns>
    public GameObject Spawn(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        GameObject obj;

        if (inactive.Count == 0)
        {
            // Instatite if stack empty
            obj = (GameObject)GameObject.Instantiate(prefab, position, rotation);

            if (nextId >= 10)
                obj.name = prefab.name + "_" + (nextId++);
            else
                obj.name = prefab.name + "_0" + (nextId++);
            var poolIdentify = obj.GetComponent<PoolIdentify>();
            if (poolIdentify)
            {
                poolIdentify.pool = this;
            }
            else
            {
                obj.AddComponent<PoolIdentify>().pool = this;
            }

            // Set to contrainer
            obj.transform.SetParent(prefabContrainer.transform);
        }
        else
        {
            obj = inactive.Pop();

            if (obj == null)
                return Spawn(position, rotation, localScale);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = localScale;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Method return gameobject belong to pool
    /// </summary>
    /// <param name="obj">Gameobject will return pool</param>
    public void Despawn(GameObject obj)
    {
        obj.SetActive(false);
        inactive.Push(obj);
    }

    /// <summary>
    /// Method to destroy pool
    /// </summary>
    public void DestroyAll()
    {
        // Return stack
        prefab = null;

        // Clear stack
        inactive.Clear();

        // Destroy child
        for (int i = 0; i < prefabContrainer.transform.childCount; i++)
            Object.Destroy(prefabContrainer.transform.GetChild(i).gameObject);

        // Destroy parent
        Object.Destroy(prefabContrainer);

        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    ///  Chekc pool exist or not when load new level
    /// </summary>
    /// <returns></returns>
    public bool CheckPoolExist()
    {
        return (prefabContrainer) ? true : false;
    }

    /// <summary>
    /// Method return all gameobject to pool
    /// </summary>
    public void ReturnPool()
    {
        Transform containerTrans = prefabContrainer.transform;
        for (int i = 0; i < containerTrans.childCount; i++)
        {
            if (containerTrans.GetChild(i).gameObject.activeSelf)
                Despawn(containerTrans.GetChild(i).gameObject);
        }
    }
}


/// <summary>
/// Main class hold pool data
/// </summary>
public class SmartPool : SingletonMono<SmartPool>
{

    const int DEFAULT_POOL_SIZE = 3;

    private Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    /// <summary>
    /// Initial dictionary for pool system
    /// </summary>
    /// <param name="prefabs"></param>
    /// <param name="quantify"></param>
    void Init(GameObject prefabs = null, int quantify = DEFAULT_POOL_SIZE)
    {
        if (Instance.pools == null)
            instance.pools = new Dictionary<GameObject, Pool>();

        if (prefabs != null && instance.pools.ContainsKey(prefabs) == false)
            instance.pools[prefabs] = new Pool(prefabs, quantify);
    }

    /// <summary>
    /// Method to preload some gameobject in to scene
    /// </summary>
    /// <param name="prefab">Prefab will instantiate</param>
    /// <param name="quantify">Number instantiate</param>
    public void Preload(GameObject prefab, int quantify)
    {
        Init(prefab, quantify);

        GameObject[] obs = new GameObject[quantify];
        for (int i = 0; i < quantify; i++)
            obs[i] = Spawn(prefab, Vector3.zero, Quaternion.identity);

        for (int i = 0; i < quantify; i++)
            Despawn(obs[i]);
    }


    /// <summary>
    ///  Method to instantiate prefab to scene
    /// </summary>
    /// <param name="prefabs">Objects will spawn</param>
    /// <param name="position">Position for gameoject</param>
    /// <param name="rotation">Rotation for gameobject</param>
    /// <param name="localScale">LocalScale for gameobject</param>
    /// <returns></returns>
    public GameObject Spawn(GameObject prefabs, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        if (!prefabs)
        {
            return null;
        }
        Init(prefabs);

        return instance.pools[prefabs].Spawn(position, rotation, localScale);
    }

    /// <summary>
    ///  Method to instantiate prefab to scene
    /// </summary>
    /// <param name="prefabs">Objects will spawn</param>
    /// <param name="position">Position for gameoject</param>
    /// <param name="rotation">Rotation for gameobject</param>
    /// <param name="localScale">LocalScale for gameobject</param>
    /// <returns></returns>
    public GameObject Spawn(GameObject prefabs, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefabs, position, rotation, Vector3.one);
    }

    public GameObject Spawn(GameObject prefabs, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = true)
    {
        var obj =  Spawn(prefabs, position, rotation, Vector3.one);
        if (parent != null)
            obj.transform.SetParent(parent, worldPositionStays);
        return obj;
    }

    /// <summary>
    /// Spawn object kèm get component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabs"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public T Spawn<T>(GameObject prefabs, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefabs, position, rotation, Vector3.one).GetComponent<T>();
    }

    /// <summary>
    /// Spawn object set parent kèm get component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabs"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <param name="worldPositionStays"></param>
    /// <returns></returns>
    public T Spawn<T>(GameObject prefabs, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = true) where T : MonoBehaviour
    {
        var obj = Spawn(prefabs, position, rotation, Vector3.one);
        if (parent != null)
            obj.transform.SetParent(parent, worldPositionStays);
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// Method to deactive gameobject
    /// </summary>
    /// <param name="prefabs">Gameobject will deactive</param>
    public void Despawn(GameObject prefabs)
    {
        PoolIdentify poolIndent = prefabs.GetComponent<PoolIdentify>();

        if (poolIndent == null)
        {


            prefabs.SetActive(false);
        }
        else
        {


            poolIndent.pool.Despawn(prefabs);
        }

    }

    /// <summary>
    /// Method will remove prefab in system pool
    /// </summary>
    /// <param name="prefabs"></param>
    public void DestroyPool(GameObject prefabs)
    {
        if (instance.pools.ContainsKey(prefabs))
        {
            instance.pools[prefabs].DestroyAll();
            instance.pools.Remove(prefabs);
        }
    }

    /// <summary>
    /// Method will make all gameoject belong prefab will deactive
    /// </summary>
    /// <param name="prefab"></param>
    public void ReturnPool(GameObject prefab)
    {
        if (instance.pools == null)
            return;

        if (instance.pools.ContainsKey(prefab))
            instance.pools[prefab].ReturnPool();
    }

    /// <summary>
    /// Method make all gameobject will deactive in pool system
    /// </summary>
    public void ReturnPoolAll()
    {
        var pools = FindObjectsOfType<PoolIdentify>();
        for (int i = 0; i < pools.Length; i++)
            Despawn(pools[i].gameObject);
    }

    /// <summary>
    /// When load new scene, we need clear garbage
    /// </summary>
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        var itemsRemove = pools.Where(f => !f.Value.CheckPoolExist()).ToArray();
        foreach (KeyValuePair<GameObject, Pool> element in itemsRemove)
        {
            if (!element.Value.CheckPoolExist())
                pools.Remove(element.Key);
        }

        // Clear resource and GC in memory
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}