using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateMapA : MonoBehaviour {

    public Transform roomPrefab;
    public Transform nullPrefab;
    public Vector2 mapSize;

    [Range(0, 100)]
    public float roomPercent;

    public int seed = 0;

    List<Coord> coordsList;
    Queue<Coord> shuffledcoordsList;
    Room[,] map;

    Coord mapCenter;

    /* PRIMARY */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Executed first on play
    void Start() {
        CreateMap();
    }

    // Call sequentially all steps of the map creation process
    public void CreateMap() {
        Generate();
        Spawn();
    }


    // Generate map layout
    public void Generate() {

        // Initiate map grid array and a list containing all possible coordinates
        map = new Room[(int)mapSize.x, (int)mapSize.y];
        coordsList = new List<Coord>();

        // Fill the map with rooms and add all the possibles coordinates to the list
        for (int x = 0; x < mapSize.x; x++) {
            for(int y = 0; y < mapSize.y; y++) {
                coordsList.Add(new Coord(x, y));
                map[x, y] = new Room();
            }
        }

        // Shuffle the coordinate list in a queue and calculate the coord for the map center room
        shuffledcoordsList = new Queue<Coord>(Utility.ShuffleArray(coordsList.ToArray(), seed));
        mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

        // Create an array containing the information about removed rooms and calculate the number of room to remove
        bool[,] nullMap = new bool[(int)mapSize.x, (int)mapSize.y];
        int nullTilesCount = (int)(mapSize.x * mapSize.y * (1f - roomPercent / 100f));
        int currentNullTilesCount = 0;

        // while the number of room removed is smaller than the target number
        while (currentNullTilesCount < nullTilesCount) {
            // Get the next random coordinate;
            Coord randomCoord = GetRandomCoord();

            // If the room at the given position is not already marked as removed
            if (!nullMap[randomCoord.x_, randomCoord.y_]) {
                // Mark the room to be removed and increase the number of removed rooms.
                nullMap[randomCoord.x_, randomCoord.y_] = true;
                currentNullTilesCount++;

                // If the marked room was not the center room, or if removing the room dont cause accessibility problems to other rooms, remove the room.
                if (roomPercent == 0f || (randomCoord != mapCenter && AllRoomsAccessible(nullMap, currentNullTilesCount))) {
                    map[randomCoord.x_, randomCoord.y_] = null;
                }
                // Else un-mark the room
                else {
                    nullMap[randomCoord.x_, randomCoord.y_] = false;
                    currentNullTilesCount--;
                }
            }
        }
    }


    // Instanciates all tiles on the game view.
    public void Spawn() {

        // Declare mapHolder name and check if already on the map
        string mapHolderName = "Generated Map";
        if (transform.FindChild(mapHolderName)) {
            // Delete object and all children if present on the map
            DestroyImmediate(transform.FindChild(mapHolderName).gameObject);
        }

        // Create mapHolder object (parent of all created tiles) and set it as child of current object
        Transform mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;

        // Create holder for each type of tile and set as child of the map holder
        Transform nullParent = new GameObject("Null").transform;
        nullParent.parent = mapHolder;
        Transform roomParent = new GameObject("Room").transform;
        roomParent.parent = mapHolder;

        // For every tile of the map...
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                // If a room is present, instanciate room panel at calculated position.
                if (map[x, y] != null) {
                    Vector3 roomPos = CoordToPosition(x, y);
                    Transform newRoom = Instantiate(roomPrefab, roomPos, Quaternion.identity) as Transform;
                    newRoom.name = "Room[" + x + "," + y + "]";
                    newRoom.parent = roomParent;
                }
                // If null, instanciate null panel at calculated position.
                else {
                    Vector3 nullpos = CoordToPosition(x, y);
                    Transform newNullTile = Instantiate(nullPrefab, nullpos, Quaternion.identity) as Transform;
                    newNullTile.name = "Null[" + x + "," + y + "]";
                    newNullTile.parent = nullParent;
                }
            }
        }
    }

    /* SECONDARY */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Check if all room are accessible starting from the center room
    bool AllRoomsAccessible(bool[,] nullMap, int currentNullTilesCount) {

        // Create a room map and a queue to temporarely store the rooms to check
        bool[,] roomMap = new bool[nullMap.GetLength(0), nullMap.GetLength(1)];
        Queue<Coord> roomsToCheck = new Queue<Coord>();
        
        // Add the center room first
        roomsToCheck.Enqueue(mapCenter);
        roomMap[mapCenter.x_, mapCenter.y_] = true;
        int accessibleRoomsCount = 1;

        // While there is still rooms to check
        while (roomsToCheck.Count > 0) {
            // Get coordinates from the next room to check
            Coord tile = roomsToCheck.Dequeue();

            // For all neighbours of the room...
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 ^ y == 0) {
                        // Store neighbour coordinates
                        Coord neighbour = new Coord(tile.x_ + x, tile.y_ + y);
                        // If neighbour position is not out of the map
                        if (neighbour.x_ >= 0 && neighbour.x_ < nullMap.GetLength(0) && neighbour.y_ >= 0 && neighbour.y_ < nullMap.GetLength(1)) 
                        {
                            // If not yet checked as a room and not checked as null
                            if (!roomMap[neighbour.x_, neighbour.y_] && !nullMap[neighbour.x_, neighbour.y_]) 
                            {
                                // Check the position as room, and add it to the queue to check its neighbours.
                                roomMap[neighbour.x_, neighbour.y_] = true;
                                roomsToCheck.Enqueue(new Coord(neighbour.x_, neighbour.y_));
                                accessibleRoomsCount++;
                            }
                        }
                    }
                }
            }
        }
        // Determine the expected number of room on the map and compare it to the number of accesible rooms.
        int realAccessibleRoomsCount = (int)(mapSize.x * mapSize.y - currentNullTilesCount);
        return realAccessibleRoomsCount == accessibleRoomsCount;
    }


    // Return gameview coordinates from the map array position (middle of the map at (0,0,0))
    Vector3 CoordToPosition(int x, int y) {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y);
    }


    // Get next random element
    public Coord GetRandomCoord() {
        // Dequeue first element and Enqueue it back at the end, then return it
        Coord random = shuffledcoordsList.Dequeue();
        shuffledcoordsList.Enqueue(random);
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

        public static bool operator==(Coord c1, Coord c2) {
            return c1.x_ == c2.x_ && c1.y_ == c2.y_;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }

    // PlaceHolder for real room class in game
    public class Room {
        // Stuff Here...
    }
}
