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
    /// Switches have two states, on and off. They can be toggled by clicking on them.
    /// </summary>
    public class Switch : IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; } // 0: none, 1: hovering, 2: left click, 3: right click, 4: middle click, 5: forward, 6: back, 7: scroll up, 8: scroll down
        public bool SwitchState { get; set; } // false: off, true: on
        public Vector2 Position { get; set; }
        public Func<int, bool> Callback { get; set; }
        private Rectangle bounds;
        public Switch(string defaultName, string defaultTooltip, Vector2 defaultPosition, Func<int, bool> defaultCallback, bool defaultSwitchState)
        {
            Name = defaultName;
            Tooltip = defaultTooltip;
            Position = defaultPosition;
            Callback = defaultCallback;
            SwitchState = defaultSwitchState;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Calculate bounds
            bounds = new((int)Position.X, (int)Position.Y, 25, 15);
            Rectangle scaledBounds = new((int)(bounds.X * GlobalGraphics.scale), (int)(bounds.Y * GlobalGraphics.scale), (int)(bounds.Width * GlobalGraphics.scale), (int)(bounds.Height * GlobalGraphics.scale));
            if (handleInput)
            {
                Accessibility.CompatAccessibility(scaledBounds, "Toggle Switch: " + Name + " (" + Tooltip + ") set to " + (SwitchState ? "On" : "Off"));
                int mouseButtonFlags = 0;
                // Check if the mouse is hovering over the button.
                if (scaledBounds.Contains(MouseInput.MouseState.Position))
                {
                    // Bitwise operations determine which mouse buttons are pressed.
                    mouseButtonFlags |= 1;
                    // Check if the mouse is clicking on the button.
                    if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        mouseButtonFlags |= 2;
                    if (MouseInput.LastMouseState.RightButton == ButtonState.Released && MouseInput.MouseState.RightButton == ButtonState.Pressed)
                        mouseButtonFlags |= 4;
                    if (MouseInput.LastMouseState.MiddleButton == ButtonState.Released && MouseInput.MouseState.MiddleButton == ButtonState.Pressed)
                        mouseButtonFlags |= 8;
                    if (MouseInput.LastMouseState.XButton1 == ButtonState.Released && MouseInput.MouseState.XButton1 == ButtonState.Pressed)
                        mouseButtonFlags |= 16;
                    if (MouseInput.LastMouseState.XButton2 == ButtonState.Released && MouseInput.MouseState.XButton2 == ButtonState.Pressed)
                        mouseButtonFlags |= 32;
                    if (MouseInput.LastMouseState.ScrollWheelValue == 0 && MouseInput.MouseState.ScrollWheelValue > 0)
                        mouseButtonFlags |= 64;
                    if (MouseInput.LastMouseState.ScrollWheelValue == 0 && MouseInput.MouseState.ScrollWheelValue < 0)
                        mouseButtonFlags |= 128;
                }
                // If state is above 0, callback
                if ((mouseButtonFlags & 2) == 2)
                {
                    SwitchState = !SwitchState;
                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                }
                // Turn flags into state (most significant bit)
                State = (int)Math.Log(mouseButtonFlags, 2) + 1;
                if (SwitchState)
                    mouseButtonFlags |= 256;
                SwitchState = Callback(mouseButtonFlags);
                if((mouseButtonFlags & 2) == 2)
                    return true;
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the sides
            Texture2D switchGraphic = GlobalContent.GetTexture("InteractiveSwitch");
            Texture2D onGraphic = GlobalContent.GetTexture("InteractiveSwitchOn");
            Texture2D offGraphic = GlobalContent.GetTexture("InteractiveSwitchOff");
            // Shadows
            spriteBatch.Draw(switchGraphic, new Rectangle(GlobalGraphics.Scale(bounds.X + 1), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(switchGraphic.Width), GlobalGraphics.Scale(switchGraphic.Height)), Color.Black);
            spriteBatch.Draw(SwitchState ? onGraphic : offGraphic, new Rectangle(GlobalGraphics.Scale(bounds.X + 4 + 1), GlobalGraphics.Scale(bounds.Y + 3 + 1), GlobalGraphics.Scale(SwitchState ? onGraphic.Width : offGraphic.Width), GlobalGraphics.Scale(SwitchState ? onGraphic.Height : offGraphic.Height)), Color.Black);
            // Normal
            spriteBatch.Draw(switchGraphic, new Rectangle(GlobalGraphics.Scale(bounds.X), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(switchGraphic.Width), GlobalGraphics.Scale(switchGraphic.Height)), Color.White);
            spriteBatch.Draw(SwitchState ? onGraphic : offGraphic, new Rectangle(GlobalGraphics.Scale(bounds.X + 4), GlobalGraphics.Scale(bounds.Y + 3), GlobalGraphics.Scale(SwitchState ? onGraphic.Width : offGraphic.Width), GlobalGraphics.Scale(SwitchState ? onGraphic.Height : offGraphic.Height)), Color.White);
            // Text & shadow
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + 29 + 1), GlobalGraphics.Scale(bounds.Y + 2 + 1)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + 29), GlobalGraphics.Scale(bounds.Y + 2)), Color.White);
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
            GlobalContent.AddTexture("InteractiveSwitch", contentManager.Load<Texture2D>("graphics/interactiveswitch"));
            GlobalContent.AddTexture("InteractiveSwitchOn", contentManager.Load<Texture2D>("graphics/interactiveswitchon"));
            GlobalContent.AddTexture("InteractiveSwitchOff", contentManager.Load<Texture2D>("graphics/interactiveswitchoff"));
        }
    }
}
#endif
