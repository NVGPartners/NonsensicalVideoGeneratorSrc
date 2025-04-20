#if MONOGAME
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        public string Name { get; set; } = "PageAddons";
        public string Tooltip { get; } = "Add, remove, or modify Workshop addons.";
        private int scrollOffset = 0;
        private int maxScrollOffset = 0;
        private bool dragging = false;
        private int dragOffset = 0;
        private bool editingSettings = false;
        private bool pluginCreation = false;
        private int settingsIndex = 0;
        private string internalTooltip = "";
        public string customPluginName = "Untitled";
        public string customPluginFileName = "untitled";
        public int templateType = 0;
        public WorkshopTag selectedFlagsWorkshop = WorkshopTag.None;
        private readonly InteractableController controller = new();
        private readonly InteractableController controllerPluginCreation = new();
        private readonly InteractableController actionController = new();
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
            SpriteFont munroSmall = L.FontSmall();
            actionController.Draw(gameTime, spriteBatch);
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
                ContentScreen? cntscr = ScreenManager.GetScreen<ContentScreen>("Content");
                if(cntscr != null)
                {
                    spriteBatch.End();
                    // Mask to specific area
                    spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle((int)GlobalGraphics.Scale(GlobalGraphics.drawOffset.X+135), (int)GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y+56), GlobalGraphics.Scale(293), GlobalGraphics.Scale(GlobalGraphics.preferredResolution.Y-56));
                    RasterizerState rasterizerState = new RasterizerState();
                    rasterizerState.ScissorTestEnable = true;
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X)+GlobalGraphics.Scale(cntscr.offset.X / GlobalGraphics.scale), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y)+GlobalGraphics.Scale((cntscr.offset.Y / GlobalGraphics.scale) + -scrollOffset), 0));
                }
                int plcount = PluginHandler.GetPluginCount();
                int offsetpl = 0;
                Color curColor = ThemeManager.GetColor("PluginEntryGenericPluginsPage");
                for(int i = 0; i < plcount; i++)
                {
                    bool filtered = false;
                    // Alternate colors so it's easier to see
                    if((i -  offsetpl) % 2 != 0)
                        curColor = ThemeManager.GetColor("PluginEntryGenericAltPluginsPage");
                    // Different addon types have different colors
                    switch(PluginHandler.plugins[i].GetAddonType())
                    {
                        case AddonType.Effect: // blue 
                            //curColor = new Color(192, 192, 255);
                            curColor = ThemeManager.GetColor("PluginEntryEffectPluginsPage");
                            if((i -  offsetpl) % 2 != 0)
                                curColor = ThemeManager.GetColor("PluginEntryEffectAltPluginsPage");
                            if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects))
                                filtered = true;
                            break;
                        case AddonType.PostRenderEffect: // green
                            //curColor = new Color(192, 255, 192);
                            curColor = ThemeManager.GetColor("PluginEntryPostRenderEffectPluginsPage");
                            if((i -  offsetpl) % 2 != 0)
                                curColor = ThemeManager.GetColor("PluginEntryPostRenderEffectAltPluginsPage");
                            if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects))
                                filtered = true;
                            break;
                        case AddonType.Theme: // red
                            //curColor = new Color(255, 192, 192);
                            curColor = ThemeManager.GetColor("PluginEntryThemePluginsPage");
                            if((i -  offsetpl) % 2 != 0)
                                curColor = ThemeManager.GetColor("PluginEntryThemeAltPluginsPage");
                            if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Themes))
                                filtered = true;
                            break;
                    }

                    // Filtered out
                    if(filtered)
                    {
                        offsetpl++;
                        continue;
                    }

                    // Draw the plugin entry
                    spriteBatch.Draw(pluginEntry, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl)), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), curColor);
                            
                    // Set up the plugin name
                    string nam = PluginHandler.plugins[i].GetDisplayName();
                    if(nam == "")
                        nam = "My";
                    if(nam.Contains(".lua"))
                        nam = nam.Replace(".lua", "");

                    // Capitalize first letter
                    nam = nam.First().ToString().ToUpper() + nam.Substring(1);

                    // Remove type from name if it already has it
                    if(PluginHandler.plugins[i].GetAddonType() == AddonType.Effect)
                    {
                        if(nam.ToLower().EndsWith(" effect"))
                            nam = nam.Substring(0, nam.Length - 7);
                    }
                    else if(PluginHandler.plugins[i].GetAddonType() == AddonType.PostRenderEffect)
                    {
                        if(nam.ToLower().EndsWith(" post-render effect") || nam.ToLower().EndsWith(" post render effect"))
                            nam = nam.Substring(0, nam.Length - 17);
                        if(nam.ToLower().EndsWith(" postrender effect"))
                            nam = nam.Substring(0, nam.Length - 15);
                        if(nam.ToLower().EndsWith(" post"))
                            nam = nam.Substring(0, nam.Length - 5);
                        if(nam.ToLower().EndsWith(" pr-effect") || nam.ToLower().EndsWith(" pr effect"))
                            nam = nam.Substring(0, nam.Length - 10);
                        if(nam.ToLower().EndsWith(" preffect"))
                            nam = nam.Substring(0, nam.Length - 8);
                    }
                    else if(PluginHandler.plugins[i].GetAddonType() == AddonType.Theme)
                    {
                        if(nam.ToLower().EndsWith(" theme"))
                            nam = nam.Substring(0, nam.Length - 6);
                        if(nam.ToLower().EndsWith(" bg"))
                            nam = nam.Substring(0, nam.Length - 3);
                        if(nam.ToLower().EndsWith(" background"))
                            nam = nam.Substring(0, nam.Length - 10);
                    }

                    // Add type to name
                    switch(PluginHandler.plugins[i].GetAddonType())
                    {
                        case AddonType.Effect:
                            if(nam.Length + 7 <= 26)
                                nam = L.T(0, "Addons:TypePrefixEffect", nam);
                            break;
                        case AddonType.PostRenderEffect:
                            if(nam.Length + 19 <= 26)
                                nam = L.T(0, "Addons:TypePrefixPostRenderEffect", nam);
                            // special case: abbreviate because this type is kinda long 
                            else if(nam.Length + 10 <= 26)
                                nam = L.T(0, "Addons:TypePrefixPostRenderEffectAlt", nam);
                            break;
                        case AddonType.Theme:
                            if(nam.Length + 6 <= 26)
                                nam = L.T(0, "Addons:TypePrefixTheme", nam);
                            break;
                    }

                    // Fit name to width
                    int maxPixels = 110 - GlobalGraphics.GetSmallStringWidth("...");
                    int namWidth = GlobalGraphics.GetSmallStringWidth(nam);
                    while(namWidth > maxPixels)
                    {
                        nam = nam.Substring(0, nam.Length - 1);
                        namWidth = GlobalGraphics.GetSmallStringWidth(nam);
                    }

                    GlobalContent.DrawString(spriteBatch, munroSmall, nam, new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.Black);
                    GlobalContent.DrawString(spriteBatch, munroSmall, nam, new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.White);
                    if(Global.canRender)
                    {
                        spriteBatch.Draw(PluginHandler.plugins[i].enabled ? interactiveSwitchOn : interactiveSwitchOff, new Rectangle(GlobalGraphics.Scale(271), GlobalGraphics.Scale(60 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl)), interactiveSwitchOn.Width * GlobalGraphics.scale, interactiveSwitchOn.Height * GlobalGraphics.scale), Color.White);
                    }
                    else
                    {
                        GlobalContent.DrawString(spriteBatch, munroSmall, "...", new Vector2(GlobalGraphics.Scale(277+1), GlobalGraphics.Scale(58 + 1 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, "...", new Vector2(GlobalGraphics.Scale(277), GlobalGraphics.Scale(58 + (i-offsetpl) * pluginEntry.Height + (i-offsetpl))), Color.White);
                    }
                }
                // create plugin
                // Alternate colors so it's easier to see
                if((plcount -  offsetpl) % 2 != 0)
                    curColor = ThemeManager.GetColor("PluginEntryGenericAltPluginsPage");
                else
                    curColor = ThemeManager.GetColor("PluginEntryGenericPluginsPage");
                spriteBatch.Draw(pluginEntryBlank, new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57 + (plcount-offsetpl) * pluginEntry.Height + (plcount-offsetpl)), pluginEntry.Width * GlobalGraphics.scale, pluginEntry.Height * GlobalGraphics.scale), curColor);
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Addons:AddonManagementButton"), new Vector2(GlobalGraphics.Scale(141+1), GlobalGraphics.Scale(58+1 + (plcount-offsetpl) * pluginEntry.Height + (plcount-offsetpl))), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Addons:AddonManagementButton"), new Vector2(GlobalGraphics.Scale(141), GlobalGraphics.Scale(58 + (plcount-offsetpl) * pluginEntry.Height + (plcount-offsetpl))), Color.White);
                // End offset
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
            }
            else
            {
                // Draw background
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), ThemeManager.GetColor("OverlayContentScreen"));
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
                Global.tooltip = internalTooltip;
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(actionController.Update(gameTime, handleInput))
                return true;
            int plcount = PluginHandler.GetPluginCount();
            int offsetpl = 0;
            for(int i = 0; i < plcount; i++)
            {
                bool filtered = false;
                switch(PluginHandler.plugins[i].GetAddonType())
                {
                    case AddonType.Effect:
                        if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects))
                            filtered = true;
                        break;
                    case AddonType.PostRenderEffect:
                        if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects))
                            filtered = true;
                        break;
                    case AddonType.Theme:
                        if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Themes))
                            filtered = true;
                        break;
                }
                if(filtered)
                {
                    offsetpl++;
                    continue;
                }
            }
            int tempMaxScrollOffset = plcount + 1 - offsetpl; // + 1 because of the create plugin button
            tempMaxScrollOffset -= 11; // 11 entries fit on the screen
            if(tempMaxScrollOffset <= 0)
                tempMaxScrollOffset = 0;
            maxScrollOffset = tempMaxScrollOffset * 16;
            offsetpl = 0;
            if(handleInput || dragging)
            {
                internalTooltip = "";
                if(!editingSettings)
                {
                    for (int i = 0; i < plcount; i++)
                    {
                        bool filtered = false;
                        switch(PluginHandler.plugins[i].GetAddonType())
                        {
                            case AddonType.Effect:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects))
                                    filtered = true;
                                break;
                            case AddonType.PostRenderEffect:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects))
                                    filtered = true;
                                break;
                            case AddonType.Theme:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Themes))
                                    filtered = true;
                                break;
                        }
                        if(filtered)
                        {
                            offsetpl++;
                            continue;
                        }
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                        {
                            internalTooltip = L.T(0, Global.canRender ? "Addons:SwitchToggleAddon" : "Addons:Loading");
                        }
                        int inRange = 254;
                        if(!Global.canRender)
                        {
                            inRange = 267; // No settings button
                        }
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(inRange)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                        {
                            try
                            {
                                // Is this a workshop plugin?
                                // If so, tooltip should be "Open workshop page."
                                // Otherwise, tooltip should be "Open plugin directory."
                                if (PluginHandler.plugins[i].workshopId != ""
                                    && PluginHandler.plugins[i].rootPath.Contains("workshop"))
                                    internalTooltip = L.T(0, "Addons:ButtonOpenWorkshop");
                                else if(PluginHandler.plugins[i].workshopId != ""
                                    && PluginHandler.plugins[i].workshopId != "stock")
                                    internalTooltip = L.T(0, "Addons:ButtonOpenCodeEditor");
                                else
                                    internalTooltip = L.T(0, "Addons:ButtonOpenStockDirectory");
                            }
                            catch {}
                        }
                        if(Global.canRender)
                        {
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(256) && MouseInput.MouseState.X < GlobalGraphics.Scale(267)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                            {
                                internalTooltip = L.T(0, "Addons:ButtonEditSettings");
                            }
                        }
                    }
                    // create plugin
                    if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                        && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((plcount-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((plcount-offsetpl) * 16) - scrollOffset))
                    {
                        internalTooltip = L.T(0, "Addons:ButtonCreateOrReload");
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
                        bool filtered = false;
                        switch(PluginHandler.plugins[i].GetAddonType())
                        {
                            case AddonType.Effect:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects))
                                    filtered = true;
                                break;
                            case AddonType.PostRenderEffect:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects))
                                    filtered = true;
                                break;
                            case AddonType.Theme:
                                if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Themes))
                                    filtered = true;
                                break;
                        }
                        if(filtered)
                        {
                            offsetpl++;
                            continue;
                        }
                        int inRange = 254;
                        if(!Global.canRender)
                        {
                            inRange = 267; // No settings button
                        }
                        // Toggle Button
                        Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(269), GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset), GlobalGraphics.Scale(21), GlobalGraphics.Scale((70 + ((i-offsetpl) * 16) - scrollOffset) - (59 + ((i-offsetpl) * 16) - scrollOffset))), (PluginHandler.plugins[i].enabled ? "Disable " : "Enable ") + "\"" + PluginHandler.plugins[i].GetDisplayName() + "\"");
                        if(Global.canRender)
                        {
                            // Settings Button
                            Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(256), GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset), GlobalGraphics.Scale(11), GlobalGraphics.Scale((70 + ((i-offsetpl) * 16) - scrollOffset) - (59 + ((i-offsetpl) * 16) - scrollOffset))), L.T(0, "Accessibility:AddonsSettings", PluginHandler.plugins[i].GetDisplayName()));
                        }
                        // Main Button
                        Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(138), GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset), GlobalGraphics.Scale(inRange-138), GlobalGraphics.Scale((70 + ((i-offsetpl) * 16) - scrollOffset) - (59 + ((i-offsetpl) * 16) - scrollOffset))), L.T(0, "Accessibility:AddonsOpenContainer", PluginHandler.plugins[i].GetDisplayName()));
                    }
                    // create plugin
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(138), GlobalGraphics.Scale(59 + ((plcount-offsetpl) * 16) - scrollOffset), GlobalGraphics.Scale(152), GlobalGraphics.Scale((70 + ((plcount-offsetpl) * 16) - scrollOffset) - (59 + ((plcount-offsetpl) * 16) - scrollOffset))), L.T(0, "Accessibility:AddonsManagement"));
                    if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        offsetpl = 0;
                        // Plugin entries
                        for (int i = 0; i < plcount; i++)
                        {
                            bool filtered = false;
                            switch(PluginHandler.plugins[i].GetAddonType())
                            {
                                case AddonType.Effect:
                                    if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects))
                                        filtered = true;
                                    break;
                                case AddonType.PostRenderEffect:
                                    if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects))
                                        filtered = true;
                                    break;
                                case AddonType.Theme:
                                    if(!PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Themes))
                                        filtered = true;
                                    break;
                            }
                            if(filtered)
                            {
                                offsetpl++;
                                continue;
                            }
                            // Toggle button
                            if (MouseInput.MouseState.X >= GlobalGraphics.Scale(269) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                            {
                                if(Global.canRender)
                                {
                                    PluginHandler.plugins[i].enabled = !PluginHandler.plugins[i].enabled;
                                    if(PluginHandler.plugins[i].enabled == false
                                        && PluginHandler.plugins[i].GetAddonType() == AddonType.Theme
                                        && SaveData.saveValues["ActiveTheme"] == Path.GetFileName(PluginHandler.plugins[i].path))
                                    {
                                        SaveData.saveValues["ActiveTheme"] = "";
                                        SaveData.Save();
                                        ThemeManager.ApplyTheme(DefaultThemes.defaultTheme);
                                    }
                                    else if(PluginHandler.plugins[i].enabled == true
                                        && PluginHandler.plugins[i].GetAddonType() == AddonType.Theme)
                                    {
                                        // Disable all other themes
                                        foreach(Plugin theme in PluginHandler.GetEnabledPluginsOfType(AddonType.Theme))
                                        {
                                            if(theme != PluginHandler.plugins[i])
                                            {
                                                theme.enabled = false;
                                            }
                                        }
                                        SaveData.saveValues["ActiveTheme"] = Path.GetFileName(PluginHandler.plugins[i].path);
                                        SaveData.Save();
                                        ThemeManager.LoadThemes();
                                        ThemeManager.themes.ForEach((Theme theme) => {
                                            if(theme.name == PluginHandler.plugins[i].GetDisplayName())
                                            {
                                                ThemeManager.ApplyTheme(theme);
                                            }
                                        });
                                    }
                                    PluginHandler.SavePluginSettings();
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                }
                                else
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                    && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((i-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((i-offsetpl) * 16) - scrollOffset))
                                {
                                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    // Open settings
                                    controller.Clear();
                                    settingsIndex = i;
                                    editingSettings = true;
                                    if(PluginHandler.plugins[i].workshopId != ""
                                        && PluginHandler.plugins[i].workshopId != "stock")
                                    {
                                        if(PluginHandler.plugins[i].submittedId != "" || PluginHandler.plugins[i].workshopId != "")
                                        {
                                            controller.Add("VisitWorkshopPage", new Button("View Workshop", "View this effect on the Workshop.", new Vector2(119+100, 60+10+19*7), (int i, string n) => {
                                                switch(i)
                                                {
                                                    case 2:
                                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        ProcessStartInfo startInfo = new();
                                                        startInfo.FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + (string.IsNullOrEmpty(PluginHandler.plugins[settingsIndex].submittedId) ? PluginHandler.plugins[settingsIndex].workshopId : PluginHandler.plugins[settingsIndex].submittedId);
                                                        startInfo.UseShellExecute = true;
                                                        Process.Start(startInfo);
                                                        return true;
                                                }
                                                return false;
                                            }));
                                        }
                                        if(PluginHandler.plugins[i].rootPath.Contains("user"))
                                        {
                                            controller.Add("Delete", new Button("Delete", "", new Vector2(119+100, 60+10+19*8), (int i, string n) => {
                                                switch(i)
                                                {
                                                    case 2:
                                                        // Delete and reload plugins
                                                        try
                                                        {
                                                            // If this was a theme, reset the theme
                                                            if(PluginHandler.plugins[settingsIndex].GetAddonType() == AddonType.Theme
                                                                && SaveData.saveValues["ActiveTheme"] == Path.GetFileName(PluginHandler.plugins[settingsIndex].path))
                                                            {
                                                                SaveData.saveValues["ActiveTheme"] = "";
                                                                ThemeManager.LoadThemes();
                                                                ThemeManager.ApplyTheme(DefaultThemes.defaultTheme);
                                                            }
                                                            Directory.Delete(Path.GetDirectoryName(PluginHandler.plugins[settingsIndex].path) ?? "", true);
                                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            if(SteamManager.initialized)
                                                                PluginHandler.LoadWorkshop();
                                                            else
                                                                PluginHandler.LoadPluginsThreaded();
                                                            editingSettings = false;
                                                            scrollOffset = 0;
                                                        }
                                                        catch
                                                        {
                                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        }
                                                        return true;
                                                }
                                                return false;
                                            }));
                                            controller.Add("Publish", new Button("Publish", "Publish to Steam Workshop.", new Vector2(139+139, 60+10+19*8), (int i, string n) => {
                                                switch(i)
                                                {
                                                    case 2: // left click
                                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        Name = L.T(0, "Addons:WorkshopTags", PluginHandler.plugins[settingsIndex].GetDisplayName());
                                                        controller.Clear();
                                                        controller.Add("LibraryCustom", new Switch("Custom Library", "Uses a custom library type.", new Vector2(139, 60+19*8), (int i, string n) => {
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
                                                        controller.Add("LibraryOverlay", new Switch("Overlay Library", "Uses the overlay library type.", new Vector2(139, 60+19*7), (int i, string n) => {
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
                                                        controller.Add("LibraryOutro", new Switch("Outro Library", "Uses the outro library type.", new Vector2(139, 60+19*6), (int i, string n) => {
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
                                                        controller.Add("LibraryIntro", new Switch("Intro Library", "Uses the intro library type.", new Vector2(139, 60+19*5), (int i, string n) => {
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
                                                        controller.Add("LibraryTransition", new Switch("Transition Library", "Uses the transition library type.", new Vector2(139, 60+19*4), (int i, string n) => {
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
                                                        controller.Add("LibraryMaterial", new Switch("Material Library", "Uses the material library type.", new Vector2(139, 60+19*3), (int i, string n) => {
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
                                                        controller.Add("LibraryMusic", new Switch("Music Library", "Uses the music library type.", new Vector2(139, 60+19*2), (int i, string n) => {
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
                                                        controller.Add("LibrarySFX", new Switch("Sound FX Library", "Uses the sound fx library type.", new Vector2(139, 60+19), (int i, string n) => {
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
                                                        controller.Add("Image", new Switch("Image", "Applies image effects.", new Vector2(139+55+55, 60), (int i, string n) => {
                                                            bool switchState = (i & 256) != 0;
                                                            if((i & 2) != 0)
                                                            {
                                                                // add or remove WorkshopTag.Image to flags
                                                                if(switchState)
                                                                    selectedFlagsWorkshop |= WorkshopTag.Effect_ImageOnly;
                                                                else
                                                                    selectedFlagsWorkshop &= ~WorkshopTag.Effect_ImageOnly;
                                                            }
                                                            return switchState;
                                                        }, (selectedFlagsWorkshop & WorkshopTag.Effect_ImageOnly) != 0));
                                                        controller.Add("Audio", new Switch("Audio", "Applies audio effects.", new Vector2(139+55, 60), (int i, string n) => {
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
                                                        controller.Add("Video", new Switch("Video", "Applies video effects.", new Vector2(139, 60), (int i, string n) => {
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
                                                        controller.Add("Submit", new Button("Submit", "Submit to Steam Workshop.", new Vector2(139+139, 60+10+19*7), (int i, string n) => {
                                                            switch(i)
                                                            {
                                                                case 2: // left click
                                                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
#if WINDOWSDX
                                                                    // Select png, jpg, or gif icon with file dialog
                                                                    System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                                                                    fileDialog.Filter = "Joint Photographic Experts Group (*.jpg, *.jpeg)|*.jpg;*.jpeg";
                                                                    fileDialog.Title = "Select Workshop Icon";
                                                                    fileDialog.InitialDirectory = Path.GetFullPath(@"templates");
                                                                    fileDialog.FileName = "workshop.jpg";
                                                                    if(fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                                    {
                                                                        // file must be under 1mb, otherwise complain
                                                                        if(new FileInfo(fileDialog.FileName).Length > 1048576)
                                                                        {
                                                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                            ConsoleOutput.WriteLine("Selected Workshop Icon is too large. Must be under 1mb.", Color.Red);
                                                                            Global.generator.progressText = L.T(0, "Addons:StatusFailPreviewSize");
                                                                            return true;
                                                                        }
                                                                        Name = "PageAddons";
                                                                        editingSettings = false;
                                                                        controller.Clear();
                                                                        Global.generator.progressText = L.T(0, "Addons:StatusUploading");
                                                                        ConsoleOutput.WriteLine("Publishing " + Path.GetFileName(PluginHandler.plugins[settingsIndex].path) + " with icon " + Path.GetFileName(fileDialog.FileName), Color.RoyalBlue);
                                                                        PluginHandler.PublishPlugin(PluginHandler.plugins[settingsIndex], selectedFlagsWorkshop, fileDialog.FileName);
                                                                    }
#endif
                                                                    return true;
                                                            }
                                                            return false;
                                                        }));
                                                        controller.Add("Back", new Button("Back", "Go back to addon list.", new Vector2(119+167, 60+10+19*8), (int i, string n) => {
                                                            switch(i)
                                                            {
                                                                case 2: // left click
                                                                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                                    controller.Clear();
                                                                    Name = "PageAddons";
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
                                    controller.Add("Back", new Button("Back", "Go back to addon list.", new Vector2(119+36, 60+10+19*8), (int i, string n) => {
                                        switch(i)
                                        {
                                            case 2: // left click
                                                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                controller.Clear();
                                                Name = "PageAddons";
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
                                                te = new Switch(s.Key, PluginHandler.plugins[i].settingTooltips[s.Key], new Vector2(139, 60+19*sindex), (int i, string keyFromIndex) => {
                                                    bool switchState = (i & 256) != 0;
                                                    if((i & 2) != 0)
                                                    {
                                                        string oldValue = PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")].ToString() ?? "";
                                                        if(!controller.interactables.ContainsKey(keyFromIndex))
                                                        {
                                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            return true;
                                                        }
                                                        PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")] = switchState ? "1" : "0";
                                                        if(oldValue != (PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")].ToString() ?? ""))
                                                        {
                                                            PluginHandler.SavePluginSettings();
                                                        }
                                                    }
                                                    return switchState;
                                                }, PluginHandler.plugins[i].settings[s.Key].ToString() == "1");
                                                break;
                                            case SettingType.TextInput:
                                            default:
                                                te = new TextEntry(s.Key, PluginHandler.plugins[i].settingTooltips[s.Key], PluginHandler.plugins[i].settings[s.Key].ToString() ?? "", new Vector2(139, 60+19*sindex), 50, 25, ty, (int i, string keyFromIndex) => {
                                                    if(i == 0)
                                                    {
                                                        string oldValue = PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")].ToString() ?? "";
                                                        if(!controller.interactables.ContainsKey(keyFromIndex))
                                                        {
                                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                            return true;
                                                        }
                                                        PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")] = controller.interactables[keyFromIndex].Tooltip;
                                                        if(oldValue != PluginHandler.plugins[settingsIndex].settings[keyFromIndex.Replace("NoLocalization:", "")].ToString())
                                                        {
                                                            PluginHandler.SavePluginSettings();
                                                        }
                                                    }
                                                    return false;
                                                });
                                                break;
                                        }
                                        tes.Add(te);
                                        sindex++;
                                    }
                                    for(int i2 = tes.Count-1; i2 >= 0; i2--)
                                    {
                                        // Get name
                                        string name = "NoLocalization:"+tes[i2].Name;
                                        controller.Add(name, tes[i2]);
                                    }
                                    return true;
                                }
                            }
                        }
                        // create plugin
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(138) && MouseInput.MouseState.X < GlobalGraphics.Scale(290)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(59 + ((plcount-offsetpl) * 16) - scrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(70 + ((plcount-offsetpl) * 16) - scrollOffset))
                        {
                            if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                            {
                                pluginCreation = true;
                                editingSettings = true;
                                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
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
            // Clear all controllers
            actionController.Clear();
            controller.Clear();
            controllerPluginCreation.Clear();
            GlobalContent.AddTexture("PluginEntryBlank", ThemeManager.LoadLayeredContent<Texture2D>("graphics/pluginentryblank"));
            GlobalContent.AddTexture("PluginEntry", ThemeManager.LoadLayeredContent<Texture2D>("graphics/pluginentry"));
            actionController.Add("ActionSteam", new ActionButton("View the Steam Workshop!", new Vector2(112, 161), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if(Global.ready)
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            ProcessStartInfo psi = new()
                            {
                                FileName = "https://steamcommunity.com/app/2516360/workshop/",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/steam")));
            actionController.Add("ReloadPlugins", new ActionButton("Force reload all addons.", new Vector2(112, 176), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(SteamManager.initialized)
                            PluginHandler.LoadWorkshop();
                        else
                            PluginHandler.LoadPluginsThreaded();
                        scrollOffset = 0;
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/reload")));
            actionController.Add("Filter", new ActionButton("Change the filter type in the addon list.", new Vector2(112, 191), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        PluginHandler.CyclePluginListFilter();
                        if(PluginHandler.pluginListFilter == (PluginListFilter.Effects | PluginListFilter.PostRenderEffects | PluginListFilter.Themes))
                            Name = "PageAddons";
                        else
                            Name = PluginHandler.GetPluginListFilter();
                        scrollOffset = 0;
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/filter")));
            actionController.Add("ActionEnableAll", new ActionButton("Enable all content in this category.", new Vector2(112, 206), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(PluginHandler.pluginListFilter == PluginListFilter.Themes)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        for(int index = 0; index < PluginHandler.plugins.Count; index++)
                        {
                            AddonType type = PluginHandler.plugins[index].GetAddonType();
                            if(PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects) && type == AddonType.Effect)
                                PluginHandler.plugins[index].enabled = true;
                            if(PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects) && type == AddonType.PostRenderEffect)
                                PluginHandler.plugins[index].enabled = true;
                        }
                        PluginHandler.SavePluginSettings();
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/enableall")));
            actionController.Add("ActionDisableAll", new ActionButton("Disable all content in this category.", new Vector2(112, 221), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(PluginHandler.pluginListFilter == PluginListFilter.Themes)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        for(int index = 0; index < PluginHandler.plugins.Count; index++)
                        {
                            AddonType type = PluginHandler.plugins[index].GetAddonType();
                            if(PluginHandler.pluginListFilter.HasFlag(PluginListFilter.Effects) && type == AddonType.Effect)
                                PluginHandler.plugins[index].enabled = false;
                            if(PluginHandler.pluginListFilter.HasFlag(PluginListFilter.PostRenderEffects) && type == AddonType.PostRenderEffect)
                                PluginHandler.plugins[index].enabled = false;
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/disableall")));
            controllerPluginCreation.Add("PluginTheme", new Switch("Theme Template", "Create a theme for " + Global.productNameShort + ".", new Vector2(139, 60+19*3), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    Switch? buttonSwitch = controllerPluginCreation.interactables["PluginMinimal"] as Switch;
                    if(buttonSwitch != null)
                        buttonSwitch.SwitchState = false;
                    if(switchState)
                        templateType = 2;
                    else
                        templateType = 0;
                }
                return switchState;
            }, templateType == 2));
            controllerPluginCreation.Add("PluginMinimal", new Switch("Minimal Template", "Use a minimal Lua file template.", new Vector2(139, 60+19*2), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    Switch? buttonSwitch = controllerPluginCreation.interactables["PluginTheme"] as Switch;
                    if(buttonSwitch != null)
                        buttonSwitch.SwitchState = false;
                    if(switchState)
                        templateType = 1;
                    else
                        templateType = 0;
                }
                return switchState;
            }, templateType == 1));
            controllerPluginCreation.Add("PluginName", new TextEntry("Addon File Name", "The internal file name of the addon.", customPluginFileName, new Vector2(139, 60+19), 50, 25, 4, (int i, string n) => {
                controllerPluginCreation.interactables["PluginName"].Tooltip = controllerPluginCreation.interactables["PluginName"].Tooltip.ToLower();
                customPluginFileName = controllerPluginCreation.interactables["PluginName"].Tooltip;
                return false;
            }));
            controllerPluginCreation.Add("PluginPrettyName", new TextEntry("Addon Name", "The human-readable name of the addon.", customPluginName, new Vector2(139, 60), 50, 25, 5, (int i, string n) => {
                customPluginName = controllerPluginCreation.interactables["PluginPrettyName"].Tooltip;
                return false;
            }));
            controllerPluginCreation.Add("Filter", new Button("Filter: "+PluginHandler.GetPluginListFilter(), "Change the filter type in the addon list.", new Vector2(119+100, 60+10+19*6), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        PluginHandler.CyclePluginListFilter();
                        scrollOffset = 0;
                        controllerPluginCreation.interactables["Filter"].Name = "Filter: "+PluginHandler.GetPluginListFilter();
                        return true;
                }
                return false;
            }));
            controllerPluginCreation.Add("Back", new Button("Back", "Go back.", new Vector2(119+36, 60+10+19*8), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        editingSettings = false;
                        pluginCreation = false;
                        return true;
                }
                return false;
            }));
            controllerPluginCreation.Add("ReloadPlugins", new Button("Reload All", "Force reload all addons.", new Vector2(119+100, 60+10+19*7), (int i, string n) => {
                switch(i)
                {
                    case 2:
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(SteamManager.initialized)
                            PluginHandler.LoadWorkshop();
                        else
                            PluginHandler.LoadPluginsThreaded();
                        scrollOffset = 0;
                        return true;
                }
                return false;
            }));
            controllerPluginCreation.Add("CreatePlugin", new Button("Create", "Create the addon as specified.", new Vector2(139+141, 60+10+19*8), (int i, string n) => {
                switch(i)
                {
                    case 2:
                        pluginCreation = false;
                        if(PluginHandler.CreatePlugin(customPluginFileName, customPluginName, templateType, out string file))
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            // load plugins
                            if(SteamManager.initialized)
                                PluginHandler.LoadWorkshop();
                            else
                                PluginHandler.LoadPluginsThreaded();
                            editingSettings = false;
                            scrollOffset = 0;
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
