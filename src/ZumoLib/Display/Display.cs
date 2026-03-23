//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System;
using System.IO;

namespace ZumoLib;

public class Display : IDisposable
{
    private const string backlightPath = "/sys/class/backlight/10-0045/brightness";
    private const string defaultFramebuffer0Path = "/dev/fb0";
    private const string defaultFramebuffer1Path = "/dev/fb1";

    private readonly string framebufferPath;
    private readonly byte[] framebuffer;
    private readonly object syncRoot = new();

    private FileStream? framebufferStream;

    public Display(int width = 480, int height = 640, string? framebufferPath = null)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0.");
        }

        Width = width;
        Height = height;
        this.framebufferPath = ResolveFramebufferPath(framebufferPath);
        framebuffer = new byte[Width * Height * 2];
    }

    public int Width { get; }

    public int Height { get; }

    public void Dim(byte value)
    {
        File.WriteAllText(backlightPath, value.ToString());
    }

    public void PowerOff()
    {
        Dim(0);
    }

    public void PowerOn()
    {
        Dim(255);
    }

    public static ushort Rgb565(byte red, byte green, byte blue)
    {
        return (ushort)(((red & 0xF8) << 8) | ((green & 0xFC) << 3) | (blue >> 3));
    }

    public void Clear(ushort color = 0)
    {
        lock (syncRoot)
        {
            for (var i = 0; i < framebuffer.Length; i += 2)
            {
                framebuffer[i] = (byte)(color & 0xFF);
                framebuffer[i + 1] = (byte)(color >> 8);
            }
        }
    }

    public void SetPixel(int x, int y, ushort color)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            return;
        }

        lock (syncRoot)
        {
            var index = ((y * Width) + x) * 2;
            framebuffer[index] = (byte)(color & 0xFF);
            framebuffer[index + 1] = (byte)(color >> 8);
        }
    }

    public void DrawLine(int x0, int y0, int x1, int y1, ushort color)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    public void DrawRectangle(int x, int y, int width, int height, ushort color)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        DrawLine(x, y, x + width - 1, y, color);
        DrawLine(x, y + height - 1, x + width - 1, y + height - 1, color);
        DrawLine(x, y, x, y + height - 1, color);
        DrawLine(x + width - 1, y, x + width - 1, y + height - 1, color);
    }

    public void FillRectangle(int x, int y, int width, int height, ushort color)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var startX = Math.Max(0, x);
        var startY = Math.Max(0, y);
        var endX = Math.Min(Width, x + width);
        var endY = Math.Min(Height, y + height);

        if (startX >= endX || startY >= endY)
        {
            return;
        }

        lock (syncRoot)
        {
            for (var row = startY; row < endY; row++)
            {
                var rowBase = row * Width * 2;
                for (var col = startX; col < endX; col++)
                {
                    var index = rowBase + (col * 2);
                    framebuffer[index] = (byte)(color & 0xFF);
                    framebuffer[index + 1] = (byte)(color >> 8);
                }
            }
        }
    }

    public void DrawImage(ParsedImage image, int x = 0, int y = 0)
    {
        int startX = Math.Max(0, x);
        int startY = Math.Max(0, y);
        int endX = Math.Min(Width, x + image.Width);
        int endY = Math.Min(Height, y + image.Height);

        if (startX >= endX || startY >= endY)
        {
            return;
        }

        lock (syncRoot)
        {
            for (int destY = startY; destY < endY; destY++)
            {
                int srcY = destY - y;
                int srcRow = srcY * image.Width;
                int dstRow = destY * Width;

                for (int destX = startX; destX < endX; destX++)
                {
                    int srcX = destX - x;
                    ushort color = image.Pixels[srcRow + srcX];

                    int index = (dstRow + destX) * 2;
                    framebuffer[index] = (byte)(color & 0xFF);
                    framebuffer[index + 1] = (byte)(color >> 8);
                }
            }
        }
    }

    public void DrawPng(string filePath, int x = 0, int y = 0)
    {
        ParsedImage image = ImageParser.ParsePng(filePath);
        DrawImage(image, x, y);
    }

    public void Flush()
    {
        lock (syncRoot)
        {
            var stream = GetOrOpenFramebufferStream();
            stream.Position = 0;
            stream.Write(framebuffer, 0, framebuffer.Length);
            stream.Flush();
        }
    }

    private FileStream GetOrOpenFramebufferStream()
    {
        if (framebufferStream != null)
        {
            return framebufferStream;
        }

        framebufferStream = new FileStream(
            framebufferPath,
            FileMode.Open,
            FileAccess.Write,
            FileShare.ReadWrite);

        return framebufferStream;
    }

    private static string ResolveFramebufferPath(string? preferredPath)
    {
        if (!string.IsNullOrWhiteSpace(preferredPath))
        {
            return preferredPath;
        }

        if (File.Exists(defaultFramebuffer1Path))
        {
            return defaultFramebuffer1Path;
        }

        return defaultFramebuffer0Path;
    }

    public void Dispose()
    {
        lock (syncRoot)
        {
            framebufferStream?.Dispose();
            framebufferStream = null;
        }
    }

}
