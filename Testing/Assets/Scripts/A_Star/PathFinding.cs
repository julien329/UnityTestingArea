using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFinding : MonoBehaviour {

    public Transform seeker, target;

    Grid grid;

    // Executed before Start() at play
    void Awake() {
        grid = GetComponent<Grid>();
    }

    // Executed on every frame
    void Update() {
        // Find the shorthest path between the seeker and the target
        FindPath(seeker.position, target.position);
    }


    // Find the shorthest path between two position
	void FindPath(Vector3 startPos, Vector3 targetPos) {
        // Find the corresponding Nodes of the given positons
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // Create a list a available Node (open) and used Node (closed)
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        // Add the starting Node to the openList
        openList.Add(startNode);

        // While the openList is not empty
        while(openList.Count > 0) {
            // Get the first Node in the openList by default
            Node currentNode = openList[0];
            // For every Node in the openList..
            for (int i = 1; i < openList.Count; i++) {
                // If the node as a lesser fCost than the current Node, or if the cost is equal but the hCost is lower than the current Node
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)) {
                    // Get the new minimal Node
                    currentNode = openList[i];
                }
            }

            // Remove the node from the openList and add it to the closedList (Node has been used)
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // If the currentNode
            if(currentNode == targetNode) {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach(Node neighbour in grid.GetNeighbours(currentNode)) {
                if (!neighbour.walkable_ || closedList.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openList.Contains(neighbour)) {
                        openList.Add(neighbour);
                    }
                }
            }
        }
    }


    void RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;
    }


    int GetDistance(Node nodeA, Node nodeB) {
        int distX = Mathf.Abs(nodeA.gridX_ - nodeB.gridX_);
        int distY = Mathf.Abs(nodeA.gridY_ - nodeB.gridY_);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);

        return 14 * distX + 10 * (distY - distX);
    }
}
