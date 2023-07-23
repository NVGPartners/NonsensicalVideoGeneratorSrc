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
    /// Dials are a way to select a value from a range of values.
    /// </summary>
    public class Dial : IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int State { get; set; } // 0: none, 1: hovering, 2: dragging, 3: stopped dragging
        public Vector2 Position { get; set; }
        public Func<int, bool> Callback { get; set; }
        private Rectangle bounds;
        private Rectangle scaledBounds;
        private int minValue = 0;
        private int maxValue = 100;
        private int value;
        private int degrees;
        private int originalValue;
        private int originalDegrees;
        private bool revolution = false;
        private Vector2 start;
        public Dial(string defaultName, string defaultTooltip, Vector2 defaultPosition, int defaultValue, int minValue, int maxValue, Func<int, bool> defaultCallback)
        {
            Name = defaultName;
            Tooltip = defaultTooltip;
            Position = defaultPosition;
            value = defaultValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            // Calculate degrees from value and min/max
            degrees = (int)MathHelper.Lerp(0, 360, (float)(value - minValue) / (maxValue - minValue));
            Callback = defaultCallback;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Calculate bounds
            bounds = new((int)Position.X, (int)Position.Y, 35, 15);
            scaledBounds = new(bounds.X * GlobalGraphics.scale, bounds.Y * GlobalGraphics.scale, bounds.Width * GlobalGraphics.scale, bounds.Height * GlobalGraphics.scale);
            if (handleInput)
            {
                // TODO: Handle dials for accessibility
                //Accessibility.CompatAccessibility(scaledBounds);
                // Check if the mouse is hovering
                switch(State)
                {
                    case 0:
                        if (scaledBounds.Contains(MouseInput.MouseState.Position))
                        {
                            State = 1;
                        }
                        break;
                    case 1:
                        if (!scaledBounds.Contains(MouseInput.MouseState.Position))
                        {
                            State = 0;
                            break;
                        }
                        // Are we dragging?
                        if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        {
                            start = MouseInput.MouseState.Position.ToVector2();
                            originalValue = value;
                            originalDegrees = degrees;
                            State = 2;
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        break;
                    case 2:
                        // Did the mouse stop dragging?
                        if (MouseInput.LastMouseState.LeftButton == ButtonState.Pressed && MouseInput.MouseState.LeftButton == ButtonState.Released)
                        {
                            State = 0;
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            break;
                        }
                        // Rotating around the origin point (bounds.X+7, bounds.Y+7, 5.5, 5.5)
                        // Calculate the angle between the start and end points
                        Vector2 end = MouseInput.MouseState.Position.ToVector2();
                        float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
                        // Convert to degrees
                        angle = MathHelper.ToDegrees(angle);
                        angle += 90;
                        // Convert to value
                        if(angle < 0)
                        {
                            angle += 360;
                        }
                        // Set revolution
                        if(angle >= 185 && angle < 190)
                        {
                            revolution = true;
                        }
                        else if(angle <= 175 && angle > 170)
                        {
                            revolution = false;
                        }
                        // When revolution is true, don't allow the angle to go below 180
                        if(revolution)
                        {
                            if(angle < 172)
                            {
                                angle = 360;
                            }
                        }
                        // When revolution is false, don't allow the angle to go above 180
                        else
                        {
                            if(angle > 187)
                            {
                                angle = 0;
                            }
                        }
                        // Calculate the value
                        value = (int)(angle / 360 * (maxValue - minValue));
                        // Set the value
                        degrees = (int)angle;
                        // Callback with the value
                        if(Callback(value))
                        {
                            // True means revert the value
                            value = originalValue;
                            degrees = originalDegrees;
                        }
                        // Return true to capture the mouse
                        return true;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the dial graphic
            Texture2D dial = GlobalContent.GetTexture("InteractiveDial");
            Texture2D[] dialValues = new Texture2D[31];
            for(int i = 0; i < 30; i++)
            {
                dialValues[i] = GlobalContent.GetTexture("InteractiveDialValue" + i);
            }
            dialValues[30] = GlobalContent.GetTexture("InteractiveDialValue29");
            // Shadows
            spriteBatch.Draw(dial, new Rectangle(GlobalGraphics.Scale(bounds.X + 1), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(dial.Width), GlobalGraphics.Scale(dial.Height)), Color.Black);
            // Normal
            spriteBatch.Draw(dial, new Rectangle(GlobalGraphics.Scale(bounds.X), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(dial.Width), GlobalGraphics.Scale(dial.Height)), State < 2 ? Color.White : Color.LightBlue);
            // Dial value graphics are angles from 0-90 degrees in the form of 30 textures
            // Calculate which graphic to draw
            int dialValue = degrees / 3;
            // If degrees are past 90, add an offset so the graphic can be rotated
            if(degrees > 270)
            {
                dialValue -= 90;
                spriteBatch.Draw(dialValues[dialValue], new Rectangle(GlobalGraphics.Scale(bounds.X + 2 + 8), GlobalGraphics.Scale(bounds.Y + 2 + 3), GlobalGraphics.Scale(dialValues[dialValue].Width), GlobalGraphics.Scale(dialValues[dialValue].Height)), null, Color.White, MathHelper.ToRadians(270), new Vector2(dialValues[dialValue].Width / 2, dialValues[dialValue].Height / 2), SpriteEffects.None, 0);
            }
            else if(degrees > 180)
            {
                dialValue -= 60;
                spriteBatch.Draw(dialValues[dialValue], new Rectangle(GlobalGraphics.Scale(bounds.X + 2 + 3), GlobalGraphics.Scale(bounds.Y + 2 + 3), GlobalGraphics.Scale(dialValues[dialValue].Width), GlobalGraphics.Scale(dialValues[dialValue].Height)), null, Color.White, MathHelper.ToRadians(180), new Vector2(dialValues[dialValue].Width / 2, dialValues[dialValue].Height / 2), SpriteEffects.None, 0);
            }
            else if(degrees > 90)
            {
                dialValue -= 30;
                spriteBatch.Draw(dialValues[dialValue], new Rectangle(GlobalGraphics.Scale(bounds.X + 2 + 3), GlobalGraphics.Scale(bounds.Y + 2 + 8), GlobalGraphics.Scale(dialValues[dialValue].Width), GlobalGraphics.Scale(dialValues[dialValue].Height)), null, Color.White, MathHelper.ToRadians(90), new Vector2(dialValues[dialValue].Width / 2, dialValues[dialValue].Height / 2), SpriteEffects.None, 0);
            }
            else if(dialValue > 0)
            {
                spriteBatch.Draw(dialValues[dialValue], new Rectangle(GlobalGraphics.Scale(bounds.X + 2), GlobalGraphics.Scale(bounds.Y + 2), GlobalGraphics.Scale(dialValues[dialValue].Width), GlobalGraphics.Scale(dialValues[dialValue].Height)), Color.White);
            }
            else
            {
                spriteBatch.Draw(dialValues[0], new Rectangle(GlobalGraphics.Scale(bounds.X + 2), GlobalGraphics.Scale(bounds.Y + 2), GlobalGraphics.Scale(dialValues[0].Width), GlobalGraphics.Scale(dialValues[0].Height)), Color.White);
            }
            // Draw text + shadow
            spriteBatch.DrawString(GlobalGraphics.fontMunro, value.ToString(System.Globalization.CultureInfo.InvariantCulture), new Vector2(GlobalGraphics.Scale(bounds.X + 17 + 1), GlobalGraphics.Scale(bounds.Y + 1 + 1)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, value.ToString(System.Globalization.CultureInfo.InvariantCulture), new Vector2(GlobalGraphics.Scale(bounds.X + 17), GlobalGraphics.Scale(bounds.Y + 1)), Color.White);
            // Label
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + bounds.Width + 4 + 1), GlobalGraphics.Scale(bounds.Y + 2 + 1)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + bounds.Width + 4), GlobalGraphics.Scale(bounds.Y + 2)), Color.White);
            // If hovering, draw tooltip
            if (State == 1 && Tooltip != "")
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
            GlobalContent.AddTexture("InteractiveDial", contentManager.Load<Texture2D>("graphics/interactivedial"));
            // Values
            for(int i = 0; i < 30; i++)
                GlobalContent.AddTexture("InteractiveDialValue" + i, contentManager.Load<Texture2D>("graphics/interactivedialvalue" + i));
        }
    }
}
#endif
