using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using MonoGame.Extended.VideoPlayback;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class GeneratePage : IPage
    {
        public string Name { get; set; } = "PageGenerate";
        public string Tooltip { get; } = "Render a nonsensical video.";
        private readonly ScrollView scrollView = new();
        private readonly InteractableController actionController = new();
        private readonly InteractableController actionControllerRendering = new();
        private readonly InteractableController controllerRendering = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Global.generator.generatorActive)
            {
                if(controllerRendering.Update(gameTime, handleInput))
                    return true;
                if(actionControllerRendering.Update(gameTime, handleInput))
                    return true;
            }
            else
            {
                if(scrollView.Update(gameTime, handleInput))
                    return true;
                if(actionController.Update(gameTime, handleInput))
                    return true;
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(Global.generator.generatorActive)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), ThemeManager.GetColor("OverlayContentScreen"));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), ThemeManager.GetColor("OverlayContentScreen"));
                // Draw text to indicate that rendering is in progress
                SpriteFont font = L.FontLarge();
                string text = L.T(0, "Generate:Rendering");
                if(Global.generator.pluginEffectTest != null)
                    text = L.T(0, "Generate:EffectTestInProgress");
                Vector2 textSize = font.MeasureString(text);
                GlobalContent.DrawString(spriteBatch, font, text, new Vector2(GlobalGraphics.Scale(1) + GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(1) + GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.Black);
                GlobalContent.DrawString(spriteBatch, font, text, new Vector2(GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.White);
                controllerRendering.Draw(gameTime, spriteBatch);
                actionControllerRendering.Draw(gameTime, spriteBatch);
            }
            else
            {
                actionController.Draw(gameTime, spriteBatch);
                scrollView.Draw(gameTime, spriteBatch);
            }
        }
        public bool ConsoleButton(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(Global.ready)
                    {
                        Global.editing = "";
                        Accessibility.allowAccessibility = true;
                        ConsoleScreen? consoleScreen = ScreenManager.GetScreen<ConsoleScreen>("Console");
                        if(consoleScreen != null)
                        {
                            if(consoleScreen.Toggle())
                            {
                                ConsoleOutput.ResetScroll();
                                GlobalContent.PlaySound("Select");
                                if(Accessibility.showDisambiguation)
                                    Accessibility.TTS(L.T(0, "Accessibility:ConsoleShown"));
                                //UserInterface.instance.music = 0;
                            }
                            else
                            {
                                GlobalContent.PlaySound("Back");
                                if(Accessibility.showDisambiguation)
                                    Accessibility.TTS(L.T(0, "Accessibility:ConsoleHidden"));
                            }
                        }
                        else
                        {
                            ConsoleOutput.WriteLine("Console not found!!!");
                            GlobalContent.PlaySound("Error");
                            if(Accessibility.showDisambiguation)
                                Accessibility.TTS(L.T(0, "Accessibility:ConsoleNotFound"));
                        }
                    }
                    return true;
            }
            return false;
        }
        public bool GenerateButton(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(!Global.canRender || Global.generator.generatorActive)
                    {
                        GlobalContent.PlaySound("Error");
                        return true;
                    }
                    GlobalContent.PlaySound("Select");
                    Global.generator.StartGeneration((sender, e) => {
                        if(e.ProgressPercentage == 100)
                        {
                            try
                            {
                                // award achievements
                                if (SteamManager.initialized && Global.canAchieve)
                                {
                                    List<string> achievements = new()
                                    {
                                        "ACHIEVEMENT_FIRST_RENDER",
                                    };
                                    if(Global.usedWorkshopPlugin)
                                    {
                                        Global.usedWorkshopPlugin = false;
                                        achievements.Add("ACHIEVEMENT_WORKSHOP_USAGE");
                                    }
                                    if(Global.rolledForOverlay)
                                    {
                                        Global.rolledForOverlay = false;
                                        achievements.Add("ACHIEVEMENT_CHROMA_KEY");
                                    }
                                    if(Global.usedAllEffectChance)
                                    {
                                        Global.usedAllEffectChance = false;
                                        achievements.Add("ACHIEVEMENT_ALL_EFFECTS");
                                    }
                                    if(Global.usedDifferentOutro)
                                    {
                                        Global.usedDifferentOutro = false;
                                        achievements.Add("ACHIEVEMENT_OUTRO_OVERRIDE");
                                    }
                                    foreach(string achievement in achievements)
                                    {
                                        Achievements.Award(achievement);
                                    }
                                }
                            }
                            catch {}
                            SaveData.saveValues["TotalVideosRendered"] = (int.Parse(SaveData.saveValues["TotalVideosRendered"], CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture);
                            SaveData.Save();
                            GlobalContent.PlaySound("RenderComplete");
                        }
                        else
                        {
                            GlobalContent.PlaySound("Error");
                        }
                        Global.justCompletedRender = true;
                        //SteamUserStats.SetAchievement("RENDER_VIDEO");
                    }, (sender, e) => {});
                    return true;
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Clear all controllers
            actionController.Clear();
            actionControllerRendering.Clear();
            controllerRendering.Clear();
            scrollView.Controller.Clear();
            // Actions
            actionControllerRendering.Add("ActionConsole", new ActionButton("View console output.", new Vector2(112, 221), ConsoleButton, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/console")));
            actionController.Add("ActionConsole", new ActionButton("View console output.", new Vector2(112, 221), ConsoleButton, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/console")));
            actionController.Add("ActionReset", new ActionButton("Reset to default parameters.", new Vector2(112, 206), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.PlaySound("Select");
                        //SaveData.saveValues["ScreenScale"] = "2";
                        //SaveData.saveValues["BackgroundSaturation"] = "0";
                        SaveData.saveValues["MinStreamDuration"] = "0.2";
                        SaveData.saveValues["MaxStreamDuration"] = "0.4";
                        SaveData.saveValues["MaxClipCount"] = "20";
                        SaveData.saveValues["VideoWidth"] = "640";
                        SaveData.saveValues["VideoHeight"] = "480";
                        SaveData.saveValues["IntrosEnabled"] = "false";
                        SaveData.saveValues["OutrosEnabled"] = "true";
                        SaveData.saveValues["PlayAutomatically"] = "true";
                        //SaveData.saveValues["GameHighScore"] = "0";
                        //SaveData.saveValues["MusicVolume"] = "50";
                        //SaveData.saveValues["SoundEffectVolume"] = "65";
                        //SaveData.saveValues["VideoVolume"] = "100";
                        SaveData.saveValues["TransitionChance"] = "20";
                        SaveData.saveValues["OverlayChance"] = "20";
                        SaveData.saveValues["EffectChance"] = "60";
                        SaveData.saveValues["TransitionEffects"] = "true";
                        SaveData.saveValues["TransitionEffectChance"] = "30";
                        //SaveData.saveValues["HiddenKeepTemporaryJobFolders"] = "false";
                        //SaveData.saveValues["HiddenVerbose"] = "false";
                        //SaveData.saveValues["DisableMotion"] = "false";
                        //SaveData.saveValues["FirstBoot"] = "true";
                        //SaveData.saveValues["DisabledMedia"] = "[]";
                        //SaveData.saveValues["TotalVideosRendered"] = "0";
                        //SaveData.saveValues["TotalMediaImported"] = "0";
                        //SaveData.saveValues["TotalClipsTrimmed"] = "0";
                        SaveData.saveValues["PlayOverlayInFull"] = "false";
                        SaveData.saveValues["MaxUniqueClips"] = "0";
                        SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                        SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "false";
                        SaveData.saveValues["ConstrainAspectRatio"] = "true";
                        SaveData.saveValues["EnableTimeOut"] = "true";
                        SaveData.saveValues["TimeOut"] = "60";
                        SaveData.saveValues["VideoFPS"] = "30";
                        SaveData.Save();
                        scrollView.Controller.interactables["MinStreamDuration"].Tooltip = SaveData.saveValues["MinStreamDuration"];
                        scrollView.Controller.interactables["MaxStreamDuration"].Tooltip = SaveData.saveValues["MaxStreamDuration"];
                        scrollView.Controller.interactables["ClipCount"].Tooltip = SaveData.saveValues["MaxClipCount"];
                        scrollView.Controller.interactables["Width"].Tooltip = SaveData.saveValues["VideoWidth"];
                        scrollView.Controller.interactables["Height"].Tooltip = SaveData.saveValues["VideoHeight"];
                        ((Switch)scrollView.Controller.interactables["SaveToLibrary"]).SwitchState = SaveData.saveValues["PlayAutomatically"] == "true";
                        ((Switch)scrollView.Controller.interactables["InsertIntro"]).SwitchState = SaveData.saveValues["IntrosEnabled"] == "true";
                        ((Switch)scrollView.Controller.interactables["InsertOutro"]).SwitchState = SaveData.saveValues["OutrosEnabled"] == "true";
                        scrollView.Controller.interactables["TransitionChance"].Tooltip = SaveData.saveValues["TransitionChance"];
                        scrollView.Controller.interactables["OverlayChance"].Tooltip = SaveData.saveValues["OverlayChance"];
                        scrollView.Controller.interactables["EffectChance"].Tooltip = SaveData.saveValues["EffectChance"];
                        ((Switch)scrollView.Controller.interactables["TransitionEffects"]).SwitchState = SaveData.saveValues["TransitionEffects"] == "true";
                        scrollView.Controller.interactables["TransitionEffectChance"].Tooltip = SaveData.saveValues["TransitionEffectChance"];
                        ((Switch)scrollView.Controller.interactables["PlayOverlayInFull"]).SwitchState = SaveData.saveValues["PlayOverlayInFull"] == "true";
                        scrollView.Controller.interactables["MaxUniqueClips"].Tooltip = SaveData.saveValues["MaxUniqueClips"];
                        ((Switch)scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"]).SwitchState = SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] == "true";
                        ((Switch)scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"]).SwitchState = SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] == "true";
                        ((Switch)scrollView.Controller.interactables["ConstrainAspectRatio"]).SwitchState = SaveData.saveValues["ConstrainAspectRatio"] == "true";
                        ((Switch)scrollView.Controller.interactables["EnableTimeOut"]).SwitchState = SaveData.saveValues["EnableTimeOut"] == "true";
                        scrollView.Controller.interactables["TimeOut"].Tooltip = SaveData.saveValues["TimeOut"];
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/reset")));
            actionController.Add("ActionPlayLast", new ActionButton("Play last rendered video.", new Vector2(112, 191), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        string renderFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "library", "video", "renders");
                        // Get last modified file
                        string lastModifiedFile = "";
                        DateTime lastModified = new(0);
                        foreach(string fileName in Directory.GetFiles(renderFolder))
                        {
                            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                            if(lastWriteTime > lastModified)
                            {
                                lastModified = lastWriteTime;
                                lastModifiedFile = fileName;
                            }
                        }
                        if (lastModifiedFile != "")
                        {
                            LibraryFile file = new LibraryFile(Path.GetFileName(lastModifiedFile), lastModifiedFile, DefaultLibraryTypes.Render, true);
                            if(file.Path != null)
                            {
                                FramePlayer.Stop();
                                GlobalContent.PlaySound("Select");
                                if(UserInterface.instance != null)
                                {
                                    if(UserInterface.instance.videoPlayer != null)
                                    {
                                        UserInterface.instance.videoPlayer.Dispose();
                                        Global.videoPlaying = false;
                                        UserInterface.instance.videoPlayer = null;
                                    }
                                    UserInterface.instance.videoPlayer = new MonoGame.Extended.Framework.Media.VideoPlayer(UserInterface.instance.GraphicsDevice);
                                    FramePlayer.canPlayBgMusic = true;
                                    if(UserInterface.instance.video != null)
                                    {
                                        UserInterface.instance.video.Dispose();
                                        UserInterface.instance.video = null;
                                    }
                                    string cachePath = VideoCache.GetCachePath(file.Path);
                                    UserInterface.instance.videoPath = cachePath;
                                    UserInterface.instance.video = VideoHelper.LoadFromFile(cachePath);
                                    //UserInterface.instance.videoPlayer.IsLooped = true;
                                    UserInterface.instance.videoPlayer.Play(UserInterface.instance.video);
                                    Global.videoPlaying = true;
                                    UserInterface.instance.videoPlayer.Volume = float.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
                                }
                                FramePlayer.canPlayBgMusic = false;
                                Global.generator.progressText = L.T(0, "Video:StatusPlay");
                                if(ScreenManager.GetScreen<VideoScreen>("Video") == null
                                    || ScreenManager.GetScreen<VideoScreen>("Video")?.screenType == ScreenType.Hidden)
                                {
                                    ScreenManager.PushNavigation("Video");
                                    ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                                }
                                return true;
                            }
                        }
                        GlobalContent.PlaySound("Error");
                        return true;
                }
                return false;
            }, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/playlast")));
            actionController.Add("ActionRender", new ActionButton("Start generating a new video.", new Vector2(112, 176), GenerateButton, ThemeManager.LoadLayeredContent<Texture2D>("graphics/actions/render")));
            // RENDERING MODE
            controllerRendering.Add("Cancel", new Button("Cancel", "Stop rendering.", new Vector2(119+36, 60+10+(19*8)), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.PlaySound("Select");
                        Global.generator.CancelGeneration(true);
                        return true;
                }
                return false;
            }));
            controllerRendering.Add("ForceConcatenate", new Button("Combine Clips Now", "Stop rendering and force concatenation.", new Vector2(119+36+104, 60+10+(19*8)), (int i, string n) => {
                switch(i)
                {
                    case 2: // left click
                        if (Global.generator.pluginEffectTest != null)
                        {
                            GlobalContent.PlaySound("Select");
                            Global.generator.CancelGeneration(true, true);
                        }
                        else
                        {
                            GlobalContent.PlaySound("Error");
                        }
                        return true;
                }
                return false;
            }));
            // PAGE 3
            scrollView.Controller.Add("DisableClipsAfterMaxUniqueClips", new Switch("Disable Clips After Max Reached", "Disable clips after they reach the max unique clip count.", new Vector2(139, 60-(8*3)+(19*20)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    // If MaxUniqueClips is 0, force disable this switch and DeleteClipsAfterMaxUniqueClips
                    if (int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture) == 0)
                    {
                        GlobalContent.PlaySound("Error");
                        SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "false";
                        SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                        Switch? pageSwitches = scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"] as Switch;
                        if (pageSwitches != null)
                            pageSwitches.SwitchState = false;
                        Switch? deleteSwitches = scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch;
                        if (deleteSwitches != null)
                            deleteSwitches.SwitchState = false;
                        SaveData.Save();
                        return false;
                    }
                    string oldValue = SaveData.saveValues["DisableClipsAfterMaxUniqueClips"];
                    SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = switchState.ToString().ToLower();
                    if (oldValue != SaveData.saveValues["DisableClipsAfterMaxUniqueClips"])
                    {
                        // At least one switch must be enabled
                        SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = (!switchState).ToString().ToLower();
                        Switch? pageSwitches = scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch;
                        if (pageSwitches != null)
                            pageSwitches.SwitchState = !switchState;
                        SaveData.Save();
                    }
                }
                return switchState;
            }, SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] == "true"));
            scrollView.Controller.Add("DeleteClipsAfterMaxUniqueClips", new Switch("Delete Clips After Max Reached", "Delete clips after they reach the max unique clip count.", new Vector2(139, 60-(8*3)+(19*19)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    // If MaxUniqueClips is 0, force disable this switch and DisableClipsAfterMaxUniqueClips
                    if (int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture) == 0)
                    {
                        GlobalContent.PlaySound("Error");
                        SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "false";
                        SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                        Switch? pageSwitches = scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"] as Switch;
                        if (pageSwitches != null)
                            pageSwitches.SwitchState = false;
                        Switch? deleteSwitches = scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch;
                        if (deleteSwitches != null)
                            deleteSwitches.SwitchState = false;
                        SaveData.Save();
                        return false;
                    }
                    string oldValue = SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"];
                    SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = switchState.ToString().ToLower();
                    if (oldValue != SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"])
                    {
                        // At least one switch must be enabled
                        SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = (!switchState).ToString().ToLower();
                        Switch? pageSwitches = scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"] as Switch;
                        if (pageSwitches != null)
                            pageSwitches.SwitchState = !switchState;
                        SaveData.Save();
                    }
                }
                return switchState;
            }, SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] == "true"));
            scrollView.Controller.Add("MaxUniqueClips", new TextEntry("Max Unique Media", "The max times a unique media file can be used.", SaveData.saveValues["MaxUniqueClips"], new Vector2(139, 60-(8*3)+(19*18)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture);
                // Range: 0-100
                if(int.Parse(scrollView.Controller.interactables["MaxUniqueClips"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["MaxUniqueClips"].Tooltip = "0";
                SaveData.saveValues["MaxUniqueClips"] = scrollView.Controller.interactables["MaxUniqueClips"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                // If changed from 0 to any other value, enable "DisableClipsAfterMaxUniqueClips" and disable "DeleteClipsAfterMaxUniqueClips"
                if (oldValue == 0 && int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture) > 0)
                {
                    Switch? pageSwitches = scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"] as Switch;
                    if (pageSwitches != null)
                        pageSwitches.SwitchState = true;
                    pageSwitches = scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch;
                    if (pageSwitches != null)
                        pageSwitches.SwitchState = false;
                    SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                    SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "true";
                }
                // If changed to 0 from any other value, disable both "DisableClipsAfterMaxUniqueClips" and "DeleteClipsAfterMaxUniqueClips"
                if (oldValue != 0 && int.Parse(SaveData.saveValues["MaxUniqueClips"], CultureInfo.InvariantCulture) == 0)
                {
                    Switch? pageSwitches = scrollView.Controller.interactables["DisableClipsAfterMaxUniqueClips"] as Switch;
                    if (pageSwitches != null)
                        pageSwitches.SwitchState = false;
                    pageSwitches = scrollView.Controller.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch;
                    if (pageSwitches != null)
                        pageSwitches.SwitchState = false;
                    SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                    SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "false";
                }
                return false;
            }));
            scrollView.Controller.Add("RepetitionOptions", new Label("Repetition Options:", new Vector2(139, 60-(8*2)+(19*17)+(10*1)+(9*1))));
            // ADVANCED MODE
            scrollView.Controller.Add("EnableTimeOut", new Switch("", "Cancel operations if they take too long.", new Vector2(139, 60-(8*2)+(19*16)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["EnableTimeOut"];
                    SaveData.saveValues["EnableTimeOut"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["EnableTimeOut"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["EnableTimeOut"] == "true"));
            scrollView.Controller.Add("TimeOut", new TextEntry("Time Out", "How long until operations are canceled, in seconds.", SaveData.saveValues["TimeOut"], new Vector2(168, 60-(8*2)+(19*16)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["TimeOut"], CultureInfo.InvariantCulture);
                // Range: 0-100
                if(int.Parse(scrollView.Controller.interactables["TimeOut"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["TimeOut"].Tooltip = "0";
                SaveData.saveValues["TimeOut"] = scrollView.Controller.interactables["TimeOut"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TimeOut"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("ConstrainAspectRatio", new Switch("Constrain Aspect Ratio", "Clips will retain their original aspect ratio when disabled.", new Vector2(139, 60-(8*2)+(19*15)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["ConstrainAspectRatio"];
                    SaveData.saveValues["ConstrainAspectRatio"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["ConstrainAspectRatio"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["ConstrainAspectRatio"] == "true"));
            scrollView.Controller.Add("PlayOverlayInFull", new Switch("Overlays Play in Full", "Play overlays at their full length.", new Vector2(139, 60-(8*2)+(19*14)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["PlayOverlayInFull"];
                    SaveData.saveValues["PlayOverlayInFull"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["PlayOverlayInFull"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["PlayOverlayInFull"] == "true"));                       
            scrollView.Controller.Add("TransitionEffectChance", new TextEntry("Transition Effect Chance", "How often transitions get effects, from 0-100.", SaveData.saveValues["TransitionEffectChance"], new Vector2(139, 60-(8*2)+(19*13)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionEffectChance"], CultureInfo.InvariantCulture);
                // Range: 0-100
                if(int.Parse(scrollView.Controller.interactables["TransitionEffectChance"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["TransitionEffectChance"].Tooltip = "0";
                if(int.Parse(scrollView.Controller.interactables["TransitionEffectChance"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    scrollView.Controller.interactables["TransitionEffectChance"].Tooltip = "100";
                SaveData.saveValues["TransitionEffectChance"] = scrollView.Controller.interactables["TransitionEffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionEffectChance"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("TransitionEffects", new Switch("Transition Effects", "Allow transitions to use effects.", new Vector2(139, 60-(8*2)+(19*12)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["TransitionEffects"];
                    SaveData.saveValues["TransitionEffects"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["TransitionEffects"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["TransitionEffects"] == "true"));
            scrollView.Controller.Add("TransitionChance", new TextEntry("Transition Chance", "How often transitions are rolled, from 0-100.", SaveData.saveValues["TransitionChance"], new Vector2(139, 60-(8*2)+(19*11)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionChance"], CultureInfo.InvariantCulture);
                if(int.Parse(scrollView.Controller.interactables["TransitionChance"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["TransitionChance"].Tooltip = "0";
                if(int.Parse(scrollView.Controller.interactables["TransitionChance"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    scrollView.Controller.interactables["TransitionChance"].Tooltip = "100";
                SaveData.saveValues["TransitionChance"] = scrollView.Controller.interactables["TransitionChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionChance"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("EffectChance", new TextEntry("Effect Chance", "How often any effect are used, from 0-100.", SaveData.saveValues["EffectChance"], new Vector2(139, 60-(8*2)+(19*10)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["EffectChance"], CultureInfo.InvariantCulture);
                if(int.Parse(scrollView.Controller.interactables["EffectChance"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["EffectChance"].Tooltip = "0";
                if(int.Parse(scrollView.Controller.interactables["EffectChance"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    scrollView.Controller.interactables["EffectChance"].Tooltip = "100";
                SaveData.saveValues["EffectChance"] = scrollView.Controller.interactables["EffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["EffectChance"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("OverlayChance", new TextEntry("Overlay Chance", "How often overlays are rolled, from 0-100.", SaveData.saveValues["OverlayChance"], new Vector2(139, 60-(8*2)+(19*9)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                int oldValue = int.Parse(SaveData.saveValues["OverlayChance"], CultureInfo.InvariantCulture);
                if(int.Parse(scrollView.Controller.interactables["OverlayChance"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["OverlayChance"].Tooltip = "0";
                if(int.Parse(scrollView.Controller.interactables["OverlayChance"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    scrollView.Controller.interactables["OverlayChance"].Tooltip = "100";
                SaveData.saveValues["OverlayChance"] = scrollView.Controller.interactables["OverlayChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["OverlayChance"], CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("FPS", new TextEntry("Output Frame Rate", "why is a description still being set here?", SaveData.saveValues["VideoFPS"], new Vector2(139, 60-(8*2)+(19*8)+(10*1)+(9*1)), 24, 4, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["VideoFPS"];
                // minimum must be 1
                if(int.Parse(scrollView.Controller.interactables["FPS"].Tooltip, CultureInfo.InvariantCulture) < 1)
                    scrollView.Controller.interactables["FPS"].Tooltip = "1";
                // maximum must be 240
                if(int.Parse(scrollView.Controller.interactables["FPS"].Tooltip, CultureInfo.InvariantCulture) > 240)
                    scrollView.Controller.interactables["FPS"].Tooltip = "240";
                SaveData.saveValues["VideoFPS"] = scrollView.Controller.interactables["FPS"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoFPS"])
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("Height", new TextEntry("Output Resolution", "Height: how tall the result is.", SaveData.saveValues["VideoHeight"], new Vector2(170, 60-(8*2)+(19*7)+(10*1)+(9*1)), 24, 4, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["VideoHeight"];
                // minimum must be 240
                if(int.Parse(scrollView.Controller.interactables["Height"].Tooltip, CultureInfo.InvariantCulture) < 128)
                    scrollView.Controller.interactables["Height"].Tooltip = "128";
                // maximum must be 2160
                if(int.Parse(scrollView.Controller.interactables["Height"].Tooltip, CultureInfo.InvariantCulture) > 2160)
                    scrollView.Controller.interactables["Height"].Tooltip = "2160";
                // height must be a multiple of 2
                if(int.Parse(scrollView.Controller.interactables["Height"].Tooltip, CultureInfo.InvariantCulture) % 2 != 0)
                    scrollView.Controller.interactables["Height"].Tooltip = (int.Parse(scrollView.Controller.interactables["Height"].Tooltip, CultureInfo.InvariantCulture) - 1).ToString(CultureInfo.InvariantCulture);
                SaveData.saveValues["VideoHeight"] = scrollView.Controller.interactables["Height"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoHeight"])
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("Width", new TextEntry("     ", "Width: how wide the result is.", SaveData.saveValues["VideoWidth"], new Vector2(139, 60-(8*2)+(19*7)+(10*1)+(9*1)), 24, 4, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["VideoWidth"];
                // minimum must be 320
                if(int.Parse(scrollView.Controller.interactables["Width"].Tooltip, CultureInfo.InvariantCulture) < 128)
                    scrollView.Controller.interactables["Width"].Tooltip = "128";
                // maximum must be 3840
                if(int.Parse(scrollView.Controller.interactables["Width"].Tooltip, CultureInfo.InvariantCulture) > 3840)
                    scrollView.Controller.interactables["Width"].Tooltip = "3840";
                // width must be a multiple of 2
                if(int.Parse(scrollView.Controller.interactables["Width"].Tooltip, CultureInfo.InvariantCulture) % 2 != 0)
                    scrollView.Controller.interactables["Width"].Tooltip = (int.Parse(scrollView.Controller.interactables["Width"].Tooltip, CultureInfo.InvariantCulture) - 1).ToString(CultureInfo.InvariantCulture);
                SaveData.saveValues["VideoWidth"] = scrollView.Controller.interactables["Width"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoWidth"])
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("AdvancedOptions", new Label("Advanced Options:", new Vector2(139, 60-(8*1)+(19*6)+(10*1)+(9*1))));
            // REGULAR MODE
            // Add text entries
            scrollView.Controller.Add("MaxStreamDuration", new TextEntry("Random Clip Length", "End of random length range.", SaveData.saveValues["MaxStreamDuration"], new Vector2(172, 60-(8*1)+(19*5)+(10*1)+(9*1)), 26, 5, 2, (int i, string n) => {
                string oldValue = SaveData.saveValues["MaxStreamDuration"];
                if(float.Parse(scrollView.Controller.interactables["MaxStreamDuration"].Tooltip, CultureInfo.InvariantCulture) < 0.2)
                    scrollView.Controller.interactables["MaxStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MaxStreamDuration"] = scrollView.Controller.interactables["MaxStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("MinStreamDuration", new TextEntry("  ", "Start of random length range.", SaveData.saveValues["MinStreamDuration"], new Vector2(139, 60-(8*1)+(19*5)+(10*1)+(9*1)), 26, 5, 2, (int i, string n) => {
                string oldValue = SaveData.saveValues["MinStreamDuration"];
                //if(float.Parse(scrollView.Controller.interactables["MinStreamDuration"].Tooltip, CultureInfo.InvariantCulture) < 0.2)
                    //scrollView.Controller.interactables["MinStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MinStreamDuration"] = scrollView.Controller.interactables["MinStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MinStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            scrollView.Controller.Add("ClipCount", new TextEntry("Clip Segment Count", "How many clips to generate.", SaveData.saveValues["MaxClipCount"], new Vector2(139, 60-(8*1)+(19*4)+(10*1)+(9*1)), 24, 3, 1, (int i, string n) => {
                string oldValue = SaveData.saveValues["MaxClipCount"];
                if(int.Parse(scrollView.Controller.interactables["ClipCount"].Tooltip, CultureInfo.InvariantCulture) < 0)
                    scrollView.Controller.interactables["ClipCount"].Tooltip = "0";
                /*
                if(int.Parse(scrollView.Controller.interactables["ClipCount"].Tooltip, CultureInfo.InvariantCulture) > 100)
                    scrollView.Controller.interactables["ClipCount"].Tooltip = "100";
                */
                SaveData.saveValues["MaxClipCount"] = scrollView.Controller.interactables["ClipCount"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxClipCount"])
                    SaveData.Save();
                return false;
            }));
            // Add switches
            scrollView.Controller.Add("InsertOutro", new Switch("Use Outro", "Ends with a random outro.", new Vector2(139, 60-(8*1)+(19*3)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["OutrosEnabled"];
                    SaveData.saveValues["OutrosEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["OutrosEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["OutrosEnabled"] == "true"));
            scrollView.Controller.Add("InsertIntro", new Switch("Use Intro", "Begins with a random intro.", new Vector2(139, 60-(8*1)+(19*2)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["IntrosEnabled"];
                    SaveData.saveValues["IntrosEnabled"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["IntrosEnabled"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["IntrosEnabled"] == "true"));
            scrollView.Controller.Add("SaveToLibrary", new Switch("Play Automatically", "Immediately start playing once complete.", new Vector2(139, 60-(8*1)+(19*1)+(10*1)+(9*1)), (int i, string n) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["PlayAutomatically"];
                    SaveData.saveValues["PlayAutomatically"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["PlayAutomatically"])
                        SaveData.Save();
                }
                return switchState;
            }, SaveData.saveValues["PlayAutomatically"] == "true"));
            scrollView.Controller.Add("CommonOptions", new Label("Common Options:", new Vector2(139, 60-(8*0)+(19*0)+(10*1)+(9*1))));
            scrollView.Controller.Add("StartRendering", new Button("Start Rendering", "Start generating a new video.", new Vector2(219, 60-(8*0)+(19*0)+(10*1)+(9*0)), GenerateButton));
            // Interactable
            scrollView.LoadContent(contentManager, graphicsDevice);
            controllerRendering.LoadContent(contentManager, graphicsDevice);
            actionController.LoadContent(contentManager, graphicsDevice);
            actionControllerRendering.LoadContent(contentManager, graphicsDevice);
            scrollView.MaxScrollOffset = 16*14;
        }
    }
}
