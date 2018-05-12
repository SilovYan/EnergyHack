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
    }
}