using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class is used to manage all interactables in one place.
    /// </summary>
    public class InteractableController : IObject
    {
        // The list of interactables to manage.
        public Dictionary<string, IInteractable> interactables = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            bool returnValue = false;
            List<string> names = new();
            try
            {
                // Update all interactables.
                foreach (KeyValuePair<string, IInteractable> interactable in interactables)
                {
                    names.Add(interactable.Value.Name + "Input");
                    if(interactable.Value.Update(gameTime, handleInput))
                        returnValue = true;
                }
            }
            catch {} // modified
            // If editing is not in the name list, set it to ""
            if (Global.editing != "" && !names.Contains(Global.editing))
                Global.editing = "";
            return returnValue;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw all interactables.
            foreach (KeyValuePair<string, IInteractable> interactable in interactables)
            {
                interactable.Value.Draw(gameTime, spriteBatch);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Load content for all interactables.
            foreach (KeyValuePair<string, IInteractable> interactable in interactables)
            {
                interactable.Value.LoadContent(contentManager, graphicsDevice);
            }
        }
        // Clear managed interactables.
        public void Clear()
        {
            interactables.Clear();
        }
        // Add an interactable to the list.
        public void Add(string name, IInteractable interactable)
        {
            interactables.Add(name, interactable);
        }
        // Remove an interactable from the list.
        public void Remove(string name)
        {
            interactables.Remove(name);
        }
    }
}