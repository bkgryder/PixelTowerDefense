using System.Collections.Generic;
using System.Linq;

namespace PixelTowerDefense.Entities
{
    public static class MeepleExtensions
    {
        public static IEnumerable<Meeple> Combatants(this IEnumerable<Meeple> source)
            => source.Where(m => m.Combatant != null);
    }
}
