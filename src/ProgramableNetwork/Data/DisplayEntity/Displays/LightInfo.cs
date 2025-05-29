using Mafi;
using UnityEngine;

namespace ProgramableNetwork.Data.DisplayEntity.Displays
{
    public struct LightInfo
    {
        public Color on;
        public Color off;
        public ColorRgba icon;

        public LightInfo(Color on, Color off, ColorRgba icon)
        {
            this.on = on;
            this.off = off;
            this.icon = icon;
        }

        public override bool Equals(object obj)
        {
            return obj is LightInfo other &&
                   on.Equals(other.on) &&
                   off.Equals(other.off) &&
                   icon.Equals(other.icon);
        }

        public override int GetHashCode()
        {
            int hashCode = -246361944;
            hashCode = hashCode * -1521134295 + on.GetHashCode();
            hashCode = hashCode * -1521134295 + off.GetHashCode();
            hashCode = hashCode * -1521134295 + icon.GetHashCode();
            return hashCode;
        }
    }
}