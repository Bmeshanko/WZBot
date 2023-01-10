using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Prime.Main;

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
            var potentialTargets = Bot.Standing.Territories.Values
                .Where(o => Bot.Map.Territories[o.ID].ConnectedTo.Keys
                .Any(c => Bot.Standing.Territories[c].OwnerPlayerID == Bot.PlayerID))
                .Where(d => d.IsNeutral).ToList();

            Dictionary<TerritoryIDType, float> weights = new Dictionary<TerritoryIDType, float>();
            foreach (var terr in potentialTargets)
            {
                var terrID = terr.ID;
                var bonusID = Bot.WhatBonus(terrID);
                BonusPath bp = new BonusPath(bonusID, Bot, terrID);
                bp.Go(); // Evaluate turns to take
                float weight = ExpansionWeight.Weigh(Bot, terrID, bonusID, bp.turnsToTake);
                if (weight < 1500)
                {
                    weights.Add(terrID, weight);
                }
            }

            foreach (var weight in weights)
            {
                AILog.Log("Expand", "Weight for Territory " + Bot.Map.Territories[weight.Key].Name + ": " + weight.Value);
            }

            Dictionary<TerritoryIDType, int> deploys = new Dictionary<TerritoryIDType, int>();
            List<TerritoryIDType> attacks = new List<TerritoryIDType>();
            while (weights.Count > 0)
            {
                AILog.Log("Expand", "Armies Remaining: " + armies);
                var expandTo = weights.OrderBy(o => o.Value).First().Key;
                weights.Remove(expandTo);

                var connectedTerrs = Bot.OurTerritories().Where(o => Bot.Map.Territories[o].ConnectedTo.ContainsKey(expandTo)).ToList();
                Dictionary<TerritoryIDType, int> potentialDeploys = new Dictionary<TerritoryIDType, int>();
                foreach (var terr in connectedTerrs)
                {
                    if (deploys.ContainsKey(terr) || attacks.Contains(terr))
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
                    if (deploys.ContainsKey(deployOn.Key))
                    {
                        deploys[deployOn.Key] += toDeploy;
                    } 
                    else
                    {
                        deploys.Add(deployOn.Key, toDeploy);
                    }
                    armies -= toDeploy;
                }
                if (toDeploy == 0 || toDeploy <= armies)
                {
                    attacks.Add(deployOn.Key);
                    var attackOrder = CreateOrders.MakeAttack(Bot, deployOn.Key, expandTo, armiesNeededToCapture);
                    if (attackOrder != null)
                    {
                        Manager.Moves.Add(attackOrder);
                    }
                }
                
            }
            foreach (var deploy in deploys)
            {
                var order = CreateOrders.MakeDeploy(Bot, deploy.Key, deploy.Value);
                if (order != null)
                {
                    Manager.Deploys.Add(order);
                }
            }
        }
    }
}
