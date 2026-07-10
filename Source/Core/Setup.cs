partial class Program
{
    // ---------------------------------------------------------------- Setup

    static void InitMotes()
    {
        motes = new Mote[90];
        for (int i = 0; i < motes.Length; i++)
        {
            bool isLeaf = i % 3 == 0;
            Color col = MoteColor(isLeaf, i);

            motes[i] = new Mote
            {
                Position = new Vector2(Rng.Next(0, WindowWidth), Rng.Next(0, WindowHeight)),
                Radius = isLeaf ? 2.2f + Rng.NextSingle() * 2.5f : 1.2f + Rng.NextSingle() * 1.4f,
                Speed = isLeaf ? 14f + Rng.NextSingle() * 28f : 4f + Rng.NextSingle() * 10f,
                Phase = Rng.NextSingle() * MathF.PI * 2f,
                Color = col,
                IsLeaf = isLeaf,
                Spin = Rng.NextSingle() * 360f,
            };
        }
    }

    static Color MoteColor(bool isLeaf, int index)
    {
        return isLeaf
            ? (index % 2 == 0 ? new Color(108, 106, 102, 255) : new Color(88, 86, 82, 255))
            : new Color(196, 194, 186, 255);
    }

    static void RefreshMoteColors()
    {
        for (int i = 0; i < motes.Length; i++)
        {
            Mote m = motes[i];
            m.Color = MoteColor(m.IsLeaf, i);
            motes[i] = m;
        }
    }

    static void ResetGame()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                tiles[x, y] = new Tile { State = 0, Durability = MaxDurability, Collapse = 0f, RegrowTimer = 0f, UntouchedTimer = 0f, EventMarked = false };
            }
        }

        enemies.Clear();
        particles.Clear();
        trails.Clear();
        floaters.Clear();
        projectiles.Clear();

        playerPos = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
        playerVel = Vector2.Zero;
        lastMoveDirection = Vector2.UnitY;

        score = 0;
        scoreDisplay = 0f;
        runFablesEarned = 0;
        waveNumber = 0;
        waveBannerTimer = 0f;
        waveInProgress = false;
        waveSwarmIndex = 0;
        waveSwarmTotal = 0;
        swarmCooldown = 0f;
        betweenWaveTimer = BetweenWavePause - 2.4f;
        waveSubtext = "";
        activeEvent = FloorEventType.None;
        eventTimer = 0f;
        eventCountdown = 0f;
        eventSide = 0;
        eventStep = 0;
        eventSafeRect = default;
        eventDangerRect = default;
        playerInEventSafeZone = false;
        floorRotTimer = 0f;
        floorRotSide = 0f;
        activeDifficulty = GetDifficultyProfile(runDifficulty);
        eventSurgeTimer = 0f;
        ResetImmersionRunState();
        nextFloorEventCooldown = Math.Max(0.5f, NextEventCooldownSpan());
        nextFloorEventTimer = nextFloorEventCooldown;
        markedStrikeCount = 0;
        eventPhase = 0;
        eventStartCountdown = 0f;
        eventActionTimer = 0f;
        eventTileQueue.Clear();
        eventShockwaves.Clear();
        eventSkyBeams.Clear();
        floorEventHistoryCount = 0;
        combo = 0;
        comboTimer = 0f;
        dashTimer = 0f;
        dashCooldown = 0f;
        paralyzeCooldown = 0f;
        verdictCooldown = 0f;
        verdictHaltTimer = 0f;
        siegeGateOpenTimer = 0f;
        siegeEventDangerLight = 0f;
        siegeEventSafeLight = 0f;
        siegeEventTorchPulse = 0f;
        runKillCount = 0;
        bannerCooldown = 0f;
        oathUsedThisRun = false;
        abilityIFrameTimer = 0f;
        oathRescueFlashTimer = 0f;
        verdictWaveTimer = 0f;
        bannerActive = false;
        bannerTimer = 0f;
        bannerPlantTimer = 0f;
        oathReinforcedTx = oathReinforcedTy = -1;
        oathTilePulseTimer = 0f;
        windDashVfxTimer = 0f;
        abilitySlotTracked[0] = abilitySlot1;
        abilitySlotTracked[1] = abilitySlot2;
        abilityFillVis[0] = AbilityReadiness(abilitySlot1);
        abilityFillVis[1] = AbilityReadiness(abilitySlot2);
        paralyzeBurstTimer = 0f;
        stepTimer = 0f;
        blightTimer = 0f;
        fireTimer = 0f;
        pendingBurstShots = 0;
        pendingBurstTimer = 0f;
        ammoInMag = GetMagazineSize(in Guns[equippedGun]);
        reloadTimer = 0f;
        weaponAimDir = Vector2.UnitY;
        weaponAimTarget = playerPos + Vector2.UnitY * 80f;
        weaponRecoil = 0f;
        cameraDashLean = 0f;
        cameraDashLeanDir = Vector2.Zero;
        trauma = 0f;
        zoomPunch = 0f;
        flash = 0f;
        hitstop = 0f;
        impactFlash = 0f;
        impactFlashSharp = false;
        adrenaline = 0f;
        gfxLightPulses.Clear();
        aiGameOverTimer = 0f;
        if (aiPilotEnabled) aiPilotBannerTimer = 8f;
        aiSteerVel = Vector2.Zero;
        aiMoveSmoothed = Vector2.Zero;
        aiTopBandTimer = 0f;
        aiCrumbleDashUrgency = 0f;
        aiDashUrgency = 0f;
        aiPostParalyzeGrace = 0f;
        ResetAiGrandmasterBrain();
        lastDeathCause = DeathCause.Unknown;
        lastDeathDetail = "";
        state = GameState.Playing;
    }

}
