using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementGraph
{
    private Dictionary<int, GraphNode> nodes;

    public GraphNode OriginNode { get; private set; }

    public MovementGraph()
    {
        nodes = new Dictionary<int, GraphNode>();
        BuildGraph();
    }

    private void BuildGraph()
    {
        // Existing Nodes
        nodes[0] = new GraphNode(0, new Vector2(0, 0)); // Origin

        // Layer 1 Nodes (Distance 1)
        nodes[1] = new GraphNode(1, new Vector2(1, 0));   // Up
        nodes[2] = new GraphNode(2, new Vector2(0, 1));   // Right
        nodes[3] = new GraphNode(3, new Vector2(-1, 0));  // Down
        nodes[4] = new GraphNode(4, new Vector2(0, -1));  // Left
        nodes[5] = new GraphNode(5, new Vector2(1, 1));   // UpRight
        nodes[6] = new GraphNode(6, new Vector2(1, -1));  // UpLeft
        nodes[7] = new GraphNode(7, new Vector2(-1, 1));  // DownRight
        nodes[8] = new GraphNode(8, new Vector2(-1, -1)); // DownLeft

        // Layer 2 Nodes (Distance 2)
        nodes[9] = new GraphNode(9, new Vector2(2, 0));     // Up (Layer 2)
        nodes[10] = new GraphNode(10, new Vector2(0, 2));   // Right (Layer 2)
        nodes[11] = new GraphNode(11, new Vector2(-2, 0));  // Down (Layer 2)
        nodes[12] = new GraphNode(12, new Vector2(0, -2));  // Left (Layer 2)
        nodes[13] = new GraphNode(13, new Vector2(2, 2));   // UpRight (Layer 2)
        nodes[14] = new GraphNode(14, new Vector2(2, -2));  // UpLeft (Layer 2)
        nodes[15] = new GraphNode(15, new Vector2(-2, 2));  // DownRight (Layer 2)
        nodes[16] = new GraphNode(16, new Vector2(-2, -2)); // DownLeft (Layer 2)

        // New Intermediary Nodes (Distance 0.5)
        nodes[17] = new GraphNode(17, new Vector2(0, 0.5f));    // Middle Right
        nodes[18] = new GraphNode(18, new Vector2(0, -0.5f));   // Middle Left
        nodes[19] = new GraphNode(19, new Vector2(0.5f, 0));    // Middle Up
        nodes[20] = new GraphNode(20, new Vector2(-0.5f, 0));   // Middle Down
        nodes[21] = new GraphNode(21, new Vector2(0.5f, 0.5f));  // Middle UpRight
        nodes[22] = new GraphNode(22, new Vector2(0.5f, -0.5f)); // Middle UpLeft
        nodes[23] = new GraphNode(23, new Vector2(-0.5f, 0.5f)); // Middle DownRight
        nodes[24] = new GraphNode(24, new Vector2(-0.5f, -0.5f));// Middle DownLeft

        // Set OriginNode
        OriginNode = nodes[0];

        // Add edges to form patterns

        // Spiral Pattern Edges (Layer 1)
        AddEdge(0, 1);  // Origin to Up
        AddEdge(1, 5);  // Up to UpRight
        AddEdge(5, 2);  // UpRight to Right
        AddEdge(2, 7);  // Right to DownRight
        AddEdge(7, 3);  // DownRight to Down
        AddEdge(3, 8);  // Down to DownLeft
        AddEdge(8, 4);  // DownLeft to Left
        AddEdge(4, 6);  // Left to UpLeft
        AddEdge(6, 1);  // UpLeft back to Up (closing the loop)

        // Spiral Pattern Edges (Layer 2)
        AddEdge(1, 9);    // Up to Up (Layer 2)
        AddEdge(9, 13);   // Up (Layer 2) to UpRight (Layer 2)
        AddEdge(13, 10);  // UpRight (Layer 2) to Right (Layer 2)
        AddEdge(10, 15);  // Right (Layer 2) to DownRight (Layer 2)
        AddEdge(15, 11);  // DownRight (Layer 2) to Down (Layer 2)
        AddEdge(11, 16);  // Down (Layer 2) to DownLeft (Layer 2)
        AddEdge(16, 12);  // DownLeft (Layer 2) to Left (Layer 2)
        AddEdge(12, 14);  // Left (Layer 2) to UpLeft (Layer 2)
        AddEdge(14, 9);   // UpLeft (Layer 2) back to Up (Layer 2) (closing the loop)

        // Connect layers for smooth transitions
        AddEdge(5, 13);   // UpRight to UpRight (Layer 2)
        AddEdge(2, 10);   // Right to Right (Layer 2)
        AddEdge(7, 15);   // DownRight to DownRight (Layer 2)
        AddEdge(3, 11);   // Down to Down (Layer 2)
        AddEdge(8, 16);   // DownLeft to DownLeft (Layer 2)
        AddEdge(4, 12);   // Left to Left (Layer 2)
        AddEdge(6, 14);   // UpLeft to UpLeft (Layer 2)

        // Zigzag Pattern Edges
        // Original Zigzag Pattern:
        // AddEdge(0, 2);    // Origin to Right
        // AddEdge(2, 4);    // Right to Left
        // AddEdge(4, 2);    // Left back to Right
        // AddEdge(2, 0);    // Return to Origin

        // Updated Zigzag Pattern with Intermediary Nodes
        AddEdge(0, 2);    // Origin to Right
        AddEdge(2, 17);   // Right to Middle Right
        AddEdge(17, 0);   // Middle Right to Origin
        AddEdge(0, 4);    // Origin to Left
        AddEdge(4, 18);   // Left to Middle Left
        AddEdge(18, 0);   // Middle Left to Origin
        AddEdge(0, 2);    // Origin to Right
        AddEdge(2, 17);   // Right to Middle Right
        AddEdge(17, 0);   // Middle Right to Origin
        AddEdge(0, 4);    // Origin to Left
        AddEdge(4, 18);   // Left to Middle Left
        AddEdge(18, 0);   // Middle Left to Origin

        // Diagonal Pattern Edges
        // Original Diagonal Pattern:
        // AddEdge(0, 5);    // Origin to UpRight
        // AddEdge(5, 8);    // UpRight to DownLeft
        // AddEdge(8, 5);    // DownLeft back to UpRight
        // AddEdge(5, 0);    // Return to Origin

        // Updated Diagonal Pattern with Intermediary Nodes
        AddEdge(0, 5);    // Origin to UpRight
        AddEdge(5, 21);   // UpRight to Middle UpRight
        AddEdge(21, 8);   // Middle UpRight to DownLeft
        AddEdge(8, 24);   // DownLeft to Middle DownLeft
        AddEdge(24, 5);   // Middle DownLeft to UpRight
        AddEdge(5, 21);   // UpRight to Middle UpRight
        AddEdge(21, 0);   // Middle UpRight to Origin
    }

    private void AddEdge(int fromId, int toId)
    {
        GraphNode fromNode = nodes[fromId];
        GraphNode toNode = nodes[toId];

        fromNode.AddNeighbor(toNode);
        toNode.AddNeighbor(fromNode); // Ensure bidirectional connection
    }

    private void ClearPatternEdges(string patternName)
    {
        // Implement logic to remove existing edges... Not sure this is necessary

    }

    public GraphNode GetNodeById(int id)
    {
        return nodes.ContainsKey(id) ? nodes[id] : null;
    }
}
