using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;

public class Settings
{
    const string path = "res://settings.cfg";
    static ConfigFile config = new ConfigFile();

    public static void Save() {
        config.Save(path);
    }

    public static void Load() {
        Error error = config.Load(path);
        //if (error != Error.Ok) return;

        Display.Resolution = config.GetValue("Display", "resolution", new Vector2(1920, 1080)).AsVector2();
        Display.Windowed = config.GetValue("Display", "windowes", true).AsBool();
        Display.MSAA = config.GetValue("Display", "msaa", true).AsBool();
        Display.VSync = config.GetValue("Display", "vsync", true).AsBool();

        Audio.Master = config.GetValue("Audio", "master", 1.0).AsDouble();
        Audio.Music = config.GetValue("Audio", "music", 1.0).AsDouble();
        Audio.SoundEffects = config.GetValue("Audio", "sound_effects", 1.0).AsDouble();

        Game.MenuAnimations = config.GetValue("Game", "menu_animations", true).AsBool();

        Debug.WriteLine("Successfully loaded Settings!");

        Save();
    }

    public struct Game {
        static bool menuAnimations;
        public static bool MenuAnimations { 
            get { return menuAnimations; } 
            set {
                menuAnimations = value;
                config.SetValue("Game", "menu_animations", value);
                Debug.WriteLine("MenuAnimations set to {0}", value);
            }
        }
    }

    public struct Display {
        public static readonly List<Vector2> AllowedResolutions = new List<Vector2>{
            new Vector2(2560, 1080),
            new Vector2(1920, 1080),
        };

        static Vector2 resolution;
        public static Vector2 Resolution { 
            get { return resolution; }
            set {
                foreach (var allowedResolution in AllowedResolutions)
                    if (allowedResolution == value) {
                        //ProjectSettings.SetSetting("display/window/size/viewport_width", value.X);
                        //ProjectSettings.SetSetting("display/window/size/viewport_height", value.Y);
                        //ProjectSettings.Save();
                        resolution = value;
                        config.SetValue("Display", "resolution", value);

                        Debug.WriteLine("Resolution set to {0}", value);
                        return;
                    }
                resolution = new Vector2(1920, 1080);
                Debug.WriteLine("Resolution set to default ({0})", resolution);
            }
        }

        static bool windowed;
        public static bool Windowed { 
            get { return windowed; }//DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed; }
            set { 
                //DisplayServer.WindowSetMode(value 
                //    ? DisplayServer.WindowMode.Windowed 
                //    : DisplayServer.WindowMode.Fullscreen, 0); 
                //ProjectSettings.SetSetting("display/window/size/mode", value ? 0 : 3); 
                //ProjectSettings.Save();
                config.SetValue("Display", "windowed", value);

                Debug.WriteLine("Windowed set to {0}", value);
            } 
        }

        static bool msaa;
        public static bool MSAA { 
            get { return msaa; }
            set {
                ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", value ? 3 : 0); 
                ProjectSettings.Save(); 
                msaa = value;
                config.SetValue("Display", "msaa", value);

                Debug.WriteLine("MSAA set to {0}", value);
            }
        }

        public static bool VSync { 
            get { return DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled; } 
            set { 
                DisplayServer.WindowSetVsyncMode(value
                    ? DisplayServer.VSyncMode.Enabled
                    : DisplayServer.VSyncMode.Disabled);
                ProjectSettings.SetSetting("display/window/vsync/vsync_mode", value); 
                ProjectSettings.Save(); 
                config.SetValue("Display", "vsync", value);
                Debug.WriteLine("VSync set to {0}", value);
            }
        }
    }

    public struct Audio {
        static double master = 1;
        public static double Master { 
            get { return master; } 
            set { 
                master = Mathf.Clamp(value, 0.0, 1.0);
                config.SetValue("Audio", "master", master);
                Debug.WriteLine("Master audio set to {0}", master);
            } 
        }
        
        static double music = 1;
        public static double Music { 
            get { return music; } 
            set { 
                music = Mathf.Clamp(value, 0.0, 1.0);
                config.SetValue("Audio", "music", music);
                Debug.WriteLine("Music audio set to {0}", music);
            } 
        }

        static double soundEffects = 1;
        public static double SoundEffects { 
            get { return soundEffects; } 
            set { 
                soundEffects = Mathf.Clamp(value, 0.0, 1.0);
                config.SetValue("Audio", "sound_effects", soundEffects);
                Debug.WriteLine("Sound effects audio set to {0}", soundEffects);
            } 
        }
    }
}