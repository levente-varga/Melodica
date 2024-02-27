using Godot;

public static class Settings {
    public static Vector2 screenResolution = new Vector2(
		ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(),
		ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32());
}