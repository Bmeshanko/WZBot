using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarLight.Shared.AI.Prime.Orders;

namespace WarLight.Shared.AI.Prime
{
    public class BonusPath
    {
        public int turnsToTake;
        public int armiesToTake;
        public Dictionary<TerritoryIDType, int> ourTerrs;
        public BonusIDType Bonus;
        public PrimeBot Bot;

        Boolean IsNeutral(TerritoryIDType terrID)
        {
            return Bot.Standing.Territories[terrID].IsNeutral;
        }

        int Leftovers(TerritoryIDType to)
        {
            float defensiveKillRate = (float) Bot.Settings.DefenseKillRate;
            return ExpansionWeight.ArmiesToTake(to, Bot) - (int) (Bot.ArmiesOnTerritory(to) * defensiveKillRate);
        }

        public BonusPath(BonusIDType bonusID, PrimeBot Bot)
        {
            this.Bonus = bonusID;
            this.Bot = Bot;
            this.ourTerrs = new Dictionary<TerritoryIDType, int>();
            var terrsWeOwn = Bot.OurTerritoriesInBonus(bonusID);
            if (terrsWeOwn.Count == 0)
            {
                terrsWeOwn = Bot.OurTerritories()
                    .Where(o => Bot.Map.Territories[o].ConnectedTo.Keys
                    .Any(c => Bot.WhatBonus(c) == Bonus)).ToList();
            }
            foreach (var terr in terrsWeOwn)
            {
                int armies = Bot.ArmiesOnTerritory(terr);
                ourTerrs.Add(terr, armies);
            }
        }

        public void Go()
        {
            Dictionary<TerritoryIDType, int> queue = new Dictionary<TerritoryIDType, int>();
            List<TerritoryIDType> visited = new List<TerritoryIDType>();
            List<int> turnsQueue = new List<int>();
            foreach(var terr in ourTerrs) 
            {
                queue.Add(terr.Key, terr.Value);
                visited.Add(terr.Key);
                turnsQueue.Add(0);
            }

            int turnNum = 0;
            int totalArmies = 0;
            // BFS To Determine # of Turns
            while (queue.Count > 0)
            {
                var dequeue = queue.ElementAt(0);
                queue.Remove(dequeue.Key);
                /*String queueString = "[";
                foreach(var a in turnsQueue)
                {
                    queueString += a + ", ";
                }
                queueString += "]";
                AILog.Log("BonusPath", queueString + " -> " + turnNum + " " + Bot.TerrString(dequeue.Key));*/

                int armies = dequeue.Value;
                // We sort by most to least connections so the extra armies go to the right place
                var connections = Bot.ConnectedToInBonusNeutral(dequeue.Key);
                if (Bot.WhatBonus(dequeue.Key) != Bonus)
                {
                    connections = Bot.Map.Bonuses[Bonus].Territories
                        .Where(o => Bot.Map.Territories[o].ConnectedTo.ContainsKey(dequeue.Key)).ToList();
                }
                connections = connections.OrderBy(o => Bot.ConnectedToInBonus(o).Count).ToList();
                foreach (var terr in connections)
                {
                    if (visited.Contains(terr)) continue;
                    int toDeploy = 1 + ExpansionWeight.ArmiesToTake(terr, Bot) - armies;
                    if (toDeploy >= 0)
                    {
                        totalArmies += toDeploy;
                        queue.Add(terr, Leftovers(terr));
                    }
                    else
                    {
                        queue.Add(terr, Leftovers(terr) - toDeploy); // There will be extras if toDeploy is negative
                    }
                    armies = 1;
                    turnsQueue.Add(turnNum + 1);
                    visited.Add(terr);
                }
                turnNum = turnsQueue.ElementAt(0);
                turnsQueue.RemoveAt(0);
            }
            this.turnsToTake = turnNum;
            this.armiesToTake = totalArmies;
        }
    }
}