using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance { get { return instance; } }

    public Dictionary<GameObject, ObjectPool<Bullet>> pools = new();

    [SerializeField] private int defaultCapacity;
    [SerializeField] private int maxCapacity;

    [SerializeField] private List<GameObject> initializeOnStart;

    private void Awake()
    {
        if(instance != null)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        foreach (var gameObject in initializeOnStart)
        {
            CreatePool(gameObject);
        }
    }

    public ObjectPool<Bullet> Get(GameObject gameObject)
    {
        if (!pools.ContainsKey(gameObject))
        {
            CreatePool(gameObject);
        }

        return pools[gameObject];
    }

    private void CreatePool(GameObject gameObject)
    {
        var bulletPool = new ObjectPool<Bullet>(
            createFunc: () => Instantiate(gameObject).GetComponent<Bullet>(),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => { 
                obj.gameObject.SetActive(false);
            },
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxCapacity
            );

        pools[gameObject] = bulletPool; 
    }
}
