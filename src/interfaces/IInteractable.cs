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
    public interface IInteractable : IObject
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; }
        public Vector2 Position { get; set; }
        public Func<int, bool> Callback { get; set; }
    }
}
#endif
