using Godot;
using System;

public partial class SettingsMenu : Node2D
{
	HSlider sMaster;
	HSlider sMusic;
	HSlider sSoundEffects;

	OptionButton obResolution;
	CheckButton cbVSync;
	CheckButton cbMSAA;
	CheckButton cbWindowed;
	CheckButton cbMenuAnimations;

	Label lMaster;
	Label lMusic;
	Label lSoundEffects;

	SettingsMenu()
	{
		LoadSettings();
	}

	public override void _Ready()
	{
		GetNodes();
		SubscribeToEvents();
		SetupViewport();
		UpdateUIValues();

		obResolution.Clear();
		for (int i = 0; i < Settings.Display.AllowedResolutions.Count; i++)
		{
			var resolution = Settings.Display.AllowedResolutions[i];
			obResolution.AddItem(string.Format("{0}x{1}", resolution.X, resolution.Y));
			if (resolution == Settings.Display.Resolution) obResolution.Selected = i;
		}
	}

	public override void _Process(double delta)
	{

	}

	private void SetupViewport()
	{
		// Sets MSAA based on setting
		RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), Settings.Display.MSAA
			? RenderingServer.ViewportMsaa.Msaa8X
			: RenderingServer.ViewportMsaa.Disabled);
	}

	private void GetNodes()
	{
		// Audio settings
		sMaster = GetNode<HSlider>("UI/TabContainer/AUDIO/Settings/Master/Value");
		lMaster = GetNode<Label>("UI/TabContainer/AUDIO/Settings/Master/ValueLabel");
		sMusic = GetNode<HSlider>("UI/TabContainer/AUDIO/Settings/Music/Value");
		lMusic = GetNode<Label>("UI/TabContainer/AUDIO/Settings/Music/ValueLabel");
		sSoundEffects = GetNode<HSlider>("UI/TabContainer/AUDIO/Settings/SoundEffects/Value");
		lSoundEffects = GetNode<Label>("UI/TabContainer/AUDIO/Settings/SoundEffects/ValueLabel");

		// Game settings
		cbMenuAnimations = GetNode<CheckButton>("UI/TabContainer/GAME/Settings/MenuAnimations/Value");

		// Display settings
		obResolution = GetNode<OptionButton>("UI/TabContainer/DISPLAY/Settings/ScreenResolution/Value");
		cbWindowed = GetNode<CheckButton>("UI/TabContainer/DISPLAY/Settings/Windowed/Value");
		cbVSync = GetNode<CheckButton>("UI/TabContainer/DISPLAY/Settings/VSync/Value");
		cbMSAA = GetNode<CheckButton>("UI/TabContainer/DISPLAY/Settings/MSAA/Value");
	}

	private void SubscribeToEvents()
	{
		sMaster.ValueChanged += OnMasterAudioChanged;
		sMusic.ValueChanged += OnMusicAudioChanged;
		sSoundEffects.ValueChanged += OnSoundEffectsAudioChanged;

		cbMenuAnimations.Toggled += OnMenuAnimationsEnabledChanged;

		obResolution.ItemSelected += OnResolutionChanged;
		cbWindowed.Toggled += OnWindowedEnabledChanged;
		cbMSAA.Toggled += OnMSAAEnabledChanged;
		cbVSync.Toggled += OnVSyncEnabledChanged;

		sMaster.DragEnded += OnAudioDragEnded;
		sMusic.DragEnded += OnAudioDragEnded;
		sSoundEffects.DragEnded += OnAudioDragEnded;
	}

	private void UpdateUIValues()
	{
		cbWindowed.ButtonPressed = Settings.Display.Windowed;
		cbVSync.ButtonPressed = Settings.Display.VSync;
		cbMSAA.ButtonPressed = Settings.Display.MSAA;
		cbMenuAnimations.ButtonPressed = Settings.Game.MenuAnimations;
		sMaster.Value = Settings.Audio.Master;
		sMusic.Value = Settings.Audio.Music;
		sSoundEffects.Value = Settings.Audio.SoundEffects;
		UpdateAudioValueLabels();
	}

	private void UpdateAudioValueLabels()
	{
		lMaster.Text = ((int)(Settings.Audio.Master * 100)).ToString();
		lMusic.Text = ((int)(Settings.Audio.Music * 100)).ToString();
		lSoundEffects.Text = ((int)(Settings.Audio.SoundEffects * 100)).ToString();
	}

	private void LoadSettings()
	{
		Settings.Load();
	}

	void OnResolutionChanged(long selected)
	{
		Settings.Display.Resolution = Settings.Display.AllowedResolutions[(int)selected];
		Settings.Save();
	}

	void OnMasterAudioChanged(double value)
	{
		Settings.Audio.Master = value;
		UpdateAudioValueLabels();
	}

	void OnMusicAudioChanged(double value)
	{
		Settings.Audio.Music = value;
		UpdateAudioValueLabels();
	}

	void OnSoundEffectsAudioChanged(double value)
	{
		Settings.Audio.SoundEffects = value;
		UpdateAudioValueLabels();
	}

	void OnAudioDragEnded(bool valueChanged)
	{
		if (valueChanged) Settings.Save();
	}

	private void OnWindowedEnabledChanged(bool value)
	{
		Settings.Display.Windowed = value;
		Settings.Save();
	}

	private void OnMSAAEnabledChanged(bool value)
	{
		Settings.Display.MSAA = value;
		Settings.Save();
		SetupViewport();
	}

	private void OnVSyncEnabledChanged(bool value)
	{
		Settings.Display.VSync = value;
		Settings.Save();
	}

	private void OnMenuAnimationsEnabledChanged(bool value)
	{
		Settings.Game.MenuAnimations = value;
		Settings.Save();
	}
}
