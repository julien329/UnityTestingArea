using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FLOOR_TYPE { linear = 2, scatteredMultipath = 3, groupedMultipath = 4 }

public class GenerateMapB : MonoBehaviour {

    public Transform roomPrefab;
    public Transform nullPrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float roomPercent;

    public int seed = 0;
    public FLOOR_TYPE floorType;

    List<Coord> tilesCoords;
    Queue<Coord> shuffledTilesCoords;
    Room[,] map;

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

        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                tilesCoords.Add(new Coord(x, y));
            }
        }

        shuffledTilesCoords = new Queue<Coord>(Utility.ShuffleArray(tilesCoords.ToArray(), seed));
        map = new Room[(int)mapSize.x, (int)mapSize.y];
        map[(int)mapSize.x / 2, (int)mapSize.y / 2] = new Room();

        int roomCount = (int)(mapSize.x * mapSize.y * roomPercent);
        int currentRoomCount = 1;
        int nbEmptyIterations = 0;

        while(currentRoomCount < roomCount && nbEmptyIterations < shuffledTilesCoords.Count) {
            Coord randomCoord = GetRandomCoord();
            nbEmptyIterations++;

            if(map[randomCoord.x_, randomCoord.y_] == null && HasNeighbour(randomCoord)) {
                map[randomCoord.x_, randomCoord.y_] = new Room();

                if (TooManyNeighbours()) {
                    map[randomCoord.x_, randomCoord.y_] = null;
                }
                else {
                    nbEmptyIterations = 0;
                    currentRoomCount++;
                }
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

    public bool HasNeighbour(Coord randomCoord) {

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 ^ y == 0) {
                    if (randomCoord.x_ + x >=0 && randomCoord.x_ + x < mapSize.x && randomCoord.y_ + y >= 0 && randomCoord.y_ + y < mapSize.y) {
                        if (map[randomCoord.x_ + x, randomCoord.y_ + y] != null)
                            return true;
                    }
                }
            }           
        }
        return false;
    }


    public bool TooManyNeighbours() {

        int nbNeighbours = 0;

        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                if (map[x, y] != null) {
                    for (int dx = -1; dx <= 1; dx++) {
                        for (int dy = -1; dy <= 1; dy++) {
                            if (dx == 0 ^ dy == 0) {
                                if (x + dx >= 0 && x + dx < mapSize.x && y + dy >= 0 && y + dy < mapSize.y) {
                                    if (map[x + dx, y + dy] != null)
                                        nbNeighbours++;
                                }
                            }
                        }
                    }
                    if (nbNeighbours > (int)floorType)
                        return true;
                    nbNeighbours = 0;
                }
            }
        }
        return false;
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

    public struct Coord {

        public int x_;
        public int y_;

        public Coord(int x, int y) {
            x_ = x;
            y_ = y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
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
