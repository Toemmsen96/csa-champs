using Testat_1.Util;
using ZumoLib;

namespace Testat_1;

class Program
{
    const int ANGLE_THRESHOLD = 50;
    const int WALL_LENGTH_THRESHOLD = 10;

    static void Main(string[] args)
    {
#if DEBUG
        // Debugger.WaitForDebugger();
#endif

        Zumo.Instance.Lidar.SetPower(true);

        while (true)
        {
            LeftAlign();
        }
    }

    private static void LeftAlign(CancellationToken token = new CancellationToken())
    {
        int correctionlessCycles = 0;
        while (!token.IsCancellationRequested && correctionlessCycles < 3)
        {
            Thread.Sleep(1000);
            var walls = LidarProcessing.GetWalls();

            var leftWall = walls
                .Where(w => GetAngularSpan(w) >= WALL_LENGTH_THRESHOLD)
                .FirstOrDefault(w =>
                {
                    double normalizedAngle = NormalizeAngle(w.Angle);
                    return Math.Abs(275 - normalizedAngle) <= ANGLE_THRESHOLD;
                });

            if (leftWall == null)
            {
                continue;
            }

            var correctionAngle = leftWall.Angle + 90;
            if (Math.Abs(correctionAngle) < 2)
            {
                correctionlessCycles++;
                continue;
            }

            Console.WriteLine("Correction Angle: " + correctionAngle);
            Zumo.Instance.Drive.Rotate((short)correctionAngle, 50, 100);
        }
    }

    private static double NormalizeAngle(double angle)
    {
        return (angle + 360.0) % 360.0;
    }

    private static double GetAngularSpan(Wall wall)
    {
        double start = NormalizeAngle(wall.Start.Angle);
        double end = NormalizeAngle(wall.End.Angle);
        return NormalizeAngle(end - start);
    }
}
