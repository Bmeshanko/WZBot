using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime
{
    public class BonusPath
    {
        public int turnsToTake;
        public List<TerritoryIDType> ourTerrs;
        public BonusIDType Bonus;
        public PrimeBot Bot;

        public BonusPath(BonusIDType bonus, PrimeBot bot, TerritoryIDType terrID)
        {
            this.Bonus = bonus;
            this.Bot = bot;
            this.ourTerrs = bot.OurTerritoriesInBonus(bonus);
            this.ourTerrs.Add(terrID);
        }

        public void Go()
        {
            List<TerritoryIDType> queue = new List<TerritoryIDType>();
            List<TerritoryIDType> visited = new List<TerritoryIDType>();
            List<int> turnsQueue = new List<int>();
            foreach(var terr in ourTerrs) 
            {
                queue.Add(terr);
                visited.Add(terr);
                turnsQueue.Add(0);
            }

            int turnNum = 0;
            // BFS To Determine # of Turns
            while (queue.Count > 0)
            {
                var dequeue = queue.ElementAt(0);
                turnNum = turnsQueue.ElementAt(0);

                queue.RemoveAt(0);
                turnsQueue.RemoveAt(0);

                foreach (var terr in Bot.ConnectedToInBonusNeutral(dequeue))
                {
                    if (visited.Contains(terr)) continue;
                    queue.Add(terr);
                    turnsQueue.Add(turnNum + 1);
                    visited.Add(terr);
                }
            }
            this.turnsToTake = turnNum;
        }
    }
}