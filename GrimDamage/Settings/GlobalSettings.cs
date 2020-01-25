using System;
using System.IO;

namespace GrimDamage.Settings
{
    static class GlobalSettings {
        public static string SavedParsePath => CreateAndReturn(Path.Combine(BaseFolder, "SavedParses"));

        public static string BaseFolder {
            get {
                string appdata = Environment.GetEnvironmentVariable("LocalAppData");
                if (appdata == null)
                    throw new Exception("Unable to find environment variable LocalAppData");
                string dir = Path.Combine(appdata, "GDDamage");
                return CreateAndReturn(dir);
            }
        }

        private static string CreateAndReturn(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
