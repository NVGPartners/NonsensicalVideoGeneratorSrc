using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif
using Newtonsoft.Json;

namespace NonsensicalVideoGenerator
{
    [Flags]
    public enum Consents
    {
        None = 0,
        DownloadFiles = 1,
        ExecutePrograms = 2,
        AddToLibrary = 4,
        All = DownloadFiles | ExecutePrograms | AddToLibrary
    }
    public class ConsentForm
    {
        // Plugins can ask to install apps, download media, open websites, etc.
        // This class is used to store the consent form data.
        public string name;
        public string pluginName;
        public Consents consents;
        public Dictionary<Consents, List<string>> consentParams = new Dictionary<Consents, List<string>>();
        public string workshopId;
        public string rootPath;
        public ConsentForm(string pluginName, string name, Consents consents, string workshopId, string rootPath, Dictionary<Consents, List<string>>? consentParams = null)
        {
            this.pluginName = pluginName;
            this.name = name;
            this.consents = consents;
            this.rootPath = rootPath;
            this.workshopId = workshopId;
            if(consentParams != null)
                this.consentParams = consentParams;
        }
        public bool CheckConsent(Consents consents)
        {
            return (this.consents & consents) == consents;
        }
        public bool CheckConsentParam(Consents consents, string param)
        {
            if(!CheckConsent(consents))
                return false;
            if(!consentParams.ContainsKey(consents))
                return false;
            return consentParams[consents].Contains(param);
        }
    }
    /// <summary>
    /// This class provides an API for plugins to get user consent.
    /// </summary>
    public static class UserConsent
    {
        public static bool needsConsent = false;
        public static ConsentForm? consentForm = null;
        public static string consentFileName = "UserConsent.json";
        // Returns true if consent is needed or requires updating
        public static bool CheckConsentForm(ConsentForm checkConsentForm)
        {
            // Create consent file if it doesn't exist
            if(!File.Exists(consentFileName))
            {
                File.WriteAllText(consentFileName, JsonConvert.SerializeObject(new List<ConsentForm>(), Formatting.Indented));
            }
            // Check if the user has already accepted the consent form
            try
            {
                List<ConsentForm>? consentForms = JsonConvert.DeserializeObject<List<ConsentForm>>(File.ReadAllText(consentFileName) ?? "[]");
                if(consentForms != null)
                {
                    foreach(ConsentForm consentForm in consentForms)
                    {
                        // Check if pluginName is loaded
                        bool pluginLoaded = false;
                        foreach(Plugin plugin in PluginHandler.plugins)
                        {
                            if(Path.GetFileName(plugin.path) == consentForm.pluginName)
                            {
                                // Plugin is loaded
                                pluginLoaded = true;
                            }
                        }
                        if(!pluginLoaded)
                        {
                            ConsoleOutput.WriteLine("Addon " + consentForm.pluginName + " is not loaded. Removing consent form.", Color.Red);
                            // Remove consent form from json file
                            consentForms.Remove(consentForm);
                            File.WriteAllText(consentFileName, JsonConvert.SerializeObject(consentForms, Formatting.Indented));
                            continue;
                        }
                        if(consentForm.pluginName == checkConsentForm.pluginName)
                        {
                            // Check if the consent form has been updated
                            if(consentForm.name != checkConsentForm.name || consentForm.consents != checkConsentForm.consents || consentForm.workshopId != checkConsentForm.workshopId || consentForm.rootPath != checkConsentForm.rootPath)
                            {
                                // Consent form has been updated
                                needsConsent = true;
                                UserConsent.consentForm = checkConsentForm;
                                // Remove existing consent form
                                consentForms.Remove(consentForm);
                                File.WriteAllText(consentFileName, JsonConvert.SerializeObject(consentForms, Formatting.Indented));
                                return true;
                            }
                            // Consent form has already been accepted
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return CheckConsentForm(checkConsentForm);
            }
            // Consent form has not been accepted
            needsConsent = true;
            UserConsent.consentForm = checkConsentForm;
            return true;
        }
        public static void ShowConsentForm()
        {
            if (consentForm == null)
                return;
            // Show the consent form to the user
            // Create the consent form HTML
            string consentFormHTML = File.ReadAllText("html/index.html");
            consentFormHTML = consentFormHTML.Replace("{{pluginName}}", consentForm.pluginName);
            consentFormHTML = consentFormHTML.Replace("{{name}}", consentForm.name);
            consentFormHTML = consentFormHTML.Replace("{{consents}}", consentForm.consents.ToString());
            consentFormHTML = consentFormHTML.Replace("{{consentParams}}", JsonConvert.SerializeObject(consentForm.consentParams));
            if(consentForm.workshopId != "" && consentForm.rootPath.Contains("workshop"))
                consentFormHTML = consentFormHTML.Replace("{{workshopid}}", consentForm.workshopId);
            else
                consentFormHTML = consentFormHTML.Replace("{{workshopid}}", "");
            // Copy to public_html folder (create if it doesn't exist)
            if(!Directory.Exists("public_html"))
                Directory.CreateDirectory("public_html");
            // Delete old files
            foreach(string file in Directory.GetFiles("public_html"))
            {
                File.Delete(file);
            }
            // Copy all files from html folder to public_html folder
            foreach(string file in Directory.GetFiles("html"))
            {
                File.Copy(file, Path.Combine("public_html", Path.GetFileName(file)));
            }
            // Overrite index.html with the consent form HTML
            File.WriteAllText(Path.Combine("public_html", "index.html"), consentFormHTML);
            // Open the consent form in the user's browser
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "public_html", "index.html");
            psi.UseShellExecute = true;
            Process.Start(psi);
            // Close application
#if MONOGAME
            if(UserInterface.instance != null)
                UserInterface.instance.Exit();
#else
            Environment.Exit(0);
#endif
        }
        public static void Accept(string pluginName)
        {
            // Accept the consent form
            if(consentForm == null)
                return;
            if(consentForm.pluginName != pluginName)
                return;
            // Add consent form to the list of accepted consent forms
            List<ConsentForm>? consentForms = JsonConvert.DeserializeObject<List<ConsentForm>>(File.ReadAllText(consentFileName) ?? "[]");
            if(consentForms != null)
            {
                consentForms.Add(consentForm);
                File.WriteAllText(consentFileName, JsonConvert.SerializeObject(consentForms, Formatting.Indented));
            }
        }
    }
}
