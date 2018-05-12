using System.Collections.Generic;
using System.Linq;

namespace EnergyHack
{
    public static class Extentions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e)
        {
            return e == null || !e.Any();
        }

        public static bool Equal<T>(this ICollection<T> collectA, ICollection<T> collectB)
        {
            if (Equals(collectA, collectB))
                return true;
            if (collectA == null || collectB == null)
                return false;
            if (collectA.Count != collectB.Count)
                return false;
            for (var i = 0; i < collectA.Count; i++)
            {
                if (!Equals(collectA.ElementAt(i), collectB.ElementAt(i)))
                    return false;
            }

            return true;
        }
    }
}