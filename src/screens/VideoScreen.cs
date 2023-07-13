using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;

namespace YTPPlusPlusPlus
{
    public class VideoScreen : IScreen
    {
        public string title { get; } = "Video";
        public int layer { get; } = 2;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        List<string> lines = new()
        {
            Global.productName + " v" + Global.productVersion + " (Steam)",
            " ",
            "If you find any bugs,",
            "please report them on",
            "the GitHub issues page.",
            "Thank you, and enjoy!"
        };
        public void Show()
        {
            toggle = true;
            offset = new(GlobalGraphics.Scale(-124), 0); // from left to right
            tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            offset = new(0, 0); // from right to left
            tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-124), 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
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
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Video Window
            Texture2D vidwindow = GlobalContent.GetTexture("VidWindow");
            spriteBatch.Draw(vidwindow, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(36), GlobalGraphics.Scale(vidwindow.Width), GlobalGraphics.Scale(vidwindow.Height)), Color.White);
            /*
            Texture2D vidbutton = GlobalContent.GetTexture("VidButton");
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(14), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(47), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(80), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            Texture2D vidbg = GlobalContent.GetTexture("VidBG");
            spriteBatch.Draw(vidbg, new Rectangle(GlobalGraphics.Scale(6), GlobalGraphics.Scale(45), GlobalGraphics.Scale(vidbg.Width), GlobalGraphics.Scale(vidbg.Height)), Color.White);
            */
            SpriteFont munro = GlobalContent.GetFont("MunroSmall");
            Vector2 lastPos = new(GlobalGraphics.Scale(8), GlobalGraphics.Scale(45));
            for(int i = 0; i < lines.Count; i++)
            {
                spriteBatch.DrawString(munro, lines[i], new Vector2(lastPos.X + GlobalGraphics.Scale(1), lastPos.Y + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(munro, lines[i], new Vector2(lastPos.X, lastPos.Y), Color.White);
                lastPos.Y += munro.MeasureString(lines[i]).Y;
            }
            // Draw window title on left side (90 degrees)
            string altTitle = "Information";
            Vector2 titleSize = munro.MeasureString(altTitle);
            spriteBatch.DrawString(munro, altTitle, new Vector2(GlobalGraphics.Scale(111), GlobalGraphics.Scale(108)), Color.White, MathHelper.ToRadians(90), new Vector2(titleSize.X, titleSize.Y), 1, SpriteEffects.None, 0);
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
            // Video Window
            GlobalContent.AddTexture("VidWindow", contentManager.Load<Texture2D>("graphics/vidwindow"));
            GlobalContent.AddTexture("VidButton", contentManager.Load<Texture2D>("graphics/vidbutton"));
            GlobalContent.AddTexture("VidBG", contentManager.Load<Texture2D>("graphics/vidbg"));
        }
    }
}
