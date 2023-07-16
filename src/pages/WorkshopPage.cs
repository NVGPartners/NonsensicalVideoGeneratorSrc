using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class WorkshopPage : IPage
    {
        public string Name { get; set; } = "Workshop";
        public string Tooltip { get; } = "Create and upload effects.";
        private readonly InteractableController controller = new();
        private int currentType = 0;
        private int previousTypeCount = 0;
        private List<string> names = new();
        private string clipUrl = "";
        private bool downloading = false;
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
            controller.Add("Label", new Label("Workshop", new Vector2(139, 60)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}