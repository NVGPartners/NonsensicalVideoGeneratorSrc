using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    public class SimpleObject : IObject
    {
        public Rectangle rectangle;
        public Color color;
        public Texture2D texture;
        public Func<bool> updateAction;
        public bool isButton = true;
        public SimpleObject(Rectangle rectangle, Color color, Texture2D texture, Func<bool> updateAction)
        {
            this.rectangle = rectangle;
            this.color = color;
            this.texture = texture;
            this.updateAction = updateAction;
        }
        public SimpleObject(Rectangle rectangle, Color color, Texture2D texture, Func<bool> updateAction, bool isButton)
        {
            this.rectangle = rectangle;
            this.color = color;
            this.texture = texture;
            this.updateAction = updateAction;
            this.isButton = isButton;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if (handleInput)
            {
                if(isButton)
                    Accessibility.CompatAccessibility(rectangle);
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    if (rectangle.Contains(MouseInput.MouseState.Position))
                    {
                        if(updateAction())
                            return true;
                    }
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, color);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
    }
    /// <summary>
    /// Here is where a mask can be applied to every screen.
    /// </summary>
    public class Mask : IObject
    {
        public Dictionary<string, SimpleObject> unmaskedObjects = new();
        public bool enabled = false;
        public Color color = new(0, 0, 0, 192);
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if (enabled)
            {
                if (handleInput)
                {
                    foreach (KeyValuePair<string, SimpleObject> unmaskedObject in unmaskedObjects)
                    {
                        if (unmaskedObject.Value.Update(gameTime, handleInput))
                        {
                            return true;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (enabled)
            {
                spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), color);
                foreach (KeyValuePair<string, SimpleObject> unmaskedObject in unmaskedObjects)
                {
                    unmaskedObject.Value.Draw(gameTime, spriteBatch);
                }
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
        public void AddUnmaskedObject(string name, SimpleObject simpleObject)
        {
            unmaskedObjects.Add(name, simpleObject);
        }
        public void RemoveUnmaskedObject(string name)
        {
            unmaskedObjects.Remove(name);
        }
        public void ClearUnmaskedObjects()
        {
            unmaskedObjects.Clear();
        }
        public void Enable()
        {
            enabled = true;
        }
        public void Enable(Dictionary<string, SimpleObject> unmaskedObjects)
        {
            enabled = true;
            // Merge the two dictionaries.
            foreach (KeyValuePair<string, SimpleObject> unmaskedObject in unmaskedObjects)
            {
                unmaskedObjects.Add(unmaskedObject.Key, unmaskedObject.Value);
            }
        }
        public void Disable(bool clearUnmaskedObjects = true)
        {
            enabled = false;
            if (clearUnmaskedObjects)
                unmaskedObjects.Clear();
        }
    }
}