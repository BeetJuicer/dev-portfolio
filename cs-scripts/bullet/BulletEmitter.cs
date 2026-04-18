using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class BulletEmitter : MonoBehaviour
{
    private ObjectPool<Bullet> bulletPool;
    [SerializeField] private GameObject bulletPrefab;
    public SO_BulletWave wave;
    public SO_BulletPattern pattern;

    private float lastWaveTime = float.MinValue;
    private bool isWaveRunning;

    [Header("Debugging")]
    [SerializeField] private bool useCustomDebugData;
    [SerializeField] private BulletWaveData waveData;
    [SerializeField] private BulletPatternData patternData;


    // In some cases, an emitter is returned to the pool with isWaveRunning set to true so when it's enabled again, it doesn't emit. It thinks it's still emitting.
    // Put this here for nested emitters that are pooled.
    private void OnEnable()
    {
        isWaveRunning = false;
    }

    private void Start()
    {
        if(!useCustomDebugData)
        {
            waveData = wave.data;
            patternData = pattern.data;
        }

        bulletPool = PoolManager.Instance.Get(bulletPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        //Just putting this here so we can change when debugging in runtime.
        if (!useCustomDebugData)
        {
            waveData = wave.data;
            patternData = pattern.data;
        }

        bulletPool = PoolManager.Instance.Get(bulletPrefab);

        if (!isWaveRunning && Time.time > lastWaveTime + waveData.restPerWave)
        {
            StartCoroutine(StartWave(waveData));
        }
    }

    private IEnumerator StartWave(BulletWaveData wave)
    {
        isWaveRunning = true;

        for (int i = 0; i < waveData.shotsPerWave; i++)
        {
            Vector2 direction = waveData.startDirection;
            if(waveData.angleIncrement != 0)
            {
                direction = RotateVector(waveData.startDirection, i * waveData.angleIncrement);
            }

            ShootPattern(patternData, direction);
            yield return new WaitForSeconds(waveData.cooldownPerShot);
        }
        lastWaveTime = Time.time;
        isWaveRunning = false;
    }


    private void ShootPattern(BulletPatternData patternData, Vector2 startDirection)
    {
        startDirection = RotateVector(startDirection, patternData.startAngleOffset);
        float startAngle = Mathf.Atan2(startDirection.y, startDirection.x);//get startDir in radians
        for (int i = 0; i < patternData.count; i++)
        {
            float currentSpreadRadians = (i * patternData.spread) * Mathf.Deg2Rad;
            float newAngle = startAngle + currentSpreadRadians;
            Vector2 fireDirection = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));//Create a new vector2 from radians
            ShootBullet(fireDirection, patternData.bulletData);
        }
    }

    private void ShootBullet(Vector2 direction, BulletData bulletData)
    {
        Bullet bullet = bulletPool.Get();
        bullet.Shoot(
            position: transform.position,
            direction: direction,
            bulletData: bulletData,
            onRelease: () => bulletPool.Release(bullet)
        );
    }

    private Vector2 RotateVector(Vector2 direction, float angle)
    {
        float radians = Mathf.Atan2(direction.y, direction.x);
        float offsetRadians = Mathf.Deg2Rad * (angle);
        float newAngleInRadians = radians + offsetRadians;
        Vector2 newDirection = new Vector2(Mathf.Cos(newAngleInRadians), Mathf.Sin(newAngleInRadians));

        return newDirection;
    }
}
