using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    public class OrderManager
    {
        public Main.PrimeBot Bot;
        public PlayerIncomeTracker IncomeTracker;
        public List<GameOrder> Cards;
        public List<GameOrder> Deploys;
        public List<GameOrder> Moves;
        public List<BonusIDType> BonusesCompleted;

        public OrderManager(Main.PrimeBot bot)
        {
            this.Bot = bot;
            this.IncomeTracker = new PlayerIncomeTracker(Bot.Incomes[Bot.PlayerID], Bot.Map);
            this.Cards = new List<GameOrder>();
            this.Moves = new List<GameOrder>();
            this.Deploys = new List<GameOrder>();
        }

        public void Go()
        {
            PlayCards.Go(Bot, this);

            AttackDefend ad = new AttackDefend(Bot, this);
            ad.Go(IncomeTracker.FreeArmiesUndeployed + Bot.armiesFromReinforcementCards);

            int deploysRemaining = CalculateArmiesRemaining();

            Expand expand = new Expand(Bot, this);
            expand.Go(deploysRemaining);
        }

        public int CalculateArmiesRemaining()
        {
            int start = IncomeTracker.FreeArmiesUndeployed + Bot.armiesFromReinforcementCards;
            int used = 0;
            foreach(GameOrderDeploy deploy in this.Deploys)
            {
                used += deploy.NumArmies;
            }
            return start - used;
        }
    }
}