using Godot;
using System;

public partial class MainMenu : Node2D
{
	private Button bPlay;
	private Button bSettings;
	private Button bQuit;
	private Button bSettingsBack;
	private Button bPlayBack;
	private Button bLibrary;
	private Button bLibraryBack;
	private Camera camera;
	private MusicPlayer musicPlayer;



	MusicData music = new MusicData
	{
		Title = "",
		Composer = "",
		BPM = 122,
		OffsetSec = 0
	};

	public override void _Ready()
	{
		GetNodes();
		SubscribeToEvents();
		SetupTitleAnimators();
	}

	public override void _Process(double delta)
	{
	}

	private void GetNodes()
	{
		bPlay = GetNode<Button>("MainMenu/UI/bPlay");
		bSettings = GetNode<Button>("MainMenu/UI/bSettings");
		bQuit = GetNode<Button>("MainMenu/UI/bQuit");
		bLibrary = GetNode<Button>("Play/UI/bLibrary");

		bSettingsBack = GetNode<Button>("Settings/UI/bBack");
		bPlayBack = GetNode<Button>("Play/UI/bBack");
		bLibraryBack = GetNode<Button>("Library/UI/bBack");

		camera = GetNode<Camera>("Camera");

		// Other
		musicPlayer = new(Musics.PulseOfDrakness);
		AddChild(musicPlayer);
	}

	private void SubscribeToEvents()
	{
		bPlay.Pressed += OnLevelSelectButtonPressed;
		bSettings.Pressed += OnSettingsButtonPressed;
		bQuit.Pressed += OnQuitButtonPressed;
		bSettingsBack.Pressed += OnBackButtonPressed;
		bPlayBack.Pressed += OnBackButtonPressed;
		bLibraryBack.Pressed += OnLibraryBackButtonPressed;
		bLibrary.Pressed += OnLibraryButtonPressed;
	}

	private void SetupTitleAnimators()
	{
		GetNode("MainMenu/UI").AddChild(new TitleAnimator("MELODICA", musicPlayer, new Vector2(1280, 200)));
		GetNode("Settings/UI").AddChild(new TitleAnimator("SETTINGS", musicPlayer, new Vector2(1280, 200)));
		GetNode("Play/UI").AddChild(new TitleAnimator("PLAY", musicPlayer, new Vector2(1280, 200)));
		GetNode("Library/UI").AddChild(new TitleAnimator("LIBRARY", musicPlayer, new Vector2(1280, 200)));
	}

	private void OnLevelSelectButtonPressed()
	{
		camera.DesiredPosition = new Vector2(2560, 0);
		camera.DesiredRotation = 0;
	}

	private void OnSettingsButtonPressed()
	{
		camera.DesiredPosition = new Vector2(-2560, 0);
		camera.DesiredRotation = 0;
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	private void OnLibraryButtonPressed()
	{
		camera.DesiredPosition = new Vector2(2560, 1080);
		camera.DesiredRotation = 0;
	}

	private void OnBackButtonPressed()
	{
		camera.DesiredPosition = new Vector2(0, 0);
		camera.DesiredRotation = 0;
	}

	private void OnLibraryBackButtonPressed()
	{
		camera.DesiredPosition = new Vector2(2560, 0);
		camera.DesiredRotation = 0;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
			GetTree().Quit();
	}
}
