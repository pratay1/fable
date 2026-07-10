partial class Program
{
    // ---------------------------------------------------------------- AI GRANDMASTER BRAIN
    // Self-contained in Program.cs - pathfinding, prediction, events, behaviors, scenario engine.

    enum AiSurvivalPhase { Calm, Patrol, Combat, EventPrep, EventActive, BossFight, TileCritical, LastStand }

    struct AiBehaviorProfile
    {
        public EnemyBehavior Behavior;
        public float ZoneMul;
        public float PanicMul;
        public float TileThreat;
        public float Priority;
        public float KiteDist;
        public bool AvoidLane;
        public bool PreemptDash;
    }

    struct AiEventProfile
    {
        public FloorEventType Event;
        public float PrepThreshold;
        public float PanicThreshold;
        public float HoldMargin;
        public bool UsesCluster;
        public bool UsesSafeRect;
        public bool UsesCenter;
        public bool UsesEdge;
        public bool MarkedSensitive;
    }

    static AiSurvivalPhase aiPhase;
    static float aiPhaseTimer;
    static float aiLastVerdictAttempt;
    static float aiBannerHoldTimer;
    static Vector2 aiPathGoal;
    static bool aiPathGoalValid;
    static float aiPathRecalcTimer;
    static readonly bool[,] aiClosedSet = new bool[GridSize, GridSize];
    static readonly float[,] aiPathG = new float[GridSize, GridSize];
    static readonly int[,] aiPathParentX = new int[GridSize, GridSize];
    static readonly int[,] aiPathParentY = new int[GridSize, GridSize];
    static readonly List<(int X, int Y)> aiPathNodes = new();
    static readonly float[,] aiPredictedHazard = new float[GridSize, GridSize];
    static readonly float[,] aiPredictedValue = new float[GridSize, GridSize];
    static bool aiPredictFieldsValid;
    static float aiPredictRecalcTimer;
    static bool aiVerdictRequest;
    static bool aiBannerRequest;
    static bool aiOathPrimed;
    static Vector2 aiBannerTarget;
    static float aiCombatRetreatTimer;
    static int aiFocusEnemyIndex = -1;
    static float aiWaveConservatism;
    static float aiScenarioScoreCache;
    static bool aiScenarioFlagsDirty = true;
    static bool aiScenarioHasCrusher;
    static bool aiScenarioHasLeech;
    static bool aiScenarioHasFast;
    static readonly List<(float F, int X, int Y)> aiPathOpen = new();

    const float AiFutureHorizonSec = 3f;
    const float AiFutureStepSec = 0.1f;
    const int AiFutureHorizonSteps = 30;

    struct AiFutureRamEntry
    {
        public float TimeFromNow;
        public FloorEventType Event;
        public float EventCountdown;
        public int EventPhase;
        public bool EventActive;
        public bool EventIncoming;
        public bool CollapseActive;
        public int QueuedCollapseTiles;
        public float SecondsUntilNextEvent;
    }

    struct AiFutureSimCtx
    {
        public FloorEventType Ev;
        public float Countdown;
        public float StartCountdown;
        public int Phase;
        public float ActionTimer;
        public float NextEventTimer;
        public float NextEventCooldown;
        public float VerdictHalt;
        public float WaveBanner;
        public int EventStep;
        public int StrikeCount;
        public Vector2 Epicenter;
        public int EventSide;
        public Rectangle SafeRect;
        public Rectangle DangerRect;
        public bool IncomingPending;
        public float IncomingCountdown;
    }

    static readonly AiFutureRamEntry[] aiFutureEventRam = new AiFutureRamEntry[AiFutureHorizonSteps];
    static readonly float[,,] aiFutureTileHazardRam = new float[AiFutureHorizonSteps, GridSize, GridSize];
    static bool aiFutureRamValid;
    static readonly bool[,] aiFutureSimMarked = new bool[GridSize, GridSize];
    static readonly bool[,] aiFutureSimCollapsed = new bool[GridSize, GridSize];
    static readonly List<(int X, int Y)> aiFutureSimQueue = new();
    static readonly List<(int X, int Y, float Sort)> aiFutureSortScratch = new();
    static readonly int[] aiFutureSimStrikeOrder = new int[GridSize * GridSize];

    static AiBehaviorProfile AiBehaviorProfileFor(EnemyBehavior b) => b switch
    {
        EnemyBehavior.Chase => new() { Behavior = EnemyBehavior.Chase, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.FastChase => new() { Behavior = EnemyBehavior.FastChase, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 110f, AvoidLane = false, PreemptDash = true },
        EnemyBehavior.CrushTiles => new() { Behavior = EnemyBehavior.CrushTiles, ZoneMul = 1.6f, PanicMul = 1.8f, TileThreat = 2.5f, Priority = 2.2f, KiteDist = 85f, AvoidLane = true, PreemptDash = true },
        EnemyBehavior.Orbit => new() { Behavior = EnemyBehavior.Orbit, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 140f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Zigzag => new() { Behavior = EnemyBehavior.Zigzag, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Hop => new() { Behavior = EnemyBehavior.Hop, ZoneMul = 1.0f, PanicMul = 1.8f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 85f, AvoidLane = true, PreemptDash = true },
        EnemyBehavior.BlightTrail => new() { Behavior = EnemyBehavior.BlightTrail, ZoneMul = 1.6f, PanicMul = 1.1f, TileThreat = 2.5f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.TileLeech => new() { Behavior = EnemyBehavior.TileLeech, ZoneMul = 1.6f, PanicMul = 1.1f, TileThreat = 2.5f, Priority = 2.2f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Kite => new() { Behavior = EnemyBehavior.Kite, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 140f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Charge => new() { Behavior = EnemyBehavior.Charge, ZoneMul = 1.0f, PanicMul = 1.8f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 110f, AvoidLane = true, PreemptDash = true },
        EnemyBehavior.PulseBlight => new() { Behavior = EnemyBehavior.PulseBlight, ZoneMul = 1.6f, PanicMul = 1.1f, TileThreat = 2.5f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Phaser => new() { Behavior = EnemyBehavior.Phaser, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 140f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Rotburst => new() { Behavior = EnemyBehavior.Rotburst, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 2.5f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Splitter => new() { Behavior = EnemyBehavior.Splitter, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Sapper => new() { Behavior = EnemyBehavior.Sapper, ZoneMul = 1.6f, PanicMul = 1.1f, TileThreat = 2.5f, Priority = 2.2f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.Lurker => new() { Behavior = EnemyBehavior.Lurker, ZoneMul = 1.0f, PanicMul = 1.1f, TileThreat = 0.6f, Priority = 1.0f, KiteDist = 140f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.BossBlight => new() { Behavior = EnemyBehavior.BossBlight, ZoneMul = 1.8f, PanicMul = 2.2f, TileThreat = 2.5f, Priority = 3.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        EnemyBehavior.BossDash => new() { Behavior = EnemyBehavior.BossDash, ZoneMul = 1.8f, PanicMul = 2.2f, TileThreat = 0.6f, Priority = 3.0f, KiteDist = 85f, AvoidLane = true, PreemptDash = true },
        EnemyBehavior.BossSmash => new() { Behavior = EnemyBehavior.BossSmash, ZoneMul = 1.8f, PanicMul = 2.2f, TileThreat = 2.5f, Priority = 3.0f, KiteDist = 85f, AvoidLane = false, PreemptDash = false },
        _ => new() { Behavior = b, ZoneMul = 1f, PanicMul = 1f, TileThreat = 0.8f, Priority = 1f, KiteDist = 90f, AvoidLane = false, PreemptDash = false },
    };

    static AiEventProfile AiEventProfileFor(FloorEventType e) => e switch
    {
        FloorEventType.CrimsonCrumble => new() { Event = FloorEventType.CrimsonCrumble, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.SafeZoneRush => new() { Event = FloorEventType.SafeZoneRush, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.Checkerfall => new() { Event = FloorEventType.Checkerfall, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.RingCollapse => new() { Event = FloorEventType.RingCollapse, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.StoneIslands => new() { Event = FloorEventType.StoneIslands, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.ScatterPits => new() { Event = FloorEventType.ScatterPits, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.MossRot => new() { Event = FloorEventType.MossRot, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.MarkedStrike => new() { Event = FloorEventType.MarkedStrike, PrepThreshold = 4.5f, PanicThreshold = 2.2f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = true },
        FloorEventType.CenterSnare => new() { Event = FloorEventType.CenterSnare, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = true, UsesEdge = true, MarkedSensitive = false },
        FloorEventType.BlightStorm => new() { Event = FloorEventType.BlightStorm, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideSurge => new() { Event = FloorEventType.TideSurge, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideRift => new() { Event = FloorEventType.TideRift, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideRecede => new() { Event = FloorEventType.TideRecede, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideColumn => new() { Event = FloorEventType.TideColumn, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideEcho => new() { Event = FloorEventType.TideEcho, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideUndertow => new() { Event = FloorEventType.TideUndertow, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideCrest => new() { Event = FloorEventType.TideCrest, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideWall => new() { Event = FloorEventType.TideWall, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideAnchor => new() { Event = FloorEventType.TideAnchor, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideFoam => new() { Event = FloorEventType.TideFoam, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberRain => new() { Event = FloorEventType.EmberRain, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberGate => new() { Event = FloorEventType.EmberGate, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberPulse => new() { Event = FloorEventType.EmberPulse, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberCross => new() { Event = FloorEventType.EmberCross, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberBridge => new() { Event = FloorEventType.EmberBridge, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberFury => new() { Event = FloorEventType.EmberFury, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberSnake => new() { Event = FloorEventType.EmberSnake, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberHive => new() { Event = FloorEventType.EmberHive, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberTide => new() { Event = FloorEventType.EmberTide, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberCage => new() { Event = FloorEventType.EmberCage, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = true, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberQuake => new() { Event = FloorEventType.EmberQuake, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberBloom => new() { Event = FloorEventType.EmberBloom, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.EmberAltar => new() { Event = FloorEventType.EmberAltar, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = true, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptSeal => new() { Event = FloorEventType.CryptSeal, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = true, UsesEdge = true, MarkedSensitive = false },
        FloorEventType.CryptWail => new() { Event = FloorEventType.CryptWail, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptTorch => new() { Event = FloorEventType.CryptTorch, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptChains => new() { Event = FloorEventType.CryptChains, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptMist => new() { Event = FloorEventType.CryptMist, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptTomb => new() { Event = FloorEventType.CryptTomb, PrepThreshold = 4.5f, PanicThreshold = 2.2f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = true },
        FloorEventType.CryptShroud => new() { Event = FloorEventType.CryptShroud, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptGlimpse => new() { Event = FloorEventType.CryptGlimpse, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptRattle => new() { Event = FloorEventType.CryptRattle, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptEcho => new() { Event = FloorEventType.CryptEcho, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptGrave => new() { Event = FloorEventType.CryptGrave, PrepThreshold = 4.5f, PanicThreshold = 2.2f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = true },
        FloorEventType.CryptLantern => new() { Event = FloorEventType.CryptLantern, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CryptVeil => new() { Event = FloorEventType.CryptVeil, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = true, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideWhirlpool => new() { Event = FloorEventType.TideWhirlpool, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideBeacon => new() { Event = FloorEventType.TideBeacon, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.TideStrike => new() { Event = FloorEventType.TideStrike, PrepThreshold = 4.5f, PanicThreshold = 2.2f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = true },
        FloorEventType.CrownTrial => new() { Event = FloorEventType.CrownTrial, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownFall => new() { Event = FloorEventType.CrownFall, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownShard => new() { Event = FloorEventType.CrownShard, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownThrone => new() { Event = FloorEventType.CrownThrone, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = true, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownEdict => new() { Event = FloorEventType.CrownEdict, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.72f, UsesCluster = false, UsesSafeRect = true, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownRot => new() { Event = FloorEventType.CrownRot, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownBolt => new() { Event = FloorEventType.CrownBolt, PrepThreshold = 4.5f, PanicThreshold = 2.2f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = true },
        FloorEventType.CrownRing => new() { Event = FloorEventType.CrownRing, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownIsles => new() { Event = FloorEventType.CrownIsles, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownStorm => new() { Event = FloorEventType.CrownStorm, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownCoronation => new() { Event = FloorEventType.CrownCoronation, PrepThreshold = 2.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = false, UsesSafeRect = false, UsesCenter = true, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownUsurpation => new() { Event = FloorEventType.CrownUsurpation, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.CrownReckoning => new() { Event = FloorEventType.CrownReckoning, PrepThreshold = 3.8f, PanicThreshold = 1.6f, HoldMargin = 0.65f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
        FloorEventType.None => default,
        _ => new() { Event = e, PrepThreshold = 3.5f, PanicThreshold = 1.8f, HoldMargin = 0.6f, UsesCluster = true, UsesSafeRect = false, UsesCenter = false, UsesEdge = false, MarkedSensitive = false },
    };

    static void ResetAiGrandmasterBrain()
    {
        ResetAiHumanSurvivalDirector();
        aiPhase = AiSurvivalPhase.Calm;
        aiPhaseTimer = 0f;
        aiLastVerdictAttempt = -999f;
        aiBannerHoldTimer = 0f;
        aiPathGoalValid = false;
        aiPathRecalcTimer = 0f;
        aiPredictFieldsValid = false;
        aiPredictRecalcTimer = 0f;
        aiVerdictRequest = false;
        aiBannerRequest = false;
        aiOathPrimed = false;
        aiCombatRetreatTimer = 0f;
        aiFocusEnemyIndex = -1;
        aiWaveConservatism = 0f;
        aiScenarioScoreCache = 0f;
        aiScenarioFlagsDirty = true;
        aiMoveSmoothed = Vector2.Zero;
        aiPathNodes.Clear();
        aiPathOpen.Clear();
        aiFutureRamValid = false;
        aiFutureSimQueue.Clear();
        aiFutureSortScratch.Clear();
    }

    static void UpdateAiWaveConservatism()
    {
        aiWaveConservatism = Math.Clamp(AiWaveSurvivalTuning(waveNumber), 0f, 1.45f);
    }

    static void EvaluateAiSurvivalPhase()
    {
        bool eventLive = activeEvent != FloorEventType.None && eventCountdown > 0f;
        if (!eventLive && aiFutureRamValid)
        {
            for (int s = 0; s < AiFutureHorizonSteps; s++)
            {
                ref AiFutureRamEntry entry = ref aiFutureEventRam[s];
                if (entry.EventActive || entry.CollapseActive)
                {
                    eventLive = true;
                    break;
                }
            }
        }
        bool bossNear = NearestBossDistance(playerPos) < 220f;
        bool combat = AiEnemiesThreatening(playerPos, 190f) >= 1;
        bool tileBad = IsFeetTileWeak() || AiVoidDashUrgency(playerPos) > 0.55f;

        AiSurvivalPhase next = AiSurvivalPhase.Calm;
        if (tileBad && combat) next = AiSurvivalPhase.LastStand;
        else if (tileBad) next = AiSurvivalPhase.TileCritical;
        else if (bossNear) next = AiSurvivalPhase.BossFight;
        else if (eventLive && activeEvent != FloorEventType.None && eventCountdown < AiEventProfileFor(activeEvent).PrepThreshold) next = AiSurvivalPhase.EventActive;
        else if (eventLive && activeEvent != FloorEventType.None) next = AiSurvivalPhase.EventPrep;
        else if (eventLive) next = AiSurvivalPhase.EventActive;
        else if (aiFutureRamValid && aiFutureEventRam[0].EventIncoming) next = AiSurvivalPhase.EventPrep;
        else if (aiFutureRamValid && aiFutureEventRam[0].SecondsUntilNextEvent < 2.4f && waveNumber >= 3) next = AiSurvivalPhase.EventPrep;
        else if (combat) next = AiSurvivalPhase.Combat;
        else if (waveInProgress) next = AiSurvivalPhase.Patrol;
        else next = AiSurvivalPhase.Calm;

        if (next != aiPhase) aiPhaseTimer = 0f;
        aiPhase = next;
        aiPhaseTimer += Raylib.GetFrameTime();
    }

    static void RebuildAiPredictiveFields(float dt)
    {
        aiPredictRecalcTimer -= dt;
        if (aiPredictRecalcTimer > 0f && aiPredictFieldsValid) return;
        aiPredictRecalcTimer = 0.08f + Math.Clamp(waveNumber * 0.0015f, 0f, 0.06f);
        aiPredictFieldsValid = true;

        float horizon = 2.4f + aiWaveConservatism * 1.8f;
        float wearRate = EffPlayerDecay() * (1f + aiWaveConservatism * 0.35f);
        int liveEnemies = 0;
        foreach (Enemy e in enemies)
        {
            if (!e.Dead && e.Spawn >= 0.5f) liveEnemies++;
        }
        float enemyPressure = Math.Clamp(liveEnemies * 0.04f, 0f, 0.8f);

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile t = ref tiles[x, y];
                float hazard = aiTileHazard[x, y];
                float value = aiTileValue[x, y];

                if (t.State == 2)
                {
                    aiPredictedHazard[x, y] = 1f;
                    aiPredictedValue[x, y] = -99999f;
                    continue;
                }

                float projectedDur = t.Durability - wearRate * horizon * 0.15f;
                if (t.EventMarked) projectedDur -= MaxDurability * 0.85f;
                if (t.State == 1) projectedDur -= MaxDurability * 0.12f * horizon;
                projectedDur -= enemyPressure * MaxDurability * 0.08f * AiBehaviorTilePressureAt(x, y);

                float durRatio = Math.Clamp(projectedDur / MaxDurability, 0f, 1.2f);
                aiPredictedHazard[x, y] = Math.Clamp(hazard + (1f - durRatio) * 0.55f + (t.EventMarked ? 0.35f : 0f), 0f, 1f);
                aiPredictedValue[x, y] = value + durRatio * 180f - aiPredictedHazard[x, y] * 240f;
            }
        }
    }

    static float AiFutureDetSort(int x, int y, int salt)
    {
        uint h = (uint)(x * 73856093 ^ y * 19349663 ^ salt * 83492791);
        return (h & 0xFFFF) / 65535f;
    }

    static int AiFutureStepIndex(float secondsFromNow)
        => Math.Clamp((int)(secondsFromNow / AiFutureStepSec), 0, AiFutureHorizonSteps - 1);

    static float AiFutureHazardAtTile(int x, int y, float secondsFromNow)
    {
        if (!aiFutureRamValid) return aiPredictedHazard[x, y];
        return aiFutureTileHazardRam[AiFutureStepIndex(secondsFromNow), x, y];
    }

    static float AiFutureWorstHazardAtTile(int x, int y, float horizonSec)
    {
        if (!aiFutureRamValid) return aiPredictedHazard[x, y];
        int maxStep = AiFutureStepIndex(horizonSec);
        float worst = 0f;
        for (int s = 0; s <= maxStep; s++)
        {
            worst = MathF.Max(worst, aiFutureTileHazardRam[s, x, y]);
        }
        return worst;
    }

    static float AiFutureRamUrgencyBoost()
    {
        if (!aiFutureRamValid) return 0f;
        float boost = 0f;

        if (activeEvent == FloorEventType.None)
        {
            float until = aiFutureEventRam[0].SecondsUntilNextEvent;
            if (until > 0f && until < 2.5f)
            {
                boost = MathF.Max(boost, (1f - until / 2.5f) * 0.35f);
            }
        }

        if (TryGetTileUnder(playerPos, out int px, out int py))
        {
            for (int s = 0; s < AiFutureHorizonSteps; s++)
            {
                float h = aiFutureTileHazardRam[s, px, py];
                if (h <= 0.75f) continue;
                float t = s * AiFutureStepSec;
                boost = MathF.Max(boost, h * (1f - t / AiFutureHorizonSec) * 0.55f);
            }
        }

        if (activeEvent != FloorEventType.None)
        {
            if (eventPhase == 0 && eventCountdown > 0f && eventCountdown < 1.2f)
            {
                boost = MathF.Max(boost, 0.5f);
            }

            for (int s = 0; s < AiFutureHorizonSteps; s++)
            {
                if (!aiFutureEventRam[s].CollapseActive) continue;
                boost = MathF.Max(boost, 0.42f + (AiFutureHorizonSteps - s) * 0.018f);
                break;
            }
        }

        return Math.Clamp(boost, 0f, 0.45f);
    }

    static bool AiFutureUsesStaggeredMarkedCollapse(FloorEventType ev)
    {
        if (ev == FloorEventType.None || ev == FloorEventType.CrownThrone) return false;
        if (AiFutureUsesSequentialStrike(ev)) return false;
        if (AiFutureUsesSafeRectSurvival(ev)) return false;
        if (AiFutureUsesCenterSnareSurvival(ev)) return false;
        return ev is FloorEventType.CrimsonCrumble or FloorEventType.Checkerfall
            or FloorEventType.RingCollapse or FloorEventType.StoneIslands
            or FloorEventType.ScatterPits or FloorEventType.BlightStorm
            or FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede
            or FloorEventType.TideColumn or FloorEventType.TideEcho or FloorEventType.TideUndertow
            or FloorEventType.TideCrest or FloorEventType.TideWall or FloorEventType.TideAnchor
            or FloorEventType.TideFoam or FloorEventType.EmberRain or FloorEventType.EmberPulse
            or FloorEventType.EmberCross or FloorEventType.EmberBridge or FloorEventType.EmberHive
            or FloorEventType.EmberFury or FloorEventType.EmberSnake or FloorEventType.EmberQuake
            or FloorEventType.EmberBloom or FloorEventType.CrownTrial or FloorEventType.CrownFall
            or FloorEventType.CrownShard or FloorEventType.CrownRing or FloorEventType.CrownIsles
            or FloorEventType.CrownCoronation or FloorEventType.CrownUsurpation or FloorEventType.CrownReckoning
            or FloorEventType.CrownStorm or FloorEventType.CryptSeal or FloorEventType.CryptWail
            or FloorEventType.CryptTorch or FloorEventType.CryptChains or FloorEventType.CryptGlimpse
            or FloorEventType.CryptRattle or FloorEventType.CryptEcho or FloorEventType.CryptShroud
            or FloorEventType.CryptLantern or FloorEventType.CryptTomb or FloorEventType.CryptMist;
    }

    static bool AiFutureUsesSafeRectSurvival(FloorEventType ev) => ev switch
    {
        FloorEventType.CrownThrone => false,
        FloorEventType.SafeZoneRush or FloorEventType.EmberGate or FloorEventType.EmberTide
            or FloorEventType.EmberCage or FloorEventType.TideBeacon or FloorEventType.CryptVeil
            or FloorEventType.CrownEdict or FloorEventType.EmberAltar => true,
        _ => AiEventProfileFor(ev).UsesSafeRect,
    };

    static bool AiFutureUsesCenterSnareSurvival(FloorEventType ev) => ev switch
    {
        FloorEventType.CenterSnare => true,
        _ => AiEventProfileFor(ev).UsesCenter && AiEventProfileFor(ev).UsesEdge,
    };

    static bool AiFutureUsesSequentialStrike(FloorEventType ev) => ev switch
    {
        FloorEventType.MarkedStrike or FloorEventType.TideStrike or FloorEventType.CryptGrave
            or FloorEventType.CrownBolt => true,
        _ => false,
    };

    static void AiFutureBuildCollapseQueue(FloorEventType ev, bool[,] marked, List<(int X, int Y)> queue, Vector2 epicenter, int side)
    {
        queue.Clear();
        aiFutureSortScratch.Clear();
        int salt = (int)ev;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (!marked[x, y] || aiFutureSimCollapsed[x, y]) continue;
                float sort = ev switch
                {
                    FloorEventType.CrimsonCrumble => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.Checkerfall => x + y + (((x + y) / 2) % 2) * 0.01f,
                    FloorEventType.RingCollapse => TileEdgeDistance(x, y),
                    FloorEventType.ScatterPits => AiFutureDetSort(x, y, salt),
                    FloorEventType.StoneIslands => Vector2.DistanceSquared(TileCenter(x, y), epicenter),
                    FloorEventType.BlightStorm => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.TideSurge => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.TideRift => MathF.Abs(x - GridSize / 2f) + MathF.Abs(y - GridSize / 2f),
                    FloorEventType.TideRecede => -TileEdgeDistance(x, y),
                    FloorEventType.TideColumn => x + AiFutureDetSort(x, y, salt) * 0.01f,
                    FloorEventType.TideEcho => x - y,
                    FloorEventType.TideUndertow => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f) + Vector2.DistanceSquared(TileCenter(x, y), epicenter) * 0.001f,
                    FloorEventType.TideCrest => y + MathF.Sin(x * 0.4f) * 0.01f,
                    FloorEventType.TideWall => side switch
                    {
                        0 => x,
                        1 => GridSize - x,
                        2 => y,
                        _ => GridSize - y,
                    },
                    FloorEventType.TideAnchor => Vector2.DistanceSquared(TileCenter(x, y), epicenter),
                    FloorEventType.TideFoam => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.EmberRain => side == 0 ? x : GridSize - 1 - x,
                    FloorEventType.EmberPulse => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.EmberCross => Math.Abs(x - GridSize / 2f) + Math.Abs(y - GridSize / 2f),
                    FloorEventType.EmberBridge => Math.Min(Math.Abs(y - 5f), Math.Abs(y - (GridSize - 7f))),
                    FloorEventType.EmberHive => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.EmberFury => AiFutureDetSort(x, y, salt + 17),
                    FloorEventType.EmberQuake => AiFutureDetSort(x, y, salt + 31),
                    FloorEventType.EmberBloom => EmberBloomSortKey(x, y),
                    FloorEventType.CryptSeal => GridCenterDistanceSq(x, y),
                    FloorEventType.CryptWail => side switch
                    {
                        0 => x + y * 0.001f,
                        1 => GridSize - x + y * 0.001f,
                        2 => y + x * 0.001f,
                        _ => GridSize - y + x * 0.001f,
                    },
                    FloorEventType.CryptTorch => CryptTorchCorridorDistance(x, y),
                    FloorEventType.CryptChains => x * 1000f + y,
                    FloorEventType.CryptMist => MathF.Atan2(y - GridSize / 2f, x - GridSize / 2f),
                    FloorEventType.CryptTomb => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.CryptShroud => CryptShroudDiagonalDistance(x, y),
                    FloorEventType.CryptGlimpse => AiFutureDetSort(x, y, salt + 53),
                    FloorEventType.CryptRattle => x + y,
                    FloorEventType.CryptEcho => TileEdgeDistance(x, y) * 2f + (TileEdgeDistance(x, y) % 2) * 0.001f,
                    FloorEventType.CryptLantern => -GridCenterDistanceSq(x, y),
                    FloorEventType.CrownTrial => Vector2.DistanceSquared(epicenter, TileCenter(x, y)),
                    FloorEventType.CrownFall => x + y + (((x + y) / 2) % 2) * 0.01f,
                    FloorEventType.CrownShard => AiFutureDetSort(x, y, salt + 71),
                    FloorEventType.CrownRing => TileEdgeDistance(x, y),
                    FloorEventType.CrownIsles => Vector2.DistanceSquared(TileCenter(x, y), epicenter),
                    FloorEventType.CrownCoronation => TileEdgeDistance(x, y),
                    FloorEventType.CrownUsurpation => Math.Min(Math.Abs(x - GridSize / 2f), Math.Abs(y - GridSize / 2f)),
                    FloorEventType.CrownReckoning => x + y + (x + y) % 3 * 0.001f,
                    FloorEventType.CrownThrone => TileEdgeDistance(x, y),
                    _ => Vector2.DistanceSquared(epicenter, TileCenter(x, y)) + AiFutureDetSort(x, y, salt) * 0.001f,
                };
                aiFutureSortScratch.Add((x, y, sort));
            }
        }

        aiFutureSortScratch.Sort((a, b) => a.Sort.CompareTo(b.Sort));
        foreach ((int x, int y, _) in aiFutureSortScratch)
        {
            queue.Add((x, y));
        }
    }

    static void AiFutureSimProcessCollapse(ref AiFutureSimCtx sim, float dt)
    {
        sim.ActionTimer -= dt;
        int burst = EventCollapseBurst(sim.Ev);
        float interval = EventCollapseInterval(sim.Ev);
        while (sim.ActionTimer <= 0f && aiFutureSimQueue.Count > 0)
        {
            for (int b = 0; b < burst && aiFutureSimQueue.Count > 0; b++)
            {
                (int x, int y) = aiFutureSimQueue[0];
                aiFutureSimQueue.RemoveAt(0);
                aiFutureSimCollapsed[x, y] = true;
                aiFutureSimMarked[x, y] = false;
            }
            sim.ActionTimer += interval;
        }

        if (aiFutureSimQueue.Count == 0 && sim.Phase == 1)
        {
            sim.Ev = FloorEventType.None;
            sim.Phase = 0;
            sim.Countdown = 0f;
        }
    }

    static void AiFutureSimTelegraphCollapse(ref AiFutureSimCtx sim, float dt)
    {
        if (sim.Phase == 0)
        {
            sim.Countdown -= dt;
            if (sim.Countdown <= 0f)
            {
                sim.Phase = 1;
                sim.ActionTimer = 0f;
                AiFutureBuildCollapseQueue(sim.Ev, aiFutureSimMarked, aiFutureSimQueue, sim.Epicenter, sim.EventSide);
            }
        }
        else
        {
            AiFutureSimProcessCollapse(ref sim, dt);
        }
    }

    static void AiFutureSimSequentialStrike(ref AiFutureSimCtx sim, float dt)
    {
        if (sim.EventStep >= sim.StrikeCount)
        {
            sim.Ev = FloorEventType.None;
            return;
        }

        int idx = aiFutureSimStrikeOrder[sim.EventStep];
        int mx = idx % GridSize;
        int my = idx / GridSize;

        if (sim.Phase == 0)
        {
            sim.Countdown -= dt;
            if (sim.Countdown <= 0f)
            {
                sim.Phase = 1;
                sim.Countdown = MarkedStrikeFireTime;
            }
        }
        else
        {
            sim.Countdown -= dt;
            if (sim.Countdown <= 0f)
            {
                aiFutureSimCollapsed[mx, my] = true;
                aiFutureSimMarked[mx, my] = false;
                sim.EventStep++;
                sim.Phase = 0;
                sim.Countdown = MarkedStrikeStrikeGap;
            }
        }
    }

    static void AiFutureSimAdvanceActiveEvent(ref AiFutureSimCtx sim, float dt)
    {
        if (sim.Ev == FloorEventType.CrownThrone)
        {
            if (sim.Phase == 0)
            {
                sim.Countdown -= dt;
                if (sim.Countdown <= 0f)
                {
                    sim.Phase = 1;
                    sim.ActionTimer = 0f;
                    AiFutureBuildCollapseQueue(sim.Ev, aiFutureSimMarked, aiFutureSimQueue, sim.Epicenter, sim.EventSide);
                }
            }
            else
            {
                AiFutureSimProcessCollapse(ref sim, dt);
            }
            return;
        }

        if (AiFutureUsesSequentialStrike(sim.Ev))
        {
            AiFutureSimSequentialStrike(ref sim, dt);
            return;
        }

        if (AiFutureUsesSafeRectSurvival(sim.Ev) || AiFutureUsesCenterSnareSurvival(sim.Ev))
        {
            sim.Countdown -= dt;
            if (sim.Countdown <= 0f) sim.Ev = FloorEventType.None;
            return;
        }

        if (AiFutureUsesStaggeredMarkedCollapse(sim.Ev))
        {
            AiFutureSimTelegraphCollapse(ref sim, dt);
            return;
        }

        sim.Countdown -= dt;
        if (sim.Countdown <= 0f) sim.Ev = FloorEventType.None;
    }

    static void AiFutureSimStep(ref AiFutureSimCtx sim, float dt)
    {
        if (sim.VerdictHalt > 0f)
        {
            sim.VerdictHalt = Math.Max(0f, sim.VerdictHalt - dt);
            return;
        }

        if (sim.WaveBanner > 0f) sim.WaveBanner = Math.Max(0f, sim.WaveBanner - dt);

        if (sim.Ev != FloorEventType.None)
        {
            AiFutureSimAdvanceActiveEvent(ref sim, dt);
            return;
        }

        if (sim.IncomingPending)
        {
            sim.IncomingCountdown -= dt;
            if (sim.IncomingCountdown <= 0f) sim.IncomingPending = false;
            return;
        }

        if (waveNumber < 3) return;

        sim.NextEventTimer += dt;
        if (sim.NextEventTimer < sim.NextEventCooldown || sim.WaveBanner > 0f) return;

        sim.NextEventTimer = 0f;
        sim.NextEventCooldown = (FloorEventCooldownMin + FloorEventCooldownMax) * 0.5f;
        sim.IncomingPending = true;
        sim.IncomingCountdown = 5.5f;
    }

    static bool AiFutureTileInAltarSafe(int x, int y, Vector2 epicenter)
    {
        int ax = Math.Clamp((int)(epicenter.X / TileSize), 0, GridSize - 1);
        int ay = Math.Clamp((int)(epicenter.Y / TileSize), 0, GridSize - 1);
        return Math.Abs(x - ax) <= 1 && Math.Abs(y - ay) <= 1;
    }

    static void AiFutureWriteTileHazardRam(int step, in AiFutureSimCtx sim)
    {
        float telegraphPressure = 0f;
        if (sim.Ev != FloorEventType.None && sim.Phase == 0 && sim.StartCountdown > 0.05f)
        {
            telegraphPressure = 1f - Math.Clamp(sim.Countdown / sim.StartCountdown, 0f, 1f);
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float hazard = aiTileHazard[x, y];

                if (aiFutureSimCollapsed[x, y] || tiles[x, y].State == 2)
                {
                    hazard = 1f;
                }
                else if (aiFutureSimMarked[x, y])
                {
                    hazard = MathF.Max(hazard, 0.42f + telegraphPressure * 0.5f);
                    if (sim.Phase == 1) hazard = MathF.Max(hazard, 0.88f);
                }

                if (sim.Ev != FloorEventType.None && sim.Countdown > 0f)
                {
                    if (AiFutureUsesSafeRectSurvival(sim.Ev) && sim.SafeRect.Width > 0f && sim.SafeRect.Height > 0f)
                    {
                        Vector2 center = TileCenter(x, y);
                        bool inSafe = sim.Ev == FloorEventType.EmberAltar
                            ? AiFutureTileInAltarSafe(x, y, sim.Epicenter)
                            : Raylib.CheckCollisionPointRec(center, sim.SafeRect);
                        if (!inSafe)
                        {
                            float pressure = 1f - Math.Clamp(sim.Countdown / Math.Max(sim.StartCountdown, 0.5f), 0f, 1f);
                            hazard = MathF.Max(hazard, 0.38f + pressure * 0.62f);
                        }
                    }
                    else if (AiFutureUsesCenterSnareSurvival(sim.Ev) && sim.DangerRect.Width > 0f && sim.DangerRect.Height > 0f)
                    {
                        Vector2 center = TileCenter(x, y);
                        if (Raylib.CheckCollisionPointRec(center, sim.DangerRect))
                        {
                            float pressure = 1f - Math.Clamp(sim.Countdown / Math.Max(sim.StartCountdown, 0.5f), 0f, 1f);
                            hazard = MathF.Max(hazard, 0.4f + pressure * 0.58f);
                        }
                    }
                }

                if (sim.Ev != FloorEventType.None && AiFutureUsesSequentialStrike(sim.Ev) && sim.EventStep < sim.StrikeCount)
                {
                    int idx = aiFutureSimStrikeOrder[sim.EventStep];
                    int sx = idx % GridSize;
                    int sy = idx / GridSize;
                    if (x == sx && y == sy)
                    {
                        float strikePressure = sim.Phase == 0
                            ? 1f - Math.Clamp(sim.Countdown / MarkedStrikeTelegraphTime, 0f, 1f)
                            : 0.95f;
                        hazard = MathF.Max(hazard, 0.55f + strikePressure * 0.42f);
                    }
                }

                if (sim.IncomingPending)
                {
                    hazard = MathF.Max(hazard, (1f - sim.IncomingCountdown / 5.5f) * 0.22f);
                }

                aiFutureTileHazardRam[step, x, y] = Math.Clamp(hazard, 0f, 1f);
            }
        }
    }

    static void AiFutureWriteEventRam(int step, float timeFromNow, in AiFutureSimCtx sim)
    {
        float untilNext = sim.Ev == FloorEventType.None && !sim.IncomingPending
            ? Math.Max(0f, sim.NextEventCooldown - sim.NextEventTimer)
            : sim.IncomingPending ? sim.IncomingCountdown : 0f;

        aiFutureEventRam[step] = new AiFutureRamEntry
        {
            TimeFromNow = timeFromNow,
            Event = sim.Ev,
            EventCountdown = sim.Countdown,
            EventPhase = sim.Phase,
            EventActive = sim.Ev != FloorEventType.None,
            EventIncoming = sim.IncomingPending,
            CollapseActive = sim.Phase == 1 && sim.Ev != FloorEventType.None,
            QueuedCollapseTiles = aiFutureSimQueue.Count,
            SecondsUntilNextEvent = untilNext,
        };
    }

    static void RebuildAiFutureRam()
    {
        aiFutureRamValid = true;

        var sim = new AiFutureSimCtx
        {
            Ev = activeEvent,
            Countdown = eventCountdown,
            StartCountdown = eventStartCountdown > 0.01f ? eventStartCountdown : Math.Max(eventCountdown, 5f),
            Phase = eventPhase,
            ActionTimer = eventActionTimer,
            NextEventTimer = nextFloorEventTimer,
            NextEventCooldown = nextFloorEventCooldown,
            VerdictHalt = verdictHaltTimer,
            WaveBanner = waveBannerTimer,
            EventStep = eventStep,
            StrikeCount = markedStrikeCount,
            Epicenter = eventEpicenter,
            EventSide = eventSide,
            SafeRect = eventSafeRect,
            DangerRect = eventDangerRect,
        };

        aiFutureSimQueue.Clear();
        if (sim.Phase == 1 && sim.Ev != FloorEventType.None)
        {
            foreach ((int x, int y) in eventTileQueue)
            {
                aiFutureSimQueue.Add((x, y));
            }
        }

        Array.Copy(markedStrikeOrder, aiFutureSimStrikeOrder, markedStrikeCount);

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                aiFutureSimMarked[x, y] = tiles[x, y].EventMarked && tiles[x, y].State != 2;
                aiFutureSimCollapsed[x, y] = tiles[x, y].State == 2;
            }
        }

        AiFutureWriteEventRam(0, 0f, sim);
        AiFutureWriteTileHazardRam(0, sim);

        for (int step = 1; step < AiFutureHorizonSteps; step++)
        {
            AiFutureSimStep(ref sim, AiFutureStepSec);
            AiFutureWriteEventRam(step, step * AiFutureStepSec, sim);
            AiFutureWriteTileHazardRam(step, sim);
        }

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float maxFuture = 0f;
                for (int s = 0; s < AiFutureHorizonSteps; s++)
                {
                    maxFuture = MathF.Max(maxFuture, aiFutureTileHazardRam[s, x, y]);
                }
                aiPredictedHazard[x, y] = Math.Clamp(MathF.Max(aiPredictedHazard[x, y], maxFuture * 0.88f), 0f, 1f);
            }
        }
    }

    static float AiBehaviorTilePressureAt(int x, int y)
    {
        Vector2 c = TileCenter(x, y);
        float pressure = 0f;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            var prof = AiBehaviorProfileFor(GetDef(e.Type).Behavior);
            if (prof.TileThreat < 0.9f) continue;
            float d = Vector2.Distance(c, e.Position);
            pressure += prof.TileThreat * 120f / (d + 48f);
        }
        return pressure;
    }

    static float AiPathTraversalCost(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return 99999f;
        ref Tile t = ref tiles[x, y];
        if (t.State == 2 || t.EventMarked) return 99999f;
        if (AiIsTileTooDamaged(x, y)) return 99999f;
        float baseCost = 1f + aiPredictedHazard[x, y] * 6f + (1f - t.Durability / MaxDurability) * 4f;
        if (aiFutureRamValid) baseCost += AiFutureWorstHazardAtTile(x, y, 2.2f) * 8f;
        baseCost += AiVoidNeighborPenalty(x, y);
        if (y < AiTopBandRows) baseCost += 2.5f + aiWaveConservatism;
        if (AiIsNearGridEdge(x, y)) baseCost += 1.8f + aiWaveConservatism * 0.8f;
        return baseCost;
    }

    static float AiPathHeuristic(int x, int y, int goalX, int goalY)
        => MathF.Abs(x - goalX) + MathF.Abs(y - goalY);

    static bool AiTryBuildPath(int startX, int startY, int goalX, int goalY)
    {
        aiPathNodes.Clear();
        if (startX == goalX && startY == goalY) return false;
        if (AiPathTraversalCost(goalX, goalY) >= 99999f) return false;

        Array.Clear(aiClosedSet, 0, aiClosedSet.Length);
        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
                aiPathG[x, y] = float.MaxValue;

        aiPathG[startX, startY] = 0f;
        aiPathParentX[startX, startY] = startX;
        aiPathParentY[startX, startY] = startY;
        aiPathOpen.Clear();
        aiPathOpen.Add((AiPathHeuristic(startX, startY, goalX, goalY), startX, startY));

        int[] dx = { -1, 1, 0, 0, -1, 1, -1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, -1, 1, 1 };
        float[] dc = { 1f, 1f, 1f, 1f, 1.414f, 1.414f, 1.414f, 1.414f };

        int safety = GridSize * GridSize * 2;
        while (aiPathOpen.Count > 0 && safety-- > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < aiPathOpen.Count; i++)
            {
                if (aiPathOpen[i].F < aiPathOpen[bestIdx].F) bestIdx = i;
            }

            (float _, int cx, int cy) = aiPathOpen[bestIdx];
            aiPathOpen.RemoveAt(bestIdx);
            if (aiClosedSet[cx, cy]) continue;
            aiClosedSet[cx, cy] = true;
            if (cx == goalX && cy == goalY)
            {
                int px = goalX, py = goalY;
                while (!(px == startX && py == startY))
                {
                    aiPathNodes.Add((px, py));
                    int npx = aiPathParentX[px, py];
                    int npy = aiPathParentY[px, py];
                    px = npx; py = npy;
                    if (aiPathNodes.Count > GridSize * GridSize) return false;
                }
                aiPathNodes.Reverse();
                return aiPathNodes.Count > 0;
            }

            float g = aiPathG[cx, cy];
            for (int i = 0; i < 8; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) continue;
                if (aiClosedSet[nx, ny]) continue;
                float step = AiPathTraversalCost(nx, ny);
                if (step >= 99999f) continue;
                float ng = g + dc[i] * step;
                if (ng >= aiPathG[nx, ny]) continue;
                aiPathG[nx, ny] = ng;
                aiPathParentX[nx, ny] = cx;
                aiPathParentY[nx, ny] = cy;
                aiPathOpen.Add((ng + AiPathHeuristic(nx, ny, goalX, goalY), nx, ny));
            }
        }
        return false;
    }

    static bool AiTryFindStrategicGoal(Vector2 from, out Vector2 goal)
    {
        goal = from;
        if (!TryGetTileUnder(from, out int sx, out int sy)) return false;

        float best = float.NegativeInfinity;
        int gx = sx, gy = sy;
        float searchBias = 1f + aiWaveConservatism * 0.45f;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float pv = aiPredictedValue[x, y];
                if (pv < 80f) continue;
                if (AiVoidDistance(x, y) <= 2) continue;
                float score = pv;
                score += tiles[x, y].Durability * 0.2f;
                score -= Vector2.Distance(from, TileCenter(x, y)) * 0.11f * searchBias;
                score -= AiEnemyLanePenalty(from, TileCenter(x, y)) * (aiPhase == AiSurvivalPhase.Combat ? 0.8f : 0.25f);
                if (activeEvent != FloorEventType.None && eventCountdown > 0f && tiles[x, y].EventMarked) score -= 4000f;
                if (score > best)
                {
                    best = score; gx = x; gy = y;
                }
            }
        }

        goal = TileCenter(gx, gy);
        return best > float.NegativeInfinity;
    }

    static Vector2 AiPathfindSteer(Vector2 from, float dt)
    {
        if (!TryGetTileUnder(from, out int sx, out int sy)) return Vector2.Zero;
        aiPathRecalcTimer -= dt;
        if (aiPathRecalcTimer <= 0f || !aiPathGoalValid)
        {
            aiPathRecalcTimer = 0.26f + aiWaveConservatism * 0.08f;
            if (AiTryFindStrategicGoal(from, out Vector2 goal))
            {
                aiPathGoal = goal;
                aiPathGoalValid = true;
                if (TryGetTileUnder(goal, out int gx, out int gy))
                    AiTryBuildPath(sx, sy, gx, gy);
            }
            else aiPathGoalValid = false;
        }

        if (aiPathNodes.Count == 0) return Vector2.Zero;
        var (tx, ty) = aiPathNodes[0];
        Vector2 node = TileCenter(tx, ty);
        if (Vector2.DistanceSquared(from, node) < 18f * 18f)
        {
            aiPathNodes.RemoveAt(0);
            if (aiPathNodes.Count == 0) return Vector2.Zero;
            (tx, ty) = aiPathNodes[0];
            node = TileCenter(tx, ty);
        }
        return SafeNormalize(node - from);
    }

    static Vector2 AiCounter_Chase(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        return steer;
    }

    static Vector2 AiCounter_FastChase(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        return steer;
    }

    static Vector2 AiCounter_CrushTiles(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_Orbit(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        Vector2 tangent = new Vector2(-dir.Y, dir.X);
        steer += tangent * 0.55f * prof.Priority;
        return steer;
    }

    static Vector2 AiCounter_Zigzag(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        return steer;
    }

    static Vector2 AiCounter_Hop(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (margin < TileSize * 0.8f) steer += dir * prof.Priority * 2.2f;
        return steer;
    }

    static Vector2 AiCounter_BlightTrail(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_TileLeech(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_Kite(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        Vector2 tangent = new Vector2(-dir.Y, dir.X);
        steer += tangent * 0.55f * prof.Priority;
        return steer;
    }

    static Vector2 AiCounter_Charge(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (margin < TileSize * 0.8f) steer += dir * prof.Priority * 2.2f;
        return steer;
    }

    static Vector2 AiCounter_PulseBlight(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_Phaser(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        Vector2 tangent = new Vector2(-dir.Y, dir.X);
        steer += tangent * 0.55f * prof.Priority;
        return steer;
    }

    static Vector2 AiCounter_Rotburst(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        if (d < 120f) steer += dir * prof.Priority * 1.6f;
        return steer;
    }

    static Vector2 AiCounter_Splitter(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (d < 120f) steer += dir * prof.Priority * 1.6f;
        return steer;
    }

    static Vector2 AiCounter_Sapper(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_Lurker(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        Vector2 tangent = new Vector2(-dir.Y, dir.X);
        steer += tangent * 0.55f * prof.Priority;
        return steer;
    }

    static Vector2 AiCounter_BossBlight(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiCounter_BossDash(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (margin < TileSize * 0.8f) steer += dir * prof.Priority * 2.2f;
        return steer;
    }

    static Vector2 AiCounter_BossSmash(Vector2 from, in Enemy e, in AiBehaviorProfile prof)
    {
        Vector2 away = from - e.Position;
        float d = away.Length();
        Vector2 dir = d > 0.01f ? away / d : Vector2.UnitY;
        float margin = AiEnemyZoneMargin(from, e);
        Vector2 steer = Vector2.Zero;
        if (margin < 0f) steer += dir * (prof.PanicMul * (3f + (-margin / TileSize) * 5f));
        else if (d < prof.KiteDist) steer += dir * (prof.PanicMul * (1f + (prof.KiteDist - d) / prof.KiteDist));
        if (TryGetTileUnder(from, out int fx, out int fy))
            steer += SafeNormalize(from - TileCenter(fx, fy)) * prof.TileThreat * 0.35f;
        return steer;
    }

    static Vector2 AiGrandmasterBehaviorSteer(Vector2 from)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (e.Dead || e.Spawn < 0.5f) continue;
            var def = GetDef(e.Type);
            var prof = AiBehaviorProfileFor(def.Behavior);
            Vector2 counter = def.Behavior switch
            {
                EnemyBehavior.Chase => AiCounter_Chase(from, in e, in prof),
                EnemyBehavior.FastChase => AiCounter_FastChase(from, in e, in prof),
                EnemyBehavior.CrushTiles => AiCounter_CrushTiles(from, in e, in prof),
                EnemyBehavior.Orbit => AiCounter_Orbit(from, in e, in prof),
                EnemyBehavior.Zigzag => AiCounter_Zigzag(from, in e, in prof),
                EnemyBehavior.Hop => AiCounter_Hop(from, in e, in prof),
                EnemyBehavior.BlightTrail => AiCounter_BlightTrail(from, in e, in prof),
                EnemyBehavior.TileLeech => AiCounter_TileLeech(from, in e, in prof),
                EnemyBehavior.Kite => AiCounter_Kite(from, in e, in prof),
                EnemyBehavior.Charge => AiCounter_Charge(from, in e, in prof),
                EnemyBehavior.PulseBlight => AiCounter_PulseBlight(from, in e, in prof),
                EnemyBehavior.Phaser => AiCounter_Phaser(from, in e, in prof),
                EnemyBehavior.Rotburst => AiCounter_Rotburst(from, in e, in prof),
                EnemyBehavior.Splitter => AiCounter_Splitter(from, in e, in prof),
                EnemyBehavior.Sapper => AiCounter_Sapper(from, in e, in prof),
                EnemyBehavior.Lurker => AiCounter_Lurker(from, in e, in prof),
                EnemyBehavior.BossBlight => AiCounter_BossBlight(from, in e, in prof),
                EnemyBehavior.BossDash => AiCounter_BossDash(from, in e, in prof),
                EnemyBehavior.BossSmash => AiCounter_BossSmash(from, in e, in prof),
                _ => AiCounter_Chase(from, in e, in prof),
            };
            if (counter == Vector2.Zero) continue;
            sum += counter; count++;
        }
        if (count == 0 || sum.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(sum) * MathF.Min(sum.Length() / count, 1.85f);
    }

    static Vector2 AiGrandmasterAmplifyEscape(Vector2 dir, Vector2 from, float mul)
    {
        if (dir == Vector2.Zero) return Vector2.Zero;
        Vector2 amplified = dir * mul;
        if (AiVoidDashUrgency(from) > 0.4f) amplified += ComputeUrgentVoidFlee(from) * 0.65f;
        return SafeNormalize(amplified);
    }

    static Vector2 AiGrandmasterRefineEventDir(Vector2 dir, Vector2 from, FloorEventType ev)
    {
        if (dir == Vector2.Zero) return Vector2.Zero;
        var prof = AiEventProfileFor(ev);
        Vector2 refined = RefineAiDirection(dir, from, true);
        if (prof.UsesCluster && !aiLargestClusterValid) RebuildLargestHealthyCluster();
        if (prof.MarkedSensitive && TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            refined = AiGrandmasterAmplifyEscape(refined, from, 2.4f);
        if (eventCountdown < prof.PanicThreshold)
            refined = AiGrandmasterAmplifyEscape(refined, from, 1.35f + (prof.PanicThreshold - eventCountdown) * 0.25f);
        return refined;
    }

    static Vector2 AiGrandmasterClusterDir(Vector2 from)
    {
        if (!aiLargestClusterValid) RebuildLargestHealthyCluster();
        if (!aiLargestClusterValid) return DirectionToNearestSafeTile(from);
        if (AiPlayerInLargestClusterInterior()) return Vector2.Zero;
        return SafeNormalize(aiLargestClusterTarget - from);
    }

    static Vector2 AiGrandmasterSafeRectDir(Vector2 from)
    {
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        Vector2 c = new Vector2(eventSafeRect.X + eventSafeRect.Width * 0.5f, eventSafeRect.Y + eventSafeRect.Height * 0.5f);
        return SafeNormalize(c - from);
    }

    static bool AiGrandmasterShouldHoldForEvent()
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f) return false;
        var prof = AiEventProfileFor(activeEvent);
        if (AiShouldHoldEventSafeZone()) return true;
        if (prof.UsesCluster && AiPlayerInLargestClusterInterior() && eventCountdown > prof.PanicThreshold) return true;
        if (prof.UsesSafeRect && PlayerInSafeRect(eventSafeRect)) return true;
        return false;
    }

    static Vector2 AiEventSteer_CrimsonCrumble(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrimsonCrumble);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrimsonCrumble);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrimsonCrumble);
    }

    static Vector2 AiEventSteer_SafeZoneRush(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.SafeZoneRush);
        if (AiPlayerDeepInSafeRushBand()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneRushInterior(), from, FloorEventType.SafeZoneRush);
    }

    static Vector2 AiEventSteer_Checkerfall(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.Checkerfall);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.Checkerfall);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.Checkerfall);
    }

    static Vector2 AiEventSteer_RingCollapse(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.RingCollapse);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.RingCollapse);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.RingCollapse);
    }

    static Vector2 AiEventSteer_StoneIslands(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.StoneIslands);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.StoneIslands);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.StoneIslands);
    }

    static Vector2 AiEventSteer_ScatterPits(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.ScatterPits);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.ScatterPits);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.ScatterPits);
    }

    static Vector2 AiEventSteer_MossRot(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.MossRot);
        if (floorRotTimer <= 0f || !TryGetTileUnder(from, out int rx, out int ry)) return Vector2.Zero;
        bool left = rx < GridSize / 2;
        bool onRot = floorRotSide < 0.5f ? left : !left;
        if (!onRot) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToSafeRotHalf(), from, FloorEventType.MossRot);
    }

    static Vector2 AiEventSteer_MarkedStrike(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.MarkedStrike);
        if (TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.MarkedStrike);
        if (eventCountdown < prof.PanicThreshold + 0.8f)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.MarkedStrike);
        return Vector2.Zero;
    }

    static Vector2 AiEventSteer_CenterSnare(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CenterSnare);
        if (PlayerInCenterSnareSafe()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToNearestEdge(), from, FloorEventType.CenterSnare);
    }

    static Vector2 AiEventSteer_BlightStorm(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.BlightStorm);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.BlightStorm);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.BlightStorm);
    }

    static Vector2 AiEventSteer_TideSurge(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideSurge);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideSurge);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideSurge);
    }

    static Vector2 AiEventSteer_TideRift(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideRift);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideRift);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideRift);
    }

    static Vector2 AiEventSteer_TideRecede(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideRecede);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideRecede);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideRecede);
    }

    static Vector2 AiEventSteer_TideColumn(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideColumn);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideColumn);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideColumn);
    }

    static Vector2 AiEventSteer_TideEcho(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideEcho);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideEcho);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideEcho);
    }

    static Vector2 AiEventSteer_TideUndertow(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideUndertow);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideUndertow);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideUndertow);
    }

    static Vector2 AiEventSteer_TideCrest(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideCrest);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideCrest);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideCrest);
    }

    static Vector2 AiEventSteer_TideWall(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideWall);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideWall);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideWall);
    }

    static Vector2 AiEventSteer_TideAnchor(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideAnchor);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideAnchor);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideAnchor);
    }

    static Vector2 AiEventSteer_TideFoam(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideFoam);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.TideFoam);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.TideFoam);
    }

    static Vector2 AiEventSteer_EmberRain(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberRain);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberRain);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberRain);
    }

    static Vector2 AiEventSteer_EmberGate(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberGate);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterSafeRectDir(from), from, FloorEventType.EmberGate);
    }

    static Vector2 AiEventSteer_EmberPulse(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberPulse);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberPulse);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberPulse);
    }

    static Vector2 AiEventSteer_EmberCross(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberCross);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberCross);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberCross);
    }

    static Vector2 AiEventSteer_EmberBridge(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberBridge);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberBridge);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberBridge);
    }

    static Vector2 AiEventSteer_EmberFury(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberFury);
        if (emberFuryStandTimer <= EmberFuryStandTime * 0.5f) return Vector2.Zero;
        Vector2 hold = lastMoveDirection.LengthSquared() > 0.01f ? SafeNormalize(lastMoveDirection) : AiGrandmasterClusterDir(from);
        return AiGrandmasterRefineEventDir(hold, from, FloorEventType.EmberFury);
    }

    static Vector2 AiEventSteer_EmberSnake(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberSnake);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberSnake);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberSnake);
    }

    static Vector2 AiEventSteer_EmberHive(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberHive);
        if (PlayerOnSafeIsland()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.EmberHive);
    }

    static Vector2 AiEventSteer_EmberTide(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberTide);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterSafeRectDir(from), from, FloorEventType.EmberTide);
    }

    static Vector2 AiEventSteer_EmberCage(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberCage);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(SafeNormalize(new Vector2(WindowWidth / 2f, WindowHeight / 2f) - from), from, FloorEventType.EmberCage);
    }

    static Vector2 AiEventSteer_EmberQuake(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberQuake);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberQuake);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberQuake);
    }

    static Vector2 AiEventSteer_EmberBloom(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberBloom);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.EmberBloom);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.EmberBloom);
    }

    static Vector2 AiEventSteer_EmberAltar(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.EmberAltar);
        if (PlayerInEmberAltar()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(SafeNormalize(eventEpicenter - from), from, FloorEventType.EmberAltar);
    }

    static Vector2 AiEventSteer_CryptSeal(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptSeal);
        if (PlayerInCenterSnareSafe()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToNearestEdge(), from, FloorEventType.CryptSeal);
    }

    static Vector2 AiEventSteer_CryptWail(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptWail);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptWail);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptWail);
    }

    static Vector2 AiEventSteer_CryptTorch(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptTorch);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptTorch);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptTorch);
    }

    static Vector2 AiEventSteer_CryptChains(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptChains);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptChains);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptChains);
    }

    static Vector2 AiEventSteer_CryptMist(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptMist);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptMist);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptMist);
    }

    static Vector2 AiEventSteer_CryptTomb(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptTomb);
        if (TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CryptTomb);
        if (eventCountdown < prof.PanicThreshold + 0.8f)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CryptTomb);
        return Vector2.Zero;
    }

    static Vector2 AiEventSteer_CryptShroud(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptShroud);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptShroud);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptShroud);
    }

    static Vector2 AiEventSteer_CryptGlimpse(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptGlimpse);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptGlimpse);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptGlimpse);
    }

    static Vector2 AiEventSteer_CryptRattle(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptRattle);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptRattle);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptRattle);
    }

    static Vector2 AiEventSteer_CryptEcho(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptEcho);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptEcho);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptEcho);
    }

    static Vector2 AiEventSteer_CryptGrave(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptGrave);
        if (TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CryptGrave);
        if (eventCountdown < prof.PanicThreshold + 0.8f)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CryptGrave);
        return Vector2.Zero;
    }

    static Vector2 AiEventSteer_CryptLantern(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptLantern);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CryptLantern);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CryptLantern);
    }

    static Vector2 AiEventSteer_CryptVeil(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CryptVeil);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterSafeRectDir(from), from, FloorEventType.CryptVeil);
    }

    static Vector2 AiEventSteer_TideWhirlpool(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideWhirlpool);
        if (!PlayerInTideWhirlpoolDanger()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionAwayFromArenaCenter(), from, FloorEventType.TideWhirlpool);
    }

    static Vector2 AiEventSteer_TideBeacon(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideBeacon);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterSafeRectDir(from), from, FloorEventType.TideBeacon);
    }

    static Vector2 AiEventSteer_TideStrike(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.TideStrike);
        if (TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.TideStrike);
        if (eventCountdown < prof.PanicThreshold + 0.8f)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.TideStrike);
        return Vector2.Zero;
    }

    static Vector2 AiEventSteer_CrownTrial(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownTrial);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownTrial);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownTrial);
    }

    static Vector2 AiEventSteer_CrownFall(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownFall);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownFall);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownFall);
    }

    static Vector2 AiEventSteer_CrownShard(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownShard);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownShard);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownShard);
    }

    static Vector2 AiEventSteer_CrownThrone(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownThrone);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToArenaCenter(), from, FloorEventType.CrownThrone);
    }

    static Vector2 AiEventSteer_CrownEdict(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownEdict);
        if (PlayerInSafeRect(eventSafeRect)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterSafeRectDir(from), from, FloorEventType.CrownEdict);
    }

    static Vector2 AiEventSteer_CrownRot(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownRot);
        if (floorRotTimer <= 0f || !TryGetTileUnder(from, out int rx, out int ry)) return Vector2.Zero;
        bool left = rx < GridSize / 2;
        bool onRot = floorRotSide < 0.5f ? left : !left;
        if (!onRot) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToSafeRotHalf(), from, FloorEventType.CrownRot);
    }

    static Vector2 AiEventSteer_CrownBolt(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownBolt);
        if (TryGetTileUnder(from, out int mx, out int my) && tiles[mx, my].EventMarked)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CrownBolt);
        if (eventCountdown < prof.PanicThreshold + 0.8f)
            return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CrownBolt);
        return Vector2.Zero;
    }

    static Vector2 AiEventSteer_CrownRing(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownRing);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownRing);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownRing);
    }

    static Vector2 AiEventSteer_CrownIsles(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownIsles);
        if (PlayerOnSafeIsland()) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(AiGrandmasterClusterDir(from), from, FloorEventType.CrownIsles);
    }

    static Vector2 AiEventSteer_CrownStorm(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownStorm);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownStorm);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownStorm);
    }

    static Vector2 AiEventSteer_CrownCoronation(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownCoronation);
        if (PlayerInCrownCenter(2)) return Vector2.Zero;
        return AiGrandmasterRefineEventDir(DirectionToArenaCenter(), from, FloorEventType.CrownCoronation);
    }

    static Vector2 AiEventSteer_CrownUsurpation(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownUsurpation);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownUsurpation);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownUsurpation);
    }

    static Vector2 AiEventSteer_CrownReckoning(Vector2 from)
    {
        var prof = AiEventProfileFor(FloorEventType.CrownReckoning);
        if (prof.UsesCluster)
        {
            Vector2 cluster = AiGrandmasterClusterDir(from);
            if (cluster == Vector2.Zero && eventCountdown > prof.PrepThreshold) return Vector2.Zero;
            return AiGrandmasterRefineEventDir(cluster == Vector2.Zero ? DirectionToSafeZoneInterior(from) : cluster, from, FloorEventType.CrownReckoning);
        }
        return AiGrandmasterRefineEventDir(DirectionToSafeZoneInterior(from), from, FloorEventType.CrownReckoning);
    }

    static Vector2 AiGrandmasterEventSteer(Vector2 from)
    {
        if (activeEvent == FloorEventType.None || eventCountdown <= 0f) return Vector2.Zero;
        if (AiGrandmasterShouldHoldForEvent()) return Vector2.Zero;
        return activeEvent switch
        {
            FloorEventType.CrimsonCrumble => AiEventSteer_CrimsonCrumble(from),
            FloorEventType.SafeZoneRush => AiEventSteer_SafeZoneRush(from),
            FloorEventType.Checkerfall => AiEventSteer_Checkerfall(from),
            FloorEventType.RingCollapse => AiEventSteer_RingCollapse(from),
            FloorEventType.StoneIslands => AiEventSteer_StoneIslands(from),
            FloorEventType.ScatterPits => AiEventSteer_ScatterPits(from),
            FloorEventType.MossRot => AiEventSteer_MossRot(from),
            FloorEventType.MarkedStrike => AiEventSteer_MarkedStrike(from),
            FloorEventType.CenterSnare => AiEventSteer_CenterSnare(from),
            FloorEventType.BlightStorm => AiEventSteer_BlightStorm(from),
            FloorEventType.TideSurge => AiEventSteer_TideSurge(from),
            FloorEventType.TideRift => AiEventSteer_TideRift(from),
            FloorEventType.TideRecede => AiEventSteer_TideRecede(from),
            FloorEventType.TideColumn => AiEventSteer_TideColumn(from),
            FloorEventType.TideEcho => AiEventSteer_TideEcho(from),
            FloorEventType.TideUndertow => AiEventSteer_TideUndertow(from),
            FloorEventType.TideCrest => AiEventSteer_TideCrest(from),
            FloorEventType.TideWall => AiEventSteer_TideWall(from),
            FloorEventType.TideAnchor => AiEventSteer_TideAnchor(from),
            FloorEventType.TideFoam => AiEventSteer_TideFoam(from),
            FloorEventType.EmberRain => AiEventSteer_EmberRain(from),
            FloorEventType.EmberGate => AiEventSteer_EmberGate(from),
            FloorEventType.EmberPulse => AiEventSteer_EmberPulse(from),
            FloorEventType.EmberCross => AiEventSteer_EmberCross(from),
            FloorEventType.EmberBridge => AiEventSteer_EmberBridge(from),
            FloorEventType.EmberFury => AiEventSteer_EmberFury(from),
            FloorEventType.EmberSnake => AiEventSteer_EmberSnake(from),
            FloorEventType.EmberHive => AiEventSteer_EmberHive(from),
            FloorEventType.EmberTide => AiEventSteer_EmberTide(from),
            FloorEventType.EmberCage => AiEventSteer_EmberCage(from),
            FloorEventType.EmberQuake => AiEventSteer_EmberQuake(from),
            FloorEventType.EmberBloom => AiEventSteer_EmberBloom(from),
            FloorEventType.EmberAltar => AiEventSteer_EmberAltar(from),
            FloorEventType.CryptSeal => AiEventSteer_CryptSeal(from),
            FloorEventType.CryptWail => AiEventSteer_CryptWail(from),
            FloorEventType.CryptTorch => AiEventSteer_CryptTorch(from),
            FloorEventType.CryptChains => AiEventSteer_CryptChains(from),
            FloorEventType.CryptMist => AiEventSteer_CryptMist(from),
            FloorEventType.CryptTomb => AiEventSteer_CryptTomb(from),
            FloorEventType.CryptShroud => AiEventSteer_CryptShroud(from),
            FloorEventType.CryptGlimpse => AiEventSteer_CryptGlimpse(from),
            FloorEventType.CryptRattle => AiEventSteer_CryptRattle(from),
            FloorEventType.CryptEcho => AiEventSteer_CryptEcho(from),
            FloorEventType.CryptGrave => AiEventSteer_CryptGrave(from),
            FloorEventType.CryptLantern => AiEventSteer_CryptLantern(from),
            FloorEventType.CryptVeil => AiEventSteer_CryptVeil(from),
            FloorEventType.TideWhirlpool => AiEventSteer_TideWhirlpool(from),
            FloorEventType.TideBeacon => AiEventSteer_TideBeacon(from),
            FloorEventType.TideStrike => AiEventSteer_TideStrike(from),
            FloorEventType.CrownTrial => AiEventSteer_CrownTrial(from),
            FloorEventType.CrownFall => AiEventSteer_CrownFall(from),
            FloorEventType.CrownShard => AiEventSteer_CrownShard(from),
            FloorEventType.CrownThrone => AiEventSteer_CrownThrone(from),
            FloorEventType.CrownEdict => AiEventSteer_CrownEdict(from),
            FloorEventType.CrownRot => AiEventSteer_CrownRot(from),
            FloorEventType.CrownBolt => AiEventSteer_CrownBolt(from),
            FloorEventType.CrownRing => AiEventSteer_CrownRing(from),
            FloorEventType.CrownIsles => AiEventSteer_CrownIsles(from),
            FloorEventType.CrownStorm => AiEventSteer_CrownStorm(from),
            FloorEventType.CrownCoronation => AiEventSteer_CrownCoronation(from),
            FloorEventType.CrownUsurpation => AiEventSteer_CrownUsurpation(from),
            FloorEventType.CrownReckoning => AiEventSteer_CrownReckoning(from),
            FloorEventType.None => Vector2.Zero,
            _ => DirectionToSafeZoneInterior(from),
        };
    }

    static void ResolveAiGrandmasterAbilities(float dt)
    {
        aiVerdictRequest = false;
        aiBannerRequest = false;
        if (state != GameState.Playing) return;

        bool eventLive = activeEvent != FloorEventType.None && eventCountdown > 0f;
        var evProf = eventLive ? AiEventProfileFor(activeEvent) : default;
        bool criticalTile = AiVoidDashUrgency(playerPos) > 0.72f || IsFeetTileWeak();
        bool swarmed = AiEnemiesThreatening(playerPos, 160f) >= 3;
        bool bossNear = NearestBossDistance(playerPos) < 240f;

        if (HasEquippedAbility(AbilityType.Verdict) && IsVerdictUnlocked() && verdictCooldown <= 0f && verdictHaltTimer <= 0f)
        {
            bool wantVerdict = false;
            if (eventLive && eventCountdown < evProf.PanicThreshold && !AiGrandmasterShouldHoldForEvent()) wantVerdict = true;
            if (eventLive && TryGetTileUnder(playerPos, out int vx, out int vy) && tiles[vx, vy].EventMarked) wantVerdict = true;
            if (criticalTile && eventLive && eventCountdown < 2.8f) wantVerdict = true;
            if (AiIsMassCollapseEvent() && !AiPlayerInLargestClusterInterior() && eventCountdown < 2.5f) wantVerdict = true;
            if (bossNear && eventLive && waveNumber >= 25 && eventCountdown < 1.8f) wantVerdict = true;
            if (AiIsTrapped(playerPos) && eventLive) wantVerdict = true;
            if (wantVerdict && Raylib.GetTime() - aiLastVerdictAttempt > 0.45f)
            {
                aiVerdictRequest = true;
                aiLastVerdictAttempt = (float)Raylib.GetTime();
            }
        }

        if (HasEquippedAbility(AbilityType.BannerOfStillness) && bannerCooldown <= 0f && !bannerActive && bannerPlantTimer <= 0f)
        {
            bool wantBanner = false;
            if (swarmed && !criticalTile && TryGetTileUnder(playerPos, out int bx, out int by))
            {
                float dur = tiles[bx, by].Durability / MaxDurability;
                if (dur > 0.62f && aiTileHazard[bx, by] < 0.35f && AiCountViableWalkExits(playerPos, 64f) >= 2) wantBanner = true;
            }
            if (bossNear && !criticalTile && AiCountViableWalkExits(playerPos, 64f) >= 2) wantBanner = true;
            if (wantBanner && aiPhase is AiSurvivalPhase.Combat or AiSurvivalPhase.BossFight)
            {
                aiBannerRequest = true;
                aiBannerTarget = playerPos;
            }
        }

        aiOathPrimed = HasEquippedAbility(AbilityType.OathOfTheBailey) && !oathUsedThisRun && criticalTile;
    }

    static Vector2 AiGrandmasterCombatRetreat(Vector2 from, float dt)
    {
        if (reloadTimer <= 0f) { aiCombatRetreatTimer = 0f; return Vector2.Zero; }
        aiCombatRetreatTimer += dt;
        if (AiEnemiesThreatening(from, 150f) == 0) return Vector2.Zero;
        Vector2 flee = ComputeUrgentEnemyFlee(from);
        if (flee == Vector2.Zero) flee = AiGrandmasterBehaviorSteer(from);
        return flee * MathF.Min(1f, 0.55f + aiCombatRetreatTimer);
    }

    static int AiGrandmasterSelectPriorityTarget()
    {
        int best = -1;
        float bestScore = float.NegativeInfinity;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (e.Dead || e.Spawn < 0.5f) continue;
            var def = GetDef(e.Type);
            var prof = AiBehaviorProfileFor(def.Behavior);
            float d = Vector2.Distance(playerPos, e.Position);
            float score = prof.Priority * 200f - d;
            if (def.Boss) score += 420f;
            if (def.InstaCollapse) score += 180f;
            if (prof.TileThreat > 1.5f) score += 160f;
            score += AiEvaluateEnemyThreat(playerPos, in e) * 0.12f;
            if (score > bestScore) { bestScore = score; best = i; }
        }
        aiFocusEnemyIndex = best;
        return best;
    }

    static float AiGrandmasterLookaheadScore(Vector2 pos, float seconds)
    {
        float score = 0f;
        if (!TryGetTileUnder(pos, out int tx, out int ty)) return -99999f;
        score += aiPredictedValue[tx, ty];
        score -= aiPredictedHazard[tx, ty] * 320f;
        float futureHazard = AiFutureWorstHazardAtTile(tx, ty, seconds + 0.12f);
        score -= futureHazard * 420f;
        score -= AiEnemyZonePenalty(pos);
        score -= NearestBossDistance(pos) * 0.22f;
        score += AiCountViableWalkExits(pos, 64f) * 45f;
        if (activeEvent != FloorEventType.None && eventCountdown > 0f && tiles[tx, ty].EventMarked) score -= 5000f;
        if (futureHazard > 0.82f) score -= 3200f;
        score -= seconds * 12f;
        return score;
    }

    static Vector2 AiGrandmasterLookaheadSteer(Vector2 from, Vector2 baseDir)
    {
        if (baseDir == Vector2.Zero) return Vector2.Zero;
        float urgency = AiSteeringUrgency();
        if (urgency < 0.35f) return baseDir;

        float baseAngle = MathF.Atan2(baseDir.Y, baseDir.X);
        float bestScore = float.NegativeInfinity;
        Vector2 best = baseDir;
        float speed = EffMoveSpeed();
        int sweep = urgency > 0.78f ? 5 : 3;
        float angleStep = urgency > 0.78f ? MathF.PI / 22f : MathF.PI / 34f;

        for (int i = -sweep; i <= sweep; i++)
        {
            float angle = baseAngle + i * angleStep;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 probe = from + dir * speed * 0.35f;
            float s0 = AiGrandmasterLookaheadScore(probe, 0.35f);
            Vector2 probe2 = from + dir * speed * 0.75f;
            float s1 = AiGrandmasterLookaheadScore(probe2, 0.75f);
            Vector2 probe3 = from + dir * speed * 1.35f;
            float s2 = urgency > 0.62f ? AiGrandmasterLookaheadScore(probe3, 1.35f) : s1;
            float total = s0 * 0.34f + s1 * 0.42f + s2 * 0.24f;
            total += Vector2.Dot(dir, baseDir) * (90f - urgency * 40f);
            if (total > bestScore) { bestScore = total; best = dir; }
        }
        return best;
    }

    static Vector2 AiGrandmasterBossSteer(Vector2 from)
    {
        float bossDist = NearestBossDistance(from);
        if (bossDist > 260f) return Vector2.Zero;
        Vector2 steer = Vector2.Zero;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            if (!GetDef(e.Type).Boss) continue;
            Vector2 away = from - e.Position;
            float d = away.Length();
            if (d < 0.01f) continue;
            Vector2 dir = away / d;
            float orbitDist = 150f + e.Radius;
            if (d < orbitDist * 0.75f) steer += dir * 2.4f;
            else if (d < orbitDist * 1.2f)
            {
                Vector2 tangent = new Vector2(-dir.Y, dir.X);
                steer += tangent * 1.1f;
            }
            else steer += dir * (95f / (d + 20f));
        }
        if (steer.LengthSquared() < 0.001f) return Vector2.Zero;
        return SafeNormalize(steer);
    }

    static Vector2 AiGrandmasterBlendSteering(Vector2 from, Vector2 baseMove, float dt)
    {
        Vector2 move = baseMove;
        float urgency = AiSteeringUrgency();

        if (bannerActive && IsInBannerZone(from) && aiPhase is not AiSurvivalPhase.EventActive and not AiSurvivalPhase.TileCritical)
        {
            move *= 0.28f;
        }

        Vector2 eventSteer = AiGrandmasterEventSteer(from);
        if (eventSteer != Vector2.Zero)
        {
            move = Vector2.Lerp(move, eventSteer, 0.48f + urgency * 0.46f);
        }
        else
        {
            Vector2 path = AiPathfindSteer(from, dt);
            if (path != Vector2.Zero) move = Vector2.Lerp(move, path, 0.2f + aiWaveConservatism * 0.1f);
        }

        Vector2 behavior = AiGrandmasterBehaviorSteer(from);
        if (behavior != Vector2.Zero)
        {
            float behaviorW = aiPhase switch
            {
                AiSurvivalPhase.BossFight => 0.38f + urgency * 0.22f,
                AiSurvivalPhase.LastStand => 0.34f + urgency * 0.24f,
                AiSurvivalPhase.Combat => 0.26f + urgency * 0.18f,
                _ => 0.18f + urgency * 0.12f,
            };
            move = Vector2.Lerp(move, behavior, behaviorW);
        }

        Vector2 boss = AiGrandmasterBossSteer(from);
        if (boss != Vector2.Zero) move = Vector2.Lerp(move, boss, 0.22f + urgency * 0.38f);

        Vector2 retreat = AiGrandmasterCombatRetreat(from, dt);
        if (retreat != Vector2.Zero) move = Vector2.Lerp(move, retreat, 0.45f + urgency * 0.28f);

        float scenarioScore = aiScenarioScoreCache;
        Vector2 scenario = AiScenarioMatrixSteer(from);
        if (scenario != Vector2.Zero)
        {
            move = Vector2.Lerp(move, scenario, MathF.Min(0.32f, scenarioScore * 0.08f + urgency * 0.06f));
        }

        if (move != Vector2.Zero)
            move = AiGrandmasterLookaheadSteer(from, SafeNormalize(move));
        return move;
    }

    static void UpdateAiGrandmasterDashLogic(float dt, ref float dashUrgency)
    {
        float behaviorDash = 0f;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            var prof = AiBehaviorProfileFor(GetDef(e.Type).Behavior);
            if (!prof.PreemptDash) continue;
            float margin = AiEnemyZoneMargin(playerPos, e);
            if (margin < TileSize * 0.25f) behaviorDash = MathF.Max(behaviorDash, 0.88f);
        }

        dashUrgency = MathF.Max(dashUrgency, behaviorDash);
        dashUrgency = MathF.Max(dashUrgency, AiCollapseEventDashUrgency(playerPos) * 1.05f);
        if (aiPhase == AiSurvivalPhase.LastStand) dashUrgency = MathF.Max(dashUrgency, 0.95f);
        if (aiPredictFieldsValid && TryGetTileUnder(playerPos, out int px, out int py))
        {
            if (aiPredictedHazard[px, py] > 0.82f) dashUrgency = MathF.Max(dashUrgency, 0.9f);
            if (aiFutureRamValid && AiFutureWorstHazardAtTile(px, py, 1.6f) > 0.86f)
            {
                dashUrgency = MathF.Max(dashUrgency, 0.94f);
            }
        }
    }

    static void RunAiGrandmasterBrain(float dt)
    {
        aiScenarioFlagsDirty = true;
        UpdateAiWaveConservatism();
        RebuildAiPredictiveFields(dt);
        aiFutureRamTimer -= dt;
        if (aiFutureRamTimer <= 0f)
        {
            aiFutureRamTimer = 0.12f;
            RebuildAiFutureRam();
        }
        EvaluateAiSurvivalPhase();
        ResolveAiGrandmasterAbilities(dt);
        AiGrandmasterSelectPriorityTarget();
        aiScenarioScoreCache = AiEvaluateScenarioMatrix(playerPos);
    }

    static float AiEvaluateEnemyThreat(Vector2 from, in Enemy e)
    {
        var def = GetDef(e.Type);
        var prof = AiBehaviorProfileFor(def.Behavior);
        float d = Vector2.Distance(from, e.Position);
        float margin = AiEnemyZoneMargin(from, e);
        float threat = prof.Priority * 100f + def.TileDecay * 2.4f;
        if (def.Boss) threat += 260f;
        if (def.InstaCollapse) threat += 140f;
        if (margin < 0f) threat += (-margin + 8f) * prof.ZoneMul * 18f;
        threat += MathF.Max(0f, prof.KiteDist - d) * 0.35f;
        if (TryGetTileUnder(from, out int fx, out int fy))
        {
            threat += aiTileHazard[fx, fy] * 80f * prof.TileThreat;
            threat += (1f - tiles[fx, fy].Durability / MaxDurability) * 60f * prof.TileThreat;
        }
        if (TryGetTileUnder(e.Position, out int ex, out int ey))
        {
            threat += aiTileHazard[ex, ey] * 40f;
            if (tiles[ex, ey].EventMarked) threat += 90f;
        }
        threat += AiEnemiesThreatening(from, 120f + def.Radius) * 12f;
        if (waveNumber >= def.MinWave + 10) threat *= 1.08f + aiWaveConservatism * 0.05f;
        return threat;
    }

    static float AiGrandmasterAggregateEnemyThreat(Vector2 from)
    {
        float total = 0f;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            total += AiEvaluateEnemyThreat(from, in e);
        }
        return total;
    }

    static void RefreshAiScenarioThreatFlags()
    {
        if (!aiScenarioFlagsDirty) return;
        aiScenarioFlagsDirty = false;
        aiScenarioHasCrusher = false;
        aiScenarioHasLeech = false;
        aiScenarioHasFast = false;
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            var def = GetDef(e.Type);
            if (def.Behavior is EnemyBehavior.CrushTiles or EnemyBehavior.BossSmash) aiScenarioHasCrusher = true;
            if (def.Behavior == EnemyBehavior.TileLeech) aiScenarioHasLeech = true;
            if (def.Behavior == EnemyBehavior.FastChase && Vector2.Distance(playerPos, e.Position) < 120f) aiScenarioHasFast = true;
        }
    }

    static float AiScenarioPhaseMultiplier()
    {
        return aiPhase switch
        {
            AiSurvivalPhase.LastStand => 1.48f,
            AiSurvivalPhase.TileCritical => 1.28f,
            AiSurvivalPhase.BossFight => 1.18f,
            AiSurvivalPhase.EventActive => 1.22f,
            AiSurvivalPhase.EventPrep => 1.08f,
            AiSurvivalPhase.Combat => 1.05f,
            AiSurvivalPhase.Patrol => 0.92f,
            AiSurvivalPhase.Calm => 0.82f,
            _ => 1f,
        };
    }

    static float AiEvaluateScenarioMatrix(Vector2 from)
    {
        RefreshAiScenarioThreatFlags();
        int bucket = Math.Clamp(waveNumber / 10, 0, 15);
        float waveScale = 1f + bucket * 0.04f + aiWaveConservatism * 0.14f;
        float phaseMul = AiScenarioPhaseMultiplier();
        float sum = 0f;

        float voidUrgency = AiVoidDashUrgency(from);
        if (voidUrgency >= 0.35f || IsFeetTileWeak())
        {
            float w = 0.85f + voidUrgency;
            if (AiCountViableWalkExits(from, 60f) <= 1) w += 0.9f;
            sum += w * phaseMul * waveScale;
        }

        int swarm = AiEnemiesThreatening(from, 170f);
        if (swarm >= 3)
        {
            float w = 0.55f + swarm * 0.12f;
            if (AiIsInsideEnemyDangerZone(from)) w += 0.75f;
            sum += w * phaseMul * waveScale;
        }

        if (activeEvent != FloorEventType.None && eventCountdown > 0f
            && TryGetTileUnder(from, out int tx, out int ty) && tiles[tx, ty].EventMarked)
        {
            sum += (1.1f + (3f - MathF.Min(eventCountdown, 3f)) * 0.2f) * phaseMul * waveScale;
        }

        if (reloadTimer > 0f)
        {
            float w = 0.45f + reloadTimer * 0.25f;
            if (AiEnemiesThreatening(from, 140f) >= 1) w += 0.65f;
            sum += w * phaseMul * waveScale;
        }

        if (TryGetTileUnder(from, out int ex, out int ey) && AiIsNearGridEdge(ex, ey))
        {
            sum += (0.7f + (AiEdgeMarginTiles - AiEdgeMargin(ex, ey)) * 0.25f) * phaseMul * waveScale;
        }

        float bossDist = NearestBossDistance(from);
        if (bossDist <= 220f)
        {
            sum += (1.2f + (220f - bossDist) * 0.004f) * phaseMul * waveScale;
        }

        if (aiScenarioHasCrusher && TryGetTileUnder(from, out int cx, out int cy))
        {
            sum += (0.95f + AiBehaviorTilePressureAt(cx, cy) * 0.02f) * phaseMul * waveScale;
        }

        if (aiScenarioHasLeech)
        {
            sum += (0.8f + aiWaveConservatism * 0.3f) * phaseMul * waveScale;
        }

        if (aiScenarioHasFast)
        {
            sum += 0.72f * phaseMul * waveScale;
        }

        if (AiIsTrapped(from))
        {
            sum += (1.35f + AiEnemiesThreatening(from, 160f) * 0.1f) * phaseMul * waveScale;
        }

        if (bannerActive && IsInBannerZone(from))
        {
            sum *= 0.55f;
        }

        aiScenarioScoreCache = sum;
        return sum;
    }

    static Vector2 AiScenarioMatrixSteer(Vector2 from)
    {
        float score = aiScenarioScoreCache > 0f ? aiScenarioScoreCache : AiEvaluateScenarioMatrix(from);
        if (score < 0.45f) return Vector2.Zero;
        Vector2 steer = Vector2.Zero;
        if (AiVoidDashUrgency(from) > 0.4f) steer += ComputeUrgentVoidFlee(from);
        if (AiEnemiesThreatening(from, 160f) >= 2) steer += ComputeUrgentEnemyFlee(from);
        if (activeEvent != FloorEventType.None && eventCountdown > 0f) steer += AiGrandmasterEventSteer(from);
        if (steer == Vector2.Zero) steer += DirectionToNearestSafeTile(from);
        return SafeNormalize(steer) * MathF.Min(score * 0.35f, 1.65f);
    }

    static float AiWaveSurvivalTuning(int wave)
    {
        float baseTuning = Math.Clamp((wave - 8) * 0.012f, 0f, 1.35f);
        if (AiVoidDashUrgency(playerPos) > 0.5f) baseTuning += 0.12f;
        return Math.Clamp(baseTuning, 0f, 1.45f);
    }
}
