using Kitchen;
using Kitchen.Modules;
using KitchenApplianceChest.Customs;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Preferences;
using KitchenLib.Utils;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenApplianceChest
{
    public class Main : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.ApplianceChest";
        public const string MOD_NAME = "Appliance Chest";
        public const string MOD_VERSION = "0.2.6";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.4";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        public static AssetBundle Bundle;

        internal static string APPLIANCE_CHEST_ENABLED_ID = "applianceChestEnabled";

        internal static PreferenceManager Manager;

        internal static PreferenceBool ApplianceChestEnabledPreference;

        internal static int ChestApplianceID;

        static Main Instance;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            UpdateUpgrades();
            Instance = this;
        }
        private void UpdateUpgrades()
        {
            Appliance applianceChest = (Appliance)GDOUtils.GetCustomGameDataObject<Chest>().GameDataObject;
            if (applianceChest != null)
            {
                if (!ApplianceChestEnabledPreference.Get())
                {
                    applianceChest.IsPurchasable = false;
                    applianceChest.IsPurchasableAsUpgrade = false;
                }
            }
        }

        protected override void OnUpdate()
        {
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");

            ChestApplianceID = AddGameDataObject<Chest>().ID;
            AddGameDataObject<LockedChest>();

            LogInfo("Done loading game data.");
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            // TODO: Uncomment the following if you have an asset bundle.
            // TODO: Also, make sure to set EnableAssetBundleDeploy to 'true' in your ModName.csproj

            // LogInfo("Attempting to load asset bundle...");
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            LogInfo("Done loading asset bundle.");

            // Register custom GDOs
            AddGameData();

            // Perform actions when game data is built
            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
                args.gamedata.ProcessesView.Initialise(args.gamedata);
            };

            RegisterPreferences();
            RegisterMenu();
        }

        protected void RegisterPreferences()
        {
            Manager = new PreferenceManager(MOD_GUID);
            ApplianceChestEnabledPreference = Manager.RegisterPreference<PreferenceBool>(new PreferenceBool(APPLIANCE_CHEST_ENABLED_ID, true));
            Manager.Load();
        }

        protected void RegisterMenu()
        {
            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) =>
            {
                args.Menus.Add(typeof(PrefMenu<PauseMenuAction>), new PrefMenu<PauseMenuAction>(args.Container, args.Module_list));
            };
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu(MOD_NAME, typeof(PrefMenu<PauseMenuAction>), typeof(PauseMenuAction));
        }

        internal static bool TryGetStoredAppliances(Entity e, out IEnumerable<Appliance> storedAppliances)
        {
            bool success = false;
            List<Appliance> tempStoredAppliances = new List<Appliance>();

            if (Instance.Require(e, out CApplianceStorage applianceStorage))
            {
                List<int> storedApplianceIds = applianceStorage.GetApplianceIDs();
                if (storedApplianceIds.Count > 0)
                {
                    foreach (int storedApplianceId in storedApplianceIds)
                    {
                        if (storedApplianceId == 0)
                            continue;
                        if (GameData.Main.TryGet(storedApplianceId, out Appliance storedAppliance, warn_if_fail: true))
                            tempStoredAppliances.Add(storedAppliance);
                    }
                }
                success = true;
            }
            storedAppliances = tempStoredAppliances;
            return success;
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }

    public class PrefMenu<T> : KLMenu<T>
    {
        Option<bool> ApplianceChestEnabledOption;

        public PrefMenu(Transform container, ModuleList module_list) : base(container, module_list)
        {
        }

        public override void Setup(int player_id)
        {
            AddLabel("Appliance Chest");
            AddInfo("Changes require game restart!");
            New<SpacerElement>();
            ApplianceChestEnabledOption = new Option<bool>(
                new List<bool> { false, true }, Main.ApplianceChestEnabledPreference.Get(), new List<string> { "Disabled", "Enabled" });
            Add<bool>(ApplianceChestEnabledOption).OnChanged += delegate (object _, bool value)
            {
                Main.ApplianceChestEnabledPreference.Set(value);
                Main.Manager.Save();
            };

            New<SpacerElement>();
            New<SpacerElement>();

            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate
            {
                RequestPreviousMenu();
            });
        }
    }
}
