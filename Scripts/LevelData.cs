using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public struct MusicData {
    public string Title;
    public string Composer;
    public double BPM;
    public double Length;
    public double OffsetSec;
}

public class Note {
    public double TimeStampSec { get; private set; }
    double length;
    bool hold;
    public NoteButton Button { get; private set; }
    public bool Used { get; private set; } = false;

    public enum NoteButton {
        A, B, X, Y
    }

    public Note(NoteButton button, double beat, double bpm, double offsetSec) {
        double bps = bpm / 60;
        double secPerBeat = 1.0 / bps;
        TimeStampSec = beat * secPerBeat + offsetSec;
        Button = button;
    }

    public void Use() {
        Used = false;
    }
}

public class LevelData {
    string title;
    public MusicData Music { get; private set; }

    public List<Note> Notes { get; private set; }

    public LevelData(MusicData musicData) {
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

    public void AddNote(Note.NoteButton button) {
        AddNoteAndPause(button, 0);
    }

    public void AddNoteAndPause(Note.NoteButton button, double pauseForBeats)
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
            if (note.TimeStampSec - playheadPositionSec < hitTresholdSec && !note.Used)
                candidates.Add(note);

        if (candidates.Count == 0) return;

        candidates.Sort((a, b) => Mathf.Sign(a.TimeStampSec - b.TimeStampSec));
        candidates[0].Use();
    }
}