using UnityEngine;
using MelonLoader;
using System.Collections.Generic;
using GlyphsEntranceRando;
using System.Data;

namespace GlyphsEntranceRando
{
    public class RoomShuffler
    {
        public static bool Shuffle()
        {
            int deadEnds = 0;
            bool stuck = false;
            Stack<Entrance> thePath = new Stack<Entrance>();
            thePath.Push(allEntrances[0x0000]);
            CacheRooms();
            SortEntrances();
            while (thePath.Peek() != allEntrances[0x0012] && deadEnds < 100) //keeps going until it leaves region1 or fails
            {
                currentRoute = new List<Entrance>();
                while (thePath.Peek() != allEntrances[0x0012] && !stuck)
                {
                    currentRoute.Add(thePath.Peek());
                    if(thePath.Peek().couple == null)
                    {
                        PairEntrance(thePath.Peek());
                        if(thePath.Peek().couple == null)
                        {
                            MelonLogger.Error($"Failed to find a pair for entrance {thePath.Peek().id}");
                            return false;
                        }
                    }
                    currentRoute.Add(thePath.Peek().couple);
                    thePath.Push(currentRoute[currentRoute.Count-1]);
                    foreach(Connection c in allRooms[thePath.Peek().roomId].connections)
                    {
                        if(c.enter.id != thePath.Peek().id) continue;   //we didnt enter through this entrance so ignore
                        if(c.obj != Objective.None)   //is this connection connecting to an objective?
                        {
                            bool collected = false;
                            foreach(CollectedObjective co in inventory)
                            {
                                collected = c.obj == co.obj && c.enter.roomId == co.rm;
                                if(collected) break;
                            }
                            if(collected) continue;     //this objective is already collected
                            if(TryCollectObjective(c))
                            {
                                for (int i = 0; i < knownObjectives.Count; i++)
                                {
                                    UncollectedObjective o = knownObjectives[i];
                                    if (o.obj == c.obj && o.rm == c.enter.roomId)
                                    {
                                        knownObjectives.RemoveAt(i);
                                        break;
                                    }
                                }
                            } 
                            else
                            {
                                bool newConnectionMade = false;
                                for (int i = 0; i < knownObjectives.Count; i++)
                                {
                                    UncollectedObjective o = knownObjectives[i];
                                    if (o.obj == c.obj && o.rm == c.enter.roomId && !o.connections.Contains(c))
                                    {
                                        o.connections.Add(c);
                                        newConnectionMade = true;
                                        break;
                                    }
                                }
                                if(!newConnectionMade)
                                {
                                    knownObjectives.Add(new UncollectedObjective
                                    {
                                        obj = c.obj,
                                        rm = c.enter.roomId,
                                        connections = new List<Connection> {c},
                                    });
                                }
                            }
                        }
                        else    //this connection must be connecting to another entrance
                        {

                        }
                    }
                }
                deadEnds++;
            }

            return false;
        }

        private static bool TryCollectObjective(Connection c)
        {
            if(c.obj == Objective.None)
            {
                MelonLogger.Error($"entrance {c.exit.id} is an entrance not an objective.");
                return false;
            }
            if(c.requirements == null)
            {
                return true;
            }
            bool collected = false;
            foreach (List<Requirement> list in c.requirements)
            {
                collected = true;
                foreach(Requirement req in list)
                {
                    if(HasReq(req))
                    {
                        collected = false;
                        break;
                    }
                }
                if(collected)   //Update logic to account for counter objectives
                {
                    inventory.Add(new CollectedObjective
                    {
                        obj = c.obj,
                        rm = c.enter.roomId,
                    });
                    break;
                }
            }
            return collected;
        }

        private static bool HasReq(Requirement req)
        {
            if ((int)req >= 0x01 && (int)req <= 0x11)   //this requirement is a counter
            {
                
            }
            else    //standard requirement
            {
                foreach (CollectedObjective cobj in inventory)
                {
                    if ((int)req == (int)cobj.obj)
                        return true;
                }
            }
        }

        private static Entrance PairEntrance(Entrance e)
        {
            Entrance pairing;
            int rand;
            switch (e.type)
            {
                case EntranceType.Left:
                    if (rightEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, rightEntrances.Count);
                    pairing = rightEntrances[rand];
                    e.couple = pairing;
                    rightEntrances.RemoveAt(rand);
                    return pairing;
                case EntranceType.Right:
                    if (leftEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, rightEntrances.Count);
                    pairing = leftEntrances[rand];
                    e.couple = pairing;
                    rightEntrances.RemoveAt(rand);
                    return pairing;
                case EntranceType.Top:
                    if (bottomEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, rightEntrances.Count);
                    pairing = bottomEntrances[rand];
                    e.couple = pairing;
                    rightEntrances.RemoveAt(rand);
                    return pairing;
                case EntranceType.Bottom:
                    if (topEntrances.Count <= 0) return null;
                    rand = UnityEngine.Random.Range(0, rightEntrances.Count);
                    pairing = topEntrances[rand];
                    e.couple = pairing;
                    rightEntrances.RemoveAt(rand);
                    return pairing;
            }
            return null;
        }

        /*
            * This method is responsible for defining the rooms and their connections in the game.
            * It initializes a list of rooms, each with its own unique ID, entrances, and connections.
            * The rooms are defined with various logical requirements to traverse from one point of the room to another.
            * Room IDs can be found using this map: https://docs.google.com/drawings/d/1DluHagwEgCopeYC3MONZ8b0qSep82Xakf4qVV6wx7fE/edit?usp=sharing
        */
        private static void CacheRooms()
        {
            allEntrances = new List<Entrance> {
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
                new Entrance(0x0012, 0x08, EntranceType.Right),
                new Entrance(0x0013, 0x09, EntranceType.Right),
                new Entrance(0x0014, 0x0A, EntranceType.Left), // to shard room
                new Entrance(0x0015, 0x0A, EntranceType.Left), // to smile token room
                new Entrance(0x0016, 0x0A, EntranceType.Right), // to map room
                new Entrance(0x0017, 0x0A, EntranceType.Right), // flower passage
                new Entrance(0x0018, 0x0A, EntranceType.Right), // towards construct
                new Entrance(0x0019, 0x0B, EntranceType.Left),
                new Entrance(0x001A, 0x0B, EntranceType.Right),
                new Entrance(0x001B, 0x0C, EntranceType.Left),
                new Entrance(0x001C, 0x0C, EntranceType.Right),
                new Entrance(0x001D, 0x0C, EntranceType.Bottom),
                new Entrance(0x001E, 0x0D, EntranceType.Left),
                new Entrance(0x001F, 0x0D, EntranceType.Right),
                new Entrance(0x0020, 0x0D, EntranceType.Top),
                new Entrance(0x0021, 0x0E, EntranceType.Left),
                new Entrance(0x0022, 0x0F, EntranceType.Top),
                new Entrance(0x0023, 0x10, EntranceType.Right),
                new Entrance(0x0024, 0x10, EntranceType.Bottom),
                new Entrance(0x0025, 0x11, EntranceType.Left),
                new Entrance(0x0026, 0x11, EntranceType.Top),
                new Entrance(0x0027, 0x12, EntranceType.Bottom),
                new Entrance(0x0028, 0x13, EntranceType.Right),
                new Entrance(0x0029, 0x14, EntranceType.Left),
                new Entrance(0x002A, 0x14, EntranceType.Right),
                new Entrance(0x002B, 0x14, EntranceType.Top),
                new Entrance(0x002C, 0x15, EntranceType.Left),
                new Entrance(0x002D, 0x15, EntranceType.Right),
                new Entrance(0x002E, 0x16, EntranceType.Left),
                new Entrance(0x002F, 0x16, EntranceType.Top),
                new Entrance(0x0030, 0x16, EntranceType.Bottom),
                new Entrance(0x0031, 0x17, EntranceType.Left),
                new Entrance(0x0032, 0x17, EntranceType.Right),
                new Entrance(0x0033, 0x18, EntranceType.Left),
                new Entrance(0x0034, 0x18, EntranceType.Right),
                new Entrance(0x0035, 0x19, EntranceType.Left),
                new Entrance(0x0036, 0x19, EntranceType.Top),
            };
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
                        allEntrances[0x0012], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0011], allEntrances[0x0011], null),
                        new Connection(allEntrances[0x0012], allEntrances[0x0012], null),
                        new Connection(allEntrances[0x0011], allEntrances[0x0012], null),
                        new Connection(allEntrances[0x0012], allEntrances[0x0011], null),
                        new Connection(allEntrances[0x0011], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0012], Objective.SaveButton, null),
                    },
                    hasWarp = true,
                },
                new Room()
                {
                    id = 0x09,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0013], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0013], allEntrances[0x0013], null),
                        new Connection(allEntrances[0x0013], Objective.SilverShard, new List<List<Requirement>>
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
                        allEntrances[0x0014], //left to shard room
                        allEntrances[0x0015], //left to smile token room
                        allEntrances[0x0016], //right to map room
                        allEntrances[0x0017], //right to flower passage
                        allEntrances[0x0018], //right towards construct
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0014], allEntrances[0x0014], null),
                        new Connection(allEntrances[0x0015], allEntrances[0x0015], null),
                        new Connection(allEntrances[0x0016], allEntrances[0x0016], null),
                        new Connection(allEntrances[0x0017], allEntrances[0x0017], null),
                        new Connection(allEntrances[0x0018], allEntrances[0x0018], null),
                        new Connection(allEntrances[0x0014], allEntrances[0x0015], null),
                        new Connection(allEntrances[0x0015], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0014], allEntrances[0x0016], null),
                        new Connection(allEntrances[0x0016], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0014], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0014], allEntrances[0x0018], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0018], allEntrances[0x0014], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0015], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0015], null),
                        new Connection(allEntrances[0x0015], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0015], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0015], allEntrances[0x0018], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0018], allEntrances[0x0015], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0016], allEntrances[0x0018], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0018], allEntrances[0x0016], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0017], allEntrances[0x0018], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x0018], allEntrances[0x0017], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0014], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0015], Objective.SaveButton, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0016], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0017], Objective.SaveButton, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0018], Objective.SaveButton, new List<List<Requirement>>
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
                        allEntrances[0x0019], //left
                        allEntrances[0x001A], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0019], allEntrances[0x0019], null),
                        new Connection(allEntrances[0x001A], allEntrances[0x001A], null),
                        new Connection(allEntrances[0x0019], allEntrances[0x001A], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Map }
                        }),
                        new Connection(allEntrances[0x0019], Objective.Map, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x001A], Objective.Map, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.Map }
                        }),
                    },
                },
                new Room() {
                    id = 0x0C,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x001B], //left
                        allEntrances[0x001C], //right
                        allEntrances[0x001D], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x001B], allEntrances[0x001B], null),
                        new Connection(allEntrances[0x001C], allEntrances[0x001C], null),
                        new Connection(allEntrances[0x001D], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001B], allEntrances[0x001C], null),
                        new Connection(allEntrances[0x001C], allEntrances[0x001B], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x001B], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001D], allEntrances[0x001B], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 }
                        }),
                        new Connection(allEntrances[0x001C], allEntrances[0x001D], null),
                        new Connection(allEntrances[0x001D], allEntrances[0x001C], new List<List<Requirement>>
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
                        allEntrances[0x001E], //left
                        allEntrances[0x001F], //right
                        allEntrances[0x0020], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x001E], allEntrances[0x001E], null),
                        new Connection(allEntrances[0x001F], allEntrances[0x001F], null),
                        new Connection(allEntrances[0x0020], allEntrances[0x0020], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001E], allEntrances[0x001F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001F], allEntrances[0x001E], null),
                        new Connection(allEntrances[0x001E], allEntrances[0x0020], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.WallJumpx1 },
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0020], allEntrances[0x001E], null),
                        new Connection(allEntrances[0x001F], allEntrances[0x0020], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0020], allEntrances[0x001F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x001E], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x001F], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x0020], Objective.VerticalMomentum, new List<List<Requirement>>
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
                        allEntrances[0x0021], //left
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0021], allEntrances[0x0021], null),
                    }
                },
                new Room()
                {
                    id = 0x0F,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0022], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0022], Objective.SilverShard, new List<List<Requirement>>
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
                        allEntrances[0x0023], //right
                        allEntrances[0x0024], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0024], allEntrances[0x0024], null),
                        new Connection(allEntrances[0x0023], allEntrances[0x0024], null),
                    }
                },
                new Room()
                {
                    id = 0x11,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0025], //left
                        allEntrances[0x0026], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0025], allEntrances[0x0025], null),
                        new Connection(allEntrances[0x0026], allEntrances[0x0026], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0025], allEntrances[0x0026], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0026], allEntrances[0x0025], null),
                    }
                },
                new Room() {
                    id = 0x12,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x0027], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0027], allEntrances[0x0027], null),
                        new Connection(allEntrances[0x0027], Objective.RuneCube, new List<List<Requirement>>
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
                        allEntrances[0x0028], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0028], allEntrances[0x0028], null),
                        new Connection(allEntrances[0x0028], Objective.SmileToken, new List<List<Requirement>>
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
                        allEntrances[0x0029], //left
                        allEntrances[0x002A], //right
                        allEntrances[0x002B], //top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0029], allEntrances[0x0029], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002A], allEntrances[0x002A], null),
                        new Connection(allEntrances[0x0029], allEntrances[0x002A], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002A], allEntrances[0x0029], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002B], allEntrances[0x0029], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.FlowerPuzzle }
                        }),
                        new Connection(allEntrances[0x002B], allEntrances[0x002A], null),
                    }
                },
                new Room()
                {
                    id = 0x15,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x002C], //left
                        allEntrances[0x002D], //right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x002C], allEntrances[0x002C], null),
                        new Connection(allEntrances[0x002D], allEntrances[0x002D], null),
                        new Connection(allEntrances[0x002C], allEntrances[0x002D], null),
                        new Connection(allEntrances[0x002D], allEntrances[0x002C], null),
                        new Connection(allEntrances[0x002C], Objective.Sword, null),
                        new Connection(allEntrances[0x002D], Objective.Sword, null),
                    }
                },
                new Room()
                {
                    id = 0x16,
                    entrances = new List<Entrance>
                    {
                        allEntrances[0x002E], //left
                        allEntrances[0x002F], //top
                        allEntrances[0x0030], //bottom
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x002E], allEntrances[0x002E], null),
                        new Connection(allEntrances[0x002F], allEntrances[0x002F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x0030], allEntrances[0x0030], null),
                        new Connection(allEntrances[0x002E], allEntrances[0x002F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x002F], allEntrances[0x002E], null),
                        new Connection(allEntrances[0x002E], allEntrances[0x0030], null),
                        new Connection(allEntrances[0x0030], allEntrances[0x002E], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum },
                            new List<Requirement> { Requirement.DashOrb },
                        }),
                        new Connection(allEntrances[0x002F], allEntrances[0x0030], null),
                        new Connection(allEntrances[0x0030], allEntrances[0x002F], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.VerticalMomentum }
                        }),
                        new Connection(allEntrances[0x002E], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x002F], Objective.VerticalMomentum, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb, Requirement.Grapple }
                        }),
                        new Connection(allEntrances[0x0030], Objective.VerticalMomentum, new List<List<Requirement>>
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
                        allEntrances[0x0031], //Left
                        allEntrances[0x0032], //Right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0031], allEntrances[0x0031], null),
                        new Connection(allEntrances[0x0032], allEntrances[0x0032], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0031], allEntrances[0x0032], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0032], allEntrances[0x0031], new List<List<Requirement>>
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
                        allEntrances[0x0033], //Left
                        allEntrances[0x0034], //Right
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0033], allEntrances[0x0033], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0034], allEntrances[0x0034], null),
                        new Connection(allEntrances[0x0033], allEntrances[0x0034], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0034], allEntrances[0x0033], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0033], Objective.DashOrb, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0034], Objective.DashOrb, new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.ConstructDefeat }
                        }),
                        new Connection(allEntrances[0x0034], Objective.ConstructDefeat, new List<List<Requirement>>
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
                        allEntrances[0x0035], //Left
                        allEntrances[0x0036], //Top
                    },
                    connections = new List<Connection>
                    {
                        new Connection(allEntrances[0x0035], allEntrances[0x0035], null),
                        new Connection(allEntrances[0x0036], allEntrances[0x0036], new List<List<Requirement>>
                        {
                            new List<Requirement> { Requirement.DashOrb }
                        }),
                        new Connection(allEntrances[0x0035], Objective.SaveButton, null),
                        new Connection(allEntrances[0x0036], Objective.SaveButton, null),
                    },
                    hasWarp = true,
                }
            };
        }

        private static void SortEntrances()
        {
            foreach (Entrance e in allEntrances)
            {
                switch (e.type)
                {
                    case EntranceType.Left:     leftEntrances.Add(e);   break;
                    case EntranceType.Right:    rightEntrances.Add(e);  break;
                    case EntranceType.Top:      topEntrances.Add(e);    break;
                    case EntranceType.Bottom:   bottomEntrances.Add(e); break;
                }
            }
        }

        public static List<Room> allRooms = new List<Room>();
        public static List<Entrance> allEntrances = new List<Entrance>();
        public static List<Entrance> rightEntrances = new List<Entrance>();
        public static List<Entrance> leftEntrances = new List<Entrance>();
        public static List<Entrance> topEntrances = new List<Entrance>();
        public static List<Entrance> bottomEntrances = new List<Entrance>();

        public static List<List<Entrance>> uncheckedEntrances = new List<List<Entrance>>();
        public static List<UncollectedObjective> knownObjectives = new List<UncollectedObjective>();
        public static List<CollectedObjective> inventory = new List<CollectedObjective>();
        public static List<Entrance> currentRoute;

        public class UncollectedObjective
        {
            public Objective obj;
            public byte rm;
            public List<Connection> connections;
        }

        public class CollectedObjective
        {
            public Objective obj;
            public byte rm;
        }
    }
}