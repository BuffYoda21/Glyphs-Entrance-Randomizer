using System.Collections.Generic;
using System;
using System.Linq;
using MelonLoader;
using Newtonsoft.Json;
using Il2CppSystem.IO;
using System.Reflection;
using MelonLoader.Utils;

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
        // Reads an embeded readable file from the assembly, note: folder.file.ext
        private static string ReadEmbeddedData(string path) {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.{path}")!;
            using var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string json = reader.ReadToEnd();
            return json;
        }
        private static int parseHex(string s) { // Parses "0x1234" to 4660
            return int.Parse(s.Substring(2), System.Globalization.NumberStyles.HexNumber);
        }
        public static class Entrances {
            private static Dictionary<int, Entrance> allEntrances = null;
            private static void Load() {
                allEntrances = new Dictionary<int, Entrance>();
                string json = ReadEmbeddedData("data.entrances.jsonc");
                var res = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                MelonLogger.Msg($"Loaded {res.Count} entrances");
                foreach (var (idStr, roomAndType) in res) {
                    int id = parseHex(idStr);
                    byte roomId = (byte)parseHex(roomAndType[0]);
                    if (!Enum.TryParse(roomAndType[1], out EntranceType entranceType)) throw new Exception($"Invalid entrance type {roomAndType[1]}");
                    allEntrances[id] = new Entrance(id, roomId, entranceType);
                }
            }
            // Gets the contents of the entrances.jsonc file
            // Returns a dictionary of entrance IDs to Entrance objects
            public static Dictionary<int, Entrance> Contents {
                get {
                    if (allEntrances == null) Load();
                    // Return a copy of the dictionary, so seperate calls don't interfere with each other
                    return new Dictionary<int, Entrance>(allEntrances);
                }
            }
        }
        public static class Rooms {
            private static List<Room> allRooms = null;
            private static void Load() { // Read room data
                string json = ReadEmbeddedData("data.rooms.jsonc");
                var res = JsonConvert.DeserializeObject<Dictionary<string, SerializedRoom>>(json);
                MelonLogger.Msg($"Loaded {res.Count} rooms");
                allRooms = new List<Room>();
                foreach (var (id, room) in res) {
                    List<Connection> connections = new List<Connection>();
                    foreach (var connection in room.connections) {
                        string entranceId = connection[0].ToString();
                        string exitOrObjective = connection[1].ToString();

                        Entrance entrance = Entrances.Contents[parseHex(entranceId)];
                        Entrance exit = null; ;
                        Objective objective = Objective.None;
                        if (!Enum.TryParse(exitOrObjective, out objective)) {
                            exit = Entrances.Contents[parseHex(exitOrObjective)];
                        }
                        List<List<Requirement>> requirements = null;
                        if (connection[2] != null) {
                            requirements = new List<List<Requirement>>();
                            foreach (var arr in (Newtonsoft.Json.Linq.JArray)connection[2]) {
                                List<Requirement> reqs = new List<Requirement>();
                                foreach (var reqStr in (Newtonsoft.Json.Linq.JArray)arr) {
                                    if (!Enum.TryParse(reqStr.ToString(), out Requirement requirement)) throw new Exception($"Invalid requirement {reqStr}");
                                    reqs.Add(requirement);
                                }
                                requirements.Add(reqs);
                            }
                        }
                        connections.Add(new Connection(entrance, exit, requirements) {
                            obj = objective,
                        });
                    }
                    allRooms.Add(new Room {
                        id = (byte)parseHex(id),
                        canMap = room.canMap,
                        bossRoom = room.bossRoom,
                        hasWarp = room.hasWarp,
                        isStartRoom = room.isStartRoom,
                        entrances = room.entrances.Select(e => Entrances.Contents[parseHex(e)]).ToList(), // fetch the entrances by id
                        connections = connections,
                    });
                }
            }
            // Gets the contents of the rooms.jsonc file
            public static List<Room> Contents {
                get {
                    if (allRooms == null) Load();
                    // Return a copy of the list, so seperate calls don't interfere with each other
                    return new List<Room>(allRooms);
                }
            }
        }
    }
}

public class SerializedEntrancePair {
    public int entrance { get; set; }
    public int couple { get; set; }
}
public class SerializedRoom {
    public List<String> entrances { get; set; } // List of hexadecimal entrance ids
                                                // This is [string, string, List<List< string(Requirement) >>][]
    public List<List<object>> connections { get; set; }
    public byte id { get; set; } = 0x00;
    public bool canMap { get; set; } = true;
    public bool bossRoom { get; set; } = false;
    public bool hasWarp { get; set; } = false;
    public bool isStartRoom { get; set; } = false;
}
