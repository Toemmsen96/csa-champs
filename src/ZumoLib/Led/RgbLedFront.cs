//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZumoLib;

public class RgbLedFront : ComDevice
{
    public RgbLedFront(ICom com) : base(com, 0x11) { }

    // Set a single LED by enum
    public void SetValue(LedFront led, byte r, byte g, byte b)
    {
        byte bitmask = (byte)(1 << ((int)led - 1));
        SetRequest($"{bitmask:X2}{r:X2}{g:X2}{b:X2}");
    }

    // Set multiple LEDs by bitmask (e.g., 0xFF for all)
    public void SetValue(byte bitmask, byte r, byte g, byte b)
    {
        SetRequest($"{bitmask:X2}{r:X2}{g:X2}{b:X2}");
    }

    /// <summary>
    /// Starts an asynchronous rainbow wave party mode across the 8 front LEDs.
    /// </summary>
    public async Task StartPartyModeAsync(CancellationToken cancellationToken = default, int delayMs = 50)
    {
        int j = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            for (int i = 0; i < 8; i++)
            {
                Wheel(((i * 256 / 8) + j) & 255, out byte r, out byte g, out byte b);
                SetValue((byte)(1 << i), r, g, b);
            }
            j = (j + 10) & 255;

            try
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
        
        // Turn off when done or cancelled
        SetValue(0xFF, 0, 0, 0);
    }

    private void Wheel(int pos, out byte r, out byte g, out byte b)
    {
        pos = 255 - pos;
        if (pos < 85)
        {
            r = (byte)(255 - pos * 3);
            g = 0;
            b = (byte)(pos * 3);
        }
        else if (pos < 170)
        {
            pos -= 85;
            r = 0;
            g = (byte)(pos * 3);
            b = (byte)(255 - pos * 3);
        }
        else
        {
            pos -= 170;
            r = (byte)(pos * 3);
            g = (byte)(255 - pos * 3);
            b = 0;
        }
    }
}