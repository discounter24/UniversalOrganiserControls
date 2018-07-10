using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3.Configuration
{

    public enum SkillSet
    {
        Civilian = 0,
        FireFighter = 1,
        PoliceOfficer = 2,
        SpecOps = 3,
        Farmer = 4,
        Fisher = 5,
        Lumberjack = 6,
        Worker = 7,
        Chef = 8,
        Thief = 9,
        Doctor = 10,
        Everyone = 255
    }

    public class Loadout
    {
        private SkillSet SkillSet;
        private List<int> Items;

        public Loadout(string line)
        {
            List<string> sequences = line.Split('/').ToList();
            Items = new List<int>();
            if (sequences.Count > 0)
            {
                string head = sequences[0];
                for (int i = 1; i < sequences.Count; i++)
                {
                    try
                    {
                        Items.Add(Convert.ToInt32(sequences[i]));
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                throw new Exception("Not a valid loadout!");
            }
        }

        public override string ToString()
        {
            string line = ((int)SkillSet).ToString();
            for (int i = 0; i < Items.Count; i++)
            {
                line += "/" + Items[i];
            }
            return line;
        }

        public void addItem(int item, int count = 1)
        {
            Items.Add(item);
        }

        public void removeItem(int item, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Items.Remove(item))
                {
                    break;
                }
            }

        }

    }
}
