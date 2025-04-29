using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Globalization;
using MonoGame.Extended.VideoPlayback;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class LibraryPage : IPage
    {
        public string Name { get; set; } = "PageLibrary";
        public string Tooltip { get; } = "View imported media and previous renders.";
        private readonly InteractableController controller = new();
        private readonly Dictionary<string, Rectangle> rects = new();
        private readonly Dictionary<LibraryRootType, List<string>> libraryTypes = new();
        private LibraryRootType currentRootType = LibraryRootType.Video;
        private LibraryType currentLibraryType = DefaultLibraryTypes.Material;
        private readonly Dictionary<LibraryType, List<LibraryFile>> libraryFileCache = new();
        private readonly Dictionary<int, Texture2D> videoPlayers = new();
        private int selectedFlags = 1 | 8; // 1 = Video, 8 = First SubType
        private int staticAnim = 0;
        private int audioAnim = 0;
        private int deleteConfirmPos = -1;
        private double lastAnimTime;
        private double lastAnimTimeAudio;
        private int page = 0;
        private bool demandChange = false;
        private bool changed = false;
        private bool organizing = false;
        private int organizeFile = -1;
        private int organizeType = -1;
        private string tooltip = "";
        private bool registered = false;
        private static bool downloading = false;
        private static KeyboardState oldKeyboardState;
        private static KeyboardState newKeyboardState;
        private readonly InteractableController actionController = new();
        public void CacheLibrary()
        {
            libraryFileCache.Clear();
            // Get library types
            libraryTypes[LibraryRootType.Video] = LibraryData.GetLibraryNames(LibraryRootType.Video);
            libraryTypes[LibraryRootType.Audio] = LibraryData.GetLibraryNames(LibraryRootType.Audio);
            libraryTypes[LibraryRootType.Image] = LibraryData.GetLibraryNames(LibraryRootType.Image);
            // Preload library files
            LibraryData.Load();
            for(int i = 0; i < DefaultLibraryTypes.AllTypes.Count; i++)
            {
                LibraryType libraryType = DefaultLibraryTypes.AllTypes[i];
                if(!libraryType.Special)
                {
                    libraryFileCache[libraryType] = LibraryData.GetFiles(libraryType, false);
                }
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            controller.Clear();
            actionController.Clear();
            actionController.Add("ActionEnableAll", new ActionButton("Enable all content in this category.", new Vector2(112, 206), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(currentLibraryType == DefaultLibraryTypes.Render)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        for(int j = 0; j < libraryFileCache[currentLibraryType].Count; j++)
                        {
                            libraryFileCache[currentLibraryType][j].Enabled = true;
                        }
                        LibraryData.SetAllEnabled(currentLibraryType, true);
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/enableall")));
            actionController.Add("ActionDisableAll", new ActionButton("Disable all content in this category.", new Vector2(112, 221), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        if(currentLibraryType == DefaultLibraryTypes.Render)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        for(int j = 0; j < libraryFileCache[currentLibraryType].Count; j++)
                        {
                            libraryFileCache[currentLibraryType][j].Enabled = false;
                        }
                        LibraryData.SetAllEnabled(currentLibraryType, false);
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/disableall")));
            // Library assets
            GlobalContent.AddTexture("DeleteConfirm", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/deleteconfirm"));
            GlobalContent.AddTexture("AddVideoOverlay", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/addvideooverlay"));
            GlobalContent.AddTexture("RenderAddVideoOverlay", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/renderaddvideooverlay"));
            GlobalContent.AddTexture("HeaderButton", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/headerbutton"));
            GlobalContent.AddTexture("HeaderButtonSelected", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/headerbuttonselected"));
            GlobalContent.AddTexture("Separator", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/separator"));
            GlobalContent.AddTexture("SubTypeButton", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/subtypebutton"));
            GlobalContent.AddTexture("SubTypeButtonSelected", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/subtypebuttonselected"));
            GlobalContent.AddTexture("TypeButton", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/typebutton"));
            GlobalContent.AddTexture("TypeButtonSelected", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/typebuttonselected"));
            GlobalContent.AddTexture("VideoHolder", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/videoholder"));
            GlobalContent.AddTexture("VideoOff", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/videooff"));
            GlobalContent.AddTexture("VideoOn", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/videoon"));
            GlobalContent.AddTexture("RenderNoButton", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/rendernobutton"));
            GlobalContent.AddTexture("SubTypeButtonOrganize", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/subtypebuttonorganize"));
            GlobalContent.AddTexture("StaticOverlay", ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/staticoverlay"));
            // TV Static Animation: 13 frames as graphics/library/staticanim/staticanim0 to graphics/library/staticanim/staticanim12
            for(int i = 0; i < 13; i++)
            {
                GlobalContent.AddTexture("StaticAnim" + i, ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/staticanim/staticanim" + i));
            }
            // Vinyl Record Animation: 2 frames as graphics/library/audioanim/audioanim0 to graphics/library/audioanim/audioanim1
            for(int i = 0; i < 2; i++)
            {
                GlobalContent.AddTexture("AudioAnim" + i, ThemeManager.LoadLayeredContent<Texture2D>("graphics/library/audioanim/audioanim" + i));
            }
            // Get textures
            Texture2D addVideoOverlay = GlobalContent.GetTexture("AddVideoOverlay");
            Texture2D headerButton = GlobalContent.GetTexture("HeaderButton");
            Texture2D headerButtonSelected = GlobalContent.GetTexture("HeaderButtonSelected");
            Texture2D separator = GlobalContent.GetTexture("Separator");
            Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
            Texture2D subTypeButtonSelected = GlobalContent.GetTexture("SubTypeButtonSelected");
            Texture2D typeButton = GlobalContent.GetTexture("TypeButton");
            Texture2D typeButtonSelected = GlobalContent.GetTexture("TypeButtonSelected");
            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
            Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
            Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
            // Set up rectangles
            rects.Clear();
            rects.Add("VideoButton", new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("AudioButton", new Rectangle(GlobalGraphics.Scale(166), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("HeaderButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(56), GlobalGraphics.Scale(headerButton.Width), GlobalGraphics.Scale(headerButton.Height)));
            rects.Add("PageLeftButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("PageRightButton", new Rectangle(GlobalGraphics.Scale(276), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
            demandChange = true;
        }
        public static void Done(bool success)
        {
            downloading = false;
            if(success)
            {
                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generator.progressText = L.T(0, "Library:StatusDownloadedClip");
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generator.progressText = L.T(0, "Library:StatusFailDownloadClip");
            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // changes
            if(changed)
            {
                // Load video players
                ChangeVideos(spriteBatch.GraphicsDevice);
                changed = false;
            }
            if(Global.justCompletedRender)
                return; // changing
            // Store textures in local variable for performance
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            Texture2D deleteConfirm = GlobalContent.GetTexture("DeleteConfirm");
            Texture2D addVideoOverlay = GlobalContent.GetTexture("AddVideoOverlay");
            Texture2D renderAddVideoOverlay = GlobalContent.GetTexture("RenderAddVideoOverlay");
            Texture2D headerButton = GlobalContent.GetTexture("HeaderButton");
            Texture2D headerButtonSelected = GlobalContent.GetTexture("HeaderButtonSelected");
            Texture2D separator = GlobalContent.GetTexture("Separator");
            Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
            Texture2D subTypeButtonSelected = GlobalContent.GetTexture("SubTypeButtonSelected");
            Texture2D typeButton = GlobalContent.GetTexture("TypeButton");
            Texture2D typeButtonSelected = GlobalContent.GetTexture("TypeButtonSelected");
            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
            Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
            Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
            Texture2D renderNoButton = GlobalContent.GetTexture("RenderNoButton");
            SpriteFont  munroSmall = L.FontSmall();
            // Draw background
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(170), GlobalGraphics.Scale(15)), ThemeManager.GetColor("BackgroundLibraryPage"));
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(219), GlobalGraphics.Scale(170), GlobalGraphics.Scale(17)), ThemeManager.GetColor("BackgroundLibraryPage"));
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71), GlobalGraphics.Scale(65), GlobalGraphics.Scale(148)), ThemeManager.GetColor("BackgroundLibraryPage"));
            // Draw separators and video holders
            int a, b;
            for (a = 0; a < 3; a++)
            {
                for (b = 0; b < 4; b++)
                {
                    spriteBatch.Draw(separator, new Rectangle(GlobalGraphics.Scale(200 + 35 * a), GlobalGraphics.Scale(71 + 37 * b), GlobalGraphics.Scale(separator.Width), GlobalGraphics.Scale(separator.Height)), Color.White);
                    if(libraryFileCache.Keys.Contains(currentLibraryType))
                    {
                        Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * a) + (a * 2)), GlobalGraphics.Scale(72 + (35 * b) + (b * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                        Rectangle staticRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(2), videoHolderRect.Y + GlobalGraphics.Scale(2), GlobalGraphics.Scale(29), GlobalGraphics.Scale(22));
                        Rectangle addVideoOverlayRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(4), videoHolderRect.Y + GlobalGraphics.Scale(26), GlobalGraphics.Scale(addVideoOverlay.Width), GlobalGraphics.Scale(addVideoOverlay.Height));
                        Rectangle renderAddVideoOverlayRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(3), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(renderAddVideoOverlay.Width), GlobalGraphics.Scale(renderAddVideoOverlay.Height));
                        Rectangle deleteConfirmRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(3), videoHolderRect.Y + GlobalGraphics.Scale(26), GlobalGraphics.Scale(deleteConfirm.Width), GlobalGraphics.Scale(deleteConfirm.Height));
                        Rectangle toggleButtonRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(11), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(videoOn.Width), GlobalGraphics.Scale(videoOn.Height));
                        Rectangle noButtonRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(10), videoHolderRect.Y + GlobalGraphics.Scale(26), GlobalGraphics.Scale(renderNoButton.Width), GlobalGraphics.Scale(renderNoButton.Height));
                        // Get library item at this position and page
                        int position = a + (b * 3) + (12 * page);
                        bool video = false;
                        if(libraryFileCache[currentLibraryType].Count > position)
                        {
                            video = true;
                            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), staticRect, Color.Black);
                            if(currentRootType == LibraryRootType.Video)
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, ThemeManager.GetColor("VideoHolderStaticAnimFilledLibraryPage"));
                            }
                            else if(currentRootType == LibraryRootType.Audio)
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("AudioAnim" + audioAnim), staticRect, Color.White);
                            }
                            int pagelessPosition = position - (12 * page);
                            if(videoPlayers.ContainsKey(pagelessPosition))
                            {
                                spriteBatch.Draw(videoPlayers[pagelessPosition], staticRect, Color.White);
                            }
                        }
                        else
                        {
                            spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, Color.White);
                            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("StaticOverlay"), staticRect, ThemeManager.GetColor("VideoHolderAddOverlayLibraryPage"));
                            }
                        }
                        // Draw video holder
                        spriteBatch.Draw(videoHolder, videoHolderRect, Color.White);
                        if(!video)
                        {
                            spriteBatch.Draw(addVideoOverlay, addVideoOverlayRect, Color.White);
                            if(currentLibraryType == DefaultLibraryTypes.Render || currentLibraryType == DefaultLibraryTypes.NoImages)
                            {
                                spriteBatch.Draw(renderAddVideoOverlay, renderAddVideoOverlayRect, Color.White);
                            }
                        }
                        else if(deleteConfirmPos == position)
                        {
                            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AskToDelete"), new Vector2(deleteConfirmRect.X - GlobalGraphics.Scale(1-1), deleteConfirmRect.Y - GlobalGraphics.Scale(14-1)), Color.Black);
                            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AskToDelete"), new Vector2(deleteConfirmRect.X - GlobalGraphics.Scale(1), deleteConfirmRect.Y - GlobalGraphics.Scale(14)), Color.White);
                            spriteBatch.Draw(deleteConfirm, deleteConfirmRect, Color.White);
                        }
                        else if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                        {
                            // Draw toggle button for state of video
                            LibraryFile file = libraryFileCache[currentLibraryType][position];
                            if(file.Enabled)
                                spriteBatch.Draw(videoOn, toggleButtonRect, Color.White);
                            else
                                spriteBatch.Draw(videoOff, toggleButtonRect, Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(renderNoButton, noButtonRect, Color.White);
                        }
                    }
                }
            }
            // Draw buttons
            spriteBatch.Draw(typeButton, rects["VideoButton"], Color.White);
            spriteBatch.Draw(typeButton, rects["AudioButton"], Color.White);
#if WINDOWSDX
            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                spriteBatch.Draw(headerButton, rects["HeaderButton"], Color.White);
#endif
            // Draw subtypes
            try
            {
                for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                {
                    spriteBatch.Draw(subTypeButton, rects[currentRootType.ToString() + libraryTypes[currentRootType][i] + "Button"], Color.White);
                }
            }
            catch
            {
                // Still loading?
            }
            // Draw selected buttons
            if((selectedFlags & 1) == 1)
                spriteBatch.Draw(typeButtonSelected, rects["VideoButton"], Color.White);
            if((selectedFlags & 2) == 2)
                spriteBatch.Draw(typeButtonSelected, rects["AudioButton"], Color.White);
            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
            {
                if((selectedFlags & 4) == 4)
                    spriteBatch.Draw(headerButtonSelected, rects["HeaderButton"], Color.White);
            }
            // Flags 8-32768 (0x8-0x8000, 13 bits) are for subtypes
            try
            {
                for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                {
                    if((selectedFlags & (8 << i)) == (8 << i))
                    {
                        spriteBatch.Draw(subTypeButtonSelected, rects[currentRootType.ToString() + libraryTypes[currentRootType][i] + "Button"], Color.White);
                    }
                }
            }
            catch
            {
                // Still loading?
            }
            // Draw page buttons
            spriteBatch.Draw(typeButton, rects["PageLeftButton"], Color.White);
            spriteBatch.Draw(typeButton, rects["PageRightButton"], Color.White);
            // Draw text
            if(Global.imageLibraryAvailable)
            {
                switch(currentRootType)
                {
                    case LibraryRootType.Video:
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
                        break;
                    case LibraryRootType.Audio:
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:ImageTitle"), new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:ImageTitle"), new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
                        break;
                    case LibraryRootType.Image:
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:ImageTitle"), new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:ImageTitle"), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                        GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
                        break;
                }
            }
            else
            {
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:VideoTitle"), new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:AudioTitle"), new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
            }
            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:BackButton"), new Vector2(GlobalGraphics.Scale(205 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:BackButton"), new Vector2(GlobalGraphics.Scale(205), GlobalGraphics.Scale(223)), Color.White);
            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:NextButton"), new Vector2(GlobalGraphics.Scale(281 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            GlobalContent.DrawString(spriteBatch, munroSmall, L.T(0, "Library:NextButton"), new Vector2(GlobalGraphics.Scale(281), GlobalGraphics.Scale(223)), Color.White);
            // Downloader
#if WINDOWSDX
            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
            {
                string totalIndicator = L.T(0, "Library:DownloadMedia");
                if((selectedFlags & 4) == 4)
                    totalIndicator =    L.T(0, "Library:DownloadMediaPasteURLS");
                if(downloading)
                    totalIndicator =    L.T(0, "Library:Downloading");
                Vector2 totalIndicatorSize = munroSmall.MeasureString(totalIndicator);
                Vector2 totalPosition = new Vector2(rects["HeaderButton"].X + rects["HeaderButton"].Width / 2 - totalIndicatorSize.X / 2 + GlobalGraphics.Scale(1), GlobalGraphics.Scale(56));
                GlobalContent.DrawString(spriteBatch, munroSmall, totalIndicator, totalPosition + new Vector2(GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, totalIndicator, totalPosition, Color.White);
            }
#endif
            // Page indicator is centered
            if(libraryFileCache.Keys.Contains(currentLibraryType))
            {
                // Page indicator
                int maxPages = (int)Math.Ceiling((double)libraryFileCache[currentLibraryType].Count / 12);
                // If the last page is full of videos, add an extra page
                if(libraryFileCache[currentLibraryType].Count % 12 == 0)
                    maxPages++;
                string pageIndicator = page + 1 + "/" + maxPages;
                Vector2 pageIndicatorSize = munroSmall.MeasureString(pageIndicator);
                int pivot = 252; 
                GlobalContent.DrawString(spriteBatch, munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot + 1) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223 + 1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223)), Color.White);
            }
            int offset = 0;
            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
            {
                string title = libraryTypes[currentRootType][i];
                if(i < DefaultLibraryTypes.defaultCounts[currentRootType] || title == "No Images")
                    title = L.T(0, "Library:" + title.Replace(" ", "")+"Title");
                GlobalContent.DrawString(spriteBatch, munroSmall, title, new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(71 + offset + 13 * i + 1)), Color.Black);
                GlobalContent.DrawString(spriteBatch, munroSmall, title, new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(71 + offset + 13 * i)), Color.White);
            }
            // Interactable
            controller.Draw(gameTime, spriteBatch);
            actionController.Draw(gameTime, spriteBatch);
            if(tooltip != "")
            {
                Global.tooltip = tooltip;
            }
        }
        // Thread for loading videos
        private BackgroundWorker? loadVideosThread;
        private void LoadVideosThread(object? sender, DoWorkEventArgs e)
        {
            if(e.Argument == null)
            {
                // huh?
                try
                {
                    if(loadVideosThread != null)
                        loadVideosThread.Dispose();
                }
                catch {}
                loadVideosThread = null;
                return;
            }
            // Args: GraphicsDevice graphicsDevice, int currentPage
            object[] args = (object[])e.Argument;
            GraphicsDevice graphicsDevice = (GraphicsDevice)args[0];
            int currentPage = (int)args[1];
            // Load videos
            int a, b;
            for (a = 0; a < 3; a++)
            {
                for (b = 0; b < 4; b++)
                {
                    try
                    {
                        // Check to make sure we're still on the same page
                        if (currentPage != page)
                            return;
                        // Cancelled?
                        if (loadVideosThread != null && loadVideosThread.CancellationPending)
                            return;
                        int position = a + (b * 3) + (12 * page);
                        if(libraryFileCache[currentLibraryType].Count > position)
                        {
                            LibraryFile libraryFile = libraryFileCache[currentLibraryType][position];
                            if(libraryFile.Path == null)
                                continue;
                            int texScale = int.Parse(SaveData.saveValues["VideoPlaybackScale"], CultureInfo.InvariantCulture);
                            int texWidth = 29 * texScale;
                            int texHeight = 23 * texScale;
                            Texture2D texture = new Texture2D(graphicsDevice, texWidth, texHeight);
                            // Generate video thumbnail using ffmpeg
                            string tempBmp = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", "thumb.bmp");
                            Process bmpProcess = Generator.GenerateThumbnail(libraryFile.Path, tempBmp, currentRootType);
                            // Defer until process is done
                            while (!bmpProcess.HasExited)
                            {
                                System.Threading.Thread.Sleep(100);
                                // Check to make sure we're still on the same page
                                if (currentPage != page)
                                    return;
                            }
                            // Check to make sure we're still on the same page
                            if (currentPage != page)
                                return;
                            // Cancelled?
                            if (loadVideosThread != null && loadVideosThread.CancellationPending)
                                return;
                            // Load bitmap
                            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(tempBmp);
                            // Convert bitmap to color data
                            Color[] colorData = new Color[texWidth * texHeight];
                            for (int i = 0; i < texWidth; i++)
                            {
                                for (int j = 0; j < texHeight; j++)
                                {
                                    System.Drawing.Color color = bitmap.GetPixel(i, j);
                                    // Black is translucent
                                    if (color.R == 0 && color.G == 0 && color.B == 0)
                                        color = System.Drawing.Color.FromArgb(192, 0, 0, 0);
                                    colorData[i + (j * texWidth)] = new Color(color.R, color.G, color.B, color.A);
                                }
                            }
                            texture.SetData(colorData);
                            // Check to make sure we're still on the same page
                            if (currentPage != page)
                                return;
                            // Cancelled?
                            if (loadVideosThread != null && loadVideosThread.CancellationPending)
                                return;
                            // Delete temp file
                            bitmap.Dispose();
                            File.Delete(tempBmp);
                            // Check to make sure we're still on the same page
                            if (currentPage != page)
                                return;
                            // Cancelled?
                            if (loadVideosThread != null && loadVideosThread.CancellationPending)
                                return;
                            // Update video players
                            int pagelessPosition = position - (12 * page);
                            videoPlayers.Add(pagelessPosition, texture);
                        }
                    }
                    catch
                    {
                        // Already added?
                    }
                }
            }
            // Done!
            try
            {
                if(loadVideosThread != null)
                    loadVideosThread.Dispose();
            }
            catch {}
            loadVideosThread = null;
        }
        public void ChangeVideos(GraphicsDevice graphicsDevice)
        {
            try
            {
                // Cancel previous thread
                if (loadVideosThread != null)
                {
                    loadVideosThread.CancelAsync();
                    loadVideosThread.Dispose();
                }
                // Clear video players
                foreach (Texture2D texture in videoPlayers.Values)
                {
                    texture.Dispose();
                }
                videoPlayers.Clear();
                // Start new thread
                loadVideosThread = new BackgroundWorker();
                loadVideosThread.DoWork += LoadVideosThread;
                loadVideosThread.WorkerSupportsCancellation = true;
                loadVideosThread.RunWorkerAsync(new object[] { graphicsDevice, page });
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Error changing videos: " + e.Message, Color.Red);
            }
        }
        private void TextInput(object? sender, TextInputEventArgs e)
        {
            if((selectedFlags & 4) == 4)
            {
                // If syn unicode character, paste from clipboard
                if (e.Character == '\u0016')
                {
                    selectedFlags &= ~4;
                    if(libraryFileCache.Keys.Contains(currentLibraryType))
                    {
                        Global.generator.progressText = L.T(0, "Library:Downloading");
                        downloading = true;
                        string clipboard = Clipboard.GetText();
                        // Remove invalid characters
                        clipboard = clipboard.Replace("\r", "");
                        clipboard = clipboard.Replace("\t", "");
                        clipboard = clipboard.Replace("\0", "");
                        if (!LibraryData.DownloadClip(clipboard.Split('\n'), currentLibraryType))
                        {
                            downloading = false;
                            Global.generator.progressText = L.T(0, "Library:StatusFailDownloadClip");
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                }
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(actionController.Update(gameTime, handleInput))
                return true;
            if(!registered)
            {
                GameWindow? window = UserInterface.instance?.Window;
                if (window != null)
                {
                    window.TextInput += TextInput;
                }
                registered = true;
            }
            if(Global.justCompletedRender)
            {
                // Reimport all and demand change
                demandChange = true;
            }
            if(demandChange)
            {
                deleteConfirmPos = -1;
                // Delete existing rects
                foreach(LibraryRootType type in libraryTypes.Keys)
                {
                    for(int i = 0; i < libraryTypes[type].Count; i++)
                    {
                        rects.Remove(type.ToString() + libraryTypes[type][i] + "Button");
                    }
                }
                // Ask library to rescan
                CacheLibrary();
                Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButton");
                // For one subtype, 60x13 at 135, 71 + 13 * i
                foreach(LibraryRootType type in libraryTypes.Keys)
                {
                    int offset = 0;
                    for(int i = 0; i < libraryTypes[type].Count; i++)
                    {
                        // if rect name is already taken, remove it
                        if(rects.ContainsKey(type.ToString() + libraryTypes[type][i] + "Button"))
                            rects.Remove(type.ToString() + libraryTypes[type][i] + "Button");
                        rects.Add(type.ToString() + libraryTypes[type][i] + "Button", new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71 + offset + 13 * i), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height)));
                    }
                }
                Global.justCompletedRender = false;
                changed = true;
                demandChange = false;
            }
            // Organizing
            if(!organizing && organizeFile > -1 && organizeType > -1)
            {
                // Get library file
                LibraryFile libraryFile = libraryFileCache[currentLibraryType][organizeFile];
                LibraryType? oldLibraryType = libraryFile.Type;
                // Get library type
                string libraryName = libraryTypes[currentRootType][organizeType];
                LibraryType libraryType = DefaultLibraryTypes.Video;
                foreach(KeyValuePair<LibraryType, string> kvPair in LibraryData.libraryNames)
                {
                    if(kvPair.Value == libraryName)
                    {
                        libraryType = kvPair.Key;
                        break;
                    }
                }
                if(!libraryType.Special)
                {
                    LibraryFile newFile = LibraryData.Organize(libraryFile, libraryType);
                    if(oldLibraryType != null)
                        libraryFileCache[oldLibraryType].Remove(libraryFile);
                    libraryFileCache[libraryType].Add(newFile);
                }
                organizeFile = -1;
                organizeType = -1;
                demandChange = true;
            }
            // staticAnim is a 15fps animation, so update every 66.666ms
            if (gameTime.TotalGameTime.TotalMilliseconds - lastAnimTime > 66.666)
            {
                // Update animation
                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                    staticAnim++;
                if(staticAnim > 12)
                    staticAnim = 0;
                // Update lastAnimTime
                lastAnimTime = gameTime.TotalGameTime.TotalMilliseconds;
            }
            // audioanim is a 4fps animation
            if(gameTime.TotalGameTime.TotalMilliseconds - lastAnimTimeAudio > 250)
            {
                // Update animation
                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                    audioAnim++;
                if(audioAnim > 1)
                    audioAnim = 0;
                // Update lastAnimTimeAudio
                lastAnimTimeAudio = gameTime.TotalGameTime.TotalMilliseconds;
            }
            // Drag and drop will allow library files to be added from explorer
            if(Global.dragDropFiles.Count > 0)
            {
                bool success = true;
                if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                {
                    foreach (string file in Global.dragDropFiles)
                    {
                        // Correct file extension?
                        // Otherwise, continue
                        List<string> extensions = new();
                        foreach (string ext in LibraryData.libraryFileTypes[currentLibraryType])
                        {
                            extensions.Add(ext);
                        }
                        if (!extensions.Contains(Path.GetExtension(file)))
                        {
                            success = false;
                            continue;
                        }
                        LibraryFile libraryFile = new(Path.GetFileNameWithoutExtension(file), file, currentLibraryType);
                        LibraryFile? newFile = LibraryData.Load(libraryFile);
                        if(newFile == null)
                        {
                            success = false;
                            continue;
                        }
                        libraryFileCache[currentLibraryType].Add(newFile);
                    }
                }
                else
                {
                    success = false;
                }
                demandChange = true;
                if(!success)
                {
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                }
                else
                {
                    GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                }
                Global.dragDropFiles.Clear();
            }
            // Standard input
            if(handleInput && !organizing)
            {
                oldKeyboardState = newKeyboardState;
                newKeyboardState = Keyboard.GetState();
                // Accessibility
                for(int i = 0; i < rects.Count; i++)
                {   
#if WINDOWSDX
                    if((currentLibraryType == DefaultLibraryTypes.Render || currentLibraryType == DefaultLibraryTypes.NoImages) && rects.Keys.ElementAt(i) == "HeaderButton")
#elif DESKTOPGL
                    if(rects.Keys.ElementAt(i) == "HeaderButton")   
#endif
                        continue;
                    if(rects.Keys.ElementAt(i).Contains("Audio") && rects.Keys.ElementAt(i) != "AudioButton" && currentRootType != LibraryRootType.Audio)
                        continue;
                    if(rects.Keys.ElementAt(i).Contains("Video") && rects.Keys.ElementAt(i) != "VideoButton" && currentRootType != LibraryRootType.Video && currentRootType != LibraryRootType.Image)
                        continue;
                    string tts = rects.Keys.ElementAt(i);
                    switch(tts)
                    {
                        case "HeaderButton":
                            string totalIndicator = L.T(0, "Library:DownloadMedia");
                            if((selectedFlags & 4) == 4)
                                totalIndicator = L.T(0, "Library:DownloadMediaPasteURLS");
                            if(downloading)
                                totalIndicator = L.T(0, "Library:Downloading");
                            tts = totalIndicator;
                            break;
                    }
                    /*
                    bool selected = false;
                    // selectedFlags 1, 2, 4 are each button
                    // bitwise
                    if((selectedFlags & 1) == 1 && rects.Keys.ElementAt(i) == "VideoButton")
                        selected = true;
                    if((selectedFlags & 2) == 2 && rects.Keys.ElementAt(i) == "AudioButton")
                        selected = true;
                    if((selectedFlags & 4) == 4 && rects.Keys.ElementAt(i) == "HeaderButton")
                        selected = true;
                    */
                    Accessibility.CompatAccessibility(rects.Values.ElementAt(i), tts);
                }
                // Video holders
                if(libraryFileCache.Keys.Contains(currentLibraryType))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int position = i + (j * 3) + (12 * page);
                            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
                            Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
                            Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
                            Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * i) + (i * 2)), GlobalGraphics.Scale(72 + (35 * j) + (j * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                            Rectangle staticRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(2), videoHolderRect.Y + GlobalGraphics.Scale(2), GlobalGraphics.Scale(29), GlobalGraphics.Scale(22));
                            // button 1: organize video 3, 27 5x5
                            Rectangle button1Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(3), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(5), GlobalGraphics.Scale(5));
                            // button 2: remove video 25, 27 5x5
                            Rectangle button2Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(25), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(5), GlobalGraphics.Scale(5));
                            Rectangle toggleButtonRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(11), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(videoOn.Width), GlobalGraphics.Scale(videoOn.Height));
                            if(libraryFileCache[currentLibraryType].Count > position)
                            {
                                Accessibility.CompatAccessibility(button1Rect, deleteConfirmPos == position ? L.T(0, "Accessibility:LibraryDeleteConfirm", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???") : L.T(0, "Accessibility:LibraryOrganize", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???"));
                                Accessibility.CompatAccessibility(button2Rect, deleteConfirmPos == position ? L.T(0, "Accessibility:LibraryDeleteCancel", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???") : L.T(0, "Accessibility:LibraryDelete", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???"));
                                if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                    Accessibility.CompatAccessibility(toggleButtonRect, libraryFileCache[currentLibraryType][position].Enabled ? L.T(0, "Accessibility:LibraryEnable", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???") : L.T(0, "Accessibility:LibraryDisable", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???"));
                                Accessibility.CompatAccessibility(staticRect, L.T(0, "Accessibility:LibraryOpen", Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) ?? "???"));
                            }
                            else
                            {
                                if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                    Accessibility.CompatAccessibility(staticRect, L.T(0, "Accessibility:LibraryAddMedia"));
                            }
                        }
                    }
                }
                tooltip = "";
                // Left click
                if ((MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed) || (MouseInput.LastMouseState.RightButton == ButtonState.Released && MouseInput.MouseState.RightButton == ButtonState.Pressed))
                {
                    bool left = MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed;
                    bool right = MouseInput.MouseState.RightButton == ButtonState.Pressed && MouseInput.LastMouseState.RightButton == ButtonState.Released;
                    if(left || right)
                    {
                        // Loop rects
                        foreach (KeyValuePair<string, Rectangle> rect in rects)
                        {
                            // Check if mouse is in rect
                            if (rect.Value.Contains(MouseInput.MouseState.Position))
                            {
                                // Check rect name
                                switch (rect.Key)
                                {
                                    case "VideoButton":
                                        if(Global.imageLibraryAvailableInternal)
                                        {
                                            if(right)
                                                Global.imageLibraryAvailable = !Global.imageLibraryAvailable;
                                        }
                                        if((Global.imageLibraryAvailableInternal && right) || (!Global.imageLibraryAvailable && left))
                                        {
                                            demandChange = true;
                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        if(!Global.imageLibraryAvailable || Global.imageLibraryAvailableInternal && right)
                                        {
                                            if(!left && !(Global.imageLibraryAvailableInternal && right))
                                                break;
                                            selectedFlags |= 1;
                                            selectedFlags &= ~2;
                                            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                            {
                                                selectedFlags &= ~(8 << i);
                                            }
                                            selectedFlags |= 8;
                                            selectedFlags &= ~4;
                                            if(!Global.imageLibraryAvailable)
                                            {
                                                currentRootType = LibraryRootType.Video;
                                                currentLibraryType = DefaultLibraryTypes.Material;
                                            }
                                            else
                                            {
                                                currentRootType = LibraryRootType.Image;
                                                for(int i = 0; i < DefaultLibraryTypes.AllTypes.Count; i++)
                                                {
                                                    if(DefaultLibraryTypes.AllTypes[i].RootType == currentRootType && !DefaultLibraryTypes.AllTypes[i].Special)
                                                    {
                                                        currentLibraryType = DefaultLibraryTypes.AllTypes[i];
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        return true;
                                    case "AudioButton":
                                        if(!left)
                                            break;
                                        if(Global.imageLibraryAvailable)
                                        {
                                            selectedFlags |= 1;
                                            selectedFlags &= ~2;
                                            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                            {
                                                selectedFlags &= ~(8 << i);
                                            }
                                            selectedFlags |= 8;
                                            selectedFlags &= ~4;
                                            if(currentRootType == LibraryRootType.Image)
                                            {
                                                currentRootType = LibraryRootType.Video;
                                                currentLibraryType = DefaultLibraryTypes.Material;
                                            }
                                            else if(currentRootType == LibraryRootType.Video)
                                            {
                                                currentRootType = LibraryRootType.Audio;
                                                currentLibraryType = DefaultLibraryTypes.SFX;
                                            }
                                            else if(currentRootType == LibraryRootType.Audio)
                                            {
                                                currentRootType = LibraryRootType.Image;
                                                for(int i = 0; i < DefaultLibraryTypes.AllTypes.Count; i++)
                                                {
                                                    if(DefaultLibraryTypes.AllTypes[i].RootType == currentRootType && !DefaultLibraryTypes.AllTypes[i].Special)
                                                    {
                                                        currentLibraryType = DefaultLibraryTypes.AllTypes[i];
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            selectedFlags |= 2;
                                            selectedFlags &= ~1;
                                            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                            {
                                                selectedFlags &= ~(8 << i);
                                            }
                                            selectedFlags |= 8;
                                            selectedFlags &= ~4;
                                            currentRootType = LibraryRootType.Audio;
                                            currentLibraryType = DefaultLibraryTypes.SFX;
                                        }
                                        demandChange = true;
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
#if WINDOWSDX
                                    case "HeaderButton":
                                        if(!left)
                                            break;
                                        if(libraryFileCache.Keys.Contains(currentLibraryType))
                                        {
                                            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                            {
                                                if(!downloading)
                                                {
                                                    selectedFlags ^= 4;
                                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                                else
                                                {
                                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
#endif
                                    case "PageLeftButton":
                                        if(!left)
                                            break;
                                        if(libraryFileCache.Keys.Contains(currentLibraryType))
                                        {
                                            if (page > 0)
                                            {
                                                page--;
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
                                    case "PageRightButton":
                                        if(!left)
                                            break;
                                        if(libraryFileCache.Keys.Contains(currentLibraryType))
                                        {
                                            int maxPages = (int)Math.Ceiling((double)libraryFileCache[currentLibraryType].Count / 12);
                                            // If the last page is full of videos, add an extra page
                                            if(libraryFileCache[currentLibraryType].Count % 12 == 0)
                                                maxPages++;
                                            if (page < maxPages - 1)
                                            {
                                                page++;
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
                                    default:
                                        if(!left)
                                            break;
                                        // Check if it's a subtype button
                                        if (rect.Key.StartsWith(currentRootType.ToString()))
                                        {
                                            // Get index
                                            int index = libraryTypes[currentRootType].IndexOf(rect.Key.Substring(currentRootType.ToString().Length, rect.Key.Length - currentRootType.ToString().Length - 6));
                                            if(index == -1)
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                // Deslect all other subtypes
                                                for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                                {
                                                    selectedFlags &= ~(8 << i);
                                                }
                                                // Select this subtype
                                                selectedFlags |= 8 << index;
                                                selectedFlags &= ~4;
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                // Reset page
                                                page = 0;
                                                // Get subtype
                                                foreach (KeyValuePair<LibraryType, string> type in LibraryData.libraryNames)
                                                {
                                                    if (type.Value == libraryTypes[currentRootType][index])
                                                    {
                                                        currentLibraryType = type.Key;
                                                        break;
                                                    }
                                                }
                                            }
                                            return true;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    // Check if it's a video holder
                    if(libraryFileCache.Keys.Contains(currentLibraryType))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                int position = i + (j * 3) + (12 * page);
                                Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
                                Texture2D videoOn = GlobalContent.GetTexture("VideoOn");
                                Texture2D videoOff = GlobalContent.GetTexture("VideoOff");
                                Texture2D renderNoButton = GlobalContent.GetTexture("RenderNoButton");
                                Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * i) + (i * 2)), GlobalGraphics.Scale(72 + (35 * j) + (j * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                                Rectangle staticRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(2), videoHolderRect.Y + GlobalGraphics.Scale(2), GlobalGraphics.Scale(29), GlobalGraphics.Scale(22));
                                // button 1: organize video 3, 27 5x5
                                Rectangle button1Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(3), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(5), GlobalGraphics.Scale(5));
                                // button 2: remove video 25, 27 5x5
                                Rectangle button2Rect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(25), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(5), GlobalGraphics.Scale(5));
                                Rectangle toggleButtonRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(11), videoHolderRect.Y + GlobalGraphics.Scale(27), GlobalGraphics.Scale(videoOn.Width), GlobalGraphics.Scale(videoOn.Height));
                                Rectangle noButtonRect = new Rectangle(videoHolderRect.X + GlobalGraphics.Scale(10), videoHolderRect.Y + GlobalGraphics.Scale(26), GlobalGraphics.Scale(renderNoButton.Width), GlobalGraphics.Scale(renderNoButton.Height));
                                bool add = false;
                                if (staticRect.Contains(MouseInput.MouseState.Position))
                                {
                                    if (libraryFileCache[currentLibraryType].Count > position)
                                    {
                                        // Open video with shell using default program
                                        LibraryFile file = libraryFileCache[currentLibraryType][position];
                                        if(file.Path != null)
                                        {
                                            if(left)
                                            {
                                                /*
                                                ProcessStartInfo startInfo = new()
                                                {
                                                    FileName = file.Path,
                                                    UseShellExecute = true
                                                };
                                                try
                                                {
                                                    Process.Start(startInfo);
                                                }
                                                catch
                                                {
                                                    LibraryData.Unload(file);
                                                    Global.justCompletedRender = true;
                                                }
                                                */
                                                FramePlayer.Stop();
                                                if(UserInterface.instance.videoPlayer != null)
                                                {
                                                    UserInterface.instance.videoPlayer.Dispose();
                                                    UserInterface.instance.videoPlayer = null;
                                                }
                                                UserInterface.instance.videoPlayer = new MonoGame.Extended.Framework.Media.VideoPlayer(UserInterface.instance.GraphicsDevice);
                                                FramePlayer.canPlayBgMusic = true;
                                                if(UserInterface.instance.video != null)
                                                {
                                                    UserInterface.instance.video.Dispose();
                                                    UserInterface.instance.video = null;
                                                }
                                                UserInterface.instance.videoPath = "";
                                                if(file.Type.RootType == LibraryRootType.Video)
                                                {
                                                    string cachePath = VideoCache.GetCachePath(file.Path);
                                                    UserInterface.instance.videoPath = cachePath;
                                                    UserInterface.instance.video = VideoHelper.LoadFromFile(cachePath);
                                                    UserInterface.instance.videoPlayer.IsLooped = true;
                                                    UserInterface.instance.videoPlayer.Play(UserInterface.instance.video);
                                                    UserInterface.instance.videoPlayer.Volume = float.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
                                                    FramePlayer.canPlayBgMusic = false;
                                                    Global.generator.progressText = L.T(0, "Video:StatusPlay");
                                                }
                                                else
                                                {
                                                    FramePlayer.PlayMedia(file);
                                                }
                                                if(ScreenManager.GetScreen<VideoScreen>("Video") == null
                                                    || ScreenManager.GetScreen<VideoScreen>("Video")?.screenType == ScreenType.Hidden)
                                                {
                                                    ScreenManager.PushNavigation("Video");
                                                    ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                                                }
                                            }
                                            if(right)
                                            {
                                                // Open directory and select file
                                                ProcessStartInfo startInfo = new()
                                                {
                                                    FileName = "explorer.exe",
                                                    Arguments = "/select, \"" + Path.GetFullPath(file.Path) + "\""
                                                };
                                                Process.Start(startInfo);
                                            }
                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        add = true;
                                    }
                                }
                                else if (button1Rect.Contains(MouseInput.MouseState.Position) && left)
                                {
                                    if(deleteConfirmPos == position)
                                    {
                                        // Stop all playback
                                        FramePlayer.Stop();
                                        if(UserInterface.instance.videoPlayer != null)
                                        {
                                            UserInterface.instance.videoPlayer.Dispose();
                                            UserInterface.instance.videoPlayer = null;
                                        }
                                        if(UserInterface.instance.video != null)
                                        {
                                            UserInterface.instance.video.Dispose();
                                            UserInterface.instance.video = null;
                                        }
                                        UserInterface.instance.videoPath = "";
                                        ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                                        // Remove video
                                        LibraryFile file = libraryFileCache[currentLibraryType][position];
                                        LibraryData.Unload(file);
                                        libraryFileCache[currentLibraryType].RemoveAt(position);
                                        demandChange = true;
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
                                    }
                                    else
                                    {
                                        // If there is a video in this position, this is the organize button
                                        if (libraryFileCache[currentLibraryType].Count > position)
                                        {
                                            // Replicate subtype objects
                                            Texture2D subTypeButton = GlobalContent.GetTexture("SubTypeButtonOrganize");
                                            int offset = 0;
                                            for(int s = 0; s < libraryTypes[currentRootType].Count; s++)
                                            {
                                                Rectangle subTypeRect = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71 + offset + 13 * s), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height));
                                                Global.mask.AddUnmaskedObject("SubType" + s, new SimpleObject(subTypeRect, Color.Gray, subTypeButton, () => {
                                                    if(subTypeRect.Contains(MouseInput.MouseState.Position))
                                                    {
                                                        organizeFile = position;
                                                        // Mouse position used to determine subtype button
                                                        Vector2 mousePosition = MouseInput.MouseState.Position.ToVector2();
                                                        for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                                        {
                                                            Rectangle subTypeRect = new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71+ 13 * i), GlobalGraphics.Scale(subTypeButton.Width), GlobalGraphics.Scale(subTypeButton.Height));
                                                            if (subTypeRect.Contains(mousePosition))
                                                            {
                                                                organizeType = i;
                                                                break;
                                                            }
                                                        }
                                                        organizing = false;
                                                        Global.mask.Disable();
                                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        return true;
                                                    }
                                                    return false;
                                                }));
                                            }
                                            int pagelessPosition = position - (12 * page);
                                            // Replicate videoplayer
                                            Texture2D videoPlayerTexture = GlobalContent.GetTexture("Pixel");
                                            if(currentRootType == LibraryRootType.Video)
                                                videoPlayerTexture = GlobalContent.GetTexture("StaticAnim" + staticAnim);
                                            else if(currentRootType == LibraryRootType.Audio)
                                                videoPlayerTexture = GlobalContent.GetTexture("AudioAnim" + audioAnim);
                                            if(videoPlayers.ContainsKey(pagelessPosition))
                                                videoPlayerTexture = videoPlayers[pagelessPosition];
                                            Global.mask.AddUnmaskedObject("VideoPlayer", new SimpleObject(staticRect, Color.White, videoPlayerTexture, () => {
                                                return false;
                                            }, false));
                                            // Replicate video holder
                                            Global.mask.AddUnmaskedObject("VideoHolder", new SimpleObject(videoHolderRect, Color.White, videoHolder, () => {
                                                if(!organizing)
                                                    return false;
                                                // If button 1 is pressed, undo
                                                if (button1Rect.Contains(MouseInput.MouseState.Position))
                                                {
                                                    organizing = false;
                                                    organizeFile = -1;
                                                    organizeType = -1;
                                                    Global.mask.Disable();
                                                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                    return true;
                                                }
                                                return false;
                                            }, false));
                                            if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                            {
                                                Texture2D videoToggle = libraryFileCache[currentLibraryType][position].Enabled ? videoOn : videoOff;
                                                Global.mask.AddUnmaskedObject("VideoToggle", new SimpleObject(toggleButtonRect, Color.White, videoToggle, () => {
                                                    return false;
                                                }, false));
                                            }
                                            else
                                            {
                                                Global.mask.AddUnmaskedObject("NoButton", new SimpleObject(noButtonRect, Color.White, renderNoButton, () => {
                                                    return false;
                                                }, false));
                                            }
                                            // Activate mask
                                            Global.mask.color = new Color(0, 0, 0, 128);
                                            Global.mask.Enable();
                                            organizing = true;
                                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            return true;
                                        }
                                        else
                                        {
                                            add = true;
                                        }
                                    }
                                }
                                else if (button2Rect.Contains(MouseInput.MouseState.Position) && left)
                                {
                                    if(deleteConfirmPos == position)
                                    {
                                        deleteConfirmPos = -1;
                                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
                                    }
                                    else
                                    {
                                        // Remove video button
                                        if (libraryFileCache[currentLibraryType].Count > position)
                                        {
                                            // If holding shift, delete video immediately
                                            if (newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift))
                                            {
                                                // Stop video playback
                                                FramePlayer.Stop();
                                                FramePlayer.canPlayBgMusic = true;
                                                if(UserInterface.instance.videoPlayer != null)
                                                {
                                                    UserInterface.instance.videoPlayer.Dispose();
                                                    UserInterface.instance.videoPlayer = null;
                                                }
                                                if(UserInterface.instance.video != null)
                                                {
                                                    UserInterface.instance.video.Dispose();
                                                    UserInterface.instance.video = null;
                                                }
                                                // Remove video
                                                LibraryFile file = libraryFileCache[currentLibraryType][position];
                                                LibraryData.Unload(file);
                                                libraryFileCache[currentLibraryType].RemoveAt(position);
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                // Delete confirm
                                                deleteConfirmPos = position;
                                                GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            return true;
                                        }
                                        else
                                        {
                                            add = true;
                                        }
                                    }
                                }
                                else if (toggleButtonRect.Contains(MouseInput.MouseState.Position) && left && deleteConfirmPos == -1)
                                {
                                    if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                    {
                                        // Toggle video button
                                        if (libraryFileCache[currentLibraryType].Count > position)
                                        {
                                            // Toggle video
                                            LibraryFile file = libraryFileCache[currentLibraryType][position];
                                            LibraryData.SetEnabled(file, !file.Enabled);
                                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            return true;
                                        }
                                        else
                                        {
                                            add = true;
                                        }
                                    }
                                }
                                if(add && left || (right && libraryFileCache[currentLibraryType].Count > position))
                                {
                                    if(right && MouseInput.MouseState.X >= GlobalGraphics.Scale(200) && MouseInput.MouseState.Y >= GlobalGraphics.Scale(71)
                                        && MouseInput.MouseState.X <= GlobalGraphics.Scale(305) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(219))
                                    {
                                        foreach(KeyValuePair<LibraryType, string> path in LibraryData.libraryPaths)
                                        {
                                            if(path.Key == currentLibraryType)
                                            {
                                                // Open directory and select file
                                                ProcessStartInfo startInfo = new()
                                                {
                                                    FileName = Path.GetFullPath(@"library\" + path.Value),
                                                    UseShellExecute = true,
                                                };
                                                Process.Start(startInfo);
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                return true;
                                            }
                                        }
                                    }
                                    else if(left && currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                    {
                                        // Add button: Open file dialog with filters from library type
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
#if WINDOWSDX
                                        if(!currentLibraryType.Special)
                                        {
                                            string filter = LibraryData.libraryNames[currentLibraryType] + "|";
                                            foreach (string extension in LibraryData.libraryFileTypes[currentLibraryType])
                                            {
                                                filter += "*" + extension + ";";
                                            }
                                            // Trim last semicolon
                                            filter = filter[..^1];
                                            System.Windows.Forms.OpenFileDialog openFileDialog = new()
                                            {
                                                Filter = filter,
                                                Multiselect = true,
                                                Title = "Add " + LibraryData.libraryNames[currentLibraryType]
                                            };
                                            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                bool success = true;
                                                foreach (string file in openFileDialog.FileNames)
                                                {
                                                    LibraryFile libraryFile = new(Path.GetFileNameWithoutExtension(file), file, currentLibraryType);
                                                    LibraryFile? newFile = LibraryData.Load(libraryFile);
                                                    if(newFile == null)
                                                    {
                                                        success = false;
                                                        continue;
                                                    }
                                                    libraryFileCache[currentLibraryType].Add(newFile);
                                                }
                                                demandChange = true;
                                                if(!success)
                                                {
                                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                                else
                                                {
                                                    GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                            }
                                        }
#endif
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                // Hovering over type buttons will set tooltip
                foreach (KeyValuePair<string, Rectangle> rect in rects)
                {
                    string rootString = currentRootType.ToString();
                    if (rect.Key.StartsWith(rootString))
                    {
                        // Mouse over?
                        if (!rect.Value.Contains(MouseInput.MouseState.Position))
                            continue;
                        // Get index
                        int index = libraryTypes[currentRootType].IndexOf(rect.Key.Substring(currentRootType.ToString().Length, rect.Key.Length - currentRootType.ToString().Length - 6));
                        if(index == -1)
                            continue;
                        // Get subtype
                        foreach (KeyValuePair<LibraryType, string> type in LibraryData.libraryNames)
                        {
                            if (type.Value == libraryTypes[currentRootType][index])
                            {
                                if(type.Key.CustomName == "")
                                {
                                    //tooltip = L.T(0, "Library:" + type.Key.ToString() + "Description");
                                    // It is actually really hard to get the type itself here
                                    // So just remove spaces for the library name and use that
                                    tooltip = L.T(0, "Library:" + type.Value.Replace(" ", "") + "Description");
                                }
                                else
                                {
                                    tooltip = type.Key.Description;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
                // Hovering over video holders will set tooltip
                if(libraryFileCache.Keys.Contains(currentLibraryType))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int position = i + (j * 3) + (12 * page);
                            Texture2D videoHolder = GlobalContent.GetTexture("VideoHolder");
                            Rectangle videoHolderRect = new Rectangle(GlobalGraphics.Scale(201 + (33 * i) + (i * 2)), GlobalGraphics.Scale(72 + (35 * j) + (j * 2)), GlobalGraphics.Scale(videoHolder.Width), GlobalGraphics.Scale(videoHolder.Height));
                            if (videoHolderRect.Contains(MouseInput.MouseState.Position))
                            {
                                if (libraryFileCache[currentLibraryType].Count > position)
                                {
                                    LibraryFile file = libraryFileCache[currentLibraryType][position];
                                    if (file.Path != null)
                                    {
                                        tooltip = Path.GetFileName(file.Path).Replace("\\", "/");
                                    }
                                }
                                else if(currentLibraryType != DefaultLibraryTypes.Render && currentLibraryType != DefaultLibraryTypes.NoImages)
                                {
                                    tooltip = L.T(0, "Library:AddMedia");
                                }
                            }
                        }
                    }
                }
            }
            // Interactable
            if(actionController.Update(gameTime, handleInput))
                return true;
            if(controller.Update(gameTime, handleInput))
                return true;
            return false;
        }
    }
}
