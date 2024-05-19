#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Interactables are prefab objects that can be used to create buttons, sliders, and other UI elements.
    /// </summary>
    public interface IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; }
        public Vector2 Position { get; set; }
        public Func<int, string, bool> Callback { get; set; }
        public bool Update(GameTime gameTime, bool handleInput, string internalName);
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, string internalName);
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, string internalName);
    }
}
#endif
