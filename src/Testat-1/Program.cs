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

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent immediate termination
            cts.Cancel();
        };
        //TestLEDs();

        Zumo.Instance.Lidar.SetPower(true);
        CalibrateColorSensor();
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                LeftAlign(cts.Token);
                Console.WriteLine("Color Sensor Reading: " + ColorSensor.RGBToReadable(Zumo.Instance.ColorSensor.ReadColorRGB()));
                //Thread.Sleep(1000);
            }
        }
        finally
        {
            Zumo.Instance.Lidar.SetPower(false);
            Zumo.Instance.Drive.Stop();
            //Zumo.Instance.RgbLedFront.SetValue(0xFF,0, 0, 0);
        }
    }

    private static void CalibrateColorSensor()
    {
        Console.WriteLine("Calibrating Color Sensor...");
        Console.WriteLine("Place the sensor on a white surface and press Enter.");
        Console.ReadLine();
        string whiteCalibrated = Zumo.Instance.ColorSensor.Calibrate(ColorSensor.CalibrationColor.White);
        Console.WriteLine("White Calibration: " + whiteCalibrated );

        Console.WriteLine("Place the sensor on a black surface and press Enter.");
        Console.ReadLine();
        string blackCalibrated = Zumo.Instance.ColorSensor.Calibrate(ColorSensor.CalibrationColor.Black);
        Console.WriteLine("Black Calibration: " + blackCalibrated );
        Console.WriteLine("Calibration complete. White: " + whiteCalibrated + ", Black: " + blackCalibrated);
    }

    private static void TestLEDs(){
        foreach (var led in LedFront.GetValues<LedFront>())
        {
            Console.WriteLine($"Testing LED: {led.GetType().Name} {led}");
            Zumo.Instance.RgbLedFront.SetValue(led, 255, 0, 0);
            Console.ReadLine();
            Zumo.Instance.RgbLedFront.SetValue(led, 0, 255, 0);
            Console.ReadLine();
            Zumo.Instance.RgbLedFront.SetValue(led, 0, 0, 255);
            Console.ReadLine();
            Console.WriteLine($"LED {led.GetType().Name} {led} test complete.");
            Zumo.Instance.RgbLedFront.SetValue(led, 0, 0, 0);
        }
    }

    private static void LeftAlign(CancellationToken token)
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
