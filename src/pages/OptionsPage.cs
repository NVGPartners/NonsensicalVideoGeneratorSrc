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
            controller.Add("MotionDisable", new Switch("Disable Motion", "Turns off screen tweening and other elements.", new Vector2(139, 60+19*4), (int i) => {
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
            controller.Add("Scale", new TextEntry("Screen Resolution Multiplier", "2, 3, or 4. A restart will be performed.", SaveData.saveValues["ScreenScale"], new Vector2(139, 60+19*3), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["ScreenScale"]);
                // Range: 1-4
                if(int.Parse(controller.interactables["Scale"].Tooltip) < 2)
                    controller.interactables["Scale"].Tooltip = "2";
                if(int.Parse(controller.interactables["Scale"].Tooltip) > 4)
                    controller.interactables["Scale"].Tooltip = "4";
                SaveData.saveValues["ScreenScale"] = controller.interactables["Scale"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["ScreenScale"]))
                {
                    SaveData.Save();
                    // Restart software through steam
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "steam://rungameid/" + Global.appId.ToString();
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
            controller.Add("VideoVolume", new TextEntry("Media Playback Volume", "In-app media volume level, from 0-100.", SaveData.saveValues["VideoVolume"], new Vector2(139, 60+19*2), 24, 3, 1, (int i) => {
                string oldValue = SaveData.saveValues["VideoVolume"];
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip) < 0)
                    controller.interactables["VideoVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip) > 100)
                    controller.interactables["VideoVolume"].Tooltip = "100";
                SaveData.saveValues["VideoVolume"] = controller.interactables["VideoVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("SFXVolume", new TextEntry("Sound Effect Volume", "Sound effect volume level, from 0-100.", SaveData.saveValues["SoundEffectVolume"], new Vector2(139, 60+19), 24, 3, 1, (int i) => {
                string oldValue = SaveData.saveValues["SoundEffectVolume"];
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip) < 0)
                    controller.interactables["SFXVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip) > 100)
                    controller.interactables["SFXVolume"].Tooltip = "100";
                SaveData.saveValues["SoundEffectVolume"] = controller.interactables["SFXVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["SoundEffectVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MusicVolume", new TextEntry("Music Volume", "Background music volume level, from 0-100.", SaveData.saveValues["MusicVolume"], new Vector2(139, 60), 24, 3, 1, (int i) => {
                string oldValue = SaveData.saveValues["MusicVolume"];
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip) < 0)
                    controller.interactables["MusicVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip) > 100)
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