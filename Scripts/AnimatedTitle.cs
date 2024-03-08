using Godot;

public partial class AnimatedTitle : AnimatedText
{
    public MusicPlayer MusicPlayer { set; get; }
    //public bool JumpingLetters { get; set; } = true;
    public bool LetterExpansionAnimation { get; set; } = true;

    public override void _Ready()
    {
        base._Ready();

        Name = "AnimatedTitle";

        AddChild(new TitleAnimator(Text, MusicPlayer, new Vector2(0, 0), !LetterExpansionAnimation));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}