using UnityEngine;
using System.Collections;

public class Node {

    public bool walkable_;
    public Vector3 worldPosition_;
    public int gridX_, gridY_;

    // Cost of the path from the start Node to this Node
    public int gCost;
    // Cost of the straight-line distance to the goal
    public int hCost;
    // Last Node used to get to this Node (used to track back the path later)
    public Node parent;

    // Constructor
    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY) {
        walkable_ = walkable;
        worldPosition_ = worldPos;
        gridX_ = gridX;
        gridY_ = gridY;
    }

    // Sum of gCost and hCost (cost to minimize)
    public int fCost {
        get { return gCost + hCost; }
    }
}
