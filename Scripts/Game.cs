using Godot;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class Game : Node2D
{
	LevelData loadedLevelData;
	LevelData beatTutorialLevelData;
	LevelData melodyTutorialLevelData;

	MusicPlayer musicPlayer;

	Label lBeat;
	Label lSmoothen;
	Label lPlayhead;
	Label lSmoothPlayhead;
	Label lDifference;

	bool showAccuracy = false;
	int perfectStreak = 0;

	const int linePosY = 860;
	List<Sprite2D> beatNotes;
	List<MeshInstance2D> melodyNotes;

	public override void _Ready()
	{
		GetNodes();
		CreateLevelData();
		loadedLevelData = beatTutorialLevelData;
		SetupLevel();
		CreateBeatNotes();
		CreateMelodyNotes();

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

		for (int i = 0; i < beatNotes.Count; i++)
		{
			beatNotes[i].Position = new Vector2(
				beatNotes[i].Position.X,
				linePosY - (float)((loadedLevelData.BeatNotes[i].TimeStampSec - (smoothen ? smoothPlayhead : playhead)) * Note.SpeedPixelPerSec)
			);
		}
		for (int i = 0; i < melodyNotes.Count; i++)
		{
			melodyNotes[i].Position = new Vector2(
				melodyNotes[i].Position.X,
				linePosY - (float)((loadedLevelData.MelodyNotes[i].TimeStampSec - (smoothen ? smoothPlayhead : playhead)) * Note.SpeedPixelPerSec + loadedLevelData.MelodyNotes[i].LengthPixel / 2)
			);
		}
	}

	private void HandleInput()
	{
		Note.Accuracy accuracy = Note.Accuracy.None;

		if (Input.IsActionJustPressed("note_green"))
		{
			accuracy = TryToFireBeatNote(BeatNote.Color.Green);
		}
		else if (Input.IsActionJustPressed("note_red"))
		{
			accuracy = TryToFireBeatNote(BeatNote.Color.Red);
		}
		else if (Input.IsActionJustPressed("note_blue"))
		{
			accuracy = TryToFireBeatNote(BeatNote.Color.Blue);
		}
		else if (Input.IsActionJustPressed("note_yellow"))
		{
			accuracy = TryToFireBeatNote(BeatNote.Color.Yellow);
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

	private Note.Accuracy TryToFireBeatNote(BeatNote.Color color)
	{
		if (loadedLevelData.BeatNotes.Count == 0) return Note.Accuracy.None;

		float smoothPlayhead = musicPlayer.GetSmoothPlaybackPosition();
		BeatNote candidate = null;
		double minDistance = 0;

		foreach (BeatNote note in loadedLevelData.BeatNotes)
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
		beatTutorialLevelData = new LevelData(Musics.BlueParrot);
		beatTutorialLevelData.StartComposing();
		beatTutorialLevelData.AddPause(64);
		for (int i = 0; i < 32; i++)
			beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 1);

		for (int i = 0; i < 8; i++)
			beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Red, 1);
		for (int i = 0; i < 8; i++)
			beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Blue, 1);
		for (int i = 0; i < 8; i++)
			beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Yellow, 1);
		for (int i = 0; i < 6; i++)
			beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 1);
		beatTutorialLevelData.AddPause(2);

		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Red, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Blue, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Red, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Yellow, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Red, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Blue, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Red, 1.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Green, 2.5);
		beatTutorialLevelData.AddBeatNoteAndPause(BeatNote.Color.Yellow, 1.5);
		beatTutorialLevelData.AddBeatNote(BeatNote.Color.Green);

		beatTutorialLevelData.FinishComposing();

		beatTutorialLevelData.AddAnimatedText(new AnimatedTitle()
		{
			Text = "MELODICA",
			Position = new Vector2(1280, 200),
			StartAtSec = 0.01,
			Duration = 4.5,
			FadeInSec = 0,
			FadeOutSec = 3,
			MusicPlayer = musicPlayer,
			LetterExpansionAnimation = false,
		});
		beatTutorialLevelData.AddAnimatedText(new AnimatedTitle()
		{
			Text = "TUTORIAL",
			Position = new Vector2(1280, 200),
			StartAtSec = 4,
			Duration = 5.5,
			FadeInSec = 3,
			FadeOutSec = 3,
			MusicPlayer = musicPlayer,
			LetterExpansionAnimation = false,
		});
		beatTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "Hit the 'A' button when a note reaches the line!",
			Position = new Vector2(1280, 500),
			StartAtSec = 15.5,
			Duration = 10.5,
			FadeInSec = 1,
			FadeOutSec = 0.3,
		});
		beatTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "Press the button that matches the note!",
			Position = new Vector2(1600, 800),
			StartAtSec = 42,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		beatTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "You get feedback on accuracy.",
			Position = new Vector2(1600, 800),
			StartAtSec = 59,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		beatTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "First tutorial finished!",
			Position = new Vector2(1280, 500),
			StartAtSec = 78,
			Duration = 6,
			FadeInSec = 1,
			FadeOutSec = 2,
		});

		melodyTutorialLevelData = new LevelData(Musics.StarSky);
		melodyTutorialLevelData.StartComposing();
		melodyTutorialLevelData.AddPause(32);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 3, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 0.25, 0.5);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 3, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 0.25, 0.5);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.E, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 0.25, 0.5);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 3, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 0.25, 0.5);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.E, 3, 0.25, 1);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 0.25, 1);

		// Vocal
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 3, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 3, 4);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 2, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 2, 4, 8);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 3, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 3, 4);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.Asharp, 2, 3, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 3, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.Asharp, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 2);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 4.5, 6);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.E, 2, 4.5, 6);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.F, 2, 1.5, 2);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 4.5, 6);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 4.5, 6);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.Asharp, 2, 1.5, 2);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 3, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 3, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.C, 3, 3, 4);

		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.Asharp, 2, 3, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.G, 2, 1.5, 4);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.A, 2, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.D, 3, 1.5, 2);
		melodyTutorialLevelData.AddMelodyNoteAndPause(MelodyNote.Tones.E, 3, 3, 4);

		melodyTutorialLevelData.FinishComposing();

		melodyTutorialLevelData.AddAnimatedText(new AnimatedTitle()
		{
			Text = "MELODICA",
			Position = new Vector2(1280, 200),
			StartAtSec = 0.01,
			Duration = 4.5,
			FadeInSec = 0,
			FadeOutSec = 3,
			MusicPlayer = musicPlayer,
			LetterExpansionAnimation = false,
		});
		melodyTutorialLevelData.AddAnimatedText(new AnimatedTitle()
		{
			Text = "TUTORIAL",
			Position = new Vector2(1280, 200),
			StartAtSec = 4,
			Duration = 5.5,
			FadeInSec = 3,
			FadeOutSec = 3,
			MusicPlayer = musicPlayer,
			LetterExpansionAnimation = false,
		});
		melodyTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "Hit the shown arrow button when a note reaches the line!",
			Position = new Vector2(1280, 500),
			StartAtSec = 15.5,
			Duration = 10.5,
			FadeInSec = 1,
			FadeOutSec = 0.3,
		});
		melodyTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "Hold the button down while the note lasts!",
			Position = new Vector2(1600, 800),
			StartAtSec = 42,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		melodyTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "You can chain notes if they are connected.",
			Position = new Vector2(1600, 800),
			StartAtSec = 59,
			Duration = 8.5,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
		melodyTutorialLevelData.AddAnimatedText(new AnimatedLabel()
		{
			Text = "Second tutorial finished!",
			Position = new Vector2(1280, 500),
			StartAtSec = 78,
			Duration = 6,
			FadeInSec = 1,
			FadeOutSec = 2,
		});
	}

	private void GetNodes()
	{
		lBeat = GetNode<Label>("UI/Correction");
		lSmoothen = GetNode<Label>("UI/Smoothen");
		lPlayhead = GetNode<Label>("UI/Playhead");
		lSmoothPlayhead = GetNode<Label>("UI/SmoothPlayhead");
		lDifference = GetNode<Label>("UI/Difference");
	}

	private void CreateBeatNotes()
	{
		beatNotes = new List<Sprite2D>();
		PackedScene noteSprite;

		foreach (BeatNote note in loadedLevelData.BeatNotes)
		{
			switch (note.ColorType)
			{
				case BeatNote.Color.Green:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteA.tscn");
					break;
				case BeatNote.Color.Red:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteB.tscn");
					break;
				case BeatNote.Color.Blue:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteX.tscn");
					break;
				case BeatNote.Color.Yellow:
					noteSprite = GD.Load<PackedScene>("res://Scenes/NoteY.tscn");
					break;
				default:
					noteSprite = new PackedScene();
					break;
			}
			Sprite2D instance = noteSprite.Instantiate<Sprite2D>();
			instance.Position = new Vector2(1280, (float)(linePosY - note.TimeStampSec * Note.SpeedPixelPerSec));
			beatNotes.Add(instance);
			GetNode<CanvasLayer>("UI").AddChild(instance);
		}
	}

	private void CreateMelodyNotes()
	{
		melodyNotes = new List<MeshInstance2D>();
		foreach (MelodyNote note in loadedLevelData.MelodyNotes)
		{
			MeshInstance2D noteMesh = new MeshInstance2D
			{
				Mesh = new QuadMesh
				{
					Material = new StandardMaterial3D
					{
						AlbedoColor = Colors.White
					}
				},
				Scale = new Vector2(16, (float)note.LengthPixel),
				Position = new Vector2((float)note.PositionX, (float)(linePosY - note.TimeStampSec * Note.SpeedPixelPerSec + note.LengthPixel / 2))
			};
			melodyNotes.Add(noteMesh);
			GetNode<CanvasLayer>("UI").AddChild(noteMesh);
		}
	}

	private void SetupLevel()
	{
		musicPlayer = new(loadedLevelData.Music);
		AddChild(musicPlayer);

		foreach (AnimatedText text in loadedLevelData.AnimatedTexts)
		{
			GetNode("UI").AddChild(text);

			// TODO: this is a code smell
			if (text is AnimatedTitle)
			{
				((AnimatedTitle)text).MusicPlayer = musicPlayer;
			}
		}
	}
}
