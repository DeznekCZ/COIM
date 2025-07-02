using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Notifications;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;

namespace ProgramableNetwork.ModuleTester
{
    internal class TestFactory : IComputingConsumerFactory, IElectricityConsumerFactory, IUnityConsumerFactory, IEntityMaintenanceProvidersFactory, INotificationsManager
    {
        public event Action<INotification> NotificationAdded;
        public event Action<INotification> NotificationRemoved;
        public event Action<INotification> NotificationSuppressChanged;

        public NotificationId AddNotification(NotificationProto proto, Option<IObjectWithTitle> withTitle, Option<object> param)
        {
            return NotificationId.Invalid;
        }

        public IComputingConsumer CreateConsumer(IComputingConsumingEntity entity)
        {
            return null;
        }

        public IElectricityConsumer CreateConsumer(IElectricityConsumingEntity entity)
        {
            return null;
        }

        public UnityConsumer CreateConsumer(IUnityConsumingEntity entity)
        {
            return null;
        }

        public IEntityMaintenanceProvider CreateFor(IMaintainedEntity entity)
        {
            return null;
        }

        public Set<INotification> FetchAllNotifications()
        {
            return [];
        }

        public void GetAllActiveNotificationsForInspector(IObjectWithTitle objectWithTitle, Lyst<INotification> result)
        {
            
        }

        public Option<INotification> GetFirstActiveNotificationForInspector(IObjectWithTitle objectWithTitle)
        {
            return Option.None;
        }

        public T GetNotificationProto<T>(Proto.ID id) where T : NotificationProto
        {
            return (T)(object)new EntityNotificationProto(new NotificationProto.ID(id.Value), Proto.Str.Empty, NotificationType.Continuous, NotificationStyle.Warning, Option.None, null, false, false, "".AsLoc());
        }

        public void RemoveAllNotificationFor(IObjectWithTitle objectWithTitle, NotificationProto notificationProto)
        {
            
        }

        public void RemoveNotification(NotificationId notificationId)
        {
            
        }

        public void UnsuppressNotification(NotificationId notificationId)
        {
            
        }
    }
}