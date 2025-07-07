using MelonLoader;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(GlyphsEntranceRando.Main), "Glyphs Entrance Randomizer", "0.1.0", "BuffYoda21")]
[assembly: MelonGame("Vortex Bros.", "GLYPHS")]

namespace GlyphsEntranceRando
{
    public class Main : MelonMod
    {
        [System.Obsolete]
        public override void OnApplicationStart()
        {
            bool randomizationSuccess = false;
            for (int i = 0; i < 1000; i++)
            {
                if (RoomShuffler.Shuffle())
                {
                    randomizationSuccess = true;
                    break;
                }
            }
            if (randomizationSuccess)
                MelonLogger.Msg("Randomization Successful!");
            else
                MelonLogger.Error("Max retries reached. Quitting...");
        }
    }

    public class Room
    {
        public byte id = 0x00;
        public bool canMap = true;
        public bool bossRoom = false;
        public bool hasWarp = false;
        public List<Connection> connections = new List<Connection>();
        public List<Entrance> entrances = new List<Entrance>();
        public bool isStartRoom = false;
    }

    public class Connection
    {
        public Connection(Entrance enter, Entrance exit, List<List<Requirement>> req) {
            this.enter = enter;
            this.exit = exit;
            this.requirements = req;
            this.obj = Objective.None;
        }

        public Connection(Entrance enter, Objective obj, List<List<Requirement>> req)
        {
            this.enter = enter;
            this.exit = null;
            this.requirements = req;
            this.obj = obj;
        }

        public Connection(Entrance enter, List<List<Requirement>> req)
        {
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

    public class Entrance
    {
        public Entrance(ushort id, byte roomId, EntranceType type)
        {
            this.id = id;
            this.roomId = roomId;
            this.type = type;
        }

        public void AddCouple(Entrance couple)
        {
            this.couple = couple;
        }

        public ushort id = 0x0000;
        public byte roomId = 0x00;
        public EntranceType type = EntranceType.Right;
        public Entrance couple = null;
    }

    public enum Objective : byte
    {
        None = 0x00,

        //Collectables and other counters
        SilverShard = 0x01,
        GoldShard = 0x02,
        SmileToken = 0x05,
        RuneCube = 0x0A,
        VoidGateShard = 0x0B,
        Sigil = 0x0C,
        Glyphstone = 0x0D,
        SerpentLock = 0x0F,
        WallJump = 0x10,
        Seeds = 0x12,    //placeholder need to check actual count

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

    public enum Requirement : byte
    {
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

    public enum EntranceType : byte
    {
        Right = 0x00,
        Left = 0x01,
        Top = 0x02,
        Bottom = 0x03
    }
}