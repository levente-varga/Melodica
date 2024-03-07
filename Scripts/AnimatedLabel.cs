using Godot;

public partial class AnimatedLabel : AnimatedText
{
    private Label label;

    public AnimatedLabel()
    {

    }

    public override void _Ready()
    {
        base._Ready();

        label = new Label
        {
            Text = Text,
            Theme = ResourceLoader.Load<Theme>("res://Themes/MenuTheme.tres")
        };

        // Need to set manually, otherwise it will only take effect AFTER _Ready(), 
        // and that messes up the calculations with Size
        label.AddThemeFontSizeOverride("", Theme.DefaultFontSize);

        AddChild(label);

        SetLabelPosition();

        Name = "AnimatedLabel";
        Theme = ResourceLoader.Load<Theme>("res://Themes/MenuTheme.tres");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

    }

    private void SetLabelPosition()
    {
        Vector2 position = new Vector2();

        switch (Alignment.Horizontal)
        {
            case HorizontalAlignment.Left:
                position.X = 0;
                break;
            case HorizontalAlignment.Right:
                position.X = -label.Size.X;
                break;
            default:
                position.X = -label.Size.X / 2f;
                break;
        }

        switch (Alignment.Vertical)
        {
            case VerticalAlignment.Top:
                position.Y = 0;
                break;
            case VerticalAlignment.Bottom:
                position.Y = -label.Size.Y;
                break;
            default:
                position.Y = -label.Size.Y / 2f;
                break;
        }

        label.Position = position;
    }
}