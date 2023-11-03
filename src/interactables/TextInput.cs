#if MONOGAME
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Text entry boxes are used to enter text.
    /// </summary>
    public class TextEntry : IInteractable
    {
        public string Name { get; set; }
        public string Tooltip { get; set; } // This is the actual text, not a tooltip. 
        public int State { get; set; } // 0: none, 1: entering text
        public Vector2 Position { get; set; }
        public Func<int, string, bool> Callback { get; set; }
        public Rectangle bounds;
        private Rectangle scaledBounds;
        private int maxChars = 0;
        private int mode = 0;
        private string hiddenToolTip = ""; // The actual tooltip variable.
        private KeyboardState oldKeyboardState;
        private KeyboardState newKeyboardState;
        public TextEntry(string defaultName, string defaultTooltip, string defaultText, Vector2 defaultPosition, int width, int maxChars, int mode, Func<int, string, bool> defaultCallback)
        {
            Name = defaultName;
            hiddenToolTip = defaultTooltip;
            Tooltip = defaultText;
            Position = defaultPosition;
            Callback = defaultCallback;
            bounds = new((int)Position.X, (int)Position.Y, width, 15);
            this.maxChars = maxChars;
            this.mode = mode;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Calculate bounds
            scaledBounds = new((int)(bounds.X * GlobalGraphics.scale), (int)(bounds.Y * GlobalGraphics.scale), (int)(bounds.Width * GlobalGraphics.scale), (int)(bounds.Height * GlobalGraphics.scale));
            if (handleInput)
            {
                Accessibility.CompatAccessibility(scaledBounds, "Text Input: " + Name + " (" + hiddenToolTip + ") set to \"" + Tooltip + "\"");
                // Capture keyboard input
                oldKeyboardState = newKeyboardState;
                newKeyboardState = Keyboard.GetState();
                // Check if the mouse is hovering over the button.
                if (scaledBounds.Contains(MouseInput.MouseState.Position))
                {
                    // Check if the mouse is clicking on the button.
                    if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                    {
                        if(Global.editing == Name + "Input")
                        {
                            // Check to make sure there's actually some text
                            if (Tooltip == "")
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            Callback(State, Name);
                            Global.editing = "";
                            //Accessibility.allowAccessibility = true;
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            Global.mask.Disable();
                        }
                        else if(Global.editing == "")
                        {
                            // Check to make sure there's actually some text
                            if (Tooltip == "")
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            Callback(State, Name);
                            Global.editing = Name + "Input";
                            //Accessibility.allowAccessibility = false;
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            Global.mask.AddUnmaskedObject("TextEntry", new SimpleObject(scaledBounds, Color.Transparent, GlobalGraphics.pixel, () => {
                                // Check to make sure there's actually some text
                                if (Tooltip == "")
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return true;
                                }
                                Callback(State, Name);
                                Global.editing = "";
                                //Accessibility.allowAccessibility = true;
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                Global.mask.Disable();
                                return true;
                            }, true));
                            Global.mask.Enable();
                        }
                        else
                        {
                            // If editing is not in the name list, set it to ""
                            if (Global.editing != "")
                            {
                                //Global.editing = "";
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                // Check to make sure there's actually some text
                                if (Tooltip == "")
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return true;
                                }
                                Callback(State, Name);
                                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                Global.mask.Disable();
                            }
                        }
                        return true;
                    }
                }
                // If state is 1, we are entering text.
                if(Global.editing == Name + "Input")
                {
                    // Check for enter
                    if ((newKeyboardState.IsKeyDown(Keys.Enter) && !oldKeyboardState.IsKeyDown(Keys.Enter)) || (newKeyboardState.IsKeyDown(Keys.Escape) && !oldKeyboardState.IsKeyDown(Keys.Escape)))
                    {
                        // Check to make sure there's actually some text
                        if (Tooltip == "")
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        Callback(0, Name);
                        Global.editing = "";
                        Accessibility.allowAccessibility = true;
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    else
                    {
                        // If number entry, use arrows to change number
                        if (mode <= 2)
                        {
                            if (newKeyboardState.IsKeyDown(Keys.Up) && !oldKeyboardState.IsKeyDown(Keys.Up))
                            {
                                switch(mode)
                                {
                                    case 1: // numbers only (inc 1)
                                        int.TryParse(Tooltip, out int num);
                                        num += 1;
                                        Tooltip = num.ToString();
                                        break;
                                    case 2: // decimals (inc 0.01)
                                        float.TryParse(Tooltip, out float dec);
                                        dec += 0.01f;
                                        Tooltip = dec.ToString("0.00");
                                        // Remove 0s at end
                                        while (Tooltip.EndsWith("0"))
                                        {
                                            Tooltip = Tooltip.Remove(Tooltip.Length - 1);
                                        }
                                        break;
                                }
                            }
                            if (newKeyboardState.IsKeyDown(Keys.Down) && !oldKeyboardState.IsKeyDown(Keys.Down))
                            {
                                switch (mode)
                                {
                                    case 1: // numbers only (dec 1)
                                        int.TryParse(Tooltip, out int num);
                                        num -= 1;
                                        Tooltip = num.ToString();
                                        break;
                                    case 2: // decimals (dec 0.01)
                                        float.TryParse(Tooltip, out float dec);
                                        dec -= 0.01f;
                                        // round to 2 decimal places in string
                                        Tooltip = dec.ToString("0.00");
                                        // Remove 0s at end
                                        while (Tooltip.EndsWith("0"))
                                        {
                                            Tooltip = Tooltip.Remove(Tooltip.Length - 1);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the sides
            Texture2D side = GlobalContent.GetTexture("InteractiveTextEntrySide");
            Texture2D inner = GlobalContent.GetTexture("InteractiveTextEntryInner");
            // Shadows
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + 1), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), Color.Black);
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + 1 + bounds.Width - 1 + side.Width/2), GlobalGraphics.Scale(bounds.Y + 1 - 1 + side.Height/2), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), null, Color.Black, MathHelper.ToRadians(180), new Vector2(side.Width/2, side.Height/2), SpriteEffects.None, 0);
            spriteBatch.Draw(inner, new Rectangle(GlobalGraphics.Scale(bounds.X + 1+4), GlobalGraphics.Scale(bounds.Y + 1), GlobalGraphics.Scale(bounds.Width-5), GlobalGraphics.Scale(inner.Height)), Color.Black);
            // Normal
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), (Global.editing != Name + "Input") ? Color.White : Color.LightBlue);
            spriteBatch.Draw(side, new Rectangle(GlobalGraphics.Scale(bounds.X + bounds.Width - 1 + side.Width/2), GlobalGraphics.Scale(bounds.Y - 1 + side.Height/2), GlobalGraphics.Scale(side.Width), GlobalGraphics.Scale(side.Height)), null, (Global.editing != Name + "Input") ? Color.White : Color.LightBlue, MathHelper.ToRadians(180), new Vector2(side.Width/2, side.Height/2), SpriteEffects.None, 0);
            spriteBatch.Draw(inner, new Rectangle(GlobalGraphics.Scale(bounds.X+4), GlobalGraphics.Scale(bounds.Y), GlobalGraphics.Scale(bounds.Width-5), GlobalGraphics.Scale(inner.Height)), (Global.editing != Name + "Input") ? Color.White : Color.LightBlue);
            // Inner text
            try
            {
                spriteBatch.DrawString(GlobalGraphics.fontMunro, Tooltip, new Vector2(GlobalGraphics.Scale(bounds.X + 4 + 1), GlobalGraphics.Scale(bounds.Y + 1 + 1)), Color.Black);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, Tooltip, new Vector2(GlobalGraphics.Scale(bounds.X + 4), GlobalGraphics.Scale(bounds.Y + 1)), Color.White);
            }
            catch (ArgumentException)
            {
                // Remove invalid characters
                Tooltip = Tooltip.Remove(Tooltip.Length - 1);
            }
            // Draw cursor every 500ms
            if ((Global.editing == Name + "Input") && gameTime.TotalGameTime.TotalMilliseconds % 500 < 250)
            {
                int cursorX = (int)GlobalGraphics.fontMunro.MeasureString(Tooltip).X;
                // could have just used a line here
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4 + 1), GlobalGraphics.Scale(bounds.Y + 1 + 1)), Color.Black);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4 + 1), GlobalGraphics.Scale(bounds.Y + 1)), Color.Black);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4 + 1), GlobalGraphics.Scale(bounds.Y)), Color.Black);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4), GlobalGraphics.Scale(bounds.Y + 1)), Color.White);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4), GlobalGraphics.Scale(bounds.Y)), Color.White);
                spriteBatch.DrawString(GlobalGraphics.fontMunro, ":", new Vector2(cursorX + GlobalGraphics.Scale(bounds.X + 4), GlobalGraphics.Scale(bounds.Y - 1)), Color.White);
            }
            // Label
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + bounds.Width + 7 + 1), GlobalGraphics.Scale(bounds.Y + 2 + 1)), Color.Black);
            spriteBatch.DrawString(GlobalGraphics.fontMunro, Name, new Vector2(GlobalGraphics.Scale(bounds.X + bounds.Width + 7), GlobalGraphics.Scale(bounds.Y + 2)), Color.White);
            // Tooltip
            if (scaledBounds.Contains(MouseInput.MouseState.Position) && Tooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(hiddenToolTip);
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
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, hiddenToolTip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("InteractiveTextEntrySide", contentManager.Load<Texture2D>("graphics/interactivetextentryside"));
            GlobalContent.AddTexture("InteractiveTextEntryInner", contentManager.Load<Texture2D>("graphics/interactivetextentryinner"));
            Register();
        }
        public TextEntry Register()
        {
            GameWindow? window = UserInterface.instance?.Window;
            if (window != null)
            {
                window.TextInput += TextInput;
            }
            return this;
        }
        private bool ValidateInput(char character)
        {
            switch(mode)
            {
                case 1: // numbers only
                    return char.IsDigit(character);
                case 2: // numbers only + decimal point
                    return char.IsDigit(character) || character == '.';
                case 3: // letters only
                    return char.IsLetter(character);
                case 4: // letters + numbers
                    return char.IsLetterOrDigit(character);
                case 5: // letters + numbers + spaces
                    return char.IsLetterOrDigit(character) || character == ' ';
                default:
                    return true;
            }
        }
        // implementation of textinputeventargs
        private void TextInput(object? sender, TextInputEventArgs e)
        {
            if (Global.editing == Name + "Input")
            {
                if(e.Key == Keys.Tab || e.Key == Keys.Enter || e.Key == Keys.Escape)
                    return;
                if (e.Key == Keys.Back)
                {
                    if (Tooltip.Length > 0)
                    {
                        Tooltip = Tooltip.Substring(0, Tooltip.Length - 1);
                    }
                }
                else if(Tooltip.Length < maxChars && ValidateInput(e.Character))
                {
                    // If syn unicode character, paste from clipboard
                    if (e.Character == '\u0016')
                    {
                        string clipboard = Clipboard.GetText();
                        if (clipboard.Length + Tooltip.Length <= maxChars)
                        {
                            // Validate each character
                            foreach (char c in clipboard)
                            {
                                // no newlines
                                if (c == '\n')
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return;
                                }
                                // validate
                                if (!ValidateInput(c))
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return;
                                }
                            }
                            Tooltip += clipboard;
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    // \u0003 is copy
                    else if (e.Character == '\u0003')
                    {
                        Clipboard.SetText(Tooltip);
                    }
                    // \u0018 is cut
                    else if (e.Character == '\u0018')
                    {
                        Clipboard.SetText(Tooltip);
                        Tooltip = "";
                    }
                    // \u0001 is select all, assume delete all
                    else if (e.Character == '\u0001')
                    {
                        Tooltip = "";
                    }
                    // Undo is \u001A
                    else if (e.Character == '\u001A')
                    {
                        Tooltip = "";
                    }
                    // Redo is \u0019
                    else if (e.Character == '\u0019')
                    {
                        // Do nothing
                    }
                    else
                    {
                        Tooltip += e.Character;
                    }
                }
                else
                {
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                }
            }
        }
    }
}
#endif
