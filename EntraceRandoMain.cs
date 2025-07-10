using System.Collections.Generic;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(GlyphsEntranceRando.Main), "Glyphs Entrance Randomizer", "0.4.0", "BuffYoda21")]
[assembly: MelonGame("Vortex Bros.", "GLYPHS")]

namespace GlyphsEntranceRando {
    public class Main : MelonMod {
        [System.Obsolete]
        public override void OnApplicationStart() {
            var harmony = new HarmonyLib.Harmony("GlyphsEntranceRando");
            harmony.PatchAll();
            LoadRandomizedEntrances();
            ClassInjector.RegisterTypeInIl2Cpp<DynamicTp>();
        }

        public void LoadRandomizedEntrances() {
            if (File.Exists(JSON_SAVE_PATH)) {
                try {
                    string json = File.ReadAllText(JSON_SAVE_PATH);
                    WarpManager.entrancePairs = JsonConvert.DeserializeObject<List<RoomShuffler.SerializedEntrancePair>>(json);
                    if (WarpManager.entrancePairs?.Count == 54) {
                        MelonLogger.Msg($"Loaded existing {JSON_SAVE_NAME}.");
                        return; // stop here if we've loaded the entrances
                    } else {
                        MelonLogger.Warning($"{JSON_SAVE_NAME} is incomplete or invalid, generating new seed.");
                    }
                } catch {
                    MelonLogger.Error($"Failed to load {JSON_SAVE_NAME}, generating new seed.");
                }
            }
            for (int i = 0; i > -1; i++) {
                if (RoomShuffler.Shuffle()) {
                    MelonLogger.Msg($"{i} randomization attempts tried");
                    MelonLogger.Msg("Randomization Successful!");
                    LoadRandomizedEntrances();
                    break;
                }
            }
        }

        public static string JSON_SAVE_NAME = "RandomizationResults.json";
        public static string JSON_SAVE_PATH = Path.Combine(MelonEnvironment.UserDataDirectory, JSON_SAVE_NAME);
    }

    public class Room {
        public byte id = 0x00;
        public bool canMap = true;
        public bool bossRoom = false;
        public bool hasWarp = false;
        public List<Connection> connections = new List<Connection>();
        public List<Entrance> entrances = new List<Entrance>();
        public bool isStartRoom = false;
    }

    public class Connection {
        public Connection(Entrance enter, Entrance exit, List<List<Requirement>> req) {
            this.enter = enter;
            this.exit = exit;
            this.requirements = req;
            this.obj = Objective.None;
        }

        public Connection(Entrance enter, Objective obj, List<List<Requirement>> req) {
            this.enter = enter;
            this.exit = null;
            this.requirements = req;
            this.obj = obj;
        }

        public Connection(Entrance enter, List<List<Requirement>> req) {
            this.enter = enter;
            this.exit = enter;
            this.requirements = req;
            this.obj = Objective.None;
        }
        public Entrance enter = null;
        public Entrance exit = null;
        public Objective obj = Objective.None;
        public List<List<Requirement>> requirements = new List<List<Requirement>>();
    }

    public class Entrance {
        public Entrance(int id, byte roomId, EntranceType type) {
            this.id = id;
            this.roomId = roomId;
            this.type = type;
        }

        public int id = 0x0000;
        public byte roomId = 0x00;
        public EntranceType type = EntranceType.Right;
        public Entrance couple = null;
    }

    public enum Objective : byte {
        None = Requirement.None,

        //Collectables and other counters
        SilverShard = Requirement.SilverShardx15,
        GoldShard = Requirement.GoldShardx1,
        SmileToken = Requirement.SmileTokenx2,
        RuneCube = Requirement.RuneCubex3,
        VoidGateShard = Requirement.VoidGateShardx7,
        Sigil = Requirement.Sigilx3,
        Glyphstone = Requirement.Glyphstonex3,
        SerpentLock = Requirement.SerpentLockx4,
        WallJump = Requirement.WallJumpx1,
        Seeds = Requirement.Seedsx10,    //placeholder need to check actual count

        //Abilities
        Sword = Requirement.Sword,
        DashOrb = Requirement.DashOrb,
        Map = Requirement.Map,
        Grapple = Requirement.Grapple,
        DashAttackOrb = Requirement.DashAttackOrb,
        Parry = Requirement.Parry,

        //Story Points
        ConstructDefeat = Requirement.ConstructDefeat,
        SerpentDefeat = Requirement.SerpentDefeat,
        FalseEnding = Requirement.FalseEnding,
        WizardTrueDefeat = Requirement.WizardTrueDefeat,
        NullDefeat = Requirement.NullDefeat,
        SpearmanDefeat = Requirement.SpearmanDefeat,
        GoodEnding = Requirement.GoodEnding,
        Clarity = Requirement.Clarity,
        LastFracture = Requirement.LastFracture,
        Act1 = Requirement.Act1,
        Act2 = Requirement.Act2,
        TrueEnding = Requirement.TrueEnding,
        SmilemaskEnding = Requirement.SmilemaskEnding,
        OmnipotenceEnding = Requirement.OmnipotenceEnding,

        //Shop Items
        SwordRune = Requirement.SwordRune,
        Shroud = Requirement.Shroud,
        FastMagic = Requirement.FastMagic,
        SwiftParry = Requirement.SwiftParry,

        //Hats
        PinkBow = Requirement.PinkBow,
        DummyHat = Requirement.DummyHat,
        TopHat = Requirement.TopHat,
        TrafficCone = Requirement.TrafficCone,
        JohnHat = Requirement.JohnHat,
        Fez = Requirement.Fez,
        Hat7 = Requirement.Hat7,
        BombHat = Requirement.BombHat,
        Hat9 = Requirement.Hat9,
        George = Requirement.George,

        //Misc
        FlowerPuzzle = Requirement.FlowerPuzzle,
        VerticalMomentum = Requirement.VerticalMomentum,
        BreakableVoidPlatform = Requirement.BreakableVoidPlatform,
        MapPuzzle = Requirement.MapPuzzle,
        SaveButton = Requirement.SaveButton,
        SaveShard = Requirement.SaveShard,
    }

    public enum Requirement : byte {
        None = 0x00,

        //Collectables and other counters
        SilverShardx15 = 0x01,
        GoldShardx1 = 0x02,
        GoldShardx2 = 0x03,
        GoldShardx3 = 0x04,
        SmileTokenx2 = 0x05,
        SmileTokenx4 = 0x06,
        SmileTokenx6 = 0x07,
        SmileTokenx8 = 0x08,
        SmileTokenx10 = 0x09,
        RuneCubex3 = 0x0A,
        VoidGateShardx7 = 0x0B,
        Sigilx3 = 0x0C,
        Glyphstonex3 = 0x0D,
        SerpentLockx4 = 0x0F,
        WallJumpx1 = 0x10,
        WallJumpx2 = 0x11,
        Seedsx10 = 0x12,    //placeholder need to check actual count

        //Abilities
        Sword = 0x13,
        DashOrb = 0x14,
        Map = 0x15,
        Grapple = 0x16,
        DashAttackOrb = 0x17,
        Parry = 0x18,

        //Story Points
        ConstructDefeat = 0x19,
        SerpentDefeat = 0x1A,
        FalseEnding = 0x1B,
        WizardTrueDefeat = 0x1C,
        NullDefeat = 0x1D,
        SpearmanDefeat = 0x1E,
        GoodEnding = 0x1F,
        Clarity = 0x20,
        LastFracture = 0x21,
        Act1 = 0x22,
        Act2 = 0x23,
        TrueEnding = 0x24,
        SmilemaskEnding = 0x25,
        OmnipotenceEnding = 0x26,

        //Shop Items
        SwordRune = 0x27,
        Shroud = 0x28,
        FastMagic = 0x29,
        SwiftParry = 0x2A,

        //Hats
        PinkBow = 0x2B,
        DummyHat = 0x2C,
        TopHat = 0x2D,
        TrafficCone = 0x2E,
        JohnHat = 0x2F,
        Fez = 0x30,
        Hat7 = 0x31,
        BombHat = 0x32,
        Hat9 = 0x33,
        George = 0x34,

        //Misc
        FlowerPuzzle = 0x35,
        VerticalMomentum = 0x36,
        BreakableVoidPlatform = 0x37,
        MapPuzzle = 0x38,
        SaveButton = 0x39,
        SaveShard = 0x3A,
    }

    public enum EntranceType : byte {
        Right = 0x00,
        Left = 0x01,
        Top = 0x02,
        Bottom = 0x03
    }
}
