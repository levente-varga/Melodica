using Godot;

public static class Constants
{

}

public static class Colors
{
    public static Color Yellow { get; } = new Color(1, 0.8f, 0.33f, 1);
    public static Color White { get; } = new Color(1, 1, 1, 1);
    public static Color Black { get; } = new Color(0, 0, 0, 1);

    public static Color ButtonGreen { get; } = new Color(0.19f, 0.73f, 0.24f, 1f);
    public static Color ButtonBlue { get; } = new Color(0.14f, 0.42f, 0.96f, 1f);
    public static Color ButtonRed { get; } = new Color(0.94f, 0.14f, 0.14f, 1f);
    public static Color ButtonYellow { get; } = new Color(1f, 0.78f, 0f, 1f);
}

public static class Musics
{
    public static MusicData PulseOfDrakness = new()
    {
        Title = "Pulse of Darkness",
        Artist = "Leblanc",
        BPM = 122,
        OffsetSec = 0,
        FilePath = "res://Music/Pulse of Darkness.mp3"
    };
    public static MusicData BlueParrot = new()
    {
        Title = "Blue Parrot",
        Artist = "Romain Garcia",
        BPM = 123,
        OffsetSec = 0,
        FilePath = "res://Music/Blue Parrot.mp3"
    };
    public static MusicData StarSky = new()
    {
        Title = "Star Sky",
        Artist = "Two Steps from Hell",
        BPM = 130,
        OffsetSec = 0,
        FilePath = "res://Music/Star Sky.mp3"
    };
}