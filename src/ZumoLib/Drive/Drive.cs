//    _____                            ____        __          __
//   /__  /  __  ______ ___  ____     / __ \____  / /_  ____  / /_
//     / /  / / / / __ `__ \/ __ \   / /_/ / __ \/ __ \/ __ \/ __/
//    / /__/ /_/ / / / / / / /_/ /  / _, _/ /_/ / /_/ / /_/ / /_
//   /____/\__,_/_/ /_/ /_/\____/  /_/ |_|\____/_.___/\____/\__/
//   (c) Hochschule Luzern T&A ========== www.hslu.ch ============
//
using System.Globalization;

namespace ZumoLib;

public class Drive : ComDevice
{
    private readonly Dictionary<string, short> turnCalibrations = new()
    {
        { "eee-05500",  129 }, // ARO: CSA-3
        { "eee-05457",  125 }, // ARO: CSA-5
        { "eee-05548",  128 }, // Toemmsen: CSA-13
    };

    private readonly Dictionary<string, sbyte> driveCalibrationsForward = new()
    {
        { "eee-05500",  0 }, // ARO: CSA-3
        { "eee-05457",  40 }, // ARO: CSA-5
        { "eee-05548",  30 }, // Toemmsen: CSA-13

    };

    private readonly Dictionary<string, sbyte> driveCalibrationsBackward = new()
    {
        { "eee-05500",  0 }, // ARO: CSA-3
        { "eee-05457",  0 }, // ARO: CSA-5
        { "eee-05548",  0 }, // Toemmsen: CSA-13
    };

    public event EventHandler Finished;

    private sbyte getDriveCorrection(bool forward)
    {
        string deviceId = System.Net.Dns.GetHostName();
        if (forward)
        {
            if (driveCalibrationsForward.TryGetValue(deviceId, out sbyte factor))
            {
                return factor;
            }
        }
        else if (driveCalibrationsBackward.TryGetValue(deviceId, out sbyte factor))
        {
            return factor;
        }
        return 0;
    }

    public Drive(ICom com) : base(com, 0x24)
    {
        string deviceId = System.Net.Dns.GetHostName();
        if (turnCalibrations.TryGetValue(deviceId, out short factor))
        {
            TurnCalib(factor);
        }
        else
        {
            TurnCalib(115);
        }
    }

    /// <summary>
    /// Fährt eine Strecke gerade aus
    /// </summary>
    /// <param name="length">die zu fahrende Strecke in mm (negativer Wert => rückwärts)</param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    /// <param name="offset">Korrekturfaktor in 0.1mm/s</param>
    public void Track(short length, ushort speed, ushort acceleration, sbyte offset = 0)
    {
        string msg = SetRequest($"C{length:X4}{speed:X4}{acceleration:X4}{offset:X2}");
    }

    /// <summary>
    /// Fährt eine Strecke gerade aus und wartet bis der Befehl fertig ist
    /// </summary>
    /// <param name="length">die zu fahrende Strecke in mm (negativer Wert => rückwärts)</param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    /// <param name="offset">Korrekturfaktor in 0.1mm/s</param>
    public async Task TrackAsync(short length, ushort speed, ushort acceleration, sbyte offset = 0)
    {
        Track(length, speed, acceleration, offset);
        await WaitForFinished();
    }

    /// <summary>
    /// Fährt eine Strecke gerade aus
    /// </summary>
    /// <param name="length">die zu fahrende Strecke in mm (negativer Wert => rückwärts)</param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    public void Track(short length, ushort speed, ushort acceleration)
    {
        sbyte offset = getDriveCorrection(length >= 0);
        Track(length, speed, acceleration, offset);
    }

    /// <summary>
    /// Fährt eine Strecke gerade aus und wartet bis der Befehl fertig ist
    /// </summary>
    /// <param name="length">die zu fahrende Strecke in mm (negativer Wert => rückwärts)</param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    public async Task TrackAsync(short length, ushort speed, ushort acceleration)
    {
        Track(length, speed, acceleration);
        await WaitForFinished();
    }

    /// <summary>
    ///  Dreht an Ort und Stelle
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    public void Turn(short angle, ushort speed, ushort acceleration)
    {
        string msg = SetRequest($"A{angle:X4}{speed:X4}{acceleration:X4}");
    }

    /// <summary>
    ///  Dreht an Ort und Stelle und wartet bis der Befehl fertig ist
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    public async Task TurnAsync(short angle, ushort speed, ushort acceleration)
    {
        Turn(angle, speed, acceleration);
        await WaitForFinished();
    }

    /// <summary>
    ///  Dreht an Ort und Stelle
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    /// <param name="factor">Korrekturfaktor Istwinkel zu Sollwinkel</param>
    public void Turn(short angle, ushort speed, ushort acceleration, short factor)
    {
        string msg = SetRequest($"B{factor:X4}");
        msg = SetRequest($"A{angle:X4}{speed:X4}{acceleration:X4}");
    }


    /// <summary>
    /// Dreht an Ort und Stelle und wartet bis der Befehl fertig ist
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="speed"></param>
    /// <param name="acceleration"></param>
    /// <param name="factor">Korrekturfaktor Istwinkel zu Sollwinkel</param>
    public async Task TurnAsync(short angle, ushort speed, ushort acceleration, short factor)
    {
        Turn(angle, speed, acceleration, factor);
        await WaitForFinished();
    }

    /// <summary>
    /// Setzt den Korrekturfaktor für den Fahrbefehl "An Ort drehen".
    /// 100 entspricht 1.00,
    /// 115 entspricht beispielweise einem Korrekturfaktor von 1.15 (Istwinkel zu Sollwinkel)
    /// </summary>
    /// <param name="factor">Korrekturfaktor Istwinkel zu Sollwinkel</param>
    public void TurnCalib(short factor)
    {
        string msg = SetRequest($"B{factor:X4}");
    }

    /// <summary>
    /// Liefert die restliche Distanz zurück, bis der Zumo anhält (Fahrbefehl fertig ist)
    /// </summary>
    /// <returns>Die Distanz in mm</returns>
    public int GetRemainingDistance()
    {
        string msg = GetRequest("2");
        int dist = int.Parse(msg.Substring(4), NumberStyles.HexNumber);
        return dist;
    }

    /// <summary>
    /// Liefert True zurück, solange ein Fahrbefehl ausgeführt wird
    /// </summary>
    /// <returns>true solange der Zumo fährt</returns>
    public bool IsRunning()
    {
        string msg = GetRequest("7");
        bool running = byte.Parse(msg.Substring(4), NumberStyles.HexNumber) == 1;
        return running;
    }

    protected override bool ProcessEvent(string message)
    {
        if (message == "5!24FF")
        {
            Finished?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    private async Task WaitForFinished()
    {
        TaskCompletionSource<bool> tcs = new();
        void handler(object? s, EventArgs e) => tcs.TrySetResult(true);
        Finished += handler;
        await tcs.Task;
        Finished -= handler;
    }
}
