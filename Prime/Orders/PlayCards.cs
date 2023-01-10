using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    static class PlayCards
    {
        public static void Go(Main.PrimeBot Bot, OrderManager Manager)
        {
            int mustPlay = Bot.CardsMustPlay;
            List<CardInstance> cards = Bot.Cards;
            foreach(var card in cards) 
            {
                if (card.CardID == CardType.Reinforcement.CardID)
                {
                    var numArmies = card.As<ReinforcementCardInstance>().Armies;
                    AILog.Log("PlayCards", "Playing reinforcement card for " + numArmies);
                    Manager.Cards.Add(GameOrderPlayCardReinforcement.Create(card.ID, Bot.PlayerID));

                }
            }
        }
    }
}
