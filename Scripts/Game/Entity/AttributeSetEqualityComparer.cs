using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Game.Attributes;

namespace Game
{  
    public class AttributeSetEqualityComparer : IEqualityComparer<HashSet<Attribute>>
    {
        public bool Equals(HashSet<Attribute> x, HashSet<Attribute> y)
        {
            // If the sets are the same object or both are null, they're equal
            if (ReferenceEquals(x, y)) return true;

            // If one is null and the other is not, they're not equal
            if (x == null || y == null) return false;

            // If the counts are different, the sets are not equal
            if (x.Count != y.Count) return false;

            // If all items in the first set are also in the second set, the sets are equal
            return !x.Except(y).Any();
        }

        public int GetHashCode(HashSet<Attribute> set)
        {
            // Use a prime number to create a unique hash code
            const int seedValue = 487;
            const int primeNumber = 31;

            return set.Aggregate(seedValue, (acc, attribute) => acc * primeNumber + attribute.GetHashCode());
        }
    }
}
