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
        public static RowContainer Row(this UiComponent fieldContainer, IField entityField, Px height)
        {
            PanelRow row = new PanelRow();
            fieldContainer.Add(row);

            Label label = new Label();
            label.Value(entityField.Name.AsLoc());
            label.Tooltip(entityField.ShortDesc.AsLoc());
            label.Width(180);
            row.BodyAdd(label);

            return row;
        }

        /// <summary>
        /// With default height Sizes.BLOCK_SIZE
        /// </summary>
        /// <param name="fieldContainer"></param>
        /// <param name="entityField"></param>
        /// <returns></returns>
        public static RowContainer Row(this UiComponent fieldContainer, IField entityField)
        {
            return fieldContainer.Row(entityField, Sizes.BLOCK_SIZE);
        }
    }
}