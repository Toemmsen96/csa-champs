using Util;
using ZumoLib;

namespace Testat_1;

class Program
{
    private const int CellSizeMm = 200;

    // Drive profile
    private const ushort TrackSpeed = 240;
    private const ushort TrackAcceleration = 500;
    private const ushort TurnSpeed = 200;
    private const ushort TurnAcceleration = 500;

    private Direction heading = Direction.Up;

    static async Task Main(string[] args)
    {
        new Task(() => Zumo.Instance.RTTTL.PlaySong(RtttlSong.AxelF)).Start();
#if DEBUG
        Debugger.WaitForDebugger();
#endif
        new Task(() => Zumo.Instance.RTTTL.PlaySong(RtttlSong.MissionImpossible)).Start();
        Program program = new();
        await program.RunAsync();
        //Zumo.Instance.Drive.Turn(360,70,70);
    }

    private async Task RunAsync()
    {
        Zumo.Instance.Lidar.SetPower(true);
        await Task.Delay(1000);

        try
        {
            while (Map.GetCurrentNode().NodeLevel < 3)
            {
                UpdateNeighboringNodes();
                if (TryGetExit(out Direction? exitDirection) && IsAllowedToLeave())
                {
                    await Move(exitDirection!.Value);
                    Map.ShiftCurrentPosition(exitDirection!.Value);
                    continue;
                }

                Direction moveDirection = GetMoveOnLayer();
                await Move(moveDirection);
                Map.ShiftCurrentPosition(moveDirection);
            }

            Zumo.Instance.Sound.PlaySound(SoundItem.SuperMario);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Failed to move on a cell: {ex.Message}");
            throw;
        }
    }

    private Direction GetMoveOnLayer()
    {
        int currentLevel = Map.GetCurrentNode().NodeLevel;

        Direction[] movementPriorities = [heading, (Direction)(((int)heading) + 90), (Direction)(((int)heading) + 270), (Direction)(((int)heading) + 180)];
        foreach (Direction direction in movementPriorities)
        {
            if (Map.GetCurrentNode().TryGetNeighbor(direction, out Node? neighbor) && neighbor!.NodeLevel == currentLevel)
            {
                return direction;
            }
        }

        throw new InvalidOperationException("No valid move found on the current layer.");
    }

    private bool IsAllowedToLeave()
    {
        Node node = Map.GetCurrentNode();
        return node.NodeLevel == 0 || false;
    }

    private bool TryGetExit(out Direction? moveDirection)
    {
        int currentLevel = Map.GetCurrentNode().NodeLevel;

        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            if (Map.GetCurrentNode().TryGetNeighbor(direction, out Node? neighbor) && neighbor!.NodeLevel > currentLevel)
            {
                moveDirection = direction;
                return true;
            }
        }

        moveDirection = null;
        return false;
    }

    private void UpdateNeighboringNodes()
    {
        Node currentNode = Map.GetCurrentNode();

        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            int baseAngle = GetOrientationAwareAngle(direction);
            bool hasWallInDirection = Enumerable
                .Range(-5, 11)
                .Select(offset => (baseAngle + offset + 360) % 360)
                .Any(angle =>
                {
                    int distance = Zumo.Instance.Lidar[angle].Distance;
                    return distance != 0 && distance < CellSizeMm;
                });

            if (hasWallInDirection)
            {
                continue;
            }

            (int x, int y) = Map.GetPositionOffset(currentNode.X, currentNode.Y, direction);
            Node neighbor = Map.GetOrCreateNode(x, y);
            currentNode.SetNeighbor(direction, neighbor);
        }
    }

    private int GetOrientationAwareAngle(Direction direction)
    {
        return ((int)direction - (int)heading) % 360;
    }

    private async Task Move(Direction direction)
    {
        int angle = ((int)direction - (int)heading + 360) % 360;
        if (angle == 90)
        {
            await Zumo.Instance.Drive.TurnAsync(90, TurnSpeed, TurnAcceleration);
        }
        else if (angle == 270)
        {
            await Zumo.Instance.Drive.TurnAsync(-90, TurnSpeed, TurnAcceleration);
        }
        else if (angle == 180)
        {
            await Zumo.Instance.Drive.TurnAsync(180, TurnSpeed, TurnAcceleration);
        }

        // TODO: keep checking for obstacles while driving and stop if an obstacle is detected within the safety distance
        await Zumo.Instance.Drive.TrackAsync(CellSizeMm, TrackSpeed, TrackAcceleration);
        heading = direction;
    }
}
