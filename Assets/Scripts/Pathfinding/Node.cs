using UnityEngine;
using System.Collections.Generic;

// Node stores necessary info for pathfinding
public class Node
{
    public bool walkable; // Is Node walkable?
    public Vector3 worldPosition; // Position of Node
    public int gridX, gridY; // Grid position of Node
    public int gCost, hCost; // Dijkstra’s cost and Heuristic cost
    public Node parent; // Parent Node

    public int fCost => gCost + hCost; // Total cost

    public Node(bool walkable, Vector3 pos, int x, int y)
    {
        this.walkable = walkable;
        this.worldPosition = pos;
        this.gridX = x;
        this.gridY = y;
    }
}