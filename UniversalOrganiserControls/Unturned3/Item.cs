using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3
{
    public class Item
    {
        private string _id;
        private string _name;

        public string ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Item(string id, string name)
        {
            _id = id;
            _name = name;
        }

        public Item(ushort id, string name)
        {
            _id = id.ToString();
            _name = name;
        }

        public static Item loadFromDir(DirectoryInfo dir)
        {
            string _name = "";
            string _id = "";

            FileInfo statFile = new FileInfo(dir.FullName + "\\" + dir.Name + ".dat");
            FileInfo englishFile = new FileInfo(dir.FullName + "\\English.dat");

            string[] lines = File.ReadAllLines(statFile.FullName);
            _id = getAttribut(lines, "ID");

            lines = File.ReadAllLines(englishFile.FullName);
            _name = getAttribut(lines, "Name");

            return new Item(_id, _name);
        }

        public static bool isValidItemDir(DirectoryInfo dir)
        {
            FileInfo statFile = new FileInfo(dir.FullName + "\\" + dir.Name + ".dat");
            FileInfo englishFile = new FileInfo(dir.FullName + "\\English.dat");

            return (statFile.Exists & englishFile.Exists);
        }

        private static string getAttribut(string[] lines, string a)
        {
            foreach (string s in lines)
            {
                if (s.StartsWith(a))
                {
                    return s.Remove(0, a.Length + 1);
                }
            }
            return "";
        }

    }
}
