using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Prime.Main;

namespace WarLight.Shared.AI.Prime.Orders
{
    static class AttackingWeight
    {
        public static float Weigh(PrimeBot Bot, TerritoryIDType terrID)
        {
            float weight = 1000f;
            weight -= 25 * Bot.BonusValue(Bot.WhatBonus(terrID));
            weight += 10 * Bot.ArmiesOnTerritory(terrID);
            return weight;
        }
    }
}
