#if MONOGAME
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
        private readonly InteractableController actionController = new();
        private readonly InteractableController controller = new();
        private readonly InteractableController controllerAdvanced = new();
        private readonly InteractableController controllerRendering = new();
        private readonly InteractableController controllerPage3 = new();
        private int page = 0;
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(actionController.Update(gameTime, handleInput))
                return true;
            if(Global.generator.generatorActive)
            {
                if(controllerRendering.Update(gameTime, handleInput))
                    return true;
            }
            else
            {
                switch(page)
                {
                    case 0:
                        if(controller.Update(gameTime, handleInput))
                            return true;
                        break;
                    case 1:
                        if(controllerAdvanced.Update(gameTime, handleInput))
                            return true;
                        break;
                    case 2:
                        if(controllerPage3.Update(gameTime, handleInput))
                            return true;
                        break;
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(Global.generator.generatorActive)
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
                if(page > 0)
                {
                    spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(137), GlobalGraphics.Scale(56), GlobalGraphics.Scale(167-1), GlobalGraphics.Scale(180)), new Color(0, 0, 0, 96));
                    spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(136), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                    spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(304-1), GlobalGraphics.Scale(57), GlobalGraphics.Scale(1), GlobalGraphics.Scale(179)), new Color(0, 0, 0, 96));
                    spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(135), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                    spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale(305-1), GlobalGraphics.Scale(58), GlobalGraphics.Scale(1), GlobalGraphics.Scale(178)), new Color(0, 0, 0, 96));
                }
                switch(page)
                {
                    case 0:
                        controller.Draw(gameTime, spriteBatch);
                        break;
                    case 1:
                        controllerAdvanced.Draw(gameTime, spriteBatch);
                        break;
                    case 2:
                        controllerPage3.Draw(gameTime, spriteBatch);
                        break;
                }
            }
            actionController.Draw(gameTime, spriteBatch);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Actions
            actionController.Add("ActionRender", new ActionButton("Start generating a new video.", new Vector2(113, 137), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(!Global.canRender || Global.generator.generatorActive)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                            ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                                            SteamUserStats.SetAchievement(achievement);
                                        }
                                    }
                                }
                                catch {}
                                SaveData.saveValues["TotalVideosRendered"] = (int.Parse(SaveData.saveValues["TotalVideosRendered"], System.Globalization.CultureInfo.InvariantCulture) + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                SaveData.Save();
                                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            Global.justCompletedRender = true;
                            //SteamUserStats.SetAchievement("RENDER_VIDEO");
                        }, (sender, e) => {});
                        return true;
                }
                return false;
            }, contentManager.Load<Texture2D>("graphics/actions/render")));
            // PAGE 3
            controllerPage3.Add("DisableClipsAfterMaxUniqueClips", new Switch("Disable Clips After Max Reached", "Disable clips after they reach the max unique clip count.", new Vector2(139, 60+19*2), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["DisableClipsAfterMaxUniqueClips"];
                    SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["DisableClipsAfterMaxUniqueClips"])
                    {
                        SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = "false";
                        (controllerPage3.interactables["DeleteClipsAfterMaxUniqueClips"] as Switch).SwitchState = false;
                        SaveData.Save();
                    }
                }
                return switchState;
            }, SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] == "true"));
            controllerPage3.Add("DeleteClipsAfterMaxUniqueClips", new Switch("Delete Clips After Max Reached", "Delete clips after they reach the max unique clip count.", new Vector2(139, 60+19), (int i) => {
                bool switchState = (i & 256) != 0;
                if((i & 2) != 0)
                {
                    string oldValue = SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"];
                    SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] = switchState.ToString().ToLower();
                    if(oldValue != SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"])
                    {
                        SaveData.saveValues["DisableClipsAfterMaxUniqueClips"] = "false";
                        (controllerPage3.interactables["DisableClipsAfterMaxUniqueClips"] as Switch).SwitchState = false;
                        SaveData.Save();
                    }
                }
                return switchState;
            }, SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"] == "true"));
            controllerPage3.Add("MaxUniqueClips", new TextEntry("Max Unique Media", "The max times a unique media file can be used.", SaveData.saveValues["MaxUniqueClips"], new Vector2(139, 60), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["MaxUniqueClips"], System.Globalization.CultureInfo.InvariantCulture);
                // Range: 0-100
                if(int.Parse(controllerPage3.interactables["MaxUniqueClips"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controllerPage3.interactables["MaxUniqueClips"].Tooltip = "0";
                SaveData.saveValues["MaxUniqueClips"] = controllerPage3.interactables["MaxUniqueClips"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["MaxUniqueClips"], System.Globalization.CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            controllerPage3.Add("Page3Label", new Label("Page 3", new Vector2(144, 64+19*8)));
            controllerPage3.Add("PrevPage", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        page = 0;
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));                  
            // RENDERING MODE
            controllerRendering.Add("Cancel", new Button("Cancel", "Stop rendering.", new Vector2(119+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        Global.generator.CancelGeneration(true);
                        return true;
                }
                return false;
            }));
            controllerRendering.Add("ForceConcatenate", new Button("Combine Clips Now", "Stop rendering and force concatenation.", new Vector2(119+36+104, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        Global.generator.CancelGeneration(true, true);
                        return true;
                }
                return false;
            }));
            // ADVANCED MODE
            controllerAdvanced.Add("AdvancedLabel", new Label("Page 2", new Vector2(144, 64+19*8)));
            controllerAdvanced.Add("BackToRegularOptions", new Button("Next Page", "Next page of options.", new Vector2(239+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        page++;
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));
            controllerAdvanced.Add("PlayOverlayInFull", new Switch("Overlays Play in Full", "Play overlays at their full length.", new Vector2(139, 60+19*6), (int i) => {
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
            controllerAdvanced.Add("TransitionEffectChance", new TextEntry("Transition Effect Chance", "How often transitions get effects, from 0-100.", SaveData.saveValues["TransitionEffectChance"], new Vector2(139, 60+19*5), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["TransitionEffectChance"], System.Globalization.CultureInfo.InvariantCulture);
                // Range: 0-100
                if(int.Parse(controllerAdvanced.interactables["TransitionEffectChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controllerAdvanced.interactables["TransitionEffectChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["TransitionEffectChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controllerAdvanced.interactables["TransitionEffectChance"].Tooltip = "100";
                SaveData.saveValues["TransitionEffectChance"] = controllerAdvanced.interactables["TransitionEffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionEffectChance"], System.Globalization.CultureInfo.InvariantCulture))
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
                int oldValue = int.Parse(SaveData.saveValues["TransitionChance"], System.Globalization.CultureInfo.InvariantCulture);
                if(int.Parse(controllerAdvanced.interactables["TransitionChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controllerAdvanced.interactables["TransitionChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["TransitionChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controllerAdvanced.interactables["TransitionChance"].Tooltip = "100";
                SaveData.saveValues["TransitionChance"] = controllerAdvanced.interactables["TransitionChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["TransitionChance"], System.Globalization.CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("EffectChance", new TextEntry("Effect Chance", "How often any effect are used, from 0-100.", SaveData.saveValues["EffectChance"], new Vector2(139, 60+19*2), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["EffectChance"], System.Globalization.CultureInfo.InvariantCulture);
                if(int.Parse(controllerAdvanced.interactables["EffectChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controllerAdvanced.interactables["EffectChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["EffectChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controllerAdvanced.interactables["EffectChance"].Tooltip = "100";
                SaveData.saveValues["EffectChance"] = controllerAdvanced.interactables["EffectChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["EffectChance"], System.Globalization.CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("OverlayChance", new TextEntry("Overlay Chance", "How often overlays are rolled, from 0-100.", SaveData.saveValues["OverlayChance"], new Vector2(139, 60+19), 24, 3, 1, (int i) => {
                int oldValue = int.Parse(SaveData.saveValues["OverlayChance"], System.Globalization.CultureInfo.InvariantCulture);
                if(int.Parse(controllerAdvanced.interactables["OverlayChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controllerAdvanced.interactables["OverlayChance"].Tooltip = "0";
                if(int.Parse(controllerAdvanced.interactables["OverlayChance"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controllerAdvanced.interactables["OverlayChance"].Tooltip = "100";
                SaveData.saveValues["OverlayChance"] = controllerAdvanced.interactables["OverlayChance"].Tooltip;
                if(oldValue != int.Parse(SaveData.saveValues["OverlayChance"], System.Globalization.CultureInfo.InvariantCulture))
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Height", new TextEntry("Output Resolution", "Height: how tall the result is.", SaveData.saveValues["VideoHeight"], new Vector2(170, 60), 24, 4, 1, (int i) => {
                string oldValue = SaveData.saveValues["VideoHeight"];
                // minimum must be 240
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 128)
                    controllerAdvanced.interactables["Height"].Tooltip = "128";
                // maximum must be 2160
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 2160)
                    controllerAdvanced.interactables["Height"].Tooltip = "2160";
                // height must be a multiple of 2
                if(int.Parse(controllerAdvanced.interactables["Height"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) % 2 != 0)
                    controllerAdvanced.interactables["Height"].Tooltip = (int.Parse(controllerAdvanced.interactables["Height"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                SaveData.saveValues["VideoHeight"] = controllerAdvanced.interactables["Height"].Tooltip;
                if(oldValue != SaveData.saveValues["VideoHeight"])
                    SaveData.Save();
                return false;
            }));
            controllerAdvanced.Add("Width", new TextEntry("     ", "Width: how wide the result is.", SaveData.saveValues["VideoWidth"], new Vector2(139, 60), 24, 4, 1, (int i) => {
                string oldValue = SaveData.saveValues["VideoWidth"];
                // minimum must be 320
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 128)
                    controllerAdvanced.interactables["Width"].Tooltip = "128";
                // maximum must be 3840
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 3840)
                    controllerAdvanced.interactables["Width"].Tooltip = "3840";
                // width must be a multiple of 2
                if(int.Parse(controllerAdvanced.interactables["Width"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) % 2 != 0)
                    controllerAdvanced.interactables["Width"].Tooltip = (int.Parse(controllerAdvanced.interactables["Width"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
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
                        page++;
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        return true;
                }
                return false;
            }));
            controller.Add("StartRendering", new Button("Start Rendering", "Start generating a new video.", new Vector2(139+36, 60+10+19*8), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(!Global.canRender || Global.generator.generatorActive)
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                            ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                                            SteamUserStats.SetAchievement(achievement);
                                        }
                                    }
                                }
                                catch {}
                                SaveData.saveValues["TotalVideosRendered"] = (int.Parse(SaveData.saveValues["TotalVideosRendered"], System.Globalization.CultureInfo.InvariantCulture) + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                SaveData.Save();
                                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            Global.justCompletedRender = true;
                            //SteamUserStats.SetAchievement("RENDER_VIDEO");
                        }, (sender, e) => {});
                        return true;
                }
                return false;
            }));
            // Add text entries
            controller.Add("MaxStreamDuration", new TextEntry("Random Clip Length", "End of random length range.", SaveData.saveValues["MaxStreamDuration"], new Vector2(172, 60+19*3), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MaxStreamDuration"];
                if(float.Parse(controller.interactables["MaxStreamDuration"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0.2)
                    controller.interactables["MaxStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MaxStreamDuration"] = controller.interactables["MaxStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MaxStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("MinStreamDuration", new TextEntry("  ", "Start of random length range.", SaveData.saveValues["MinStreamDuration"], new Vector2(139, 60+19*3), 26, 5, 2, (int i) => {
                string oldValue = SaveData.saveValues["MinStreamDuration"];
                //if(float.Parse(controller.interactables["MinStreamDuration"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0.2)
                    //controller.interactables["MinStreamDuration"].Tooltip = "0.2";
                SaveData.saveValues["MinStreamDuration"] = controller.interactables["MinStreamDuration"].Tooltip;
                if(oldValue != SaveData.saveValues["MinStreamDuration"])
                    SaveData.Save();
                return false;
            }));
            controller.Add("ClipCount", new TextEntry("Clip Segment Count", "How many clips to generate.", SaveData.saveValues["MaxClipCount"], new Vector2(139, 60+19*2), 24, 3, 1, (int i) => {
                string oldValue = SaveData.saveValues["MaxClipCount"];
                if(int.Parse(controller.interactables["ClipCount"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) < 0)
                    controller.interactables["ClipCount"].Tooltip = "0";
                /*
                if(int.Parse(controller.interactables["ClipCount"].Tooltip, System.Globalization.CultureInfo.InvariantCulture) > 100)
                    controller.interactables["ClipCount"].Tooltip = "100";
                */
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
            controllerPage3.LoadContent(contentManager, graphicsDevice);
            actionController.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
