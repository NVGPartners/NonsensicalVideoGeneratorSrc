#if MONOGAME
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NonsensicalVideoGenerator
{ 
    public static class MouseInput
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
            // Respect Global.drawOffset
            int oX = GlobalGraphics.Scale((int)Global.drawOffset.X);
            int oY = GlobalGraphics.Scale((int)Global.drawOffset.Y);
            MouseState overrideMouseState = _mouseState;
            if(Accessibility.allowAccessibility)
            {
                // Hover over accessibility options if active
                if(Accessibility.showDisambiguation && Accessibility.disambiguationOptions.Count > 0 && Accessibility.disambiguationOptions.Count > -(Accessibility.offset))
                {
                    int x = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.X + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Y + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Height / 2;
                    overrideMouseState = new MouseState(x, y, _mouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
                // Build mouse state from accessibility if active
                if(Accessibility.selectedDisambiguationOption != -1 && Accessibility.disambiguationOptions.Count > Accessibility.selectedDisambiguationOption)
                {
                    int x = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.X + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Y + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Height / 2;
                    bool right = Accessibility.right;
                    lastMouseState = new MouseState(x, y, lastMouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                    overrideMouseState = new MouseState(x, y, lastMouseState.ScrollWheelValue, right ? ButtonState.Released : ButtonState.Pressed, ButtonState.Released, right ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
            }
            // Apply draw offset
            overrideMouseState = new MouseState(overrideMouseState.X - oX, overrideMouseState.Y - oY, overrideMouseState.ScrollWheelValue, overrideMouseState.LeftButton, overrideMouseState.MiddleButton, overrideMouseState.RightButton, overrideMouseState.XButton1, overrideMouseState.XButton2);
            return overrideMouseState;
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
    }
}
#endif
