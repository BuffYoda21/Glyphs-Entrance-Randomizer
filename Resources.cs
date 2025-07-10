using System.Collections.Generic;
using Newtonsoft.Json;
using Il2CppSystem.IO;
using MelonLoader.Utils;
using MelonLoader;

namespace GlyphsEntranceRando {
    public class Resources {
        public class ResultsJSON {
            public static string NAME = "RandomizationResults.json";
            private static string PATH = Path.Combine(MelonEnvironment.UserDataDirectory, NAME);
            // This property is used to get and set the contents of the JSON file.
            public static List<SerializedEntrancePair> Contents {
                get {
                    if (!File.Exists(PATH)) return null;
                    try {
                        string json = File.ReadAllText(PATH);
                        return JsonConvert.DeserializeObject<List<SerializedEntrancePair>>(json);
                    } catch {
                        MelonLogger.Error($"Failed to load {NAME}. File exists, but may be invalid.");
                        return null;
                    }
                }
                set {
                    File.WriteAllText(PATH, JsonConvert.SerializeObject(value, Formatting.Indented));
                }
            }
        }
    }
}
public class SerializedEntrancePair {
    public int entrance { get; set; }
    public int couple { get; set; }
}
