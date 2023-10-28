#if MONOGAME
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class PluginsPage : IPage
    {
        public string Name { get; set; } = "Addons";
        public string Tooltip { get; } = "Add, remove, or modify Workshop addons.";
        private int scrollOffset = 0;
        private int maxScrollOffset = 0;
        private bool dragging = false;
        private int dragOffset = 0;
        private bool editingSettings = false;
        private bool pluginCreation = false;
        private int settingsIndex = 0;
        private int setting = 0;
        private string internalTooltip = "";
        public string customPluginName = "Untitled";
        public string customPluginFileName = "untitled";
        public bool customPluginMinimal = false;
        public WorkshopTag selectedFlagsWorkshop = WorkshopTag.None;
        private readonly InteractableController controller = new();
        private readonly InteractableController controllerPluginCreation = new();
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // 135, 56 PluginPage
            // 136, 57 PluginEntry
            // 294, 69-214 ScrollHandle (9x9)
            Texture2D pluginPage = GlobalContent.GetTexture("PluginPage");
            Texture2D pluginEntry = GlobalContent.GetTexture("PluginEntry");
            Texture2D pluginEntryBlank = GlobalContent.GetTexture("PluginEntryBlank");
            Texture2D scrollHandle = GlobalContent.GetTexture("ScrollHandle");
            Texture2D interactiveSwitchOn = GlobalContent.GetTexture("InteractiveSwitchOn");
            Texture2D interactiveSwitchOff = GlobalContent.GetTexture("InteractiveSwitchOff");
            SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            if(!editingSettings)
            {
                // Draw scroll bar
                spriteBatch.Draw(pluginPage, new Rectangle(GlobalGraphics.Scale(293), GlobalGraphics.Scale(57), pluginPage.Width * GlobalGraphics.scale, pluginPage.Height * GlobalGraphics.scale), Color.White);
                // Move the scroll handle relative to the scroll offset and the max scroll offset.
                if(maxScrollOffset > 0)
                {
                    spriteBatch.Draw(scrollHandle, new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset), scrollHandle.Width * GlobalGraphics.scale, scrollHandle.Height * GlobalGraphics.scale), Color.White);
                }
                // End existing spritebatch
                spriteBatch.End();
                // Mask to specific area
                spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(293), GlobalGraphics.Scale(236)); 
                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.ScissorTestEnable = true;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Matrix.CreateTranslation(GlobalGraphics.Scale(ScreenManager.GetScreen<ContentScreen>("Content").offset.X / GlobalGraphics.scale), GlobalGraphics.Scale((ScreenManager.GetScreen<ContentScreen>("Content").offset.Y / GlobalGraphics.scale) + -scrollOffset), 0));
                int plcount = PluginHandler.GetPluginCount() + 1;
                for(int i = 0; i < plcount; i++)
                {
                    // Alternate colors so it's easier to see
                    Color curColor = new Color(i % 2 == 0 ? 255 : 192, i % 2 == 0 ? 255 : 192, i % 2 == 0 ? 255 : 192);
                    if(i < plcount-1)
                    {
                        // Different addon types have different colors
                        switch(PluginHandler.plugins[i].GetAddonType())
                        {
                            case AddonType.Effect: // blue 
                                //curColor = new Color(192, 192, 255);
                                curColor = new Color(i % 2 == 0 ? 192 : 128, i % 2 == 0 ? 192 : 128, i % 2 == 0 ? 255 : 192);
                                break;
                            case AddonType.PostRenderEffect: // green
                                //curColor = new Color(192, 255, 192);
                                curColor = new Color(i % 2 == 0 ? 128 : 64, i % 2 == 0 ? 255 : 192, i % 2 == 0 ? 128 : 64);
                                break;
                            case AddonType.Theme: // red
                                //curColor = new Color(255, 192, 192);
                                curColor = new Color(i % 2 == 0 ? 255 : 192, i % 2 == 0 ? 128 : 64, i % 2 == 0 ? 128 : 64);
                                break;
                        }
                        spriteBatch.Draw(pluginEntry, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + i * pluginEntry.Height + i), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), curColor);
                        spriteBatch.DrawString(munroSmall, PluginHandler.plugins[i].GetDisplayName(), new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + i * pluginEntry.Height + i)), Color.Black);
                        spriteBatch.DrawString(munroSmall, PluginHandler.plugins[i].GetDisplayName(), new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                        if(Global.canRender)
                        {
                            spriteBatch.Draw(PluginHandler.plugins[i].enabled ? interactiveSwitchOn : interactiveSwitchOff, new Rectangle(GlobalGraphics.Scale(271), GlobalGraphics.Scale(60 + i * pluginEntry.Height + i), interactiveSwitchOn.Width * GlobalGraphics.scale, interactiveSwitchOn.Height * GlobalGraphics.scale), Color.White);
                        }
                        else
                        {
                            spriteBatch.DrawString(munroSmall, "...", new Vector2(GlobalGraphics.Scale(277+1), GlobalGraphics.Scale(58 + 1 + i * pluginEntry.Height + i)), Color.Black);
                            spriteBatch.DrawString(munroSmall, "...", new Vector2(GlobalGraphics.Scale(277), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                        }
                    }
                    else
                    {
                        // Alternate colors so it's easier to see
                        spriteBatch.Draw(pluginEntryBlank, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + i * pluginEntry.Height + i), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), curColor);
                        // create plugin
                        spriteBatch.DrawString(munroSmall, "Addon Management", new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + i * pluginEntry.Height + i)), Color.Black);
                        spriteBatch.DrawString(munroSmall, "Addon Management", new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + i * pluginEntry.Height + i)), Color.White);
                    }
                }
                // End offset
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            else
            {
                // Draw background
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                if(pluginCreation)
                {
                    // Interactable
                    controllerPluginCreation.Draw(gameTime, spriteBatch);
                }
                else
                {
                    // Interactable
                    controller.Draw(gameTime, spriteBatch);
                }
            }
            if (internalTooltip != "")
            {
                // Get text size
                Vector2 tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(internalTooltip);
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
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, internalTooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            int plcount = PluginHandler.GetPluginCount() + 1;
            int tempMaxScrollOffset = plcount;
            tempMaxScrollOffset -= 11;
            if(tempMaxScrollOffset <= 0)
            {
                tempMaxScrollOffset = 0;
            }
            maxScrollOffset = tempMaxScrollOffset * 16;
            if(handleInput)
            {
                internalTooltip = "";
                if(!editingSettings)
                {
                    for (int i = 0; i < plcount; i++)
                    {
                        if(i < plcount-1)
                        {
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                internalTooltip = Global.canRender ?"Click to toggle addon." : "Loading...";
                            }
                            int inRange = 254;
                            if(!Global.canRender)
                            {
                                inRange = 267; // No settings button
                            }
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                // Is this a workshop plugin?
                                // If so, tooltip should be "Open workshop page."
                                // Otherwise, tooltip should be "Open plugin directory."
                                if (PluginHandler.plugins[i].workshopId != ""
                                    && PluginHandler.plugins[i].rootPath.Contains("workshop"))
                                    internalTooltip = "Open workshop page.";
                                else if(PluginHandler.plugins[i].workshopId != ""
                                    && PluginHandler.plugins[i].workshopId != "stock")
                                    internalTooltip = "Open in code editor.";
                                else
                                    internalTooltip = "Open stock directory.";
                            }
                            if(Global.canRender)
                            {
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(256) && MouseInput.MouseState.X < GlobalGraphics.Scale(267)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                {
                                    internalTooltip = "Edit settings.";
                                }
                            }
                        }
                        else
                        {
                            // create plugin
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                            {
                                internalTooltip = "Create or reload addons.";
                            }
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
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(303)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(78 + scrollOffset * (214 - 69) / maxScrollOffset))
                                {
                                    dragging = true;
                                    dragOffset = MouseInput.MouseState.Y - GlobalGraphics.Scale(69 + scrollOffset * (214 - 69) / maxScrollOffset);
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    return true;
                                }
                                // 293, 57, 11x11 Scroll Up
                                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                                {
                                    if(scrollOffset - 1 >= 0)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                                // 293, 224, 11x11 Scroll Down
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                                {
                                    if (scrollOffset + 1 <= maxScrollOffset)
                                    {
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (MouseInput.MouseState.LeftButton == ButtonState.Released)
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                dragging = false;
                            }
                            else
                            {
                                int newY = MouseInput.MouseState.Y - dragOffset;
                                if (newY >= GlobalGraphics.Scale(69) && newY <= GlobalGraphics.Scale(214))
                                {
                                    scrollOffset = (newY - GlobalGraphics.Scale(69)) * maxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
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
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            // 293, 224, 11x11 Scroll Down
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                        }
                        dragging = false;
                    }
                    // Accessibility
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(57), GlobalGraphics.Scale(11), GlobalGraphics.Scale(11)), "Scroll Up");
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(224), GlobalGraphics.Scale(11), GlobalGraphics.Scale(11)), "Scroll Down");
                    for (int i = 0; i < plcount; i++)
                    {
                        if(i < plcount-1)
                        {
                            int inRange = 254;
                            if(!Global.canRender)
                            {
                                inRange = 267; // No settings button
                            }
                            // Toggle Button
                            Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(269), GlobalGraphics.Scale(59 + (i * 16) - scrollOffset), GlobalGraphics.Scale(21), GlobalGraphics.Scale((70 + (i * 16) - scrollOffset) - (59 + (i * 16) - scrollOffset))), (PluginHandler.plugins[i].enabled ? "Disable " : "Enable ") + "\"" + PluginHandler.plugins[i].GetDisplayName() + "\"");
                            if(Global.canRender)
                            {
                                // Settings Button
                                Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(256), GlobalGraphics.Scale(59 + (i * 16) - scrollOffset), GlobalGraphics.Scale(11), GlobalGraphics.Scale((70 + (i * 16) - scrollOffset) - (59 + (i * 16) - scrollOffset))), "Settings for \"" + PluginHandler.plugins[i].GetDisplayName() + "\"");
                            }
                            // Main Button
                            Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(138), GlobalGraphics.Scale(59 + (i * 16) - scrollOffset), GlobalGraphics.Scale(inRange-138), GlobalGraphics.Scale((70 + (i * 16) - scrollOffset) - (59 + (i * 16) - scrollOffset))), "Open container for \"" + PluginHandler.plugins[i].GetDisplayName() + "\"");
                        }
                        else
                        {
                            // create plugin
                            Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(138), GlobalGraphics.Scale(59 + (i * 16) - scrollOffset), GlobalGraphics.Scale(152), GlobalGraphics.Scale((70 + (i * 16) - scrollOffset) - (59 + (i * 16) - scrollOffset))), "Addon Management (Create or reload addons.)");
                        }
                    }
                    if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        // Plugin entries
                        for (int i = 0; i < plcount; i++)
                        {
                            if(i < plcount-1)
                            {
                                // Toggle button
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                {
                                    if(Global.canRender)
                                    {
                                        PluginHandler.plugins[i].enabled = !PluginHandler.plugins[i].enabled;
                                        PluginHandler.SavePluginSettings();
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    else
                                    {
                                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    }
                                    return true;
                                }
                                // Open folder containing plugin
                                int inRange = 254;
                                if(!Global.canRender)
                                {
                                    inRange = 267; // No settings button
                                }
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                {
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    // Open directory and select file
                                    ProcessStartInfo startInfo = new();
                                    // Workshop plugin should open workshop page
                                    if(PluginHandler.plugins[i].workshopId != ""
                                        && PluginHandler.plugins[i].rootPath.Contains("workshop"))
                                    {
                                        startInfo.FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + PluginHandler.plugins[i].workshopId;
                                        startInfo.UseShellExecute = true;
                                        Process.Start(startInfo);
                                    }
                                    else if(PluginHandler.plugins[i].workshopId != ""
                                        && PluginHandler.plugins[i].workshopId != "stock")
                                    {
                                        try
                                        {
                                            startInfo.FileName = "code";
                                            startInfo.Arguments = "\"" + Path.GetFullPath(PluginHandler.plugins[i].path) + "\"";
                                            startInfo.UseShellExecute = true;
                                            startInfo.CreateNoWindow = true;
                                            Process.Start(startInfo);
                                        }
                                        catch
                                        {
                                            startInfo.FileName = "explorer.exe";
                                            startInfo.Arguments = "/select, \"" + Path.GetFullPath(PluginHandler.plugins[i].path) + "\"";
                                            Process.Start(startInfo);
                                        }
                                    }
                                    else
                                    {
                                        startInfo.FileName = "explorer.exe";
                                        startInfo.Arguments = "/select, \"" + Path.GetFullPath(PluginHandler.plugins[i].path) + "\"";
                                        Process.Start(startInfo);
                                    }
                                    return true;
                                }
                                // Settings button
                                if(Global.canRender)
                                {
                                    if (MouseInput.MouseState.X >= GlobalGraphics.Scale(256) && MouseInput.MouseState.X < GlobalGraphics.Scale(267)
                                        && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                    {
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        // Open settings
                                        controller.Clear();
                                        settingsIndex = i;
                                        editingSettings = true;
                                        if(PluginHandler.plugins[i].workshopId != ""
                                            && PluginHandler.plugins[i].workshopId != "stock")
                                        {
                                            if(PluginHandler.plugins[i].submittedId != "")
                                            {
                                                controller.Add("VisitWorkshopPage", new Button("View Workshop", "View this effect on the Workshop.", new Vector2(119+100, 60+10+19*7), (int i) => {
                                                    switch(i)
                                                    {
                                                        case 2:
                                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            ProcessStartInfo startInfo = new();
                                                            startInfo.FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + PluginHandler.plugins[settingsIndex].submittedId;
                                                            startInfo.UseShellExecute = true;
                                                            Process.Start(startInfo);
                                                            return true;
                                                    }
                                                    return false;
                                                }));
                                            }
                                            if(PluginHandler.plugins[i].rootPath.Contains("user"))
                                            {
                                                controller.Add("Delete", new Button("Delete", "", new Vector2(119+100, 60+10+19*8), (int i) => {
                                                    switch(i)
                                                    {
                                                        case 2:
                                                            // Delete and reload plugins
                                                            try
                                                            {
                                                                Directory.Delete(Path.GetDirectoryName(PluginHandler.plugins[settingsIndex].path), true);
                                                                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                if(SteamManager.initialized)
                                                                    PluginHandler.LoadWorkshop();
                                                                else
                                                                    PluginHandler.LoadPluginsThreaded();
                                                                editingSettings = false;
                                                            }
                                                            catch
                                                            {
                                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            }
                                                            return true;
                                                    }
                                                    return false;
                                                }));
                                                controller.Add("Publish", new Button("Publish", "Publish to Steam Workshop.", new Vector2(139+139, 60+10+19*8), (int i) => {
                                                    switch(i)
                                                    {
                                                        case 2: // left click
                                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            Name = "Workshop Tags: " + PluginHandler.plugins[settingsIndex].GetDisplayName();
                                                            controller.Clear();
                                                            controller.Add("LibraryCustom", new Switch("Custom Library", "Uses a custom library type.", new Vector2(139, 60+19*8), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Custom;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Custom;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Custom) != 0));
                                                            controller.Add("LibraryOverlay", new Switch("Overlay Library", "Uses the overlay library type.", new Vector2(139, 60+19*7), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Overlay;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Overlay;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Overlay) != 0));
                                                            controller.Add("LibraryOutro", new Switch("Outro Library", "Uses the outro library type.", new Vector2(139, 60+19*6), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Outro;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Outro;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Outro) != 0));
                                                            controller.Add("LibraryIntro", new Switch("Intro Library", "Uses the intro library type.", new Vector2(139, 60+19*5), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Intro;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Intro;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Intro) != 0));
                                                            controller.Add("LibraryTransition", new Switch("Transition Library", "Uses the transition library type.", new Vector2(139, 60+19*4), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Transition;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Transition;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Transition) != 0));
                                                            controller.Add("LibraryMaterial", new Switch("Material Library", "Uses the material library type.", new Vector2(139, 60+19*3), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Material;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Material;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Material) != 0));
                                                            controller.Add("LibraryMusic", new Switch("Music Library", "Uses the music library type.", new Vector2(139, 60+19*2), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_Music;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_Music;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_Music) != 0));
                                                            controller.Add("LibrarySFX", new Switch("Sound FX Library", "Uses the sound fx library type.", new Vector2(139, 60+19), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Library_SFX;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Library_SFX;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Library_SFX) != 0));
                                                            controller.Add("Audio", new Switch("Audio", "Applies audio effects.", new Vector2(139+55, 60), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Effect_AudioOnly;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Effect_AudioOnly;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Effect_AudioOnly) != 0));
                                                            controller.Add("Video", new Switch("Video", "Applies video effects.", new Vector2(139, 60), (int i) => {
                                                                bool switchState = (i & 256) != 0;
                                                                if((i & 2) != 0)
                                                                {
                                                                    // add or remove WorkshopTag.Video to flags
                                                                    if(switchState)
                                                                        selectedFlagsWorkshop |= WorkshopTag.Effect_VideoOnly;
                                                                    else
                                                                        selectedFlagsWorkshop &= ~WorkshopTag.Effect_VideoOnly;
                                                                }
                                                                return switchState;
                                                            }, (selectedFlagsWorkshop & WorkshopTag.Effect_VideoOnly) != 0));
                                                            controller.Add("Submit", new Button("Submit", "Submit to Steam Workshop.", new Vector2(139+139, 60+10+19*7), (int i) => {
                                                                switch(i)
                                                                {
                                                                    case 2: // left click
                                                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                        // Select png, jpg, or gif icon with file dialog
                                                                        System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                                                                        fileDialog.Filter = "Joint Photographic Experts Group (*.jpg, *.jpeg)|*.jpg;*.jpeg";
                                                                        fileDialog.Title = "Select Workshop Icon";
                                                                        fileDialog.InitialDirectory = Path.GetFullPath(@"templates");
                                                                        fileDialog.FileName = "workshop.jpg";
                                                                        if(fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                                        {
                                                                            Name = "Addons";
                                                                            editingSettings = false;
                                                                            controller.Clear();
                                                                            Global.generator.progressText = "Uploading...";
                                                                            ConsoleOutput.WriteLine("Publishing " + Path.GetFileName(PluginHandler.plugins[settingsIndex].path) + " with icon " + Path.GetFileName(fileDialog.FileName), Color.RoyalBlue);
                                                                            PluginHandler.PublishPlugin(PluginHandler.plugins[settingsIndex], selectedFlagsWorkshop, fileDialog.FileName);
                                                                        }
                                                                        return true;
                                                                }
                                                                return false;
                                                            }));
                                                            controller.Add("Back", new Button("Back", "Go back to effect addon list.", new Vector2(119+167, 60+10+19*8), (int i) => {
                                                                switch(i)
                                                                {
                                                                    case 2: // left click
                                                                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                        controller.Clear();
                                                                        Name = "Addons";
                                                                        editingSettings = false;
                                                                        return true;
                                                                }
                                                                return false;
                                                            }));
                                                            return true;
                                                    }
                                                    return false;
                                                }));
                                            }
                                        }
                                        controller.Add("Back", new Button("Back", "Go back to addon list.", new Vector2(119+36, 60+10+19*8), (int i) => {
                                            switch(i)
                                            {
                                                case 2: // left click
                                                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                    controller.Clear();
                                                    Name = "Addons";
                                                    editingSettings = false;
                                                    return true;
                                            }
                                            return false;
                                        }));
                                        Name = PluginHandler.plugins[i].GetDisplayName();
                                        int sindex = 0;
                                        List<IInteractable> tes = new();
                                        foreach(KeyValuePair<string, object> s in PluginHandler.plugins[i].settings)
                                        {
                                            // if s is Display Name, skip
                                            if(s.Key == "Addon Type")
                                                continue;
                                            if(s.Key == "Display Name")
                                                continue;
                                            if(!PluginHandler.plugins[i].settingTypes.Keys.Contains(s.Key))
                                                continue;
                                            SettingType type = PluginHandler.plugins[i].settingTypes[s.Key];
                                            IInteractable te;
                                            int ty = 6;
                                            switch(type)
                                            {
                                                case SettingType.TextInputNumbers:
                                                    ty = 1;
                                                    goto default;
                                                case SettingType.TextInputDecimals:
                                                    ty = 2;
                                                    goto default;
                                                case SettingType.TextInputLetters:
                                                    ty = 3;
                                                    goto default;
                                                case SettingType.TextInputLettersNumbers:
                                                    ty = 4;
                                                    goto default;
                                                case SettingType.TextInputLettersNumbersSpaces:
                                                    ty = 5;
                                                    goto default;
                                                case SettingType.Label:
                                                    te = new Label((string)PluginHandler.plugins[i].settings[s.Key], new Vector2(139, 60+4+19*sindex));
                                                    break;
                                                case SettingType.Switch:
                                                    te = new Switch(s.Key, PluginHandler.plugins[i].settingTooltips[s.Key], new Vector2(139, 60+19*sindex), (int i) => {
                                                        bool switchState = (i & 256) != 0;
                                                        if((i & 1) != 0)
                                                        {
                                                            // Set "setting" to the setting index where mouse cursor y is
                                                            int y = MouseInput.MouseState.Y / GlobalGraphics.scale;
                                                            Rectangle source = new(139, 70, 25, 15);
                                                            int separator = 4;
                                                            setting = (y - source.Y - separator) / (source.Height + separator);
                                                            if(setting < 0)
                                                                setting = 0;
                                                            while(PluginHandler.plugins[settingsIndex].settingTypes.ElementAt(setting).Value == SettingType.Label)
                                                                setting++;
                                                            if(setting >= PluginHandler.plugins[settingsIndex].settings.Count)
                                                                setting = PluginHandler.plugins[settingsIndex].settings.Count - 1;
                                                        }
                                                        if((i & 2) != 0)
                                                        {
                                                            // Use "setting" to get the key of the setting
                                                            string keyFromIndex = PluginHandler.plugins[settingsIndex].settings.Keys.ElementAt(setting);
                                                            string oldValue = PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString();
                                                            PluginHandler.plugins[settingsIndex].settings[keyFromIndex] = switchState ? "1" : "0";
                                                            if(oldValue != PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString())
                                                                PluginHandler.SavePluginSettings();
                                                            else
                                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        }
                                                        return switchState;
                                                    }, PluginHandler.plugins[i].settings[s.Key].ToString() == "1");
                                                    break;
                                                case SettingType.TextInput:
                                                default:
                                                    te = new TextEntry(s.Key, PluginHandler.plugins[i].settingTooltips[s.Key], PluginHandler.plugins[i].settings[s.Key].ToString(), new Vector2(139, 60+19*sindex), 50, 25, ty, (int i) => {
                                                        if(i == 0)
                                                        {
                                                            // Set "setting" to the setting index where mouse cursor y is
                                                            int y = MouseInput.MouseState.Y / GlobalGraphics.scale;
                                                            Rectangle source = new(139, 60, 53, 15);
                                                            int separator = 4;
                                                            setting = (y - source.Y - separator) / (source.Height + separator);
                                                            if(setting < 0)
                                                                setting = 0;
                                                            while(PluginHandler.plugins[settingsIndex].settingTypes.ElementAt(setting).Value == SettingType.Label)
                                                                setting++;
                                                            if(setting >= PluginHandler.plugins[settingsIndex].settings.Count)
                                                                setting = PluginHandler.plugins[settingsIndex].settings.Count - 1;
                                                            // Use "setting" to get the key of the setting
                                                            string keyFromIndex = PluginHandler.plugins[settingsIndex].settings.Keys.ElementAt(setting);
                                                            string oldValue = PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString();
                                                            if(!controller.interactables.ContainsKey(keyFromIndex))
                                                            {
                                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                return true;
                                                            }
                                                            PluginHandler.plugins[settingsIndex].settings[keyFromIndex] = controller.interactables[keyFromIndex].Tooltip;
                                                            if(oldValue != PluginHandler.plugins[settingsIndex].settings[keyFromIndex].ToString())
                                                            {
                                                                PluginHandler.SavePluginSettings();
                                                                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            }
                                                        }
                                                        return false;
                                                    }).Register();
                                                    break;
                                            }
                                            tes.Add(te);
                                            sindex++;
                                        }
                                        for(int i2 = tes.Count-1; i2 >= 0; i2--)
                                        {
                                            // Get name
                                            string name = tes[i2].Name;
                                            controller.Add(name, tes[i2]);
                                        }
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                // create plugin
                                if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + (i * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + (i * 16) - scrollOffset))
                                {
                                    if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                                    {
                                        pluginCreation = true;
                                        editingSettings = true;
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                else if(pluginCreation)
                {
                    // Interactable
                    if(controllerPluginCreation.Update(gameTime, handleInput))
                        return true;
                }
                else
                {
                    // Interactable
                    if(controller.Update(gameTime, handleInput))
                        return true;
                }
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("PluginPage", contentManager.Load<Texture2D>("graphics/pluginpage"));
            GlobalContent.AddTexture("PluginEntryBlank", contentManager.Load<Texture2D>("graphics/pluginentryblank"));
            GlobalContent.AddTexture("PluginEntry", contentManager.Load<Texture2D>("graphics/pluginentry"));
            GlobalContent.AddTexture("ScrollHandle", contentManager.Load<Texture2D>("graphics/scrollhandle"));
            controllerPluginCreation.Add("PluginMinimal", new Switch("Minimal Template", "Use a minimal Lua file template.", new Vector2(139, 60+19*2), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    customPluginMinimal = switchState;
                }
                return switchState;
            }, customPluginMinimal));
            controllerPluginCreation.Add("PluginName", new TextEntry("Addon File Name", "The internal file name of the addon.", customPluginFileName, new Vector2(139, 60+19), 50, 25, 4, (int i) => {
                controllerPluginCreation.interactables["PluginName"].Tooltip = controllerPluginCreation.interactables["PluginName"].Tooltip.ToLower();
                customPluginFileName = controllerPluginCreation.interactables["PluginName"].Tooltip;
                return false;
            }));
            controllerPluginCreation.Add("PluginPrettyName", new TextEntry("Addon Name", "The human-readable name of the addon.", customPluginName, new Vector2(139, 60), 50, 25, 5, (int i) => {
                customPluginName = controllerPluginCreation.interactables["PluginPrettyName"].Tooltip;
                return false;
            }));
            controllerPluginCreation.Add("Back", new Button("Back", "Go back.", new Vector2(119+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        editingSettings = false;
                        pluginCreation = false;
                        return true;
                }
                return false;
            }));
            controllerPluginCreation.Add("ReloadPlugins", new Button("Reload All", "Force reload all addons.", new Vector2(119+100, 60+10+19*7), (int i) => {
                switch(i)
                {
                    case 2:
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(SteamManager.initialized)
                            PluginHandler.LoadWorkshop();
                        else
                            PluginHandler.LoadPluginsThreaded();
                        return true;
                }
                return false;
            }));
            controllerPluginCreation.Add("CreatePlugin", new Button("Create", "Create the addon as specified.", new Vector2(139+141, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2:
                        pluginCreation = false;
                        if(PluginHandler.CreatePlugin(customPluginFileName, customPluginName, customPluginMinimal, out string file))
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            // load plugins
                            if(SteamManager.initialized)
                                PluginHandler.LoadWorkshop();
                            else
                                PluginHandler.LoadPluginsThreaded();
                            editingSettings = false;
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            editingSettings = false;
                        }
                        return true;
                }
                return false;
            }));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
            controllerPluginCreation.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
