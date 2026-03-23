using System;
using System.Collections.Generic;
using System.Linq;

namespace ZumoLib;

public enum RtttlSong
{
    MissionImpossible,
    IndianaJones,
    StarWars,
    Bond007,
    AxelF,
    Barbie,
    BennyHill,
    Bird,
    Cantina,
    Dream,
    Entertainer,
    FinalCountdown,
    Popcorn,
    RichMan,
    TakeOnMe,
    YMCA
}

public static class RTTTLSongs
{
    private static readonly IReadOnlyDictionary<RtttlSong, string> Songs = new Dictionary<RtttlSong, string>
    {
        [RtttlSong.MissionImpossible] = "Mission Impossible:d=16,o=6,b=95:32d,32d#,32d,32d#,32d,32d#,32d,32d#,32d,32d,32d#,32e,32f,32f#,32g,g,8p,g,8p,a#,p,c7,p,g,8p,g,8p,f,p,f#,p,g,8p,g,8p,a#,p,c7,p,g,8p,g,8p,f,p,f#,p,a#,g,2d,32p,a#,g,2c#,32p,a#,g,2c,a#5,8c,2p,32p,a#5,g5,2f#,32p,a#5,g5,2f,32p,a#5,g5,2e,d#,8d",
        [RtttlSong.IndianaJones] = "Indiana Jones:d=4,o=5,b=250:e,8p,8f,8g,8p,1c6,8p.,d,8p,8e,1f,p.,g,8p,8a,8b,8p,1f6,p,a,8p,8b,2c6,2d6,2e6,e,8p,8f,8g,8p,1c6,p,d6,8p,8e6,1f.6,g,8p,8g,e.6,8p,d6,8p,8g,e.6,8p,d6,8p,8g,f.6,8p,e6,8p,8d6,2c6",
        [RtttlSong.StarWars] = "Star Wars:d=4,o=5,b=45:32p,32f#,32f#,32f#,8b.,8f#.6,32e6,32d#6,32c#6,8b.6,16f#.6,32e6,32d#6,32c#6,8b.6,16f#.6,32e6,32d#6,32e6,8c#.6,32f#,32f#,32f#,8b.,8f#.6,32e6,32d#6,32c#6,8b.6,16f#.6,32e6,32d#6,32c#6,8b.6,16f#.6,32e6,32d#6,32e6,8c#6",
        [RtttlSong.Bond007] = "007:o=5,d=4,b=320,b=320:c,8d,8d,d,2d,c,c,c,c,8d#,8d#,2d#,d,d,d,c,8d,8d,d,2d,c,c,c,c,8d#,8d#,d#,2d#,d,c#,c,c6,1b.,g,f,1g.",
        [RtttlSong.AxelF] = "Axel:o=5,d=8,b=125,b=125:16g,16g,a#.,16g,16p,16g,c6,g,f,4g,d6.,16g,16p,16g,d#6,d6,a#,g,d6,g6,16g,16f,16p,16f,d,a#,2g,4p,16f6,d6,c6,a#,4g,a#.,16g,16p,16g,c6,g,f,4g,d6.,16g,16p,16g,d#6,d6,a#,g,d6,g6,16g,16f,16p,16f,d,a#,2g",
        [RtttlSong.Barbie] = "Barbie Girl:o=5,d=8,b=125,b=125:g#,e,g#,c#6,4a,4p,f#,d#,f#,b,4g#,f#,e,4p,e,c#,4f#,4c#,4p,f#,e,4g#,4f#",
        [RtttlSong.BennyHill] = "Benny Hill:o=5,d=16,b=125,b=125:8d.,e,8g,8g,e,d,a4,b4,d,b4,8e,d,b4,a4,b4,8a4,a4,a#4,b4,d,e,d,4g,4p,d,e,d,8g,8g,e,d,a4,b4,d,b4,8e,d,b4,a4,b4,8d,d,d,f#,a,8f,4d,4p,d,e,d,8g,g,g,8g,g,g,8g,8g,e,8e.,8c,8c,8c,8c,e,g,a,g,a#,8g,a,b,a#,b,a,b,8d6,a,b,d6,8b,8g,8d,e6,b,b,d,8a,8g,4g",
        [RtttlSong.Bird] = "Birdy Song:o=5,d=16,b=100,b=100:g,g,a,a,e,e,8g,g,g,a,a,e,e,8g,g,g,a,a,c6,c6,8b,8b,8a,8g,8f,f,f,g,g,d,d,8f,f,f,g,g,d,d,8f,f,f,g,g,a,b,8c6,8a,8g,8e,4c",
        [RtttlSong.Cantina] = "Cantina:o=5,d=8,b=250,b=250:a,p,d6,p,a,p,d6,p,a,d6,p,a,p,g#,4a,a,g#,a,4g,f#,g,f#,4f.,d.,16p,4p.,a,p,d6,p,a,p,d6,p,a,d6,p,a,p,g#,a,p,g,p,4g.,f#,g,p,c6,4a#,4a,4g",
        [RtttlSong.Dream] = "Dream:o=4,d=8,b=220,b=220:c3,4p.,c,4p,d#3,p,d#,d#3,p,d#,p,d#3,p,f3,4p.,f,4p,g3,p,g,g3,p,a#3,p,c,p,c3,p,f5,p,c,p,c5,d#3,f5,d#,d#3,p,d#,f5,d#3,g5,f3,p,f5,p,f,p,f5,g3,g5,g,g3,p,a#3,p,c,p,c3,g5,f5,p,c,g5,c5,d#3,f5,d#,d#3,p,d#,f5,d#3,g5,f3,g,f5,p,f,g5,f5,g3,g5,g,g3,p,a#3,d#5,c,p,c3,g5,f5,c5,c,g5,c5,d#3,f5,d#,d#3,g5,d#,f5,d#3,g5,f3,g,f5,d#5,f,g5,f5,g3,g5,g,g3,f5,a#3,d#5,c,c5,c3,g5,f5,p,c,g5,c5,d#3,f5,d#,d#3,p,d#,f5,d#3,g5,f3,g,f5,p,f,g5,f5,g3,g5,g,g3,p,a#3,d#5,c,p,c3,p,f5,p,c,p,c5,d#3,f5,d#,d#3,p,d#,f5,d#3,g5,f3,p,f5,p,f,p,f5,g3,g5,g,g3,p,a#3,p,c,p,c3,4p.,c,4p,d#3,p,d#,d#3,p,d#,p,d#3,p,f3,4p.,f,4p,g3,p,g,g3,p,a#3,p,c",
        [RtttlSong.Entertainer] = "Entertainer:o=5,d=8,b=140,b=140:d,d#,e,4c6,e,4c6,e,2c6.,c6,d6,d#6,e6,c6,d6,4e6,b,4d6,2c6,4p,d,d#,e,4c6,e,4c6,e,2c6.,p,a,g,f#,a,c6,4e6,d6,c6,a,2d6",
        [RtttlSong.FinalCountdown] = "Final Countdown:o=5,d=16,b=125,b=125:b,a,4b,4e,4p,8p,c6,b,8c6,8b,4a,4p,8p,c6,b,4c6,4e,4p,8p,a,g,8a,8g,8f#,8a,4g.,f#,g,4a.,g,a,8b,8a,8g,8f#,4e,4c6,2b.,b,c6,b,a,1b",
        [RtttlSong.Popcorn] = "Popcorn:o=5,d=16,b=160,b=160:a,p,g,p,a,p,e,p,c,p,e,p,8a4,8p,a,p,g,p,a,p,e,p,c,p,e,p,8a4,8p,a,p,b,p,c6,p,b,p,c6,p,a,p,b,p,a,p,b,p,g,p,a,p,g,p,a,p,f,8a,8p,a,p,g,p,a,p,e,p,c,p,e,p,8a4,8p,a,p,g,p,a,p,e,p,c,p,e,p,8a4,8p,a,p,b,p,c6,p,b,p,c6,p,a,p,b,p,a,p,b,p,g,p,a,p,g,p,a,p,b,4c6",
        [RtttlSong.RichMan] = "Rich Man's World:o=6,d=8,b=112,b=112:e,e,e,e,e,e,16e5,16a5,16c,16e,d#,d#,d#,d#,d#,d#,16f5,16a5,16c,16d#,4d,c,a5,c,4c,2a5,32a5,32c,32e,a6",
        [RtttlSong.TakeOnMe] = "Take On Me:o=5,d=8,b=160,b=160:f#,f#,f#,d,p,b4,p,e,p,e,p,e,g#,g#,a,b,a,a,a,e,p,d,p,f#,p,f#,p,f#,e,e,f#,e,f#,f#,f#,d,p,b4,p,e,p,e,p,e,g#,g#,a,b,a,a,a,e,p,d,p,f#,p,f#,p,f#,e,e5",
        [RtttlSong.YMCA] = "YMCA:o=5,d=8,b=160,b=160:c#6,a#,2p,a#,g#,f#,g#,a#,4c#6,a#,4c#6,d#6,a#,2p,a#,g#,f#,g#,a#,4c#6,a#,4c#6,d#6,b,2p,b,a#,g#,a#,b,4d#6,f#6,4d#6,4f6.,4d#6.,4c#6.,4b.,4a#,4g#"
    };

    public static IReadOnlyCollection<RtttlSong> All => Songs.Keys.ToArray();

    public static string Get(RtttlSong song)
    {
        if (!Songs.TryGetValue(song, out var rtttl))
        {
            throw new ArgumentOutOfRangeException(nameof(song), song, "RTTTL song not found.");
        }

        return rtttl;
    }
}
