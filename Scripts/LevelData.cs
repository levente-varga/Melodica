using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public struct MusicData
{
    public string Title;
    public string Composer;
    public double BPM;
    public double Length;
    public double OffsetSec;
}

public class Note
{
    public double TimeStampSec { get; private set; }
    double length;
    bool hold;
    public Color ColorType { get; private set; }
    public bool Fired { get; private set; } = false;

    public enum Color
    {
        Green, Red, Blue, Yellow
    }

    const float perfectAccuracyRangeSec = 0.045f;
    const float goodAccuracyRangeSec = 0.11f;
    const float acceptableAccuracyRangeSec = 0.25f;
    public enum Accuracy
    {
        None,       // Button press but no note in range
        Perfect,    // 
        Good,       // 
        Acceptable,         // 
        Miss,       // Note left range without firing
    }

    public Note(Color button, double beat, double bpm, double offsetSec)
    {
        double bps = bpm / 60;
        double secPerBeat = 1.0 / bps;
        TimeStampSec = beat * secPerBeat + offsetSec;
        ColorType = button;
    }

    public Accuracy Fire(double playhead)
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

    public double GetDistance(double playhead)
    {
        return Mathf.Abs(playhead - TimeStampSec);
    }
}

public class LevelData
{
    string title;
    public MusicData Music { get; private set; }

    public List<Note> Notes { get; private set; }

    public LevelData(MusicData musicData)
    {
        Music = musicData;
        Notes = new List<Note>();
    }

    double composingAtBeat;
    const double hitTresholdSec = 0.5;

    public void StartComposing()
    {
        Notes.Clear();
        composingAtBeat = 0;
    }

    public void AddNote(Note.Color button)
    {
        AddNoteAndPause(button, 0);
    }

    public void AddNoteAndPause(Note.Color button, double pauseForBeats)
    {
        Notes.Add(new Note(button, composingAtBeat, Music.BPM, Music.OffsetSec));
        composingAtBeat += pauseForBeats;
    }

    public void AddPause(double pauseForBeats)
    {
        composingAtBeat += pauseForBeats;
    }

    private void HitNote(double playheadPositionSec)
    {
        List<Note> candidates = new List<Note>();

        foreach (Note note in Notes)
            if (note.TimeStampSec - playheadPositionSec < hitTresholdSec && !note.Fired)
                candidates.Add(note);

        if (candidates.Count == 0) return;

        candidates.Sort((a, b) => Mathf.Sign(a.TimeStampSec - b.TimeStampSec));
        candidates[0].Fire(playheadPositionSec);
    }
}