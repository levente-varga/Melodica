using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer
{
	public override void _Ready()
	{
		AdjustVolume();
		Play();
	}

	public override void _Process(double delta)
	{
		AdjustVolume();
	}

	private void AdjustVolume() {
		float masterFactor = Mathf.Pow((float)Settings.Audio.Master, 0.2f);
		float musicFactor = Mathf.Pow((float)Settings.Audio.Music, 0.2f);
		VolumeDb = 100 * masterFactor * musicFactor - 100;
	}
}
