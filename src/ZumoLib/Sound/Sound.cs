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

public class Sound : ComDevice
{

    public Sound(ICom com) : base(com, 0x50) { }

    public bool PlaySound(UInt16 frequencyHz, UInt16 durationMs)
    {
        frequencyHz = (UInt16)Math.Clamp(frequencyHz, (UInt16)20, (UInt16)65535);
        durationMs = (UInt16)Math.Clamp(durationMs, (UInt16)0, (UInt16)65535);
        String message = $"0{frequencyHz:X4}{durationMs:X4}";
        return SetRequest(5, 0x50, message) == "5<50Sound";
    }
    public bool PlaySound(SoundItem item)
    {
        return SetRequest(5, 0x50, $"1{((int)item):X1}") == "5<50Sound";
    }

}
