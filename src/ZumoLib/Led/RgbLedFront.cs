//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System;

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
}
