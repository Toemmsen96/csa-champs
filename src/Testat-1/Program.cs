using ZumoLib;

namespace ZumoApp;

class Program
{
    private static bool running = true;

    private const string DemoImageFileName = "CoderAlien.png";
    private const float MaxLidarDistanceMm = 4500f;

    static void Main(string[] args)
    {
        Console.WriteLine("Zumo starting...");

        var display = Zumo.Instance.Display;
        display.PowerOn();

        var black = Display.Rgb565(0, 0, 0);
        var blue = Display.Rgb565(0, 0, 255);

        DrawStartupImage(display, DemoImageFileName, black);

        Zumo.Instance.Cm4Button.ButtonChanged += ButtonChanged;

        Zumo.Instance.Lidar.SetPower(true);
        Thread.Sleep(500);

        const int TARGET_DISTANCE = 400;
        const short BASE_SPEED = 200;
        const int MAX_ERROR = 150;
        const int OBSTACLE_THRESHOLD = 400;

        do
        {
            int left45 = Zumo.Instance.Lidar[315].Distance;
            int left90 = Zumo.Instance.Lidar[270].Distance;
            int left135 = Zumo.Instance.Lidar[225].Distance;

            int avgDistance = (left135 + left90 + left45) / 3;
            int distError = avgDistance - TARGET_DISTANCE;
            distError = Math.Max(-MAX_ERROR, Math.Min(MAX_ERROR, distError));

            int angleError = left45 - left135;
            angleError = Math.Max(-100, Math.Min(100, angleError));

            int frontCenter = Zumo.Instance.Lidar[0].Distance;
            int frontLeft = Zumo.Instance.Lidar[45].Distance;
            int frontRight = Zumo.Instance.Lidar[315].Distance;

            int minFrontDist = Math.Min(frontCenter, Math.Min(frontLeft, frontRight));

            short speedMultiplier = 100;
            short obstacleAvoidance = 0;

            if (minFrontDist < OBSTACLE_THRESHOLD && minFrontDist > 0)
            {
                speedMultiplier = (short)Math.Max(30, minFrontDist * 100 / OBSTACLE_THRESHOLD);

                if (frontCenter < frontRight)
                {
                    obstacleAvoidance = 150;
                }
                else if (frontCenter < frontLeft)
                {
                    obstacleAvoidance = 100;
                }
            }

            short distCorrection = (short)(distError / 150.0f * 80);
            short angleCorrection = (short)(angleError / 100.0f * 80);
            short totalCorrection = (short)(distCorrection + angleCorrection - obstacleAvoidance);

            short leftSpeed = (short)((BASE_SPEED * speedMultiplier / 100) - totalCorrection);
            short rightSpeed = (short)((BASE_SPEED * speedMultiplier / 100) + totalCorrection);

            Zumo.Instance.Drive.ConstantSpeed(leftSpeed, rightSpeed);

            RenderLidarOnDisplay(lidar, display, black, blue);
            Thread.Sleep(50);
        } while (running);
    }

    private static void DrawStartupImage(Display display, string imageFileName, ushort backgroundColor)
    {
        string? imagePath = FindFileInCurrentOrParentDirectories(imageFileName, maxDepth: 6);
        if (imagePath is null)
        {
            return;
        }

        ParsedImage source = ImageParser.ParsePng(imagePath);
        ParsedImage scaled = ResizeNearest(source, display.Width, display.Height);

        int x = (display.Width - scaled.Width) / 2;
        int y = (display.Height - scaled.Height) / 2;

        display.Clear(backgroundColor);
        display.DrawImage(scaled, x, y);
        display.Flush();
    }

    private static string? FindFileInCurrentOrParentDirectories(string fileName, int maxDepth)
    {
        string current = Directory.GetCurrentDirectory();
        for (int depth = 0; depth <= maxDepth; depth++)
        {
            string candidate = Path.Combine(current, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            DirectoryInfo? parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        return null;
    }

    private static ParsedImage ResizeNearest(ParsedImage source, int maxWidth, int maxHeight)
    {
        if (source.Width <= maxWidth && source.Height <= maxHeight)
        {
            return source;
        }

        float scale = Math.Min((float)maxWidth / source.Width, (float)maxHeight / source.Height);
        int targetWidth = Math.Max(1, (int)(source.Width * scale));
        int targetHeight = Math.Max(1, (int)(source.Height * scale));

        ushort[] targetPixels = new ushort[targetWidth * targetHeight];
        for (int y = 0; y < targetHeight; y++)
        {
            int srcY = Math.Min(source.Height - 1, (int)(y / scale));
            int srcRow = srcY * source.Width;
            int dstRow = y * targetWidth;

            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = Math.Min(source.Width - 1, (int)(x / scale));
                targetPixels[dstRow + x] = source.Pixels[srcRow + srcX];
            }
        }

        return new ParsedImage(targetWidth, targetHeight, targetPixels);
    }

    private static void RenderLidarOnDisplay(Lidar lidar, Display display, ushort backgroundColor, ushort centerColor)
    {
        int width = display.Width;
        int height = display.Height;
        int maxRadius = Math.Min(width, height) / 4;
        int centerX = maxRadius + 8;
        int centerY = height - maxRadius - 8;

        display.Clear(backgroundColor);

        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
                display.SetPixel(centerX + dx, centerY + dy, centerColor);

        for (int i = 0; i < 360; i++)
        {
            var p = lidar[i];
            if (p.Distance > 0 && p.Distance < MaxLidarDistanceMm)
            {
                double angleRad = (-i + 180.0) * Math.PI / 180.0;
                float distFraction = p.Distance / MaxLidarDistanceMm;

                int px = centerX + (int)(Math.Sin(angleRad) * distFraction * maxRadius);
                int py = centerY + (int)(Math.Cos(angleRad) * distFraction * maxRadius);

                byte r = (byte)Math.Min(255, 511 * distFraction);
                byte g = (byte)Math.Min(255, 511 - 511 * distFraction);
                ushort color = Display.Rgb565(r, g, 0);

                display.SetPixel(px, py, color);
                display.SetPixel(px + 1, py, color);
                display.SetPixel(px - 1, py, color);
                display.SetPixel(px, py + 1, color);
                display.SetPixel(px, py - 1, color);
            }
        }

        display.Flush();
    }

    private static void RestoreDisplay(Display display, ushort backgroundColor)
    {
        display.Clear(backgroundColor);
        display.Flush();
    }

    public static void ButtonChanged(object? sender, ButtonStateChangedEventArgs args)
    {
        if (args.Pressed)
        {
            running = false;
            Zumo.Instance.Drive.Stop();
            Zumo.Instance.Lidar.SetPower(false);
            RestoreDisplay(Zumo.Instance.Display, Display.Rgb565(0, 0, 0));
        }
    }
}
