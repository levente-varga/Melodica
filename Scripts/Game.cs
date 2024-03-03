using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Game : Node2D
{
	LevelData levelData;

	AudioStreamPlayer musicPlayer;

	const int linePosY = 860;
	List<Sprite2D> notes;
	const double noteSpeedPixelPerSec = 256;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNodes();

		levelData = new LevelData(new MusicData {
			Title = "Blue Parrot",
			Composer = "Romain Garcia",
			BPM = 123,
			OffsetSec = 0,
		});
		levelData.StartComposing();
		levelData.AddPause(64);
		for (int i = 0; i < 32; i++)
			levelData.AddNoteAndPause(Note.NoteButton.A, 1);

		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.NoteButton.B, 1);
		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.NoteButton.A, 1);
		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.NoteButton.X, 1);
		for (int i = 0; i < 6; i++)
			levelData.AddNoteAndPause(Note.NoteButton.Y, 1);
		levelData.AddPause(2);

		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.B, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.X, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.B, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.Y, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.B, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.X, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.B, 1.5);
		levelData.AddNoteAndPause(Note.NoteButton.A, 2.5);
		levelData.AddNoteAndPause(Note.NoteButton.Y, 1.5);
		levelData.AddNote(Note.NoteButton.A);

		CreateNotes();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float playhead = musicPlayer.GetPlaybackPosition();

		foreach (Sprite2D note in notes) {
			note.Position += new Vector2(0, (float)(delta * noteSpeedPixelPerSec));
		}

		//if (Input.)

		Debug.WriteLine("Timestamp: {0}, Position: {1}", levelData.Notes[0].TimeStampSec, notes[0].Position);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
			if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
				GetTree().Quit();
	}

	void GetNodes() {
		musicPlayer = GetNode<AudioStreamPlayer>("musicPlayer");
	}

	void CreateNotes() {
		notes = new List<Sprite2D>();
		PackedScene noteSprite;
		
		foreach (Note note in levelData.Notes) {
			switch (note.Button) {
				case Note.NoteButton.A:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteA.tscn");
					break;
				case Note.NoteButton.B:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteB.tscn");
					break;
				case Note.NoteButton.X:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteX.tscn");
					break;
				case Note.NoteButton.Y:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteY.tscn");
					break;
				default:
					noteSprite = new PackedScene();
					break;
			}
			Sprite2D instance = noteSprite.Instantiate<Sprite2D>();
			instance.Position = new Vector2(1280, (float)(linePosY - note.TimeStampSec * noteSpeedPixelPerSec));
			notes.Add(instance);
			GetNode<CanvasLayer>("CanvasLayer").AddChild(instance);
		}
	}

	void ShowText(string text, double timeStamp, double duration, double fateDuration = 1) {
		//AddChild();
	}

	void ShowTitle(string text, double timeStamp, double duration, double fadeDuration = 1) {

	}
}
