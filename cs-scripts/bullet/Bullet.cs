using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float shotTime;
    private float elapsed;
    private Action onRelease;

    private BulletData bulletData;

    private bool isInitialized; 
    private void Start()
    {
        
    }

    public void Shoot(Vector2 position, Vector2 direction, BulletData bulletData, Action onRelease)
    {
        isInitialized = true;

        transform.position = position;
            this.onRelease = onRelease;
            this.bulletData = bulletData;
            this.direction = direction;

            elapsed = 0;
            shotTime = Time.time;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        if(Time.time > shotTime + bulletData.lifetime)
            onRelease?.Invoke();

        elapsed += Time.deltaTime;
        float t = elapsed / bulletData.lifetime;

        float currentSpeed = bulletData.speed * bulletData.speedCurve.Evaluate(t);
        transform.Translate(currentSpeed * Time.deltaTime * direction, Space.World);    
    }
}
