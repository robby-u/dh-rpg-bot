using dotHack_Discord_Game.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace dotHack_Discord_Game
{
    public class Data
    {
        #region Formatting Function
        private static string AddSquareBrackets(string json)
        {
            string result = $"[{json}]";
            return result;
        }
        #endregion

        #region Save Function
        public static async Task SaveSettings(Player p)
        {
            var filePath = Directory.GetCurrentDirectory() + "/saves/" + Bot.client.CurrentUser.Id.ToString() + ".json";
            string json = AddSquareBrackets(JsonConvert.SerializeObject(p));
            using (StreamWriter str = new StreamWriter(filePath))
            {
                await str.WriteAsync(json);
            }
        }
        #endregion

        #region Load Function
        public static async Task LoadSettings()
        {
            var filePath = Directory.GetCurrentDirectory() + "/saves/";
            string[] jsonFile = Directory.GetFiles(filePath, Bot.client.CurrentUser.Id + ".json");
            StreamReader reader = new StreamReader(jsonFile[0]);
            string jsonString = reader.ReadToEnd();

            try
            {
                #region Load Declarations
                ulong _Id = default, _Kills;
                string _Name = default;
                int _Level, _Experience, _Max_Experience;
                JobClass _Class;
                Weapon _Equip;

                var objs = JArray.Parse(jsonString).ToObject<List<JObject>>();
                #endregion

                #region Load Player Data
                _Id = (ulong)Convert.ToInt64(objs[0]["Id"].ToString().Trim().Replace(",", ""));
                _Name = Convert.ToString(objs[0]["Name"].ToString().Trim().Replace(",", ""));
                _Kills = (ulong)Convert.ToInt64(objs[0]["Kills"].ToString().Trim().Replace(",", ""));
                _Level = Convert.ToInt32(objs[0]["Level"].ToString().Trim().Replace(",", ""));
                _Experience = Convert.ToInt32(objs[0]["Experience"].ToString().Trim().Replace(",", ""));
                _Max_Experience = Convert.ToInt32(objs[0]["Max_Experience"].ToString().Trim().Replace(",", ""));
                _Class = (JobClass)Convert.ToInt32(objs[0]["Class"].ToString().Trim().Replace(",", ""));

                Player player = new Player(_Id, _Name);

                player.Class = _Class;
                player.Level = _Level;
                player.Kills = _Kills;
                player.Experience = _Experience;
                player.Max_Experience = _Max_Experience;
                #endregion

                #region Load Equipped Weapon Data
                string eq_name = objs[0]["Equip"]["Name"].ToString().Trim().Replace(",", "");
                int eq_level = Convert.ToInt32(objs[0]["Equip"]["RequiredLevel"].ToString().Trim().Replace(",", ""));
                int eq_class = Convert.ToInt32(objs[0]["Equip"]["RequiredClass"].ToString().Trim().Replace(",", ""));
                int eq_attack = Convert.ToInt32(objs[0]["Equip"]["Attack"].ToString().Trim().Replace(",", ""));
                double eq_crit_rate = Convert.ToDouble(objs[0]["Equip"]["Crit_Rate"].ToString().Trim().Replace(",", ""));

                _Equip = new Weapon(eq_name, eq_level, eq_attack, eq_crit_rate, (JobClass)eq_class);
                #endregion

                #region Load Inventory Storage
                JArray equipsInv = (JArray)objs[0]["Inventory"];
                int equipsInv_Length = equipsInv.Count;

                JArray itemsInv = (JArray)objs[0]["Items"];
                int itemsInv_Length = itemsInv.Count;

                if(equipsInv_Length > 0)
                {
                    // we need to load equips stored in the inventory
                    for (int i = 0; i < equipsInv_Length; i++)
                    {
                        string eqi_name = objs[0]["Inventory"][i]["Name"].ToString().Trim().Replace(",", "");
                        int eqi_level = Convert.ToInt32(objs[0]["Inventory"][i]["RequiredLevel"].ToString().Trim().Replace(",", ""));
                        int eqi_class = Convert.ToInt32(objs[0]["Inventory"][i]["RequiredClass"].ToString().Trim().Replace(",", ""));
                        int eqi_attack = Convert.ToInt32(objs[0]["Inventory"][i]["Attack"].ToString().Trim().Replace(",", ""));
                        double eqi_crit_rate = Convert.ToDouble(objs[0]["Inventory"][i]["Crit_Rate"].ToString().Trim().Replace(",", ""));
                        Weapon weapon = new Weapon(eqi_name, eqi_level, eqi_attack, eqi_crit_rate, (JobClass)eqi_class);

                        player.Inventory.Add(weapon);
                    }
                }

                if(itemsInv_Length > 0)
                {
                    // todo: logic for items
                }
                #endregion

                #region Add User to Online List
                Bot.Players.Add(player.Id.ToString(), player);
                await Bot.SendMessage("User " + player.Name + " loaded their " + player.Class + ".");
                #endregion
            }
            catch (Exception ex)
            {
                await Bot.SendMessage("[DEBUG] Exception caught: " + ex.Message + "\n\nLog: " + ex.ToString());
            }
        }
        #endregion
    }
}
