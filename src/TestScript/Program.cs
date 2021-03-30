using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using Mono.Cecil;

namespace TestScript
{
    class Program
    {
        private static readonly string BepInExAssemblyName = Assembly.GetAssembly(typeof(TypeLoader)).GetName().Name;
        
        private static bool HasBepinPlugins(AssemblyDefinition ass)
        {
            if (ass.MainModule.AssemblyReferences.All(r => r.Name != BepInExAssemblyName) 
                || ass.MainModule.GetTypeReferences().All(r => r.FullName != "BepInEx.BaseUnityPlugin"))
            {
                return false;
            }
            return true;
        }

        public static List<string> FindPluginTypes(string directory, Func<AssemblyDefinition, bool> assemblyFilter = null)
        {
            var result = new List<string>();
            var SearchFolders = new List<string>() { directory }; SearchFolders.AddRange(Directory.EnumerateDirectories(directory));
            IEnumerable<string> dlls = SearchFolders.SelectMany(directory => Directory.EnumerateFiles(directory, "*.dll"));

            foreach (string dll in dlls)
            {
                try
                {
                    var ass = AssemblyDefinition.ReadAssembly(dll);
                    if (!assemblyFilter?.Invoke(ass) ?? false)
                    {
                        ass.Dispose();
                        continue;
                    }
                    result.Add(dll);
                    ass.Dispose();
                }
                catch (BadImageFormatException e)
                {
                    //Logger.LogDebug($"Skipping loading {dll} because it's not a valid .NET assembly. Full error: {e.Message}");
                }
                catch (Exception e)
                {
                    //Logger.LogError(e.ToString());
                }
            }
            return result;
        }

        static void ReloadPlugins()
        {
            string ScriptDirectory = @"C:\Users\aleks\AppData\Roaming\r2modmanPlus-local\DysonSphereProgram\profiles\Default\BepInEx\scripts";

            var pluginsToLoad = FindPluginTypes(ScriptDirectory, HasBepinPlugins);

            if (pluginsToLoad.Count > 0)
            {
                foreach (var pathKV in pluginsToLoad)
                {
                    Console.Out.WriteLine(pathKV);
                }
                Console.Out.WriteLine("Reloaded all plugins!");
            }
            else
            {
                Console.Out.WriteLine("No plugins to reload");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ReloadPlugins();
        }
    }
}
