using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct BoidData
{
    public Vector2 position;
    public Vector2 velocity;
}


public class BoidManager : MonoBehaviour
{

    [SerializeField] private ComputeShader boidComputeShader;

    [Header("Boid Data")]
    [SerializeField] private float visualRange;
    [SerializeField] private float protectedRange;
    [SerializeField] private float avoidanceFactor;
    [SerializeField] private float matchingFactor;
    [SerializeField] private float centeringFactor;
    [SerializeField] private float turnFactor;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float leftMargin;
    [SerializeField] private float rightMargin;
    [SerializeField] private float topMargin;
    [SerializeField] private float bottomMargin;

    Boid[] boids;
    int boidCount = 0;
    ComputeBuffer boidBuffer;

    int kernel = 0;
    BoidData[] data;
    BoidData[] result;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        boids = FindObjectsByType<Boid>(FindObjectsSortMode.None);
        foreach (Boid boid in boids)
        {
            boid.Initialize();
            boid.minSpeed = minSpeed;
            boid.maxSpeed = maxSpeed;
            boid.visualRange = visualRange;
        }

        boidCount = boids.Length;
        kernel = boidComputeShader.FindKernel("CSMain");
        boidBuffer = new ComputeBuffer(boidCount, sizeof(float) * 4);
        data = new BoidData[boidCount];
        result = new BoidData[boidCount];
    }

    void Update()
    {
        for (int i = 0; i < boidCount; i++)
        {
            data[i].position = boids[i].transform.position;
            data[i].velocity = boids[i].velocity;
        }

        boidBuffer.SetData(data);

        boidComputeShader.SetFloat("visualRange", visualRange);
        boidComputeShader.SetFloat("protectedRange", protectedRange);
        boidComputeShader.SetFloat("avoidanceFactor", avoidanceFactor);
        boidComputeShader.SetFloat("matchingFactor", matchingFactor);
        boidComputeShader.SetFloat("centeringFactor", centeringFactor);
        boidComputeShader.SetFloat("turnFactor", turnFactor);
        boidComputeShader.SetFloat("minSpeed", minSpeed);
        boidComputeShader.SetFloat("maxSpeed", maxSpeed);
        boidComputeShader.SetFloat("leftMargin", leftMargin);
        boidComputeShader.SetFloat("rightMargin", rightMargin);
        boidComputeShader.SetFloat("topMargin", topMargin);
        boidComputeShader.SetFloat("bottomMargin", bottomMargin);
        boidComputeShader.SetInt("boidCount", boidCount);
        boidComputeShader.SetBuffer(kernel, "boids", boidBuffer);
        boidComputeShader.Dispatch(kernel, Mathf.CeilToInt(boidCount / 64f), 1, 1);

        boidBuffer.GetData(result);

        for (int i = 0; i < result.Length; i++)
        {
            boids[i].UpdateBoid(result[i]);
        }
    }

    void OnDestroy()
    {
        boidBuffer.Release();
    }

}
