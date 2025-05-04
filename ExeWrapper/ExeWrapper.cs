using System;
using System.Reflection;

namespace ExeWrapper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Get assembly from args
            string assemblyPath = "NVGWindowsDX";
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg.ToLower() == "-desktopgl")
                    {
                        assemblyPath = "NVGDesktopGL";
                        break;
                    }
                    else if (arg.ToLower() == "-windowsdx")
                    {
                        assemblyPath = "NVGWindowsDX";
                        break;
                    }
                }
            }
            // Load the assembly
            Assembly nvgAssembly = Assembly.LoadFrom(assemblyPath);

            // NonsensicalVideoGenerator namespace -> Program.Main(args);
            Type? programType = nvgAssembly.GetType("NonsensicalVideoGenerator.Program");
            if (programType != null)
            {
                // Get the Main method
                MethodInfo? mainMethod = programType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (mainMethod != null)
                {
                    // Invoke the Main method with the provided arguments
                    mainMethod.Invoke(null, [args]);
                }
                else
                {
                    Console.WriteLine("Main method not found.");
                }
            }
            else
            {
                Console.WriteLine("Program type not found.");
            }
        }
    }
}
