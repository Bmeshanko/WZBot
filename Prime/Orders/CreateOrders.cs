using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    public static class CreateOrders
    {
        public static GameOrderDeploy MakeDeploy(Main.PrimeBot Bot, TerritoryIDType terrID, int amount)
        {
            // if (OrderManager.Deploys.Any(o => o.) If we already deployed here, return null
            AILog.Log("MakeDeploy", "Deploying " + amount + " to " + Bot.Map.Territories[terrID].Name);
            return GameOrderDeploy.Create(Bot.PlayerID, amount, terrID, false);
        }

        public static GameOrderAttackTransfer MakeAttack(Main.PrimeBot Bot, TerritoryIDType from, TerritoryIDType to, int amount)
        {
            AILog.Log("MakeAttack", "Attacking with " + amount + " from " + Bot.Map.Territories[from].Name + " to " + Bot.Map.Territories[to].Name);
            return GameOrderAttackTransfer.Create(Bot.PlayerID, from, to, AttackTransferEnum.AttackTransfer, false, new Armies(amount), false);
        }
    }
}