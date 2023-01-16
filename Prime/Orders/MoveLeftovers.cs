using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Prod;

namespace WarLight.Shared.AI.Prime.Orders
{
    class MoveLeftovers
    {
        PrimeBot Bot;
        OrderManager Manager;
        public MoveLeftovers(PrimeBot bot, OrderManager manager)
        {
            this.Bot = bot;
            this.Manager = manager;
        }

        public Dictionary<TerritoryIDType, int> CalculateAvailableArmies()
        {
            Dictionary<TerritoryIDType, int> availableArmies = new Dictionary<TerritoryIDType, int>();
            // Armies on The Territory
            foreach (var terr in Bot.OurTerritories())
            {
                availableArmies.Add(terr, Bot.Standing.Territories[terr].NumArmies.NumArmies - 1); // -1 For One Army Stands Guard
            }

            // Add Deploys
            foreach (var deploy in Manager.DeployTracker)
            {
                availableArmies[deploy.Key] += deploy.Value;
            }

            // Subtract Attacks
            foreach (var attack in Manager.AttackTracker)
            {
                availableArmies[attack.from] -= attack.number;
            }
            return availableArmies.Where(o => o.Value > 0).ToDictionary(k => k.Key, v => v.Value);
        }

        public void Go()
        {
            var availableArmies = CalculateAvailableArmies();
            Dictionary<TerritoryIDType, float> weights = new Dictionary<TerritoryIDType, float>();
            foreach(var terr in availableArmies.Keys)
            {
                weights.Add(terr, PositionalWeight.Weigh(Bot, terr));
            }

            // We always want to move the armies on the territory to the highest weight.
            foreach(var w in weights)
            {
                AILog.Log("MoveLeftovers", "Territory: " + Bot.TerrString(w.Key));
                Dictionary<TerritoryIDType, float> neighbors = new Dictionary<TerritoryIDType, float>();
                foreach(var terr in Bot.Map.Territories[w.Key].ConnectedTo.Keys)
                {
                    if (weights.ContainsKey(terr)) neighbors.Add(terr, weights[terr]);
                }
                neighbors = neighbors.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                if (neighbors.First().Key == w.Key) continue;
                if (Attack.HasFrom(Manager.AttackTracker, w.Key))
                {
                    Manager.AttackTracker.Where(o => o.from.Equals(w.Key)).First().AddNumber(availableArmies[w.Key]);
                }
                else
                {
                    Manager.AttackTracker.Add(new Attack(w.Key, neighbors.First().Key, availableArmies[w.Key]));
                }
            }
        }
    }
}
