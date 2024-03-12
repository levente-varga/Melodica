using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public struct MusicData
{
    public string Title;
    public string Artist;
    public double BPM;
    public double Length;
    public double OffsetSec;
    public string FilePath;

    public double BeatToSec(double beat)
    {
        return OffsetSec + beat / BPM * 60;
    }

    public double SecToBeat(double seconds)
    {
        return (seconds - OffsetSec) * BPM / 60;
    }

    public double BeatLengthInSec(double beat)
    {
        return beat / BPM * 60;
    }

    public double SecLengthInBeat(double seconds)
    {
        return seconds * BPM / 60;
    }
}



public abstract class Note
{
    protected const float perfectAccuracyRangeSec = 0.045f;
    protected const float goodAccuracyRangeSec = 0.11f;
    protected const float acceptableAccuracyRangeSec = 0.25f;
    public const double SpeedPixelPerSec = 256;
    public double TimeStampSec { get; private set; }
    public double LengthSec { get; private set; } = 0;
    public double LengthPixel { get; private set; } = 0;
    public bool Firing { get; protected set; } = false;
    public bool Fired { get; protected set; } = false;

    public enum Accuracy
    {
        None, Perfect, Good, Acceptable,
    }

    public Note(double beat, double lengthBeat, double bpm, double offsetSec)
    {
        TimeStampSec = CalculateTimeStamp(beat, bpm, offsetSec);
        LengthPixel = CalculateLength(lengthBeat, bpm);
        LengthSec = lengthBeat;
    }

    public double GetDistance(double playhead)
    {
        return Mathf.Abs(playhead - TimeStampSec);
    }

    private double CalculateTimeStamp(double beat, double bpm, double offsetSec)
    {
        double bps = bpm / 60;
        double secPerBeat = 1.0 / bps;
        return beat * secPerBeat + offsetSec;
    }

    private double CalculateLength(double lengthBeat, double bpm)
    {
        double bps = bpm / 60;
        double secPerBeat = 1.0 / bps;
        Debug.WriteLine($"Length: {lengthBeat * secPerBeat * SpeedPixelPerSec}, BPS = {bps}, SPB = {secPerBeat}, lengthBeat = {lengthBeat}, speed P/s = {SpeedPixelPerSec}");
        return lengthBeat * secPerBeat * SpeedPixelPerSec;
    }

    public abstract Accuracy Fire(double playhead);
}



public class MelodyNote : Note
{
    const int WidthPixel = 16;
    const int GapWidthPixel = 8;
    public int Octave { get; private set; }
    public Tones Tone { get; private set; }
    public double PositionX { get; private set; }
    public enum Tones
    {
        C, Csharp, D, Dsharp, E, F, Fsharp, G, Gsharp, A, Asharp, B
    }

    public MelodyNote(Tones tone, int octave, double beat, double length, double bpm, double offset)
        : base(beat, length, bpm, offset)
    {
        this.Tone = tone;
        Octave = octave;
    }

    public override Accuracy Fire(double playhead)
    {
        throw new System.NotImplementedException();
    }

    public void CalculatePosition(Tones lowestTone, int lowestOctave, Tones highestTone, int highestOctave)
    {
        double screenCenterX = PositionX = Settings.Display.Resolution.X / 2;
        int distance = (int)highestTone - (int)lowestTone + (highestOctave - lowestOctave) * 12;
        if (distance == 0)
        {
            PositionX = screenCenterX;
            return;
        }
        int totalWidthPixel = distance * WidthPixel + (distance - 1) * GapWidthPixel;
        int positionInsideDistance = (int)Tone - (int)lowestTone + (Octave - lowestOctave) * 12;
        double leftmostPosition = screenCenterX - totalWidthPixel / 2;
        PositionX = leftmostPosition + positionInsideDistance * (WidthPixel + GapWidthPixel);
    }
}



public class BeatNote : Note
{
    public Color ColorType { get; private set; }

    public enum Color
    {
        Green, Red, Blue, Yellow
    }

    public BeatNote(Color button, double beat, double lengthSec, double bpm, double offsetSec)
        : base(beat, lengthSec, bpm, offsetSec)
    {
        ColorType = button;
    }

    public override Accuracy Fire(double playhead)
    {
        Fired = true;
        double distance = GetDistance(playhead);

        if (distance > acceptableAccuracyRangeSec)
        {
            Fired = false;
            return Accuracy.None;
        }
        else if (distance > goodAccuracyRangeSec)
        {
            return Accuracy.Acceptable;
        }
        else if (distance > perfectAccuracyRangeSec)
        {
            return Accuracy.Good;
        }
        else
        {
            return Accuracy.Perfect;
        }
    }
}



public class LevelData
{
    public MusicData Music { get; private set; }

    public List<AnimatedText> AnimatedTexts { get; private set; }
    public List<BeatNote> BeatNotes { get; private set; }
    public List<MelodyNote> MelodyNotes { get; private set; }

    public LevelData(MusicData musicData)
    {
        Music = musicData;
        BeatNotes = new List<BeatNote>();
        MelodyNotes = new List<MelodyNote>();
    }

    double composingAtBeat;

    public void AddAnimatedText(AnimatedText animatedText)
    {
        if (AnimatedTexts == null) AnimatedTexts = new();
        AnimatedTexts.Add(animatedText);
    }

    public void StartComposing()
    {
        BeatNotes.Clear();
        MelodyNotes.Clear();
        composingAtBeat = 0;
    }

    public void AddBeatNote(BeatNote.Color button)
    {
        AddBeatNoteAndPause(button, 0);
    }

    public void AddMelodyNote(MelodyNote.Tones key, int octave, double lengthSec)
    {
        AddMelodyNoteAndPause(key, octave, lengthSec, 0);
    }

    public void AddBeatNoteAndPause(BeatNote.Color button, double pauseForBeats)
    {
        BeatNotes.Add(new BeatNote(button, composingAtBeat, 0, Music.BPM, Music.OffsetSec));
        composingAtBeat += pauseForBeats;
    }

    public void AddMelodyNoteAndPause(MelodyNote.Tones key, int octave, double lengthSec, double pauseForBeats)
    {
        MelodyNotes.Add(new MelodyNote(key, octave, composingAtBeat, lengthSec, Music.BPM, Music.OffsetSec));
        composingAtBeat += pauseForBeats;
    }

    public void AddPause(double pauseForBeats)
    {
        composingAtBeat += pauseForBeats;
    }

    public void FinishComposing()
    {
        if (MelodyNotes.Count == 0) return;

        int lowestOctave = MelodyNotes[0].Octave;
        int highestOctave = lowestOctave;
        MelodyNote.Tones lowestTone = MelodyNotes[0].Tone;
        MelodyNote.Tones highestTone = lowestTone;

        foreach (MelodyNote note in MelodyNotes)
        {
            if (note.Octave * 12 + (int)note.Tone > highestOctave * 12 + (int)highestTone)
            {
                highestOctave = note.Octave;
                highestTone = note.Tone;
            }
            if (note.Octave * 12 + (int)note.Tone < lowestOctave * 12 + (int)lowestTone)
            {
                lowestOctave = note.Octave;
                lowestTone = note.Tone;
            }
        }

        foreach (MelodyNote note in MelodyNotes)
        {
            note.CalculatePosition(lowestTone, lowestOctave, highestTone, highestOctave);
        }
    }
}