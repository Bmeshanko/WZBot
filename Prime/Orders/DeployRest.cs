using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Prime;

namespace WarLight.Shared.AI.Prime.Orders
{
    class DeployRest
    {
        PrimeBot Bot;
        OrderManager Manager;

        public DeployRest(PrimeBot bot, OrderManager manager)
        {
            Bot = bot;
            Manager = manager;
        }
        
        public void Go(int armies)
        {
            List<TerritoryIDType> potentialDeploys = Bot.EdgeTerritories();
            Dictionary<TerritoryIDType, float> weights = new Dictionary<TerritoryIDType, float>();
            foreach(var terr in potentialDeploys)
            {
                float weight = 0f;
                foreach(var connection in Bot.Map.Territories[terr].ConnectedTo.Keys)
                {
                    if (Bot.NeutralBorders().Contains(connection))
                    {
                        weight += ExpansionWeight.Weigh(Bot, connection);
                    }
                    else if (Bot.EnemyBorders().Contains(connection))
                    {
                        weight += AttackingWeight.Weigh(Bot, connection);
                    }
                }
                weights.Add(terr, weight);
            }

            while (weights.Count > 0)
            {

            }
        }
    }
}
