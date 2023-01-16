using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime.Orders
{
    public class OrderManager
    {
        public PrimeBot Bot;
        public PlayerIncomeTracker IncomeTracker;
        public List<GameOrder> Cards;
        public List<GameOrder> Deploys;
        public List<GameOrder> Moves;
        public Dictionary<TerritoryIDType, int> DeployTracker;
        public List<Attack> AttackTracker;

        public OrderManager(PrimeBot bot)
        {
            this.Bot = bot;
            this.IncomeTracker = new PlayerIncomeTracker(Bot.Incomes[Bot.PlayerID], Bot.Map);
            this.Cards = new List<GameOrder>();
            this.Moves = new List<GameOrder>();
            this.Deploys = new List<GameOrder>();
            this.DeployTracker = new Dictionary<TerritoryIDType, int>();
            this.AttackTracker = new List<Attack>();
        }

        public void Go()
        {
            PlayCards.Go(Bot, this);

            int armies = CalculateArmiesRemaining();

            AttackDefend ad = new AttackDefend(Bot, this);
            ad.Go(armies);

            armies = CalculateArmiesRemaining();

            Expand e = new Expand(Bot, this);
            e.Go(armies);

            armies = CalculateArmiesRemaining();

            DeployRest dr = new DeployRest(Bot, this);
            dr.Go(armies);

            MoveLeftovers ml = new MoveLeftovers(Bot, this);
            //ml.Go();

            TrackDeploys();
            TrackAttacks();
        }

        public int CalculateArmiesRemaining()
        {
            int start = IncomeTracker.FreeArmiesUndeployed + Bot.armiesFromReinforcementCards;
            int used = 0;
            foreach(var deploy in this.DeployTracker)
            {
                used += deploy.Value;
            }
            return start - used;
        }

        public void TrackDeploys()
        {
            foreach(var deploy in this.DeployTracker)
            {
                AILog.Log("OrderManager", "Deploying " + deploy.Value + " to " + Bot.TerrString(deploy.Key));
                this.Deploys.Add(GameOrderDeploy.Create(Bot.PlayerID, deploy.Value, deploy.Key, false));
            }
        }

        public void TrackAttacks()
        {
            foreach(var attack in this.AttackTracker)
            {
                AILog.Log("OrderManager", "Attacking from " + Bot.TerrString(attack.from) + " to " + Bot.TerrString(attack.to) + " with " + attack.number);
                this.Moves.Add(GameOrderAttackTransfer.Create(Bot.PlayerID, attack.from, attack.to, AttackTransferEnum.AttackTransfer,
                    false, new Armies(attack.number), false));
            }
        }
    }
}