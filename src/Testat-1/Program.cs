using NLog.Common;
using Util;
using ZumoLib;

namespace Testat_1;

class Program
{
    private const int CellSizeMm = 220;
    private const int SecurePerimeterMm = 80;

    // Drive profile
    private const ushort TrackSpeed = 200;
    private const ushort TrackAcceleration = 500;
    private const ushort TurnSpeed = 200;
    private const ushort TurnAcceleration = 500;

    private Direction heading = Direction.Up;
    private CancellationTokenSource? _partyModeCts;
    private Task? _partyModeTask;
    private readonly LidarDisplay _lidarDisplay = new LidarDisplay();

    static async Task Main(string[] args)
    {
        Args.Parse(args);
        //new Task(() => Zumo.Instance.RTTTL.PlaySong(RtttlSong.AxelF)).Start();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Zumo.Instance.Lidar.SetPower(false);
            Zumo.Instance.Drive.Track(0, 0, 100);

            Environment.Exit(0);
        };

#if DEBUG
        Debugger.WaitForDebugger();
#endif

        try
        {
            Program program = new();
            await program.RunAsync();
        }
        finally
        {
            Zumo.Instance.Lidar.SetPower(false);
        }
    }

    private void StartPartyMode()
    {
        if (_partyModeCts == null)
        {
            _partyModeCts = new CancellationTokenSource();
            _partyModeTask = Zumo.Instance.RgbLedFront.StartPartyModeAsync(_partyModeCts.Token, delayMs: 40);
        }
    }

    private async Task StopPartyModeAsync()
    {
        if (_partyModeCts != null)
        {
            _partyModeCts.Cancel();
            if (_partyModeTask != null)
            {
                await _partyModeTask;
            }
            _partyModeCts.Dispose();
            _partyModeCts = null;
            _partyModeTask = null;
        }
    }

    private async Task RunAsync()
    {
        Zumo.Instance.Lidar.SetPower(true);
        if (Args.Calibrate)
        {
            Calibrate();
        }
        await Task.Delay(1000);

        StartPartyMode();
        _lidarDisplay.StartLidarDisplay();

        try
        {
            while (Map.GetCurrentNode().NodeLevel < 3)
            {
                Thread.Sleep(1000);
                UpdateNeighboringNodes();
                Console.WriteLine(Map.GetCurrentNode().ToString());
                if (TryGetExit(out Direction? exitDirection) && await IsAllowedToLeaveAsync())
                {
                    await Move(exitDirection!.Value);
                    Map.ShiftCurrentPosition(exitDirection!.Value);
                    continue;
                }

                Direction moveDirection = GetMoveOnLayer();
                await Move(moveDirection);
                Map.ShiftCurrentPosition(moveDirection);
            }

            await StopPartyModeAsync();
            await _lidarDisplay.StopLidarDisplayAsync();
            Zumo.Instance.Sound.PlaySound(SoundItem.SuperMario);
        }
        catch (InvalidOperationException ex)
        {
            await StopPartyModeAsync();
            await _lidarDisplay.StopLidarDisplayAsync();
            Console.WriteLine($"Failed to move on a cell: {ex.Message}");
            throw;
        }
    }

    private async Task TryAlignWithWall()
    {
        try
        {
            double correction = LidarTools.GetAlignmentCorrection();
            if (Math.Abs(correction) > 3)
            {
                await Zumo.Instance.Drive.TurnAsync((short)-correction, TurnSpeed, TurnAcceleration);
            }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Alignment correction failed: {ex.Message}");
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

    private void Calibrate()
    {
        Console.WriteLine("Starting calibration...");
        Console.WriteLine("Please place the robot on white surface and press the CM4 button.");
        while (Zumo.Instance.Cm4Button.Pressed)
        {
            Thread.Sleep(100);
        }
        Zumo.Instance.ColorSensor.Calibrate(ColorSensor.CalibrationColor.White);
        Console.WriteLine("White calibrated. Please place the robot on black surface and press the CM4 button.");
        while (Zumo.Instance.Cm4Button.Pressed)
        {
            Thread.Sleep(100);
        }
        Zumo.Instance.ColorSensor.Calibrate(ColorSensor.CalibrationColor.Black);
        Console.WriteLine("Calibration completed.");
        Console.WriteLine("Press the CM4 button to start the robot.");
        while (Zumo.Instance.Cm4Button.Pressed)
        {
            Thread.Sleep(100);
        }
    }

    private async Task<bool> IsAllowedToLeaveAsync()
    {
        Node node = Map.GetCurrentNode();

        if (node.NodeLevel == 0)
        {
            Console.WriteLine("At starting node, allowing to leave regardless of color.");
            return true; // Always allow leaving the starting node
        }

        await StopPartyModeAsync();
        await Task.Delay(100); // Give the LEDs a moment to turn completely off

        string rgb = Zumo.Instance.ColorSensor.ReadColorRGB();

        StartPartyMode();

        Console.WriteLine($"Current Node Level: {node.NodeLevel}, Detected Color RGB: {rgb}");

        if (rgb == "Invalid" || string.IsNullOrWhiteSpace(rgb) || rgb.Length < 7)
        {
            return false;
        }

        int red = Convert.ToInt32(rgb.Substring(1, 2), 16);
        int green = Convert.ToInt32(rgb.Substring(3, 2), 16);
        int blue = Convert.ToInt32(rgb.Substring(5, 2), 16);
        if (red >= 200 && green < 100 && blue < 100)
        {
            Zumo.Instance.Sound.PlaySound(4000, 500);
            Zumo.Instance.RgbLedRearLeft.SetValue(255, 0, 0);
            Zumo.Instance.RgbLedRearRight.SetValue(255, 0, 0);
            return true;
        }
        else if (red < 100 && green >= 200 && blue < 100)
        {
            Zumo.Instance.Sound.PlaySound(4000, 500);
            Zumo.Instance.RgbLedRearLeft.SetValue(0, 255, 0);
            Zumo.Instance.RgbLedRearRight.SetValue(0, 255, 0);
            return true;
        }
        else
        {
            return false;
        }
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


        if (angle != 0)
        {
            try
            {
                TryAlignWithWall();
            }
            catch (System.Exception)
            {
                Console.WriteLine("Alignment failed, but continuing with movement.");
            }
        }

        try
        {
            await DriveCellSecure();
        }
        catch (InvalidOperationException)
        {
            await Zumo.Instance.Drive.TrackAsync(-30, 50, 100);
        }
        heading = direction;
    }

    private async Task DriveCellSecure()
    {
        var task = Zumo.Instance.Drive.TrackAsync(CellSizeMm, TrackSpeed, TrackAcceleration);
        while (!task.IsCompleted)
        {
            var isTooClose = Enumerable
                .Range(-35, 70)
                .Select(offset => (360 + offset) % 360)
                .Any(angle =>
                {
                    int distance = Zumo.Instance.Lidar[angle].Distance;
                    return distance != 0 && distance < SecurePerimeterMm;
                });

            if (isTooClose)
            {
                await Zumo.Instance.Drive.TrackAsync(0, 0, 0);
                throw new InvalidOperationException("Obstacle detected within secure perimeter while driving.");
            }
            await Task.Delay(10);
        }
    }
}
