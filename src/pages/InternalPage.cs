#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Debug page.
    /// </summary>
    public class InternalPage : IPage
    {
        public string Name { get; set; } = "Debug";
        public string Tooltip { get; } = "";
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
            if(Debug.GetDebugMode())
            {
                DrawButton(spriteBatch, 20, 50, "Debug Mode: Enabled");
            }
        }
        public void DrawButton(SpriteBatch spriteBatch, int x, int y, string text)
        {
            GlobalGraphics.DrawButton(spriteBatch, GlobalGraphics.Scale(x), GlobalGraphics.Scale(y), Color.Transparent, text, Color.White, Color.Gray);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
