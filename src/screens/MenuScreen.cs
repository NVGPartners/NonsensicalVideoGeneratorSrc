#if MONOGAME
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This is the main menu screen. It is the first screen that is displayed when the application starts.
    /// </summary>
    public class MenuScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Main Menu";
        public int layer { get; } = 8;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        private int hoveringPage = 0;
        private bool hovering = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        public void Show()
        {
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(GlobalGraphics.Scale(-124), 0); // from left to right
                tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(0, 0);
            }
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(0, 0); // from right to left
                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-124), 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(GlobalGraphics.Scale(-124), 0);
            }
            hiding = true;
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            if (useBool)
            {
                if (toggleTo)
                {
                    Show();
                    return true;
                }
                else
                {
                    Hide();
                    return false;
                }
            }
            else
            {
                if (toggle)
                {
                    Hide();
                    return false;
                }
                else
                {
                    Show();
                    return true;
                }
            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Menu Window
            Texture2D menuwindowbg = GlobalContent.GetTexture("MenuWindowBG");
            spriteBatch.Draw(menuwindowbg, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(134), GlobalGraphics.Scale(menuwindowbg.Width), GlobalGraphics.Scale(menuwindowbg.Height)), Color.White);
            // Options
            Texture2D menuselected = GlobalContent.GetTexture("MenuSelected"); // standard option
            Texture2D menuselected2 = GlobalContent.GetTexture("MenuSelected2"); // top option
            Texture2D menuselected3 = GlobalContent.GetTexture("MenuSelected3"); // bottom option
            int pageOffset = 16;
            int currentPage = Pagination.GetParentPage();
            int whichOption = currentPage == 0 ? 1 : (currentPage == Pagination.GetTopPageCount() - 1 ? 2 : 0); // 0: middle, 1: top, 2: bottom
            spriteBatch.Draw(whichOption == 1 ? menuselected2 : (whichOption == 2 ? menuselected3 : menuselected), new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(134 + (pageOffset * currentPage)), GlobalGraphics.Scale(menuselected.Width), GlobalGraphics.Scale(menuselected.Height)), Color.White);
            // Draw option text
            string name;
            for (int i = 0; i < Pagination.GetTopPageCount(); i++)
            {
                name = Pagination.GetPage(i).Name;
                if(i == 2)
                    name = "Addons";
                spriteBatch.DrawString(GlobalContent.GetFont("Munro"), name, new Vector2(GlobalGraphics.Scale(7+1), GlobalGraphics.Scale(136 + (pageOffset * i)+1)), Color.Black);
                spriteBatch.DrawString(GlobalContent.GetFont("Munro"), name, new Vector2(GlobalGraphics.Scale(7), GlobalGraphics.Scale(136 + (pageOffset * i))), Color.White);
            }
            Texture2D menuwindow = GlobalContent.GetTexture("MenuWindow");
            spriteBatch.Draw(menuwindow, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(132), GlobalGraphics.Scale(menuwindow.Width), GlobalGraphics.Scale(menuwindow.Height)), Color.White);
            // Draw window title
            Vector2 titleSize = GlobalContent.GetFont("MunroSmall").MeasureString(title);
            spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), title, new Vector2(GlobalGraphics.Scale(52), GlobalGraphics.Scale(203)), Color.White, MathHelper.ToRadians(90), new Vector2(titleSize.X, titleSize.Y), 1, SpriteEffects.None, 0);
            // If hovering, draw tooltip
            if (hovering && !Global.exiting)
            {
                string tooltip = Pagination.GetPage(hoveringPage).Tooltip;
                // Get text size
                Vector2 tooltipSize = GlobalContent.GetFont("MunroSmall").MeasureString(tooltip);
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 10, MouseInput.MouseState.Position.Y + 10);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2); 
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), ThemeManager.GetColor("BackgroundTooltip"));
                // White text
                spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // When animation is done, set screen type
            if (hiding && offset.X == GlobalGraphics.Scale(-124))
            {
                screenType = ScreenType.Hidden;
                hiding = false;
            }
            else if (showing)
            {
                screenType = ScreenType.Drawn;
                showing = false;
                hiding = false;
            }
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if(hiding || screenType == ScreenType.Hidden)
                return false;
            // Input
            if(handleInput)
            {
                // Accessibility
                // -2 because the exit and game buttons are not included
                for(int i = 0; i < Pagination.GetTopPageCount()-2; i++)
                {
                    string name = Pagination.GetPage(i).Name;
                    if(i == 2)
                        name = "Addons";
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(i * 16 + 134), GlobalGraphics.Scale(45), GlobalGraphics.Scale(16)), name + " (" + Pagination.GetPage(i).Tooltip + ")");
                }
                // Bounds of each segment
                if(MouseInput.MouseState.X >= GlobalGraphics.Scale(4) && MouseInput.MouseState.X < GlobalGraphics.Scale(49) && MouseInput.MouseState.Y >= GlobalGraphics.Scale(134) && MouseInput.MouseState.Y < GlobalGraphics.Scale(230))
                {
                    // Figure out which section the mouse is in
                    int segment = (int)Math.Floor((double)(MouseInput.MouseState.Y - GlobalGraphics.Scale(134)) / GlobalGraphics.Scale(16));
                    // Check to ensure this segment is valid
                    if (segment >= Pagination.GetTopPageCount())
                    {
                        hovering = false;
                    }
                    else
                    {
                        hoveringPage = segment;
                        hovering = true;
                        // Get click
                        if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                        {
                            // Play sound
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);    
                            if(Pagination.DrawnPage != segment)
                            {
                                string name = Pagination.GetPage(segment).Name;
                                if(segment == 2)
                                    name = "Addons";
                                DiscordRPC.curtab = DiscordRPC.ToTab(name);
                                if(segment == Pagination.GetPageCount() - 2)
                                {
                                    ScreenManager.PushNavigation("Pastime Game");
                                    ScreenManager.GetScreen<PastimeGameScreen>("Pastime Game")?.Show();
                                    ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                                }
                                else
                                {
                                    if(Pagination.DrawnPage == Pagination.GetPageCount() - 2)
                                    {
                                        ScreenManager.GetScreen<PastimeGameScreen>("Pastime Game")?.Hide();
                                        ScreenManager.PushNavigation("Content");
                                        ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                                    }
                                }
                                // Set pagination
                                Pagination.SetPage(segment);
                            }
                            return true;
                        }
                    }
                }
                else
                {
                    hovering = false;
                }
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Menu Window
            GlobalContent.AddTexture("MenuWindow", ThemeManager.LoadLayeredContent<Texture2D>("graphics/menuwindow"));
            GlobalContent.AddTexture("MenuWindowBG", ThemeManager.LoadLayeredContent<Texture2D>("graphics/menuwindowbg"));
            GlobalContent.AddTexture("MenuSelected", ThemeManager.LoadLayeredContent<Texture2D>("graphics/menuselected"));
            GlobalContent.AddTexture("MenuSelected2", ThemeManager.LoadLayeredContent<Texture2D>("graphics/menuselected2"));
            GlobalContent.AddTexture("MenuSelected3", ThemeManager.LoadLayeredContent<Texture2D>("graphics/menuselected3"));
            if(!Global.canRender)
            {
                if(Global.pluginsLoaded)
                    Show();
                else
                    Hide();
            }
        }
    }
}
#endif
