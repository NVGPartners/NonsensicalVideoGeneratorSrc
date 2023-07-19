using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Generate page.
    /// </summary>
    public class GeneratePage : IPage
    {
        public string Name { get; set; } = "Generate";
        public string Tooltip { get; } = "Render a nonsensical video.";
        private readonly InteractableController controller = new();
        private readonly InteractableController controllerAdvanced = new();
        private readonly InteractableController controllerRendering = new();
        private bool advanced = false;
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(advanced)
            {
                if(controllerAdvanced.Update(gameTime, handleInput))
                    return true;
            }
            else if(Global.generatorFactory.generatorActive)
            {
                if(controllerRendering.Update(gameTime, handleInput))
                    return true;
            }
            else
            {
                if(controller.Update(gameTime, handleInput))
                    return true;
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(advanced)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                controllerAdvanced.Draw(gameTime, spriteBatch);
            }
            else if(Global.generatorFactory.generatorActive)
            {
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                // Draw text to indicate that rendering is in progress
                SpriteFont font = GlobalContent.GetFont("Munro");
                string text = "Rendering is in progress.";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, "Rendering is in progress.", new Vector2(GlobalGraphics.Scale(1) + GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(1) + GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.Black);
                spriteBatch.DrawString(font, "Rendering is in progress.", new Vector2(GlobalGraphics.Scale(135) + (GlobalGraphics.Scale(306) - GlobalGraphics.Scale(135) - textSize.X) / 2, GlobalGraphics.Scale(58) + (GlobalGraphics.Scale(236) - GlobalGraphics.Scale(58) - textSize.Y) / 2), Color.White);
                controllerRendering.Draw(gameTime, spriteBatch);
            }
            else
            {
                controller.Draw(gameTime, spriteBatch);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // RENDERING MODE
            controllerRendering.Add("Cancel", new Button("Cancel", "Stop rendering.", new Vector2(119+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        Global.generatorFactory.CancelGeneration(true);
                        return true;
                }
                return false;
            }));
            controllerRendering.Add("ForceConcatenate", new Button("Combine Clips Now", "Stop rendering and force concatenation.", new Vector2(119+36+104, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        Global.generatorFactory.CancelGeneration(true, true);
                        return true;
                }
                return false;
            }));
            // ADVANCED MODE
            controllerAdvanced.Add("AdvancedLabel", new Label("Advanced Options", new Vector2(144, 64+19*8)));
            controllerAdvanced.Add("BackToRegularOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        advanced = false;
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));                                      
            controllerAdvanced.Add("TransitionEffectChance", new TextEntry("Transition Effect Chance", "How often transitions get effects, from 0-100.", SaveData.saveValues["TransitionEffectChance"], new Vector2(139, 60+19*5), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionEffectChance"]);
                // Range: 0-100
                if(int.Parse(controllerAdvanced.interactables["TransitionEffectChance"].Tooltip) < 0)
                    controllerAdvanced.interactables["TransitionEffectChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["TransitionEffectChance"].Tooltip) > 100)
                    controllerAdvanced.interactables["TransitionEffectChance"].Tooltip = "100";
                SaveData.saveValues["TransitionEffectChance"] = controllerAdvanced.interactables["TransitionEffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionEffectChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("TransitionEffects", new Switch("Transition Effects", "Allow transitions to use effects.", new Vector2(139, 60+19*4), (int i) => {
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
            controllerAdvanced.Add("TransitionChance", new TextEntry("Transition Chance", "How often transitions are rolled, from 0-100.", SaveData.saveValues["TransitionChance"], new Vector2(139, 60+19*3), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionChance"]);
                if(int.Parse(controllerAdvanced.interactables["TransitionChance"].Tooltip) < 0)
                    controllerAdvanced.interactables["TransitionChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["TransitionChance"].Tooltip) > 100)
                    controllerAdvanced.interactables["TransitionChance"].Tooltip = "100";
                SaveData.saveValues["TransitionChance"] = controllerAdvanced.interactables["TransitionChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("EffectChance", new TextEntry("Effect Chance", "How often any effect are used, from 0-100.", SaveData.saveValues["EffectChance"], new Vector2(139, 60+19*2), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["EffectChance"]);
                if(int.Parse(controllerAdvanced.interactables["EffectChance"].Tooltip) < 0)
                    controllerAdvanced.interactables["EffectChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["EffectChance"].Tooltip) > 100)
                    controllerAdvanced.interactables["EffectChance"].Tooltip = "100";
                SaveData.saveValues["EffectChance"] = controllerAdvanced.interactables["EffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["EffectChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("OverlayChance", new TextEntry("Overlay Chance", "How often overlays are rolled, from 0-100.", SaveData.saveValues["OverlayChance"], new Vector2(139, 60+19), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["OverlayChance"]);
                if(int.Parse(controllerAdvanced.interactables["OverlayChance"].Tooltip) < 0)
                    controllerAdvanced.interactables["OverlayChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["OverlayChance"].Tooltip) > 100)
                    controllerAdvanced.interactables["OverlayChance"].Tooltip = "100";
                SaveData.saveValues["OverlayChance"] = controllerAdvanced.interactables["OverlayChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["OverlayChance"]))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Height", new TextEntry("Output Resolution", "Height: how tall the result is.", SaveData.saveValues["VideoHeight"], new Vector2(170, 60), 24, 4, 1, (int i) => {
                string oldValue = SaveData.saveValues["VideoHeight"];
                // minimum must be 240
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip) < 240)
                    controllerAdvanced.interactables["Height"].Tooltip = "240";
                // maximum must be 2160
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip) > 2160)
                    controllerAdvanced.interactables["Height"].Tooltip = "2160";
                // height must be a multiple of 2
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip) % 2 != 0)
                    controllerAdvanced.interactables["Height"].Tooltip = (int.Parse(controllerAdvanced.interactables["Height"].Tooltip) - 1).ToString();
                SaveData.saveValues["VideoHeight"] = controllerAdvanced.interactables["Height"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoHeight"])
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Width", new TextEntry("", "Width: how wide the result is.", SaveData.saveValues["VideoWidth"], new Vector2(139, 60), 24, 4, 1, (int i) => {
                string oldValue = SaveData.saveValues["VideoWidth"];
                // minimum must be 320
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip) < 320)
                    controllerAdvanced.interactables["Width"].Tooltip = "320";
                // maximum must be 3840
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip) > 3840)
                    controllerAdvanced.interactables["Width"].Tooltip = "3840";
                // width must be a multiple of 2
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip) % 2 != 0)
                    controllerAdvanced.interactables["Width"].Tooltip = (int.Parse(controllerAdvanced.interactables["Width"].Tooltip) - 1).ToString();
                SaveData.saveValues["VideoWidth"] = controllerAdvanced.interactables["Width"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoWidth"])
                    SaveData.Save();
                return false;
            }));
            // REGULAR MODE
            // Add buttons
            controller.Add("MoreOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        advanced = true;
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));
            controller.Add("StartRendering", new Button("Start Rendering", "Start generating a new video.", new Vector2(139+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(!Global.canRender)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            return true;
                        }
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        Global.generatorFactory.StartGeneration((sender, e) => {
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
                                            ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                                            SteamUserStats.SetAchievement(achievement);
                                        }
                                    }
                                }
                                catch {}
                                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            }
                            Global.justCompletedRender = true;
                            //SteamUserStats.SetAchievement("RENDER_VIDEO");
                        }, (sender, e) => {});
                        return true;
                }
                return false;
            }));
            // Add text entries
            controller.Add("MaxStreamDuration", new TextEntry("Random Length", "End of random length range.", SaveData.saveValues["MaxStreamDuration"], new Vector2(172, 60+19*3), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MaxStreamDuration"];
                if(float.Parse(controller.interactables["MaxStreamDuration"].Tooltip) < 0.2)
                    controller.interactables["MaxStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MaxStreamDuration"] = controller.interactables["MaxStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MinStreamDuration", new TextEntry("", "Start of random length range.", SaveData.saveValues["MinStreamDuration"], new Vector2(139, 60+19*3), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MinStreamDuration"];
                if(float.Parse(controller.interactables["MinStreamDuration"].Tooltip) < 0.2)
                    controller.interactables["MinStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MinStreamDuration"] = controller.interactables["MinStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MinStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("ClipCount", new TextEntry("Clip Segment Count", "How many clips to generate.", SaveData.saveValues["MaxClipCount"], new Vector2(139, 60+19*2), 24, 3, 1, (int i) => {
                string oldValue = SaveData.saveValues["MaxClipCount"];
                if(int.Parse(controller.interactables["ClipCount"].Tooltip) < 0)
                    controller.interactables["ClipCount"].Tooltip = "0";
                if(int.Parse(controller.interactables["ClipCount"].Tooltip) > 100)
                    controller.interactables["ClipCount"].Tooltip = "100";
                SaveData.saveValues["MaxClipCount"] = controller.interactables["ClipCount"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxClipCount"])
                    SaveData.Save();
                return false;
            }));
            // Add switches
            controller.Add("InsertOutro", new Switch("Insert Outro", "Ends with a random outro.", new Vector2(219, 60+19), (int i) => {
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
            controller.Add("InsertIntro", new Switch("Insert Intro", "Begins with a random intro.", new Vector2(139, 60+19), (int i) => {
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
            controller.Add("SaveToLibrary", new Switch("Play Automatically", "Immediately start playing once complete.", new Vector2(139, 60), (int i) => {
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
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
            controllerAdvanced.LoadContent(contentManager, graphicsDevice);
            controllerRendering.LoadContent(contentManager, graphicsDevice);
        }
    }
}