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
        Hat10 = 0x1B,

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
        FlowerPuzzleSolution = 0x2E,
        VerticalMomentum = 0x2F,
    }

    public enum Requirements : byte
    {
        None = 0x00,
        Sword = 0x01,
        DashOrb = 0x02,
        Map = 0x03,
        Grapple = 0x04,
        DashAttackOrb = 0x05,
        Parry = 0x06,
        ConstructDefeat = 0x07,
        SerpentDefeat = 0x08,
        FalseEnding = 0x09,
        WizardTrueDefeat = 0x0A,
        NullDefeat = 0x0B,
        SpearmanDefeat = 0x0C,
        GoodEnding = 0x0D,
        LastFracture = 0x0E,
        Clarity = 0x0F,
        Shardx15 = 0x10,
        GoldShardx1 = 0x11,
        GoldShardx2 = 0x12,
        GoldShardx3 = 0x13,
        SwordRune = 0x14,
        Shroud = 0x15,
        FastMagic = 0x16,
        SwiftParry = 0x17,
        Tokensx2 = 0x18,
        Tokensx4 = 0x19,
        Tokensx6 = 0x1A,
        Tokensx8 = 0x1B,
        Tokensx10 = 0x1C,
        Glyphstonesx3 = 0x1D,
        FlowerPuzzleSolved = 0x1E,
        SerpentLocksx4 = 0x1F,
        VoidGateShardx7 = 0x20,
        VoidPlatformBroken = 0x21,
        WallJumpx1 = 0x22,
        WallJumpx2 = 0x23,
        MapPuzzleSolved = 0x24,
        RuneCubesx3 = 0x25,
        Sigilx3 = 0x26,
        Act1 = 0x27,
        Act2 = 0x28,
        VerticalMomentum = 0x29,
    }

    public enum EntranceType : byte
    {
        Right = 0x00,
        Left = 0x01,
        Top = 0x02,
        Bottom = 0x03
    }
}