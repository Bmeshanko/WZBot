using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WarLight.Shared.AI.Prime
{

    public class PrimeBot : IWarLightAI
    {
        public GameIDType GameID;
        public PlayerIDType PlayerID;
        public Dictionary<PlayerIDType, GamePlayer> Players;
        public MapDetails Map;
        public GameStanding DistributionStanding;
        public GameSettings Settings;
        public int NumberOfTurns;
        public Dictionary<PlayerIDType, PlayerIncome> Incomes;
        public PlayerIncome BaseIncome;
        public GameOrder[] prevTurn;
        public GameStanding Standing;
        public GameStanding previousTurnStanding;
        public Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        public List<CardInstance> Cards;
        public int CardsMustPlay;
        public Stopwatch timer;
        public List<String> directives;
        public List<GamePlayer> Opponents;
        public int armiesFromReinforcementCards;


        public String Name()
        {
            return "Prime Version 1.0";
        }

        public String Description()
        {
            return "Bot developed by Benjamin628 in Summer 2021.";
        }

        public bool SupportsSettings(GameSettings settings, out string whyNot)
        {
            whyNot = null;
            return true; // Come back later - No way I make a bot that understands all settings.
        }

        public bool RecommendsSettings(GameSettings settings, out string whyNot)
        {
            whyNot = null;
            return true; // Come back later - No way I make a bot that understands all settings.
        }
        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives)
        {
            this.DistributionStanding = distributionStanding;
            this.Standing = latestTurnStanding;
            this.PlayerID = myPlayerID;
            this.Players = players;
            this.Map = map;
            this.Settings = gameSettings;
            this.TeammatesOrders = teammatesOrders;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.Incomes = incomes;
            this.BaseIncome = Incomes[PlayerID];
        }
        public List<TerritoryIDType> GetPicks()
        {
            Picks.MakePicks picks = new Picks.MakePicks(this);
            return picks.Commit(this);
        }

        public List<GameOrder> GetOrders()
        {
            Orders.OrderManager om = new Orders.OrderManager(this);
            om.Go();
            List<GameOrder> orders = new List<GameOrder>();
            
            // Enforcing the Phase order
            foreach (GameOrder card in om.Cards)
            {
                orders.Add(card);
            }
            foreach (GameOrder deploy in om.Deploys)
            {
                orders.Add(deploy);
            }
            foreach (GameOrder attack in om.Moves)
            {
                orders.Add(attack);
            }
            return orders;
        }

        public GamePlayer GamePlayerReference
        {
            get { return Players[PlayerID]; }
        }

        public int BonusValue(BonusIDType bonusID)
        {
            if (Settings.OverriddenBonuses.ContainsKey(bonusID))
                return Settings.OverriddenBonuses[bonusID];
            else
                return Map.Bonuses[bonusID].Amount;
        }

        public int NumTerritories(BonusIDType bonusID)
        {
            return Map.Bonuses[bonusID].Territories.ToArray().Length;
        }

        public string TerrString(TerritoryIDType terrID)
        {
            return Map.Territories[terrID].Name;
        }

        public String BonusString(BonusIDType bonusID)
        {
            return Map.Bonuses[bonusID].Name;
        }

        public List<TerritoryIDType> ConnectedToInBonus(TerritoryIDType terrID)
        {
            BonusIDType bonus = Map.Territories[terrID].PartOfBonuses.First();
            return Map.Territories.Keys.Where(o => Map.Territories[o].PartOfBonuses.First() == bonus && o != terrID && Map.Territories[o].ConnectedTo.Keys.Contains(terrID)).ToList();
        }

        public List<TerritoryIDType> ConnectedToInBonusNeutral(TerritoryIDType terrID)
        {
            return ConnectedToInBonus(terrID).Where(o => Standing.Territories[o].OwnerPlayerID != PlayerID).ToList();
        }

        public int ArmiesOnTerritory(TerritoryIDType terr)
        {
            return Standing.Territories[terr].NumArmies.NumArmies;
        }

        public BonusIDType WhatBonus(TerritoryIDType terrID)
        {
            return Map.Territories[terrID].PartOfBonuses.First();
        }

        public List<TerritoryIDType> OurTerritories()
        {
            return Standing.Territories.Keys.Where(o => Standing.Territories[o].OwnerPlayerID == PlayerID).ToList();
        }

        public List<TerritoryIDType> OurTerritoriesInBonus(BonusIDType bonus)
        {
            return OurTerritories().Where(o => Map.Bonuses[bonus].Territories.Contains(o)).ToList();
        }

        public List<BonusIDType> BonusNeighbors(BonusIDType bonusID)
        {
            BonusDetails bonus = Map.Bonuses[bonusID];

            var terrs = bonus.Territories.ToList();
            var neighbors = new List<BonusIDType>();

            foreach (var terr in terrs)
            {
                var connections = Map.Territories[terr].ConnectedTo.Keys.ToList();
                foreach (var connection in connections)
                {
                    BonusIDType partOf = Map.Territories[connection].PartOfBonuses.First(); // Will not work with superbonuses.
                    if (partOf.GetHashCode() != bonusID.GetHashCode() && !neighbors.Contains(partOf))
                    {
                        neighbors.Add(partOf);
                    }
                }
            }
            return neighbors;
        }

        public Boolean IsWasteland(BonusIDType bonus)
        {
            BonusDetails details = Map.Bonuses[bonus];
            foreach (var terr in details.Territories)
            {
                if (DistributionStanding.Territories[terr].NumArmies.NumArmies == 10)
                {
                    return true;
                }
            }
            return false;
        }

        public List<BonusIDType> Wastelands()
        {
            List<BonusIDType> bonuses = new List<BonusIDType>();
            foreach(var bonus in Map.Bonuses)
            {
                if (IsWasteland(bonus.Key))
                {
                    bonuses.Add(bonus.Key);
                }
            }
            return bonuses;
        }

        public List<TerritoryIDType> Borders()
        {
            return Standing.Territories.Keys
                .Where(o => Map.Territories[o].ConnectedTo.Keys
                .Any(c => Standing.Territories[c].OwnerPlayerID == PlayerID))
                .Where(e => Standing.Territories[e].OwnerPlayerID != PlayerID).ToList();
        }

        public List<TerritoryIDType> EnemyBorders()
        {
            return Borders().Where(o => Standing.Territories[o].OwnerPlayerID != PlayerID && !(Standing.Territories[o].IsNeutral)).ToList();
        }

        public List<TerritoryIDType> NeutralBorders()
        {
            return Borders().Where(o => Standing.Territories[o].OwnerPlayerID != PlayerID && (Standing.Territories[o].IsNeutral)).ToList();
        }

        public List<TerritoryIDType> EdgeTerritories()
        {
            return OurTerritories()
                .Where(o => Map.Territories[o].ConnectedTo.Keys
                .Any(c => Borders().Contains(c))).ToList();
        }
    }
}
