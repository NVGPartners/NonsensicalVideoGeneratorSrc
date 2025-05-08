using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace NonsensicalVideoGenerator
{
    public class CustomScriptLoader : ScriptLoaderBase
    {
        // Sandbox location is .\plugins\workshop\%id%\plugin.lua
        public string sandboxLocation = Path.Combine(Directory.GetCurrentDirectory(), "plugins", "workshop");
        public override object LoadFile(string file, Table globalContext)
        {
            if (File.Exists(file))
            {
                return File.ReadAllText(file);
            }
            else
            {
                return "";
            }
        }

        public override bool ScriptFileExists(string name)
        {
            return File.Exists(name);
        }
    }
}