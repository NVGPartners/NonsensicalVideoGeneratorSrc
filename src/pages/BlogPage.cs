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
        private readonly InteractableController actionController = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Interactable
            if(actionController.Update(gameTime, handleInput))
                return true;
            if(controller.Update(gameTime, handleInput))
                return true;
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
            string title = L.T(0, "Blog:UpdateTitle", L.T(0, "Blog:UpdateMonth" + BlogData.Month.ToString(CultureInfo.InvariantCulture)), BlogData.Year.ToString(CultureInfo.InvariantCulture), "v" + Global.productVersion.Remove(0, 2).Remove(3));
            Vector2 titleSize = font.MeasureString(title);
            GlobalContent.DrawString(spriteBatch, font, title, new Vector2(GlobalGraphics.Scale(220+1) - titleSize.X / 2, GlobalGraphics.Scale(59+1)), Color.Black);
            GlobalContent.DrawString(spriteBatch, font, title, new Vector2(GlobalGraphics.Scale(220) - titleSize.X / 2, GlobalGraphics.Scale(59)), Color.White);
            // Draw banner
            spriteBatch.Draw(banner, new Rectangle(GlobalGraphics.Scale(140+1), GlobalGraphics.Scale(73+1), GlobalGraphics.Scale(banner.Width), GlobalGraphics.Scale(banner.Height)), Color.Black);
            spriteBatch.Draw(banner, new Rectangle(GlobalGraphics.Scale(140), GlobalGraphics.Scale(73), GlobalGraphics.Scale(banner.Width), GlobalGraphics.Scale(banner.Height)), Color.White);
            // Draw subtitle
            int increment = 10;
            int subtitleHeight = 85;
            for(int i = 0; i < BlogData.Subtitle.Count; i++)
            {
                string subtitle = BlogData.Subtitle[i].Replace("%version%", Global.productVersion.Remove(7)).Replace("%month%", BlogData.Month.ToString("D2", CultureInfo.InvariantCulture)).Replace("%day%", BlogData.Day.ToString("D2", CultureInfo.InvariantCulture)).Replace("%year%", BlogData.Year.ToString(CultureInfo.InvariantCulture)).Replace("%tier%", L.T(0, "Blog:UpdateTier" + BlogData.Tier.ToString(CultureInfo.InvariantCulture)));
                Vector2 subtitleSize = font.MeasureString(subtitle);
                GlobalContent.DrawString(spriteBatch, font, subtitle, new Vector2(GlobalGraphics.Scale(273+1) - subtitleSize.X / 2, GlobalGraphics.Scale(subtitleHeight+1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, subtitle, new Vector2(GlobalGraphics.Scale(273) - subtitleSize.X / 2, GlobalGraphics.Scale(subtitleHeight)), Color.White);
                subtitleHeight += increment;
            }
            // Draw description
            int descriptionHeight = 133;
            for(int i = 0; i < BlogData.Description.Count; i++)
            {
                string text = BlogData.Description[i].Replace("%footer%", L.T(0, "Blog:UpdateFooter"));
                Vector2 descriptionSize = font.MeasureString(text);
                GlobalContent.DrawString(spriteBatch, font, text, new Vector2(GlobalGraphics.Scale(220+1) - descriptionSize.X / 2, GlobalGraphics.Scale(descriptionHeight+1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, text, new Vector2(GlobalGraphics.Scale(220) - descriptionSize.X / 2, GlobalGraphics.Scale(descriptionHeight)), Color.White);
                descriptionHeight += increment;
            }
            actionController.Draw(gameTime, spriteBatch);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("BlogBanner", contentManager.Load<Texture2D>("blog/banner"));
            // Interactable
            ParseBlog();
            controller.LoadContent(contentManager, graphicsDevice);
            actionController.Clear();
            actionController.Add("ActionDiscord", new ActionButton("Join our Discord server!", new Vector2(112, 191+15), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if(Global.ready)
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            ProcessStartInfo psi = new()
                            {
                                FileName = "https://discord.gg/8ppmspR6Wh",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/discord")));
            actionController.Add("ActionGitHub", new ActionButton("View the issue tracker on GitHub!", new Vector2(112, 191+(15*2)), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if(Global.ready)
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            ProcessStartInfo psi = new()
                            {
                                FileName = "https://github.com/KiwifruitDev/NonsensicalVideoGenerator",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/github")));
        }
        public void ParseBlog()
        {
            controller.Clear();
            // Add URL button
            controller.Add("BlogURL", new Button("Read More", "View announcement event on Steam.", new Vector2(220, 60+15+19*8), (int i, string n) => {
                switch(i)
                {
                    case 2:
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        ProcessStartInfo startInfo = new()
                        {
                            FileName = BlogData.Url.Replace("%postid%", BlogData.PostId).Replace("%locale%", L.GetLocale().name),
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
