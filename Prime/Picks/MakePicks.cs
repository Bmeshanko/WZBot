using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prime.Picks
{
    
    public class MakePicks
    {
        public PrimeBot Bot;
        public MakePicks(PrimeBot bot)
        {
            this.Bot = bot;
        }

        public float GetWeight(PrimeBot bot, TerritoryIDType terrID)
        {
            TerritoryDetails territory = bot.Map.Territories[terrID];
            BonusIDType bonusID = territory.PartOfBonuses.First(); // This will not work with superbonuses.

            int value = bot.BonusValue(bonusID);
            int territories = bot.NumTerritories(bonusID);

            EfficiencyCalculator ec = new EfficiencyCalculator();
            int turnsForBonus = ec.TurnsForBonus(bot, terrID, bonusID);

            float weight = 1000;
            weight *= (float)Math.Pow(((1 / (float)(territories - value))), 4);
            weight *= (float)territories / (float)value;
            weight *= (float)(1 - 0.2 * Math.Pow((turnsForBonus - 1.75), 2));
            

            if (hasWasteland(bot, bonusID, terrID)) weight = 0;

            return weight;
        }

        public bool hasWasteland(PrimeBot bot, BonusIDType bonusID, TerritoryIDType terrID)
        {
            GameStanding distribution = bot.DistributionStanding;
            
            BonusDetails bonus = bot.Map.Bonuses[bonusID];
            var terrsLeft = bonus.Territories.ExceptOne(terrID).ToHashSet(true);

            foreach (var terr in terrsLeft)
            {
                if (distribution.Territories[terr].NumArmies.NumArmies != bot.Settings.InitialNonDistributionArmies)
                    return true;
            }

            return false;
        }
        
        // If Neighboring bonuses have wastelands, then expansion will be more difficult.
        

        public KeyValuePair<TerritoryIDType, float> applyPenalty(PrimeBot bot, float highest, KeyValuePair<TerritoryIDType, float> weight)
        {
            BonusIDType bonus = bot.Map.Territories[weight.Key].PartOfBonuses.First(); // Will not work with superbonuses.

            List<BonusIDType> neighbors = Bot.BonusNeighbors(bonus);
            List<TerritoryIDType> neighboringInDist = bot.DistributionStanding.Territories.Values
                                                        .Where((o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution 
                                                        && neighbors
                                                        .Contains(bot.Map.Territories[o.ID].PartOfBonuses.First())))
                                                        .Select(o => o.ID).ToList();

            float currentWeight = weight.Value;

            foreach(var terr in neighboringInDist)
            {
                float neighborWeight = GetWeight(bot, terr);

                if (neighborWeight == 0)
                {
                    currentWeight *= 0.85f;
                }
                else if (neighborWeight / highest < 0.75)
                {
                    currentWeight *= 0.9f;
                }
            }

            KeyValuePair<TerritoryIDType, float> newWeight = new KeyValuePair<TerritoryIDType, float>(weight.Key, currentWeight);
            return newWeight;
        }

        public List<TerritoryIDType> Commit(PrimeBot bot)
        {
            int maxPicks = bot.Settings.LimitDistributionTerritories == 0 ? bot.Map.Territories.Count : (bot.Settings.LimitDistributionTerritories * bot.Players.Values.Count(o => o.State == GamePlayerState.Playing));
            var allAvailable = bot.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToHashSet(true);

            var expansionWeights = allAvailable.ToDictionary(o => o, o => GetWeight(bot, o));

            var ordered = expansionWeights.OrderByDescending(o => o.Value).ToList();

            foreach (var pair in ordered)
            {
                applyPenalty(bot, 1000, pair);
                AILog.Log("MakePicks", "Picking weight for " + bot.TerrString(pair.Key) + " in " + bot.BonusString(bot.Map.Territories[pair.Key].PartOfBonuses.First()) + ": " + pair.Value);
            }

            return ordered.Select(o => o.Key).Take(maxPicks).ToList();

        }

    }
}
