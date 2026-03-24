//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace ZumoLib;

public class Drive : ComDevice
{
    private const byte DRIVE_DISPATCHER = 0x24;
    private const byte ENCODER_DISPATCHER = 0x22;

    public Drive(ICom com) : base(com, 0xD1) { }

    public bool Forward(short distance, short speed = 10, short acceleration = 1)
    {
        string response = SetRequest(5, DRIVE_DISPATCHER, $"2{distance:X4}{speed:X4}{acceleration:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public bool Rotate(short angle, short speed = 1000, short acceleration = 1000)
    {
        string response = SetRequest(5, DRIVE_DISPATCHER, $"A{angle:X4}{speed:X4}{acceleration:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public bool ConstantSpeed(short leftSpeed, short rightSpeed)
    {
        string response = SetRequest(5, DRIVE_DISPATCHER, $"1{leftSpeed:X4}{rightSpeed:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public bool CurveWithRadius(short angle, short radius, short speed = 1000, short acceleration = 1000)
    {
        string response = SetRequest(5, DRIVE_DISPATCHER, $"9{angle:X4}{radius:X4}{speed:X4}{acceleration:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public bool SetRotationCalibrationFactor(short calibrationFactor)
    {
        string response = SetRequest(5, DRIVE_DISPATCHER, $"B{calibrationFactor:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public (short leftSpeed, short rightSpeed) GetCurrentSpeed()
    {
        string response = GetRequest(5, DRIVE_DISPATCHER, "1");
        if (response.Length >= 8)
        {
            short left = short.Parse(response.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
            short right = short.Parse(response.Substring(4, 4), System.Globalization.NumberStyles.HexNumber);
            return (left, right);
        }
        return (0, 0);
    }

    public short GetRemainingDistance()
    {
        string response = GetRequest(5, DRIVE_DISPATCHER, "2");
        if (response.Length >= 4)
        {
            return short.Parse(response.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
        }
        return 0;
    }

    public (short leftSpeed, short rightSpeed) GetEncoderSpeed()
    {
        string response = GetRequest(5, ENCODER_DISPATCHER, "0");
        if (response.Length >= 9)
        {
            short left = short.Parse(response.Substring(1, 4), System.Globalization.NumberStyles.HexNumber);
            short right = short.Parse(response.Substring(5, 4), System.Globalization.NumberStyles.HexNumber);
            return (left, right);
        }
        return (0, 0);
    }

    public (short leftDistance, short rightDistance) GetEncoderDistance()
    {
        string response = GetRequest(5, ENCODER_DISPATCHER, "1");
        if (response.Length >= 9)
        {
            short left = short.Parse(response.Substring(1, 4), System.Globalization.NumberStyles.HexNumber);
            short right = short.Parse(response.Substring(5, 4), System.Globalization.NumberStyles.HexNumber);
            return (left, right);
        }
        return (0, 0);
    }

    public bool ResetEncoderDistance()
    {
        string response = SetRequest(5, ENCODER_DISPATCHER, "0");
        return !string.IsNullOrEmpty(response);
    }

    public bool SetEncoderDistanceFactor(short calibrationFactor)
    {
        string response = SetRequest(5, ENCODER_DISPATCHER, $"1{calibrationFactor:X4}");
        return !string.IsNullOrEmpty(response);
    }

    public void Stop()
    {
        ConstantSpeed(0, 0);
    }
}