//  _____                                             
// |_   _|                                            
//   | | ___   ___ _ __ ___  _ __ ___  ___  ___ _ __  
//   | |/ _ \ / _ \ '_ ` _ \| '_ ` _ \/ __|/ _ \ '_ \ 
//   | | (_) |  __/ | | | | | | | | | \__ \  __/ | | |
//   \_/\___/ \___|_| |_| |_|_| |_| |_|___/\___|_| |_|
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
        frequencyHz = (UInt16)Math.Clamp(frequencyHz, (UInt16)10, (UInt16)15500); // 15500 Hz is the highest frequency the Zumo can produce
        durationMs = (UInt16)Math.Clamp(durationMs, (UInt16)0, (UInt16)65535);
        String message = $"0{frequencyHz:X4}{durationMs:X4}";
        return SetRequest(5, 0x50, message) == "5<50Sound";
    }
    public bool PlaySound(SoundItem item)
    {
        return SetRequest(5, 0x50, $"1{((int)item):X1}") == "5<50Sound";
    }

}
