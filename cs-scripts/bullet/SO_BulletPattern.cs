using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletPattern", menuName = "Scriptable Objects/BulletPattern")]
public class SO_BulletPattern : ScriptableObject
{
    public BulletPatternData data;
}

[System.Serializable]
public struct BulletPatternData
{
    public int count;
    public float spread;
    public float startAngleOffset;
    public BulletData bulletData;
}

[System.Serializable]
public struct BulletData
{
    public float lifetime;

    public float speed;
    public AnimationCurve speedCurve;

    //public float rotationAngle;
    //public float rotationSpeed;
    //public AnimationCurve rotationSpeedCurve;
}