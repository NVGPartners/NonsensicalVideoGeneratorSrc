using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Disambiguation is called on-screen when the user wants to select a UI element.
    /// </summary>
    public class DisambiguationOption
    {
        public Rectangle bounds = new();
        public DisambiguationOption(Rectangle bounds)
        {
            this.bounds = bounds;
        }
    }
    /// <summary>
    /// This class handles user accessibility.
    /// </summary>
    public static class Accessibility
    {
        public static bool allowAccessibility = true;
        public static bool showDisambiguation = false;
        public static List<DisambiguationOption> disambiguationOptions = new();
        public static int selectedDisambiguationOption = -1;
        public static int hovered = -1;
        public static bool selected = false;
        public static bool right = false;
        public static int offset = 0;
        private static KeyboardState oldKeyboardState;
        private static KeyboardState newKeyboardState;
        public static bool PreUpdate(GameTime gameTime)
        {
            bool result = false;
            // Capture keyboard input
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();
            // Check if user pressed tab key and disambiguation is not already showing.
            if(newKeyboardState.IsKeyDown(Keys.Tab) && !oldKeyboardState.IsKeyDown(Keys.Tab))
            {
                if(!showDisambiguation)
                {
                    GlobalContent.GetSound("Disambiguation").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    showDisambiguation = true;
                    selectedDisambiguationOption = -1;
                    hovered = -1;
                    offset = 0;
                }
                else
                {
                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    showDisambiguation = false;
                }
                result = true;
            }
            // Check if user pressed escape key and disambiguation is already showing.
            if(newKeyboardState.IsKeyDown(Keys.Escape) && !oldKeyboardState.IsKeyDown(Keys.Escape) && showDisambiguation)
            {
                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                showDisambiguation = false;
                result = true;
            }
            return result;
        }
        public static void PostUpdate(GameTime gameTime)
        {
            if(MouseInput.done)
            {
                Accessibility.selected = false;
                Accessibility.right = false;
                Accessibility.selectedDisambiguationOption = -1;
                MouseInput.done = false;
            }
        }
        public static void PreDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }
        public static void PostDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw disambiguation options.
            if(showDisambiguation)
            {
                // Pressing ctrl will increase the offset.
                // If disambiguation options are more than 10, then the user can scroll through the options.
                if((newKeyboardState.IsKeyDown(Keys.LeftAlt) && !oldKeyboardState.IsKeyDown(Keys.LeftAlt))
                    || (newKeyboardState.IsKeyDown(Keys.RightAlt) && !oldKeyboardState.IsKeyDown(Keys.RightAlt)))
                {
                    if(disambiguationOptions.Count > 9)
                    {
                        // Shift modifier
                        if(newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift))
                        {
                            offset += 1;
                        }
                        else
                        {
                            offset -= 1;
                        }
                        // Only 46 possible keys, so shift until the last option is able to be selected.
                        if(offset < -(disambiguationOptions.Count - 9 - 1))
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            offset = -(disambiguationOptions.Count - 9 - 1);
                        }
                        else if(offset > 0)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            offset = 0;
                        }
                        else
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    }
                }
                // Cycle through options for hover using gameTime
                int count = disambiguationOptions.Count;
                if(count > 0)
                    hovered = (int)(gameTime.TotalGameTime.TotalMilliseconds / 500) % count;
                // Draw black overlay.
                Texture2D pixel = GlobalContent.GetTexture("Pixel");
                spriteBatch.Draw(pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), Color.Black * 0.5f);
                SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
                // Draw help text at the top of the screen.
                string[] help = new string[]
                {
                    "Disambiguation mode:",
                    "Press tab or escape to close.",
                    "Use alt and shift+alt to scroll through options.",
                    "Press a number key to select an option.",
                };
                for(int i = 0; i < help.Length; i++)
                {
                    Vector2 size = munroSmall.MeasureString(help[i]);
                    // Center horizontally
                    Vector2 position = new((GlobalGraphics.scaledWidth - size.X) / 2, GlobalGraphics.Scale(8 + i * 8));
                    // Draw opaque background
                    spriteBatch.Draw(pixel, new Rectangle((int)position.X, (int)position.Y + GlobalGraphics.Scale(4), (int)size.X, (int)size.Y - GlobalGraphics.Scale(6)), Color.Black);
                    spriteBatch.DrawString(munroSmall, help[i], position, Color.White);
                }
                for(int i = 0; i < disambiguationOptions.Count; i++)
                {
                    // Key
                    string key = "";
                    if(i+offset >= 0)
                    {
                        if(i+offset < 10)
                        {
                            if (i+offset<9)
                            {
                                key = (i+offset+1).ToString();
                            }
                            else
                            {
                                key = "0";
                            }
                        }
                    }
                    Vector2 offsetv = new();
                    TutorialScreen? tutorialScreen = null;
                    tutorialScreen = ScreenManager.GetScreen<TutorialScreen>("Initial Setup");
                    if(tutorialScreen != null && tutorialScreen.screenType == ScreenType.Drawn)
                    {
                        offsetv = tutorialScreen.offset;
                    }
                    // Draw box
                    spriteBatch.Draw(pixel, new Rectangle(disambiguationOptions[i].bounds.X + (int)offsetv.X, disambiguationOptions[i].bounds.Y + (int)offsetv.Y, disambiguationOptions[i].bounds.Width, disambiguationOptions[i].bounds.Height), Color.White * 0.25f);
                    // Draw
                    if(i+offset >= 0)
                    {
                        int shadowDepth = GlobalGraphics.Scale(1);
                        spriteBatch.DrawString(munroSmall, key, new Vector2(disambiguationOptions[i].bounds.X + disambiguationOptions[i].bounds.Width / 2 - munroSmall.MeasureString(key).X / 2 + shadowDepth, disambiguationOptions[i].bounds.Y + disambiguationOptions[i].bounds.Height / 2 - munroSmall.MeasureString(key).Y / 2 + shadowDepth) + offsetv, Color.Black);
                        spriteBatch.DrawString(munroSmall, key, new Vector2(disambiguationOptions[i].bounds.X + disambiguationOptions[i].bounds.Width / 2 - munroSmall.MeasureString(key).X / 2, disambiguationOptions[i].bounds.Y + disambiguationOptions[i].bounds.Height / 2 - munroSmall.MeasureString(key).Y / 2) + offsetv, Color.White);
                        // Input
                        Keys key2 = Keys.None;
                        if(i+offset < 10)
                        {
                            if(i+offset<9)
                            {
                                key2 = (Keys)(i+offset+49);
                            }
                            else
                            {
                                key2 = Keys.D0;
                            }
                        }
                        // Check if user pressed key and disambiguation is already showing.
                        if(newKeyboardState.IsKeyDown(key2) && !oldKeyboardState.IsKeyDown(key2))
                        {
                            selectedDisambiguationOption = i;
                            showDisambiguation = false;
                            // Shift modifier
                            if(newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift))
                            {
                                right = true;
                            }
                            else
                            {
                                right = false;
                            }
                            GlobalContent.GetSound("CompatSelect").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                    }
                }
            }
            disambiguationOptions.Clear();
        }
        // Backwards-compatible accessibility function.
        public static void CompatAccessibility(Rectangle bounds)
        {
            disambiguationOptions.Add(new DisambiguationOption(bounds));
        }
    }
}