using UnityEngine;
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
        public Connection(Entrance enter, Entrance exit, List<List<Requirements>> req) {
            this.enter = enter;
            this.exit = exit;
            this.requirements = req;
            this.obj = Objective.None;
        }

        public Connection(Entrance enter, Objective obj, List<List<Requirements>> req)
        {
            this.enter = enter;
            this.exit = null;
            this.requirements = req;
            this.obj = obj;
        }

        public Connection(Entrance enter, List<List<Requirements>> req)
        {
            this.enter = enter;
            this.exit = enter;
            this.requirements = req;
            this.obj = Objective.None;
        }
        public Entrance enter = null;
        public Entrance exit = null;
        public Objective obj = Objective.None;
        public List<List<Requirements>> requirements = new List<List<Requirements>>();
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
        SmileToken = 0x03,
        RuneCube = 0x04,
        SaveShard = 0x05,
        VoidGateShard = 0x06,
        Sigil = 0x07,
        Glyphstone = 0x08,
        SerpentLock = 0x09,
        SaveButton = 0x0A,
        WallJump = 0x0B,

        //Abilities
        Sword = 0x0C,
        DashOrb = 0x0D,
        Map = 0x0E,
        Grapple = 0x0F,
        DashAttackOrb = 0x10,
        Parry = 0x11,

        //Hats
        PinkBow = 0x12,
        DummyHat = 0x13,
        TopHat = 0x14,
        TrafficCone = 0x15,
        JohnHat = 0x16,
        Fez = 0x17,
        Hat7 = 0x18,
        BombHat = 0x19,
        Hat9 = 0x1A,
        George = 0x1B,

        //Story Points
        ConstructDefeat = 0x1C,
        SerpentDefeat = 0x1D,
        FalseEnding = 0x1E,
        WizardTrueDefeat = 0x1F,
        NullDefeat = 0x20,
        SpearmanDefeat = 0x21,
        GoodEnding = 0x22,
        Clarity = 0x23,
        LastFracture = 0x24,
        Act1 = 0x25,
        Act2 = 0x26,
        TrueEnding = 0x27,
        SmilemaskEnding = 0x28,
        OmnipotenceEnding = 0x29,

        //Shop Items
        SwordRune = 0x2A,
        Shroud = 0x2B,
        FastMagic = 0x2C,
        SwiftParry = 0x2D,

        //Misc
        FlowerPuzzle = 0x2E,
        VerticalMomentum = 0x2F,
        BreakableVoidPlatform = 0x30,
        MapPuzzle = 0x31,
    }

    public enum Requirements : byte
    {
        None = 0x00,

        //Collectables and other counters
        SilverShardx15 = 0x01,
        GoldShardx1 = 0x02,
        GoldShardx2 = 0x03,
        GoldShardx3 = 0x04,
        Tokensx2 = 0x05,
        Tokensx4 = 0x06,
        Tokensx6 = 0x07,
        Tokensx8 = 0x08,
        Tokensx10 = 0x09,
        RuneCubex3 = 0x0A,
        VoidGateShardx7 = 0x0B,
        Sigilx3 = 0x0C,
        Glyphstonesx3 = 0x0D,
        SerpentLocksx4 = 0x0F,
        WallJumpx1 = 0x10,
        WallJumpx2 = 0x11,

        //Abilities
        Sword = 0x12,
        DashOrb = 0x13,
        Map = 0x14,
        Grapple = 0x15,
        DashAttackOrb = 0x16,
        Parry = 0x17,

        //Story Points
        ConstructDefeat = 0x18,
        SerpentDefeat = 0x19,
        FalseEnding = 0x1A,
        WizardTrueDefeat = 0x1B,
        NullDefeat = 0x1C,
        SpearmanDefeat = 0x1D,
        GoodEnding = 0x1E,
        Clarity = 0x1F,
        LastFracture = 0x20,
        Act1 = 0x21,
        Act2 = 0x22,
        TrueEnding = 0x23,
        SmilemaskEnding = 0x24,
        OmnipotenceEnding = 0x25,

        //Shop Items
        SwordRune = 0x26,
        Shroud = 0x27,
        FastMagic = 0x28,
        SwiftParry = 0x29,

        //Misc
        FlowerPuzzle = 0x2A,
        VerticalMomentum = 0x2B,
        BreakableVoidPlatform = 0x2C,
        MapPuzzle = 0x2D,
    }

    public enum EntranceType : byte
    {
        Right = 0x00,
        Left = 0x01,
        Top = 0x02,
        Bottom = 0x03
    }
}