using Godot;
using System;

public partial class PlayMenu : Node2D
{
	Button bPlay;

	public override void _Ready()
	{
		GetNodes();
		SubscribeToEvents();
	}

	private void GetNodes() {
		bPlay = GetNode<Button>("UI/bQuickPlay");
	}

	private void SubscribeToEvents() {
		bPlay.Pressed += OnPlayPressed;
	}

	public override void _Process(double delta)
	{
	}

	void OnPlayPressed() {
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
}
