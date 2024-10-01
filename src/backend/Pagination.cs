using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Here is where pages are kept. Each page is a class that implements IObject with a name and tooltip.
    /// </summary>
    public static class Pagination
    {
        public static int TopPageCount = 6; // Amount of valid pages
        public static IPage[] Pages = new IPage[]
        {
            // Selectable pages (from menu)
            new GeneratePage(),
            new LibraryPage(),
            new PluginsPage(),
            new OptionsPage(),
            new BlogPage(),
            new PastimePage(),
            // Debug page is not selectable from menu
            new DebugPage(),
        };
        // Page hierarchy is a legacy feature
        // TODO: This should be removed
        public static int SelectedPage = 0;
        public static int DrawnPage = 0;
        public static void SetParentPage(int page)
        {
            SelectedPage = page;
        }
        public static void SetSubPage(int page)
        {
            DrawnPage = page;
        }
        public static void SetTopPage(int parent, int sub)
        {
            SelectedPage = parent;
            DrawnPage = sub;
        }
        public static void SetPage(int bothPages)
        {
            SelectedPage = bothPages;
            DrawnPage = bothPages;
        }
        public static int GetParentPage()
        {
            return SelectedPage;
        }
        public static int GetSubPage()
        {
            return DrawnPage;
        }
        public static IPage GetPage(int page)
        {
            return Pages[page];
        }
        public static string GetSubPageName()
        {
            return Pages[DrawnPage].Name;
        }
        public static string GetSubPageTooltip()
        {
            return Pages[DrawnPage].Tooltip;
        }
        public static string GetParentPageName()
        {
            return Pages[SelectedPage].Name;
        }
        public static string GetParentPageTooltip()
        {
            return Pages[SelectedPage].Tooltip;
        }
        public static int GetTopPageCount()
        {
            return TopPageCount;
        }
        public static int GetSubPageCount()
        {
            return Pages.Length - TopPageCount;
        }
        public static int GetPageCount()
        {
            return Pages.Length;
        }
        // Draw, update, and loadcontent forwarders
        public static bool Update(GameTime gameTime, bool handleInput)
        {
            return Pages[DrawnPage].Update(gameTime, handleInput);
        }
        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Pages[DrawnPage].Draw(gameTime, spriteBatch);
        }
        public static void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach(IPage page in Pages)
                page.LoadContent(content, graphicsDevice);
        }
    }
}
