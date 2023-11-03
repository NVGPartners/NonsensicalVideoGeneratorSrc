#if MONOGAME
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class OptionsPage : IPage
    {
        public string Name { get; set; } = "Options";
        public string Tooltip { get; } = "Change application settings.";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            controller.Draw(gameTime, spriteBatch);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Add dials
            controller.Add("VideoPlaybackScale", new TextEntry("Video Playback Resolution", "The screen scale multiplier for video playback.", SaveData.saveValues["VideoPlaybackScale"], new Vector2(139, 60+19*7), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["VideoPlaybackScale"], System.Globalization.CultureInfo.InvariantCulture);
                // Range: 1-4
                if(int.Parse(controller.interactables["VideoPlaybackScale"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 1)
                    controller.interactables["VideoPlaybackScale"].Tooltip = "1";
                if(int.Parse(controller.interactables["VideoPlaybackScale"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 4)
                    controller.interactables["VideoPlaybackScale"].Tooltip = "4";
                SaveData.saveValues["VideoPlaybackScale"] = controller.interactables["VideoPlaybackScale"].Tooltip;
                return false;
            }));
            controller.Add("MuteMusicWhileTabbedOut", new Switch("Mute Music While Inactive", "Don't play music while NVG is in the background.", new Vector2(139, 60+19*6), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["MuteMusicWhileTabbedOut"];
                    SaveData.saveValues["MuteMusicWhileTabbedOut"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["MuteMusicWhileTabbedOut"])
                        SaveData.Save();
                    if(switchState)
                        DiscordRPC.Initialize();
                    else
                        DiscordRPC.Shutdown();
                }
                return switchState;
            }, SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true"));
            controller.Add("EnableDiscordRPC", new Switch("Enable Discord RPC", "Tell others that you're using NVG.", new Vector2(139, 60+19*5), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["EnableDiscordRPC"];
                    SaveData.saveValues["EnableDiscordRPC"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["EnableDiscordRPC"])
                        SaveData.Save();
                    if(switchState)
                        DiscordRPC.Initialize();
                    else
                        DiscordRPC.Shutdown();
                }
                return switchState;
            }, SaveData.saveValues["EnableDiscordRPC"] == "true"));
            controller.Add("MotionDisable", new Switch("Disable Motion", "Turns off screen tweening and other elements.", new Vector2(139, 60+19*4), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["DisableMotion"];
                    SaveData.saveValues["DisableMotion"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["DisableMotion"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["DisableMotion"] == "true"));
            controller.Add("Scale", new TextEntry("Screen Resolution", "The screen scale multiplier for the UI. Restarts when set.", SaveData.saveValues["ScreenScale"], new Vector2(139, 60+19*3), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture);
                // Range: 1-4
                if(int.Parse(controller.interactables["Scale"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 1)
                    controller.interactables["Scale"].Tooltip = "1";
                if(int.Parse(controller.interactables["Scale"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 4)
                    controller.interactables["Scale"].Tooltip = "4";
                SaveData.saveValues["ScreenScale"] = controller.interactables["Scale"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture))
                {
                    SaveData.Save();
                    // Restart software through steam
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "steam://run/" + Global.appId.ToString();
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "open";
                    Process.Start(startInfo);
                    // Close software
                    try
                    {
                        SteamAPI.Shutdown();
                    } catch {}
                    UserInterface.instance.Exit();
                }
                return false;
            }));
            controller.Add("VideoVolume", new TextEntry("Media Playback Volume", "In-app media volume level, from 0-100.", SaveData.saveValues["VideoVolume"], new Vector2(139, 60+19*2), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["VideoVolume"];
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controller.interactables["VideoVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controller.interactables["VideoVolume"].Tooltip = "100";
                SaveData.saveValues["VideoVolume"] = controller.interactables["VideoVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("SFXVolume", new TextEntry("Sound Effect Volume", "Sound effect volume level, from 0-100.", SaveData.saveValues["SoundEffectVolume"], new Vector2(139, 60+19), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["SoundEffectVolume"];
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controller.interactables["SFXVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controller.interactables["SFXVolume"].Tooltip = "100";
                SaveData.saveValues["SoundEffectVolume"] = controller.interactables["SFXVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["SoundEffectVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MusicVolume", new TextEntry("Music Volume", "Background music volume level, from 0-100.", SaveData.saveValues["MusicVolume"], new Vector2(139, 60), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["MusicVolume"];
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controller.interactables["MusicVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controller.interactables["MusicVolume"].Tooltip = "100";
                SaveData.saveValues["MusicVolume"] = controller.interactables["MusicVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["MusicVolume"])
                    SaveData.Save();
                return false;
            }));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
