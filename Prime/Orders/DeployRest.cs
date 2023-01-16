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
                weights.Add(terr, PositionalWeight.Weigh(Bot, terr));
            }

            while (weights.Count > 0 && armies > 0)
            {
                var deployOn = weights.OrderBy(o => o.Value).First().Key;
                weights.Remove(deployOn);

                int toDeploy = 0;
                
                if (armies <= 2) toDeploy = armies;
                else if (armies < 4) toDeploy = 2;
                else toDeploy = armies - 2;

                if (Manager.DeployTracker.ContainsKey(deployOn)) Manager.DeployTracker[deployOn] += toDeploy;
                else Manager.DeployTracker.Add(deployOn, toDeploy);

                if (Attack.HasFrom(Manager.AttackTracker, deployOn))
                    Manager.AttackTracker.Where(o => o.from.Equals(deployOn)).First().AddNumber(toDeploy);

                armies -= toDeploy;
            }
        }
    }
}
