using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    static class PositionalWeight
    {
        public static float Weigh(PrimeBot Bot, TerritoryIDType terrID)
        {
            List<float> attackingWeights = new List<float>();
            List<float> expansionWeights = new List<float>();
            foreach(var terr in Bot.Map.Territories[terrID].ConnectedTo.Keys)
            {
                if (Bot.EnemyBorders().Contains(terr))
                {
                    attackingWeights.Add(AttackingWeight.Weigh(Bot, terr));
                }
                if (Bot.NeutralBorders().Contains(terr))
                {
                    expansionWeights.Add(ExpansionWeight.Weigh(Bot, terr));
                }
                attackingWeights.Sort();
                expansionWeights.Sort();
                if (attackingWeights.Count > 1)
                {
                    attackingWeights.Remove(attackingWeights.Count - 1);
                }
                if (expansionWeights.Count > 1)
                {
                    expansionWeights.Remove(expansionWeights.Count - 1);
                }
            }
            if (attackingWeights.Count > 0 && expansionWeights.Count > 0)
            {
                return attackingWeights.Average() + expansionWeights.Average();
            }
            if (expansionWeights.Count > 0)
            {
                return expansionWeights.Average() + 1000f;
            }
            if (attackingWeights.Count > 0)
            {
                return attackingWeights.Average() + 1000f;
            }
            return 2500f;
        }
    }
}
