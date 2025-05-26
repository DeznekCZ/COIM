using Mafi;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Syncers;

namespace BucketWheelExcavator.Ui
{
    internal class Tile3fListComparator : ICollectionComparator<Tile3f, IIndexable<Tile3f>>
    {
        public bool AreSame(IIndexable<Tile3f> collection, Lyst<Tile3f> lastKnown)
        {
            if (collection.Count != lastKnown.Count)
                return false;
            for (int i = 0; i < collection.Count; i++)
                if (lastKnown[i] != collection[i])
                    return false;
            return true;
        }
    }
}