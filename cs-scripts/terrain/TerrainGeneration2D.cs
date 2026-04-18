using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class TerrainGeneration2D : MonoBehaviour
{
    [SerializeField] private GameObject black;
    [SerializeField] private GameObject white;

    [SerializeField] private int height;
    [SerializeField] private int width;
    [Tooltip("This seed will not be followed if randomizeSeed is enabled.")]
    [SerializeField] private int seed;
    [SerializeField] private int maxGroundHeight;
    //receommend 4 to 20
    [SerializeField] private float magnification = 7f;


    [Header("Settings")]
    [SerializeField] private bool randomizeSeed;
    [SerializeField] private bool closedWalls;
    [SerializeField] private bool isSurfaceTerrain;

    [Header("Tiles")]
    [SerializeField] Tilemap tilemap;
    [SerializeField] RuleTile groundRuleTile;
    [SerializeField] Tile waterTile;

    private Dictionary<TileType, GameObject> tileSet = new();

    private List<List<TileType>> noiseGrid = new();

    public enum TileType
    {
        Air = 0,
        Ground = 1,
        Water = 2
    }


    private int xOffset = 0;
    private int yOffset = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateTileSet();
    }

    public void SetSurfaceTerrain(bool surface)
    {
        isSurfaceTerrain = surface;
    }

    // Using a float because the unity slider's onValueChanged event only passes a float
    public void SetMagnification(float mag)
    {
        mag = Mathf.FloorToInt(mag);
        magnification = mag;
    }

    void CreateTileSet()
    {
        tileSet.Add(TileType.Air, white);
        tileSet.Add(TileType.Ground, black);
    }   

    public void StartGeneration()
    {
        GenerateTerrain(0, 0);
    }

    public void RestartGeneration()
    {
        tilemap.ClearAllTiles();
        GenerateTerrain(0, 0);
    }

    public void GenerateTerrain(int startX, int startY)
    {
        if(randomizeSeed)
            seed = Random.Range(-1000000, 1000000);

        Random.InitState(seed);

        xOffset = Random.Range(-100000, 100000);
        yOffset = Random.Range(-100000, 100000);

        for (int y = 0; y < height; y++)
        {
            noiseGrid.Add(new List<TileType>());

            for (int x = 0; x < width; x++)
            {
                TileType tileType = TileType.Air;

                if (closedWalls && (x == 0 || x == width - 1 || y == 0 || y == height - 1))
                {
                    tileType = TileType.Ground;
                }
                else
                {
                    if(isSurfaceTerrain)
                    {
                        tileType = (TileType)GetIdUsingHeight(x, y);
                    }
                    else
                    {
                        tileType = (TileType)GetIdUsingPerlin(x, y);
                    }
                }

                noiseGrid[y].Add(tileType);
                CreateTile(tileType, x, y);
            }
        }      
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise((x + xOffset) / magnification, 
                                            (y + yOffset) / magnification);

        float clampedPerlin = Mathf.Clamp01(rawPerlin);
        float scaledPerlin = clampedPerlin * tileSet.Count;
        if(scaledPerlin == tileSet.Count)
        {
            scaledPerlin = tileSet.Count - 1;
        }

        return Mathf.FloorToInt(scaledPerlin);
    }

    int GetIdUsingHeight(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise((x + xOffset) / magnification, 0f);

        int groundHeight = Mathf.FloorToInt(rawPerlin * height);

        if (y <= groundHeight)
            return 1;   // black (ground)
        else
            return 0;   // white (air)
    }

    void CreateTile(TileType tileId, int x, int y)
    {
        switch(tileId)
        {
            case TileType.Ground:
                tilemap.SetTile(new Vector3Int(x, y, 0), groundRuleTile);
                break;
            case TileType.Air:
                // --TODO:
                break;
            case TileType.Water:
                // --TODO:
                break;
            default:
                break;
        }
    }

 
}
