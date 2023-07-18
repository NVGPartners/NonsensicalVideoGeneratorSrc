using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class PastimePage : IPage
    {
        public string Name { get; set; } = "Pastime";
        public string Tooltip { get; } = "Play a game while generating.";
        private readonly InteractableController controller = new();
        public bool active = false;
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
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}