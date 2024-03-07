using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Game : Node2D
{
	LevelData levelData;

	AudioStreamPlayer musicPlayer;

	Label label;

	const int linePosY = 860;
	List<Sprite2D> notes;
	List<Sprite2D> test;
	const double noteSpeedPixelPerSec = 256;

	public override void _Ready()
	{
		GetNodes();

		levelData = new LevelData(new MusicData
		{
			Title = "Blue Parrot",
			Composer = "Romain Garcia",
			BPM = 123,
			OffsetSec = 0,
		});
		levelData.StartComposing();
		levelData.AddPause(64);
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

		CreateNotes();

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

	float CalculateScale(float startScale, float desiredScale, float timeElapsed, float timestamp)
	{
		float growthFactor = Mathf.Pow(desiredScale / startScale, 1.0f / timestamp);
		return startScale * Mathf.Pow(growthFactor, timeElapsed);
	}

	float CalculatePosition(float startPosition, float linePosition, float timeElapsed, float timestamp, float accelerationFactor)
	{
		float distance = linePosition - startPosition;
		float speedFactor = distance / Mathf.Pow(timestamp, accelerationFactor);
		return startPosition + speedFactor * Mathf.Pow(timeElapsed, accelerationFactor);
	}

	float CalculateScale2(float startScale, float desiredScale, float distanceFactor)
	{
		return desiredScale * MathF.Pow(distanceFactor, 1.2f);
	}

	float CalculatePosition2(float startPosition, float linePosition, float timeElapsed, float timestamp)
	{
		float totalDistance = linePosition - startPosition;
		float timeLeft = timestamp - timeElapsed;
		return startPosition + Mathf.Pow(timeElapsed - timestamp, 2) * totalDistance;
		float speedFactor = totalDistance / Mathf.Pow(timestamp, 2);
		return startPosition + speedFactor * Mathf.Pow(timeElapsed, 2);
	}


	double lastPlayhead = 0;
	double lastMusicPosition = 0;
	double musicPosition = 0;
	bool smoothen = true;
	double correction = 0;
	double correctionFactor = 3;
	bool debug = true;
	public override void _Process(double delta)
	{
		double playhead = musicPlayer.GetPlaybackPosition();

		lastMusicPosition = musicPosition;
		if (smoothen)
		{
			musicPosition += delta;
		}

		if (playhead != lastPlayhead)
		{
			double difference = playhead - musicPosition;
			correction += difference / correctionFactor;

			if (!smoothen)
			{
				musicPosition = playhead;
			}

			lastPlayhead = playhead;
		}

		double currentCorrection = correction / correctionFactor;
		correction -= currentCorrection;

		if (smoothen)
		{
			musicPosition += currentCorrection;
		}

		MoveNotes(delta, correction * (smoothen ? 1 : 0));
		label.Text = string.Format("Smoothen: {0}, Correction: {1}{2:F5}", smoothen ? "true" : "false", correction < 0 ? "-" : " ", Mathf.Abs(correction));
		HandleInput();
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

		if (accuracy != Note.Accuracy.None)
		{
			string text = "";
			switch (accuracy)
			{
				case Note.Accuracy.Perfect: text = "Perfect"; break;
				case Note.Accuracy.Good: text = "Good"; break;
				case Note.Accuracy.Acceptable: text = "OK"; break;
				default: text = "?"; break;
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
				FadeOutSec = 0.5,
			});
		}
	}

	Note.Accuracy TryToFireBeatNote(Note.Color color)
	{
		if (levelData.Notes.Count == 0) return Note.Accuracy.None;

		float playhead = musicPlayer.GetPlaybackPosition();
		Note candidate = null;
		double minDistance = 0;

		foreach (Note note in levelData.Notes)
		{
			if (note.Fired || note.ColorType != color) continue;
			if (candidate == null || note.GetDistance(playhead) < minDistance)
			{
				minDistance = note.GetDistance(playhead);
				candidate = note;
			}
		}

		GetNode("UI").AddChild(new AnimatedLabel()
		{
			Text = $"{(candidate == null ? "No candidate" : $"{minDistance:F4}")}",
			Font = "res://Fonts/SourceCodePro-Light.ttf",
			Alignment = new AnimatedText.TextAlignment() { Horizontal = HorizontalAlignment.Right },
			Position = new Vector2(1000, 800),
			Velocity = new Vector2(0, -60),
			StartAtSec = 0.001,
			Duration = 0.2,
			FadeInSec = 0,
			FadeOutSec = 0.6,
		});

		if (candidate == null) return Note.Accuracy.None;
		return candidate.Fire(musicPlayer.GetPlaybackPosition());
	}

	private void MoveNotes(double delta, double correction)
	{
		float playhead = musicPlayer.GetPlaybackPosition();

		for (int i = 0; i < notes.Count; i++)
		{
			notes[i].Position += new Vector2(0, (float)(delta * noteSpeedPixelPerSec));
			test[i].Position = new Vector2(1400, linePosY - (float)((levelData.Notes[i].TimeStampSec - (playhead - correction)) * noteSpeedPixelPerSec));
			continue;


			float distance = playhead - (float)levelData.Notes[i].TimeStampSec;
			float A = 900, B = 3.5f;
			notes[i].Position = new Vector2(
				notes[i].Position.X,
				linePosY + Mathf.Pow(distance + B, 5) + (distance + B) * 100f - A
			);
			notes[i].Scale = new Vector2(
				-Mathf.Pow(distance - B, 2) / 4,
				-Mathf.Pow(distance - B, 2) / 4
			);


			float startScale = 0.01f; // Initial scale when far away
			float desiredScale = 0.2f; // Scale when crossing the line
			float startPosition = 300; // Initial Y position

			notes[i].Position = new Vector2(notes[i].Position.X, CalculatePosition(startPosition, linePosY, playhead, (float)levelData.Notes[i].TimeStampSec, 20));
			float scale = CalculateScale2(startScale, desiredScale, (notes[i].Position.Y - startPosition) / (linePosY - startPosition));
			notes[i].Scale = new Vector2(scale, scale);
			if (i == 0) label.Text = notes[0].Position.ToString();
		}
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

	void GetNodes()
	{
		musicPlayer = GetNode<AudioStreamPlayer>("musicPlayer");
		label = GetNode<Label>("UI/Label");
	}

	void CreateNotes()
	{
		notes = new List<Sprite2D>();
		test = new List<Sprite2D>();
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
			Sprite2D instance2 = noteSprite.Instantiate<Sprite2D>();
			instance.Position = new Vector2(1280, (float)(linePosY - note.TimeStampSec * noteSpeedPixelPerSec));
			notes.Add(instance);
			test.Add(instance2);
			GetNode<CanvasLayer>("UI").AddChild(instance);
			GetNode<CanvasLayer>("UI").AddChild(instance2);
		}
	}

	void ShowAccuracyTest(Note.Accuracy accuracy)
	{
		switch (accuracy)
		{
			case Note.Accuracy.Perfect:
				break;
		}
	}

	void QueueText(string text, Vector2 position, double timeStamp, double duration, double fadeInDuration = 1, double fadeOutDuration = 1)
	{
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

		timer.Timeout += () =>
		{
			switch (phase)
			{
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

	enum AnimationPhase
	{
		Waiting,
		FadeIn,
		Duration,
		FadeOut,
	}

	void QueueTitle(string text, Vector2 position, double timeStamp, double duration, double fadeInDuration = 1, double fadeOutDuration = 1)
	{
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

		timer.Timeout += () =>
		{
			switch (phase)
			{
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
