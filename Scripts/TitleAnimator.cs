using Godot;
using System.Collections.Generic;

public partial class TitleAnimator : Control
{
	MusicPlayer musicPlayer;

	const int letterWidth = 150;
	const int letterGap = 50;
	const float placementAnimationDelaySec = 2;
	const float desiredLerpStrength = 3;


	bool animationEnabled = false;
	List<Sprite2D> letters;
	List<Vector2> desiredPositions;

	float horizontalLerpStrength = 0;
	List<float> verticalOffset;
	int currentLetter = 0;
	int beatSkips = 0;
	int beatSkipsLeft;
	bool startInPlace = false;

	string title;

	Timer metronome;
	Timer delay;

	Vector2 center = new Vector2(Settings.Display.Resolution.X / 2, 200);
	public Vector2 Center
	{
		get { return center; }
		set
		{
			center = new Vector2(
				Mathf.Clamp(value.X, 0, Settings.Display.Resolution.X),
				Mathf.Clamp(value.Y, 0, Settings.Display.Resolution.Y));
		}
	}

	public TitleAnimator(string title, MusicPlayer musicPlayer, Vector2 center, bool startInPlace = false)
	{
		this.title = title;
		this.center = center;
		this.startInPlace = startInPlace;
		this.musicPlayer = musicPlayer;
	}

	private void CreateLetterSprites()
	{
		letters = new List<Sprite2D>();
		foreach (char letter in title.ToUpper())
		{
			Sprite2D letterSprite = new Sprite2D
			{
				Texture = ImageTexture.CreateFromImage(
					Image.LoadFromFile("res://Assets/Images/Letters/" + letter + ".png")
				),
				Position = center
			};
			letters.Add(letterSprite);
			AddChild(letterSprite);
		}
	}

	private void CalculateDesiredPositions()
	{
		desiredPositions = new List<Vector2>();
		int totalWidth = letters.Count * letterWidth + (letters.Count - 1) * letterGap;
		int leftmostPositionX = (int)center.X - totalWidth / 2 + letterWidth / 2;
		for (int i = 0; i < letters.Count; i++)
		{
			int desiredPositionX = leftmostPositionX + i * (letterWidth + letterGap);
			desiredPositions.Add(new Vector2(desiredPositionX, center.Y));
		}
	}

	public override void _Ready()
	{
		CreateLetterSprites();
		CalculateDesiredPositions();

		verticalOffset = new List<float>();
		foreach (var _ in letters) verticalOffset.Add(0);

		if (!Settings.Game.MenuAnimations || startInPlace) PutLettersToDesiredPositions();

		beatSkips = (4 - letters.Count % 4) % 4;
		beatSkipsLeft = beatSkips;

		if (startInPlace) StartAnimation();
		else
		{
			delay = new Timer
			{
				OneShot = true,
				WaitTime = placementAnimationDelaySec,
				Autostart = true,
			};
			delay.Timeout += StartAnimation;
			AddChild(delay);
		}
	}

	public override void _Process(double delta)
	{
		if (!Settings.Game.MenuAnimations)
		{
			PutLettersToDesiredPositions();
		}
		else if (animationEnabled)
		{
			AnimateLetters(delta);
		}
	}

	private void PutLettersToDesiredPositions()
	{
		horizontalLerpStrength = desiredLerpStrength;
		for (int i = 0; i < letters.Count; i++)
			letters[i].Position = desiredPositions[i];
	}

	private void StartAnimation()
	{
		animationEnabled = true;
		metronome = new Timer
		{
			OneShot = false,
			Autostart = true,
			WaitTime = 60.0 / musicPlayer.MusicData.BPM
		};
		musicPlayer.OnBeat += OnBeat;
		AddChild(metronome);
	}

	private void OnBeat(int beat)
	{
		if (letters.Count == 0) return;
		if (currentLetter < 0) currentLetter = 0;
		else if (currentLetter >= letters.Count)
		{
			if (beatSkipsLeft-- > 0)
			{
				return;
			}
			else
			{
				beatSkipsLeft = beatSkips;
				currentLetter = 0;
			}
		}
		if (Settings.Game.MenuAnimations)
			verticalOffset[currentLetter] = -30 * horizontalLerpStrength;
		currentLetter++;
	}

	private void AnimateLetters(double delta)
	{
		for (int i = 0; i < letters.Count; i++)
		{
			Sprite2D letter = letters[i];
			Vector2 desiredPosition = desiredPositions[i];

			letter.Position = new Vector2(
				Mathf.Lerp(letter.Position.X, desiredPosition.X, (float)delta * horizontalLerpStrength),
				Mathf.Lerp(letter.Position.Y, desiredPosition.Y + verticalOffset[i], (float)delta * desiredLerpStrength * 4));

			horizontalLerpStrength = Mathf.Lerp(horizontalLerpStrength, desiredLerpStrength, (float)delta * 0.2f);
			verticalOffset[i] = Mathf.Lerp(verticalOffset[i], 0, (float)delta * 0.7f);
		}
	}
}
