// RTTTL parsing and playback functionality
//  _____                                             
// |_   _|                                            
//   | | ___   ___ _ __ ___  _ __ ___  ___  ___ _ __  
//   | |/ _ \ / _ \ '_ ` _ \| '_ ` _ \/ __|/ _ \ '_ \ 
//   | | (_) |  __/ | | | | | | | | | \__ \  __/ | | |
//   \_/\___/ \___|_| |_| |_|_| |_| |_|___/\___|_| |_|
// Based on https://github.com/Toemmsen96/Pathfinder-PREN2-Team21/blob/main/RpiGPIOlib/RTTTL.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace ZumoLib
{
    /// <summary>
    /// Class that contains music-related functionality for playing songs via Sound driver
    /// </summary>
    public class RTTTL
    {
        public readonly record struct Note(int Frequency, int Duration);

        private readonly Sound _sound;

        public RTTTL(Sound sound)
        {
            _sound = sound ?? throw new ArgumentNullException(nameof(sound));
        }

        /// <summary>
        /// Default settings for RTTTL parsing
        /// </summary>
        private struct RtttlDefaults
        {
            public int Duration { get; set; }
            public int Octave { get; set; }
            public int Beat { get; set; }

            public RtttlDefaults(int duration = 4, int octave = 6, int beat = 63)
            {
                Duration = duration;
                Octave = octave;
                Beat = beat;
            }
        }

        /// <summary>
        /// Parses RTTTL defaults section
        /// </summary>
        private static RtttlDefaults ParseDefaults(string unparsedDefaults)
        {
            var defaults = new RtttlDefaults();

            foreach (var option in unparsedDefaults.Split(','))
            {
                var parts = option.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "d":
                        if (int.TryParse(value, out int duration))
                            defaults.Duration = duration;
                        break;
                    case "o":
                        if (int.TryParse(value, out int octave))
                            defaults.Octave = octave;
                        break;
                    case "b":
                        if (int.TryParse(value, out int beat))
                            defaults.Beat = beat;
                        break;
                }
            }

            return defaults;
        }

        /// <summary>
        /// Converts RTTTL melody string to Note array
        /// </summary>
        private static Note[] ParseMelody(string melody, RtttlDefaults defaults)
        {
            var notes = new[] { "c", "c#", "d", "d#", "e", "f", "f#", "g", "g#", "a", "a#", "b" };
            const double middleC = 261.63;

            // RTTTL notes are parsed as: [duration][note][#][.][octave][.] where the dot can appear
            // before or after octave depending on song source.
            var notePattern = @"^(?<duration>1|2|4|8|16|32|64)?(?<note>[a-gp])(?<sharp>#?)(?<preDot>\.?)(?<octave>[0-9]?)(?<postDot>\.?)$";
            var regex = new Regex(notePattern, RegexOptions.IgnoreCase);

            return melody.Split(',')
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(unparsedNote =>
                {
                    var match = regex.Match(unparsedNote.Trim());
                    if (!match.Success)
                        return new Note(0, 100); // Default rest note

                    // Parse duration
                    var duration = defaults.Duration;
                    if (match.Groups["duration"].Success && int.TryParse(match.Groups["duration"].Value, out int parsedDuration))
                        duration = parsedDuration;

                    // Parse note including optional sharp
                    var baseNote = match.Groups["note"].Value.ToLower();
                    var hasSharp = match.Groups["sharp"].Success && match.Groups["sharp"].Value == "#";
                    var noteName = hasSharp && baseNote != "p" ? $"{baseNote}#" : baseNote;
                    
                    // Parse dot (increases duration by 50%). Support either position.
                    var hasDot =
                        (match.Groups["preDot"].Success && match.Groups["preDot"].Value == ".") ||
                        (match.Groups["postDot"].Success && match.Groups["postDot"].Value == ".");
                    
                    // Parse octave
                    var octave = defaults.Octave;
                    if (!string.IsNullOrWhiteSpace(match.Groups["octave"].Value) &&
                        int.TryParse(match.Groups["octave"].Value, out int parsedOctave))
                        octave = parsedOctave;

                    // Calculate duration in milliseconds
                    var durationMs = (int)((240.0 / defaults.Beat / duration) * (hasDot ? 1.5 : 1) * 1000);

                    // Calculate frequency
                    int frequency = 0;
                    if (noteName != "p") // 'p' is pause/rest
                    {
                        var noteIndex = Array.IndexOf(notes, noteName);
                        if (noteIndex >= 0)
                        {
                            frequency = (int)(middleC * Math.Pow(2, octave - 4 + noteIndex / 12.0));
                        }
                    }

                    return new Note(frequency, durationMs);
                })
                .ToArray();
        }

        /// <summary>
        /// Parses a complete RTTTL string into a Note array
        /// </summary>
        /// <param name="rtttl">RTTTL format string (name:defaults:melody)</param>
        /// <returns>Array of Note objects</returns>
        public static Note[] ParseRtttl(string rtttl)
        {
            if (string.IsNullOrWhiteSpace(rtttl))
                throw new ArgumentException("RTTTL string cannot be null or empty", nameof(rtttl));

            var parts = rtttl.Split(':', 3);
            if (parts.Length < 3)
                throw new ArgumentException("Invalid RTTTL format. Expected format: name:defaults:melody", nameof(rtttl));

            var defaults = ParseDefaults(parts[1]);
            return ParseMelody(parts[2], defaults);
        }

        /// <summary>
        /// Plays an RTTTL melody once
        /// </summary>
        /// <param name="rtttl">RTTTL format string</param>
        public void PlayRtttlOnce(string rtttl)
        {
            var notes = ParseRtttl(rtttl);
            PlayNotesOnce(notes);
        }

        public void PlaySong(RtttlSong song)
        {
            PlayRtttlOnce(RTTTLSongs.Get(song));
        }

        /// <summary>
        /// Plays an array of notes once
        /// </summary>
        private void PlayNotesOnce(Note[] notes)
        {
            foreach (var note in notes)
            {
                var durationMs = (int)Math.Clamp(note.Duration, 0, Int32.MaxValue);
                var noteTimer = Stopwatch.StartNew();

                if (note.Frequency > 0)
                {
                    // Keep a short gap between notes for more natural RTTTL timing. Ensure short notes are at least 20ms.
                    var toneDurationMs = Math.Max(20, (int)(durationMs * 0.95));
                    var frequency = (UInt16)Math.Clamp(note.Frequency, 20, UInt16.MaxValue);
                    var toneDuration = (UInt16)Math.Clamp(toneDurationMs, 0, UInt16.MaxValue);

                    // Very short notes can fail sporadically on the bus; retry once before moving on.
                    if (!_sound.PlaySound(frequency, toneDuration))
                    {
                        System.Threading.Thread.Sleep(1);
                        _sound.PlaySound(frequency, toneDuration);
                    }

                    // Wait for the remaining duration (short gap between notes)
                    var remainingMs = durationMs - (int)noteTimer.ElapsedMilliseconds;
                    if (remainingMs > 0)
                    {
                        System.Threading.Thread.Sleep(remainingMs);
                    }
                }
                else
                {
                    // For pauses, sleep for the specified duration, ensuring it's at least a bit longer if it's too short
                    var pauseDuration = Math.Max(20, durationMs);
                    var pauseMs = pauseDuration - (int)noteTimer.ElapsedMilliseconds;
                    if (pauseMs > 0)
                    {
                        System.Threading.Thread.Sleep(pauseMs);
                    }
                }
            }
        }
    }
}