using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// News page.
    /// </summary>
    public class BlogPage : IPage
    {
        public string Name { get; set; } = "PageBlog";
        public string Tooltip { get; } = "Check out what's new!";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            if(handleInput && SaveData.saveValues["LastVersion"] != BlogData.LastVersion)
            {
                Pagination.SetPage(5);
                SaveData.saveValues["LastVersion"] = BlogData.LastVersion;
                SaveData.Save();
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            controller.Draw(gameTime, spriteBatch);
            // Banner (160x 81y)
            Texture2D banner = GlobalContent.GetTexture("BlogBanner");
            SpriteFont font = L.FontLarge();
            // Draw title
            Vector2 titleSize = font.MeasureString(BlogData.Title);
            spriteBatch.DrawString(font, BlogData.Title, new Vector2(GlobalGraphics.Scale(220+1) - titleSize.X / 2, GlobalGraphics.Scale(59+1)), Color.Black);
            spriteBatch.DrawString(font, BlogData.Title, new Vector2(GlobalGraphics.Scale(220) - titleSize.X / 2, GlobalGraphics.Scale(59)), Color.White);
            // Draw banner
            spriteBatch.Draw(banner, new Rectangle(GlobalGraphics.Scale(140+1), GlobalGraphics.Scale(73+1), GlobalGraphics.Scale(banner.Width), GlobalGraphics.Scale(banner.Height)), Color.Black);
            spriteBatch.Draw(banner, new Rectangle(GlobalGraphics.Scale(140), GlobalGraphics.Scale(73), GlobalGraphics.Scale(banner.Width), GlobalGraphics.Scale(banner.Height)), Color.White);
            // Draw subtitle
            int increment = 10;
            int subtitleHeight = 90;
            for(int i = 0; i < BlogData.Subtitle.Count; i++)
            {
                Vector2 subtitleSize = font.MeasureString(BlogData.Subtitle[i]);
                spriteBatch.DrawString(font, BlogData.Subtitle[i], new Vector2(GlobalGraphics.Scale(273+1) - subtitleSize.X / 2, GlobalGraphics.Scale(subtitleHeight+1)), Color.Black);
                spriteBatch.DrawString(font, BlogData.Subtitle[i], new Vector2(GlobalGraphics.Scale(273) - subtitleSize.X / 2, GlobalGraphics.Scale(subtitleHeight)), Color.White);
                subtitleHeight += increment;
            }
            // Draw description
            int descriptionHeight = 133;
            for(int i = 0; i < BlogData.Description.Count; i++)
            {
                Vector2 descriptionSize = font.MeasureString(BlogData.Description[i]);
                spriteBatch.DrawString(font, BlogData.Description[i], new Vector2(GlobalGraphics.Scale(220+1) - descriptionSize.X / 2, GlobalGraphics.Scale(descriptionHeight+1)), Color.Black);
                spriteBatch.DrawString(font, BlogData.Description[i], new Vector2(GlobalGraphics.Scale(220) - descriptionSize.X / 2, GlobalGraphics.Scale(descriptionHeight)), Color.White);
                descriptionHeight += increment;
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("BlogBanner", contentManager.Load<Texture2D>("blog/banner"));
            // Interactable
            ParseBlog();
            controller.LoadContent(contentManager, graphicsDevice);
        }
        public void ParseBlog()
        {
            controller.Clear();
            // Add URL button
            controller.Add("BlogURL", new Button("Read More", "View announcement event on Steam.", new Vector2(220, 60+10+19*8), (int i, string n) => {
                switch(i)
                {
                    case 2:
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        ProcessStartInfo startInfo = new()
                        {
                            FileName = BlogData.Url,
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                        return true;
                }
                return false;
            }));
        }
    }
}
