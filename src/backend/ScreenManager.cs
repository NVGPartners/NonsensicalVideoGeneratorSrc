#if MONOGAME
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{
    public static class ScreenManager
    {
        /// <summary>
        /// This is the list of screens to draw, ordered by layer. The boolean value indicates whether the screen is visible.
        /// </summary>
        public static List<IScreen> drawnScreens = new List<IScreen>();
        /// <summary>
        /// Navigation stack of all screen names.
        /// </summary>
        public static Stack<string> navigationStack { get; set; } = new Stack<string>();
        public static KeyboardState lastKeyboardState;
        public static KeyboardState keyboardState;
        public static bool hideEverything = false;
        public static void LoadScreens()
        {
            // Clear existing screens.
            drawnScreens.Clear();
            navigationStack.Clear();
            // Load every screen in the assembly.
            Type screenType = typeof(IScreen);
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => {
                    Type[] t = new Type[0];
                    try
                    {
                        t = s.GetTypes();
                    }
                    catch(Exception)
                    {
                        t = new Type[0];
                    }
                    return t;
                }).Where(p => screenType.IsAssignableFrom(p) && p.IsClass).ToArray();
            foreach (Type type in types)
            {
                // Add the screen to the list.
                IScreen? screen = (IScreen?)Activator.CreateInstance(type);
                if(screen != null)
                {
                    // Set drawnScreens.
                    drawnScreens.Add(screen);
                    /*
                    switch(screen.screenType)
                    {
                        case ScreenType.Drawn:
                            ConsoleOutput.WriteLine("Drawn screen: " + screen.title + " " + screen.layer);
                            break;
                        case ScreenType.Hidden:
                            ConsoleOutput.WriteLine("Hidden screen: " + screen.title + " " + screen.layer);
                            break;
                    }
                    */
                }
            }
        }
        public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                drawnScreens[i].LoadContent(contentManager, graphicsDevice);
            }
        }
        public static void PushNavigation(string name)
        {
            // Find the screen with the matching name.
            IScreen? screen = null;
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                if(drawnScreens[i].title == name)
                {
                    screen = drawnScreens[i];
                    break;
                }
            }
            if(screen == null)
            {
                ConsoleOutput.WriteLine("Screen not found: " + name, Color.Red);
                return;
            }
            // Don't re-push the same screen.
            if(navigationStack.Count > 0 && navigationStack.Peek() == name)
            {
                return;
            }
            // If the screen takes up an existing layer, set the other layer to be hidden.
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                if(drawnScreens[i].layer == screen.layer && drawnScreens[i].screenType == ScreenType.Drawn)
                {
                    drawnScreens[i].screenType = ScreenType.Hidden;
                    drawnScreens[i].currentPlacement = navigationStack.Count;
                    //ConsoleOutput.WriteLine("Hidden screen: " + drawnScreens[i].title + " " + drawnScreens[i].layer);
                }
            }
            // Set the screen to be drawn.
            screen.screenType = ScreenType.Drawn;
            screen.currentPlacement = -1;
            // Push the screen to the navigation stack.
            navigationStack.Push(name);
            //ConsoleOutput.WriteLine("Drawn screen: " + screen.title + " " + screen.layer);
        }
        public static void PopNavigation()
        {
            if(!CanPopNavigation())
                return;
            // Remove the current screen from the navigation stack.
            string hideScreen = navigationStack.Pop();
            // Find the screen with the matching name.
            IScreen? screen = null;
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                if(drawnScreens[i].title == hideScreen)
                {
                    screen = drawnScreens[i];
                    break;
                }
            }
            if(screen == null)
            {
                ConsoleOutput.WriteLine("Screen not found: " + hideScreen, Color.Red);
                return;
            }
            // Set the screen to be hidden.
            screen.screenType = ScreenType.Hidden;
            screen.currentPlacement = -1;
            // Find the previous screen in the navigation stack using the currentPlacement.
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                if(drawnScreens[i].layer == screen.layer && drawnScreens[i].currentPlacement == navigationStack.Count && drawnScreens[i].screenType == ScreenType.Hidden)
                {
                    // Set the previous screen to be drawn.
                    drawnScreens[i].screenType = ScreenType.Drawn;
                    drawnScreens[i].currentPlacement = -1;
                    //ConsoleOutput.WriteLine("Drawn screen: " + drawnScreens[i].title + " " + drawnScreens[i].layer);
                }
            }
            //ConsoleOutput.WriteLine("Hidden screen: " + screen.title + " " + screen.layer);
        }
        public static bool CanPopNavigation()
        {
            return navigationStack.Count > 0;
        }
        public static T? GetScreen<T>(string name) where T : IScreen
        {
            // Find the screen with the matching name.
            IScreen? screen = null;
            for(int i = 0; i < drawnScreens.Count; i++)
            {
                if(drawnScreens[i].title == name)
                {
                    screen = drawnScreens[i];
                    break;
                }
            }
            if(screen == null)
            {
                ConsoleOutput.WriteLine("Screen not found: " + name, Color.Red);
                return default;
            }
            return (T)screen;
        }
        public static void Update(GameTime gameTime)
        {
            if(Debug.frame)
                Debug.paused = false;
            lastKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            // Handle mouse input, so that screens don't have to do that.
            // Only change if the window is active and the mouse is over the window.
            if(UserInterface.instance != null)
            {
                MouseInput.LastMouseState = MouseInput.MouseState;
                if((Accessibility.allowAccessibility && !Accessibility.showDisambiguation)
                    || (UserInterface.instance.IsActive && MouseInput.MouseState.X >= 0 && MouseInput.MouseState.X <= GlobalGraphics.scaledWidth
                    && MouseInput.MouseState.Y >= 0 && MouseInput.MouseState.Y <= GlobalGraphics.scaledHeight && !Global.dragDrop))
                {
                    MouseInput._mouseState = Mouse.GetState();
                }
            }
            bool handleInput = Accessibility.showDisambiguation || (UserInterface.instance != null && UserInterface.instance.IsActive && MouseInput.MouseState.X >= 0 && MouseInput.MouseState.X <= GlobalGraphics.scaledWidth &&
                MouseInput.MouseState.Y >= 0 && MouseInput.MouseState.Y <= GlobalGraphics.scaledHeight && !Global.dragDrop);
            if(!Debug.paused)
            {
                if(Accessibility.PreUpdate(gameTime))
                    handleInput = false;
            }
            // Update the drawn screens in layer order and reversed.
            List<IScreen> orderedScreens = drawnScreens.OrderBy(s => s.layer).ToList();
            orderedScreens.Reverse();
            if(!Debug.paused)
            {
                for(int i = 0; i < orderedScreens.Count; i++)
                {
                    if(orderedScreens[i].Update(gameTime, orderedScreens[i].screenType == ScreenType.Drawn && handleInput))
                    {
                        handleInput = false;
                    }
                }
            }
            // Toggle debug mode
            // F20 or CTRL+F3 will toggle debug mode
            if((keyboardState.IsKeyDown(Keys.F20)
                || (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.F3)))
                && lastKeyboardState.IsKeyUp(Keys.F20)
                && (lastKeyboardState.IsKeyUp(Keys.LeftControl)
                || lastKeyboardState.IsKeyUp(Keys.F3)))
            {
                Debug.SetDebugMode(!Debug.GetDebugMode());
            }
            // DEBUG
            if(Debug.GetDebugMode())
            {
                // F4 will toggle hiding everything
                if(keyboardState.IsKeyDown(Keys.F4) && lastKeyboardState.IsKeyUp(Keys.F4))
                {
                    hideEverything = !hideEverything;
                    if(hideEverything)
                    {
                        GetScreen<ContentScreen>("Content")?.Hide();
                        GetScreen<HeaderScreen>("Header")?.Hide();
                        GetScreen<VideoScreen>("Video")?.Hide();
                        GetScreen<MenuScreen>("Menu")?.Hide();
                        GetScreen<SocialScreen>("Socials")?.Hide();
                    }
                    else
                    {
                        PushNavigation("Content");
                        PushNavigation("Header");
                        PushNavigation("Video");
                        PushNavigation("Menu");
                        PushNavigation("Socials");
                        GetScreen<ContentScreen>("Content")?.Show();
                        GetScreen<HeaderScreen>("Header")?.Show();
                        GetScreen<VideoScreen>("Video")?.Show();
                        GetScreen<MenuScreen>("Menu")?.Show();
                        GetScreen<SocialScreen>("Socials")?.Show();
                    }
                }
                // F6 will pause
                if(keyboardState.IsKeyDown(Keys.F6) && lastKeyboardState.IsKeyUp(Keys.F6))
                {
                    Debug.paused = !Debug.paused;
                }
                // F7 will advance a frame
                if(keyboardState.IsKeyDown(Keys.F7) && lastKeyboardState.IsKeyUp(Keys.F7))
                {
                    Debug.frame = true;
                }
                // F8 will call update 2x
                if(keyboardState.IsKeyDown(Keys.F8) && !Debug.debugSpeedDebounce)
                {
                    Debug.debugSpeedDebounce = true;
                    for(int i = 0; i < Debug.debugSpeedBoost-1; i++)
                    {
                        Update(gameTime);
                    }
                    Debug.debugSpeedDebounce = false;
                }
            }
            if(!Debug.paused)
                Accessibility.PostUpdate(gameTime);
            if(!Debug.paused && Debug.frame)
            {
                Debug.frame = false;
                Debug.paused = true;
            }
        }
        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Accessibility.PreDraw(gameTime, spriteBatch);
            // Draw the screens in layer order.
            List<IScreen> orderedScreens = drawnScreens.OrderBy(s => s.layer).ToList();
            for(int i = 0; i < orderedScreens.Count; i++)
            {
                if(orderedScreens[i].screenType == ScreenType.Drawn)
                {
                    orderedScreens[i].Draw(gameTime, spriteBatch);
                }
            }
            Accessibility.PostDraw(gameTime, spriteBatch);
        }
    }
}
#endif
