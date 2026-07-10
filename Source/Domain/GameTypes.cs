struct Tile
{
    public int State;
    public float Durability;
    public float Collapse;
    public float RegrowTimer;
    public float UntouchedTimer;
    public bool EventMarked;
}

enum TileHealthVisual
{
    Pristine,
    Sturdy,
    Worn,
    Cracked,
    Fractured,
    Critical,
}

struct Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Alpha;
    public float Fade;
    public float Rotation;
    public float Spin;
    public float Size;
    public float Drag;
    public bool Glow;
}

struct DashTrail
{
    public Vector2 Position;
    public float Alpha;
    public float Radius;
}

struct FloatingText
{
    public Vector2 Position;
    public Vector2 Velocity;
    public string Text;
    public Color Color;
    public float Life;
    public float MaxLife;
    public int Size;
}

struct Mote
{
    public Vector2 Position;
    public float Radius;
    public float Speed;
    public float Phase;
    public Color Color;
    public bool IsLeaf;
    public float Spin;
}

struct Projectile
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Life;
    public float Size;
    public float Damage;
    public int Pierce;
    public float Homing;
    public GunFireStyle Style;
    public Vector2 BeamEnd;
    public float BeamWidth;
    public float SpinRate;
    public float SpawnLife;
}

enum GunFireStyle
{
    Standard,
    ArcFan,
    Buckshot,
    Homing,
    Lance,
    Repeater,
    Volley,
    Burst,
    Laser,
    Sniper,
    RingPulse,
    CrossBurst,
    Mortar,
    DriftOrb,
    FlailArc,
}

struct Gun
{
    public string Name;
    public string Desc;
    public Color Color;
    public float FireCooldown;
    public int Count;
    public float Spread;
    public float Speed;
    public float Damage;
    public int Pierce;
    public float Homing;
    public int FableCost;
    public int LevelReq;
    public int WaveReq;
    public GunFireStyle Style;
    public int BurstCount;
    public float BurstGap;
}

enum EnemyBehavior
{
    Chase,
    FastChase,
    CrushTiles,
    Orbit,
    Zigzag,
    Hop,
    BlightTrail,
    TileLeech,
    Kite,
    Charge,
    PulseBlight,
    Phaser,
    Rotburst,
    Splitter,
    Sapper,
    Lurker,
    BossBlight,
    BossDash,
    BossSmash,
}

enum EnemyType
{
    BrambleStalker,
    Foxshade,
    Rootbeast,
    WillowWisp,
    MudHopper,
    NettleMoth,
    SapLeech,
    AcornSwarm,
    Thornback,
    Rotcap,
    FernLurker,
    BarkHusk,
    SporeDrifter,
    CreekSprite,
    MossGolem,
    VineLasher,
    PetalGhost,
    CinderKit,
    HollowStag,
    GloomSeed,
    ReedSerpent,
    BogShambler,
    BrambleLord,
    FoxWarden,
    GroveTitan,
}

readonly struct EnemyDef
{
    public readonly string Name;
    public readonly string Desc;
    public readonly Color Color;
    public readonly float Radius;
    public readonly float Speed;
    public readonly float HpBase;
    public readonly float HpPerWave;
    public readonly int Sides;
    public readonly EnemyBehavior Behavior;
    public readonly float TileDecay;
    public readonly bool InstaCollapse;
    public readonly bool Boss;
    public readonly int MinWave;
    public readonly float RotSpeed;

    public EnemyDef(string name, string desc, Color color, float radius, float speed, float hpBase, float hpPerWave,
        int sides, EnemyBehavior behavior, float tileDecay, bool instaCollapse, bool boss, int minWave, float rotSpeed)
    {
        Name = name;
        Desc = desc;
        Color = color;
        Radius = radius;
        Speed = speed;
        HpBase = hpBase;
        HpPerWave = hpPerWave;
        Sides = sides;
        Behavior = behavior;
        TileDecay = tileDecay;
        InstaCollapse = instaCollapse;
        Boss = boss;
        MinWave = minWave;
        RotSpeed = rotSpeed;
    }
}

enum FloorEventType
{
    None,
    CrimsonCrumble,
    SafeZoneRush,
    Checkerfall,
    RingCollapse,
    StoneIslands,
    ScatterPits,
    MossRot,
    MarkedStrike,
    CenterSnare,
    BlightStorm,
    TideSurge,
    TideRift,
    TideRecede,
    TideColumn,
    TideEcho,
    TideUndertow,
    TideCrest,
    TideWall,
    TideAnchor,
    TideFoam,
    EmberRain,
    EmberGate,
    EmberPulse,
    EmberCross,
    EmberBridge,
    EmberFury,
    EmberSnake,
    EmberHive,
    EmberTide,
    EmberCage,
    EmberQuake,
    EmberBloom,
    EmberAltar,
    CryptSeal,
    CryptWail,
    CryptTorch,
    CryptChains,
    CryptMist,
    CryptTomb,
    CryptShroud,
    CryptGlimpse,
    CryptRattle,
    CryptEcho,
    CryptGrave,
    CryptLantern,
    CryptVeil,
    TideWhirlpool,
    TideBeacon,
    TideStrike,
    CrownTrial,
    CrownFall,
    CrownShard,
    CrownThrone,
    CrownEdict,
    CrownRot,
    CrownBolt,
    CrownRing,
    CrownIsles,
    CrownStorm,
    CrownCoronation,
    CrownUsurpation,
    CrownReckoning,
    SallyForth,
    Portcullis,
    HeraldsCall,
}

enum EventFamily
{
    General,
    Tide,
    Ember,
    Crypt,
    Crown,
}

enum BlessingType
{
    None,
    SwiftMarch,
    DeepPockets,
    Stonecraft,
    LongFuse,
    ThornVolley,
    LuckySigil,
    IronSoles,
    KeenEye,
    BannerWard,
    SiegeRations,
    LastLight,
    WindBlessing,
}

enum OathType
{
    None,
    NoVerdict,
    NoOath,
    HeraldryBound,
    PureNightmare,
}

enum SiegeObjectiveType
{
    None,
    HoldBanner,
    ClearBreach,
    ProtectCorner,
}

enum GunAffixType
{
    None,
    Volatile,
    Steady,
    Leeching,
    Piercing,
    Quickdraw,
    Heavy,
    Lucky,
    Ranger,
}

enum GameState
{
    MainMenu,
    DifficultySelect,
    Playing,
    Paused,
    GameOver,
    Customize,
    Settings,
}

enum Difficulty
{
    TotalBeginner,
    Squire,
    Knight,
    Champion,
    FableNightmare,
    PracticeHall,
}

readonly struct DifficultyProfile
{
    public readonly string Title;
    public readonly string Tagline;
    public readonly string Detail;
    public readonly Color Accent;
    public readonly Color AccentHi;
    public readonly float EnemyHpMult;
    public readonly float EnemySpeedMult;
    public readonly float GruntCountMult;
    public readonly float SwarmCountMult;
    public readonly int MinWaveForEvents;
    public readonly float FirstEventCooldown;
    public readonly float EventCooldownMin;
    public readonly float EventCooldownMax;
    public readonly float EventIntensityMult;
    public readonly float WavePauseMult;
    public readonly float SwarmIntervalMult;
    public readonly float EventStackChance;
    public readonly float EventSurgeChance;
    public readonly bool EasyEventsOnly;
    public readonly int GruntMinWaveBonus;

    public DifficultyProfile(string title, string tagline, string detail, Color accent, Color accentHi,
        float enemyHpMult, float enemySpeedMult, float gruntCountMult, float swarmCountMult,
        int minWaveForEvents, float firstEventCooldown, float eventCooldownMin, float eventCooldownMax,
        float eventIntensityMult, float wavePauseMult, float swarmIntervalMult,
        float eventStackChance, float eventSurgeChance, bool easyEventsOnly, int gruntMinWaveBonus)
    {
        Title = title;
        Tagline = tagline;
        Detail = detail;
        Accent = accent;
        AccentHi = accentHi;
        EnemyHpMult = enemyHpMult;
        EnemySpeedMult = enemySpeedMult;
        GruntCountMult = gruntCountMult;
        SwarmCountMult = swarmCountMult;
        MinWaveForEvents = minWaveForEvents;
        FirstEventCooldown = firstEventCooldown;
        EventCooldownMin = eventCooldownMin;
        EventCooldownMax = eventCooldownMax;
        EventIntensityMult = eventIntensityMult;
        WavePauseMult = wavePauseMult;
        SwarmIntervalMult = swarmIntervalMult;
        EventStackChance = eventStackChance;
        EventSurgeChance = eventSurgeChance;
        EasyEventsOnly = easyEventsOnly;
        GruntMinWaveBonus = gruntMinWaveBonus;
    }
}

enum CustomizeTab
{
    Cosmetics,
    Weapons,
    Upgrades,
    Abilities,
    Rank,
    Bestiary,
    Glossary,
}

enum AbilityType
{
    Paralyze,
    WindStep,
    OathOfTheBailey,
    Verdict,
    BannerOfStillness,
}

enum RebindTarget
{
    None,
    Fire,
    Ability1,
    Ability2,
}

enum DeathCause
{
    Unknown,
    FellThrough,
    FloorGaveWay,
    EnemyGrasp,
    SafeZoneFailed,
    CenterSnare,
    TideDrowned,
    TideBeaconLost,
}

class Enemy
{
    public EnemyType Type;
    public Vector2 Position;
    public Vector2 Vel;
    public float Speed;
    public float Radius;
    public float Spawn;
    public float Wobble;
    public float Hp;
    public float MaxHp;
    public float Hit;
    public float AbilityTimer;
    public float Phase;
    public int AbilityStep;
    public int StrikeTx;
    public int StrikeTy;
    public int StrikeDepth;
    public float ParalyzeTimer;
    public bool Dead;
    public bool Elite;
}

