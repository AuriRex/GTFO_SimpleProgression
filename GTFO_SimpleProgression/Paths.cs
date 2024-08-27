using System.IO;

namespace SimpleProgression
{
    internal class Paths
    {
        private static string _saveFolderPath;
        public static string SaveFolderPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_saveFolderPath))
                {
                    _saveFolderPath = Path.Combine(BepInEx.Paths.BepInExRootPath, "LocalProgression/");
                    Directory.CreateDirectory(_saveFolderPath);
                }
                return _saveFolderPath;
            }
        }


        private static string _vanityItemsLayerDropsPath;
        public static string VanityItemsLayerDropsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_vanityItemsLayerDropsPath))
                {
                    _vanityItemsLayerDropsPath = Path.Combine(VanityFolderPath, "VanityLayerDrops.json");
                }
                return _vanityItemsLayerDropsPath;
            }
        }


        private static string _vanityItemsFilePath;
        public static string VanityItemsFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_vanityItemsFilePath))
                {
                    _vanityItemsFilePath = Path.Combine(VanityFolderPath, "VanityData.json");
                }
                return _vanityItemsFilePath;
            }
        }


        private static string _vanityFolderPath;
        public static string VanityFolderPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_vanityFolderPath))
                {
                    _vanityFolderPath = Path.Combine(SaveFolderPath, "Vanity/");
                    Directory.CreateDirectory(_vanityFolderPath);
                }
                return _vanityFolderPath;
            }
        }


        private static string _boostersPath;
        public static string BoostersFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_boostersPath))
                {
                    var path = Path.Combine(SaveFolderPath, "Boosters/");
                    Directory.CreateDirectory(path);
                    _boostersPath = Path.Combine(path, "BoosterData.json");
                }
                return _boostersPath;
            }
        }
    }
}
