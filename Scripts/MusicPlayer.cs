using System.Diagnostics;
using Godot;

public partial class MusicPlayer : AudioStreamPlayer
{
	private MusicData? musicData;
	public MusicData? MusicData
	{
		get { return musicData; }
		set
		{
			musicData = value;
			Stream = GD.Load<AudioStream>(MusicData?.FilePath);
		}
	}
	const float volumeFactor = 0.2f;
	const double correctionFactor = 30;
	double smoothPlayhead = 0;
	double remainingCorrection = 0;

	public event OnBeatEventHandler OnBeat;
	public delegate void OnBeatEventHandler(int beat);

	public double Beat { get; private set; } = 0;

	public MusicPlayer(MusicData musicData)
	{
		MusicData = musicData;
	}

	public override void _Ready()
	{
		base._Ready();

		AdjustVolume();
		Stream = GD.Load<AudioStream>(MusicData?.FilePath);

		Timer correctionTimer = new()
		{
			WaitTime = 0.5f,
			OneShot = false,
			Paused = true
		};
		correctionTimer.Timeout += CalculatCorrection;
		AddChild(correctionTimer);

		Play();
		correctionTimer.Paused = false;
		correctionTimer.Start();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (MusicData == null) return;

		double previousBeat = Beat;
		Beat = (smoothPlayhead - MusicData?.OffsetSec) * (MusicData?.BPM / 60f) ?? 0;
		if ((int)previousBeat < (int)Beat || (previousBeat == 0 && Beat > 0))
			OnBeat?.Invoke((int)Beat);

		AdjustVolume();
		ApplyCorrection(delta);
	}

	private void ApplyCorrection(double delta)
	{
		double correction = remainingCorrection / correctionFactor;
		smoothPlayhead += delta + correction;
		remainingCorrection -= correction;
	}

	private void CalculatCorrection()
	{
		double playhead = GetPlaybackPosition();
		double difference = playhead - smoothPlayhead;
		remainingCorrection += difference;
	}

	private void AdjustVolume()
	{
		float masterFactor = Mathf.Pow((float)Settings.Audio.Master, volumeFactor);
		float musicFactor = Mathf.Pow((float)Settings.Audio.Music, volumeFactor);
		VolumeDb = 100 * masterFactor * musicFactor - 100;
	}

	public float GetSmoothPlaybackPosition()
	{
		return (float)smoothPlayhead;
	}
}
