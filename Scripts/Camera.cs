using Godot;
using System;

public partial class Camera : Camera2D
{
    private Vector2 desiredPosition;
    private float desiredRotation;
    private Vector2 desiredZoom;
	private float desiredLerpStrength = 10f;
    private float lerpStrength = 10f;

	public Vector2 DesiredPosition {
		set { desiredPosition = value; }
		get { return desiredPosition; }
	}

	public float DesiredRotation {
		set { desiredRotation = value; }
		get { return desiredRotation; }
	}

	public float DesiredRotationInRadian {
		set { desiredRotation = value * 180.0f / MathF.PI; }
		get { return desiredRotation * MathF.PI / 180.0f; }
	}

	public Vector2 DesiredZoom {
		set { desiredZoom = value; }
		get { return desiredZoom; }
	}

    public override void _Ready()
    {
        Reset();
    }

    public override void _Process(double delta)
    {
        if (Settings.Game.MenuAnimations) {
            Position = Position.Lerp(desiredPosition, (float)delta * lerpStrength);
            Zoom = Zoom.Lerp(desiredZoom, (float)delta * lerpStrength);
            Rotation = Mathf.Lerp(Rotation, DesiredRotationInRadian, (float)delta * lerpStrength);
            lerpStrength = Mathf.Lerp(lerpStrength, desiredLerpStrength, (float)delta * 0.01f);
        }
        else {
            Position = DesiredPosition;
            Rotation = DesiredRotation;
            Zoom = DesiredZoom;
        }
    }

    private void Reset()
    {
        desiredPosition = new Vector2(0, 0);
        desiredZoom = new Vector2(1, 1);
        desiredRotation = 0;
    }
}
