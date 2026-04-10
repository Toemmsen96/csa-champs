namespace Util;

public class Node(int x, int y)
{
    private readonly Dictionary<Direction, Node> neighbors = [];
    public int X { get; } = x;
    public int Y { get; } = y;
    public int NodeLevel
    {
        get
        {
            return Math.Max(Math.Abs(3 - X), Math.Abs(Y - 3));
        }
    }

    public void SetNeighbor(Direction direction, Node node)
    {
        neighbors[direction] = node;
        node.neighbors[GetOppositeDirection(direction)] = this;
    }

    public bool TryGetNeighbor(Direction direction, out Node? neighbor)
    {
        return neighbors.TryGetValue(direction, out neighbor);
    }

    private static Direction GetOppositeDirection(Direction direction)
    {
        return (Direction)(((int)direction + 180) % 360);
    }
}
