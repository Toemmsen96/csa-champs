namespace Util;

/* Board
    #|# # # # #|#
    #|#|# # #|#|#
    #|#|# #_#|#|#
    #|#|#|S|#|#|#
    #|#|#_#_#|#|#
    #|#|# # #|#|#
    #|# # # # #|#
*/

public class Map
{
    private static int currentX = 3;
    private static int currentY = 3;
    private static readonly Node[,] nodes = new Node[7, 7];
    public static Node GetOrCreateNode(int x, int y)
    {
        if (nodes[x, y] == null)
        {
            nodes[x, y] = new Node(x, y);
        }

        return nodes[x, y];
    }

    public static Node GetCurrentNode()
    {
        return GetOrCreateNode(currentX, currentY);
    }

    public static (int x, int y) GetPositionOffset(int x, int y, Direction direction)
    {
        return direction switch
        {
            Direction.Up => (x, y - 1),
            Direction.Right => (x + 1, y),
            Direction.Down => (x, y + 1),
            Direction.Left => (x - 1, y),
            _ => (x, y),
        };
    }

    internal static void ShiftCurrentPosition(Direction direction)
    {
        (int x, int y) = GetPositionOffset(currentX, currentY, direction);
        currentX = x;
        currentY = y;
    }
}