using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public static SimplePool Instance { get; private set; }

    [System.Serializable]
    public class PoolItem
    {
        public string key;
        public GameObject prefab;
        public int prewarm = 10;
    }

    public PoolItem[] items;

    readonly Dictionary<string, Queue<GameObject>> pools = new();
    readonly Dictionary<string, GameObject> prefabByKey = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // init
        foreach (var it in items)
        {
            prefabByKey[it.key] = it.prefab;
            var q = new Queue<GameObject>();
            pools[it.key] = q;

            for (int i = 0; i < it.prewarm; i++)
            {
                var go = CreateNew(it.key);
                go.SetActive(false);
                q.Enqueue(go);
            }
        }
    }

    GameObject CreateNew(string key)
    {
        var go = Instantiate(prefabByKey[key], transform);
        go.name = $"{key}_Pooled";
        return go;
    }

    public GameObject Spawn(string key, Vector3 pos, Quaternion rot)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogError($"Pool key not found: {key}");
            return null;
        }

        var q = pools[key];
        var go = q.Count > 0 ? q.Dequeue() : CreateNew(key);

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        var p = go.GetComponent<IPoolable>();
        p?.OnSpawned();

        return go;
    }

    public void Despawn(string key, GameObject go)
    {
        if (go == null) return;

        var p = go.GetComponent<IPoolable>();
        p?.OnDespawned();

        go.SetActive(false);
        go.transform.SetParent(transform);

        if (!pools.ContainsKey(key))
        {
            Destroy(go);
            return;
        }

        pools[key].Enqueue(go);
    }
}
