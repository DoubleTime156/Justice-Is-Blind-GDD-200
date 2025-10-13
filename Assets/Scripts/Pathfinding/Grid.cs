using System;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public int gridSizeX = 27;
    public int gridSizeY = 27;
    public float nodeSize = 1f;
    public Transform player;

    Node[,] grid;

    float halfWidth, halfHeight;

    void Awake()
    {
        halfWidth = gridSizeX / 2f;
        halfHeight = gridSizeY / 2f;
        CreateGrid();
    }

    void FixedUpdate()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = player.position - new Vector3(halfWidth * nodeSize, halfHeight * nodeSize, 0);
        //bottomLeft = RoundVector3(bottomLeft);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + new Vector3(x * nodeSize + nodeSize / 2f, y * nodeSize + nodeSize / 2f, 0);
                bool walkable = !Physics2D.OverlapCircle(new Vector3(worldPoint.x, worldPoint.y, 0), nodeSize / 2f, unwalkableMask) &&
                                (worldPoint - player.position).magnitude < 13.0f;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    Vector3 RoundVector3(Vector3 vec)
    {
        vec.x = MathF.Round(vec.x);
        vec.y = MathF.Round(vec.y);
        vec.z = MathF.Round(vec.z);
        return vec;
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x - (player.position.x - halfWidth * nodeSize)) / (gridSizeX * nodeSize));
        float percentY = Mathf.Clamp01((worldPos.y - (player.position.y - halfHeight * nodeSize)) / (gridSizeY * nodeSize));

        int x = Mathf.FloorToInt((gridSizeX - 1) * percentX);
        int y = Mathf.FloorToInt((gridSizeY - 1) * percentY);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }


    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }
        return neighbours;
    }

    // Debug - Grid visual
    /*
    void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.black;
                // Draw a small sphere at each node
                Gizmos.DrawCube(n.worldPosition + Vector3.forward, new Vector3(nodeSize * 0.5f, nodeSize * 0.5f, 0.01f));
            }
        }
    }
    */
    
}
