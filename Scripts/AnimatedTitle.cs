using Godot;

public partial class AnimatedTitle : AnimatedText
{
    public MusicData Music { set; get; } = new MusicData();
    //public bool JumpingLetters { get; set; } = true;
    public bool LetterExpansionAnimation { get; set; } = true;

    public override void _Ready()
    {
        base._Ready();

        Name = "AnimatedTitle";

        AddChild(new TitleAnimator(Text, Music, new Vector2(0, 0), !LetterExpansionAnimation));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}