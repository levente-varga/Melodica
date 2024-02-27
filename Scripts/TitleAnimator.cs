using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class TitleAnimator : Control
{
	const float delaySec = 2;
	bool animationEnabled = false;
	List<Sprite2D> letters;
	List<Vector2> desiredPositions;
	const int letterWidth = 150;
	const int letterGap = 50;
	float horizontalLerpStrength = 0;
	const float desiredLerpStrength = 3;
	List<float> verticalAccelerations;
	int currentLetter = 1;
	double bpm = 122;

	// Text to animate
	string pathToText;
	string text;

	Timer metronome;
	Timer delay;

	AudioStreamPlayer musicPlayer;

	Vector2 center = new Vector2(Settings.screenResolution.X / 2, 200);
	public Vector2 Center {
		get { return center; }
		set {
			center = new Vector2(
				Mathf.Max(0, Mathf.Min(Settings.screenResolution.X, value.X)),
				Mathf.Max(0, Mathf.Min(Settings.screenResolution.Y, value.Y)));
		}
	}

	public TitleAnimator(string pathToText, string text) {
		this.pathToText = pathToText;
		this.text = text;
	}

	private void LoadLetterSprites(string pathToText, string text) {
		Dictionary<char, int> foundLetters = new Dictionary<char, int>();
		for (char c = 'A'; c <= 'Z'; c++) foundLetters.Add(c, 0);

		letters = new List<Sprite2D>();
		foreach (char letter in text) {
			int count = ++foundLetters[letter];
			letters.Add(GetParent().GetNode<Sprite2D>(pathToText + letter.ToString() + (count > 1 ? count.ToString() : "")));
		}
	}

	private void CalculateDesiredPositions() {
		desiredPositions = new List<Vector2>();
		int totalWidth = letters.Count * letterWidth + (letters.Count - 1) * letterGap;
		int leftmostPositionX = (int)center.X - totalWidth / 2 + letterWidth / 2;
		for (int i = 0; i < letters.Count; i++) {
			int desiredPositionX = leftmostPositionX + i * (letterWidth + letterGap);
			desiredPositions.Add(new Vector2(desiredPositionX, center.Y));
		}
	}

	public override void _Ready()
	{
		LoadLetterSprites(pathToText, text.ToUpper());
		CalculateDesiredPositions();

		verticalAccelerations = new List<float>();
		foreach (var _ in letters) verticalAccelerations.Add(0);

        delay = new Timer { 
			OneShot = true, 
			WaitTime = delaySec,
			Autostart = true, 
		};
		delay.Timeout += StartAnimation;
		AddChild(delay);
	}

	public override void _Process(double delta)
	{
		if (animationEnabled) {
			AnimateLetters(delta);
		}
	}

	private void StartAnimation() {
		animationEnabled = true;
		metronome = new Timer { 
			OneShot = false, 
			Autostart = true, 
			WaitTime = 60.0 / bpm
		};
		metronome.Timeout += OnMetronomeTick;
		AddChild(metronome);
	}

	private void OnMetronomeTick() {
		if (currentLetter < 0 || currentLetter >= letters.Count) currentLetter = 0;
		if (letters.Count == 0) return;
		verticalAccelerations[currentLetter] = -20;
		currentLetter++;
	}

	private void AnimateLetters(double delta) {
		for (int i = 0; i < letters.Count; i++) {
			Sprite2D letter = letters[i];
			Vector2 desiredPosition = desiredPositions[i];

			letter.Position = new Vector2(
				Mathf.Lerp(letter.Position.X, desiredPosition.X, (float)delta * horizontalLerpStrength), 
				Mathf.Lerp(letter.Position.Y + verticalAccelerations[i], desiredPosition.Y, (float)delta * desiredLerpStrength * 4));

			horizontalLerpStrength = Mathf.Lerp(horizontalLerpStrength, desiredLerpStrength, (float)delta * 0.7f);
			verticalAccelerations[i] = Mathf.Lerp(verticalAccelerations[i], 0, (float)delta * 0.7f);
		}
	}
}
