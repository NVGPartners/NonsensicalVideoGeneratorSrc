using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    public enum ContextMenuType
    {
        Generic,
        
    }
    public static class ContextMenu
    {
        private static bool visible = false;
        private static ContextMenuType type = ContextMenuType.Generic;
        private static List<string> items = new List<string>();
        public static bool IsVisible()
        {
            return visible;
        }
    }
}
