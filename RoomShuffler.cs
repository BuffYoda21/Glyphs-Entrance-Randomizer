using System.Collections.Generic;
using Il2CppSystem.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace GlyphsEntranceRando {
    public class RoomShuffler {
        public static bool Shuffle() {
            ResetState();
            Queue<Entrance> toExplore = new Queue<Entrance>();
            HashSet<Connection> insufficentRequirements = new HashSet<Connection>();
            CacheRooms();
            SortEntrances();
            bool endVisited = false;
            bool goal = false;
            toExplore.Enqueue(allEntrances[0x0001]);
            while (toExplore.Count > 0 && !goal) {
                MelonLogger.Msg($"{toExplore.Count} entrances to explore");
                currentEntrance = toExplore.Dequeue();
                if (currentEntrance.couple == null) {
                    PairEntrance(currentEntrance);
                    if (currentEntrance == null) {
                        MelonLogger.Error($"Failed to pair entrance {currentEntrance.id}");
                        return false;
                    }
                }
                List<Connection> shuffledConnections = new List<Connection>(allRooms[currentEntrance.couple.roomId].connections);
                ShuffleList(shuffledConnections);
                foreach (Connection c in shuffledConnections) {
                    if (c.enter.id != currentEntrance.couple.id) continue;   //we didnt enter through this entrance so ignore
                    if (CheckConnection(c))
                        toExplore.Enqueue(c.exit);
                    else if (c.exit != null)
                        insufficentRequirements.Add(c);
                }
                foreach (Connection c in insufficentRequirements) {
                    if (CheckConnection(c))
                        toExplore.Enqueue(c.exit);
                }
                if (allEntrances[0x0011].couple != null)
                    endVisited = true;
                if (endVisited && HasReq(Requirement.ConstructDefeat))
                    goal = true;
            }
            if (goal) {
                bool incomplete = false;
                if (!PairRemainingEntrances())
                    incomplete = true;
                int unpairedCount = 0;
                foreach (var (_, e) in allEntrances) {
                    if (e.couple == null)
                        unpairedCount++;
                }
                if (unpairedCount > 0)
                    MelonLogger.Error($"{unpairedCount} entrances are unpaired!");
                else
                    MelonLogger.Msg("All entrances are paired.");
                List<SerializedEntrancePair> pairs = new List<SerializedEntrancePair>();
                foreach (var (_, e) in allEntrances) {
                    if (e.couple == null) continue;
                    pairs.Add(new SerializedEntrancePair { entrance = e.id, couple = e.couple.id });
                }
                string json = JsonConvert.SerializeObject(pairs, Formatting.Indented);
                string userDataDir = MelonEnvironment.UserDataDirectory;
                string savePath = Path.Combine(userDataDir, "RandomizationResults.json");
                File.WriteAllText(savePath, json);
                if (!incomplete)
                    return true;
                return false;
            } else {
                MelonLogger.Error("Randomization Failed. Outputting partial results.");
                MelonLogger.Msg($"Sword: {HasReq(Requirement.Sword)}, Construct: {HasReq(Requirement.ConstructDefeat)}");
                List<SerializedEntrancePair> pairs = new List<SerializedEntrancePair>();
                foreach (var (_, e) in allEntrances) {
                    if (e.couple == null) continue;
                    pairs.Add(new SerializedEntrancePair { entrance = e.id, couple = e.couple.id });
                }
                string json = JsonConvert.SerializeObject(pairs, Formatting.Indented);
                string userDataDir = MelonEnvironment.UserDataDirectory;
                string savePath = Path.Combine(userDataDir, "RandomizationResults.json");
                File.WriteAllText(savePath, json);
                return false;
            }
        }

        private static void ResetState() {
            allEntrances.Clear();
            allRooms.Clear();
            leftEntrances.Clear();
            rightEntrances.Clear();
            topEntrances.Clear();
            bottomEntrances.Clear();
            uncheckedEntrances.Clear();
            knownObjectives.Clear();
            inventory.Clear();
            counters = new InventoryCounters();
        }

        private static bool CheckConnection(Connection c)   //returns true if a new entrance should be added to toExplore
        {

            if (c.obj != Objective.None)   //is this connection connecting to an objective?
            {
                bool collected = false;
                foreach (CollectedObjective co in inventory) {
                    collected = c.obj == co.obj && c.enter.roomId == co.rm;
                    if (collected) break;
                }
                if (collected) return false;     //this objective is already collected
                if (TryCollectObjective(c)) {
                    for (int i = 0; i < knownObjectives.Count; i++) {
                        UncollectedObjective o = knownObjectives[i];
                        if (o.obj == c.obj && o.rm == c.enter.roomId) {
                            knownObjectives.RemoveAt(i);
                            break;
                        }
                    }
                } else {
                    bool newConnectionMade = false;
                    for (int i = 0; i < knownObjectives.Count; i++) {
                        UncollectedObjective o = knownObjectives[i];
                        if (o.obj == c.obj && o.rm == c.enter.roomId && !o.connections.Contains(c)) {
                            o.connections.Add(c);
                            newConnectionMade = true;
                            break;
                        }
                    }
                    if (!newConnectionMade) {
                        knownObjectives.Add(new UncollectedObjective {
                            obj = c.obj,
                            rm = c.enter.roomId,
                            connections = new List<Connection> { c },
                        });
                    }
                }
            } else    //this connection must be connecting to another entrance
              {
                if (c.exit.couple != null) return false;    //this entrance is already coupled meaning we visited it already so ignore
                bool reqMet = false;
                if (c.requirements == null)
                    reqMet = true;
                else {
                    foreach (List<Requirement> list in c.requirements) {
                        reqMet = true;
                        foreach (Requirement req in list) {
                            if (!HasReq(req)) {
                                reqMet = false;
                                break;
                            }
                        }
                        if (reqMet) {
                            break;
                        }
                    }
                }
                if (reqMet)
                    return true;
            }
            return false;
        }

        private static bool TryCollectObjective(Connection c) {
            if (c.obj == Objective.None) {
                MelonLogger.Error($"entrance {c.exit.id} is an entrance not an objective.");
                return false;
            }
            bool collected = false;
            if (c.requirements == null) {
                collected = true;
            } else {
                foreach (List<Requirement> list in c.requirements) {
                    collected = true;
                    foreach (Requirement req in list) {
                        if (!HasReq(req)) {
                            collected = false;
                            break;
                        }
                    }
                    if (collected) break;
                }
            }
            if (collected) {
                if ((int)c.obj >= 0x01 && (int)c.obj <= 0x12)   //this objective is a counter
                {
                    switch (c.obj) {
                        case Objective.SilverShard: counters.silverShard++; break;
                        case Objective.GoldShard: counters.goldShard++; break;
                        case Objective.SmileToken: counters.smileToken++; break;
                        case Objective.RuneCube: counters.runeCube++; break;
                        case Objective.VoidGateShard: counters.voidGateShard++; break;
                        case Objective.Sigil: counters.sigil++; break;
                        case Objective.Glyphstone: counters.glyphstone++; break;
                        case Objective.SerpentLock: counters.serpentLock++; break;
                        case Objective.WallJump: counters.wallJump++; break;
                        case Objective.Seeds: counters.seeds++; break;
                    }
                } else    //standard objective
                  {
                    inventory.Add(new CollectedObjective {
                        obj = c.obj,
                        rm = c.enter.roomId,
                    });
                }
            }
            return collected;
        }

        private static bool HasReq(Requirement req) {
            if ((int)req >= 0x01 && (int)req <= 0x12)   //this requirement is a counter
            {
                switch (req) {
                    case Requirement.SilverShardx15: return counters.silverShard >= 15;
                    case Requirement.GoldShardx1: return counters.goldShard >= 1;
                    case Requirement.GoldShardx2: return counters.goldShard >= 2;
                    case Requirement.GoldShardx3: return counters.goldShard >= 3;
                    case Requirement.SmileTokenx2: return counters.smileToken >= 2;
                    case Requirement.SmileTokenx4: return counters.smileToken >= 4;
                    case Requirement.SmileTokenx6: return counters.smileToken >= 6;
                    case Requirement.SmileTokenx8: return counters.smileToken >= 8;
                    case Requirement.SmileTokenx10: return counters.smileToken >= 10;
                    case Requirement.RuneCubex3: return counters.runeCube >= 3;
                    case Requirement.VoidGateShardx7: return counters.voidGateShard >= 7;
                    case Requirement.Sigilx3: return counters.sigil >= 3;
                    case Requirement.Glyphstonex3: return counters.glyphstone >= 3;
                    case Requirement.SerpentLockx4: return counters.serpentLock >= 4;
                    case Requirement.WallJumpx1: return counters.wallJump >= 1;
                    case Requirement.WallJumpx2: return counters.wallJump >= 2;
                    case Requirement.Seedsx10: return counters.seeds >= 10;
                }
                return false;
            } else    //standard requirement
              {
                foreach (CollectedObjective cobj in inventory) {
                    if ((int)req == (int)cobj.obj)
                        return true;
                }
                return false;
            }
        }

        private static Entrance PairEntrance(Entrance e) {
            Entrance pairing;
            int rand;
            if (e.couple != null)
                return e.couple;
            switch (e.type) {
                case EntranceType.Left:
                    if (rightEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, rightEntrances.Count);
                    pairing = rightEntrances[rand];
                    rightEntrances.RemoveAt(rand);
                    e.couple = pairing;
                    pairing.couple = e;
                    for (int i = 0; i < leftEntrances.Count; i++) {
                        if (e.id == leftEntrances[i].id) {
                            leftEntrances.RemoveAt(i);
                            break;
                        }
                    }
                    VerifyEntrancePairings();
                    return pairing;
                case EntranceType.Right:
                    if (leftEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, leftEntrances.Count);
                    pairing = leftEntrances[rand];
                    leftEntrances.RemoveAt(rand);
                    e.couple = pairing;
                    pairing.couple = e;
                    for (int i = 0; i < rightEntrances.Count; i++) {
                        if (e.id == rightEntrances[i].id) {
                            rightEntrances.RemoveAt(i);
                            break;
                        }
                    }
                    VerifyEntrancePairings();
                    return pairing;
                case EntranceType.Top:
                    if (bottomEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, bottomEntrances.Count);
                    pairing = bottomEntrances[rand];
                    bottomEntrances.RemoveAt(rand);
                    e.couple = pairing;
                    pairing.couple = e;
                    for (int i = 0; i < topEntrances.Count; i++) {
                        if (e.id == topEntrances[i].id) {
                            topEntrances.RemoveAt(i);
                            break;
                        }
                    }
                    VerifyEntrancePairings();
                    return pairing;
                case EntranceType.Bottom:
                    if (topEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, topEntrances.Count);
                    pairing = topEntrances[rand];
                    topEntrances.RemoveAt(rand);
                    e.couple = pairing;
                    pairing.couple = e;
                    for (int i = 0; i < bottomEntrances.Count; i++) {
                        if (e.id == bottomEntrances[i].id) {
                            bottomEntrances.RemoveAt(i);
                            break;
                        }
                    }
                    VerifyEntrancePairings();
                    return pairing;
            }
            return null;
        }

        public static bool PairRemainingEntrances() {
            VerifyEntrancePairings();
            while (rightEntrances.Count > 0) {
                if (rightEntrances[0].couple != null) {
                    rightEntrances.RemoveAt(0);
                    continue;
                }
                if (PairEntrance(rightEntrances[0]) == null) {
                    MelonLogger.Error($"Failed to pair entrance {rightEntrances[0].id} at the end of randomization");
                    return false;
                }
            }
            while (topEntrances.Count > 0) {
                if (topEntrances[0].couple != null) {
                    topEntrances.RemoveAt(0);
                    continue;
                }
                if (PairEntrance(topEntrances[0]) == null) {
                    MelonLogger.Error($"Failed to pair entrance {topEntrances[0].id} at the end of randomization");
                    return false;
                }
            }
            VerifyEntrancePairings();
            if (leftEntrances.Count > 0 || bottomEntrances.Count > 0) {
                MelonLogger.Error("Some entrances failed to pair");
                return false;
            }
            return true;
        }

        private static void VerifyEntrancePairings() {
            int i = 0;
            while (true) {
                if (i >= rightEntrances.Count)
                    break;
                if (rightEntrances[i].couple != null)
                    rightEntrances.RemoveAt(i);
                else
                    i++;
            }
            i = 0;
            while (true) {
                if (i >= leftEntrances.Count)
                    break;
                if (leftEntrances[i].couple != null)
                    leftEntrances.RemoveAt(i);
                else
                    i++;
            }
            i = 0;
            while (true) {
                if (i >= topEntrances.Count)
                    break;
                if (topEntrances[i].couple != null)
                    topEntrances.RemoveAt(i);
                else
                    i++;
            }
            i = 0;
            while (true) {
                if (i >= bottomEntrances.Count)
                    break;
                if (bottomEntrances[i].couple != null)
                    bottomEntrances.RemoveAt(i);
                else
                    i++;
            }
        }

        /*
            * This method is responsible for defining the rooms and their connections in the game.
            * It initializes a list of rooms, each with its own unique ID, entrances, and connections.
            * The rooms are defined with various logical requirements to traverse from one point of the room to another.
            * Room IDs can be found using this map: https://docs.google.com/drawings/d/1DluHagwEgCopeYC3MONZ8b0qSep82Xakf4qVV6wx7fE/edit?usp=sharing
        */
        private static void CacheRooms() {
            foreach (Entrance e in new List<Entrance> {
                new Entrance(0x0000, 0x00, EntranceType.Bottom),
                new Entrance(0x0001, 0x01, EntranceType.Right),
                new Entrance(0x0002, 0x02, EntranceType.Left),
                new Entrance(0x0003, 0x02, EntranceType.Right),
                new Entrance(0x0004, 0x03, EntranceType.Left),
                new Entrance(0x0005, 0x03, EntranceType.Right),
                new Entrance(0x0006, 0x04, EntranceType.Left),
                new Entrance(0x0007, 0x04, EntranceType.Right),
                new Entrance(0x0008, 0x04, EntranceType.Bottom),
                new Entrance(0x0009, 0x05, EntranceType.Left),
                new Entrance(0x000A, 0x05, EntranceType.Right),
                new Entrance(0x000B, 0x06, EntranceType.Left),
                new Entrance(0x000C, 0x06, EntranceType.Right),
                new Entrance(0x000D, 0x06, EntranceType.Top),
                new Entrance(0x000E, 0x07, EntranceType.Left),
                new Entrance(0x000F, 0x07, EntranceType.Right),
                new Entrance(0x0010, 0x07, EntranceType.Bottom),
                new Entrance(0x0011, 0x08, EntranceType.Left),
                new Entrance(0x0012, 0x09, EntranceType.Right),
                new Entrance(0x0013, 0x0A, EntranceType.Left), // to shard room
                new Entrance(0x0014, 0x0A, EntranceType.Left), // to smile token room
                new Entrance(0x0015, 0x0A, EntranceType.Right), // to map room
                new Entrance(0x0016, 0x0A, EntranceType.Right), // flower passage
                new Entrance(0x0017, 0x0A, EntranceType.Right), // towards construct
                new Entrance(0x0018, 0x0B, EntranceType.Left),
                new Entrance(0x0019, 0x0B, EntranceType.Right),
                new Entrance(0x001A, 0x0C, EntranceType.Left),
                new Entrance(0x001B, 0x0C, EntranceType.Right),
                new Entrance(0x001C, 0x0C, EntranceType.Bottom),
                new Entrance(0x001D, 0x0D, EntranceType.Left),
                new Entrance(0x001E, 0x0D, EntranceType.Right),
                new Entrance(0x001F, 0x0D, EntranceType.Top),
                new Entrance(0x0020, 0x0E, EntranceType.Left),
                new Entrance(0x0021, 0x0F, EntranceType.Top),
                new Entrance(0x0022, 0x10, EntranceType.Right),
                new Entrance(0x0023, 0x10, EntranceType.Bottom),
                new Entrance(0x0024, 0x11, EntranceType.Left),
                new Entrance(0x0025, 0x11, EntranceType.Top),
                new Entrance(0x0026, 0x12, EntranceType.Bottom),
                new Entrance(0x0027, 0x13, EntranceType.Right),
                new Entrance(0x0028, 0x14, EntranceType.Left),
                new Entrance(0x0029, 0x14, EntranceType.Right),
                new Entrance(0x002A, 0x14, EntranceType.Top),
                new Entrance(0x002B, 0x15, EntranceType.Left),
                new Entrance(0x002C, 0x15, EntranceType.Right),
                new Entrance(0x002D, 0x16, EntranceType.Left),
                new Entrance(0x002E, 0x16, EntranceType.Top),
                new Entrance(0x002F, 0x16, EntranceType.Bottom),
                new Entrance(0x0030, 0x17, EntranceType.Left),
                new Entrance(0x0031, 0x17, EntranceType.Right),
                new Entrance(0x0032, 0x18, EntranceType.Left),
                new Entrance(0x0033, 0x18, EntranceType.Right),
                new Entrance(0x0034, 0x19, EntranceType.Left),
                new Entrance(0x0035, 0x19, EntranceType.Top),
            }) {
                allEntrances[e.id] = e;
            }
            allRooms = new List<Room>
            {
                new Room()
                {
                    id = 0x00,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0000], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0000], allEntrances[0x0000], null),
                        new Connection(allEntrances[0x0000], Objective.SilverShard, null)
                    },
                },
                new Room()
                {
                    id = 0x01,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0001], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0001], allEntrances[0x0001], null),
                    },
                    isStartRoom = true,
                },
                new Room()
                {
                    id = 0x02,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0002], //left
                        allEntrances[0x0003], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0002], allEntrances[0x0002], null),
                        new Connection(allEntrances[0x0003], allEntrances[0x0003], null),
                        new Connection(allEntrances[0x0002], allEntrances[0x0003], null),
                        new Connection(allEntrances[0x0003], allEntrances[0x0002], null),
                    },
                },
                new Room()
                {
                    id = 0x03,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0004], //left
                        allEntrances[0x0005], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0004], allEntrances[0x0004], null),
                        new Connection(allEntrances[0x0005], allEntrances[0x0005], null),
                        new Connection(allEntrances[0x0004], allEntrances[0x0005], null),
                        new Connection(allEntrances[0x0005], allEntrances[0x0004], null),
                        new Connection(allEntrances[0x0004], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0005], Objective.SaveButton, null),
                    },
                    hasWarp = true,
                },
                new Room()
                {
                    id = 0x04,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0006], //left
                        allEntrances[0x0007], //right
                        allEntrances[0x0008], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0006], allEntrances[0x0006], null),
                        new Connection(allEntrances[0x0007], allEntrances[0x0007], null),
                        new Connection(allEntrances[0x0008], allEntrances[0x0008], null),
                        new Connection(allEntrances[0x0006], allEntrances[0x0007], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0007], allEntrances[0x0006], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0006], allEntrances[0x0008], null),
                        new Connection(allEntrances[0x0008], allEntrances[0x0006], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x0007], allEntrances[0x0008], null),
                        new Connection(allEntrances[0x0008], allEntrances[0x0007], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x05,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0009], //left
                        allEntrances[0x000A], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0009], allEntrances[0x0009], null),
                        new Connection(allEntrances[0x000A], allEntrances[0x000A], null),
                        new Connection(allEntrances[0x0009], allEntrances[0x000A], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb },
                        }),
                        new Connection(allEntrances[0x000A], allEntrances[0x0009], null),
                    },
                },
                new Room()
                {
                    id = 0x06,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x000B], //left
                        allEntrances[0x000C], //right
                        allEntrances[0x000D], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x000B], allEntrances[0x000B], null),
                        new Connection(allEntrances[0x000C], allEntrances[0x000C], null),
                        new Connection(allEntrances[0x000D], allEntrances[0x000D], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x000B], allEntrances[0x000C], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x000C], allEntrances[0x000B], null),
                        new Connection(allEntrances[0x000B], allEntrances[0x000D], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x000D], allEntrances[0x000B], null),
                        new Connection(allEntrances[0x000C], allEntrances[0x000D], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x000D], allEntrances[0x000C], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    },
                },
                new Room()
                {
                    id = 0x07,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x000E], //left
                        allEntrances[0x000F], //right
                        allEntrances[0x0010], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x000E], allEntrances[0x000E], null),
                        new Connection(allEntrances[0x000F], allEntrances[0x000F], null),
                        new Connection(allEntrances[0x0010], allEntrances[0x0010], null),
                        new Connection(allEntrances[0x000E], allEntrances[0x000F], null),
                        new Connection(allEntrances[0x000F], allEntrances[0x000E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x000E], allEntrances[0x0010], null),
                        new Connection(allEntrances[0x0010], allEntrances[0x000E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x000F], allEntrances[0x0010], null),
                        new Connection(allEntrances[0x0010], allEntrances[0x000F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                    },
                },
                new Room()
                {
                    id = 0x08,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0011], //left
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0011], allEntrances[0x0011], null),
                        new Connection(allEntrances[0x0011], Objective.SaveButton, null),
                    },
                    hasWarp = true,
                },
                new Room()
                {
                    id = 0x09,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0012], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0012], allEntrances[0x0012], null),
                        new Connection(allEntrances[0x0012], Objective.SilverShard, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    },
                },
                new Room()
                {
                    id = 0x0A,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0013], //left to shard room
                        allEntrances[0x0014], //left to smile token room
                        allEntrances[0x0015], //right to map room
                        allEntrances[0x0016], //right to flower passage
                        allEntrances[0x0017], //right towards construct
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0013], allEntrances[0x0013], null),
                        new Connection(allEntrances[0x0014], allEntrances[0x0014], null),
                        new Connection(allEntrances[0x0015], allEntrances[0x0015], null),
                        new Connection(allEntrances[0x0016], allEntrances[0x0016], null),
                        new Connection(allEntrances[0x0017], allEntrances[0x0017], null),
                        new Connection(allEntrances[0x0013], allEntrances[0x0014], null),
                        new Connection(allEntrances[0x0014], allEntrances[0x0013], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0013], allEntrances[0x0015], null),
                        new Connection(allEntrances[0x0015], allEntrances[0x0013], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0013], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0013], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0013], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0013], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0014], allEntrances[0x0015], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0015], allEntrances[0x0014], null),
                        new Connection(allEntrances[0x0014], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0014], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0015], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0015], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0015], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0015], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0013], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0014], Objective.SaveButton, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0015], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0016], Objective.SaveButton, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0017], Objective.SaveButton, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    },
                    hasWarp = true,
                },
                new Room()
                {
                    id = 0x0B,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0018], //left
                        allEntrances[0x0019], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0018], allEntrances[0x0018], null),
                        new Connection(allEntrances[0x0019], allEntrances[0x0019], null),
                        new Connection(allEntrances[0x0018], allEntrances[0x0018], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Map }
                        }),
                        new Connection(allEntrances[0x0018], Objective.Map, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0019], Objective.Map, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.Map }
                        }),
                    },
                },
                new Room() {
                    id = 0x0C,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x001A], //left
                        allEntrances[0x001B], //right
                        allEntrances[0x001C], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x001A], allEntrances[0x001A], null),
                        new Connection(allEntrances[0x001B], allEntrances[0x001B], null),
                        new Connection(allEntrances[0x001C], allEntrances[0x001C], null),
                        new Connection(allEntrances[0x001A], allEntrances[0x001B], null),
                        new Connection(allEntrances[0x001B], allEntrances[0x001A], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x001A], allEntrances[0x001C], null),
                        new Connection(allEntrances[0x001C], allEntrances[0x001A], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x001B], allEntrances[0x001C], null),
                        new Connection(allEntrances[0x001C], allEntrances[0x001B], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb },
                            new List<Requirement> { Requirement.VerticalMomentum },
                        }),
                    }
                },
                new Room()
                {
                    id = 0x0D,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x001D], //left
                        allEntrances[0x001E], //right
                        allEntrances[0x001F], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x001D], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001E], allEntrances[0x001E], null),
                        new Connection(allEntrances[0x001F], allEntrances[0x001F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001D], allEntrances[0x001E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001E], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001D], allEntrances[0x001F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001F], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001E], allEntrances[0x001F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x001F], allEntrances[0x001E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x001D], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001E], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001F], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x0E,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0020], //left
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0020], allEntrances[0x0020], null),
                    }
                },
                new Room()
                {
                    id = 0x0F,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0021], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0021], Objective.SilverShard, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x10,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0022], //right
                        allEntrances[0x0023], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0023], allEntrances[0x0023], null),
                        new Connection(allEntrances[0x0022], allEntrances[0x0023], null),
                    }
                },
                new Room()
                {
                    id = 0x11,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0024], //left
                        allEntrances[0x0025], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0024], allEntrances[0x0024], null),
                        new Connection(allEntrances[0x0025], allEntrances[0x0025], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0024], allEntrances[0x0025], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0025], allEntrances[0x0024], null),
                    }
                },
                new Room() {
                    id = 0x12,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0026], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0026], allEntrances[0x0026], null),
                        new Connection(allEntrances[0x0026], Objective.RuneCube, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x13,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0027], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0027], allEntrances[0x0027], null),
                        new Connection(allEntrances[0x0027], Objective.SmileToken, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x14,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0028], //left
                        allEntrances[0x0029], //right
                        allEntrances[0x002A], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0028], allEntrances[0x0028], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0029], allEntrances[0x0029], null),
                        new Connection(allEntrances[0x0028], allEntrances[0x0029], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0029], allEntrances[0x0028], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002A], allEntrances[0x0028], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002A], allEntrances[0x0029], null),
                    }
                },
                new Room()
                {
                    id = 0x15,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x002B], //left
                        allEntrances[0x002C], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x002B], allEntrances[0x002B], null),
                        new Connection(allEntrances[0x002C], allEntrances[0x002C], null),
                        new Connection(allEntrances[0x002B], allEntrances[0x002C], null),
                        new Connection(allEntrances[0x002C], allEntrances[0x002B], null),
                        new Connection(allEntrances[0x002B], Objective.Sword, null),
                        new Connection(allEntrances[0x002C], Objective.Sword, null),
                    }
                },
                new Room()
                {
                    id = 0x16,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x002D], //left
                        allEntrances[0x002E], //top
                        allEntrances[0x002F], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x002D], allEntrances[0x002D], null),
                        new Connection(allEntrances[0x002E], allEntrances[0x002E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x002F], allEntrances[0x002F], null),
                        new Connection(allEntrances[0x002D], allEntrances[0x002E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x002E], allEntrances[0x002D], null),
                        new Connection(allEntrances[0x002D], allEntrances[0x002F], null),
                        new Connection(allEntrances[0x002F], allEntrances[0x002D], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum },
                            new List<Requirement> { Requirement.DashOrb },
                        }),
                        new Connection(allEntrances[0x002E], allEntrances[0x002F], null),
                        new Connection(allEntrances[0x002F], allEntrances[0x002E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x002F], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x002E], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x002F], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x17,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0030], //Left
                        allEntrances[0x0031], //Right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0030], allEntrances[0x0030], null),
                        new Connection(allEntrances[0x0031], allEntrances[0x0031], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0030], allEntrances[0x0031], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0031], allEntrances[0x0030], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.ConstructDefeat }
                        }),
                    }
                },
                new Room()
                {
                    id = 0x18,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0032], //Left
                        allEntrances[0x0033], //Right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0032], allEntrances[0x0032], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0033], allEntrances[0x0033], null),
                        new Connection(allEntrances[0x0032], allEntrances[0x0033], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0033], allEntrances[0x0032], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0032], Objective.DashOrb, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0033], Objective.DashOrb, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0033], Objective.ConstructDefeat, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.Sword },
                            new List<Requirement> { Requirement.DashOrb, Requirement.DashAttackOrb },
                        }),
                    }
                },
                new Room()
                {
                    id = 0x19,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0034], //Left
                        allEntrances[0x0035], //Top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0034], allEntrances[0x0034], null),
                        new Connection(allEntrances[0x0035], allEntrances[0x0035], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0034], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0035], Objective.SaveButton, null),
                    },
                    hasWarp = true,
                }
            };
        }

        private static void SortEntrances() {
            foreach (var (_, e) in allEntrances) {
                switch (e.type) {
                    case EntranceType.Left: leftEntrances.Add(e); break;
                    case EntranceType.Right: rightEntrances.Add(e); break;
                    case EntranceType.Top: topEntrances.Add(e); break;
                    case EntranceType.Bottom: bottomEntrances.Add(e); break;
                }
            }
            ShuffleList(leftEntrances);
            ShuffleList(rightEntrances);
            ShuffleList(topEntrances);
            ShuffleList(bottomEntrances);
        }

        private static void ShuffleList<T>(List<T> list) {
            for (int i = 0; i < list.Count; i++) {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }

        public static List<Room> allRooms = new List<Room>();
        public static Dictionary<int, Entrance> allEntrances = new Dictionary<int, Entrance>();
        public static List<Entrance> rightEntrances = new List<Entrance>();
        public static List<Entrance> leftEntrances = new List<Entrance>();
        public static List<Entrance> topEntrances = new List<Entrance>();
        public static List<Entrance> bottomEntrances = new List<Entrance>();

        public static List<List<Entrance>> uncheckedEntrances = new List<List<Entrance>>();
        public static List<UncollectedObjective> knownObjectives = new List<UncollectedObjective>();
        public static List<CollectedObjective> inventory = new List<CollectedObjective>();
        public static InventoryCounters counters = new InventoryCounters();
        public static Entrance currentEntrance;

        public class UncollectedObjective {
            public Objective obj;
            public byte rm;
            public List<Connection> connections;
        }

        public class CollectedObjective {
            public Objective obj;
            public byte rm;
        }

        public class SerializedEntrancePair {
            public ushort entrance { get; set; }
            public ushort couple { get; set; }
        }

        public class InventoryCounters {
            public int silverShard = 0;
            public int goldShard = 0;
            public int smileToken = 0;
            public int runeCube = 0;
            public int voidGateShard = 0;
            public int sigil = 0;
            public int glyphstone = 0;
            public int serpentLock = 0;
            public int wallJump = 0;
            public int seeds = 0;
        }
    }
}
