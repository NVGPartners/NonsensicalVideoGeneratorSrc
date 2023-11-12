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
    public class ActionButton : Button
    {
        public Texture2D Icon { get; set; }
        public ActionButton(string defaultTooltip, Vector2 defaultPosition, Func<int, string, bool> defaultCallback, Texture2D icon) : base("", defaultTooltip, defaultPosition, defaultCallback)
        {
            Icon = icon;
        }
        public override bool Update(GameTime gameTime, bool handleInput)
        {
            if (handleInput)
            {
                // Calculate bounds
                Texture2D actionButton = GlobalContent.GetTexture("ActionButton");
                Rectangle scaledBounds = new((int)(Position.X * GlobalGraphics.scale), (int)(Position.Y * GlobalGraphics.scale), (int)(actionButton.Width * GlobalGraphics.scale), (int)(actionButton.Height * GlobalGraphics.scale));
                int mouseButton = 0;
                // Check if the mouse is hovering over the button.
                Accessibility.CompatAccessibility(scaledBounds, "Action Button: " + Tooltip);
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
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the button
            Texture2D actionButton = GlobalContent.GetTexture("ActionButton");
            // Shadow
            spriteBatch.Draw(actionButton, new Rectangle((int)GlobalGraphics.Scale(Position.X + 1), (int)GlobalGraphics.Scale(Position.Y + 1), GlobalGraphics.Scale(actionButton.Width), GlobalGraphics.Scale(actionButton.Height)), Color.Black);
            // Normal
            spriteBatch.Draw(actionButton, new Rectangle((int)GlobalGraphics.Scale(Position.X), (int)GlobalGraphics.Scale(Position.Y), GlobalGraphics.Scale(actionButton.Width), GlobalGraphics.Scale(actionButton.Height)), Color.White);
            // Icon
            spriteBatch.Draw(Icon, new Rectangle((int)GlobalGraphics.Scale(Position.X), (int)GlobalGraphics.Scale(Position.Y), GlobalGraphics.Scale(Icon.Width), GlobalGraphics.Scale(Icon.Height)), Color.White);
            // If hovering, draw tooltip
            if (State >= 1 && Tooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalContent.GetFont("MunroSmall").MeasureString(Tooltip);
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
                spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), Tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public override void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
    }
}
#endif
