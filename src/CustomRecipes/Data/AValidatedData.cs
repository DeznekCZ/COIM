using Mafi;
using Mafi.Core.Mods;
using System;

namespace CustomRecipes
{
    public abstract class AValidatedData : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            try
            {
                RegisterDataInternal(registrator);
            }
            catch (System.Exception e)
            {
                Log.Error("CustomExtension: Failed to load custom extension: (edict, recipe, research)");
                Log.Exception(e);
                throw e;
            }
        }

        protected abstract void RegisterDataInternal(ProtoRegistrator registrator);
    }
}