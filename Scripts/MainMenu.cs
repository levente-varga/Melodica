using Godot;
using System;

public partial class MainMenu : Node2D
{
	private Button _levelSelectButton;
	private Button _settingsButton;
	private Button _quitButton;
	private Button _settingsBackButton;
	private Button _levelSelectBackButton;
	private Camera _camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_levelSelectButton = GetNode<Button>("MainMenu/UI/LevelSelectButton");
		_settingsButton = GetNode<Button>("MainMenu/UI/SettingsButton");
		_quitButton = GetNode<Button>("MainMenu/UI/QuitButton");
		_settingsBackButton = GetNode<Button>("Settings/UI/BackButton");
		_levelSelectBackButton = GetNode<Button>("Play/UI/BackButton");
		_camera = GetNode<Camera>("Camera");

		_levelSelectButton.Pressed += OnLevelSelectButtonPressed;
		_settingsButton.Pressed += OnSettingsButtonPressed;
		_quitButton.Pressed += OnQuitButtonPressed;
		_settingsBackButton.Pressed += OnBackButtonPressed;
		_levelSelectBackButton.Pressed += OnBackButtonPressed;

		AddChild(new TitleAnimator("MainMenu/UI/", "MELODICA"));
		AddChild(new TitleAnimator("Settings/UI/", "SETTINGS"));
		AddChild(new TitleAnimator("Play/UI/", "PLAY"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnLevelSelectButtonPressed() {
		_camera.DesiredPosition = new Vector2(2560, 0);
		_camera.DesiredRotation = 0;
	}

	private void OnSettingsButtonPressed() {
		_camera.DesiredPosition = new Vector2(-2560, 0);
		_camera.DesiredRotation = 0;
	}

	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}

	private void OnBackButtonPressed() {
		_camera.DesiredPosition = new Vector2(0, 0);
		_camera.DesiredRotation = 0;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
			GetTree().Quit();
	}
}
