using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateMapA : MonoBehaviour {

    public Transform roomPrefab;
    public Transform nullPrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float roomPercent;

    public int seed = 0;

    List<Coord> tilesCoords;
    Queue<Coord> shuffledTilesCoords;
    Room[,] map;

    Coord mapCenter;

    /* PRIMARY */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void Start() {
        CreateMap();
    }

    public void CreateMap() {
        Generate();
        Spawn();
    }

    public void Generate() {

        tilesCoords = new List<Coord>();
        map = new Room[(int)mapSize.x, (int)mapSize.y];

        for (int x = 0; x < mapSize.x; x++) {
            for(int y = 0; y < mapSize.y; y++) {
                tilesCoords.Add(new Coord(x, y));
                map[x, y] = new Room();
            }
        }

        shuffledTilesCoords = new Queue<Coord>(Utility.ShuffleArray(tilesCoords.ToArray(), seed));
        mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

        bool[,] nullMap = new bool[(int)mapSize.x, (int)mapSize.y];
        int nullTilesCount = (int)(mapSize.x * mapSize.y * (1f - roomPercent));
        int currentNullTilesCount = 0;

        for (int i = 0; i < nullTilesCount; i++) {
            Coord randomCoord = GetRandomCoord();

            if (!nullMap[randomCoord.x_, randomCoord.y_]) {
                nullMap[randomCoord.x_, randomCoord.y_] = true;
                currentNullTilesCount++;

                if (roomPercent == 0f || (randomCoord != mapCenter && AllRoomsAccessible(nullMap, currentNullTilesCount))) {
                    map[randomCoord.x_, randomCoord.y_] = null;
                }
                else {
                    nullMap[randomCoord.x_, randomCoord.y_] = false;
                    currentNullTilesCount--;
                    i--;
                }
            }
            else {
                i--;
            }
        }
    }


    public void Spawn() {

        string mapHolderName = "Generated Map";
        if (transform.FindChild(mapHolderName)) {
            DestroyImmediate(transform.FindChild(mapHolderName).gameObject);
        }

        Transform mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;

        Transform nullParent = new GameObject("Null").transform;
        nullParent.parent = mapHolder;
        Transform roomParent = new GameObject("Room").transform;
        roomParent.parent = mapHolder;

        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (map[x, y] != null) {
                    Vector3 roomPos = CoordToPosition(x, y);
                    Transform newRoom = Instantiate(roomPrefab, roomPos, Quaternion.identity) as Transform;
                    newRoom.parent = roomParent;
                }
                else {
                    Vector3 nullpos = CoordToPosition(x, y);
                    Transform newNullTile = Instantiate(nullPrefab, nullpos, Quaternion.identity) as Transform;
                    newNullTile.parent = nullParent;
                }
            }
        }
    }

    /* SECONDARY */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    bool AllRoomsAccessible(bool[,] nullMap, int currentNullTilesCount) {

        bool[,] roomMap = new bool[nullMap.GetLength(0), nullMap.GetLength(1)];
        Queue<Coord> roomsToCheck = new Queue<Coord>();
        roomsToCheck.Enqueue(mapCenter);

        roomMap[mapCenter.x_, mapCenter.y_] = true;
        int accessibleRoomsCount = 1;

        while (roomsToCheck.Count > 0) {
            Coord tile = roomsToCheck.Dequeue();

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    Vector2 neighbour = new Vector2(tile.x_ + x, tile.y_ + y);
                    if (x == 0 || y == 0) {
                        if (neighbour.x >= 0 && neighbour.x < nullMap.GetLength(0) && neighbour.y >= 0 && neighbour.y < nullMap.GetLength(1)) 
                        {
                            if (!roomMap[(int)neighbour.x, (int)neighbour.y] && !nullMap[(int)neighbour.x, (int)neighbour.y]) 
                            {
                                roomMap[(int)neighbour.x, (int)neighbour.y] = true;
                                roomsToCheck.Enqueue(new Coord((int)neighbour.x, (int)neighbour.y));
                                accessibleRoomsCount++;
                            }
                        }
                    }
                }
            }
        }

        int realAccessibleRoomsCount = (int)(mapSize.x * mapSize.y - currentNullTilesCount);
        return realAccessibleRoomsCount == accessibleRoomsCount;
    }


    Vector3 CoordToPosition(int x, int y) {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y);
    }


    public Coord GetRandomCoord() {
        Coord random = shuffledTilesCoords.Dequeue();
        shuffledTilesCoords.Enqueue(random);
        return random;
    }

    /* STRUCT/CLASS */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    #pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    public struct Coord {

        public int x_;
        public int y_;

        public Coord(int x, int y) {
            x_ = x;
            y_ = y;
        }

        public static bool operator==(Coord c1, Coord c2) {
            return c1.x_ == c2.x_ && c1.y_ == c2.y_;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }


    public class Room {
        // Stuff Here...
    }
}
