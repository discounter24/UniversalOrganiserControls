using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.Configuration
{
    public class AdvancedConfig
    {

        public JsonBrowser Browser;

        public JsonServer Server;

        public JsonMode Easy;
        public JsonMode Normal;
        public JsonMode Hard;


        public AdvancedConfig()
        {
            Browser = new JsonBrowser();

            Server = new JsonServer();

            Easy = new JsonMode();
            Normal = new JsonMode();
            Hard = new JsonMode();
        }


        public string getJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }


        public static AdvancedConfig loadJson(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    return JsonConvert.DeserializeObject<AdvancedConfig>(File.ReadAllText(file));
                }
                catch (Exception)
                {
                    return null;
                }
            } else {
                return null;
            }

        }

        public void save(U3Server server)
        {
            AdvancedConfig.save(server, this);
        }

        public static AdvancedConfig loadJson(U3Server server)
        {
            FileInfo configFile = new FileInfo(server.ServerInformation.ServerDirectory.FullName + "\\Config.json");
            if (configFile.Exists)
            {
                return JsonConvert.DeserializeObject<AdvancedConfig>(File.ReadAllText(server.ServerInformation.ServerDirectory.FullName + "\\Config.json"));
            }
            else
            {
                return null;
            }
        }

        public static void save(U3Server server, AdvancedConfig config)
        {
            try
            {
                FileInfo configFile = new FileInfo(server.ServerInformation.ServerDirectory.FullName + "\\Config.json");
                File.WriteAllText(configFile.FullName, config.getJson());
            }
            catch (Exception)
            {
                throw;
            }
        }



        public class JsonBrowser
        {
            public string Icon = "";
            public string Desc_Hint = "";
            public string Desc_Full = "";
        }

        public class JsonServer
        {
            public bool VAC_Secure = true;
            public bool BattlEye_Secure = true;
        }

        public class JsonMode
        {
            public JsonModeItems Items;
            public JsonModeVehicles Vehicles;
            public JsonModeZombies Zombies;
            public JsonModeAnimals Animals;
            public JsonModeBarricades Barricades;
            public JsonModeStructures Structures;
            public JsonModePlayers Players;
            public JsonModeObjects Objects;
            public JsonModeEvents Events;
            public JsonModeGameplay Gameplay;


            public JsonMode()
            {
                Items = new JsonModeItems();
                Vehicles = new JsonModeVehicles();
                Zombies = new JsonModeZombies();
                Animals = new JsonModeAnimals();
                Barricades = new JsonModeBarricades();
                Structures = new JsonModeStructures();
                Players = new JsonModePlayers();
                Objects = new JsonModeObjects();
                Events = new JsonModeEvents();
                Gameplay = new JsonModeGameplay();
            }
        }

        public class JsonModeItems
        {
            public double Spawn_Chance;
            public double Despawn_Dropped_Time;
            public double Despawn_Natural_Time;
            public double Respawn_Time;

            public double Quality_Full_Chance;
            public double Quality_Multiplier;

            public double Gun_Bullets_Full_Chance;
            public double Gun_Bullets_Multiplier;
            public double Magazine_Bullets_Full_Chance;
            public double Magazine_Bullets_Multiplier;
            public double Crate_Bullets_Full_Chance;
            public double Crate_Bullets_Multiplier;
            public bool Has_Durability;
        }

        public class JsonModeVehicles
        {
            public double Respawn_Time;
            public double Armor_Multiplier;
        }

        public class JsonModeZombies
        {
            public double Spawn_Chance;
            public double Loot_Chance;
            public double Crawler_Chance;
            public double Sprinter_Chance;
            public double Flanker_Chance;
            public double Burner_Chance;
            public double Acid_Chance;
            public double Respawn_Day_Time;
            public double Respawn_Night_Time;
            public double Respawn_Beacon_Time;
            public double Damage_Multiplier;
            public double Armor_Multiplier;
            public double Beacon_Experience_Multiplier;
            public double Full_Moon_Experience_Multiplier;
            public bool Slow_Movement;
            public bool Can_Stun;
        }

        public class JsonModeAnimals
        {
            public double Respawn_Time;
            public double Damage_Multiplier;
            public double Armor_Multiplier;
        }

        public class JsonModeBarricades
        {
            public int Decay_Time;
            public double Armor_Multiplier;
        }

        public class JsonModeStructures
        {
            public int Decay_Time;
            public double Armor_Multiplier;
        }

        public class JsonModePlayers
        {
            public int Health_Regen_Min_Food;
            public int Health_Regen_Min_Water;
            public int Health_Regen_Ticks;
            public int Food_Use_Ticks;
            public int Food_Damage_Ticks;
            public int Water_Use_Ticks;
            public int Water_Damage_Ticks;
            public int Virus_Infect;
            public int Virus_Use_Ticks;
            public int Virus_Damage_Ticks;
            public int Leg_Regen_Ticks;
            public int Bleed_Damage_Ticks;
            public int Bleed_Regen_Ticks;
            public double Armor_Multiplier;
            public double Experience_Multiplier;
            public double Detect_Radius_Multiplier;
            public double Lose_Skills_PvP;
            public double Lose_Skills_PvE;
            public double Lose_Items_PvP;
            public double Lose_Items_PvE;
            public bool Lose_Clothes_PvP;
            public bool Lose_Clothes_PvE;
            public bool Can_Hurt_Legs;
            public bool Can_Break_Legs;
            public bool Can_Fix_Legs;
            public bool Can_Start_Bleeding;
            public bool Can_Stop_Bleeding;
        }

        public class JsonModeObjects
        {
            public double Binary_State_Reset_Multiplier;
            public double Fuel_Reset_Multiplier;
            public double Water_Reset_Multiplier;
            public double Resource_Reset_Multiplier;
            public double Rubble_Reset_Multiplier;
        }

        public class JsonModeEvents
        {
            public double Rain_Frequency_Min;
            public double Rain_Frequency_Max;
            public double Rain_Duration_Min;
            public double Rain_Duration_Max;
            public double Airdrop_Frequency_Min;
            public double Airdrop_Frequency_Max;
        }

        public class JsonModeGameplay
        {
            public int Repair_Level_Max;
            public bool Hitmarkers;
            public bool Crosshair;
            public bool Ballistics;
            public bool Chart;
            public bool Group_Map;
            public bool Group_HUD;
            public bool Timer_Exit;
            public int Timer_Respawn;
            public int Timer_Home;
        }
    }
}
