using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Speech.Synthesis;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Disambiguation is called on-screen when the user wants to select a UI element.
    /// </summary>
    public class DisambiguationOption
    {
        public Rectangle bounds = new();
        public string tts = "";
        public DisambiguationOption(Rectangle bounds, string tts)
        {
            this.bounds = bounds;
            this.tts = tts;
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
        public static bool selected = false;
        public static bool holdItForMe = false;
        public static bool right = false;
        public static int offset = 0;
        public static bool enableTts = false;
        public static bool waiting = false;
        private static KeyboardState oldKeyboardState;
        private static KeyboardState newKeyboardState;
        private static KeyboardState oldKeyboardState2;
        private static KeyboardState newKeyboardState2;
        public static SpeechSynthesizer synth = new SpeechSynthesizer();
        public static bool PreUpdate(GameTime gameTime)
        {
            if(!allowAccessibility)
                return false;
            bool result = false;
            // Capture keyboard input
            oldKeyboardState2 = newKeyboardState2;
            newKeyboardState2 = Keyboard.GetState();
            // Check if user pressed f1 key and disambiguation is not already showing.
            if(newKeyboardState2.IsKeyDown(Keys.F1) && !oldKeyboardState2.IsKeyDown(Keys.F1))
            {
                if(!showDisambiguation)
                {
                    GlobalContent.GetSound("Disambiguation").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    showDisambiguation = true;
                    selectedDisambiguationOption = -1;
                    offset = 0;
                    result = true;
                    waiting = true;
                    string ttshelp = "";
                    foreach(string str in help)
                    {
                        ttshelp += str + "; ";
                    }
                    if(enableTts)
                        synth.SpeakAsync(ttshelp);
                }
                else
                {
                    synth.SpeakAsyncCancelAll();
                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    showDisambiguation = false;
                    result = true;
                }
            }
            // F2 toggle tts
            if(newKeyboardState2.IsKeyDown(Keys.F2) && !oldKeyboardState2.IsKeyDown(Keys.F2))
            {
                enableTts = !enableTts;
                if(synth.State == SynthesizerState.Speaking)
                    synth.SpeakAsyncCancelAll();
                // Use text to speech to say "text to speech enabled" or "text to speech disabled"
                if(enableTts)
                {
                    synth = new SpeechSynthesizer();
                    synth.SetOutputToDefaultAudioDevice();
                    synth.SpeakAsync("Text to speech enabled");
                }
                else
                {
                    synth.SpeakAsync("Text to speech disabled");
                }
                result = true;
            }
            // Check if user pressed escape key and disambiguation is already showing.
            if(newKeyboardState2.IsKeyDown(Keys.Escape) && !oldKeyboardState2.IsKeyDown(Keys.Escape) && showDisambiguation)
            {
                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                showDisambiguation = false;
                result = true;
            }
            return result;
        }
        public static void PostUpdate(GameTime gameTime)
        {
            if(!allowAccessibility)
                return;
            if(selectedDisambiguationOption > -1)
            {
                selected = false;
                right = false;
                if(holdItForMe)
                {
                    showDisambiguation = true; // show disambiguation again
                    holdItForMe = false;
                }
                else
                {
                    offset = 0;
                }
                selectedDisambiguationOption = -1;
            }
        }
        public static void PreDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!allowAccessibility)
                return;
        }
        public static void TTS(string overrideString = "")
        {
            if(!allowAccessibility)
                return;
            string ttsString = overrideString;
            if(disambiguationOptions.Count > 0 && disambiguationOptions.Count > -(offset))
            {
                if(enableTts)
                {
                    if(synth.State == SynthesizerState.Speaking)
                        synth.SpeakAsyncCancelAll();
                    if(overrideString != "")
                        synth.SpeakAsync(overrideString);
                    else
                    {
                        ttsString = disambiguationOptions[-(offset)].tts;
                        synth.SpeakAsync(disambiguationOptions[-offset].tts);
                    }
                }
                else
                {
                    ttsString = disambiguationOptions[-(offset)].tts;
                }
            }
            else if(enableTts && overrideString != "")
            {
                if(synth.State == SynthesizerState.Speaking)
                    synth.SpeakAsyncCancelAll();
                synth.SpeakAsync(overrideString);
            }
            help[help.Length - 1] = ttsString;
        }
        public static string[] help = new string[]
        {
            "Nonsensical Video Generator accessibility help:",
            "Press F1 to toggle keyboard navigation. Press F2 to toggle text to speech.",
            "Press enter to select an option.",
            "Press space to select and close keyboard navigation.",
            "Use arrow keys or tab to cycle through options.",
            ""
        };
        public static void PostDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!allowAccessibility)
                return;
            // Draw disambiguation options.
            if(selectedDisambiguationOption > -1)
                return;
            if(showDisambiguation)
            {
                // Capture keyboard input
                oldKeyboardState = newKeyboardState;
                newKeyboardState = Keyboard.GetState();
                if(offset < -(disambiguationOptions.Count - 1))
                {
                    offset = -(disambiguationOptions.Count - 1);
                }
                else if(offset > 0)
                {
                    offset = 0;
                }
                // Pressing ctrl will increase the offset.
                // If disambiguation options are more than 10, then the user can scroll through the options.
                if(newKeyboardState.IsKeyDown(Keys.Tab) && !oldKeyboardState.IsKeyDown(Keys.Tab)
                    && !newKeyboardState.IsKeyDown(Keys.LeftControl) && !newKeyboardState.IsKeyDown(Keys.RightControl))
                {
                    if(disambiguationOptions.Count > 0)
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
                        if(offset < -(disambiguationOptions.Count - 1))
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            offset = -(disambiguationOptions.Count - 1);
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
                        TTS();
                    }
                }
                // Arrow keys
                if((newKeyboardState.IsKeyDown(Keys.Down) && !oldKeyboardState.IsKeyDown(Keys.Down))
                    || (newKeyboardState.IsKeyDown(Keys.Up) && !oldKeyboardState.IsKeyDown(Keys.Up)))
                {
                    if(disambiguationOptions.Count > 0)
                    {
                        // Up
                        if(newKeyboardState.IsKeyDown(Keys.Up) && !oldKeyboardState.IsKeyDown(Keys.Up))
                        {
                            offset += 1;
                        }
                        else
                        {
                            offset -= 1;
                        }
                        // Only 46 possible keys, so shift until the last option is able to be selected.
                        if(offset < -(disambiguationOptions.Count - 1))
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            offset = -(disambiguationOptions.Count - 1);
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
                        TTS();
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    }
                }
                if(offset < -(disambiguationOptions.Count - 1))
                    offset = -(disambiguationOptions.Count - 1);
                else if(offset > 0)
                    offset = 0;
                if(waiting)
                {
                    waiting = false;
                    TTS();
                }
                // Draw black overlay.
                Texture2D pixel = GlobalContent.GetTexture("Pixel");
                spriteBatch.Draw(pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), Color.Black * 0.5f);
                SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
                // Draw help text at the top of the screen.
                spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(4), GlobalGraphics.scaledWidth - GlobalGraphics.Scale(8), GlobalGraphics.Scale(1) + GlobalGraphics.Scale(9) * help.Length), Color.Black);
                for(int i = 0; i < help.Length; i++)
                {
                    Vector2 size = munroSmall.MeasureString(help[i]);
                    // Center horizontally
                    Vector2 position = new(GlobalGraphics.Scale(6), GlobalGraphics.Scale(2 + i * 9));
                    // Draw opaque background
                    //spriteBatch.Draw(pixel, new Rectangle((int)position.X - GlobalGraphics.Scale(2), (int)position.Y + GlobalGraphics.Scale(4) - GlobalGraphics.Scale(2), (int)size.X + GlobalGraphics.Scale(2), (int)size.Y - GlobalGraphics.Scale(5) + GlobalGraphics.Scale(2)), Color.Black);
                    spriteBatch.DrawString(munroSmall, help[i], position, Color.White);
                }
                for(int i = 0; i < disambiguationOptions.Count; i++)
                {
                    // Key
                    string key = "";
                    if(i+offset >= 0)
                    {
                        if(i+offset < 1)
                        {
                            if (i+offset<9)
                            {
                                key = "[ ]"; //(i+offset+1).ToString();
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
                        if(i+offset < 1)
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
                        if(newKeyboardState.IsKeyDown(Keys.Space) && !oldKeyboardState.IsKeyDown(Keys.Space)
                            || newKeyboardState.IsKeyDown(Keys.Enter) && !oldKeyboardState.IsKeyDown(Keys.Enter))
                        {
                            selectedDisambiguationOption = i;
                            showDisambiguation = false;
                            if(newKeyboardState.IsKeyDown(Keys.Enter) && !oldKeyboardState.IsKeyDown(Keys.Enter))
                            {
                                //right = true;
                                holdItForMe = true;
                            }
                            else
                            {
                                holdItForMe = false;
                                //right = false;
                            }
                            GlobalContent.GetSound("CompatSelect").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            TTS("Selected " + disambiguationOptions[i].tts);
                            return;
                        }
                    }
                }
            }
            disambiguationOptions.Clear();
        }
        // Backwards-compatible accessibility function.
        public static void CompatAccessibility(Rectangle bounds, string tts)
        {
            if(!allowAccessibility)
                return;
            disambiguationOptions.Add(new DisambiguationOption(bounds, tts));
        }
    }
}