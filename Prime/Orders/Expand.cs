using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Prime;

namespace WarLight.Shared.AI.Prime.Orders
{
    class Expand
    {
        PrimeBot Bot;
        OrderManager Manager;
        public Expand(PrimeBot bot, OrderManager manager) 
        {
            this.Bot = bot;
            this.Manager = manager;
        }

        public void Go(int armies)
        {
            var potentialTargets = Bot.NeutralBorders();

            Dictionary<TerritoryIDType, float> weights = new Dictionary<TerritoryIDType, float>();
            foreach (var terr in potentialTargets)
            {
                var bonusID = Bot.WhatBonus(terr);
                
                float weight = ExpansionWeight.Weigh(Bot, terr);
                if (weight < 1500)
                {
                    weights.Add(terr, weight);
                }
            }

            foreach (var weight in weights)
            {
                AILog.Log("Expand", "Weight for Territory " + Bot.Map.Territories[weight.Key].Name + ": " + weight.Value);
            }

            while (weights.Count > 0)
            {
                var expandTo = weights.OrderBy(o => o.Value).First().Key;
                weights.Remove(expandTo);

                var connectedTerrs = Bot.OurTerritories().Where(o => Bot.Map.Territories[o].ConnectedTo.ContainsKey(expandTo)).ToList();
                Dictionary<TerritoryIDType, int> potentialDeploys = new Dictionary<TerritoryIDType, int>();
                foreach (var terr in connectedTerrs)
                {
                    if (Manager.DeployTracker.ContainsKey(terr) || Attack.HasFrom(Manager.AttackTracker, terr))
                    {
                        potentialDeploys.Add(terr, 1);
                    } 
                    else
                    {
                        potentialDeploys.Add(terr, Bot.ArmiesOnTerritory(terr));
                    }
                }
                var deployOn = potentialDeploys.OrderByDescending(o => o.Value).First();
                int armiesNeededToCapture = ExpansionWeight.ArmiesToTake(expandTo, Bot);
                int toDeploy = Math.Max(0, 1 + armiesNeededToCapture - deployOn.Value);
                if (toDeploy != 0 && toDeploy <= armies)
                {
                    if (armies - toDeploy < 3)
                    {
                        armiesNeededToCapture += (armies - toDeploy);
                        toDeploy = armies;
                    }
                    if (Manager.DeployTracker.ContainsKey(deployOn.Key))
                    {
                        Manager.DeployTracker[deployOn.Key] += toDeploy;
                    } 
                    else
                    {
                        Manager.DeployTracker.Add(deployOn.Key, toDeploy);
                    }
                    armies -= toDeploy;
                }
                if (toDeploy == 0 || toDeploy <= armies)
                {
                    Manager.AttackTracker.Add(new Attack(deployOn.Key, expandTo, armiesNeededToCapture));
                }
            }
        }
    }
}
