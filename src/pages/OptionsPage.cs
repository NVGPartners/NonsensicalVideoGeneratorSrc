#if MONOGAME
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class OptionsPage : IPage
    {
        public string Name { get; set; } = "PageOptions";
        public string Tooltip { get; } = "Change application settings.";
        private readonly InteractableController actionController = new();
        private readonly InteractableController actionController2 = new();
        private readonly InteractableController controller = new();
        public bool viewingLocalizations = false;
        private int scrollOffset = 0;
        private int maxScrollOffset = 0;
        private bool dragging = false;
        private int dragOffset = 0;
        private string internalTooltip = "";
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Global.selectLanguage)
            {
                Global.selectLanguage = false;
                LocaleAction(2, "");
            }
            // Interactable
            if(viewingLocalizations)
            {
                if(actionController2.Update(gameTime, handleInput))
                    return true;
                int plcount = L.locales.Count - 1;
                int offsetpl = 0;
                int tempMaxScrollOffset = plcount - offsetpl;
                tempMaxScrollOffset -= 11; // 11 entries fit on the screen
                if(tempMaxScrollOffset <= 0)
                    tempMaxScrollOffset = 0;
                maxScrollOffset = tempMaxScrollOffset * 16;
                offsetpl = 0;
                if(handleInput || dragging)
                {
                    internalTooltip = "";
                    for (int i = 0; i < plcount; i++)
                    {
                        int inRange = 290;
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                        {
                            // Capitalize first letter of L.locales[i+1].name
                            string properLocaleName = L.locales[i+1].name.Substring(0, 1).ToUpper() + L.locales[i+1].name.Substring(1);
                            internalTooltip = properLocaleName + " (" + Math.Round(L.locales[i+1].percentageComplete * 100) + "%)";
                        }
                    }
                    if(maxScrollOffset > 0)
                    {
                        if (MouseInput.MouseState.ScrollWheelValue != MouseInput.LastMouseState.ScrollWheelValue)
                        {
                            // 120 is the scroll wheel value for one scroll.
                            // One entry is one offset.
                            int lines = (MouseInput.MouseState.ScrollWheelValue - MouseInput.LastMouseState.ScrollWheelValue) / 120;
                            int oldScrollOffset = scrollOffset;
                            // Round up or down scroll offset to the nearest 16 multiple.
                            scrollOffset = (int)Math.Round(scrollOffset / 16.0) * 16;
                            // If it's the same
                            if (scrollOffset == oldScrollOffset)
                            {
                                scrollOffset -= lines * 16;
                            }
                            // If it's less than 0, set it to 0.
                            if (scrollOffset < 0)
                            {
                                scrollOffset = 0;
                            }
                            // If it's more than the max scroll offset, set it to the max scroll offset.
                            if (scrollOffset > maxScrollOffset)
                            {
                                scrollOffset = maxScrollOffset;
                            }
                        }
                        // Scroll handle
                        if(!dragging)
                        {
                            if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                            {
                                // directly on handle (start dragging)
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(303)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(78 + scrollOffset * (214 - 69) / maxScrollOffset))
                                {
                                    dragging = true;
                                    dragOffset = MouseInput.MouseState.Y - GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset);
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return true;
                                }
                                // on scroll bar empty space (move center of handle to there)
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294+3) && MouseInput.MouseState.X < GlobalGraphics.Scale(303-3)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69+4) && MouseInput.MouseState.Y < GlobalGraphics.Scale(223-4))
                                {
                                    scrollOffset = (MouseInput.MouseState.Y - GlobalGraphics.Scale(69) - GlobalGraphics.Scale(4)) * maxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return true;
                                }
                                // 293, 57, 11x11 Scroll Up
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                                {
                                    if(scrollOffset - 1 >= 0)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        int oldScrollOffset = scrollOffset;
                                        // Round down scroll offset to the nearest 16 multiple.
                                        scrollOffset = (int)(Math.Floor((double)scrollOffset / 16) * 16);
                                        // If it's the same, subtract 16.
                                        if (scrollOffset == oldScrollOffset)
                                        {
                                            scrollOffset -= 16;
                                        }
                                        // If it's less than 0, set it to 0.
                                        if (scrollOffset < 0)
                                        {
                                            scrollOffset = 0;
                                        }
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                                // 293, 224, 11x11 Scroll Down
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                                {
                                    if (scrollOffset + 1 <= maxScrollOffset)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        int oldScrollOffset = scrollOffset;
                                        // Round up scroll offset to the nearest 16 multiple.
                                        scrollOffset = (int)(Math.Ceiling((double)scrollOffset / 16) * 16);
                                        // If it's the same, add 16.
                                        if (scrollOffset == oldScrollOffset)
                                        {
                                            scrollOffset += 16;
                                        }
                                        // If it's more than the max, set it to the max.
                                        if (scrollOffset > maxScrollOffset)
                                        {
                                            scrollOffset = maxScrollOffset;
                                        }
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (MouseInput.MouseState.LeftButton == ButtonState.Released)
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                dragging = false;
                            }
                            else
                            {
                                int newY = MouseInput.MouseState.Y - dragOffset;
                                if (newY >= GlobalGraphics.Scale(69) && newY <= GlobalGraphics.Scale(214))
                                {
                                    scrollOffset = (newY - GlobalGraphics.Scale(69)) * maxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
                                }
                                else if (newY < GlobalGraphics.Scale(69))
                                {
                                    scrollOffset = 0;
                                }
                                else if (newY > GlobalGraphics.Scale(214))
                                {
                                    scrollOffset = maxScrollOffset;
                                }
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                        {
                            // 293, 57, 11x11 Scroll Up
                            if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                            {
                                // Set to 0 because this is the top.
                                scrollOffset = 0;
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            // 293, 224, 11x11 Scroll Down
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                            {
                                // Set to max because this is the bottom.
                                scrollOffset = maxScrollOffset;
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                        }
                        dragging = false;
                    }
                    // Accessibility
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(57), GlobalGraphics.Scale(11), GlobalGraphics.Scale(11)), "Scroll Up");
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(224), GlobalGraphics.Scale(11), GlobalGraphics.Scale(11)), "Scroll Down");
                    offsetpl = 0;
                    for (int i = 0; i < plcount; i++)
                    {
                        int inRange = 290;
                        // Main Button
                        Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(138), GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset), GlobalGraphics.Scale(inRange-138), GlobalGraphics.Scale((70 + ((i-offsetpl) * 16) - scrollOffset) - (59 + ((i-offsetpl) * 16) - scrollOffset))), L.T(0, "Accessibility:AddonsOpenContainer", L.locales[i+1].name));
                    }
                    if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        offsetpl = 0;
                        // Plugin entries
                        for (int i = 0; i < plcount; i++)
                        {
                            // Open folder containing plugin
                            int inRange = 290;
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                L.LoadLocale(L.locales[i+1].name);
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if(actionController.Update(gameTime, handleInput))
                    return true;
                if(controller.Update(gameTime, handleInput))
                    return true;
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(viewingLocalizations)
            {
                actionController2.Draw(gameTime, spriteBatch);
                // 135, 56 PluginPage
                // 136, 57 PluginEntry
                // 294, 69-214 ScrollHandle (9x9)
                Texture2D pluginPage = GlobalContent.GetTexture("PluginPage");
                Texture2D pluginEntry = GlobalContent.GetTexture("PluginEntryBlank");
                Texture2D scrollHandle = GlobalContent.GetTexture("ScrollHandle");
                // Draw scroll bar
                spriteBatch.Draw(pluginPage, new Rectangle(GlobalGraphics.Scale(293), GlobalGraphics.Scale(57), pluginPage.Width * GlobalGraphics.scale, pluginPage.Height * GlobalGraphics.scale), Color.White);
                // Move the scroll handle relative to the scroll offset and the max scroll offset.
                if(maxScrollOffset > 0)
                {
                    spriteBatch.Draw(scrollHandle, new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset), scrollHandle.Width * GlobalGraphics.scale, scrollHandle.Height * GlobalGraphics.scale), Color.White);
                }
                // End existing spritebatch
                ContentScreen? cntscr = ScreenManager.GetScreen<ContentScreen>("Content");
                if(cntscr != null)
                {
                    spriteBatch.End();
                    // Mask to specific area
                    spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(293), GlobalGraphics.Scale(236)); 
                    RasterizerState rasterizerState = new RasterizerState();
                    rasterizerState.ScissorTestEnable = true;
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Matrix.CreateTranslation(GlobalGraphics.Scale(Global.drawOffset.X)+GlobalGraphics.Scale(cntscr.offset.X / GlobalGraphics.scale), GlobalGraphics.Scale(Global.drawOffset.Y)+GlobalGraphics.Scale((cntscr.offset.Y / GlobalGraphics.scale) + -scrollOffset), 0));
                }
                int plcount = L.locales.Count - 1;
                int offsetpl = 0;
                for(int i = 0; i < plcount; i++)
                {
                    Color curColor;
                    // Alternate colors so it's easier to see
                    if ((i - offsetpl) % 2 != 0)
                        curColor = ThemeManager.GetColor("PluginEntryGenericAltPluginsPage");
                    else
                        curColor = ThemeManager.GetColor("PluginEntryGenericPluginsPage");

                    // Show selected locale
                    if (i+1 == L.localeIndex)
                    {
                        curColor = ThemeManager.GetColor("PluginEntryPostRenderEffectPluginsPage");
                        if((i -  offsetpl) % 2 != 0)
                            curColor = ThemeManager.GetColor("PluginEntryPostRenderEffectAltPluginsPage");
                    }

                    // Draw the plugin entry
                    spriteBatch.Draw(pluginEntry, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl)), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), curColor);
                            
                    // Set up the plugin name
                    string nam = L.locales[i+1].localizedName;
                    string localeFontName = L.locales[i+1].fontSmall;
                    SpriteFont localeFont = L.FontSmall();

                    if (GlobalContent.CheckFont(localeFontName))
                    {
                        localeFont = GlobalContent.GetFont(localeFontName);
                    }

                    GlobalContent.DrawString(spriteBatch, localeFont, nam, new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.Black);
                    GlobalContent.DrawString(spriteBatch, localeFont, nam, new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.White);
                }
                // End offset
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(Global.drawOffset.X), GlobalGraphics.Scale(Global.drawOffset.Y), 0));
                if (internalTooltip != "")
                {
                    Global.tooltip = internalTooltip;
                }
            }
            else
            {
                actionController.Draw(gameTime, spriteBatch);
                controller.Draw(gameTime, spriteBatch);
            }
        }
        public bool LocaleAction(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    viewingLocalizations = !viewingLocalizations;
                    if(viewingLocalizations)
                        Name = "PageLocales";
                    else
                        Name = "PageOptions";
                    DiscordRPC.curtab = DiscordRPC.ToTab(Name);
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Clear all controllers
            actionController.Clear();
            actionController2.Clear();
            controller.Clear();
            // Add dials
            actionController.Add("ActionReset", new ActionButton("Reset to default parameters.", new Vector2(112, 206), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        SaveData.saveValues["MusicVolume"] = "50";
                        SaveData.saveValues["SoundEffectVolume"] = "100";
                        SaveData.saveValues["VideoVolume"] = "100";
                        SaveData.saveValues["DisableMotion"] = "false";
                        SaveData.saveValues["EnableDiscordRPC"] = "true";
                        SaveData.saveValues["MuteMusicWhileTabbedOut"] = "true";
                        SaveData.saveValues["VideoPlaybackScale"] = "2";
                        SaveData.saveValues["SkipPhotosensitiveWarningScreen"] = "false";
                        SaveData.Save();
                        controller.interactables["MusicVolume"].Tooltip = SaveData.saveValues["MusicVolume"];
                        controller.interactables["SFXVolume"].Tooltip = SaveData.saveValues["SoundEffectVolume"];
                        controller.interactables["VideoVolume"].Tooltip = SaveData.saveValues["VideoVolume"];
                        ((Switch)controller.interactables["MotionDisable"]).SwitchState = SaveData.saveValues["DisableMotion"] == "true";
                        ((Switch)controller.interactables["EnableDiscordRPC"]).SwitchState = SaveData.saveValues["EnableDiscordRPC"] == "true";
                        ((Switch)controller.interactables["MuteMusicWhileTabbedOut"]).SwitchState = SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true";
                        controller.interactables["VideoPlaybackScale"].Tooltip = SaveData.saveValues["VideoPlaybackScale"];
                        ((Switch)controller.interactables["SkipPhotosensitiveWarningScreen"]).SwitchState = SaveData.saveValues["SkipPhotosensitiveWarningScreen"] == "true";
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/reset")));
            actionController.Add("ActionLocales", new ActionButton("View localizations to select a different language.", new Vector2(112, 221), LocaleAction, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/locales")));
            actionController2.Add("ActionReset", new ActionButton("Reset to default parameters.", new Vector2(112, 206), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        SaveData.saveValues["Locale"] = "english";
                        SaveData.Save();
                        L.LoadLocale("english");
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/reset")));
            actionController2.Add("ActionOptions", new ActionButton("View localizations to select a different language.", new Vector2(112, 221), LocaleAction, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/options")));
            controller.Add("SkipPhotosensitiveWarningScreen", new Switch("Skip Intro", "Load the menu immediately when starting.", new Vector2(139, 60+19*8), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["SkipPhotosensitiveWarningScreen"];
                    SaveData.saveValues["SkipPhotosensitiveWarningScreen"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["SkipPhotosensitiveWarningScreen"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["SkipPhotosensitiveWarningScreen"] == "true"));
            controller.Add("VideoPlaybackScale", new TextEntry("Video Playback Resolution", "The screen scale multiplier for video playback.", SaveData.saveValues["VideoPlaybackScale"], new Vector2(139, 60+19*7), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["VideoPlaybackScale"], CultureInfo.InvariantCulture);
                // Range: 1-4
                if(int.Parse(controller.interactables["VideoPlaybackScale"].Tooltip, CultureInfo.InvariantCulture) < 1)
                    controller.interactables["VideoPlaybackScale"].Tooltip = "1";
                if(int.Parse(controller.interactables["VideoPlaybackScale"].Tooltip, CultureInfo.InvariantCulture) > 4)
                    controller.interactables["VideoPlaybackScale"].Tooltip = "4";
                SaveData.saveValues["VideoPlaybackScale"] = controller.interactables["VideoPlaybackScale"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["VideoPlaybackScale"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            controller.Add("MuteMusicWhileTabbedOut", new Switch("Mute Music While Inactive", "Don't play music while " + Global.productNameShort + " is in the background.", new Vector2(139, 60+19*6), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["MuteMusicWhileTabbedOut"];
                    SaveData.saveValues["MuteMusicWhileTabbedOut"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["MuteMusicWhileTabbedOut"])
                        SaveData.Save();
                    if(switchState)
                        DiscordRPC.Initialize();
                    else
                        DiscordRPC.Shutdown();
                }
                return switchState;
            }, SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true"));
            controller.Add("EnableDiscordRPC", new Switch("Enable Discord RPC", "Tell others that you're using " + Global.productNameShort + ".", new Vector2(139, 60+19*5), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["EnableDiscordRPC"];
                    SaveData.saveValues["EnableDiscordRPC"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["EnableDiscordRPC"])
                        SaveData.Save();
                    if(switchState)
                        DiscordRPC.Initialize();
                    else
                        DiscordRPC.Shutdown();
                }
                return switchState;
            }, SaveData.saveValues["EnableDiscordRPC"] == "true"));
            controller.Add("MotionDisable", new Switch("Disable Motion", "Turns off screen tweening and other elements.", new Vector2(139, 60+19*4), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["DisableMotion"];
                    SaveData.saveValues["DisableMotion"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["DisableMotion"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["DisableMotion"] == "true"));
            controller.Add("Scale", new TextEntry("Screen Resolution", "The screen scale multiplier for the UI. Restarts when set.", SaveData.saveValues["ScreenScale"], new Vector2(139, 60+19*3), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["ScreenScale"], CultureInfo.InvariantCulture);
                // Range: 1-4
                if(int.Parse(controller.interactables["Scale"].Tooltip, CultureInfo.InvariantCulture) < 1)
                    controller.interactables["Scale"].Tooltip = "1";
                if(int.Parse(controller.interactables["Scale"].Tooltip, CultureInfo.InvariantCulture) > 4)
                    controller.interactables["Scale"].Tooltip = "4";
                SaveData.saveValues["ScreenScale"] = controller.interactables["Scale"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["ScreenScale"], CultureInfo.InvariantCulture))
                {
                    SaveData.Save();
                    // Restart software through steam
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "steam://run/" + Global.appId.ToString();
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "open";
                    Process.Start(startInfo);
                    // Close software
                    try
                    {
                        SteamAPI.Shutdown();
                    } catch {}
                    if(UserInterface.instance != null)
                        UserInterface.instance.Exit();
                }
                return false;
            }));
            controller.Add("VideoVolume", new TextEntry("Media Playback Volume", "In-app media volume level, from 0-100.", SaveData.saveValues["VideoVolume"], new Vector2(139, 60+19*2), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["VideoVolume"];
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    controller.interactables["VideoVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["VideoVolume"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    controller.interactables["VideoVolume"].Tooltip = "100";
                SaveData.saveValues["VideoVolume"] = controller.interactables["VideoVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("SFXVolume", new TextEntry("Sound Effect Volume", "Sound effect volume level, from 0-100.", SaveData.saveValues["SoundEffectVolume"], new Vector2(139, 60+19), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["SoundEffectVolume"];
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    controller.interactables["SFXVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["SFXVolume"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    controller.interactables["SFXVolume"].Tooltip = "100";
                SaveData.saveValues["SoundEffectVolume"] = controller.interactables["SFXVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["SoundEffectVolume"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MusicVolume", new TextEntry("Music Volume", "Background music volume level, from 0-100.", SaveData.saveValues["MusicVolume"], new Vector2(139, 60), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["MusicVolume"];
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    controller.interactables["MusicVolume"].Tooltip = "0";
                if(int.Parse(controller.interactables["MusicVolume"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    controller.interactables["MusicVolume"].Tooltip = "100";
                SaveData.saveValues["MusicVolume"] = controller.interactables["MusicVolume"].Tooltip;
                if(oldValue != SaveData.saveValues["MusicVolume"])
                    SaveData.Save();
                return false;
            }));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
