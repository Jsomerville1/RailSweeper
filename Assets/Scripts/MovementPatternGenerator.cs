using System.Collections.Generic;
using UnityEngine;

public class MovementPatternGenerator
{
    private MovementAI movementAI;

    public MovementPatternGenerator(MovementGraph graph)
    {
        movementAI = new MovementAI(graph);
    }

    public MovementDirection GetNextDirection()
    {
        return movementAI.GetNextDirection();
    }
}
