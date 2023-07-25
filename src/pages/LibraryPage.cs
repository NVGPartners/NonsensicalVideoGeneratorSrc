#if MONOGAME
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

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class LibraryPage : IPage
    {
        public string Name { get; set; } = "Library";
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
        private string clipUrl = "";
        private static bool downloading = false;
        private static KeyboardState oldKeyboardState;
        private static KeyboardState newKeyboardState;
        public void CacheLibrary()
        {
            libraryFileCache.Clear();
            // Get library types
            libraryTypes[LibraryRootType.Video] = LibraryData.GetLibraryNames(LibraryRootType.Video);
            libraryTypes[LibraryRootType.Audio] = LibraryData.GetLibraryNames(LibraryRootType.Audio);
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
            GameWindow? window = UserInterface.instance?.Window;
            if (window != null)
            {
                window.TextInput += TextInput;
            }
            // Library assets
            GlobalContent.AddTexture("DeleteConfirm", contentManager.Load<Texture2D>("graphics/library/deleteconfirm"));
            GlobalContent.AddTexture("AddVideoOverlay", contentManager.Load<Texture2D>("graphics/library/addvideooverlay"));
            GlobalContent.AddTexture("RenderAddVideoOverlay", contentManager.Load<Texture2D>("graphics/library/renderaddvideooverlay"));
            GlobalContent.AddTexture("HeaderButton", contentManager.Load<Texture2D>("graphics/library/headerbutton"));
            GlobalContent.AddTexture("HeaderButtonSelected", contentManager.Load<Texture2D>("graphics/library/headerbuttonselected"));
            GlobalContent.AddTexture("Separator", contentManager.Load<Texture2D>("graphics/library/separator"));
            GlobalContent.AddTexture("SubTypeButton", contentManager.Load<Texture2D>("graphics/library/subtypebutton"));
            GlobalContent.AddTexture("SubTypeButtonSelected", contentManager.Load<Texture2D>("graphics/library/subtypebuttonselected"));
            GlobalContent.AddTexture("TypeButton", contentManager.Load<Texture2D>("graphics/library/typebutton"));
            GlobalContent.AddTexture("TypeButtonSelected", contentManager.Load<Texture2D>("graphics/library/typebuttonselected"));
            GlobalContent.AddTexture("VideoHolder", contentManager.Load<Texture2D>("graphics/library/videoholder"));
            GlobalContent.AddTexture("VideoOff", contentManager.Load<Texture2D>("graphics/library/videooff"));
            GlobalContent.AddTexture("VideoOn", contentManager.Load<Texture2D>("graphics/library/videoon"));
            GlobalContent.AddTexture("RenderNoButton", contentManager.Load<Texture2D>("graphics/library/rendernobutton"));
            GlobalContent.AddTexture("SubTypeButtonOrganize", contentManager.Load<Texture2D>("graphics/library/subtypebuttonorganize"));
            GlobalContent.AddTexture("StaticOverlay", contentManager.Load<Texture2D>("graphics/library/staticoverlay"));
            // TV Static Animation: 13 frames as graphics/library/staticanim/staticanim0 to graphics/library/staticanim/staticanim12
            for(int i = 0; i < 13; i++)
            {
                GlobalContent.AddTexture("StaticAnim" + i, contentManager.Load<Texture2D>("graphics/library/staticanim/staticanim" + i));
            }
            // Vinyl Record Animation: 2 frames as graphics/library/audioanim/audioanim0 to graphics/library/audioanim/audioanim1
            for(int i = 0; i < 2; i++)
            {
                GlobalContent.AddTexture("AudioAnim" + i, contentManager.Load<Texture2D>("graphics/library/audioanim/audioanim" + i));
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
            rects.Add("VideoButton", new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("AudioButton", new Rectangle(GlobalGraphics.Scale(166), GlobalGraphics.Scale(56), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("HeaderButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(56), GlobalGraphics.Scale(headerButton.Width), GlobalGraphics.Scale(headerButton.Height)));
            rects.Add("PageLeftButton", new Rectangle(GlobalGraphics.Scale(200), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            rects.Add("PageRightButton", new Rectangle(GlobalGraphics.Scale(276), GlobalGraphics.Scale(223), GlobalGraphics.Scale(typeButton.Width), GlobalGraphics.Scale(typeButton.Height)));
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
        public static void Done(bool success)
        {
            downloading = false;
            if(success)
            {
                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Downloaded clip.";
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Failed to download clip.";
            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // changes
            if(changed)
            {
                // Load video players
                if(currentRootType == LibraryRootType.Video)
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
            SpriteFont munroSmall = GlobalContent.GetFont("MunroSmall");
            // Draw background
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(56), GlobalGraphics.Scale(170), GlobalGraphics.Scale(15)), Color.Gray);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(219), GlobalGraphics.Scale(170), GlobalGraphics.Scale(17)), Color.Gray);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(71), GlobalGraphics.Scale(65), GlobalGraphics.Scale(148)), Color.Gray);
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
                            if(currentRootType == LibraryRootType.Video)
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, new Color(64, 64, 64, 255));
                                int pagelessPosition = position - (12 * page);
                                if(videoPlayers.ContainsKey(pagelessPosition))
                                {
                                    spriteBatch.Draw(videoPlayers[pagelessPosition], staticRect, Color.White);
                                }
                            }
                            else
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("AudioAnim" + audioAnim), staticRect, Color.White);
                            }
                        }
                        else
                        {   
                            spriteBatch.Draw(GlobalContent.GetTexture("StaticAnim" + staticAnim), staticRect, Color.White);
                            if(currentLibraryType != DefaultLibraryTypes.Render)
                            {
                                spriteBatch.Draw(GlobalContent.GetTexture("StaticOverlay"), staticRect, new Color(255, 255, 255, 128));
                            }
                        }
                        // Draw video holder
                        spriteBatch.Draw(videoHolder, videoHolderRect, Color.White);
                        if(!video)
                        {
                            spriteBatch.Draw(addVideoOverlay, addVideoOverlayRect, Color.White);
                            if(currentLibraryType == DefaultLibraryTypes.Render)
                            {
                                spriteBatch.Draw(renderAddVideoOverlay, renderAddVideoOverlayRect, Color.White);
                            }
                        }
                        else if(deleteConfirmPos == position)
                        {
                            spriteBatch.DrawString(munroSmall, "Delete?", new Vector2(deleteConfirmRect.X - GlobalGraphics.Scale(1-1), deleteConfirmRect.Y - GlobalGraphics.Scale(14-1)), Color.Black);
                            spriteBatch.DrawString(munroSmall, "Delete?", new Vector2(deleteConfirmRect.X - GlobalGraphics.Scale(1), deleteConfirmRect.Y - GlobalGraphics.Scale(14)), Color.White);
                            spriteBatch.Draw(deleteConfirm, deleteConfirmRect, Color.White);
                        }
                        else if(currentLibraryType != DefaultLibraryTypes.Render)
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
            if(currentLibraryType != DefaultLibraryTypes.Render)
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
            if(currentLibraryType != DefaultLibraryTypes.Render)
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
            spriteBatch.DrawString(munroSmall, "Video", new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Video", new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(56)), Color.White);
            spriteBatch.DrawString(munroSmall, "Audio", new Vector2(GlobalGraphics.Scale(170 + 1), GlobalGraphics.Scale(56 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Audio", new Vector2(GlobalGraphics.Scale(170), GlobalGraphics.Scale(56)), Color.White);
            spriteBatch.DrawString(munroSmall, "Back", new Vector2(GlobalGraphics.Scale(205 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Back", new Vector2(GlobalGraphics.Scale(205), GlobalGraphics.Scale(223)), Color.White);
            spriteBatch.DrawString(munroSmall, "Next", new Vector2(GlobalGraphics.Scale(281 + 1), GlobalGraphics.Scale(223 + 1)), Color.Black);
            spriteBatch.DrawString(munroSmall, "Next", new Vector2(GlobalGraphics.Scale(281), GlobalGraphics.Scale(223)), Color.White);
            // Downloader
#if WINDOWSDX
            if(currentLibraryType != DefaultLibraryTypes.Render)
            {
                string totalIndicator = "Click to download media";
                if((selectedFlags & 4) == 4)
                    totalIndicator =    "Paste URLs to download";
                if(downloading)
                    totalIndicator =    "Downloading...";
                Vector2 totalIndicatorSize = munroSmall.MeasureString(totalIndicator);
                Vector2 totalPosition = new Vector2((rects["HeaderButton"].X + rects["HeaderButton"].Width / 2 - totalIndicatorSize.X / 2) + GlobalGraphics.Scale(1), GlobalGraphics.Scale(56));
                spriteBatch.DrawString(munroSmall, totalIndicator, totalPosition + new Vector2(GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(munroSmall, totalIndicator, totalPosition, Color.White);
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
                string pageIndicator = (page + 1) + "/" + maxPages;
                Vector2 pageIndicatorSize = munroSmall.MeasureString(pageIndicator);
                int pivot = 252; 
                spriteBatch.DrawString(munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot + 1) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223 + 1)), Color.Black);
                spriteBatch.DrawString(munroSmall, pageIndicator, new Vector2(GlobalGraphics.Scale(pivot) - pageIndicatorSize.X / 2, GlobalGraphics.Scale(223)), Color.White);
            }
            int offset = 0;
            for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
            {
                spriteBatch.DrawString(munroSmall, libraryTypes[currentRootType][i], new Vector2(GlobalGraphics.Scale(139 + 1), GlobalGraphics.Scale(71 + offset + 13 * i + 1)), Color.Black);
                spriteBatch.DrawString(munroSmall, libraryTypes[currentRootType][i], new Vector2(GlobalGraphics.Scale(139), GlobalGraphics.Scale(71 + offset + 13 * i)), Color.White);
            }
            // Interactable
            controller.Draw(gameTime, spriteBatch);
            if(tooltip != "")
            {
                Vector2 tooltipSize;
                try
                {
                    // Get text size
                    tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString(tooltip);
                }
                catch (Exception)
                {
                    tooltipSize = GlobalGraphics.fontMunroSmall.MeasureString("(INVALID TEXT)");
                    string extension = Path.GetExtension(tooltip);
                    tooltip = "(INVALID TEXT)" + extension;
                }
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 10, MouseInput.MouseState.Position.Y + 10);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2); 
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), new Color(0, 0, 0, 255));
                // White text
                spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
        }
        // Thread for loading videos
        private BackgroundWorker loadVideosThread;
        private void LoadVideosThread(object? sender, DoWorkEventArgs e)
        {
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
                            // Get thumbnail of video using shell
                            ShellFile shellFile = ShellFile.FromFilePath(libraryFile.Path);
                            BitmapSource bitmapSource = shellFile.Thumbnail.BitmapSource;
                            // Check to make sure we're still on the same page
                            if (currentPage != page)
                                return;
                            // Cancelled?
                            if (loadVideosThread != null && loadVideosThread.CancellationPending)
                                return;
                            // Convert to texture
                            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            bitmapSource.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                            bitmap.UnlockBits(data);
                            Texture2D texture = new(graphicsDevice, bitmap.Width, bitmap.Height);
                            Color[] colorData = new Color[bitmap.Width * bitmap.Height];
                            for (int i = 0; i < colorData.Length; i++)
                            {
                                System.Drawing.Color color = bitmap.GetPixel(i % bitmap.Width, i / bitmap.Width);
                                colorData[i] = new Color(color.R, color.G, color.B, color.A);
                            }
                            texture.SetData(colorData);
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
                        Global.generatorFactory.progressText = "Downloading...";
                        downloading = true;
                        string clipboard = Clipboard.GetText();
                        // Remove invalid characters
                        clipboard = clipboard.Replace("\r", "");
                        clipboard = clipboard.Replace("\t", "");
                        clipboard = clipboard.Replace("\0", "");
                        if (!LibraryData.DownloadClip(clipboard.Split('\n'), currentLibraryType))
                        {
                            downloading = false;
                            Global.generatorFactory.progressText = "Failed to download media.";
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                }
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
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
                LibraryType libraryType = DefaultLibraryTypes.All;
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
                demandChange = true;
                if(!success)
                {
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                }
                else
                {
                    GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                    if(currentLibraryType == DefaultLibraryTypes.Render && rects.Keys.ElementAt(i) == "HeaderButton")
#elif DESKTOPGL
                    if(rects.Keys.ElementAt(i) == "HeaderButton")   
#endif
                        continue;
                    if(rects.Keys.ElementAt(i).Contains("Audio") && rects.Keys.ElementAt(i) != "AudioButton" && currentRootType != LibraryRootType.Audio)
                        continue;
                    if(rects.Keys.ElementAt(i).Contains("Video") && rects.Keys.ElementAt(i) != "VideoButton" && currentRootType != LibraryRootType.Video)
                        continue;
                    string tts = rects.Keys.ElementAt(i);
                    switch(tts)
                    {
                        case "HeaderButton":
                            string totalIndicator = "Click to download media";
                            if((selectedFlags & 4) == 4)
                                totalIndicator = "Paste URLs to download";
                            if(downloading)
                                totalIndicator = "Downloading...";
                            tts = totalIndicator;
                            break;
                    }
                    bool selected = false;
                    // selectedFlags 1, 2, 4 are each button
                    // bitwise
                    if((selectedFlags & 1) == 1 && rects.Keys.ElementAt(i) == "VideoButton")
                        selected = true;
                    if((selectedFlags & 2) == 2 && rects.Keys.ElementAt(i) == "AudioButton")
                        selected = true;
                    if((selectedFlags & 4) == 4 && rects.Keys.ElementAt(i) == "HeaderButton")
                        selected = true;
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
                                Accessibility.CompatAccessibility(button1Rect, (deleteConfirmPos == position ? "Confirm delete " : "Organize ") + "\"" + Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) + "\"");
                                Accessibility.CompatAccessibility(button2Rect, (deleteConfirmPos == position ? "Cancel delete " : "Delete ") + "\"" + Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) + "\"");
                                if(currentLibraryType != DefaultLibraryTypes.Render)
                                    Accessibility.CompatAccessibility(toggleButtonRect, (libraryFileCache[currentLibraryType][position].Enabled ? "Disable " : "Enable ") + "\"" + Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) + "\"");
                                Accessibility.CompatAccessibility(staticRect, "Open \"" + Path.GetFileName(libraryFileCache[currentLibraryType][position].Path) + "\"");
                            }
                            else
                            {
                                if(currentLibraryType != DefaultLibraryTypes.Render)
                                    Accessibility.CompatAccessibility(staticRect, "Add media");
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
                    if(left)
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
                                        selectedFlags |= 1;
                                        selectedFlags &= ~2;
                                        for (int i = 0; i < libraryTypes[currentRootType].Count; i++)
                                        {
                                            selectedFlags &= ~(8 << i);
                                        }
                                        selectedFlags |= 8;
                                        selectedFlags &= ~4;
                                        currentRootType = LibraryRootType.Video;
                                        currentLibraryType = DefaultLibraryTypes.Material;
                                        demandChange = true;
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
                                    case "AudioButton":
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
                                        demandChange = true;
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        return true;
#if WINDOWSDX
                                    case "HeaderButton":
                                        if(libraryFileCache.Keys.Contains(currentLibraryType))
                                        {
                                            if(currentLibraryType != DefaultLibraryTypes.Render)
                                            {
                                                if(!downloading)
                                                {
                                                    selectedFlags ^= 4;
                                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                                else
                                                {
                                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
#endif
                                    case "PageLeftButton":
                                        if(libraryFileCache.Keys.Contains(currentLibraryType))
                                        {
                                            if (page > 0)
                                            {
                                                page--;
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
                                    case "PageRightButton":
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
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        return true;
                                    default:
                                        // Check if it's a subtype button
                                        if (rect.Key.StartsWith(currentRootType.ToString()))
                                        {
                                            // Get index
                                            int index = libraryTypes[currentRootType].IndexOf(rect.Key.Substring(currentRootType.ToString().Length, rect.Key.Length - currentRootType.ToString().Length - 6));
                                            if(index == -1)
                                            {
                                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                FramePlayer.PlayMedia(file);
                                            }
                                            if(right)
                                            {
                                                // Open directory and select file
                                                ProcessStartInfo startInfo = new()
                                                {
                                                    FileName = "explorer.exe",
                                                    Arguments = "/select, \"" + (Path.GetFullPath(file.Path)) + "\""
                                                };
                                                Process.Start(startInfo);
                                            }
                                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                        }
                                        else
                                        {
                                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                        // Remove video
                                        LibraryFile file = libraryFileCache[currentLibraryType][position];
                                        LibraryData.Unload(file);
                                        libraryFileCache[currentLibraryType].RemoveAt(position);
                                        demandChange = true;
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                        return true;
                                                    }
                                                    return false;
                                                }));
                                            }
                                            int pagelessPosition = position - (12 * page);
                                            bool useVideoPlayer = currentRootType == LibraryRootType.Video;
                                            if(useVideoPlayer && !videoPlayers.ContainsKey(pagelessPosition))
                                                useVideoPlayer = false;
                                            // Replicate videoplayer
                                            Texture2D videoPlayerTexture = useVideoPlayer ? videoPlayers[pagelessPosition] : GlobalContent.GetTexture("AudioAnim" + audioAnim);
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
                                                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                    return true;
                                                }
                                                return false;
                                            }, false));
                                            if(currentLibraryType != DefaultLibraryTypes.Render)
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
                                            Global.mask.Enable();
                                            organizing = true;
                                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                // Remove video
                                                LibraryFile file = libraryFileCache[currentLibraryType][position];
                                                LibraryData.Unload(file);
                                                libraryFileCache[currentLibraryType].RemoveAt(position);
                                                demandChange = true;
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                            }
                                            else
                                            {
                                                // Delete confirm
                                                deleteConfirmPos = position;
                                                GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                    if(currentLibraryType != DefaultLibraryTypes.Render)
                                    {
                                        // Toggle video button
                                        if (libraryFileCache[currentLibraryType].Count > position)
                                        {
                                            // Toggle video
                                            LibraryFile file = libraryFileCache[currentLibraryType][position];
                                            LibraryData.SetEnabled(file, !file.Enabled);
                                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                return true;
                                            }
                                        }
                                    }
                                    else if(left && currentLibraryType != DefaultLibraryTypes.Render)
                                    {
                                        // Add button: Open file dialog with filters from library type
                                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                                else
                                                {
                                                    GlobalContent.GetSound("AddSource").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                                }
                                            }
                                        }
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
                    if (rect.Key.StartsWith(currentRootType.ToString()))
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
                                tooltip = type.Key.Description;
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
                                else if(currentLibraryType != DefaultLibraryTypes.Render)
                                {
#if WINDOWSDX
                                    tooltip = "Add Media: Click or Drag and Drop";
#elif DESKTOPGL
                                    tooltip = "Add Media";
#endif
                                }
                            }
                        }
                    }
                }
            }
            // Interactable
            if(controller.Update(gameTime, handleInput))
                return true;
            return false;
        }
    }
}
#endif
