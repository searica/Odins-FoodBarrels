using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PieceManager;
using ServerSync;
using UnityEngine;


namespace OdinsFoodBarrels
{
    [BepInPlugin(HGUIDLower, ModName, ModVersion)]
    public class OdinsFoodBarrelsPlugin : BaseUnityPlugin
    {
        public const string ModVersion = "1.0.14";
        public const string ModName = "OdinsFoodBarrels";
        internal const string Author = "Gravebear";
        internal const string HGUID = Author + "." + "OdinsFoodBarrels";
        internal const string HGUIDLower = "gravebear.odinsfoodbarrels";
        private const string ModGUID = "Harmony." + Author + "." + ModName;
        private static string ConfigFileName = HGUIDLower + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        public static string ConnectionError = "";



        private static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private static ConfigEntry<Toggle> serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        private enum Toggle
        {
            On = 1,
            Off = 0
        }

        private void Awake()
        {
            BuildPiece OH_Raspberries = new("odinsnummies", "OH_Raspberries");
            OH_Raspberries.Name.English("Raspberry Barrel");
            OH_Raspberries.Description.English("A barrel of Raspberries");
            OH_Raspberries.RequiredItems.Add("Raspberry", 50, true);
            OH_Raspberries.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Blue_Mushrooms = new("odinsnummies", "OH_Blue_Mushrooms");
            OH_Blue_Mushrooms.Name.English("Blue Mushroom Basket");
            OH_Blue_Mushrooms.Description.English("A basket of BlueMushrooms");
            OH_Blue_Mushrooms.RequiredItems.Add("MushroomBlue", 50, true);
            OH_Blue_Mushrooms.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Blueberries = new("odinsnummies", "OH_Blueberries");
            OH_Blueberries.Name.English("Blueberry Barrel");
            OH_Blueberries.Description.English("A barrel of Blueberrys");
            OH_Blueberries.RequiredItems.Add("Blueberries", 50, true);
            OH_Blueberries.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Carrots = new("odinsnummies", "OH_Carrots");
            OH_Carrots.Name.English("Carrot Barrel");
            OH_Carrots.Description.English("A barrel of Carrots");
            OH_Carrots.RequiredItems.Add("Carrot", 50, true);
            OH_Carrots.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_CloudBerries = new("odinsnummies", "OH_CloudBerries");
            OH_CloudBerries.Name.English("Cloudberry Barrel");
            OH_CloudBerries.Description.English("A barrel of Cloudberries");
            OH_CloudBerries.RequiredItems.Add("Cloudberry", 50, true);
            OH_CloudBerries.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Fish = new("odinsnummies", "OH_Fish");
            OH_Fish.Name.English("Fish Barrel");
            OH_Fish.Description.English("A barrel of Raw Fish");
            OH_Fish.RequiredItems.Add("FishRaw", 50, true);
            OH_Fish.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Honey = new("odinsnummies", "OH_Honey");
            OH_Honey.Name.English("Honey Barrel");
            OH_Honey.Description.English("A barrel of Honey");
            OH_Honey.RequiredItems.Add("Honey", 50, true);
            OH_Honey.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Red_Mushrooms = new("odinsnummies", "OH_Red_Mushrooms");
            OH_Red_Mushrooms.Name.English("Red Mushroom Basket");
            OH_Red_Mushrooms.Description.English("A Red Mushroom Basket");
            OH_Red_Mushrooms.RequiredItems.Add("Mushroom", 50, true);
            OH_Red_Mushrooms.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Turnips = new("odinsnummies", "OH_Turnips");
            OH_Turnips.Name.English("Turnip Barrel");
            OH_Turnips.Description.English("A barrel of Turnips");
            OH_Turnips.RequiredItems.Add("Turnip", 50, true);
            OH_Turnips.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Yellow_Mushrooms = new("odinsnummies", "OH_Yellow_Mushrooms");
            OH_Yellow_Mushrooms.Name.English("Yellow Mushroom Basket");
            OH_Yellow_Mushrooms.Description.English("A Yellow Mushroom Basket");
            OH_Yellow_Mushrooms.RequiredItems.Add("MushroomYellow", 50, true);
            OH_Yellow_Mushrooms.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Thistle = new("odinsnummies", "OH_Thistle");
            OH_Thistle.Name.English("Thistle Basket");
            OH_Thistle.Description.English("A Thistle Basket");
            OH_Thistle.RequiredItems.Add("Thistle", 50, true);
            OH_Thistle.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Dandelion = new("odinsnummies", "OH_Dandelion");
            OH_Dandelion.Name.English("Dandelion Basket");
            OH_Dandelion.Description.English("A Dandelion Basket");
            OH_Dandelion.RequiredItems.Add("Dandelion", 50, true);
            OH_Dandelion.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Barley = new("odinsnummies", "OH_Barley");
            OH_Barley.Name.English("Barley Barrel");
            OH_Barley.Description.English("A barrel of Barley");
            OH_Barley.RequiredItems.Add("Barley", 50, true);
            OH_Barley.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Flax = new("odinsnummies", "OH_Flax");
            OH_Flax.Name.English("Flax Barrel");
            OH_Flax.Description.English("A barrel of Flax");
            OH_Flax.RequiredItems.Add("Flax", 50, true);
            OH_Flax.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Onions = new("odinsnummies", "OH_Onions");
            OH_Onions.Name.English("Onion Barrel");
            OH_Onions.Description.English("A barrel of Onions");
            OH_Onions.RequiredItems.Add("Onion", 50, true);
            OH_Onions.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Egg_Basket = new("odinsnummies", "OH_Egg_Basket");
            OH_Egg_Basket.Name.English("Egg Basket");
            OH_Egg_Basket.Description.English("A basket of Eggs");
            OH_Egg_Basket.RequiredItems.Add("ChickenEgg", 50, true);
            OH_Egg_Basket.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_JotunPuffs_Basket = new("odinsnummies", "OH_JotunPuffs_Basket");
            OH_JotunPuffs_Basket.Name.English("JotunPuffs Basket");
            OH_JotunPuffs_Basket.Description.English("A basket of JotunPuffs");
            OH_JotunPuffs_Basket.RequiredItems.Add("MushroomJotunPuffs", 50, true);
            OH_JotunPuffs_Basket.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Magecap = new("odinsnummies", "OH_Magecap");
            OH_Magecap.Name.English("Magecap Basket");
            OH_Magecap.Description.English("A basket of Magecap");
            OH_Magecap.RequiredItems.Add("MushroomMagecap", 50, true);
            OH_Magecap.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_RoyalJelly = new("odinsnummies", "OH_RoyalJelly");
            OH_RoyalJelly.Name.English("RoyalJelly Barrel");
            OH_RoyalJelly.Description.English("A barrel of RoyalJelly");
            OH_RoyalJelly.RequiredItems.Add("RoyalJelly", 50, true);
            OH_RoyalJelly.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Sap_Barrel = new("odinsnummies", "OH_Sap_Barrel");
            OH_Sap_Barrel.Name.English("Sap Barrel");
            OH_Sap_Barrel.Description.English("A barrel of Sap");
            OH_Sap_Barrel.RequiredItems.Add("Sap", 50, true);
            OH_Sap_Barrel.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Seedbag = new("odinsnummies", "OH_Seedbag");
            OH_Seedbag.Name.English("OdinsSeedbag");
            OH_Seedbag.Description.English("A bag to put what ever in.");
            OH_Seedbag.RequiredItems.Add("DeerHide", 5, true);
            OH_Seedbag.Category.Set(BuildPieceCategory.Misc);


            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new(ModGUID);
            harmony.PatchAll(assembly);


        }


    }
}