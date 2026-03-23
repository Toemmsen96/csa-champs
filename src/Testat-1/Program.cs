using ZumoLib;

namespace ZumoApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Zumo starting...");

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
        Console.ReadKey();
        Console.CancelKeyPress += (s, e) =>
        {
            lidar.SetPower(false);
            Console.WriteLine("Lidar stopped.");
        };

        Console.Clear();
        Console.WriteLine("Lidar started.");
        lidar.SetPower(true);

        while (true)
        {
            renderImageInConsole(lidar);
            Console.WriteLine($"Speed: {lidar.Speed}".PadRight(40));
            Thread.Sleep(300);
        }
    }

    private static byte[]? _bmp = null;

    private static void renderImageInConsole(Lidar lidar)
    {
        int size = 800;
        float maxDistMm = 4500f;

        if (_bmp == null)
        {
            _bmp = new byte[54 + (size * size * 3)];
            _bmp[0] = (byte)'B'; _bmp[1] = (byte)'M'; // Signature
            BitConverter.GetBytes(_bmp.Length).CopyTo(_bmp, 2); // File size
            BitConverter.GetBytes(54).CopyTo(_bmp, 10); // Pixel data offset
            BitConverter.GetBytes(40).CopyTo(_bmp, 14); // DIB header size
            BitConverter.GetBytes(size).CopyTo(_bmp, 18); // Width
            BitConverter.GetBytes(size).CopyTo(_bmp, 22); // Height
            BitConverter.GetBytes((short)1).CopyTo(_bmp, 26); // Color planes
            BitConverter.GetBytes((short)24).CopyTo(_bmp, 28); // Bits per pixel
            BitConverter.GetBytes(size * size * 3).CopyTo(_bmp, 34); // Image data size
        }
        else
        {
            // Dim existing pixels by 25 (255 / 10 is approx 25), fading them out over 10 iterations!
            for (int i = 54; i < _bmp.Length; i++)
            {
                if (_bmp[i] >= 25) _bmp[i] -= 25;
                else _bmp[i] = 0;
            }
        }

        byte[] bmp = _bmp;

        void setPixel(int px, int py, byte pr, byte pg, byte pb)
        {
            if (px >= 0 && px < size && py >= 0 && py < size)
            {
                int idx = 54 + (py * size + px) * 3;
                bmp[idx] = pb;
                bmp[idx + 1] = pg;
                bmp[idx + 2] = pr;
            }
        }

        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
                setPixel(size / 2 + dx, size / 2 + dy, 0, 0, 255);

        for (int i = 0; i < 360; i++)
        {
            var p = lidar[i];
            if (p.Distance > 0 && p.Distance < maxDistMm)
            {
                double angleRad = i * Math.PI / 180.0;
                float distFraction = p.Distance / maxDistMm;

                int px = (size / 2) + (int)(Math.Sin(angleRad) * distFraction * (size / 2));
                int py = (size / 2) + (int)(Math.Cos(angleRad) * distFraction * (size / 2));

                byte r = (byte)Math.Min(255, 511 * distFraction);
                byte g = (byte)Math.Min(255, 511 - 511 * distFraction);
                setPixel(px, py, r, g, 0);
                setPixel(px + 1, py, r, g, 0);
                setPixel(px - 1, py, r, g, 0);
                setPixel(px, py + 1, r, g, 0);
                setPixel(px, py - 1, r, g, 0);
            }
        }

        Console.SetCursorPosition(0, 0);

        string base64 = Convert.ToBase64String(bmp);
        Console.WriteLine($"\x1b]1337;File=inline=1;width={size * 2}px;height={size * 2}px;preserveAspectRatio=1:{base64}\x07");
    }

    public static void ButtonChanged(object? sender, ButtonStateChangedEventArgs args)
    {
        Console.WriteLine("Button State: " + args.Pressed);
    }
}
