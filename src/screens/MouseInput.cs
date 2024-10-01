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
            // Respect GlobalGraphics.drawOffset
            int oX = GlobalGraphics.Scale((int)GlobalGraphics.drawOffset.X);
            int oY = GlobalGraphics.Scale((int)GlobalGraphics.drawOffset.Y);
            MouseState overrideLastState = lastMouseState;
            MouseState overrideMouseState = _mouseState;
            if(Accessibility.allowAccessibility)
            {
                // Hover over accessibility options if active
                if(Accessibility.showDisambiguation && Accessibility.disambiguationOptions.Count > 0 && Accessibility.disambiguationOptions.Count > -(Accessibility.offset))
                {
                    int x = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.X + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Y + Accessibility.disambiguationOptions[-(Accessibility.offset)].bounds.Height / 2;
                    overrideMouseState = new MouseState(x + oX, y + oY, _mouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
                // Build mouse state from accessibility if active
                if(Accessibility.selectedDisambiguationOption != -1 && Accessibility.disambiguationOptions.Count > Accessibility.selectedDisambiguationOption)
                {
                    int x = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.X + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Width / 2;
                    int y = Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Y + Accessibility.disambiguationOptions[Accessibility.selectedDisambiguationOption].bounds.Height / 2;
                    bool right = Accessibility.right;
                    overrideLastState = new MouseState(x + oX, y + oY, lastMouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                    overrideMouseState = new MouseState(x + oX, y + oY, lastMouseState.ScrollWheelValue, right ? ButtonState.Released : ButtonState.Pressed, ButtonState.Released, right ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released, ButtonState.Released);
                }
            }
            // Apply draw offset
            overrideLastState = new MouseState(overrideLastState.X - oX, overrideLastState.Y - oY, overrideLastState.ScrollWheelValue, overrideLastState.LeftButton, overrideLastState.MiddleButton, overrideLastState.RightButton, overrideLastState.XButton1, overrideLastState.XButton2);
            overrideMouseState = new MouseState(overrideMouseState.X - oX, overrideMouseState.Y - oY, overrideMouseState.ScrollWheelValue, overrideMouseState.LeftButton, overrideMouseState.MiddleButton, overrideMouseState.RightButton, overrideMouseState.XButton1, overrideMouseState.XButton2);
            lastMouseState = overrideLastState;
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
