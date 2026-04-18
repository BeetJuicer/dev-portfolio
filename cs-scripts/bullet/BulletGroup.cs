using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

//Attach this script to a bullet to assign it as the core bullet in a bullet group.
public class BulletGroup : MonoBehaviour
{
    private enum BulletGroupShape
    {
        Orbit
    }

    [SerializeField] private int bulletCount;
    [SerializeField] private GameObject bulletTypePrefab;
    [SerializeField] private BulletGroupShape bulletGroupShape;

    [Header("ORBIT")]//separate this data later.
    [SerializeField] private float radius;
    [SerializeField] private float rotationSpeed;
    float angleOffset = 0;

    private ObjectPool<Bullet> bulletPool;
    List<Bullet> bullets = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        angleOffset = 360f / bulletCount;
    }

    void Start()
    {
        bulletPool = PoolManager.Instance.Get(bulletTypePrefab);
        for (int i = 0; i < bulletCount; i++)
        {
            Bullet bullet = bulletPool.Get();
            bullets.Add(bullet);
            bullet.transform.parent = transform;
        }

        switch(bulletGroupShape)
        {
            case BulletGroupShape.Orbit:
                CreateOrbitGroup();
                break;
            default:
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        float delta = rotationSpeed * Time.deltaTime;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 dir = Rotate(bullets[i].transform.localPosition, delta);
            bullets[i].transform.localPosition = dir * radius;
        }
    }

    void CreateOrbitGroup()
    {
        Vector2 startDirection = Vector2.right;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float currentOffset = angleOffset * i * Mathf.Deg2Rad;
            Vector2 finalDir = Rotate(startDirection, currentOffset);

            bullets[i].transform.localPosition = finalDir * radius;
        }
    }

    Vector2 Rotate(Vector2 direction, float angle)
    {
        float dirInRads = Mathf.Atan2(direction.y, direction.x);
        float finalRads = angle + dirInRads;
        Vector2 finalDir = new Vector2(Mathf.Cos(finalRads), Mathf.Sin(finalRads));

        return finalDir;
    }
}
