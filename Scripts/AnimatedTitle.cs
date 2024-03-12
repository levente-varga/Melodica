using Godot;

public partial class AnimatedTitle : AnimatedText
{
    private MusicPlayer musicPlayer;
    public MusicPlayer MusicPlayer
    {
        get { return musicPlayer; }
        set
        {
            musicPlayer = value;
            if (titleAnimator != null)
                titleAnimator.MusicPlayer = musicPlayer;
        }
    }

    private bool jumpingLetters = true;
    public bool JumpingLetters
    {
        get { return jumpingLetters; }
        set
        {
            jumpingLetters = value;
            if (titleAnimator != null)
                titleAnimator.JumpingLetters = jumpingLetters;
        }
    }

    private bool letterExpansionAnimation = true;
    public bool LetterExpansionAnimation
    {
        get { return letterExpansionAnimation; }
        set
        {
            letterExpansionAnimation = value;
            if (titleAnimator != null)
                titleAnimator.StartInPlace = !LetterExpansionAnimation;
        }
    }

    TitleAnimator titleAnimator;

    public override void _Ready()
    {
        base._Ready();

        Name = "AnimatedTitle";

        titleAnimator = new(Text)
        {
            Center = new Vector2(0, 0),
            StartInPlace = !LetterExpansionAnimation,
            JumpingLetters = JumpingLetters,
            MusicPlayer = MusicPlayer,
        };

        AddChild(titleAnimator);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}