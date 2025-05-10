using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Xml.Linq;
using Mafi.Localization;
using Mafi;

namespace ProgramableNetwork
{
    public static class IFieldExtensions
    {
        public static Row Row(this UiComponent fieldContainer, IField entityField, Px height)
        {
            Row row = new Row();
            row.Class(Cls.groupHeader);
            row.Height(height);
            fieldContainer.Add(row);

            Label label = new Label();
            label.Value(entityField.Name.AsLoc());
            label.Tooltip(entityField.ShortDesc.AsLoc());
            label.Size(width: 180, height: height);
            row.Add(label);

            return row;
        }

        /// <summary>
        /// With default height Sizes.BLOCK_SIZE
        /// </summary>
        /// <param name="fieldContainer"></param>
        /// <param name="entityField"></param>
        /// <returns></returns>
        public static Row Row(this UiComponent fieldContainer, IField entityField)
        {
            return fieldContainer.Row(entityField, Sizes.BLOCK_SIZE);
        }
    }
}