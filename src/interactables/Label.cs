using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Labels are interactables that display text and do not have any functionality.
    /// </summary>
    public class Label : IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; } // Never used, use ClickableLabel for input or tooltip functionality.
        public Vector2 Position { get; set; }
        public Func<int, string, bool> Callback { get; set; }
        private bool UseShadow = true;
        private bool Underline = false;
        private Color TextColor = Color.White;
        private Color ShadowColor = Color.Black;
        public Label(string defaultName, Vector2 defaultPosition)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, string, bool>((i, n) => false); // Dummy function
        }
        public Label(string defaultName, Vector2 defaultPosition, bool underline = false)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, string, bool>((i, n) => false); // Dummy function
            Underline = underline;
        }
        public Label(string defaultName, Vector2 defaultPosition, bool defaultUseShadow, Color defaultTextColor, Color defaultShadowColor, bool underline = false)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, string, bool>((i, n) => false); // Dummy function
            UseShadow = defaultUseShadow;
            TextColor = defaultTextColor;
            ShadowColor = defaultShadowColor;
            Underline = underline;
        }
        public bool Update(GameTime gameTime, bool handleInput, string internalName, Vector2 mousePosition)
        {
            // Nothing to update
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, string internalName, Vector2 mousePosition)
        {
            // Text & shadow, that's it
            SpriteFont drawFont = L.FontLarge();
            // Localize title
            string localizedTitle;
            if(internalName.StartsWith("NoLocalization:"))
            {
                localizedTitle = Name;
            }
            else
            {
                string token = "Interactable:"+internalName+"Title";
                // check if the token exists
                string localized = L.T(0, token, PluginHandler.GetPluginListFilter());
                if (localized != token || L.GetLocale().name == "dummy")
                    localizedTitle = localized;
                else
                    localizedTitle = ""; // no title
            }
            // Draw underline
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            Vector2 size = drawFont.MeasureString(localizedTitle);
            if(UseShadow)
                GlobalContent.DrawString(spriteBatch, drawFont, localizedTitle, new Vector2(GlobalGraphics.Scale(Position.X + 1), GlobalGraphics.Scale(Position.Y - 3 + 1)), ShadowColor);
            if(Underline)
            {
                if(UseShadow)
                    spriteBatch.Draw(pixel, new Rectangle((int)GlobalGraphics.Scale(Position.X), (int)GlobalGraphics.Scale(Position.Y + 7 + 1), (int)size.X, GlobalGraphics.Scale(1)), ShadowColor);
                spriteBatch.Draw(pixel, new Rectangle((int)GlobalGraphics.Scale(Position.X), (int)GlobalGraphics.Scale(Position.Y + 7), (int)size.X, GlobalGraphics.Scale(1)), TextColor);
            }
            GlobalContent.DrawString(spriteBatch, drawFont, localizedTitle, new Vector2(GlobalGraphics.Scale(Position.X), GlobalGraphics.Scale(Position.Y-3)), TextColor);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, string internalName)
        {
            // Default fonts are already loaded
        }
    }
}
