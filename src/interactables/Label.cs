#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        public Func<int, bool> Callback { get; set; }
        private string Font = "Munro";
        private bool UseShadow = true;
        private Color TextColor = Color.White;
        private Color ShadowColor = Color.Black;
        public Label(string defaultName, Vector2 defaultPosition)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, bool>(i => false); // Dummy function
        }
        public Label(string defaultName, Vector2 defaultPosition, bool defaultUseShadow, Color defaultTextColor, Color defaultShadowColor, string defaultFont = "Munro")
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, bool>(i => false); // Dummy function
            Font = defaultFont;
            UseShadow = defaultUseShadow;
            TextColor = defaultTextColor;
            ShadowColor = defaultShadowColor;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Nothing to update
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Text & shadow, that's it
            SpriteFont drawFont = GlobalContent.GetFont(Font);
            if(UseShadow)
                spriteBatch.DrawString(drawFont, Name, new Vector2(GlobalGraphics.Scale(Position.X + 1), GlobalGraphics.Scale(Position.Y - 3 + 1)), ShadowColor);
            spriteBatch.DrawString(drawFont, Name, new Vector2(GlobalGraphics.Scale(Position.X), GlobalGraphics.Scale(Position.Y-3)), TextColor);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Default fonts are already loaded
        }
    }
}
#endif
