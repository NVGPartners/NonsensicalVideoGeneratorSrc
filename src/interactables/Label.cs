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
        public Func<int, string, bool> Callback { get; set; }
        private bool UseShadow = true;
        private Color TextColor = Color.White;
        private Color ShadowColor = Color.Black;
        public Label(string defaultName, Vector2 defaultPosition)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, string, bool>((i, n) => false); // Dummy function
        }
        public Label(string defaultName, Vector2 defaultPosition, bool defaultUseShadow, Color defaultTextColor, Color defaultShadowColor)
        {
            Name = defaultName;
            Tooltip = "";
            Position = defaultPosition;
            Callback = new Func<int, string, bool>((i, n) => false); // Dummy function
            UseShadow = defaultUseShadow;
            TextColor = defaultTextColor;
            ShadowColor = defaultShadowColor;
        }
        public bool Update(GameTime gameTime, bool handleInput, string internalName)
        {
            // Nothing to update
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, string internalName)
        {
            // Text & shadow, that's it
            SpriteFont drawFont = L.FontLarge();
            if(UseShadow)
                spriteBatch.DrawString(drawFont, L.T(0, "Interactable:"+internalName+"Title"), new Vector2(GlobalGraphics.Scale(Position.X + 1), GlobalGraphics.Scale(Position.Y - 3 + 1)), ShadowColor);
            spriteBatch.DrawString(drawFont, L.T(0, "Interactable:"+internalName+"Title"), new Vector2(GlobalGraphics.Scale(Position.X), GlobalGraphics.Scale(Position.Y-3)), TextColor);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, string internalName)
        {
            // Default fonts are already loaded
        }
    }
}
#endif
