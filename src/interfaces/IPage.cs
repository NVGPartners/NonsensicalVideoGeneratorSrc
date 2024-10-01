using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// A page, which is drawn from the content window and selectable from the menu screen.
    /// </summary>
    public interface IPage : IObject
    {
        public string Name { get; set; }
        public string Tooltip { get; }
    }
}