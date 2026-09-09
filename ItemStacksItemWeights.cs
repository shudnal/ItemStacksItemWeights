using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ConditionalConfigSync;
using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ItemStacksItemWeights
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    [BepInDependency("_shudnal.ConditionalConfigSync", "1.0.5")]
    public class ItemStacksItemWeights : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.ItemStacksItemWeights";
        public const string pluginName = "Item Stacks Item Weights";
        public const string pluginVersion = "1.1.1";

        public readonly Harmony harmony = new(pluginID);

        public static ItemStacksItemWeights instance;

        public static readonly ConfigSync configSync = new(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion, ModRequired = true };

        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;

        public static ConfigEntry<bool> hideStackSize;
        public static ConfigEntry<bool> compactStackSize;
        public static ConfigEntry<bool> compactAmountSize;
        public static ConfigEntry<KeyboardShortcut> showFullStackSize;

        public static readonly CustomSyncedValue<ItemConfigurations> configurationFile = new(configSync, $"{pluginID}.ConfigurationFile", new());

        public static readonly string filename = $"{pluginID}.yml";
        public static readonly string filepath = Path.Combine(Paths.ConfigPath, pluginID);
        public static readonly string fullpath = Path.Combine(filepath, filename);

        public FileSystemWatcher fileWatcher;
        private bool reloadPending;
        private float reloadAt;
        private int reloadAttempts;

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
                File.WriteAllText(fullpath, new Serializer().Serialize(ItemConfigurations.CreateDefaultTemplate()));

            fileWatcher = new FileSystemWatcher(filepath, filename);
            fileWatcher.Changed += ScheduleConfigReload;
            fileWatcher.Created += ScheduleConfigReload;
            fileWatcher.Renamed += ScheduleConfigReload;
            fileWatcher.Deleted += ScheduleConfigReload;
            fileWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileWatcher.EnableRaisingEvents = true;

            configurationFile.ValueChanged += ItemProcessing.OnConfigFileChange;
            LoadConfigs();
            ItemProcessing.OnConfigFileChange();

            DocGen.InitCommand();
        }

        private void OnDestroy()
        {
            reloadPending = false;
            fileWatcher?.Dispose();
            fileWatcher = null;
            configurationFile.ValueChanged -= ItemProcessing.OnConfigFileChange;
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
            compactStackSize = config("General", "Compact stack size", defaultValue: false, "Display large stack sizes in a compact format (e.g. 1k instead of 1000, 1M instead of 1000000).");
            compactAmountSize = config("General", "Compact amount size", defaultValue: false, "Display large amount of items in a compact format (e.g. 1k instead of 1000, 1M instead of 1000000). The number is rounded down.");
            showFullStackSize = config("General", "Show full stack size", defaultValue: new KeyboardShortcut(KeyCode.LeftAlt), "Hold to display stack sizes without changes. [Not Synced with Server]", false);

            hideStackSize.SettingChanged += (s,e) => ItemStackText.ClearCache();
            compactStackSize.SettingChanged += (s, e) => ItemStackText.ClearCache();
            compactAmountSize.SettingChanged += (s, e) => ItemStackText.ClearCache();
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            configSync.AddConfigEntry(configEntry, ConfigSyncMode.Conditional, serverControlledByDefault: synchronizedSetting);

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        public void ClearConfigs()
        {
            configurationFile.AssignLocalValueIfChanged(new ItemConfigurations());
            LogInfo($"Config file cleared: {filename}");
        }

        private void ScheduleConfigReload(object sender, FileSystemEventArgs args)
        {
            reloadPending = true;
            reloadAt = Time.unscaledTime + 0.25f;
            reloadAttempts = 3;
        }

        private void Update()
        {
            if (!reloadPending || Time.unscaledTime < reloadAt)
                return;

            reloadAttempts--;
            if (TryLoadConfigs(out Exception error))
                reloadPending = false;
            else if (reloadAttempts > 0)
                reloadAt = Time.unscaledTime + 0.25f;
            else
            {
                reloadPending = false;
                LogWarning($"Could not reload {filename}; keeping the last valid configuration. {error}");
            }
        }

        public void LoadConfigs()
        {
            if (!TryLoadConfigs(out Exception error))
            {
                LogWarning($"Could not load {filename}; keeping the last valid configuration. {error}");
                ScheduleConfigReload(null, null);
            }
        }

        private bool TryLoadConfigs(out Exception error)
        {
            error = null;
            if (!File.Exists(fullpath))
            {
                ClearConfigs();
                return true;
            }

            try
            {
                ItemConfigurations configFile = new Deserializer().Deserialize<ItemConfigurations>(File.ReadAllText(fullpath))
                    ?? new ItemConfigurations();
                configFile.EnsureInitialized();
                configFile.Validate();
                configurationFile.AssignLocalValueIfChanged(configFile);
                LogInfo($"Config file changed: {filename}");
                return true;
            }
            catch (Exception exception)
            {
                error = exception;
                return false;
            }
        }
    }
}
