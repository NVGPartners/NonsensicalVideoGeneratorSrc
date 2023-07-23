#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// These are all UI objects that can be interacted with.
    /// </summary>
    public interface IObject
    {
        public bool Update(GameTime gameTime, bool handleInput);
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice);
    }
}
#endif
