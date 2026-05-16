using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public static class ProtoPickerOptionButtonExtensions {
        public static ProtoPickerOptionButton AsProtoPickerOptionButton(this Button button)
            => new ProtoPickerOptionButton(button);
    }
}