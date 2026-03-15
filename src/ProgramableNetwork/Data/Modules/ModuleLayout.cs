using Mafi;
using Mafi.Unity.UiToolkit.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork
{
    public class ModuleLayout
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">when is not set, the width is calculated by number of inputs or outputs</param>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        public ModuleLayout(ModuleProto proto)
        {
            Inputs = proto.Inputs?.Count ?? 0;
            Outputs = proto.Outputs?.Count ?? 0;
            Displays = proto.Displays == null ? 0 : Sum(proto.Displays.Select(d => d.Width)).IntegerPart;
            Fields = proto.Fields?.Count ?? 0;
            Display = proto.DisplayFunction;
            BaseWidth = proto.BaseWidth;
            DynamicWidth = proto.WidthFunction;
        }

        private static Fix32 Sum(IEnumerable<Fix32> enumerable)
        {
            Fix32 result = Fix32.Zero;
            foreach (Fix32 fix in enumerable) {
                result += fix;
            }
            return result;
        }

        public int Inputs { get; }
        public int Outputs { get; }
        public int Displays { get; }
        public int Fields { get; }
        public int BaseWidth { get; }
        public Action<Module, UiComponent> Display { get; }
        public Func<Module, int> DynamicWidth { get; }

        /// <summary>
        /// Returns with in slots
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public int GetWidth(Module module)
        {
            if (DynamicWidth != null) {
				return DynamicWidth.Invoke(module);
			} else {
				return BaseWidth;
			}
		}
    }
}