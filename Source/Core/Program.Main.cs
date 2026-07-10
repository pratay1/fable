partial class Program
{
    static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(WindowWidth, WindowHeight, "Fable");
        Raylib.SetTargetFPS(TargetFps);
        Raylib.SetExitKey(KeyboardKey.Null);

        tiles = new Tile[GridSize, GridSize];
        aiTileValue = new float[GridSize, GridSize];
        aiTileHazard = new float[GridSize, GridSize];
        enemies = new List<Enemy>();
        particles = new List<Particle>();
        trails = new List<DashTrail>();
        floaters = new List<FloatingText>();
        projectiles = new List<Projectile>();

        gunUnlocked = new bool[Guns.Length];
        gunUnlocked[0] = true;
        shopBarVis = new float[Guns.Length];
        upgradeLevels = new int[UpgradeCount];
        bodyUnlocked = new bool[BodyPalette.Length];
        bodyUnlocked[0] = bodyUnlocked[1] = bodyUnlocked[2] = true;
        accessoryUnlocked = new bool[AccessoryNames.Length];
        accessoryUnlocked[0] = true;
        bestiaryKills = new int[EnemyCatalog.Length];
        mottoUnlocked = new bool[MottoLines.Length];
        difficultyRecords = new DifficultyRecord[Enum.GetValues<Difficulty>().Length];
        abilityUnlocked = new bool[AbilityCount];
        abilityUnlocked[(int)AbilityType.Paralyze] = true;
        abilityUnlocked[(int)AbilityType.WindStep] = true;

        LoadGame();
        ApplyFpsCap();
        activeDifficulty = GetDifficultyProfile(runDifficulty);
        if (ShouldResetProgressOnce())
        {
            ResetProgress();
        }
        ResizeUnlockArrays();
        ApplyLevelUnlocks();
        InitMotes();
        InitGfx();
        state = GameState.MainMenu;

        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }

        SaveGame();
        UnloadGfx();
        Raylib.EnableCursor();
        Raylib.CloseWindow();
    }

}
