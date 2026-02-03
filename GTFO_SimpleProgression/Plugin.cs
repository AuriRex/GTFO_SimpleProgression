using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Clonesoft.Json;
using DropServer;
using DropServer.BoosterImplants;
using Globals;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SimpleProgression;
using SimpleProgression.Core;
using SimpleProgression.Impl;
using SimpleProgression.Models;

[assembly: AssemblyVersion(Plugin.VERSION)]
[assembly: AssemblyFileVersion(Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(Plugin.VERSION)]

namespace SimpleProgression;

[BepInPlugin(GUID, MOD_NAME, VERSION)]
// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : BasePlugin
{
    public const string ALLVANITY_GUID = "dev.aurirex.gtfo.allvanity";
    
    public const string GUID = "dev.aurirex.gtfo.simpleprogression";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    internal static Logger L;

    private static readonly Harmony _harmony = new(GUID);

    private const string CONFIG_FILE_NAME = "SimpleProgression_Config.json";
    internal static SPConfig SPConfig = new();
    
    internal static bool IsAllVanityLoaded => IL2CPPChainloader.Instance.Plugins.Any(
        kvp => string.Equals(kvp.Key, ALLVANITY_GUID, StringComparison.InvariantCultureIgnoreCase));
    
    public override void Load()
    {
        L = new Logger(Log);
        Log.LogMessage($"Initializing {MOD_NAME}");

        LoadConfig();
        
        ClassInjector.RegisterTypeInIl2Cpp<LocalDropServerAPI>(new RegisterTypeOptions
        {
            Interfaces = new[] { typeof(IDropServerClientAPI) },
            LogSuccess = true,
        });
        
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void LoadConfig()
    {
        try
        {
            var path = Path.Combine(BepInEx.Paths.ConfigPath, CONFIG_FILE_NAME);

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                SPConfig = JsonConvert.DeserializeObject<SPConfig>(json);
                return;
            }

            var jsonNew = JsonConvert.SerializeObject(new SPConfig(), Formatting.Indented);
            File.WriteAllText(path, jsonNew);
        }
        catch (Exception ex)
        {
            Log.LogError($"Error while loading config file: {ex.GetType().FullName}: {ex.Message}");
            Log.LogWarning($"StackTrace:\n{ex.StackTrace}");
            SPConfig = new();
        }
    }

    internal static void OnDataBlocksReady()
    {
        if (SPConfig.UnlockAllLevels)
            Global.AllowFullRundown = true;
        
        var maxCount = (int)Plugin.SPConfig.MaxBoosterCountPerCategory;
        for (var i = 0; i < 3; i++)
        {
            BoosterImplantConstants.InventoryLimitPerCategory[i] = maxCount;
            BoosterImplantConstants.ARTIFACT_TO_BOOSTER_RATIO_FOR_FULL_HEAT[i] = 0.15f;
        }
        
        try
        {
            LocalVanityItemDropper.Instance.Init();
            LocalBoosterDropper.Instance.Init();
        }
        catch(Exception ex)
        {
            L.Exception(ex);
        }
    }
}