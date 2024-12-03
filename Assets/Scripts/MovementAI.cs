using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAI
{
    private MovementGraph graph;
    private List<GraphNode> currentPath;
    private int currentPathIndex;
    private System.Random random;

    public MovementAI(MovementGraph graph)
    {
        this.graph = graph;
        currentPath = new List<GraphNode>();
        currentPathIndex = 0;
        random = new System.Random();
        GenerateNewPath();
    }

    public MovementDirection GetNextDirection()
    {
        if (currentPathIndex >= currentPath.Count - 1)
        {
            GenerateNewPath();
            currentPathIndex = 0;
        }

        GraphNode currentNode = currentPath[currentPathIndex];
        GraphNode nextNode = currentPath[currentPathIndex + 1];

        MovementDirection direction = GetDirectionBetweenNodes(currentNode, nextNode);

        currentPathIndex++;
        return direction;
    }

    private void GenerateNewPath()
    {
        currentPath.Clear();
        GraphNode currentNode = graph.OriginNode;

        // Randomly select a pattern
        int patternChoice = random.Next(0, 4); // 0: Spiral, 1: Zigzag, 2: Diagonal

        switch (patternChoice)
        {
            case 0:
                currentPath = GenerateSpiralPath();

                break;
            case 1:
                currentPath = GenerateZigzagPath();

                break;
            case 2:
                currentPath = GenerateDiagonalPath();

                break;
            case 3:
                currentPath = GenerateBSpiralPath();
                Debug.Log("Using New Spiral!!!!!!!!!!!!!!!!!!");
                break;
            default:
                currentPath = new List<GraphNode> { currentNode };
                break;
        }
    }

    private List<GraphNode> GenerateSpiralPath()
    {
        // Updated Spiral Path with Intermediary Nodes
        return new List<GraphNode>
        {
            graph.GetNodeById(0),  // Origin
            graph.GetNodeById(1),  // Up
            graph.GetNodeById(19), // Middle Up
            graph.GetNodeById(5),  // UpRight
            graph.GetNodeById(21), // Middle UpRight
            graph.GetNodeById(2),  // Right
            graph.GetNodeById(17), // Middle Right
            graph.GetNodeById(7),  // DownRight
            graph.GetNodeById(23), // Middle DownRight
            graph.GetNodeById(3),  // Down
            graph.GetNodeById(20), // Middle Down
            graph.GetNodeById(8),  // DownLeft
            graph.GetNodeById(24), // Middle DownLeft
            graph.GetNodeById(4),  // Left
            graph.GetNodeById(18), // Middle Left
            graph.GetNodeById(6),  // UpLeft
            graph.GetNodeById(22), // Middle UpLeft
            graph.GetNodeById(1),  // Back to Up
            graph.GetNodeById(19), // Middle Up
            graph.GetNodeById(0)   // Return to Origin
        };
    }

        private List<GraphNode> GenerateBSpiralPath()
    {
        // Updated Spiral Path with Intermediary Nodes
        return new List<GraphNode>
        {
            graph.GetNodeById(0),  
            graph.GetNodeById(21),  
            graph.GetNodeById(1), 
            graph.GetNodeById(6), 
            graph.GetNodeById(4), 
            graph.GetNodeById(3),  
            graph.GetNodeById(10),
            graph.GetNodeById(9),
            graph.GetNodeById(12),
            graph.GetNodeById(11), 
            graph.GetNodeById(7), 
            graph.GetNodeById(2),  
            graph.GetNodeById(7),
            graph.GetNodeById(11),  
            graph.GetNodeById(12), 
            graph.GetNodeById(9),  
            graph.GetNodeById(10), 
            graph.GetNodeById(3),  
            graph.GetNodeById(4), 
            graph.GetNodeById(6),
            graph.GetNodeById(1),
            graph.GetNodeById(21),
            graph.GetNodeById(0)   
        };
    }

    private List<GraphNode> GenerateZigzagPath()
    {
        // Updated Zigzag Path with Intermediary Nodes
        return new List<GraphNode>
        {
            graph.GetNodeById(0), 
            graph.GetNodeById(17), 
            graph.GetNodeById(2), 
            graph.GetNodeById(10),  
            graph.GetNodeById(2), 
            graph.GetNodeById(17), 
            graph.GetNodeById(0), 
            graph.GetNodeById(18),  
            graph.GetNodeById(4),
            graph.GetNodeById(12),  
            graph.GetNodeById(4), 
            graph.GetNodeById(18), 
            graph.GetNodeById(0)  
        };
    }

    private List<GraphNode> GenerateDiagonalPath()
    {
        // Updated Diagonal Path with Intermediary Nodes
        return new List<GraphNode>
        {
            graph.GetNodeById(0),  
            graph.GetNodeById(22),  
            graph.GetNodeById(6), 
            graph.GetNodeById(14), 
            graph.GetNodeById(6), 
            graph.GetNodeById(22),  
            graph.GetNodeById(0), 
            graph.GetNodeById(24),
            graph.GetNodeById(7),
            graph.GetNodeById(15),
            graph.GetNodeById(7),
            graph.GetNodeById(24),
            graph.GetNodeById(0)
        };
    }

    private MovementDirection GetDirectionBetweenNodes(GraphNode fromNode, GraphNode toNode)
    {
        Vector2 directionVector = toNode.Position - fromNode.Position;
        Vector2 dir = directionVector.normalized;

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
}
