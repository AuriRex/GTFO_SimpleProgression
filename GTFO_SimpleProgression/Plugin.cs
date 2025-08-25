using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using DropServer;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SimpleProgression;
using SimpleProgression.Core;
using SimpleProgression.Impl;

[assembly: AssemblyVersion(Plugin.VERSION)]
[assembly: AssemblyFileVersion(Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(Plugin.VERSION)]

namespace SimpleProgression;

[BepInPlugin(GUID, MOD_NAME, VERSION)]
public class Plugin : BasePlugin
{
    public const string ALLVANITY_GUID = "dev.aurirex.gtfo.allvanity";
    
    public const string GUID = "dev.aurirex.gtfo.simpleprogression";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    internal static Logger L;

    private static readonly Harmony _harmony = new(GUID);

    internal static bool IsAllVanityLoaded => IL2CPPChainloader.Instance.Plugins.Any(
        kvp => string.Equals(kvp.Key, ALLVANITY_GUID, StringComparison.InvariantCultureIgnoreCase));
    
    public override void Load()
    {
        L = new Logger(Log);
        Log.LogMessage($"Initializing {MOD_NAME}");

        ClassInjector.RegisterTypeInIl2Cpp<LocalDropServerAPI>(new RegisterTypeOptions
        {
            Interfaces = new[] { typeof(IDropServerClientAPI) },
            LogSuccess = true,
        });
        
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    internal static void OnDataBlocksReady()
    {
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