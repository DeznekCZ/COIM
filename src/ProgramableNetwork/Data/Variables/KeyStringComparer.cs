using Mafi.Collections;
using Mafi.Core.Syncers;
using System.Collections.Generic;

namespace ProgramableNetwork.Data.Variables
{
    internal class KeyStringComparer : ICollectionComparator<string, IEnumerable<string>>
    {
        public bool AreSame(IEnumerable<string> collection, Lyst<string> lastKnown)
        {
            if (collection is null && lastKnown is null)
                return true;
            if (collection is null || lastKnown is null)
                return false;
            
            Lyst<string> newList = new Lyst<string>(collection);
            if (newList.Count != lastKnown.Count)
                return false;

            for (int i = 0; i < newList.Count; i++)
                if (newList[i] != lastKnown[i])
                    return false;

            return true;
        }
    }
}