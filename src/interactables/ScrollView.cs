using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// ScrollViews are simple UI elements that can be scrolled through to view a list of items.
    /// </summary>
    public class ScrollView : IObject
    {
        public int ScrollOffset = 0;
        public int MaxScrollOffset = 0;
        public bool Dragging = false;
        public int DragOffset = 0;
        public int EntryHeight = 16;
        public readonly InteractableController Controller = new();
        public virtual bool Update(GameTime gameTime, bool handleInput)
        {
            if(handleInput || Dragging)
            {
                if(MaxScrollOffset > 0)
                {
                    if (MouseInput.MouseState.ScrollWheelValue != MouseInput.LastMouseState.ScrollWheelValue)
                    {
                        // 120 is the scroll wheel value for one scroll.
                        // One entry is one offset.
                        int lines = (MouseInput.MouseState.ScrollWheelValue - MouseInput.LastMouseState.ScrollWheelValue) / 120;
                        int oldScrollOffset = ScrollOffset;
                        // Round up or down scroll offset to the nearest 16 multiple.
                        ScrollOffset = ScrollOffset / EntryHeight * EntryHeight;
                        // If it's the same
                        if (ScrollOffset == oldScrollOffset)
                        {
                            ScrollOffset -= lines * EntryHeight;
                        }
                        // If it's less than 0, set it to 0.
                        if (ScrollOffset < 0)
                        {
                            ScrollOffset = 0;
                        }
                        // If it's more than the max scroll offset, set it to the max scroll offset.
                        if (ScrollOffset > MaxScrollOffset)
                        {
                            ScrollOffset = MaxScrollOffset;
                        }
                    }
                    // Scroll handle
                    if(!Dragging)
                    {
                        if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        {
                            // directly on handle (start dragging)
                            if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(303)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69 + ScrollOffset * (214 - 69) / MaxScrollOffset) && MouseInput.MouseState.Y < GlobalGraphics.Scale(78 + ScrollOffset * (214 - 69) / MaxScrollOffset))
                            {
                                Dragging = true;
                                DragOffset = MouseInput.MouseState.Y - GlobalGraphics.Scale(69 + ScrollOffset * (214 - 69) / MaxScrollOffset);
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            // on scroll bar empty space (move center of handle to there)
                            if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294+3) && MouseInput.MouseState.X < GlobalGraphics.Scale(303-3)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(69+4) && MouseInput.MouseState.Y < GlobalGraphics.Scale(223-4))
                            {
                                ScrollOffset = (MouseInput.MouseState.Y - GlobalGraphics.Scale(69) - GlobalGraphics.Scale(4)) * MaxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                return true;
                            }
                            // 293, 57, 11x11 Scroll Up
                            if(MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                                && MouseInput.MouseState.Y >= GlobalGraphics.Scale(57) && MouseInput.MouseState.Y < GlobalGraphics.Scale(68))
                            {
                                if(ScrollOffset - 1 >= 0)
                                {
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    int oldScrollOffset = ScrollOffset;
                                    // Round down scroll offset to the nearest 16 multiple.
                                    ScrollOffset = ScrollOffset / EntryHeight * 16;
                                    // If it's the same, subtract 16.
                                    if (ScrollOffset == oldScrollOffset)
                                    {
                                        ScrollOffset -= EntryHeight;
                                    }
                                    // If it's less than 0, set it to 0.
                                    if (ScrollOffset < 0)
                                    {
                                        ScrollOffset = 0;
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
                                if (ScrollOffset + 1 <= MaxScrollOffset)
                                {
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    int oldScrollOffset = ScrollOffset;
                                    // Round up scroll offset to the nearest 16 multiple.
                                    ScrollOffset = ScrollOffset / EntryHeight * EntryHeight;
                                    // If it's the same, add 16.
                                    if (ScrollOffset == oldScrollOffset)
                                    {
                                        ScrollOffset += EntryHeight;
                                    }
                                    // If it's more than the max, set it to the max.
                                    if (ScrollOffset > MaxScrollOffset)
                                    {
                                        ScrollOffset = MaxScrollOffset;
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
                            Dragging = false;
                        }
                        else
                        {
                            int newY = MouseInput.MouseState.Y - DragOffset;
                            if (newY >= GlobalGraphics.Scale(69) && newY <= GlobalGraphics.Scale(214))
                            {
                                ScrollOffset = (newY - GlobalGraphics.Scale(69)) * MaxScrollOffset / (GlobalGraphics.Scale(214) - GlobalGraphics.Scale(69));
                            }
                            else if (newY < GlobalGraphics.Scale(69))
                            {
                                ScrollOffset = 0;
                            }
                            else if (newY > GlobalGraphics.Scale(214))
                            {
                                ScrollOffset = MaxScrollOffset;
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
                            ScrollOffset = 0;
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        // 293, 224, 11x11 Scroll Down
                        if (MouseInput.MouseState.X >= GlobalGraphics.Scale(294) && MouseInput.MouseState.X < GlobalGraphics.Scale(304)
                            && MouseInput.MouseState.Y >= GlobalGraphics.Scale(224) && MouseInput.MouseState.Y < GlobalGraphics.Scale(235))
                        {
                            // Set to max because this is the bottom.
                            ScrollOffset = MaxScrollOffset;
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                    }
                    Dragging = false;
                }
                // offset mouse y by scroll offset
                Controller.mousePosition = new Vector2(MouseInput.MouseState.X, MouseInput.MouseState.Y + GlobalGraphics.Scale(ScrollOffset));
                if(Controller.Update(gameTime, handleInput))
                    return true;
            }
            return false;
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // 135, 56 PluginPage
            // 136, 57 PluginEntry
            // 294, 69-214 ScrollHandle (9x9)
            Texture2D pluginPage = GlobalContent.GetTexture("PluginPage");
            Texture2D scrollHandle = GlobalContent.GetTexture("ScrollHandle");
            // Draw scroll bar
            spriteBatch.Draw(pluginPage, new Rectangle(GlobalGraphics.Scale(293), GlobalGraphics.Scale(57), pluginPage.Width * GlobalGraphics.scale, pluginPage.Height * GlobalGraphics.scale), Color.White);
            // Move the scroll handle relative to the scroll offset and the max scroll offset.
            if(MaxScrollOffset > 0)
            {
                spriteBatch.Draw(scrollHandle, new Rectangle(GlobalGraphics.Scale(294), GlobalGraphics.Scale(69 + ScrollOffset * (214 - 69) / MaxScrollOffset), scrollHandle.Width * GlobalGraphics.scale, scrollHandle.Height * GlobalGraphics.scale), Color.White);
            }
            // End existing spritebatch
            ContentScreen? cntscr = ScreenManager.GetScreen<ContentScreen>("Content");
            if(cntscr != null)
            {
                spriteBatch.End();
                // Mask to specific area 
                spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle((int)GlobalGraphics.Scale(GlobalGraphics.drawOffset.X+135), (int)GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y+56), GlobalGraphics.Scale(293), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(56));
                RasterizerState rasterizerState = new RasterizerState
                {
                    ScissorTestEnable = true
                };
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X)+GlobalGraphics.Scale(cntscr.offset.X / GlobalGraphics.scale), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y)+GlobalGraphics.Scale((cntscr.offset.Y / GlobalGraphics.scale) + -ScrollOffset), 0));
            }
            // Draw the controller
            Controller.Draw(gameTime, spriteBatch);
            // End offset
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
        }
        public virtual void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            Controller.overrideMousePosition = true;
        }
    }
}
