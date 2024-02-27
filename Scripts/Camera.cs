using Godot;
using System;

public partial class Camera : Camera2D
{
    private Vector2 _desiredPosition;
    private float _desiredRotation;
    private Vector2 _desiredZoom;
	private float _desiredLerpStrength = 10f;
    private float _lerpStrength = 10f;

	public Vector2 DesiredPosition {
		set { _desiredPosition = value; }
		get { return _desiredPosition; }
	}

	public float DesiredRotation {
		set { _desiredRotation = value; }
		get { return _desiredRotation; }
	}

	public float DesiredRotationInRadian {
		set { _desiredRotation = value * 180.0f / MathF.PI; }
		get { return _desiredRotation * MathF.PI / 180.0f; }
	}

	public Vector2 DesiredZom {
		set { _desiredZoom = value; }
		get { return _desiredZoom; }
	}

    public override void _Ready()
    {
        Reset();
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(_desiredPosition, (float)delta * _lerpStrength);
        Zoom = Zoom.Lerp(_desiredZoom, (float)delta * _lerpStrength);
        Rotation = Mathf.Lerp(Rotation, DesiredRotationInRadian, (float)delta * _lerpStrength);
        _lerpStrength = Mathf.Lerp(_lerpStrength, _desiredLerpStrength, (float)delta * 0.01f);
    }

    private void Reset()
    {
        _desiredPosition = new Vector2(0, 0);
        _desiredZoom = new Vector2(1, 1);
        _desiredRotation = 0;
    }
}
