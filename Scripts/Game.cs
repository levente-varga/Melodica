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
			levelData.AddNoteAndPause(Note.NoteButton.X, 1);
		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.NoteButton.Y, 1);
		for (int i = 0; i < 6; i++)
			levelData.AddNoteAndPause(Note.NoteButton.A, 1);
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

		ShowTitle("MELODICA", new Vector2(1280, 200), 0.01, 4.5, 0, 3);
		ShowTitle("TUTORIAL", new Vector2(1280, 200), 4,    5.5, 3, 3);
		ShowText("Hit the 'A' button when a note reaches the line!", new Vector2(1280, 500), 15.5, 10.5, 1, 0.3);
		ShowText("Press the button that matches the note!", new Vector2(1600, 800), 42, 8.5, 1, 2);
		ShowText("You get feedback on accuracy.", new Vector2(1600, 800), 59, 8.5, 1, 2);
		ShowText("First tutorial finished!", new Vector2(1280, 500), 78, 10, 1, 3);
	}

	public override void _Process(double delta)
	{
		float playhead = musicPlayer.GetPlaybackPosition();

		foreach (Sprite2D note in notes) {
			note.Position += new Vector2(0, (float)(delta * noteSpeedPixelPerSec));
		}
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
			GetNode<CanvasLayer>("UI").AddChild(instance);
		}
	}

	void ShowText(string text, Vector2 position, double timeStamp, double duration, double fadeInDuration = 1, double fadeOutDuration = 1) {
		AnimationPhase phase = AnimationPhase.Waiting;

        Label textNode = new Label
        {
			Name = "AnimatedText",
			Text = text,
			AnchorsPreset = 10,
            Modulate = Color.Color8(255, 255, 255, 0),
			Theme = ResourceLoader.Load<Theme>("res://Themes/MenuTheme.tres"),
        };

        GetNode("UI").AddChild(textNode);

		textNode.Position = position - textNode.Size / 2;

		Timer timer = new Timer();
		AddChild(timer);
		timer.Start(timeStamp);

		timer.Timeout += () => {
			switch (phase) {
				case AnimationPhase.Waiting:
					timer.Start(fadeInDuration);
					Tween fadeInTween = CreateTween();
					phase = AnimationPhase.FadeIn;
					fadeInTween.TweenProperty(textNode, "modulate", Color.Color8(255, 255, 255, 255), fadeInDuration);
					break;
				case AnimationPhase.FadeIn:
					phase = AnimationPhase.Duration;
					timer.Start(duration);
					break;
				case AnimationPhase.Duration:
					timer.Start(fadeOutDuration);
					phase = AnimationPhase.FadeOut;
					Tween fadeOutTween = CreateTween();
					fadeOutTween.TweenProperty(textNode, "modulate", Color.Color8(255, 255, 255, 0), fadeOutDuration);
					break;
				case AnimationPhase.FadeOut:
					timer.OneShot = true;
					textNode.Dispose();
					break;
			}
		};
	}

	enum AnimationPhase {
		Waiting, 
		FadeIn,
		Duration,
		FadeOut,
	}

	void ShowTitle(string text, Vector2 position, double timeStamp, double duration, double fadeInDuration = 1, double fadeOutDuration = 1) {
		AnimationPhase phase = AnimationPhase.Waiting;

        Node2D titleNode = new Node2D
        {
			Name = "AnimatedTitle",
            Modulate = Color.Color8(255, 255, 255, 0)
        };

        GetNode("UI").AddChild(titleNode);
		titleNode.AddChild(new TitleAnimator(text, levelData.Music, position, true));

		Timer timer = new Timer();
		AddChild(timer);
		timer.Start(timeStamp);

		timer.Timeout += () => {
			switch (phase) {
				case AnimationPhase.Waiting:
					timer.Start(fadeInDuration);
					Tween fadeInTween = CreateTween();
					phase = AnimationPhase.FadeIn;
					fadeInTween.TweenProperty(titleNode, "modulate", Color.Color8(255, 255, 255, 255), fadeInDuration);
					break;
				case AnimationPhase.FadeIn:
					phase = AnimationPhase.Duration;
					timer.Start(duration);
					break;
				case AnimationPhase.Duration:
					timer.Start(fadeOutDuration);
					phase = AnimationPhase.FadeOut;
					Tween fadeOutTween = CreateTween();
					fadeOutTween.TweenProperty(titleNode, "modulate", Color.Color8(255, 255, 255, 0), fadeOutDuration);
					break;
				case AnimationPhase.FadeOut:
					timer.OneShot = true;
					titleNode.Dispose();
					break;
			}
		};
	}
}
