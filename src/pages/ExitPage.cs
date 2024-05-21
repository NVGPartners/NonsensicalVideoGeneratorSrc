#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// News page.
    /// </summary>
    public class ExitPage : IPage
    {
        public string Name { get; set; } = "PageBlog";
        public string Tooltip { get; } = "Check out what's new!";
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
#endif
