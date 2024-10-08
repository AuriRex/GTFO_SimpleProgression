﻿using BepInEx;
using BepInEx.Unity.IL2CPP;
using DropServer;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SimpleProgression.Core;
using SimpleProgression.Impl;
using System;
using System.Reflection;

[assembly: AssemblyVersion(SimpleProgression.Plugin.VERSION)]
[assembly: AssemblyFileVersion(SimpleProgression.Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(SimpleProgression.Plugin.VERSION)]

namespace SimpleProgression
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BasePlugin
    {
        public const string GUID = "dev.aurirex.gtfo.simpleprogression";
        public const string NAME = "Simple Progression";
        public const string VERSION = "1.0.0";

        internal static Logger L;

        private static Harmony _harmony;

        public override void Load()
        {
            L = new Logger(Log);
            Log.LogMessage($"Initializing {NAME}");

            ClassInjector.RegisterTypeInIl2Cpp<LocalDropServerAPI>(new RegisterTypeOptions
            {
                Interfaces = new[] { typeof(IDropServerClientAPI) },
                LogSuccess = true,
            });

            _harmony = new Harmony(GUID);
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
}