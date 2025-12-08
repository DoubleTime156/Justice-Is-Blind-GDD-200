using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class Grid : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap collisionMap;
    public Tilemap semiCollisionMap;
    public LayerMask unwalkableMask;
    public float nodeSize;

    private Node[,] grid;
    private int gridSizeX;
    private int gridSizeY;
    private int bottomLeftX;
    private int bottomLeftY;
    private Vector3 worldPoint;
    private Vector3 bottomLeft;

    private int semiGridSizeX;
    private int semiGridSizeY;

    void Awake()
    {
        CreateGrid();
        UpdateGrid();
    }

    void CreateGrid()
    {
        // Setup Obstacle Map
        BoundsInt bounds = collisionMap.cellBounds;

        // Width and height in cells
        gridSizeX = bounds.size.x;
        gridSizeY = bounds.size.y;

        // Set grid size and bottomleft position
        grid = new Node[gridSizeX, gridSizeY];
        bottomLeft = collisionMap.CellToWorld(bounds.min);

        // Setup Semi-Obstacle Map
        BoundsInt semiBounds = semiCollisionMap.cellBounds;

        // Width and height in cells
        semiGridSizeX = semiBounds.size.x;
        semiGridSizeY = semiBounds.size.y;
    }

    void UpdateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                worldPoint = bottomLeft + new Vector3(
                    x * nodeSize + nodeSize / 2f,
                    y * nodeSize + nodeSize / 2f,
                    0f
                );

                Vector3Int cellObstacle = collisionMap.WorldToCell(worldPoint);
                Vector3Int cellSemi = semiCollisionMap.WorldToCell(worldPoint);

                bool hasObstacle = collisionMap.HasTile(cellObstacle);
                bool hasSemiObstacle = semiCollisionMap.HasTile(cellSemi);

                bool walkable = !(hasObstacle || hasSemiObstacle);

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }


    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        // Set world position to world point
        int x = Mathf.FloorToInt((worldPos.x - bottomLeft.x) / nodeSize) % gridSizeX;
        int y = Mathf.FloorToInt((worldPos.y - bottomLeft.y) / nodeSize) % gridSizeY;

        // If outside of grid, set x/y to 0 or max gridSizeX/gridSizeY
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }



    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    // Walkable check
                    if (!grid[checkX, checkY].walkable) continue;

                    // Diagonal Check
                    if (x != 0 && y != 0)
                    {
                        if (!grid[checkX, node.gridY].walkable || !grid[node.gridX, checkY].walkable)
                        {
                            continue;
                        }
                    }
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }
    int GetIndex(int x, int y)
    {
        return 4 + y + 3 * x;
    }

    // Debug - Grid visual
    /*
    void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.darkBlue;
                // Draw a small sphere at each node
                Gizmos.DrawWireCube(n.worldPosition + Vector3.forward, new Vector3(nodeSize - 0.5f, nodeSize - 0.5f, 0.01f));
            }
        }
    }
    */
}
