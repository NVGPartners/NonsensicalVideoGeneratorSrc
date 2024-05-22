#if MONOGAME
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Debug page.
    /// </summary>
    public class InternalPage : IPage
    {
        public string Name { get; set; } = "PageDebug";
        public string Tooltip { get; } = "";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            if(MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
            {
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+6))
                {
                    // Search for the next logical entry in locales/*.json
                    string currentLocale = L.GetLocale().name;
                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", L.localeFolder);
                    if(!File.Exists(Path.Combine(path, currentLocale + ".json")))
                        currentLocale = L.defaultLocale;
                    string[] files = Directory.GetFiles(path, "*.json");
                    string newLocale = currentLocale;
                    for(int i = 0; i < files.Length; i++)
                    {
                        if(files[i].EndsWith(currentLocale + ".json"))
                        {
                            // Get the next logical entry.
                            if(i + 1 < files.Length)
                            {
                                newLocale = Path.GetFileNameWithoutExtension(files[i + 1]);
                                break;
                            }
                            // Wrap around to the first entry.
                            newLocale = Path.GetFileNameWithoutExtension(files[0]);
                            break;
                        }
                    }
                    newLocale = newLocale.Replace(".json", "");
                    L.LoadLocale(newLocale);
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+9) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+9+6))
                {
                    // Generate a new random seed.
                    Global.randomSeed = new Random().Next();
                    Global.generator.globalRandom = new Random(Global.randomSeed);
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*2)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*2)+6))
                {
                    // Toggle user resizing.
                    UserInterface.instance.Window.AllowUserResizing = !UserInterface.instance.Window.AllowUserResizing;
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*3)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*3)+6))
                {
                    // Toggle game cheat.
                    Debug.gameCheat = !Debug.gameCheat;
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*4)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*4)+6))
                {
                    // Toggle screen scale.
                    if(SaveData.saveValues["ScreenScale"] == "1")
                        SaveData.saveValues["ScreenScale"] = "2";
                    else if(SaveData.saveValues["ScreenScale"] == "2")
                        SaveData.saveValues["ScreenScale"] = "3";
                    else if(SaveData.saveValues["ScreenScale"] == "3")
                        SaveData.saveValues["ScreenScale"] = "4";
                    else if(SaveData.saveValues["ScreenScale"] == "4")
                        SaveData.saveValues["ScreenScale"] = "1";
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*5)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*5)+6))
                {
                    // Toggle console.
                    ConsoleScreen? consoleScreen = ScreenManager.GetScreen<ConsoleScreen>("Console");
                    if(consoleScreen != null)
                    {
                        ConsoleOutput.ResetScroll();
                        if(consoleScreen.offset.X == 0)
                        {
                            consoleScreen.screenType = ScreenType.Drawn;
                            consoleScreen.offset = new Vector2(GlobalGraphics.scaledWidth, 0);
                            UserInterface.instance.Resize(GlobalGraphics.scaledWidth*2, GlobalGraphics.scaledHeight);
                        }
                        else
                        {
                            consoleScreen.screenType = ScreenType.Hidden;
                            consoleScreen.offset = new Vector2(0, GlobalGraphics.scaledHeight);
                            UserInterface.instance.Resize(GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight);
                        }
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*6)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*6)+6))
                {
                    // Toggle speed boost.
                    Debug.debugSpeedBoost++;
                    if(Debug.debugSpeedBoost > 10)
                        Debug.debugSpeedBoost = 2;
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
                if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(9*7)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(9*7)+6))
                {
                    // Set draw offset X to 53 and resize to 427x240 (wide screen).
                    // If draw offset X is already 53, set it to 0 and resize to 320x240 (standard screen).
                    if(Global.drawOffset.X == 0)
                    {
                        Global.drawOffset = new Vector2(53, 0);
                        UserInterface.instance.Resize(GlobalGraphics.Scale(427), GlobalGraphics.Scale(240));
                    }
                    else
                    {
                        Global.drawOffset = new Vector2(0, 0);
                        UserInterface.instance.Resize(GlobalGraphics.Scale(320), GlobalGraphics.Scale(240));
                    }
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            controller.Draw(gameTime, spriteBatch);
            if(Debug.GetDebugMode())
            {
                DrawButton(spriteBatch, 10, 33, "Locale: " + L.GetLocale().name + " " + L.GetLocale().localizedName);
                DrawButton(spriteBatch, 10, 33+9, "Random Seed: " + Global.randomSeed.ToString(CultureInfo.InvariantCulture));
                DrawButton(spriteBatch, 10, 33+(9*2), "User Resizing: " + (UserInterface.instance.Window.AllowUserResizing ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 10, 33+(9*3), "Game Cheat: " + (Debug.gameCheat ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 10, 33+(9*4), "Screen Scale: " + SaveData.saveValues["ScreenScale"]);
                DrawButton(spriteBatch, 10, 33+(9*5), "Toggle Console On Right");
                DrawButton(spriteBatch, 10, 33+(9*6), "Speed Boost: x" + Debug.debugSpeedBoost);
                DrawButton(spriteBatch, 10, 33+(9*7), "Draw Offset: " + Global.drawOffset.X.ToString(CultureInfo.InvariantCulture) + ", " + Global.drawOffset.Y.ToString(CultureInfo.InvariantCulture));
                spriteBatch.DrawString(L.FontSmall(), "Mouse: " + MouseInput.MouseState.Position.X.ToString(CultureInfo.InvariantCulture) + ", " + MouseInput.MouseState.Position.Y.ToString(CultureInfo.InvariantCulture), new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*8) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), Debug.debugBuild ? "Debug Build" : (Debug.GetDebugMode() ? "Release Build; Debug Mode" : "Release Build"), new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*7) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "CTRL + F3 / F20: Toggle Debug Mode", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*6) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "F4: Toggle Main Window Tween", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*5) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "F6: Pause", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*4) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "F7: Advance Frame", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*3) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "F8: Speed Boost", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10*2) - GlobalGraphics.Scale(10)), Color.White);
                spriteBatch.DrawString(L.FontSmall(), "F9: Reload Locales", new Vector2(GlobalGraphics.Scale(10), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(10) - GlobalGraphics.Scale(10)), Color.White);
            }
        }
        public void DrawButton(SpriteBatch spriteBatch, int x, int y, string text)
        {
            GlobalGraphics.DrawButton(spriteBatch, GlobalGraphics.Scale(x), GlobalGraphics.Scale(y), Color.Transparent, text, Color.White, Color.Gray);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
