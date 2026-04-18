using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Size")]
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(4, 4);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(10, 10);

    [Header("Corridor")]
    [SerializeField] private int minCorridorLength = 2;
    [SerializeField] private int maxCorridorLength = 6;
    [SerializeField] private int corridorWidth = 1;

    [Header("Generation")]
    [SerializeField] private int goalRoomCount = 8;

    /// <summary>
    /// Weight: 0 = always pick smallest room, 1 = always pick largest.
    /// Corridor length follows the same weight inversely (longer rooms → shorter corridors).
    /// </summary>
    [Range(0f, 1f)]
    [SerializeField] private float distanceSizeWeight = 0.5f;

    [Header("Carving")]
    [SerializeField] private bool enableCarving = true;
    [SerializeField] private int pillarSize = 1;
    /// <summary>One pillar spawns per this many interior cells. e.g. 10 = 1 pillar per 10 cells.</summary>
    [SerializeField] private int cellsPerPillar = 10;

    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase ruleTile;

    // All occupied tiles: true = floor, false = wall/blocked
    private HashSet<Vector2Int> floorTiles = new();
    private HashSet<Vector2Int> allOccupied = new(); // rooms + corridors footprint

    private List<RoomData> rooms = new();

    void Start()
    {
        GenerateDungeon();
        if (enableCarving) CarveRooms();
        PaintTiles();
    }


    void GenerateDungeon()
    {
        Vector2Int startSize = RandomRoomSize(0);
        Vector2Int startOrigin = -startSize / 2;

        Direction entranceDir = RandomDirection();
        RoomData startRoom = new RoomData(startOrigin, startSize, depth: 0, entranceDirection: entranceDir);
        RegisterRoom(startRoom);

        Queue<RoomData> frontier = new();
        frontier.Enqueue(startRoom);

        while (frontier.Count > 0 && rooms.Count < goalRoomCount)
        {
            RoomData current = frontier.Dequeue();
            int attempts = 8;

            while (attempts-- > 0 && rooms.Count < goalRoomCount)
            {
                Direction dir = RandomDirection();

                // Never expand toward the entrance of the start room
                if (current == startRoom && dir == entranceDir) continue;

                if (!TrySpawnNeighbor(current, dir, out RoomData newRoom))
                    continue;

                frontier.Enqueue(newRoom);
            }
        }
    }

    bool TrySpawnNeighbor(RoomData from, Direction dir, out RoomData newRoom)
    {
        newRoom = null;

        int childDepth = from.Depth + 1;
        Vector2Int roomSize = RandomRoomSize(childDepth);
        int corridorLen = RandomCorridorLength(childDepth);

        // Start from the center of the edge wall in `dir`
        Vector2Int edgeCenter = GetEdgeCenter(from, dir);
        Vector2Int step = DirectionHelper.CoordinateValue(dir);

        // Build corridor tiles
        List<Vector2Int> corridorTiles = new();
        for (int i = 1; i <= corridorLen; i++)
        {
            Vector2Int baseTile = edgeCenter + step * i;
            for (int w = -(corridorWidth / 2); w <= corridorWidth / 2; w++)
            {
                Vector2Int perpOffset = dir is Direction.North or Direction.South
                    ? new Vector2Int(w, 0)
                    : new Vector2Int(0, w);
                corridorTiles.Add(baseTile + perpOffset);
            }
        }

        // Room origin: place room so its entry edge aligns with corridor end
        Vector2Int corridorEnd = edgeCenter + step * corridorLen;
        Vector2Int newRoomOrigin = GetRoomOriginFromEntryEdge(corridorEnd, dir, roomSize);

        // Collect all tiles the new room would occupy
        List<Vector2Int> roomTiles = GetRoomTiles(newRoomOrigin, roomSize);

        // Collision check — corridors may overlap themselves but not existing rooms
        foreach (Vector2Int t in roomTiles)
            if (allOccupied.Contains(t)) return false;

        // Also check corridor doesn't cut through an existing room interior
        foreach (Vector2Int t in corridorTiles)
            if (allOccupied.Contains(t) && !floorTiles.Contains(t)) return false;

        // Commit corridor
        foreach (Vector2Int t in corridorTiles)
            floorTiles.Add(t);

        // Commit room
        newRoom = new RoomData(newRoomOrigin, roomSize, childDepth);
        RegisterRoom(newRoom);
        return true;
    }


    void CarveRooms()
    {
        foreach (RoomData room in rooms)
        {
            List<Vector2Int> interior = GetInteriorTiles(room).ToList();

            // If the room doesn't have enough cells for even one pillar, skip it
            if (interior.Count < cellsPerPillar) continue;

            int pillarCount = interior.Count / cellsPerPillar;

            for (int p = 0; p < pillarCount && interior.Count > 0; p++)
            {
                Vector2Int anchor = interior[Random.Range(0, interior.Count)];
                for (int dx = 0; dx < pillarSize; dx++)
                    for (int dy = 0; dy < pillarSize; dy++)
                    {
                        Vector2Int tile = anchor + new Vector2Int(dx, dy);
                        if (floorTiles.Contains(tile))
                        {
                            floorTiles.Remove(tile);
                            interior.Remove(tile);
                        }
                    }
            }
        }
    }


    void PaintTiles()
    {
        if (tilemap == null || ruleTile == null) return;

        foreach (Vector2Int pos in floorTiles)
            tilemap.SetTile((Vector3Int)pos, ruleTile);
    }


    void RegisterRoom(RoomData room)
    {
        rooms.Add(room);
        List<Vector2Int> tiles = GetRoomTiles(room.Origin, room.Size);
        foreach (Vector2Int t in tiles)
        {
            floorTiles.Add(t);
            allOccupied.Add(t);
        }
    }

    static List<Vector2Int> GetRoomTiles(Vector2Int origin, Vector2Int size)
    {
        List<Vector2Int> tiles = new(size.x * size.y);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                tiles.Add(origin + new Vector2Int(x, y));
        return tiles;
    }

    static IEnumerable<Vector2Int> GetInteriorTiles(RoomData room)
    {
        // Exclude the outer ring (1-tile border) so we don't carve walls
        for (int x = 1; x < room.Size.x - 1; x++)
            for (int y = 1; y < room.Size.y - 1; y++)
                yield return room.Origin + new Vector2Int(x, y);
    }

    /// <summary>Returns the center tile of the room's edge in direction dir.</summary>
    static Vector2Int GetEdgeCenter(RoomData room, Direction dir)
    {
        Vector2Int center = room.Origin + room.Size / 2;
        return dir switch
        {
            Direction.North => new Vector2Int(center.x, room.Origin.y + room.Size.y - 1),
            Direction.South => new Vector2Int(center.x, room.Origin.y),
            Direction.East => new Vector2Int(room.Origin.x + room.Size.x - 1, center.y),
            Direction.West => new Vector2Int(room.Origin.x, center.y),
            _ => center
        };
    }

    /// <summary>Given the last corridor tile and the direction traveled, compute new room origin.</summary>
    static Vector2Int GetRoomOriginFromEntryEdge(Vector2Int corridorEnd, Direction dir, Vector2Int roomSize)
    {
        return dir switch
        {
            // corridor came from south → room is above corridorEnd
            Direction.North => new Vector2Int(corridorEnd.x - roomSize.x / 2, corridorEnd.y + 1),
            Direction.South => new Vector2Int(corridorEnd.x - roomSize.x / 2, corridorEnd.y - roomSize.y),
            Direction.East => new Vector2Int(corridorEnd.x + 1, corridorEnd.y - roomSize.y / 2),
            Direction.West => new Vector2Int(corridorEnd.x - roomSize.x, corridorEnd.y - roomSize.y / 2),
            _ => corridorEnd
        };
    }

    /// <summary>
    /// Returns a room size that grows with depth.
    /// distanceSizeWeight controls how strongly depth drives the scaling:
    ///   0 = pure random within min/max, depth has no effect.
    ///   1 = depth alone determines the lerp t; room reaches max size at goalRoomCount depth.
    /// </summary>
    Vector2Int RandomRoomSize(int depth)
    {
        float depthT = goalRoomCount > 1 ? Mathf.Clamp01((float)depth / (goalRoomCount - 1)) : 0f;
        // Blend: random component fades out as weight increases, depth component fades in.
        float t = Mathf.Lerp(Random.value, depthT, distanceSizeWeight);
        int w = Mathf.RoundToInt(Mathf.Lerp(minRoomSize.x, maxRoomSize.x, t));
        int h = Mathf.RoundToInt(Mathf.Lerp(minRoomSize.y, maxRoomSize.y, t));
        return new Vector2Int(w, h);
    }

    /// <summary>
    /// Returns a corridor length that grows with depth, mirroring room size scaling.
    /// </summary>
    int RandomCorridorLength(int depth)
    {
        float depthT = goalRoomCount > 1 ? Mathf.Clamp01((float)depth / (goalRoomCount - 1)) : 0f;
        float t = Mathf.Lerp(Random.value, depthT, distanceSizeWeight);
        return Mathf.RoundToInt(Mathf.Lerp(minCorridorLength, maxCorridorLength, t));
    }

    static Direction RandomDirection()
    {
        Direction[] dirs = { Direction.North, Direction.South, Direction.East, Direction.West };
        return dirs[Random.Range(0, dirs.Length)];
    }

    // ─────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (rooms == null) return;
        int maxDepth = rooms.Count > 0 ? rooms.Max(r => r.Depth) : 1;

        foreach (RoomData room in rooms)
        {
            float t = maxDepth > 0 ? (float)room.Depth / maxDepth : 0f;
            Gizmos.color = new Color(t, 1f - t, 0.3f, 0.5f); // green→red as depth increases
            Gizmos.DrawWireCube(
                new Vector3(room.Origin.x + room.Size.x * 0.5f, room.Origin.y + room.Size.y * 0.5f, 0),
                new Vector3(room.Size.x, room.Size.y, 0)
            );
        }
    }
}


class RoomData
{
    public Vector2Int Origin;
    public Vector2Int Size;
    public int Depth;
    public Direction? EntranceDirection; // only set on the start room

    public RoomData(Vector2Int origin, Vector2Int size, int depth = 0, Direction? entranceDirection = null)
    {
        Origin = origin;
        Size = size;
        Depth = depth;
        EntranceDirection = entranceDirection;
    }
}


public enum Direction { North, South, East, West }

static class DirectionHelper
{
    public static Direction Opposite(Direction dir) => dir switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => dir
    };

    public static Vector2Int CoordinateValue(Direction dir) => dir switch
    {
        Direction.North => Vector2Int.up,
        Direction.South => Vector2Int.down,
        Direction.East => Vector2Int.right,
        Direction.West => Vector2Int.left,
        _ => Vector2Int.zero
    };
}