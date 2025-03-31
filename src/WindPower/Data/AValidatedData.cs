using Mafi;
using Mafi.Core.Mods;
using System;

namespace WindPower
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
                Log.Exception(e);
                throw e;
            }
        }

        protected abstract void RegisterDataInternal(ProtoRegistrator registrator);
    }
}