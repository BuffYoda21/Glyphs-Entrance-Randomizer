using System.Collections.Generic;
using System;
using System.Linq;
using MelonLoader;

namespace GlyphsEntranceRando {
    public class RoomShuffler {
        public static bool Shuffle() {
            ResetState();
            Queue<Entrance> toExplore = new Queue<Entrance>();
            HashSet<Connection> insufficentRequirements = new HashSet<Connection>();

            Dictionary<int, Entrance> allEntrances = Resources.Entrances.Contents;
            SortEntrances(allEntrances.Values.ToList());

            bool goal = false;
            toExplore.Enqueue(Resources.Entrances.StartingEntrance);
            while (!goal && toExplore.TryDequeue(out Entrance currentEntrance)) {
                //MelonLogger.Msg($"{toExplore.Count + 1} entrances to explore");
                Entrance couple;
                if (!coupledEntrances.TryGetValue(currentEntrance, out couple)) {
                    couple = PairEntrance(currentEntrance);
                    if (couple == null) {
                        MelonLogger.Error($"Failed to pair entrance {currentEntrance.id}");
                        return false;
                    }
                }
                List<Room> allRooms = Resources.Rooms.Contents;
                List<Connection> shuffledConnections = new List<Connection>(allRooms[couple.roomId].connections)
                  .Where(c => c.enter.id == couple.id) // Only connections that enter through the current entrance
                  .ToList();
                ShuffleList(shuffledConnections);
                foreach (Connection c in shuffledConnections) {
                    if (CheckConnection(c))
                        toExplore.Enqueue(c.exit);
                    else if (c.exit != null)
                        insufficentRequirements.Add(c);
                }
                foreach (Connection c in insufficentRequirements.Where(CheckConnection)) {
                    toExplore.Enqueue(c.exit);
                }

                bool endVisited = coupledEntrances.ContainsKey(Resources.Entrances.EndingEntrance);
                goal = endVisited && HasReq(Requirement.ConstructDefeat);
            }
            bool success = goal;
            if (goal) {
                success = PairRemainingEntrances();
                int unpairedCount = allEntrances.Values.Count(e => !coupledEntrances.ContainsKey(e));
                if (unpairedCount > 0)
                    MelonLogger.Error($"{unpairedCount} entrances are unpaired!");
                else
                    MelonLogger.Msg("All entrances are paired.");
                Resources.ResultsJSON.Contents = allEntrances.Values
                    .Where(e => coupledEntrances.ContainsKey(e))
                    .Select(e => new SerializedEntrancePair { entrance = e.id, couple = coupledEntrances[e].id })
                    .ToList();
            } //else {
              //MelonLogger.Error("Randomization Failed. Outputting partial results.");
              //MelonLogger.Msg($"Sword: {HasReq(Requirement.Sword)}, Construct: {HasReq(Requirement.ConstructDefeat)}");
              //}
            return success;
        }

        private static void ResetState() {
            leftEntrances.Clear();
            rightEntrances.Clear();
            topEntrances.Clear();
            bottomEntrances.Clear();
            coupledEntrances.Clear();
            knownObjectives.Clear();
            inventory.Clear();
            counters = new InventoryCounters();
        }

        private static bool CheckConnection(Connection c) { //returns true if a new entrance should be added to toExplore
            if (c.obj != Objective.None) { //is this connection connecting to an objective?
                bool collected = inventory.Any(co => c.obj == co.obj && c.enter.roomId == co.rm);
                if (collected) return false;     //this objective is already collected
                if (TryCollectObjective(c)) {
                    knownObjectives.RemoveAll(o => o.obj == c.obj && o.rm == c.enter.roomId);
                } else {
                    UncollectedObjective o = knownObjectives.Find(o => o.obj == c.obj && o.rm == c.enter.roomId && !o.connections.Contains(c));
                    if (o != null) {
                        o.connections.Add(c);
                    } else {
                        knownObjectives.Add(new UncollectedObjective {
                            obj = c.obj,
                            rm = c.enter.roomId,
                            connections = new List<Connection> { c },
                        });
                    }
                }
                return false;
            } else { //this connection must be connecting to another entrance
                if (coupledEntrances.ContainsKey(c.exit)) return false;    //this entrance is already coupled meaning we visited it already so ignore
                bool reqMet = c.requirements == null || c.requirements.Any(list => list.All(HasReq));
                return reqMet;
            }
        }

        private static bool TryCollectObjective(Connection c) {
            if (c.obj == Objective.None) {
                MelonLogger.Error($"entrance {c.exit.id} is an entrance not an objective.");
                return false;
            }
            bool collected = c.requirements == null || c.requirements.Any(list => list.All(HasReq));
            if (collected) {
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
                    default: { //standard objective
                            inventory.Add(new CollectedObjective {
                                obj = c.obj,
                                rm = c.enter.roomId,
                            });
                            break;
                        }
                }
            }
            return collected;
        }

        private static bool HasReq(Requirement req) {
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
                default: return inventory.Any(cobj => (int)cobj.obj == (int)req); //standard requirement
            }
        }

        private static Entrance PairEntrance(Entrance e) {
            if (coupledEntrances.TryGetValue(e, out Entrance couple)) return couple;
            List<Entrance> directionToPair = null, directionToRemove = null;
            switch (e.type) {
                case EntranceType.Left:
                    directionToPair = rightEntrances;
                    directionToRemove = leftEntrances;
                    break;
                case EntranceType.Right:
                    directionToPair = leftEntrances;
                    directionToRemove = rightEntrances;
                    break;
                case EntranceType.Top:
                    directionToPair = bottomEntrances;
                    directionToRemove = topEntrances;
                    break;
                case EntranceType.Bottom:
                    directionToPair = topEntrances;
                    directionToRemove = bottomEntrances;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (directionToPair.Count <= 0) return null;
            int rand = UnityEngine.Random.Range(0, directionToPair.Count);
            Entrance pairing = directionToPair[rand];
            directionToPair.RemoveAt(rand);

            coupledEntrances[e] = pairing;
            coupledEntrances[pairing] = e;

            directionToRemove.Remove(e);
            VerifyEntrancePairings();
            return pairing;
        }

        public static bool PairRemainingEntrances() {
            VerifyEntrancePairings();
            // PairEntrance calls VerifyEntrancePairings, which will remove paired entrances from the lists, so the while loops will exit
            while (rightEntrances.Count > 0) {
                if (PairEntrance(rightEntrances[0]) == null) {
                    MelonLogger.Error($"Failed to pair entrance {rightEntrances[0].id} at the end of randomization");
                    return false;
                }
            }
            while (topEntrances.Count > 0) {
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
            rightEntrances.RemoveAll(coupledEntrances.ContainsKey);
            leftEntrances.RemoveAll(coupledEntrances.ContainsKey);
            topEntrances.RemoveAll(coupledEntrances.ContainsKey);
            bottomEntrances.RemoveAll(coupledEntrances.ContainsKey);
        }

        private static void SortEntrances(List<Entrance> entrances) {
            foreach (var e in entrances) {
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

        // Used to keep track of which entrances have been paired (instead of modifying the object)
        public static Dictionary<Entrance, Entrance> coupledEntrances = new Dictionary<Entrance, Entrance>();
        public static List<Entrance> rightEntrances = new List<Entrance>();
        public static List<Entrance> leftEntrances = new List<Entrance>();
        public static List<Entrance> topEntrances = new List<Entrance>();
        public static List<Entrance> bottomEntrances = new List<Entrance>();

        public static List<UncollectedObjective> knownObjectives = new List<UncollectedObjective>();
        public static List<CollectedObjective> inventory = new List<CollectedObjective>();
        public static InventoryCounters counters = new InventoryCounters();

        public class UncollectedObjective {
            public Objective obj;
            public byte rm;
            public List<Connection> connections;
        }

        public class CollectedObjective {
            public Objective obj;
            public byte rm;
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
