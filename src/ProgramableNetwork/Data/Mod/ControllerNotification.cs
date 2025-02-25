using Mafi;
using Mafi.Core.Gfx;
using Mafi.Core.Mods;
using Mafi.Core.Notifications;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mafi.Unity.Assets.Unity.UserInterface;

namespace ProgramableNetwork.Data.Mod
{
    public class ControllerNotification : AValidatedData
    {
        public static EntityNotificationProto.ID InfoNotification = new EntityNotificationProto.ID("ProgramableNetwork_GeneralNotification");
        public static EntityNotificationProto.ID WarningNotification = new EntityNotificationProto.ID("ProgramableNetwork_WarningNotification");
        public static EntityNotificationProto.ID ErrorNotification = new EntityNotificationProto.ID("ProgramableNetwork_ErrorNotification");

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.NotificationProtoBuilder
                .Start("Controller: Info", InfoNotification)
                .Description("Controller requires action")
                .SetType(NotificationType.Continuous)
                .SetStyle(NotificationStyle.Success)
                .AddEntityIcon(EntityIcons.Broken_png, ColorRgba.Green)
                .AddIcon(EntityIcons.Broken_png)
                .BuildAndAdd();

            registrator.NotificationProtoBuilder
                .Start("Controller: Warning", WarningNotification)
                .Description("Controller may require action or has minor issue")
                .SetType(NotificationType.Continuous)
                .SetStyle(NotificationStyle.Warning)
                .AddEntityIcon(EntityIcons.Broken_png, ColorRgba.Orange)
                .AddIcon(EntityIcons.Broken_png)
                .BuildAndAdd();

            registrator.NotificationProtoBuilder
                .Start("Controller: Error", ErrorNotification)
                .Description("Controller contains issue, which may broke his behaviour")
                .SetType(NotificationType.Continuous)
                .SetStyle(NotificationStyle.Critical)
                .AddEntityIcon(EntityIcons.Broken_png, ColorRgba.Red)
                .AddIcon(EntityIcons.Broken_png)
                .BuildAndAdd();
        }
    }
}
