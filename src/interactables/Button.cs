#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Buttons are simple UI elements that can be clicked on to perform a single action.
    /// </summary>
    public class Button : IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; } // 0: none, 1: hovering, 2: left click, 3: right click, 4: middle click, 5: forward, 6: back, 7: scroll up, 8: scroll down
        public Vector2 Position { get; set; }
        public Func<int, string, bool> Callback { get; set; }
        public Vector2 textSize;
        public Vector2 textPosition;
        public Rectangle bounds;
        public Button(string defaultName, string defaultTooltip, Vector2 defaultPosition, Func<int, string, bool> defaultCallback)
        {
            Name = defaultName;
            Tooltip = defaultTooltip;
            Position = defaultPosition;
            Callback = defaultCallback;
        }
        public virtual bool Update(GameTime gameTime, bool handleInput, string internalName)
        {
            // Calculate bounds
            textSize = (L.FontLarge().MeasureString(Name) / GlobalGraphics.scale) - new Vector2(GlobalGraphics.Scale(1), 0);
            textPosition = new(Position.X - textSize.X / 2, Position.Y - textSize.Y / 2);
            bounds = new((int)textPosition.X-4, (int)textPosition.Y-4, (int)textSize.X+8, 15);
            Rectangle scaledBounds = new((int)(bounds.X * GlobalGraphics.scale), (int)(bounds.Y * GlobalGraphics.scale), (int)(bounds.Width * GlobalGraphics.scale), (int)(bounds.Height * GlobalGraphics.scale));
            if (handleInput)
            {
                int mouseButton = 0;
                // Check if the mouse is hovering over the button.
                Accessibility.CompatAccessibility(scaledBounds, L.T(0, "Accessibility:InteractableButton", L.T(0, "Interactable:"+internalName+"Title"), L.T(0, "Interactable:"+internalName+"Tooltip")));
                if (scaledBounds.Contains(MouseInput.MouseState.Position))
                {
                    // Check if the mouse is clicking on the button.
                    if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        mouseButton = 2;
                    else if (MouseInput.LastMouseState.RightButton == ButtonState.Released && MouseInput.MouseState.RightButton == ButtonState.Pressed)
                        mouseButton = 3;
                    else if (MouseInput.LastMouseState.MiddleButton == ButtonState.Released && MouseInput.MouseState.MiddleButton == ButtonState.Pressed)
                        mouseButton = 4;
                    else if (MouseInput.LastMouseState.XButton1 == ButtonState.Released && MouseInput.MouseState.XButton1 == ButtonState.Pressed)
                        mouseButton = 5;
                    else if (MouseInput.LastMouseState.XButton2 == ButtonState.Released && MouseInput.MouseState.XButton2 == ButtonState.Pressed)
                        mouseButton = 6;
                    else if (MouseInput.LastMouseState.ScrollWheelValue == 0 && MouseInput.MouseState.ScrollWheelValue > 0)
                        mouseButton = 7;
                    else if (MouseInput.LastMouseState.ScrollWheelValue == 0 && MouseInput.MouseState.ScrollWheelValue < 0)
                        mouseButton = 8;
                    else
                        mouseButton = 1;
                }
                // If state is above -1, callback
                if (mouseButton > -1)
                {
                    State = mouseButton;
                    bool result = Callback(mouseButton, Name);
                    if (result)
                        return true;
                }
            }
            return false;
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, string internalName)
        {
            // Draw the sides
            Texture2D side = GlobalContent.GetTexture("InteractiveButtonSide");
            Texture2D inner = GlobalContent.GetTexture("InteractiveButtonInner");
            // Shadows
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + 1), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), Color.Black);
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + 1 + bounds.Width - 1 + side.Width/2), GlobalGraphics.Scale(bounds.Y + 1 - 1 + side.Height/2), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), null, Color.Black, MathHelper.ToRadians(180), new Vector2(side.Width/2, side.Height/2), SpriteEffects.None, 0);
            spriteBatch.Draw(inner, new Rectangle(GlobalGraphics.Scale(bounds.X + 1+3), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(bounds.Width-3), GlobalGraphics.Scale(inner.Height)), Color.Black);
            // Normal
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), Color.White);
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + bounds.Width - 1 + side.Width/2), GlobalGraphics.Scale(bounds.Y - 1 + side.Height/2), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), null, Color.White, MathHelper.ToRadians(180), new Vector2(side.Width/2, side.Height/2), SpriteEffects.None, 0);
            spriteBatch.Draw(inner, new Rectangle(GlobalGraphics.Scale(bounds.X+3), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(bounds.Width-3), GlobalGraphics.Scale(inner.Height)), Color.White);
            // Text & shadow
            spriteBatch.DrawString(L.FontLarge(), L.T(0, "Interactable:"+internalName+"Title"), new Vector2(GlobalGraphics.Scale(textPosition.X + 1), GlobalGraphics.Scale(textPosition.Y - 3 + 1)), Color.Black);
            spriteBatch.DrawString(L.FontLarge(), L.T(0, "Interactable:"+internalName+"Title"), new Vector2(GlobalGraphics.Scale(textPosition.X), GlobalGraphics.Scale(textPosition.Y-3)), Color.White);
            // If hovering, draw tooltip
            if (State >= 1 && Tooltip != "")
            {
                Global.tooltip = L.T(0, "Interactable:"+internalName+"Tooltip");
            }
        }
        public virtual void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, string internalName)
        {
        }
    }
}
#endif
