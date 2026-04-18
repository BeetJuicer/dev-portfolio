using UnityEngine;

[CreateAssetMenu(fileName = "BulletWave", menuName = "Scriptable Objects/BulletWave")]
public class SO_BulletWave : ScriptableObject
{
    public BulletWaveData data;
}

[System.Serializable]
public struct BulletWaveData
{
    public int shotsPerWave;
    public float cooldownPerShot;
    public float angleIncrement;
    public float restPerWave;
    public Vector2 startDirection;
}
