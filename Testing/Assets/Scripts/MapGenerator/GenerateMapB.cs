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
    Queue<Coord> shuffledcoordsList;
    Room[,] map;

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

        // Initiate the list of coordinates
        tilesCoords = new List<Coord>();

        // Add all the possible coordinates to the list
        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                tilesCoords.Add(new Coord(x, y));
            }
        }

        // Shuffle the coordinate list in a queue and initiate the map array
        shuffledcoordsList = new Queue<Coord>(Utility.ShuffleArray(tilesCoords.ToArray(), seed));
        map = new Room[(int)mapSize.x, (int)mapSize.y];
        
        // Add the center room to the array as the first room
        map[(int)mapSize.x / 2, (int)mapSize.y / 2] = new Room();
        int currentRoomCount = 1;

        // Calculate the number of required rooms
        int roomCount = (int)(mapSize.x * mapSize.y * roomPercent);
        int nbEmptyIterations = 0;

        // While the number of created room is lower than the target number, and while it is not impossible to add more
        while(currentRoomCount < roomCount && nbEmptyIterations < shuffledcoordsList.Count) {
            // Get the next random coordinate
            Coord randomCoord = GetRandomCoord();
            nbEmptyIterations++;

            // If the given position is null and has a room as neighbour
            if (map[randomCoord.x_, randomCoord.y_] == null && HasNeighbour(randomCoord)) {
                // Add a new room at the position
                map[randomCoord.x_, randomCoord.y_] = new Room();

                // If adding the room cause a room in the map to have too many neighbour, remove it
                if (TooManyNeighbours()) {
                    map[randomCoord.x_, randomCoord.y_] = null;
                }
                // Else reset the number of unsuccesful iterations and increment the number of room added
                else {
                    nbEmptyIterations = 0;
                    currentRoomCount++;
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

    // Return wether or not a position in the map has at least one room as neighbour.
    public bool HasNeighbour(Coord randomCoord) {
        // For every neighbour positions...
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 ^ y == 0) {
                    // If the neighbour position is not out of map
                    if (randomCoord.x_ + x >=0 && randomCoord.x_ + x < mapSize.x && randomCoord.y_ + y >= 0 && randomCoord.y_ + y < mapSize.y) {
                        // If the neighbour position is not null, return true
                        if (map[randomCoord.x_ + x, randomCoord.y_ + y] != null)
                            return true;
                    }
                }
            }           
        }
        // There is no room around, return false;
        return false;
    }


    // Check if a room in the map has too many neighbours
    public bool TooManyNeighbours() {

        int nbNeighbours = 0;

        // For every tile in the map...
        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                // If the position is a room
                if (map[x, y] != null) {
                    // For every neighbour of the room...
                    for (int dx = -1; dx <= 1; dx++) {
                        for (int dy = -1; dy <= 1; dy++) {
                            if (dx == 0 ^ dy == 0) {
                                // If the neighbour position is not out of map
                                if (x + dx >= 0 && x + dx < mapSize.x && y + dy >= 0 && y + dy < mapSize.y) {
                                    // If the neighbour position is a room, increment the number of neighbours
                                    if (map[x + dx, y + dy] != null)
                                        nbNeighbours++;
                                }
                            }
                        }
                    }
                    // If the number of neighbours for a room is greater than the max, return true
                    if (nbNeighbours > (int)floorType)
                        return true;
                    nbNeighbours = 0;
                }
            }
        }
        return false;
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
