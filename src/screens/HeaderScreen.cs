using System;
using System.Collections.Generic;

#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;
#endif


namespace NonsensicalVideoGenerator
{
#if MONOGAME
    public class ClickHeader
    {
        public Rectangle clickOffset = new();
        public Rectangle clickOffset2 = new();
        public double timeClicked = 0;
        public float growth = 0.25f;
        public Color clickColor = new();
        public ClickHeader(Rectangle clickOffset, Rectangle clickOffset2, double timeClicked, float growth = 0.25f, Color clickColor = new())
        {
            this.clickOffset = clickOffset;
            this.clickOffset2 = clickOffset2;
            this.timeClicked = timeClicked;
            this.growth = growth;
            this.clickColor = clickColor;
        }
    }
    /// <summary>
    /// This is the about screen.
    /// </summary>
    public class HeaderScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Header";
        public int layer { get; } = 4;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        public List<ClickHeader> clickHeaderList = new();
        private readonly Tweener tween = new();
        public float jump = 0f;
        private bool jumping = false;
        private bool down = false;
        private double jumpTime = 0;
        public void Show()
        {
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(GlobalGraphics.Scale(-320), 0); // from left to right
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
                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(GlobalGraphics.Scale(-320), 0);
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
            // Logo.
            SpriteFont font = GlobalContent.GetFont("MunroSmall");

            /*
            Texture2D logobg = GlobalContent.GetTexture("LogoBG");
            spriteBatch.Draw(logobg, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(7), GlobalGraphics.Scale(logobg.Width), GlobalGraphics.Scale(logobg.Height)), Color.White);
            spriteBatch.DrawString(font, "v" + Global.productVersion, new Vector2(GlobalGraphics.Scale(7+1), GlobalGraphics.Scale(8+1)), Color.Black);
            spriteBatch.DrawString(font, "v" + Global.productVersion, new Vector2(GlobalGraphics.Scale(7), GlobalGraphics.Scale(8)), Color.White);
            spriteBatch.DrawString(font, "Steam Version", new Vector2(GlobalGraphics.Scale(7+1), GlobalGraphics.Scale(18+1)), Color.Black);
            spriteBatch.DrawString(font, "Steam Version", new Vector2(GlobalGraphics.Scale(7), GlobalGraphics.Scale(18)), Color.White);
            */

            Texture2D logomask = GlobalContent.GetTexture("LogoMask");
            Texture2D logo = GlobalContent.GetTexture("Logo");
            Texture2D iconmask = GlobalContent.GetTexture("IconMask");
            Texture2D icon = GlobalContent.GetTexture("Icon");
            for(int i = clickHeaderList.Count - 1; i >= 0; i--)
            {
                Rectangle clickOffset = clickHeaderList[i].clickOffset;
                Rectangle clickOffset2 = clickHeaderList[i].clickOffset2;
                Color clickColor = clickHeaderList[i].clickColor;
                spriteBatch.Draw(logomask, new Rectangle(GlobalGraphics.Scale(clickOffset.X), GlobalGraphics.Scale(clickOffset.Y + (int)jump), GlobalGraphics.Scale(clickOffset.Width), GlobalGraphics.Scale(clickOffset.Height)), clickColor);
                spriteBatch.Draw(iconmask, new Rectangle(GlobalGraphics.Scale(clickOffset2.X), GlobalGraphics.Scale(clickOffset2.Y + (int)jump), GlobalGraphics.Scale(clickOffset2.Width), GlobalGraphics.Scale(clickOffset2.Height)), clickColor);
            }
            spriteBatch.Draw(logo, new Rectangle(GlobalGraphics.Scale(11 + 21 + 1), GlobalGraphics.Scale(10 + 1 + (int)jump), GlobalGraphics.Scale(logo.Width), GlobalGraphics.Scale(logo.Height)), Color.Black);
            spriteBatch.Draw(logo, new Rectangle(GlobalGraphics.Scale(11 + 21), GlobalGraphics.Scale(10 + (int)jump), GlobalGraphics.Scale(logo.Width), GlobalGraphics.Scale(logo.Height)), Color.White);
            spriteBatch.Draw(icon, new Rectangle(GlobalGraphics.Scale(11 + 1), GlobalGraphics.Scale(10 + 1 + (int)jump), GlobalGraphics.Scale(icon.Width), GlobalGraphics.Scale(icon.Height)), Color.Black);
            spriteBatch.Draw(icon, new Rectangle(GlobalGraphics.Scale(11), GlobalGraphics.Scale(10 + (int)jump), GlobalGraphics.Scale(icon.Width), GlobalGraphics.Scale(icon.Height)), Color.White);

            // Draw rendering progress
            if(Global.generatorFactory.progressText != "")
            {
                string rendering = Global.videoTitle;
                // measure to center horizontally (one on top of the other)
                Vector2 renderingSize = font.MeasureString(rendering);
                Vector2 progressSize = font.MeasureString(Global.generatorFactory.progressText != "" ? Global.generatorFactory.progressText : (Global.generatorFactory.failureReason != "" ? Global.generatorFactory.failureReason : Global.generatorFactory.progress + "%"));
                spriteBatch.DrawString(font, rendering, new Vector2(GlobalGraphics.Scale(320/2) - renderingSize.X/2 + GlobalGraphics.Scale(1), GlobalGraphics.Scale(8 + 1)), Color.Black);
                spriteBatch.DrawString(font, Global.generatorFactory.progressText, new Vector2(GlobalGraphics.Scale(320/2) - progressSize.X/2 + GlobalGraphics.Scale(1), GlobalGraphics.Scale(8 + 1) + renderingSize.Y), Color.Black);
                spriteBatch.DrawString(font, rendering, new Vector2(GlobalGraphics.Scale(320/2) - renderingSize.X/2, GlobalGraphics.Scale(8)), Color.White);
                spriteBatch.DrawString(font, Global.generatorFactory.progressText, new Vector2(GlobalGraphics.Scale(320/2) - progressSize.X/2, GlobalGraphics.Scale(8) + renderingSize.Y), Color.White);
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
            if (hiding && offset.X == GlobalGraphics.Scale(-320))
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
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                for(int i = 0; i < clickHeaderList.Count; i++)
                {
                    double timeClicked = clickHeaderList[i].timeClicked;
                    Rectangle clickOffset = clickHeaderList[i].clickOffset;
                    Rectangle clickOffset2 = clickHeaderList[i].clickOffset2;
                    float growth = clickHeaderList[i].growth;
                    // Make the logo mask zoom and fade out with timeClicked
                    // Color is still translucent
                    int col = (int)(128 * (1 - (gameTime.TotalGameTime.TotalSeconds - timeClicked) / 0.5));
                    // Increase width/height over time
                    Texture2D logo = GlobalContent.GetTexture("Logo");
                    Vector2 pivot = new Vector2(logo.Width / 2, logo.Height / 2);
                    int width = (int)(logo.Width * (1 + growth * (gameTime.TotalGameTime.TotalSeconds - timeClicked) / 0.5));
                    int height = (int)(logo.Height * (1 + growth * (gameTime.TotalGameTime.TotalSeconds - timeClicked) / 0.5));
                    // Offset
                    clickOffset = new Rectangle((int)(11 + 21 - (width - logo.Width) / 2), (int)(10 - (height - logo.Height) / 2), width, height);
                    // Increase width/height over time
                    Texture2D icon = GlobalContent.GetTexture("Icon");
                    Vector2 pivot2 = new Vector2(icon.Width / 2, icon.Height / 2);
                    int width2 = (int)(icon.Width * (1 + growth * (gameTime.TotalGameTime.TotalSeconds - timeClicked) / 0.5));
                    int height2 = (int)(icon.Height * (1 + growth * (gameTime.TotalGameTime.TotalSeconds - timeClicked) / 0.5));
                    // Offset
                    clickOffset2 = new Rectangle((int)(11 - (width2 - icon.Width) / 2), (int)(10 - (height2 - icon.Height) / 2), width2, height2);
                    // Apply
                    clickHeaderList[i].clickOffset = clickOffset;
                    clickHeaderList[i].clickOffset2 = clickOffset2;
                    clickHeaderList[i].clickColor = new Color(col, col, col, -col);
                    // Remove if time is up
                    if(gameTime.TotalGameTime.TotalSeconds - timeClicked >= 0.5)
                    {
                        clickHeaderList.RemoveAt(i);
                        i--;
                    }
                }
                // Jump
                if(jumping)
                {
                    // If we're going down, set down to true
                    if(jump < 0 && !down)
                    {
                        down = true;
                    }
                    // If down is true and we've reached 0, stop jumping
                    if(down && jump >= 0)
                    {
                        jumping = false;
                        jump = 0f;
                    }
                    // Jump high and fast
                    jump = (float)(-Math.Sin((gameTime.TotalGameTime.TotalSeconds - jumpTime) * 10) * 5);
                }
            }
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if(hiding || screenType == ScreenType.Hidden)
                return false;
            if(handleInput)
            {
                // Allow clicking on the logo
                if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                {
                    Texture2D logo = GlobalContent.GetTexture("Logo");
                    if(MouseInput.MouseState.X >= GlobalGraphics.Scale(11) && MouseInput.MouseState.X <= GlobalGraphics.Scale(11) + GlobalGraphics.Scale(logo.Width) + GlobalGraphics.Scale(21) && MouseInput.MouseState.Y >= GlobalGraphics.Scale(10) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(10) + GlobalGraphics.Scale(logo.Height))
                    {
                        if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                        {
                            Texture2D icon = GlobalContent.GetTexture("Icon");
                            Color clickColor = new Color(255, 255, 255, 255);
                            Rectangle clickOffset = new Rectangle(11 + 21, 10, logo.Width, logo.Height);
                            Rectangle clickOffset2 = new Rectangle(11, 10, icon.Width, icon.Height);
                            float growth = Global.generatorFactory.globalRandom.Next(20, 30) / 100f;
                            clickHeaderList.Add(new ClickHeader(clickOffset, clickOffset2, gameTime.TotalGameTime.TotalSeconds, growth, clickColor));
                        }
                        // Roll for jump
                        if(!jumping)
                        {
                            if(Global.generatorFactory.globalRandom.Next(0, 100) < 10)
                            {
                                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                                {
                                    jump = 0f;
                                    jumpTime = gameTime.TotalGameTime.TotalSeconds;
                                    jumping = true;
                                    down = false;
                                }
                                GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                        }
                        else
                        {
                            GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Logo.
            GlobalContent.AddTexture("Logo", contentManager.Load<Texture2D>("graphics/logo"));
            GlobalContent.AddTexture("LogoMask", contentManager.Load<Texture2D>("graphics/logomask"));
            GlobalContent.AddTexture("Icon", contentManager.Load<Texture2D>("graphics/icon"));
            GlobalContent.AddTexture("IconMask", contentManager.Load<Texture2D>("graphics/iconmask"));
            GlobalContent.AddTexture("LogoBG", contentManager.Load<Texture2D>("graphics/bannerbg"));
        }
    }
#endif
}
