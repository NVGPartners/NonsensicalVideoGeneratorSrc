using System;
using System.Collections.Generic;
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
        public static void LoadScreens()
        {
            // Clear existing screens.
            drawnScreens.Clear();
            navigationStack.Clear();
            // Load every screen in the assembly.
            Type screenType = typeof(IScreen);
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => {
                    Type[] t = null;
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
        public static string GetTitle()
        {
            // Get the current screen from the navigation stack.
            if(navigationStack.Count > 0)
            {
                if(navigationStack.Peek() == "Modal")
                {
                    ModalScreen? screen = (ModalScreen?)drawnScreens.Find(x => x.title == "Modal");
                    if(screen != null)
                    {
                        return screen.modalTitle;
                    }
                }
                return navigationStack.Peek();
            }
            else
                return "Main Menu";
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
            // Handle mouse input, so that screens don't have to do that.
            // Only change if the window is active and the mouse is over the window.
            if(UserInterface.instance != null)
            {
                MouseInput.LastMouseState = MouseInput.MouseState;
                MouseInput._mouseState = Mouse.GetState();
            }
            Accessibility.allowAccessibility = UserInterface.instance.IsActive;
            bool handleInput = UserInterface.instance.IsActive && MouseInput.MouseState.X >= 0 && MouseInput.MouseState.X <= GlobalGraphics.scaledWidth &&
                MouseInput.MouseState.Y >= 0 && MouseInput.MouseState.Y <= GlobalGraphics.scaledHeight && !Global.dragDrop;
            if(Accessibility.PreUpdate(gameTime))
                handleInput = false;
            // Update the drawn screens in layer order and reversed.
            List<IScreen> orderedScreens = drawnScreens.OrderBy(s => s.layer).ToList();
            orderedScreens.Reverse();
            for(int i = 0; i < orderedScreens.Count; i++)
            {
                if(orderedScreens[i].Update(gameTime, orderedScreens[i].screenType == ScreenType.Drawn && handleInput))
                {
                    handleInput = false;
                }
            }
            Accessibility.PostUpdate(gameTime);
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
        public static void ShowModal(string title, string[] message, string[] buttons, int defaultButton, Action<int> callback)
        {
            // Push modal screen.
            PushNavigation("Modal");
            // Set the modal screen's properties.
            ModalScreen? modalScreen = (ModalScreen?)drawnScreens.Find(s => s.title == "Modal");
            if(modalScreen != null)
            {
                modalScreen.modalTitle = title;
                modalScreen.modalText = message;
                modalScreen.buttons = buttons;
                modalScreen.defaultButton = defaultButton;
                modalScreen.callback = callback;
            }
        }
        public static void ShowError(string title, string[] message, string[] buttons, int defaultButton, Action<int> callback)
        {
            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
            ShowModal(title, message, buttons, defaultButton, callback);
        }
    }
}