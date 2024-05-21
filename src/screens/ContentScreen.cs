#if MONOGAME
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This is the help screen.
    /// </summary>
    public class ContentScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Content";
        public int layer { get; set; } = 3;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private readonly InteractableController actionController = new();
        public void Show()
        {
            layer = 3;
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(0, GlobalGraphics.Scale(240)); // from bottom to top
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
                offset = new(0, 0); // from top to bottom
                tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(240)), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(0, GlobalGraphics.Scale(240));
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
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Pagination.SelectedPage != Pagination.TopPageCount)
            {
                if(actionController.Update(gameTime, handleInput))
                    return true;
            }
            // When animation is done, set screen type
            if (hiding && offset.Y == GlobalGraphics.Scale(240))
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
            // Pagination
            if(Pagination.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            if(Pagination.SelectedPage != Pagination.TopPageCount)
            {
                // Main Window
                Texture2D mainwindow = GlobalContent.GetTexture("MainWindow");
                spriteBatch.Draw(mainwindow, new Rectangle(GlobalGraphics.Scale(128-33), GlobalGraphics.Scale(36), GlobalGraphics.Scale(mainwindow.Width), GlobalGraphics.Scale(mainwindow.Height)), Color.White);
                // Draw the center title bar text.
                string pageTitle = Pagination.GetSubPageName();
                if(pageTitle.StartsWith("Page") && !pageTitle.Contains(" ") && !pageTitle.Contains("."))
                    pageTitle = L.T(0, pageTitle.Replace("Page","")+":Title");
                // Center within bounds of x 128 and x 312
                Vector2 titleSize = L.FontSmall().MeasureString(pageTitle);
                spriteBatch.DrawString(L.FontSmall(), pageTitle, new Vector2(GlobalGraphics.Scale(220) - titleSize.X / 2, GlobalGraphics.Scale(37)), Color.White);
                actionController.Draw(gameTime, spriteBatch);
            }
            // Pagination
            Pagination.Draw(gameTime, spriteBatch);
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            actionController.Clear();
            // Main Window
            GlobalContent.AddTexture("MainWindow", ThemeManager.LoadLayeredContent<Texture2D>("graphics/mainwindow"));
            // Pagination
            Pagination.LoadContent(contentManager, graphicsDevice);
            if(!Global.canRender)
            {
                if(Global.pluginsLoaded)
                    Show();
                else
                    Hide();
            }
            actionController.Add("ActionDiscord", new ActionButton("Join our Discord server!", new Vector2(112, 191), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if(Global.ready)
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            ProcessStartInfo psi = new()
                            {
                                FileName = "https://discord.gg/8ppmspR6Wh",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/discord")));
            actionController.Add("ActionSteam", new ActionButton("View the Steam Workshop!", new Vector2(112, 191+15), (int i, string n) => {
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
            actionController.Add("ActionGitHub", new ActionButton("View the issue tracker on GitHub!", new Vector2(112, 191+(15*2)), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if(Global.ready)
                        {
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            ProcessStartInfo psi = new()
                            {
                                FileName = "https://github.com/KiwifruitDev/NonsensicalVideoGenerator",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/github")));
        }
    }
}
#endif
