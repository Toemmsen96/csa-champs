//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System;
using System.Device.Gpio;

namespace ZumoLib;

public class Cm4Led : ILed
{
    public event EventHandler<LedStateChangedEventArgs>? LedStateChanged;

    internal Cm4Led(GpioController gpio, int pin)
    {
        Pin = pin;
        Gpio = gpio;

        Gpio.OpenPin(Pin, PinMode.Output);
    }

    internal GpioController Gpio { get; }
    internal int Pin { get; }
    private bool enabled;

    public bool Enabled
    {
        get { return enabled; }
        set
        {
            Gpio.Write(Pin, value ? PinValue.High : PinValue.Low);
            enabled = value;
            LedStateChanged?.Invoke(this, new LedStateChangedEventArgs(enabled));
        }
    }

    public void Toggle() { Enabled = !Enabled; }

}