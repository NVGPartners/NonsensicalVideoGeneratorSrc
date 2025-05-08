using System;
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
        public override bool Update(GameTime gameTime, bool handleInput, string internalName, Vector2 mousePosition)
        {
            if (handleInput)
            {
                // Calculate bounds
                Texture2D actionButton = GlobalContent.GetTexture("ActionButton");
                Rectangle scaledBounds = new((int)(Position.X * GlobalGraphics.scale), (int)(Position.Y * GlobalGraphics.scale), (int)(actionButton.Width * GlobalGraphics.scale), (int)(actionButton.Height * GlobalGraphics.scale));
                int mouseButton = 0;
                // Check if the mouse is hovering over the button.
                Accessibility.CompatAccessibility(scaledBounds, L.T(0, "Accessibility:InteractableActionButton", Tooltip));
                if (scaledBounds.Contains(mousePosition))
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
                    bool result = Callback(mouseButton, internalName);
                    if (result)
                        return true;
                }
            }
            return false;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, string internalName, Vector2 mousePosition)
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
                if(internalName.StartsWith("NoLocalization:"))
                {
                    Global.tooltip = Tooltip;
                }
                else
                {
                    string token = "Interactable:"+internalName+"Tooltip";
                    // check if the token exists
                    string localized = L.T(0, token);
                    if (localized != token || L.GetLocale().name == "dummy")
                        Global.tooltip = localized;
                    else
                        Global.tooltip = ""; // no tooltip
                }
            }
        }
        public override void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, string internalName)
        {
        }
    }
}
