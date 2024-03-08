using Godot;
using System.Collections.Generic;

public partial class Game : Node2D
{
	LevelData levelData;

	MusicPlayer musicPlayer;

	Label lBeat;
	Label lSmoothen;
	Label lPlayhead;
	Label lSmoothPlayhead;
	Label lDifference;

	bool showAccuracy = false;
	int perfectStreak = 0;

	const int linePosY = 860;
	List<Sprite2D> notes;
	const double noteSpeedPixelPerSec = 256;

	public override void _Ready()
	{
		CreateLevelData();
		GetNodes();
		CreateNotes();
		CreateTexts();

		Timer showAccuracyTimer = new Timer()
		{
			WaitTime = 61.5,
			Autostart = true,
			OneShot = true,
		};
		showAccuracyTimer.Timeout += () =>
		{
			showAccuracy = true;
		};
		AddChild(showAccuracyTimer);
	}

	bool smoothen = true;
	bool debug = true;
	public override void _Process(double delta)
	{
		MoveNotes(delta);
		UpdateDebugInfo();
		HandleInput();
	}

	private void UpdateDebugInfo()
	{
		double playhead = musicPlayer.GetPlaybackPosition();
		double smoothPlayhead = musicPlayer.GetSmoothPlaybackPosition();
		lBeat.Text = string.Format("Beat:        {0:F1}", musicPlayer.Beat);
		lSmoothen.Text = string.Format("Smoothen:    {0}", smoothen ? "true " : "false");
		lPlayhead.Text = string.Format("Playhead:    {0:F4}", playhead);
		lSmoothPlayhead.Text = string.Format("Smooth:      {0:F4}", smoothPlayhead);
		double difference = smoothPlayhead - playhead;
		lDifference.Text = string.Format("Difference: {0}{1:F4}", difference < 0 ? "-" : " ", Mathf.Abs(difference));
	}

	private void MoveNotes(double delta)
	{
		float playhead = musicPlayer.GetPlaybackPosition();
		float smoothPlayhead = musicPlayer.GetSmoothPlaybackPosition();

		for (int i = 0; i < notes.Count; i++)
		{
			notes[i].Position = new Vector2(1280, linePosY - (float)((levelData.Notes[i].TimeStampSec - (smoothen ? smoothPlayhead : playhead)) * noteSpeedPixelPerSec));
		}
	}

	private void HandleInput()
	{
		Note.Accuracy accuracy = Note.Accuracy.None;

		if (Input.IsActionJustPressed("note_green"))
		{
			accuracy = TryToFireBeatNote(Note.Color.Green);
		}
		else if (Input.IsActionJustPressed("note_red"))
		{
			accuracy = TryToFireBeatNote(Note.Color.Red);
		}
		else if (Input.IsActionJustPressed("note_blue"))
		{
			accuracy = TryToFireBeatNote(Note.Color.Blue);
		}
		else if (Input.IsActionJustPressed("note_yellow"))
		{
			accuracy = TryToFireBeatNote(Note.Color.Yellow);
		}

		if (accuracy != Note.Accuracy.None && showAccuracy)
		{
			string text;
			switch (accuracy)
			{
				case Note.Accuracy.Perfect: text = "Perfect"; break;
				case Note.Accuracy.Good: text = "Good"; break;
				case Note.Accuracy.Acceptable: text = "OK"; break;
				default: text = "?"; break;
			}
			if (accuracy == Note.Accuracy.Perfect)
			{
				perfectStreak++;
				if (perfectStreak > 1) text = $"x{perfectStreak} {text}";
			}
			else
			{
				perfectStreak = 0;
			}
			GetNode("UI").AddChild(new AnimatedLabel()
			{
				Text = text,
				Position = new Vector2(1260, 830),
				Alignment = new AnimatedText.TextAlignment() { Horizontal = HorizontalAlignment.Right },
				Velocity = new Vector2(-200, 0),
				Drag = 8,
				Color = accuracy == Note.Accuracy.Perfect ? Colors.Yellow : Colors.White,
				StartAtSec = 0.001,
				Duration = 0.2,
				FadeInSec = 0,
				FadeOutSec = 0.4,
			});
		}
	}

	private Note.Accuracy TryToFireBeatNote(Note.Color color)
	{
		if (levelData.Notes.Count == 0) return Note.Accuracy.None;

		float smoothPlayhead = musicPlayer.GetSmoothPlaybackPosition();
		Note candidate = null;
		double minDistance = 0;

		foreach (Note note in levelData.Notes)
		{
			if (note.Fired || note.ColorType != color) continue;
			if (candidate == null || note.GetDistance(smoothPlayhead) < minDistance)
			{
				minDistance = note.GetDistance(smoothPlayhead);
				candidate = note;
			}
		}

		if (debug) GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = $"{(candidate == null ? "No candidate" : $"{minDistance:F4}")}",
			Font = "res://Fonts/SourceCodePro-Light.ttf",
			Color = new Color(1, 1, 1, 0.15f),
			Alignment = new AnimatedText.TextAlignment() { Horizontal = HorizontalAlignment.Right },
			Position = new Vector2(1260, 890),
			Velocity = new Vector2(-200, 0),
			Drag = 8,
			StartAtSec = 0.001,
			Duration = 0.2,
			FadeInSec = 0,
			FadeOutSec = 0.4,
		});

		if (candidate == null) return Note.Accuracy.None;
		return candidate.Fire(musicPlayer.GetPlaybackPosition());
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
				GetTree().Quit();
			if (eventKey.Pressed && eventKey.Keycode == Key.Space)
				smoothen = !smoothen;
		}
	}

	private void CreateLevelData()
	{
		levelData = new LevelData(Musics.BlueParrot);
		levelData.StartComposing();
		levelData.AddPause(1);
		levelData.AddNote(Note.Color.Green);
		levelData.AddPause(63);
		for (int i = 0; i < 32; i++)
			levelData.AddNoteAndPause(Note.Color.Green, 1);

		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.Color.Red, 1);
		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.Color.Blue, 1);
		for (int i = 0; i < 8; i++)
			levelData.AddNoteAndPause(Note.Color.Yellow, 1);
		for (int i = 0; i < 6; i++)
			levelData.AddNoteAndPause(Note.Color.Green, 1);
		levelData.AddPause(2);

		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Red, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Blue, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Red, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Yellow, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Red, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Blue, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Red, 1.5);
		levelData.AddNoteAndPause(Note.Color.Green, 2.5);
		levelData.AddNoteAndPause(Note.Color.Yellow, 1.5);
		levelData.AddNote(Note.Color.Green);
	}

	private void GetNodes()
	{
		lBeat = GetNode<Label>("UI/Correction");
		lSmoothen = GetNode<Label>("UI/Smoothen");
		lPlayhead = GetNode<Label>("UI/Playhead");
		lSmoothPlayhead = GetNode<Label>("UI/SmoothPlayhead");
		lDifference = GetNode<Label>("UI/Difference");

		musicPlayer = new(levelData.Music);
		AddChild(musicPlayer);
	}

	private void CreateNotes()
	{
		notes = new List<Sprite2D>();
		PackedScene noteSprite;

		foreach (Note note in levelData.Notes)
		{
			switch (note.ColorType)
			{
				case Note.Color.Green:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteA.tscn");
					break;
				case Note.Color.Red:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteB.tscn");
					break;
				case Note.Color.Blue:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteX.tscn");
					break;
				case Note.Color.Yellow:
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

	private void CreateTexts()
	{
		GetNode("UI").AddChild(new AnimatedTitle()
		{
			Text = "MELODICA",
			Position = new Vector2(1280, 200),
			StartAtSec = 0.01,
			Duration = 4.5,
			FadeInSec = 0,
			FadeOutSec = 3,
			Music = levelData.Music,
			LetterExpansionAnimation = false,
		});
		GetNode("UI").AddChild(new AnimatedTitle()
		{
			Text = "TUTORIAL",
			Position = new Vector2(1280, 200),
			StartAtSec = 4,
			Duration = 5.5,
			FadeInSec = 3,
			FadeOutSec = 3,
			Music = levelData.Music,
			LetterExpansionAnimation = false,
		});
		GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = "Hit the 'A' button when a note reaches the line!",
			Position = new Vector2(1280, 500),
			StartAtSec = 15.5,
			Duration = 10.5,
			FadeInSec = 1,
			FadeOutSec = 0.3,
		});
		GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = "Press the button that matches the note!",
			Position = new Vector2(1600, 800),
			StartAtSec = 42,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = "You get feedback on accuracy.",
			Position = new Vector2(1600, 800),
			StartAtSec = 59,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = "First tutorial finished!",
			Position = new Vector2(1280, 500),
			StartAtSec = 78,
			Duration = 6,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
	}
}
