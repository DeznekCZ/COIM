using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafi.Core.Syncers
{
    public static class UpdaterRegistrarSelfExtensions
    {
        [MustUseReturnValue]
        public static TriggerBuilder<T> Observe<T, O>(this O reg, Func<O, T> provider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => provider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<T> ObserveNoCompare<T, O>(this O reg, Func<O, T> provider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => provider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> Observe<T, O>(this O reg, Func<O, IEnumerable<T>> collectionProvider, ICollectionComparator<T, IEnumerable<T>> comparator)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => collectionProvider((O)reg), comparator);
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> Observe<T, O>(this O reg, Func<O, IReadOnlyCollection<T>> collectionProvider, ICollectionComparator<T, IReadOnlyCollection<T>> comparator)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => collectionProvider((O)reg), comparator);
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> Observe<T, O>(this O reg, Func<O, IIndexable<T>> collectionProvider, ICollectionComparator<T, IIndexable<T>> comparator)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => collectionProvider((O)reg), comparator);
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> ObserveIndexable<T, O>(this O reg, Func<O, IIndexable<T>> collectionProvider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.ObserveIndexable(reg, () => collectionProvider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> ObserveEnumerable<T, O>(this O reg, Func<O, IEnumerable<T>> collectionProvider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.ObserveEnumerable(reg, () => collectionProvider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> ObserveImmutable<T, O>(this O reg, Func<O, ImmutableArray<T>> collectionProvider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.ObserveImmutable(reg, () => collectionProvider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> ObserveArraySlice<T, O>(this O reg, Func<O, ReadOnlyArraySlice<T>> collectionProvider)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.ObserveArraySlice(reg, () => collectionProvider((O)reg));
        }

        [MustUseReturnValue]
        public static TriggerBuilder<Lyst<T>> Observe<T, O>(this O reg, Func<O, ImmutableArray<T>> collectionProvider, ICollectionComparator<T, ImmutableArray<T>> comparator)
            where O : IUpdaterRegistrar
        {
            return UpdaterRegistrarExtensions.Observe(reg, () => collectionProvider((O)reg), comparator);
        }

        //[MustUseReturnValue]
        //public static TriggerBuilder<Lyst<T>> Observe<T, O>(this O reg, Func<O, ReadOnlyArraySlice<T>> collectionProvider, ICollectionComparator<T, ReadOnlyArraySlice<T>> comparator)
        //    where O : IUpdaterRegistrar
        //{
        //    return UpdaterRegistrarExtensions.Observe(reg, () => collectionProvider((O)reg), comparator);
        //}

        public static void DoOnSyncPeriodically<O>(this O reg, Action<O> action, Duration? intervalMaybe = null)
            where O : IUpdaterRegistrar
        {
            UpdaterRegistrarExtensions.DoOnSyncPeriodically(reg, () => action((O)reg), intervalMaybe);
        }

        public static T With<T>(this T t, Action<T> action)
        {
            action(t);
            return t;
        }
    }
}
