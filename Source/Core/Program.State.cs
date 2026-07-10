partial class Program
{
    const int WindowWidth = 800;
    const int WindowHeight = 800;
    const float UiFooterReserve = 108f;
    const float UiHintBarHeight = 32f;
    static float UiFooterTop => WindowHeight - UiFooterReserve;
    static float UiHintBarY => WindowHeight - UiHintBarHeight - 12f;
    static float UiBackButtonY => UiFooterTop + 10f;
    const int UhdRenderScale = 2;
    const float WindowCloseBtnSize = 32f;
    const float WindowCloseBtnPad = 12f;
    const float WindowDragBarHeight = 28f;
    const int GridSize = 20;
    const int TileSize = 40;

    const float PlayerRadius = 16f;
    const float EquippedWeaponScaleMul = 2.65f;
    const float PlayerSpeed = 205f;
    const float PlayerAccel = 24f;
    const float DashSpeed = 780f;
    const float DashDuration = 0.16f;
    const float DashCameraLeanDeg = 2.1f;
    const float DashCooldown = 1.25f;

    const float ParalyzeCooldown = 30f;
    const float ParalyzeRadiusBase = 128f;
    const float ParalyzeDurationBase = 1.35f;
    const float ParalyzeBurstTime = 0.65f;

    const int AbilityCount = 5;
    const float VerdictCooldown = 60f;
    const float VerdictHaltDuration = 30f;
    const int VerdictUnlockKills = 30;
    const float BannerDuration = 8.5f;
    const float BannerRadius = 122f;
    const float BannerCooldown = 3.8f;
    const float BannerPlantTime = 0.38f;
    const float BannerEnemySlow = 0.34f;
    const float BannerFireRateMul = 1.38f;
    const float OathIFrameTime = 2.6f;
    const float VerdictIFrameTime = 1.85f;
    const float OathRescueFlashTime = 1.1f;
    const float VerdictWaveTime = 0.95f;

    const float MaxDurability = 100f;
    const float PlayerDurabilityDecayRate = 38f;
    const float StalkerDurabilityDecayRate = 16f;
    const float SprinterDurabilityDecayRate = 4f;

    const float WaveIntervalBase = 5f;
    const float WaveIntervalMin = 2.5f;
    const float SwarmIntervalBase = 2.4f;
    const float BetweenWavePause = 3.2f;
    const float TileRegrowTime = 10f;
    const float TileIdleRegenDelay = 15f;
    const float TileIdleRegenRate = 22f;
    const float MossRotAreaDecayRate = 11f;
    const int SafeIslandSize = 3;
    const int SafeIslandCount = 4;
    const float FloorEventCooldownMin = 5f;
    const float FloorEventCooldownMax = 15f;
    const float SafeRushBandTiles = 5f;
    const float CenterSnareMarginTiles = 3f;
    const float EmberRainColumnInterval = 0.32f;
    const float EmberPulseInterval = 0.72f;
    const float EmberSnakeStepInterval = 0.28f;
    const float EmberBloomInterval = 0.5f;
    const float EmberQuakeInterval = 0.45f;
    const float EmberFuryStandTime = 0.55f;
    const int EmberGateCorridorTiles = 2;
    const int EmberHiveRadius = 1;
    const int EmberSnakeLength = 42;
    const int EmberBloomSeedCount = 4;
    const int PointsPerEnemyKill = 100;
    const float ComboWindow = 2.6f;
    const float WaveBannerTime = 2.2f;
    const float CollapseTime = 0.32f;
    const int TargetFps = 0;
    const float BossTelegraphTime = 1.9f;
    const float BossCutTelegraphTime = 1.35f;
    const float BossCutComboCooldown = 8.5f;
    const float EnemyTelegraphTime = 1.15f;
    const float CrushTelegraphTime = 1.3f;
    const float CrushCooldown = 2.2f;
    const float ChargeTelegraphTime = 1.05f;
    const float ContactWarnMul = 1.7f;
    const float AiDashUrgencyThreshold = 0.82f;
    const float AiEnemyDangerTiles = 2f;
    const float AiVoidDangerRadius = 30f;
    const float AiCriticalTileHealthRatio = 0.2f;
    const float AiHealthyTileRatio = 0.7f;
    const int AiEdgeMarginTiles = 2;
    const int AiTopBandRows = 3;
    const float AiMaxWalkDurabilityRatio = 0.5f;

    const float TraumaDecay = 1.1f;
    const float MaxShakeOffset = 34f;
    const float MaxShakeRotation = 4.8f;

    static readonly string SaveDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fable");
    static readonly string SavePath = Path.Combine(SaveDirectory, "fable_save.txt");
    static readonly string ProgressResetFlagPath = Path.Combine(SaveDirectory, "fable_progress_v2.flag");

    // Upgrade indices
    const int UpSwift = 0;
    const int UpDash = 1;
    const int UpRapid = 2;
    const int UpThorns = 3;
    const int UpSplit = 4;
    const int UpFortune = 5;
    const int UpFeet = 6;
    const int UpPierce = 7;
    const int UpParalyzeReach = 8;
    const int UpParalyzeHold = 9;
    const int UpMagazine = 10;
    const int UpReload = 11;
    const int UpgradeCount = 12;
    const int UpgradeMax = 6;

    static readonly Random Rng = new();

    readonly struct VisualTheme
    {
        public Color CanopyTop { get; init; }
        public Color CanopyMid { get; init; }
        public Color ForestFloor { get; init; }
        public Color EarthPit { get; init; }
        public Color ForestShadow { get; init; }
        public Color MossLight { get; init; }
        public Color LeafGold { get; init; }
        public Color PlayerBright { get; init; }
        public Color Emerald { get; init; }
        public Color Amber { get; init; }
        public Color Danger { get; init; }
        public Color Gold { get; init; }
        public Color Bark { get; init; }
        public Color UiPanel { get; init; }
        public Color UiPanelDeep { get; init; }
        public Color UiBorder { get; init; }
        public Color UiBorderLight { get; init; }
        public Color UiAccent { get; init; }
        public Color SodDark { get; init; }
        public Color SodMid { get; init; }
        public Color TileSeam { get; init; }
        public Color BgMist { get; init; }
        public Color BgGlow { get; init; }
        public Color BlightRot { get; init; }
        public Color BlightSick { get; init; }
    }

    static readonly VisualTheme MedievalTheme = new()
    {
        CanopyTop = new Color(6, 6, 6, 255),
        CanopyMid = new Color(4, 4, 4, 255),
        ForestFloor = new Color(2, 2, 2, 255),
        EarthPit = new Color(14, 14, 16, 255),
        ForestShadow = new Color(8, 8, 10, 255),
        MossLight = new Color(80, 78, 74, 255),
        LeafGold = new Color(112, 110, 104, 255),
        PlayerBright = new Color(210, 208, 202, 255),
        Emerald = new Color(58, 56, 54, 255),
        Amber = new Color(46, 44, 42, 255),
        Danger = new Color(44, 42, 40, 255),
        Gold = new Color(180, 176, 168, 255),
        Bark = new Color(72, 70, 66, 255),
        UiPanel = new Color(28, 28, 32, 240),
        UiPanelDeep = new Color(16, 16, 20, 255),
        UiBorder = new Color(88, 86, 82, 255),
        UiBorderLight = new Color(118, 116, 112, 255),
        UiAccent = new Color(156, 154, 148, 255),
        SodDark = new Color(20, 20, 24, 255),
        SodMid = new Color(30, 30, 34, 255),
        TileSeam = new Color(16, 16, 18, 255),
        BgMist = new Color(42, 40, 38, 255),
        BgGlow = new Color(140, 138, 132, 255),
        BlightRot = new Color(36, 36, 40, 255),
        BlightSick = new Color(48, 46, 44, 255),
    };

    static VisualTheme ActiveTheme => MedievalTheme;
    static Color CanopyTop => ActiveTheme.CanopyTop;
    static Color CanopyMid => ActiveTheme.CanopyMid;
    static Color ForestFloor => ActiveTheme.ForestFloor;
    static Color EarthPit => ActiveTheme.EarthPit;
    static Color ForestShadow => ActiveTheme.ForestShadow;
    static Color MossLight => ActiveTheme.MossLight;
    static Color LeafGold => ActiveTheme.LeafGold;
    static Color PlayerBright => ActiveTheme.PlayerBright;
    static Color Emerald => ActiveTheme.Emerald;
    static Color Amber => ActiveTheme.Amber;
    static Color Danger => ActiveTheme.Danger;
    static Color Gold => ActiveTheme.Gold;
    static Color Bark => ActiveTheme.Bark;
    static Color UiPanel => ActiveTheme.UiPanel;
    static Color UiPanelDeep => ActiveTheme.UiPanelDeep;
    static Color UiBorder => ActiveTheme.UiBorder;
    static Color UiBorderLight => ActiveTheme.UiBorderLight;
    static Color UiAccent => ActiveTheme.UiAccent;
    static Color SodDark => ActiveTheme.SodDark;
    static Color SodMid => ActiveTheme.SodMid;
    static Color TileSeam => ActiveTheme.TileSeam;

    static readonly EnemyDef[] EnemyCatalog =
    {
        new("Thorn Squire", "A hedge-knight's scout. Each stride wears the flagstones.", new Color(108, 82, 68, 255), 14f, 95f, 14f, 2.2f, 5, EnemyBehavior.Chase, 16f, false, false, 1, 70f),
        new("Fox-Helm Scout", "Scarlet runner skirting the outer wall.", new Color(156, 72, 58, 255), 8f, 185f, 10f, 1.6f, 3, EnemyBehavior.FastChase, 4f, false, false, 1, 220f),
        new("Battering Brute", "Siege-born giant. Its footfalls shatter the bailey.", new Color(86, 80, 72, 255), 24f, 34f, 28f, 4f, 6, EnemyBehavior.CrushTiles, 0f, false, false, 3, 25f),
        new("Grave Lamp", "Pale lantern-ghost drifting along the parapets.", new Color(184, 168, 128, 255), 10f, 72f, 12f, 1.8f, 4, EnemyBehavior.Orbit, 6f, false, false, 2, 120f),
        new("Mire Leaper", "Bog-trooper in soaked mail, hopping worn tiles.", new Color(96, 84, 68, 255), 11f, 68f, 16f, 2f, 4, EnemyBehavior.Hop, 10f, false, false, 3, 90f),
        new("Ash Moth", "Cinder-swarm loosed from the pitch pots.", new Color(112, 106, 98, 255), 9f, 128f, 11f, 1.5f, 3, EnemyBehavior.Zigzag, 5f, false, false, 4, 180f),
        new("Pitch Drinker", "Clings to mortar and drinks the stone dry.", new Color(72, 68, 62, 255), 12f, 58f, 20f, 2.8f, 5, EnemyBehavior.TileLeech, 32f, false, false, 4, 50f),
        new("Rat Pack", "Starved war-rats poured through a breach.", new Color(118, 98, 76, 255), 6f, 165f, 6f, 0.8f, 3, EnemyBehavior.FastChase, 2f, false, false, 2, 260f),
        new("Spike Pavise", "Shield-wall breaker bristling with iron barbs.", new Color(98, 102, 110, 255), 16f, 64f, 24f, 3f, 6, EnemyBehavior.PulseBlight, 12f, false, false, 5, 40f),
        new("Mold Knight", "Fungal helm and rotted tabard. Bursts spores on death.", new Color(96, 88, 72, 255), 13f, 52f, 18f, 2.4f, 5, EnemyBehavior.Rotburst, 8f, false, false, 5, 35f),
        new("Murk Stalker", "Waits in shadow seams, then strikes tile by tile.", new Color(68, 78, 66, 255), 12f, 88f, 15f, 2f, 5, EnemyBehavior.Lurker, 9f, false, false, 6, 80f),
        new("Splinter Guard", "Cursed palisade timber given marching orders.", new Color(108, 88, 64, 255), 18f, 48f, 32f, 4f, 6, EnemyBehavior.Chase, 14f, false, false, 4, 30f),
        new("Soot Wraith", "Leaves a trail of blight like blown forge embers.", new Color(78, 74, 70, 255), 11f, 76f, 13f, 1.7f, 4, EnemyBehavior.BlightTrail, 7f, false, false, 6, 55f),
        new("Cistern Shade", "Well-spirit phasing through cracks in the keep.", new Color(88, 98, 108, 255), 9f, 110f, 12f, 1.6f, 3, EnemyBehavior.Phaser, 3f, false, false, 7, 200f),
        new("Mortar Golem", "Living rubble from a collapsed bastion.", new Color(82, 78, 72, 255), 19f, 32f, 30f, 3.5f, 6, EnemyBehavior.CrushTiles, 0f, false, false, 8, 20f),
        new("Chain Lasher", "Whip-sergeant dragging hooked links across stone.", new Color(118, 112, 98, 255), 13f, 92f, 17f, 2.2f, 5, EnemyBehavior.Charge, 11f, false, false, 7, 95f),
        new("Veil Maiden", "Mourning shade in torn funeral linen.", new Color(176, 168, 158, 255), 10f, 142f, 11f, 1.4f, 4, EnemyBehavior.Kite, 4f, false, false, 8, 150f),
        new("Cinder Imp", "Forge-pit imp whose heat saps the flagstones.", new Color(168, 92, 52, 255), 9f, 118f, 13f, 1.8f, 3, EnemyBehavior.Sapper, 18f, false, false, 9, 170f),
        new("Pale Hart", "Bone-antlered herald beast charging the walls.", new Color(148, 138, 122, 255), 17f, 78f, 28f, 3.6f, 6, EnemyBehavior.Charge, 10f, false, false, 9, 45f),
        new("Blight Pod", "Cursed seed that splits into smaller horrors.", new Color(88, 72, 92, 255), 14f, 62f, 22f, 2.6f, 5, EnemyBehavior.Splitter, 9f, false, false, 10, 42f),
        new("Moat Serpent", "Thin serpent circling the flooded ditch.", new Color(76, 92, 84, 255), 10f, 135f, 12f, 1.7f, 4, EnemyBehavior.Zigzag, 5f, false, false, 8, 210f),
        new("Fen Shambler", "Mire giant pulsing waves of rot through the yard.", new Color(64, 78, 68, 255), 19f, 44f, 30f, 3.8f, 6, EnemyBehavior.PulseBlight, 15f, false, false, 10, 28f),
        new("Thorn Marshal", "Mini-boss. Commands blight across the bailey.", new Color(108, 98, 68, 255), 22f, 58f, 72f, 11f, 7, EnemyBehavior.BossBlight, 16f, false, true, 5, 45f),
        new("Scarlet Warden", "Mini-boss. Fox-crested knight who dashes the rim.", new Color(148, 58, 48, 255), 13f, 168f, 58f, 8f, 4, EnemyBehavior.BossDash, 4f, false, true, 5, 260f),
        new("Keep Colossus", "Siege titan. Carves lanes through the battlements.", new Color(56, 58, 62, 255), 34f, 28f, 210f, 15f, 8, EnemyBehavior.BossSmash, 0f, false, true, 10, 18f),
    };

    static readonly Color[] BodyPalette =
    {
        new Color(168, 164, 158, 255),
        new Color(128, 124, 118, 255),
        new Color(98, 94, 90, 255),
        new Color(148, 142, 132, 255),
        new Color(188, 180, 168, 255),
        new Color(108, 104, 98, 255),
        new Color(178, 172, 162, 255),
        new Color(138, 132, 124, 255),
        new Color(118, 112, 106, 255),
        new Color(158, 152, 144, 255),
        new Color(148, 38, 42, 255),
        new Color(42, 68, 118, 255),
        new Color(48, 92, 58, 255),
        new Color(178, 142, 38, 255),
        new Color(92, 42, 98, 255),
        new Color(118, 28, 38, 255),
        new Color(62, 112, 152, 255),
        new Color(158, 82, 38, 255),
        new Color(88, 38, 52, 255),
        new Color(38, 38, 44, 255),
        new Color(122, 98, 72, 255),
        new Color(72, 118, 108, 255),
    };
    static readonly Color AutoPilotBody = new(168, 42, 38, 255);
    static readonly Color AutoPilotBright = new(220, 72, 58, 255);
    static readonly string[] BodyNames =
    {
        "Ash", "Slate", "Iron", "Parchment", "Marble", "Charcoal", "Silver", "Basalt", "Obsidian", "Ivory",
        "Gules", "Azure", "Vert", "Or", "Purpure", "Sanguine", "Celeste", "Tenn�", "Murrey", "Sable", "Bronze", "Cendr�e",
    };
    static readonly int[] BodyFableCost = { 0, 0, 0, 80, 0, 140, 0, 200, 0, 260, 0, 300, 0, 340, 0, 380, 0, 420, 0, 460, 0, 500 };
    static readonly int[] BodyLevelReq = { 0, 0, 0, 0, 5, 0, 12, 0, 20, 0, 28, 0, 32, 0, 36, 0, 40, 0, 44, 0, 48, 0 };
    static readonly int[] BodyWaveReq = { 0, 0, 0, 0, 0, 8, 0, 15, 0, 25, 0, 30, 0, 35, 0, 40, 0, 45, 0, 50, 0, 55 };

    const int AccessoryCursorCrown = 36;
    const string CursorCrownEasterEgg = "cursor";

    static readonly string[] AccessoryNames = BuildAccessoryNames();
    static readonly int[] AccessoryFableCost = BuildAccessoryFableCosts();
    static readonly int[] AccessoryLevelReq = BuildAccessoryLevelReqs();
    static readonly int[] AccessoryWaveReq = BuildAccessoryWaveReqs();

    static readonly string[] UpgradeNames =
    {
        "Swiftness", "Wind Step", "Rapid Fire", "Sharp Thorns", "Split Shot", "Fortune", "Light Feet", "Piercing",
        "Static Reach", "Lockstep Hold", "Deep Magazines", "Quick Hands",
    };
    static readonly string[] UpgradeDesc =
    {
        "+5% move speed", "-8% dash cooldown", "+14% fire rate", "+0.75 shot damage",
        "+1 projectile", "+15% fable gain", "-8% floor wear", "+1 pierce",
        "+22 paralyze radius", "+0.4s paralyze duration", "+15% magazine size", "-10% reload time",
    };
    static readonly int[] UpgradeBaseCost = { 40, 50, 45, 50, 70, 60, 40, 80, 90, 95, 55, 50 };

    static readonly string[] AbilityNames =
    {
        "Paralyze", "Wind Step", "Oath of the Bailey", "Verdict", "Banner of Stillness",
    };
    static readonly string[] AbilityDesc =
    {
        "Arc lightning freezes every foe in a wide ring. Your bread-and-butter crowd control.",
        "A blinding wind-dash with brief invulnerability. Leap crumbling stone and slip through grasping claws.",
        "Once per run: the keep itself catches you. Void death snaps you to solid stone with reinforced footing.",
        "Invoke judgment: end the active floor event and forbid any new catastrophes for 30 seconds. Unlocks after 30 kills in one run.",
        "Plant a battle standard. Time slows in its halo: tiles hold, events stall, foes drag, and your volley quickens.",
    };
    static readonly string[] AbilityTagline =
    {
        "STATIC REACH", "WIND DASH", "DEATH DEFIED", "JUDGMENT", "STILLNESS",
    };
    static readonly string[] AbilityHudName =
    {
        "PARALYZE", "WIND STEP", "OATH OF THE BAILEY", "VERDICT", "BANNER OF STILLNESS",
    };
    static readonly int[] AbilityFableCost = { 0, 0, 3200, -1, 3800 };

    static readonly Gun[] Guns = BuildGunCatalog();

    // World / run state
    static Tile[,] tiles = null!;
    static List<Enemy> enemies = null!;
    static List<Particle> particles = null!;
    static List<DashTrail> trails = null!;
    static List<FloatingText> floaters = null!;
    static List<Projectile> projectiles = null!;
    static Mote[] motes = null!;

    static Vector2 playerPos;
    static Vector2 playerVel;
    static Vector2 lastMoveDirection;
    static Vector2 weaponAimDir = Vector2.UnitY;
    static Vector2 weaponAimTarget;
    static float weaponRecoil;
    static GameState state;

    static int score;
    static float scoreDisplay;
    static int highScore;
    static bool lastRunNewBest;
    static DeathCause lastDeathCause;
    static string lastDeathDetail = "";
    static int runFablesEarned;

    static int waveNumber;
    static float waveBannerTimer;
    static bool waveInProgress;
    static int waveSwarmIndex;
    static int waveSwarmTotal;
    static float swarmCooldown;
    static float betweenWaveTimer;
    static string waveSubtext = "";

    static FloorEventType activeEvent;
    static float eventTimer;
    static float eventCountdown;
    static int eventSide;
    static int eventStep;
    static Rectangle eventSafeRect;
    static Rectangle eventDangerRect;
    static bool playerInEventSafeZone;
    static float floorRotTimer;
    static float floorRotSide;
    static float nextFloorEventTimer;
    static float nextFloorEventCooldown;
    static readonly int[] markedStrikeOrder = new int[GridSize * GridSize];
    static int markedStrikeCount;
    static int eventPhase;
    static float eventStartCountdown;
    static float eventActionTimer;
    static Vector2 eventEpicenter;
    static float eventBlightBoltTimer;
    static bool eventTideRowStrike;
    static int emberFuryTileIdx = -1;
    static float emberFuryStandTimer;
    static readonly Queue<(int X, int Y)> eventTileQueue = new();

    struct EventShockwave
    {
        public Vector2 Center;
        public float Radius;
        public float MaxRadius;
        public Color Color;
        public float Life;
        public float MaxLife;
    }

    struct EventSkyBeam
    {
        public Vector2 Ground;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Width;
        public bool Charging;
    }

    static readonly List<EventShockwave> eventShockwaves = new();
    static readonly List<EventSkyBeam> eventSkyBeams = new();
    const int MaxEventShockwaves = 4;
    const int MaxEventSkyBeams = 5;
    const int MaxParticles = 1400;
    const float MarkedStrikeTelegraphTime = 0.32f;
    const float MarkedStrikeFireTime = 0.03f;
    const float MarkedStrikeStrikeGap = 0.05f;
    const float CryptGraveTelegraphTime = 0.26f;
    const float CryptGraveFireTime = 0.03f;
    const float CryptGraveStrikeGap = 0.038f;
    const float CryptVeilRotateInterval = 1.12f;
    const int FloorEventRepeatCooldown = 3;
    static readonly FloorEventType[] floorEventHistory = new FloorEventType[FloorEventRepeatCooldown];
    static int floorEventHistoryCount;

    static int combo;
    static float comboTimer;

    static float dashTimer;
    static float dashCooldown;
    static float paralyzeCooldown;
    static float verdictCooldown;
    static float verdictHaltTimer;
    static float siegeGateOpenTimer;
    static float siegeEventDangerLight;
    static float siegeEventSafeLight;
    static float siegeEventTorchPulse;
    const float SiegeGateOpenDuration = 3.4f;
    static int runKillCount;
    static float bannerCooldown;
    static bool oathUsedThisRun;
    static float abilityIFrameTimer;
    static float oathRescueFlashTimer;
    static float verdictWaveTimer;
    static Vector2 verdictWaveOrigin;
    static bool bannerActive;
    static float bannerTimer;
    static float bannerPlantTimer;
    static Vector2 bannerPos;
    static AbilityType[] abilitySlotTracked = { AbilityType.Paralyze, AbilityType.WindStep };
    static int oathReinforcedTx = -1;
    static int oathReinforcedTy = -1;
    static float oathTilePulseTimer;
    static float windDashVfxTimer;
    static float paralyzeBurstTimer;
    static float paralyzeBurstRadius;
    static Vector2 paralyzeBurstOrigin;
    static float stepTimer;
    static float blightTimer;
    static float fireTimer;
    static int pendingBurstShots;
    static float pendingBurstTimer;
    static Vector2 pendingBurstTarget;
    static int pendingBurstGun;
    static int ammoInMag;
    static float reloadTimer;
    static bool cursorLocked;

    static float trauma;
    static float zoomPunch;
    static float flash;
    static Color flashColor = Danger;
    static float hitstop;
    static float impactFlash;
    static Color impactFlashColor = Color.White;
    static bool impactFlashSharp;
    static float adrenaline;
    static float glowPulse;
    static float cameraDashLean;
    static Vector2 cameraDashLeanDir;
    static RenderTexture2D sceneTarget;
    static Shader uhdCompositeShader;
    static bool uhdShaderReady;
    static int uhdLocResolution;
    static int uhdLocTime;
    static int uhdLocSharpen;
    static int uhdLocVignette;
    static int uhdLocGrain;
    static int uhdLocExposure;
    static int uhdLocWarmth;
    static int uhdLocAdrenaline;
    static bool gfxReady;
    static bool UhdShadersActive => uhdShaderReady && uhdShaders;
    static readonly List<GfxLightPulse> gfxLightPulses = new();
    static float menuTime;
    static float frameTime;
    static float aiIntelTimer;
    static float aiFutureRamTimer;
    static RenderTexture2D menuCastleBake;
    static bool menuCastleBakeReady;
    static int menuCastleBakeW;
    static int menuCastleBakeH;
    static MenuCastleLayout menuCastleLayoutCached;
    static bool menuCastleLayoutValid;

    struct GfxLightPulse
    {
        public Vector2 Position;
        public Color Color;
        public float Radius;
        public float Intensity;
        public float Life;
        public float MaxLife;
    }

    // Smoothed visual meters (delta-time interpolated, never jagged)
    static float[] abilityFillVis = new float[2];
    static float comboFillVis;
    static float[] shopBarVis = null!;

    // Immersion / meta expansion
    static float nearDeathPulse;
    static int lastComboNarrationTier;
    static float betweenWaveVignetteTimer;
    static int[] bestiaryKills = null!;
    static readonly string[] MottoLines =
    {
        "Hold the line.", "Stone endures.", "The bailey remembers.",
        "No retreat but death.", "Crown or crumble.", "Stillness is victory.",
    };
    static readonly int[] MottoLevelReq = { 10, 20, 35, 50, 65, 80 };
    static bool[] mottoUnlocked = null!;
    static readonly string[] ChronicleBuffer = new string[5];
    static int chronicleCount;
    static int chronicleWrite;
    struct DifficultyRecord { public int BestWave; public int BestScore; public int BestKills; }
    static DifficultyRecord[] difficultyRecords = null!;
    static bool heraldryPatterns;
    static BlessingType[] activeBlessings = new BlessingType[6];
    static int activeBlessingCount;
    static bool blessingPickActive;
    static BlessingType[] blessingChoices = new BlessingType[3];
    static int runOathFlags;
    static SiegeObjectiveType siegeObjective;
    static float siegeObjectiveTimer;
    static int siegeCornerTx;
    static int siegeCornerTy;
    static bool siegeObjectiveDone;
    static bool siegeObjectiveFailed;
    static int siegeGruntsSpawned;
    static int siegeGruntsKilled;
    static int runLockedBodyIndex = -1;
    static float reinforceCooldown;
    static GunAffixType runGunAffix;
    static bool royalPardonUsed;
    static bool eventChainActive;
    static int[] playerTrailTiles = new int[8];
    static int playerTrailWrite;
    static bool reverseSiegeActive;
    static float reverseSiegeTimer;
    static readonly int[] playerTrailX = new int[8];
    static readonly int[] playerTrailY = new int[8];
    const int AccessoryStormGlass = 37;
    const int AccessoryKeepBanner = 21;
    const int UpLockstep = 9;
    const int ReinforceFableBase = 10;
    const float ReinforceCooldownTime = 4.5f;
    const float ReinforceFortifiedMult = 1.45f;
    const float ReinforceFreshTimer = 10f;
    const int ReinforceRadius = 1;

    // Persistent meta
    static int fables;
    static int playerLevel = 1;
    static int playerXp;
    static int maxWaveReached;
    static float levelUpBannerTimer;
    static float customizeScroll;
    static float settingsScroll;
    static GameState settingsReturnState = GameState.MainMenu;
    static int uiInputBlockFrames;
    static string settingsTypeBuffer = "";
    static float settingsEggBannerTimer;
    static float levelBarVis;
    static int equippedGun;
    static bool[] gunUnlocked = null!;
    static int[] upgradeLevels = null!;
    static int bodyColorIndex;
    static int accessoryIndex;
    static bool[] bodyUnlocked = null!;
    static bool[] accessoryUnlocked = null!;

    static Difficulty runDifficulty = Difficulty.Knight;
    static int difficultyMenuIndex = (int)Difficulty.Knight;
    static float difficultySelectAnim;
    static DifficultyProfile activeDifficulty;
    static float eventSurgeTimer;

    static readonly DifficultyProfile[] DifficultyProfiles =
    {
        new("TOTAL BEGINNER", "Learn the stones in peace.",
            "Fewer foes � gentle swarms � catastrophes arrive very late and stay mild.",
            new Color(118, 176, 138, 255), new Color(168, 220, 182, 255),
            0.52f, 0.68f, 0.45f, 0.55f,
            6, 0f, 10f, 15f, 0.55f, 1.55f, 1.45f, 0f, 0f, true, 4),
        new("SQUIRE", "A forgiving tour of the siege.",
            "Reduced pressure � slower catastrophes � room to learn your kit.",
            new Color(132, 158, 196, 255), new Color(178, 204, 236, 255),
            0.72f, 0.82f, 0.65f, 0.75f,
            5, 0f, 8f, 14f, 0.72f, 1.25f, 1.2f, 0f, 0f, false, 2),
        new("KNIGHT", "The siege as it was meant to be felt.",
            "Balanced waves � standard arena rhythm � fair but unforgiving.",
            new Color(196, 168, 108, 255), new Color(228, 206, 156, 255),
            1f, 1f, 1f, 1f,
            4, 0f, FloorEventCooldownMin, FloorEventCooldownMax, 1f, 1f, 1f, 0f, 0f, false, 0),
        new("CHAMPION", "The walls remember every mistake.",
            "Heavier swarms � faster catastrophes � elite foes arrive sooner.",
            new Color(196, 128, 72, 255), new Color(236, 176, 108, 255),
            1.22f, 1.12f, 1.28f, 1.22f,
            3, 0f, 4f, 10f, 1.22f, 0.82f, 0.82f, 0.15f, 0.18f, false, -1),
        new("FABLE (NIGHTMARE)", "The story ends in blood and falling stone.",
            "Events from the first breath � catastrophes stack � the arena never rests.",
            new Color(132, 10, 22, 255), new Color(210, 34, 48, 255),
            1.58f, 1.38f, 1.65f, 1.5f,
            0, 0f, 2f, 6f, 1.65f, 0.58f, 0.62f, 0.42f, 0.35f, false, -3),
        new("PRACTICE HALL", "Learn catastrophes without the swarm.",
            "No grunts � gentle event timers � scores stay off the record.",
            new Color(148, 168, 196, 255), new Color(196, 214, 236, 255),
            0f, 0f, 0f, 0f,
            0, 2f, 6f, 12f, 0.45f, 2f, 2.5f, 0f, 0f, true, 99),
    };

    static readonly FloorEventType[] EasyFloorEventPool =
    {
        FloorEventType.MossRot, FloorEventType.ScatterPits, FloorEventType.SafeZoneRush,
        FloorEventType.StoneIslands, FloorEventType.CenterSnare, FloorEventType.TideBeacon,
        FloorEventType.EmberGate, FloorEventType.CryptTorch, FloorEventType.CryptLantern,
        FloorEventType.CryptMist,
    };

    static DifficultyProfile GetDifficultyProfile(Difficulty d)
        => DifficultyProfiles[Math.Clamp((int)d, 0, DifficultyProfiles.Length - 1)];

    static float NextEventCooldownSpan()
    {
        float min = activeDifficulty.EventCooldownMin;
        float max = Math.Min(15f, Math.Max(min + 0.01f, activeDifficulty.EventCooldownMax));
        return min + Rng.NextSingle() * (max - min);
    }

    // Settings
    static float shakeScale = 1f;
    static bool flashEnabled = true;
    static bool uhdEnabled = true;
    static bool uhdShaders = true;
    static bool autoFire = true;
    static KeyboardKey fireKey = KeyboardKey.J;
    static KeyboardKey abilityKey1 = KeyboardKey.Q;
    static KeyboardKey abilityKey2 = KeyboardKey.Space;
    static RebindTarget rebindTarget;
    static AbilityType abilitySlot1 = AbilityType.Paralyze;
    static AbilityType abilitySlot2 = AbilityType.WindStep;
    static bool[] abilityUnlocked = null!;
    static bool showControlLegend = true;
    static bool floatingTextEnabled = true;
    static bool reduceMotion = false;
    static float vignetteScale = 1f;
    static float particleDensity = 1f;
    static bool backgroundMotes = true;
    static bool menuCastleEnabled = true;
    static bool showFps;
    static int fpsCap;
    static bool showTopHud = true;
    static bool showWeaponHud = true;
    static bool showAbilityHud = true;
    static bool showComboMeter = true;
    static bool showWaveBanner = true;
    static bool showFloorEventHud = true;
    static bool showEventWarningBorder = true;
    static bool showEnemyTelegraphs = true;
    static bool showEnemyHealthBars = true;
    static bool showBossHud = true;
    static bool showLevelUpBanner = true;
    static bool lockCursorInGame = true;
    static bool hitStopEnabled = true;
    static bool pauseOnFocusLoss;
    static float filmGrainScale = 1f;
    static float bloomScale = 1f;
    static bool autoPausedForFocus;

    // UI
    static CustomizeTab customizeTab = CustomizeTab.Cosmetics;

    // AI pilot
    static bool aiPilotEnabled;
    static Vector2 aiMove;
    static bool aiDashRequest;
    static bool aiParalyzeRequest;
    static float aiDashUrgency;
    static float aiPostParalyzeGrace;
    static bool dashBlockedThisFrame;
    static float aiGameOverTimer;
    static float aiPilotBannerTimer;
    static Vector2 aiAnchorPos;
    static Vector2 aiSteerVel;
    static Vector2 aiMoveSmoothed;
    static float aiTopBandTimer;
    static float aiCrumbleDashUrgency;
    static bool windowDragging;
    static bool windowCloseHovered;
    static float[,] aiTileValue = null!;
    static float[,] aiTileHazard = null!;

}
