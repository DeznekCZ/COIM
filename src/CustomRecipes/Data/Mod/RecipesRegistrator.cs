using Mafi;
using Mafi.Core.Mods;
using CustomRecipes.Python;
using System.Collections.Generic;
using System.IO;
using CustomRecipes.ModuleParser.Registrator;

namespace CustomRecipes.Data.Mod
{
    internal class RecipesRegistrator : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            DirectoryInfo modules = new DirectoryInfo(typeof(RecipesRegistrator).Assembly.Location + "/../Recipes");
            Log.Info("Location of modules: " + modules.FullName);

            List<Class> allTemplates = new List<Class>();

            int failed = 0;
            foreach (FileInfo file in modules.EnumerateFiles())
            {
                try
                {
                    RecipeRegistrator.Register(registrator, file.FullName);
                }
                catch (System.Exception e)
                {
                    Log.Error("Parsing of python module failed: " + file.Name);
                    Log.Exception(e);
                    failed++;
                }
            }

            // TOTO register technologies later

            if (failed > 0)
            {
                throw new CheckException("Modules was not loaded, see log: " + failed);
            }
        }
    }
}
