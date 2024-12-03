using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
public class GraphNode
{
    public int Id { get; private set; }
    public Vector2 Position { get; private set; }
    public MovementDirection MovementDirection { get; private set; }
    public List<GraphNode> Neighbors { get; private set; }

    public GraphNode(int id, Vector2 position)
    {
        Id = id;
        Position = position;
        MovementDirection = GetMovementDirectionFromPosition(position);
        Neighbors = new List<GraphNode>();
    }

    private MovementDirection GetMovementDirectionFromPosition(Vector2 position)
    {
        Vector2 dir = position.normalized;

        if (dir == Vector2.up)
            return MovementDirection.Up;
        else if (dir == Vector2.down)
            return MovementDirection.Down;
        else if (dir == Vector2.left)
            return MovementDirection.Left;
        else if (dir == Vector2.right)
            return MovementDirection.Right;
        else if (dir == (Vector2.up + Vector2.right).normalized)
            return MovementDirection.UpRight;
        else if (dir == (Vector2.up + Vector2.left).normalized)
            return MovementDirection.UpLeft;
        else if (dir == (Vector2.down + Vector2.right).normalized)
            return MovementDirection.DownRight;
        else if (dir == (Vector2.down + Vector2.left).normalized)
            return MovementDirection.DownLeft;
        else
            return MovementDirection.None;
    }

    public void AddNeighbor(GraphNode neighbor)
    {
        if (!Neighbors.Contains(neighbor))
            Neighbors.Add(neighbor);
    }
}
