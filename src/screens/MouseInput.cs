#if MONOGAME
using System;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{ 
    class MouseInput
    {
        public static MouseState _mouseState;
        private static MouseState lastMouseState;
        public static MouseState MouseState
        {
            get { return CompatMouseState(); }
            set { }
        }
        public static MouseState CompatMouseState()
        {
            if(Accessibility.allowAccessibility)
            {
                // Hover over accessibility options if active
                if(Accessibility.showDisambiguation && Accessibility.disambiguationOptions.Count > 0 && Accessibility.disambiguationOptions.Count > -(Accessibility.offset))
                {
                    int x = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.X + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Y + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Height / 2;
                    _mouseState = new MouseState(x, y, _mouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
                // Build mouse state from accessibility if active
                if(Accessibility.selectedDisambiguationOption != -1 && Accessibility.disambiguationOptions.Count > Accessibility.selectedDisambiguationOption)
                {
                    int x = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.X + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Y + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Height / 2;
                    bool right = Accessibility.right;
                    lastMouseState = new MouseState(x, y, lastMouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                    return new MouseState(x, y, lastMouseState.ScrollWheelValue, right ? ButtonState.Released : ButtonState.Pressed, ButtonState.Released, right ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
            }
            return _mouseState;
        }
        public static MouseState LastMouseState
        {
            get
            {
                return lastMouseState;
            }
            set
            {
                lastMouseState = value;
            }
        }
        public MouseInput()
        {
        }
        public static int getMouseX()
        {
            return Mouse.GetState().X;
        }

        public static int getMouseY()
        {
            return Mouse.GetState().Y;
        }
    }
}
#endif
