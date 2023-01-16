using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prime
{
    public class Attack
    {
        public TerritoryIDType from;
        public TerritoryIDType to;
        public int number;

        public Attack(TerritoryIDType from, TerritoryIDType to, int number)
        {
            this.from = from;
            this.to = to;
            this.number = number;
        }

        public void AddNumber(int number) 
        { 
            this.number += number;
        }
        public Boolean Equals(Attack other)
        {
            return other.from == this.from && other.to == this.to;
        }

        public static Boolean HasFrom(List<Attack> list, TerritoryIDType from)
        {
            foreach (Attack attack in list)
            {
                if (attack.from == from) return true;
            }
            return false;
        }

        public static List<Attack> FindFroms(List<Attack> list, TerritoryIDType from)
        {
            List<Attack> ret = new List<Attack>();
            foreach (Attack attack in list)
            {
                if (attack.from == from) ret.Add(attack);
            }
            return ret;
        }
    }
}
