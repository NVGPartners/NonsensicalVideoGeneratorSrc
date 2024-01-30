#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    public class PastimeGameObstacle : IObject
    {
        public int distance = 320;
        public int spacingPlacementY = 200;
        public int spacing = 38;
        public static int width = 32;
        public int height = 240;
        public bool isDead = true;
        public bool point = false;
        public Rectangle[] hitboxes = new Rectangle[2];
        public PastimeGameObstacle(int offset)
        {
            distance = 320 + offset;
            spacingPlacementY = Global.generator.globalRandom.Next(spacing*2, height-(spacing*2));
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(!isDead)
                distance -= 2;
            if(distance < -width)
            {
                point = false;
                distance = 320;
                spacingPlacementY = Global.generator.globalRandom.Next(spacing*2, height-(spacing*2));
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Two lines, each 240 pixels long (max height of screen)
            // The spacingPlacementY variable determines which bar is higher, the top or the bottom.
            // The spacing variable determines the spacing between the two bars.
            hitboxes[0] = new Rectangle(GlobalGraphics.Scale(distance), GlobalGraphics.Scale(spacingPlacementY - (spacing/2)), GlobalGraphics.Scale(width), GlobalGraphics.Scale(height));
            hitboxes[1] = new Rectangle(GlobalGraphics.Scale(distance), GlobalGraphics.Scale(spacingPlacementY - (spacing*2) - height), GlobalGraphics.Scale(width), GlobalGraphics.Scale(height));
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), hitboxes[0], ThemeManager.GetColor("ObstaclePastimeGameScreen"));
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), hitboxes[1], ThemeManager.GetColor("ObstaclePastimeGameScreen"));
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
        public bool CheckCollision(Rectangle hitbox)
        {
            if (hitboxes[0].Intersects(hitbox) || hitboxes[1].Intersects(hitbox))
                return true;
            return false;
        }
    }
    public class PastimeGamePlayer : IObject
    {
        public int distance = 0;
        public float spacingPlacementY = 0f;
        public int spacing = 0;
        public int width = 0;
        public int height = 0;
        public float velocity = 0f;
        public float gravity = 0.1f;
        public float jump = 2.25f;
        public bool dead = false;
        public bool waiting = true;
        public int points = 0;
        public PastimeGamePlayer()
        {
            distance = 32;
            spacingPlacementY = 120;
            spacing =  (320 / 2) - (5/2) - 1;
            width = 5;
            height = 5;
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // time increases exponentially
            if(waiting)
            {
                // math.sin is used to make the bird bob up and down
                spacingPlacementY = (float)((240 / 2) - (height/2) + (Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4) * 10));
                if (handleInput && MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    waiting = false;
                    velocity = -jump;
                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
            }
            else
            {
                distance += (int)(gameTime.ElapsedGameTime.TotalSeconds * 100);
                velocity += gravity;
                spacingPlacementY += velocity;
                if(spacingPlacementY < -height)
                {
                    spacingPlacementY = -height;
                    velocity = 0;
                }
                if(!dead && handleInput)
                {
                    if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                    {
                        velocity = -jump;
                        GlobalContent.GetSound("Hover").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        return true;
                    }
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing) + GlobalGraphics.Scale(1), (int)GlobalGraphics.Scale(spacingPlacementY) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(width), GlobalGraphics.Scale(height)), Color.Black);
            //spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing), (int)GlobalGraphics.Scale(spacingPlacementY), GlobalGraphics.Scale(width), GlobalGraphics.Scale(height)), Color.White);
            // Draw a + sign with two lines that fit the bird's hitbox
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing) + GlobalGraphics.Scale(1) + GlobalGraphics.Scale(width/2), (int)GlobalGraphics.Scale(spacingPlacementY) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(1), GlobalGraphics.Scale(height)), Color.Black);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing) + GlobalGraphics.Scale(1), (int)GlobalGraphics.Scale(spacingPlacementY) + GlobalGraphics.Scale(1) + GlobalGraphics.Scale(height/2), GlobalGraphics.Scale(width), GlobalGraphics.Scale(1)), Color.Black);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing) + GlobalGraphics.Scale(width/2), (int)GlobalGraphics.Scale(spacingPlacementY), GlobalGraphics.Scale(1), GlobalGraphics.Scale(height)), Color.White);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(spacing), (int)GlobalGraphics.Scale(spacingPlacementY) + GlobalGraphics.Scale(height/2), GlobalGraphics.Scale(width), GlobalGraphics.Scale(1)), Color.White);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
    }
    /// <summary>
    /// This screen was made for April Fools 2023, it now functions as the credits screen.
    /// </summary>
    public class PastimeGameScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Pastime Game";
        public int layer { get; } = 1;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public float timer = 0f;
        public int phase = 0;
        public int highScore = 0;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private readonly List<PastimeGameObstacle> obstacles = new() {
            new PastimeGameObstacle(0),
            new PastimeGameObstacle((320/2) - (PastimeGameObstacle.width/4)),
        };
        private readonly List<string> highScoreTeases = new()
        {
            "Wowza!",
            "You're getting good at this!",
            "You're a pro!",
            "You're a god!",
            "How did you do that?",
            "Good job!",
            "Nice!",
            "What a score!",
            "You're a legend!",
            "Great work!",
        };
        private int currentCredit = 0;
        private readonly List<string> creditRoll = new()
        {
            "Score points to view credits!",
            "Lead Developer:",
            "kiwifruitdev",
            "Sound Designers:",
            "gmm2003",
            "bobbyiguess",
            "Moderators:",
            "0zne",
            "brettbagel",
            "spiral2839",
            "nuppington",
            "treycen",
            "Special Thanks:",
            "vinesauce",
            "meleekirby",
            "supositware",
            "devanwolf",
            "deemacias",
            "Superstars:",
            "thigo",
            "eddsworldfan69420",
            "fireafyanimations",
            "goggleskun",
            "1greg7",
            "gungholizard",
            "nobodyhasabandonedme",
            "jankespro12",
            "kraggerthemenace",
            "lindinhodebonito",
            "marcioleo123",
            "revilleaj",
            "rowster64",
            "thebritishytper",
            "thet00nedl00n",
            "madclown55",
            "waterdeervt",
            "OSS Licenses:",
            "MonoGame (Ms-PL)",
            "Newtonsoft.Json (MIT License)",
            "Steamworks.NET (MIT License)",
            "MoonSharp (BSD-3-Clause License)",
            "DiscordRichPresence (MIT License)",
            "yt-dlp (Unlicense)",
            "Font Licenses:",
            "Munro (SIL Open Font License)",
            "End of credits",
        };
        private PastimeGamePlayer player = new();
        public void Show()
        {
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(GlobalGraphics.scaledWidth, 0); // from bottom to top
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
                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.scaledWidth, 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(GlobalGraphics.scaledWidth, 0);
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
            // When animation is done, set screen type
            if (hiding && offset.X >= GlobalGraphics.scaledWidth)
            {
                screenType = ScreenType.Hidden;
                hiding = false;
            }
            else if (showing)
            {
                screenType = ScreenType.Drawn;
                showing = false;
                hiding = false;
                // Reset state
                obstacles.Clear();
                obstacles.Add(new PastimeGameObstacle(0));
                obstacles.Add(new PastimeGameObstacle(320 / 2));
                player = new PastimeGamePlayer();
            }
            handleInput = handleInput && !hiding && !showing;
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            // Update obstacles
            foreach (PastimeGameObstacle obstacle in obstacles)
            {
                obstacle.Update(gameTime, handleInput);
            }
            if(handleInput)
            {
                Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(player.spacing), (int)GlobalGraphics.Scale(player.spacingPlacementY) + GlobalGraphics.Scale(player.height/2), GlobalGraphics.Scale(player.width), GlobalGraphics.Scale(player.height/2)), "Player; Press Shift+Space to exit");
            }
            /*
            if (handleInput && MouseInput.MouseState.RightButton == ButtonState.Pressed && MouseInput.LastMouseState.RightButton == ButtonState.Released)
            {
                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Hide();
                ScreenManager.PushNavigation("Video");
                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                ScreenManager.PushNavigation("Content");
                ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                ScreenManager.PushNavigation("Header");
                ScreenManager.GetScreen<HeaderScreen>("Header")?.Show();
                ScreenManager.PushNavigation("Socials");
                ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                return true;
            }
            */
            // Query obstacles so player can collide with them
            if (!player.dead)
            {
                foreach (PastimeGameObstacle obstacle in obstacles)
                {
                    if(!player.waiting)
                    {
                        obstacle.isDead = false;
                    }
                    // If player is in an obstacle, die
                    if(obstacle.CheckCollision(new Rectangle(GlobalGraphics.Scale(player.spacing), GlobalGraphics.Scale((int)player.spacingPlacementY), GlobalGraphics.Scale(player.width), GlobalGraphics.Scale(player.height))))
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        player.dead = true;
                        // timer in 0.05 seconds
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                        if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                        {
                            tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(10)), 0.01f)
                                .Easing(EasingFunctions.Linear);
                        }
                        phase = 1;
                        break;
                    }
                    // Once past obstacle, earn point
                    else if(obstacle.distance + obstacle.spacing < player.spacing)
                    {
                        if(!obstacle.isDead && !obstacle.point)
                        {
                            obstacle.point = true;
                            GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            player.points++;
                            currentCredit++;
                            if(currentCredit > creditRoll.Count-1)
                            {
                                currentCredit = 1;
                            }
                            if(player.points >= highScore)
                            {
                                GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                highScore = player.points;
                                SaveData.saveValues["GameHighScore"] = highScore.ToString();
                                SaveData.Save();
                                // get random high score tease
                                int rand = Global.generator.globalRandom.Next(0, highScoreTeases.Count);
                                ConsoleOutput.WriteLine(highScoreTeases[rand] + " New high score: " + highScore, Color.Cyan);
                            }
                            if(player.points == 50 && !Global.highScore50)
                            {
                                Global.highScore50 = true;
                                string achievement = "ACHIEVEMENT_HIGH_SCORE";
                                ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                                SteamUserStats.SetAchievement(achievement);
                            }
                        }
                    }
                    if(phase == 0 && player.spacingPlacementY > 240-player.height)
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        player.dead = true;
                        // timer in 0.05 seconds
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                        if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                        {
                            tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(10)), 0.01f)
                                .Easing(EasingFunctions.Linear);
                        }
                        phase = 1;
                    }
                }
            }
            // Re-evaluate dead
            // Check timer
            if (phase > 0 && timer <= (float)gameTime.TotalGameTime.TotalMilliseconds)
            {
                switch(phase)
                {
                    case 1:
                        if(!hiding && !showing)
                        {
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(0, -GlobalGraphics.Scale(10)), 0.05f)
                                    .Easing(EasingFunctions.Linear);
                            }
                        }
                        phase = 2;
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                        break;
                    case 2:
                        if(!hiding && !showing)
                        {
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(10)), 0.05f)
                                    .Easing(EasingFunctions.Linear);
                            }
                        }
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                        phase = 6;
                        break;
                    case 6:
                        if(!hiding && !showing)
                        {
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(0)), 0.05f)
                                    .Easing(EasingFunctions.ExponentialInOut);
                            }
                        }
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                        phase = 0;
                        break;
                    case 3:
                        if(!hiding && !showing)
                        {
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320*2), GlobalGraphics.Scale(0)), 0.5f)
                                    .Easing(EasingFunctions.ExponentialInOut);
                            }
                        }
                        phase = 4;
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 500f;
                        break;
                    case 4:
                        player = new PastimeGamePlayer();
                        obstacles.Clear();
                        if(!hiding && !showing)
                        {
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                offset = new Vector2(GlobalGraphics.Scale(320*2), GlobalGraphics.Scale(0));
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(0), GlobalGraphics.Scale(0)), 0.5f)
                                    .Easing(EasingFunctions.ExponentialInOut);
                            }
                        }
                        phase = 5;
                        timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 500f;
                        break;
                    case 5:
                        obstacles.Add(new PastimeGameObstacle(0));
                        obstacles.Add(new PastimeGameObstacle(320 / 2));
                        phase = 0;
                        break;
                }
            }
            else if (phase == 0 && player.dead)
            {
                // Set all obstacles to dead
                foreach (PastimeGameObstacle obstacle in obstacles)
                {
                    obstacle.isDead = true;
                }
                phase = 3;
                timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
            }
            // Update player
            if(player.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Flappy bird clone
            // Draw all obstacles
            foreach (PastimeGameObstacle obstacle in obstacles)
            {
                obstacle.Draw(gameTime, spriteBatch);
            }
            // Draw player
            player.Draw(gameTime, spriteBatch);
            // Draw points
            SpriteFont font = GlobalContent.GetFont("MunroSmall");
            Vector2 textSize = font.MeasureString(player.points.ToString());
            // Center horizontally
            if (!player.waiting)
            {
                spriteBatch.DrawString(font, player.points.ToString(), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8) + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(font, player.points.ToString(), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8)), Color.White);
                // Draw score text
                string scoreText = "Score";
                if(player.points == highScore)
                {
                    scoreText = "High Score";
                }
                textSize = font.MeasureString(scoreText);
                // Center horizontally
                spriteBatch.DrawString(font, scoreText, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(16) + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(font, scoreText, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(16)), Color.White);
            }
            else
            {
                // Draw high score text
                textSize = font.MeasureString("High Score");
                // Center horizontally
                spriteBatch.DrawString(font, "High Score", new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(16) + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(font, "High Score", new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(16)), Color.White);
                // Draw high score
                textSize = font.MeasureString(highScore.ToString());
                // Center horizontally
                spriteBatch.DrawString(font, highScore.ToString(), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8) + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(font, highScore.ToString(), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8)), Color.White);
            }
            textSize = font.MeasureString(creditRoll[currentCredit]);
            spriteBatch.DrawString(font, creditRoll[currentCredit], new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9)), Color.Black);
            spriteBatch.DrawString(font, creditRoll[currentCredit], new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8)), Color.White);
            // Draw render progress on right side (pastime)
            /*
            if(Global.generator.progressText != "")
            {
                textSize = font.MeasureString(Global.videoTitle);
                spriteBatch.DrawString(font, Global.videoTitle, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9)), Color.Black);
                spriteBatch.DrawString(font, Global.videoTitle, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8)), Color.White);
                Vector2 textSize2 = font.MeasureString(Global.generator.progressText);
                spriteBatch.DrawString(font, Global.generator.progressText, new Vector2(GlobalGraphics.Scale(320) - textSize2.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9) + textSize.Y), Color.Black);
                spriteBatch.DrawString(font, Global.generator.progressText, new Vector2(GlobalGraphics.Scale(320) - textSize2.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8) + textSize.Y), Color.White);
            }
            */
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
            // Load player
            player.LoadContent(contentManager, graphicsDevice);
            foreach (PastimeGameObstacle obstacle in obstacles)
            {
                obstacle.LoadContent(contentManager, graphicsDevice);
            }
            // Set high score
            highScore = int.Parse(SaveData.saveValues["GameHighScore"], System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
