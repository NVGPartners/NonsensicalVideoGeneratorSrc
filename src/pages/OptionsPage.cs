using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
            // Draw text
            spriteBatch.DrawString(GlobalGraphics.fontMunro, "Music: " + GlobalContent.GetSongTitleByIndex(int.Parse(SaveData.saveValues["ActiveMusic"])), new Vector2(GlobalGraphics.Scale(1+139), GlobalGraphics.Scale(1+51+19*6)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, "Music: " + GlobalContent.GetSongTitleByIndex(int.Parse(SaveData.saveValues["ActiveMusic"])), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(51+19*6)), Color.White);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, "By: " + GlobalContent.GetSongArtistByIndex(int.Parse(SaveData.saveValues["ActiveMusic"])), new Vector2(GlobalGraphics.Scale(1+139), GlobalGraphics.Scale(1+51+12+19*6)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, "By: " + GlobalContent.GetSongArtistByIndex(int.Parse(SaveData.saveValues["ActiveMusic"])), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(51+12+19*6)), Color.White);
            // Interactable
            controller.Draw(gameTime, spriteBatch);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Add labels
            controller.Add("DialLabel", new Label("Click and rotate dials slowly.", new Vector2(139, 60)));
            // Add switches
            controller.Add("ShuffleMusic", new Switch("Shuffle", "Shuffle the music on startup.", new Vector2(139+85, 51+19*5), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["ShuffleMusic"];
                    SaveData.saveValues["ShuffleMusic"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["ShuffleMusic"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["ShuffleMusic"] == "true"));
            controller.Add("ActiveMusic", new Dial("Music Type", "Change the current background music.", new Vector2(139, 51+19*5), int.Parse(SaveData.saveValues["ActiveMusic"]), 0, GlobalContent.GetSongCount() - 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["ActiveMusic"]);
                SaveData.saveValues["ActiveMusic"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["ActiveMusic"]))
                    SaveData.Save();
                return false;
            }));
            // Add dials
            controller.Add("BGSaturation", new Dial("Background Saturation", "Adjust color in the animated background.", new Vector2(139, 51+19*4), int.Parse(SaveData.saveValues["BackgroundSaturation"]), 0, 100, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["BackgroundSaturation"]);
                SaveData.saveValues["BackgroundSaturation"] = i.ToString();
                if(oldValue != int.Parse(SaveData.saveValues["BackgroundSaturation"]))
                    SaveData.Save();
                return false;
            }));
            controller.Add("Scale", new Dial("Scale (May cause issues)", "Screen size multiplier, requires restart.", new Vector2(139, 51+19*3), int.Parse(SaveData.saveValues["ScreenScale"]) - 1, 0, 3, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["ScreenScale"]);
                SaveData.saveValues["ScreenScale"] = (i + 1).ToString();
                if(oldValue != int.Parse(SaveData.saveValues["ScreenScale"]))
                    SaveData.Save();
                return false;
            }));
            controller.Add("SFXVolume", new Dial("Sound Effect Volume", "Sound effect volume level.", new Vector2(139, 51+19*2), int.Parse(SaveData.saveValues["SoundEffectVolume"]), 0, 100, (int i) => {
                string oldValue = SaveData.saveValues["SoundEffectVolume"];
                SaveData.saveValues["SoundEffectVolume"] = i.ToString();
                if(oldValue != SaveData.saveValues["SoundEffectVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MusicVolume", new Dial("Music Volume", "Background music volume level.", new Vector2(139, 51+19), int.Parse(SaveData.saveValues["MusicVolume"]), 0, 100, (int i) => {
                string oldValue = SaveData.saveValues["MusicVolume"];
                SaveData.saveValues["MusicVolume"] = i.ToString();
                if(oldValue != SaveData.saveValues["MusicVolume"])
                    SaveData.Save();
                return false;
            }));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}