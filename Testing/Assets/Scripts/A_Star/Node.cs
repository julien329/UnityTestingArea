using UnityEngine;
using System.Collections;

public class Node {

    public bool walkable_;
    public Vector3 worldPosition_;
    public int gridX_, gridY_;

    public int gCost;
    public int hCost;
    public Node parent;

    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY) {
        walkable_ = walkable;
        worldPosition_ = worldPos;
        gridX_ = gridX;
        gridY_ = gridY;
    }

    public int fCost {
        get { return gCost + hCost; }
    }
}
