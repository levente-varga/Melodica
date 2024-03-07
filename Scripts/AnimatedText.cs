using System.Diagnostics;
using Godot;

public abstract partial class AnimatedText : Control
{
    public string Text { set; get; } = "";
    public Vector2 Velocity { set; get; } = new Vector2(0, 0);
    public Color Color { set; get; } = new Color(1, 1, 1, 1);
    public double Duration { set; get; } = 0;
    public double StartAtSec { set; get; } = 0;
    public double FadeInSec { set; get; } = 0;
    public double FadeOutSec { set; get; } = 0;
    public TextAlignment Alignment { set; get; } = new TextAlignment() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center };

    AnimationPhase animationPhase = AnimationPhase.Waiting;

    public struct TextAlignment
    {
        public HorizontalAlignment Horizontal { get; set; } = HorizontalAlignment.Center;
        public VerticalAlignment Vertical { get; set; } = VerticalAlignment.Center;
        public TextAlignment() { }
    }

    protected enum AnimationPhase
    {
        Waiting,
        FadeIn,
        Duration,
        FadeOut,
    }

    public delegate void PhaseEventHandler();

    public event PhaseEventHandler OnFadeInStart;
    public event PhaseEventHandler OnFadeInEnd;
    public event PhaseEventHandler OnFadeOutStart;
    public event PhaseEventHandler OnFadeOutEnd;

    public AnimatedText()
    {

    }

    public override void _Ready()
    {
        base._Ready();

        Name = "AnimatedText";
        Size = new Vector2(0, 0);
        Modulate = new Color(Color.R, Color.G, Color.B, 0);
        Theme = ResourceLoader.Load<Theme>("res://Themes/MenuTheme.tres");

        // Need to set manually, otherwise it will only take effect AFTER _Ready(), 
        // and that messes up the calculations with Size
        AddThemeFontSizeOverride("", Theme.DefaultFontSize);

        Timer timer = new Timer();
        AddChild(timer);
        timer.Start(StartAtSec);

        timer.Timeout += () =>
        {
            switch (animationPhase)
            {
                case AnimationPhase.Waiting:
                    OnFadeInStart?.Invoke();
                    animationPhase = AnimationPhase.FadeIn;
                    timer.Start(FadeInSec);
                    Tween fadeInTween = CreateTween();
                    fadeInTween.TweenProperty(this, "modulate", Color, FadeInSec);
                    break;
                case AnimationPhase.FadeIn:
                    OnFadeInEnd?.Invoke();
                    animationPhase = AnimationPhase.Duration;
                    timer.Start(Duration);
                    break;
                case AnimationPhase.Duration:
                    OnFadeOutStart?.Invoke();
                    animationPhase = AnimationPhase.FadeOut;
                    timer.Start(FadeOutSec);
                    Tween fadeOutTween = CreateTween();
                    fadeOutTween.TweenProperty(this, "modulate", new Color(Color.R, Color.G, Color.B, 0), FadeOutSec);
                    break;
                case AnimationPhase.FadeOut:
                    OnFadeInEnd?.Invoke();
                    timer.Dispose();
                    Dispose();
                    break;
            }
        };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (animationPhase != AnimationPhase.Waiting)
        {
            Position += Velocity * (float)delta;
        }
    }
}