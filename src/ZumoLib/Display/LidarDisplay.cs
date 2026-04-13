using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZumoLib;
public class LidarDisplay
{
    private CancellationTokenSource? _lidarDisplayCts;
    private Task? _lidarDisplayTask;

    public void StartLidarDisplay()
    {
        Zumo.Instance.Display.PowerOn();
        if (_lidarDisplayCts == null)
        {
            _lidarDisplayCts = new CancellationTokenSource();
            _lidarDisplayTask = UpdateLidarDisplayAsync(_lidarDisplayCts.Token);
        }
    }

    public async Task StopLidarDisplayAsync()
    {
        if (_lidarDisplayCts != null)
        {
            _lidarDisplayCts.Cancel();
            if (_lidarDisplayTask != null)
            {
                await _lidarDisplayTask;
            }
            _lidarDisplayCts.Dispose();
            _lidarDisplayCts = null;
            _lidarDisplayTask = null;
        }
        Zumo.Instance.Display.Clear();
        Zumo.Instance.Display.Flush();
        Zumo.Instance.Display.PowerOff();
    }

    private async Task UpdateLidarDisplayAsync(CancellationToken cancellationToken)
    {
        var display = Zumo.Instance.Display;
        int size = 160;
        int maxDist = 800; // max visible mm distance on radar
        int cx = size / 2;
        int cy = display.Height - (size / 2);

        while (!cancellationToken.IsCancellationRequested)
        {
            display.FillRectangle(0, display.Height - size, size, size, 0x0000); 
            
            // Draw axis
            display.DrawLine(cx, display.Height - size, cx, display.Height - 1, Display.Rgb565(50, 50, 50));
            display.DrawLine(0, cy, size, cy, Display.Rgb565(50, 50, 50));

            for (int angle = 0; angle < 360; angle += 2)
            {
                int dist = Zumo.Instance.Lidar[angle].Distance;
                if (dist > 0 && dist < maxDist)
                {
                    double rad = angle * Math.PI / 180.0;
                    int px = cx + (int)(Math.Sin(rad) * dist * (size / 2.0) / maxDist);
                    int py = cy - (int)(Math.Cos(rad) * dist * (size / 2.0) / maxDist);

                    if (px >= 0 && px < size && py >= display.Height - size && py < display.Height)
                    {
                        display.SetPixel(px, py, Display.Rgb565(0, 255, 0)); // Green point for obstacle
                    }
                }
            }

            // draw robot in center
            display.FillRectangle(cx - 2, cy - 2, 5, 5, Display.Rgb565(255, 0, 0));
            
            display.Flush();

            try
            {
                await Task.Delay(150, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}