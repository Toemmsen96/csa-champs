using ZumoLib;

namespace ZumoApp;

class Program
{
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

        // Test Button
        Zumo.Instance.Cm4Button.ButtonChanged += ButtonChanged;

        // Test Led
        for (int i = 0; i < 6; i++)
        {
            Zumo.Instance.Cm4Led.Toggle();
            Thread.Sleep(100);
        }

        // Test Lidar
        Lidar lidar = Zumo.Instance.Lidar;
        lidar.SetPower(false);
        Console.WriteLine("Press any key to start Lidar...");
        //Zumo.Instance.RTTTL.PlaySong(RtttlSong.BennyHill);
        Console.ReadKey();
        Console.CancelKeyPress += (s, e) =>
        {
            lidar.SetPower(false);
            RestoreDisplay(display, black);
            Console.WriteLine("Lidar stopped.");
        };

        Console.Clear();
        Console.WriteLine("Lidar started.");
        lidar.SetPower(true);

        while (true)
        {
            RenderLidarOnDisplay(lidar, display, black, blue);
            Console.SetCursorPosition(0, 1);
            Console.WriteLine($"Speed: {lidar.Speed}".PadRight(40));
            Thread.Sleep(80);
        }
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
        Console.WriteLine("Button State: " + args.Pressed);
    }
}
