using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
    public Grid grid; // Reference grid to pathfind on

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            List<Node> neighbors = grid.GetNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor)) continue;

                int newG = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newG < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return (dx > dy) ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }
}
