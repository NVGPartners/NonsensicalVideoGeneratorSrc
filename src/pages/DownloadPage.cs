using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class DownloadPage : IPage
    {
        public string Name { get; set; } = "Download";
        public string Tooltip { get; } = "Download media from external sources.";
        private readonly InteractableController controller = new();
        private int currentType = 0;
        private int previousTypeCount = 0;
        private List<string> names = new();
        private string clipUrl = "";
        private bool downloading = false;
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(!downloading)
            {
                // Interactable
                if(controller.Update(gameTime, handleInput))
                    return true;
                if(previousTypeCount != LibraryData.libraryFileTypes.Count)
                {
                    previousTypeCount = LibraryData.libraryFileTypes.Count;
                    controller.Remove("TypeDial");
                    names.Clear();
                    foreach(KeyValuePair<LibraryType, string> fileType in LibraryData.libraryNames)
                    {
                        names.Add(fileType.Value);
                    }
                    // Remove first three (invalid) names
                    names.RemoveRange(0, 3);
                    if(names.Count > 0)
                    {
                        currentType = 0;
                        controller.Add("TypeDial", new Dial(names[0], "Current library type to download.", new Vector2(208, (51+19*2)), currentType, 0, names.Count - 1, (int i) => {
                            int oldValue = currentType;
                            currentType = i;
                            controller.interactables["TypeDial"].Name = names[currentType];
                            return false;
                        }));
                    }
                    else
                    {
                        currentType = -1;
                    }
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(downloading)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                // Draw text to indicate that rendering is in progress
                SpriteFont font = GlobalContent.GetFont("Munro");
                string text = "Downloading clip...";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, text, new Vector2(GlobalGraphics.Scale(1) + GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(1) + GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.Black);
                spriteBatch.DrawString(font, text, new Vector2(GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.White);
            }
            else
            {
                // Interactable
                controller.Draw(gameTime, spriteBatch);
            }
        }
        public void Done(bool success)
        {
            downloading = false;
            if(success)
            {
                controller.interactables["DownloadClipUrl"].Tooltip = "";
                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Downloaded clip.";
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Failed to download clip.";
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Add buttons
            controller.Add("DownloadClip", new Button("Download Clip", "", new Vector2(134+36, (51+19*2)+10), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        // Get library type from currentType
                        foreach(KeyValuePair<LibraryType, string> fileType in LibraryData.libraryNames)
                        {
                            if(fileType.Value == names[currentType])
                            {
                                Global.generatorFactory.progressText = "Downloading clip...";
                                downloading = true;
                                if(!LibraryData.DownloadClip(clipUrl, fileType.Key, Done))
                                {
                                    Done(false);
                                }
                                break;
                            }
                        }
                        return true;
                }
                return false;
            }));
            controller.Add("DownloadClipUrl", new TextEntry("Clip URL", "", clipUrl, new Vector2(139, 51+19*1), GlobalGraphics.width, 999, 6, (int i) => {
                string oldValue = clipUrl;
                clipUrl = controller.interactables["DownloadClipUrl"].Tooltip;
                return false;
            }));
            controller.Add("Label", new Label("Type in a url to download:", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}