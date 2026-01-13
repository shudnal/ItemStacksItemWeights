using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace ItemStacksItemWeights
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class ItemStacksItemWeights : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.ItemStacksItemWeights";
        public const string pluginName = "Item Stacks Item Weights";
        public const string pluginVersion = "1.0.2";

        public readonly Harmony harmony = new(pluginID);

        public static ItemStacksItemWeights instance;

        public static readonly ConfigSync configSync = new(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;

        public static ConfigEntry<bool> hideStackSize;

        public static readonly CustomSyncedValue<ItemConfigurations> configurationFile = new(configSync, $"{pluginID}.ConfigurationFile", new());

        public static readonly string filename = $"{pluginID}.yml";
        public static readonly string filepath = Path.Combine(Paths.ConfigPath, pluginID);
        public static readonly string fullpath = Path.Combine(filepath, filename);

        public FileSystemWatcher fileWatcher;

        private void Awake()
        {
            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            Game.isModded = true;

            harmony.PatchAll();
        }

        private void Start()
        {
            if (!Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);

            if (!File.Exists(fullpath))
                File.WriteAllText(fullpath, new Serializer().Serialize(new ItemConfigurations()));

            fileWatcher = new FileSystemWatcher(filepath, filename);
            fileWatcher.Changed += (s, e) => LoadConfigs();
            fileWatcher.Created += (s, e) => ClearConfigs();
            fileWatcher.Renamed += (s, e) => ClearConfigs();
            fileWatcher.Deleted += (s, e) => ClearConfigs();
            fileWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileWatcher.EnableRaisingEvents = true;

            LoadConfigs();

            configurationFile.ValueChanged += ItemProcessing.OnConfigFileChange;

            DocGen.InitCommand();
        }

        private void OnDestroy()
        {
            Config.Save();
            instance = null;
            harmony?.UnpatchSelf();
        }

        public static void LogInfo(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(data);
        }

        public static void LogWarning(object data)
        {
            instance.Logger.LogWarning(data);
        }

        public void ConfigInit()
        {
            configLocked = config("General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only.");
            loggingEnabled = config("General", "Logging enabled", defaultValue: false, "Enable logging. [Not Synced with Server]", false);
            hideStackSize = config("General", "Hide stack size", defaultValue: false, "Hide stack size of items");
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        public void ClearConfigs()
        {
            configurationFile.AssignValueIfChanged(new ItemConfigurations());
            LogInfo($"Config file cleared: {filename}");
        }

        public void LoadConfigs()
        {
            if (!File.Exists(fullpath))
                return;

            ItemConfigurations configFile;
            try
            {
                configFile = new Deserializer().Deserialize<ItemConfigurations>(File.ReadAllText(fullpath));
            }
            catch (Exception e)
            {
                configFile = new ItemConfigurations();
                LogInfo($"Error when reading {filename}:\n{e}");
            }

            configurationFile.AssignValueIfChanged(configFile);
            LogInfo($"Config file changed: {filename}");
        }

        [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
        public static class HotkeyBar_UpdateIcons_HideStackSize
        {
            public static void Postfix(HotkeyBar __instance)
            {
                if (!hideStackSize.Value)
                    return;

                for (int index = 0; index < __instance.m_elements.Count; index++)
                {
                    HotkeyBar.ElementData elementData = __instance.m_elements[index];
                    if (elementData.m_amount.gameObject.activeInHierarchy)
                        elementData.m_amount.SetText(elementData.m_stackText.ToFastString());
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
        public static class InventoryGrid_UpdateGui_HideStackSize
        {
            public static void Postfix(InventoryGrid __instance)
            {
                if (!hideStackSize.Value)
                    return;

                foreach (InventoryGrid.Element element in __instance.m_elements)
                    if (element.m_amount.gameObject.activeInHierarchy)
                        element.m_amount.SetText(element.m_amount.text.Split('/')[0]);
            }
        }
    }
}
