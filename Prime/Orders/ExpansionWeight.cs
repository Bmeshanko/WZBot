using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    static class ExpansionWeight
    {
        public static int ArmiesToTake(int armiesOnTerritory, float offensiveKillRate)
        {
            if (armiesOnTerritory == 0) return 1;
            float defensiveArmies = armiesOnTerritory;
            float armies = 2f;
            while (true)
            {
                if (armies * offensiveKillRate >= (armiesOnTerritory - 0.5))
                {
                    return (int) armies;
                }
                armies++;
            }
        }

        public static int ArmiesToTake(TerritoryIDType terrID, PrimeBot bot)
        {
            int armiesOnTerritory = bot.ArmiesOnTerritory(terrID);
            return ArmiesToTake(armiesOnTerritory, (float) bot.Settings.OffenseKillRate);
        }

        public static int BonusCompletionWeight(PrimeBot Bot, BonusIDType bonusID) 
        {
            int numArmies = 0;
            int numTerritories = 0;
            foreach(TerritoryStanding terr in Bot.Standing.Territories.Values.Where(o => Bot.Map.Bonuses[bonusID].Territories.Contains(o.ID)))
            {
                if (terr.OwnerPlayerID == Bot.PlayerID)
                {
                    numTerritories++;
                    numArmies += terr.NumArmies.NumArmies;
                }
            }
            return numTerritories * 3 + numArmies;
        }

        public static float Weigh(PrimeBot Bot, TerritoryIDType terrID)
        {
            float weight = 1000f;
            BonusIDType bonusID = Bot.WhatBonus(terrID);
            BonusPath bp = new BonusPath(bonusID, Bot);
            bp.Go(); // Evaluate turns to take
            int turnsToTake = bp.turnsToTake;

            if (Bot.BonusValue(bonusID) == 0)
            {
                weight += 1000;
            }

            weight -= (Bot.BonusValue(bonusID) * 10);
            weight += (Bot.NumTerritories(bonusID) * 20);
            weight += (turnsToTake * 30);
            weight += Bot.Standing.Territories[terrID].NumArmies.NumArmies;
            weight -= Bot.NumberOfTurns * 10;
            weight -= BonusCompletionWeight(Bot, bonusID) * 10;
            weight -= (Bot.OurIncome() - bp.armiesToTake) * 50;
            weight -= Bot.ConnectedToInBonus(terrID).Count;
            if (Bot.Wastelands().Contains(bonusID)) weight += 800;
            return weight;
        }
    }
}
