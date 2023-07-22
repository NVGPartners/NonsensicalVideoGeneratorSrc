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
        public Func<int, bool> Callback { get; set; }
        private Vector2 textSize;
        private Vector2 textPosition;
        private Rectangle bounds;
        public Button(string defaultName, string defaultTooltip, Vector2 defaultPosition, Func<int, bool> defaultCallback)
        {
            Name = defaultName;
            Tooltip = defaultTooltip;
            Position = defaultPosition;
            Callback = defaultCallback;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Calculate bounds
            textSize = (GlobalGraphics.fontMunro.MeasureString(Name) / GlobalGraphics.scale) - new Vector2(GlobalGraphics.Scale(1), 0);
            textPosition = new(Position.X - textSize.X / 2, Position.Y - textSize.Y / 2);
            bounds = new((int)textPosition.X-4, (int)textPosition.Y-4, (int)textSize.X+8, 15);
            Rectangle scaledBounds = new((int)(bounds.X * GlobalGraphics.scale), (int)(bounds.Y * GlobalGraphics.scale), (int)(bounds.Width * GlobalGraphics.scale), (int)(bounds.Height * GlobalGraphics.scale));
            if (handleInput)
            {
                int mouseButton = 0;
                // Check if the mouse is hovering over the button.
                Accessibility.CompatAccessibility(scaledBounds, "Button: " + Name + " (" + Tooltip + ")");
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
                    bool result = Callback(mouseButton);
                    if (result)
                        return true;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
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
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(textPosition.X + 1), GlobalGraphics.Scale(textPosition.Y - 3 + 1)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(textPosition.X), GlobalGraphics.Scale(textPosition.Y-3)), Color.White);
            // If hovering, draw tooltip
            if (State >= 1 && Tooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(Tooltip);
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 10, MouseInput.MouseState.Position.Y + 10);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2); 
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), new Color(0, 0, 0, 255));
                // White text
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, Tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("InteractiveButtonSide", contentManager.Load<Texture2D>("graphics/interactivebuttonside"));
            GlobalContent.AddTexture("InteractiveButtonInner", contentManager.Load<Texture2D>("graphics/interactivebuttoninner"));
        }
    }
}