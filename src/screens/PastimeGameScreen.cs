using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;
using Newtonsoft.Json;

namespace NonsensicalVideoGenerator
{
    public class PastimeGameObstacle
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
        public bool Update(GameTime gameTime, bool handleInput, int speed = 2, int spacingOverride = -1)
        {
            if(!isDead)
                distance -= speed;
            if(distance < -width)
            {
                point = false;
                distance = 320;
                if (spacingOverride > 0)
                    spacing = spacingOverride;
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
        public bool CheckCollision(Rectangle hitbox)
        {
            if (hitboxes[0].Intersects(hitbox) || hitboxes[1].Intersects(hitbox))
                return true;
            return false;
        }
    }
    public class PastimeGamePlayer
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
                    if(!Debug.gameCheat)
                        velocity = -jump;
                    GlobalContent.PlaySound("Option");
                    return true;
                }
            }
            else
            {
                distance += (int)(gameTime.ElapsedGameTime.TotalSeconds * 100);
                if(Debug.gameCheat)
                {
                    // Use mouse cursor to control the bird
                    spacingPlacementY = (MouseInput.MouseState.Y / GlobalGraphics.scale) - 1;
                    spacing = (MouseInput.MouseState.X / GlobalGraphics.scale) - 1;
                }
                else
                {
                    spacing =  (320 / 2) - (5/2) - 1;
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
                            GlobalContent.PlaySound("Hover");
                            return true;
                        }
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
    }
    /// <summary>
    /// This screen was made for April Fools 2023, it now functions as the credits screen.
    /// </summary>
    public class PastimeGameScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Game";
        public int layer { get; set; } = 1;
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
        private string currentCreditKey = "";
        private int currentCreditIndex = -1;
        private string creditsFile = "credits.json";
        private Dictionary<string, List<string>> credits = new();
        private PastimeGamePlayer player = new();
        private int baseObstacleSpeed = 2;
        private int obstacleSpeed = 2;
        private int minSpacing = 18; // Minimum gap between obstacles
        private int baseSpacing = 38;
        public float tryAgainY = -40f;
        private bool showTryAgain = false;
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
            if(Pagination.SelectedPage == 5)
            {
                // Gradually increase difficulty
                obstacleSpeed = baseObstacleSpeed + (player.points / 10); // Speed up every 10 points
                int newSpacing = baseSpacing - (player.points / 10) * 2; // Decrease gap every 10 points
                if (newSpacing < minSpacing) newSpacing = minSpacing;
                // Update obstacles
                foreach (PastimeGameObstacle obstacle in obstacles)
                {
                    obstacle.Update(gameTime, handleInput, obstacleSpeed, newSpacing);
                }
                if(handleInput)
                {
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(player.spacing), (int)GlobalGraphics.Scale(player.spacingPlacementY) + GlobalGraphics.Scale(player.height/2), GlobalGraphics.Scale(player.width), GlobalGraphics.Scale(player.height/2)), L.T(0, "Accessibility:GamePlayer"));
                }
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
                            GlobalContent.PlaySound("Error");
                            player.dead = true;
                            // timer in 0.05 seconds
                            timer = (float)gameTime.TotalGameTime.TotalMilliseconds + 50f;
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(10)), 0.01f)
                                    .Easing(EasingFunctions.Linear);
                            }
                            phase = 1;

                            // Show "Try Again!" message
                            showTryAgain = true;
                            tryAgainY = -40f;
                            tween.TweenTo(this, t => t.tryAgainY, GlobalGraphics.Scale(120), 0.25f)
                                .Easing(EasingFunctions.ExponentialOut);

                            break;
                        }
                        // Once past obstacle, earn point
                        else if(obstacle.distance + obstacle.spacing < player.spacing)
                        {
                            if(!obstacle.isDead && !obstacle.point)
                            {
                                obstacle.point = true;
                                GlobalContent.PlaySound("AddSource");
                                if(player.points < 2147483647)
                                    player.points++;
                                if(player.points == 2147483647)
                                    GlobalContent.PlaySound("RenderComplete");
                                // Increment currentCreditIndex
                                // If out of range of the key, increment currentCreditKey and set currentCreditIndex to -1
                                if(credits.Count > 0)
                                {
                                    if(currentCreditIndex == -1)
                                    {
                                        currentCreditIndex = 0;
                                    }
                                    else
                                    {
                                        currentCreditIndex++;
                                    }
                                    if(currentCreditIndex >= credits[currentCreditKey].Count)
                                    {
                                        currentCreditIndex = -1;
                                        foreach (string key in credits.Keys)
                                        {
                                            if(key == currentCreditKey)
                                            {
                                                currentCreditKey = "";
                                            }
                                            else if(currentCreditKey == "")
                                            {
                                                currentCreditKey = key;
                                                break;
                                            }
                                        }
                                        // Loop back to the beginning
                                        if(currentCreditKey == "")
                                        {
                                            foreach (string key in credits.Keys)
                                            {
                                                currentCreditKey = key;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if(player.points >= highScore)
                                {
                                    GlobalContent.PlaySound("Prompt");
                                    highScore = player.points;
                                    SaveData.saveValues["GameHighScore"] = highScore.ToString(CultureInfo.InvariantCulture);
                                    SaveData.Save();
                                }
                                if(player.points == 50 && !Global.highScore50)
                                {
                                    Global.highScore50 = true;
                                    string achievement = "ACHIEVEMENT_HIGH_SCORE";
                                    Achievements.Award(achievement);
                                }
                            }
                        }
                        if(phase == 0 && player.spacingPlacementY > 240-player.height)
                        {
                            GlobalContent.PlaySound("Error");
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
                            // Hide "Try Again!" after a short delay and reset on restart
                            showTryAgain = false;
                            tryAgainY = -40f;
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
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X)+offset.X, GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y)+offset.Y, 0));
            // Flappy bird clone
            // Draw all obstacles
            foreach (PastimeGameObstacle obstacle in obstacles)
            {
                obstacle.Draw(gameTime, spriteBatch);
            }
            // Draw player
            player.Draw(gameTime, spriteBatch);
            // Draw points
            SpriteFont font = L.FontSmall();
            string points = player.points.ToString(CultureInfo.InvariantCulture);
            if (player.points == 2147483647)
                points = "";
            Vector2 textSize = font.MeasureString(points);
            // Center horizontally
            if (!player.waiting && player.points < 2147483647)
            {
                GlobalContent.DrawString(spriteBatch, font, points, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8) + GlobalGraphics.Scale(1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, points, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8)), Color.White);
                // Draw score text
                string scoreText = L.T(0, "Game:Score");
                if(player.points == highScore)
                {
                    scoreText = L.T(0, "Game:HighScore");
                }
                textSize = font.MeasureString(scoreText);
                // Center horizontally
                GlobalContent.DrawString(spriteBatch, font, scoreText, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(24) + GlobalGraphics.Scale(1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, scoreText, new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(24)), Color.White);
            }
            else if(highScore < 2147483647)
            {
                // Draw high score text
                textSize = font.MeasureString(L.T(0, "Game:HighScore"));
                // Center horizontally
                GlobalContent.DrawString(spriteBatch, font, L.T(0, "Game:HighScore"), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(24) + GlobalGraphics.Scale(1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, L.T(0, "Game:HighScore"), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(24)), Color.White);
                // Draw high score
                textSize = font.MeasureString(highScore.ToString(CultureInfo.InvariantCulture));
                // Center horizontally
                GlobalContent.DrawString(spriteBatch, font, highScore.ToString(CultureInfo.InvariantCulture), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8) + GlobalGraphics.Scale(1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, highScore.ToString(CultureInfo.InvariantCulture), new Vector2(GlobalGraphics.Scale(160) - (textSize.X / 2), GlobalGraphics.Scale(240) - textSize.Y - GlobalGraphics.Scale(8)), Color.White);
            }
            string credit = "";
            if(credits.Count > 0 && currentCreditKey != "" && credits.ContainsKey(currentCreditKey))
            {
                if(currentCreditIndex == -1 || currentCreditIndex >= credits[currentCreditKey].Count)
                {
                    credit = L.T(0, "Game:Credits"+currentCreditKey);
                }
                else
                {
                    credit = credits[currentCreditKey][currentCreditIndex];
                }
            }
            textSize = font.MeasureString(credit);
            GlobalContent.DrawString(spriteBatch, font, credit, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9)), Color.Black);
            GlobalContent.DrawString(spriteBatch, font, credit, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8)), Color.White);
            // Draw render progress on right side (pastime)
            /*
            if(Global.generator.progressText != "")
            {
                textSize = font.MeasureString(Global.videoTitle);
                GlobalContent.DrawString(spriteBatch, font, Global.videoTitle, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9)), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, Global.videoTitle, new Vector2(GlobalGraphics.Scale(320) - textSize.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8)), Color.White);
                Vector2 textSize2 = font.MeasureString(Global.generator.progressText);
                GlobalContent.DrawString(spriteBatch, font, Global.generator.progressText, new Vector2(GlobalGraphics.Scale(320) - textSize2.X - GlobalGraphics.Scale(8), GlobalGraphics.Scale(9) + textSize.Y), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, Global.generator.progressText, new Vector2(GlobalGraphics.Scale(320) - textSize2.X - GlobalGraphics.Scale(9), GlobalGraphics.Scale(8) + textSize.Y), Color.White);
            }
            */
            // Draw "Try Again!" message if needed
            if (showTryAgain)
            {
                SpriteFont fontLarge = L.FontLarge();
                string msg = L.T(0, "Try Again!");
                Vector2 textSizeTryAgain = fontLarge.MeasureString(msg);
                Vector2 pos = new Vector2(GlobalGraphics.Scale(160) - textSizeTryAgain.X / 2, tryAgainY);
                GlobalContent.DrawString(spriteBatch, fontLarge, msg, pos + new Vector2(2, 2), Color.Black * 0.5f);
                GlobalContent.DrawString(spriteBatch, fontLarge, msg, pos, Color.White);
            }
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Set high score
            highScore = int.Parse(SaveData.saveValues["GameHighScore"], CultureInfo.InvariantCulture);
            // Load credits from credits.json
            credits.Clear();
            string creditPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", creditsFile);
            if (File.Exists(creditPath))
            {
                Dictionary<string, List<string>>? newCredits = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(creditPath));
                // Add start and end credits
                if (newCredits != null)
                {
                    credits.Add("Start", new List<string>());
                    foreach (string key in newCredits.Keys)
                    {
                        credits.Add(key, newCredits[key]);
                    }
                    credits.Add("End", new List<string>());
                    // Get first key
                    foreach (string key in credits.Keys)
                    {
                        currentCreditKey = key;
                        break;
                    }
                }
            }
            if (credits == null)
                ConsoleOutput.WriteLine("Could not find credits file: " + creditsFile, Color.Red);
        }
    }
}
