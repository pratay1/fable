partial class Program
{
    // ---------------------------------------------------------------- Immersion systems

    static Color HeraldryAccent(Color baseCol, float mix = 0.35f)
        => LerpColor(baseCol, BodyColor(), mix);

    static int GetRunTimePhase() => Math.Clamp(waveNumber / 8, 0, 3);

    static Color GetTimeOfDayTint()
    {
        return GetRunTimePhase() switch
        {
            0 => new Color(255, 238, 210, 255),
            1 => new Color(255, 255, 248, 255),
            2 => new Color(255, 214, 188, 255),
            _ => new Color(188, 200, 228, 255),
        };
    }

    static float GetTimeOfDayStrength() => 0.06f + (waveNumber % 8) / 8f * 0.08f;

    static void ResetImmersionRunState()
    {
        nearDeathPulse = 0f;
        lastComboNarrationTier = 0;
        betweenWaveVignetteTimer = 0f;
        activeBlessingCount = 0;
        Array.Fill(activeBlessings, BlessingType.None);
        blessingPickActive = false;
        siegeObjective = SiegeObjectiveType.None;
        siegeObjectiveDone = false;
        siegeObjectiveFailed = false;
        siegeGruntsSpawned = 0;
        siegeGruntsKilled = 0;
        runLockedBodyIndex = (runOathFlags & (1 << (int)OathType.HeraldryBound)) != 0 ? bodyColorIndex : -1;
        reinforceCooldown = 0f;
        royalPardonUsed = false;
        eventChainActive = false;
        reverseSiegeActive = false;
        reverseSiegeTimer = 0f;
        playerTrailWrite = 0;
        Array.Fill(playerTrailTiles, -1);
        runGunAffix = playerLevel >= 25 ? (GunAffixType)(1 + Rng.Next(8)) : GunAffixType.None;
    }

    static void RecordPlayerTrailTile()
    {
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return;
        int slot = playerTrailWrite % playerTrailTiles.Length;
        playerTrailTiles[slot] = tx + ty * GridSize;
        playerTrailX[slot] = tx;
        playerTrailY[slot] = ty;
        playerTrailWrite++;
    }

    static bool PlayerRecentlyOnTile(int tx, int ty)
    {
        for (int i = 0; i < playerTrailTiles.Length; i++)
        {
            if (playerTrailTiles[i] < 0) continue;
            if (playerTrailX[i] == tx && playerTrailY[i] == ty) return true;
        }
        return false;
    }

    static bool HasSynergyBannerLockstep() => accessoryIndex == AccessoryKeepBanner && upgradeLevels[UpLockstep] > 0;
    static bool HasSynergyStormPierce() => accessoryIndex == AccessoryStormGlass && upgradeLevels[UpPierce] > 0;
    static float EffBannerDuration() => BannerDuration * (HasSynergyBannerLockstep() ? 1.2f : 1f);
    static int StormGlassPeekCount() => accessoryIndex == AccessoryStormGlass ? 1 + (upgradeLevels[UpPierce] > 0 ? 1 : 0) : 0;

    static Vector2 ApplyFloorStateBias(Vector2 step, in Enemy enemy, EnemyBehavior behavior)
    {
        if (!TryGetTileUnder(enemy.Position, out int etx, out int ety)) return step;
        Vector2 bias = Vector2.Zero;

        if (behavior == EnemyBehavior.Zigzag)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = etx + dx, ny = ety + dy;
                    if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) continue;
                    if (tiles[nx, ny].State == 1)
                        bias += Vector2.Normalize(TileCenter(nx, ny) - enemy.Position);
                }
            }
        }
        else if (behavior == EnemyBehavior.TileLeech)
        {
            float best = float.MaxValue;
            Vector2 target = Vector2.Zero;
            for (int dy = -3; dy <= 3; dy++)
            {
                for (int dx = -3; dx <= 3; dx++)
                {
                    int nx = etx + dx, ny = ety + dy;
                    if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) continue;
                    ref Tile t = ref tiles[nx, ny];
                    if (t.State == 2) continue;
                    if (t.Durability < best)
                    {
                        best = t.Durability;
                        target = TileCenter(nx, ny);
                    }
                }
            }
            if (best < float.MaxValue) bias = target - enemy.Position;
        }
        else if (behavior == EnemyBehavior.CrushTiles)
        {
            for (int i = 0; i < playerTrailTiles.Length; i++)
            {
                if (playerTrailTiles[i] < 0) continue;
                bias += Vector2.Normalize(TileCenter(playerTrailX[i], playerTrailY[i]) - enemy.Position);
            }
        }

        if (bias.LengthSquared() < 0.01f) return step;
        Vector2 blended = Vector2.Normalize(step + Vector2.Normalize(bias) * 0.45f);
        return blended * step.Length();
    }

    static void TickBossPhases(ref Enemy enemy, ref readonly EnemyDef def)
    {
        if (!def.Boss) return;
        float hpRatio = enemy.Hp / Math.Max(1f, enemy.MaxHp);

        if (def.Behavior == EnemyBehavior.BossBlight && hpRatio < 0.5f && enemy.Phase < 1f)
        {
            enemy.Phase = 1f;
            enemy.AbilityTimer = Math.Min(enemy.AbilityTimer, 3.6f);
            BlightArea(enemy.Position, 2);
            SpawnFloatingText(enemy.Position + new Vector2(0, -30f), "THORNS RISE", Danger, 18);
            AddTrauma(0.28f);
        }
        else if (def.Behavior == EnemyBehavior.BossDash && hpRatio < 0.5f && enemy.Phase < 1f)
        {
            enemy.Phase = 1f;
            if (enemy.AbilityTimer > 1.8f) enemy.AbilityTimer = 1.8f;
            SpawnFloatingText(enemy.Position + new Vector2(0, -30f), "FOX FURY", Danger, 18);
            AddTrauma(0.22f);
        }
        else if (def.Behavior == EnemyBehavior.BossSmash)
        {
            if (hpRatio < 0.66f && enemy.Phase < 1f)
            {
                enemy.Phase = 1f;
                enemy.StrikeDepth = Math.Max(enemy.StrikeDepth, 3);
            }
            if (hpRatio < 0.33f && enemy.Phase < 2f)
            {
                enemy.Phase = 2f;
                enemy.StrikeDepth = Math.Max(enemy.StrikeDepth, 4);
                SpawnFloatingText(enemy.Position + new Vector2(0, -36f), "THE KEEP SPLITS", Danger, 20);
                AddTrauma(0.35f);
            }
        }
    }

    static void UpdateSiegeObjective(float dt)
    {
        if (siegeObjective == SiegeObjectiveType.None || siegeObjectiveFailed) return;

        if (siegeObjective == SiegeObjectiveType.ClearBreach)
        {
            siegeObjectiveTimer -= dt;
            if (siegeObjectiveTimer <= 0f && siegeGruntsKilled < siegeGruntsSpawned)
                siegeObjectiveFailed = true;
        }
        else if (siegeObjective == SiegeObjectiveType.ProtectCorner)
        {
            if (tiles[siegeCornerTx, siegeCornerTy].State == 2)
                siegeObjectiveFailed = true;
        }
        else if (siegeObjective == SiegeObjectiveType.HoldBanner && waveInProgress
            && waveSwarmIndex >= waveSwarmTotal - 1 && waveSwarmTotal > 0)
        {
            int cx = GridSize / 2;
            if (!TryGetTileUnder(playerPos, out int px, out int py)
                || px < cx - 1 || px > cx || py < cx - 1 || py > cx)
            {
                siegeObjectiveFailed = true;
            }
            else
            {
                siegeObjectiveDone = true;
            }
        }
    }

    static void CompleteSiegeObjectiveBonus()
    {
        if (siegeObjective == SiegeObjectiveType.None || siegeObjectiveFailed) return;
        if (siegeObjective == SiegeObjectiveType.ClearBreach && siegeGruntsKilled < siegeGruntsSpawned) return;
        if (siegeObjective == SiegeObjectiveType.HoldBanner && !siegeObjectiveDone) return;

        siegeObjectiveDone = true;
        int bonus = 12 + waveNumber * 2;
        fables += bonus;
        runFablesEarned += bonus;
        SpawnFloatingText(playerPos + new Vector2(0, -58f), "+" + bonus + " siege bonus", Gold, 18);
    }

    static string SiegeObjectiveLabel() => siegeObjective switch
    {
        SiegeObjectiveType.HoldBanner => "HOLD THE BANNER TILE",
        SiegeObjectiveType.ClearBreach => "CLEAR THE BREACH",
        SiegeObjectiveType.ProtectCorner => "PROTECT THE CORNER",
        _ => "",
    };

    static void ApplyCollapseProximityShake(int tileX, int tileY)
    {
        if (!TryGetTileUnder(playerPos, out int px, out int py)) return;
        int dist = Math.Max(Math.Abs(tileX - px), Math.Abs(tileY - py));
        if (dist <= 2) AddTrauma((3 - dist) * 0.06f * shakeScale);
    }

    static void SpawnComboNarration(int c)
    {
        if (!floatingTextEnabled) return;
        int tier = c switch { >= 35 => 4, >= 20 => 3, >= 10 => 2, >= 5 => 1, _ => 0 };
        if (tier <= lastComboNarrationTier) return;
        lastComboNarrationTier = tier;
        string msg = tier switch { 1 => "HOLDING THE LINE", 2 => "UNBREAKABLE", 3 => "LEGENDARY STREAK", _ => "THE BAILEY STANDS" };
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 72f), msg, ComboColor(c), 22);
    }

    static void TriggerBossHitStop(bool boss)
    {
        if (hitStopEnabled && boss) hitstop = Math.Max(hitstop, 0.06f);
    }

    static void UpdateNearDeathPulse(float dt)
    {
        float target = 0f;
        if (TryGetTileUnder(playerPos, out int tx, out int ty))
        {
            ref Tile feet = ref tiles[tx, ty];
            if (activeEvent != FloorEventType.None && eventCountdown > 0f && feet.EventMarked && eventCountdown < 2f) target = 1f;
            else if (feet.State != 2 && feet.Durability < MaxDurability * 0.18f) target = 0.75f;
        }
        nearDeathPulse = Approach(nearDeathPulse, target, 4f, dt);
    }

    static void UpdateBetweenWaveAmbience(float dt)
    {
        if (waveInProgress) return;
        betweenWaveVignetteTimer += dt;
        if (betweenWaveVignetteTimer < 0.35f || Rng.NextSingle() > 0.55f) return;
        betweenWaveVignetteTimer = 0f;
        AddParticle(new Particle
        {
            Position = new Vector2(Rng.Next(40, WindowWidth - 40), Rng.Next(40, WindowHeight - 40)),
            Velocity = new Vector2(Rng.NextSingle() * 12f - 6f, -18f - Rng.NextSingle() * 22f),
            Color = new Color(196, 128, 64, 255),
            Alpha = 0.55f, Fade = 0.9f, Size = 2.5f, Drag = 1.2f, Glow = true,
        });
    }

    static void PushChronicleEntry()
    {
        var (cause, _) = GetDeathCauseCopy();
        ChronicleBuffer[chronicleWrite % ChronicleBuffer.Length] = $"Wave {waveNumber} - {cause} - {activeDifficulty.Title}";
        chronicleWrite++;
        chronicleCount = Math.Min(chronicleCount + 1, ChronicleBuffer.Length);
    }

    static void UnlockMottosForLevel()
    {
        for (int i = 0; i < MottoLines.Length; i++)
        {
            if (!mottoUnlocked[i] && playerLevel >= MottoLevelReq[i]) mottoUnlocked[i] = true;
        }
    }

    static string GetUnlockedMotto()
    {
        for (int i = MottoLines.Length - 1; i >= 0; i--)
            if (mottoUnlocked[i]) return MottoLines[i];
        return "";
    }

    static void UpdateDifficultyRecords(bool final)
    {
        int di = (int)runDifficulty;
        if (di < 0 || di >= difficultyRecords.Length) return;
        ref DifficultyRecord rec = ref difficultyRecords[di];
        if (waveNumber > rec.BestWave) rec.BestWave = waveNumber;
        if (score > rec.BestScore) rec.BestScore = score;
        if (final && runKillCount > rec.BestKills) rec.BestKills = runKillCount;
    }

    static bool IsPracticeRun() => runDifficulty == Difficulty.PracticeHall;

    static void IncrementBestiary(EnemyType type)
    {
        int i = (int)type;
        if (i >= 0 && i < bestiaryKills.Length) bestiaryKills[i]++;
    }

    static EventFamily GetEventFamily(FloorEventType ev) => ev switch
    {
        FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede or FloorEventType.TideColumn
            or FloorEventType.TideEcho or FloorEventType.TideUndertow or FloorEventType.TideCrest or FloorEventType.TideWall
            or FloorEventType.TideAnchor or FloorEventType.TideFoam or FloorEventType.TideWhirlpool or FloorEventType.TideBeacon
            or FloorEventType.TideStrike or FloorEventType.SallyForth => EventFamily.Tide,
        FloorEventType.EmberRain or FloorEventType.EmberGate or FloorEventType.EmberPulse or FloorEventType.EmberCross
            or FloorEventType.EmberBridge or FloorEventType.EmberFury or FloorEventType.EmberSnake or FloorEventType.EmberHive
            or FloorEventType.EmberTide or FloorEventType.EmberCage or FloorEventType.EmberQuake or FloorEventType.EmberBloom
            or FloorEventType.EmberAltar => EventFamily.Ember,
        FloorEventType.CryptSeal or FloorEventType.CryptWail or FloorEventType.CryptTorch or FloorEventType.CryptChains
            or FloorEventType.CryptMist or FloorEventType.CryptTomb or FloorEventType.CryptShroud or FloorEventType.CryptGlimpse
            or FloorEventType.CryptRattle or FloorEventType.CryptEcho or FloorEventType.CryptGrave or FloorEventType.CryptLantern
            or FloorEventType.CryptVeil or FloorEventType.Portcullis => EventFamily.Crypt,
        FloorEventType.CrownTrial or FloorEventType.CrownFall or FloorEventType.CrownShard or FloorEventType.CrownThrone
            or FloorEventType.CrownEdict or FloorEventType.CrownRot or FloorEventType.CrownBolt or FloorEventType.CrownRing
            or FloorEventType.CrownIsles or FloorEventType.CrownStorm or FloorEventType.CrownCoronation or FloorEventType.CrownUsurpation
            or FloorEventType.CrownReckoning or FloorEventType.HeraldsCall => EventFamily.Crown,
        _ => EventFamily.General,
    };

    static string GetArenaQuadrantName(int tx, int ty)
    {
        bool left = tx < GridSize / 2;
        bool top = ty < GridSize / 2;
        return (left, top) switch
        {
            (true, true) => "NW BAILEY",
            (false, true) => "NE WATCH",
            (false, false) => "SE CHAPEL",
            _ => "SW BREACH",
        };
    }

    static string GetEventGlossaryTip(FloorEventType ev) => ev switch
    {
        FloorEventType.SafeZoneRush or FloorEventType.TideBeacon or FloorEventType.EmberGate => "Reach the glowing safe band before time runs out.",
        FloorEventType.CenterSnare or FloorEventType.CryptSeal or FloorEventType.TideWhirlpool => "The center dies - sprint for the rim.",
        FloorEventType.StoneIslands or FloorEventType.TideAnchor or FloorEventType.CrownIsles => "Find the safe islands; marked stone collapses.",
        FloorEventType.MarkedStrike or FloorEventType.TideStrike or FloorEventType.CryptGrave => "Leave marked tiles before they fall in sequence.",
        FloorEventType.MossRot or FloorEventType.CrownRot => "One half of the arena rots faster - stay on the healthy side.",
        FloorEventType.SallyForth => "The safe rim rotates - keep moving along the wall.",
        FloorEventType.Portcullis => "Columns fall in order - read the telegraph lanes.",
        FloorEventType.HeraldsCall => "Only your herald's footing holds - stay on the safe island.",
        _ => "Unmarked tiles are safe until the countdown ends.",
    };

    static readonly (FloorEventType Precursor, FloorEventType FollowUp)[] EventChains =
    {
        (FloorEventType.CryptMist, FloorEventType.CryptVeil),
        (FloorEventType.CryptVeil, FloorEventType.CryptEcho),
        (FloorEventType.TideSurge, FloorEventType.TideCrest),
        (FloorEventType.EmberRain, FloorEventType.EmberPulse),
        (FloorEventType.CrownTrial, FloorEventType.CrownStorm),
    };

    static float BlessingMoveMult()
    {
        float m = 1f;
        for (int i = 0; i < activeBlessingCount; i++)
            if (activeBlessings[i] == BlessingType.SwiftMarch) m += 0.05f;
        return m;
    }

    static float BlessingFableMult()
    {
        float m = 1f;
        for (int i = 0; i < activeBlessingCount; i++)
        {
            if (activeBlessings[i] == BlessingType.DeepPockets) m += 0.12f;
            if (activeBlessings[i] == BlessingType.LuckySigil) m += 0.08f;
        }
        return m;
    }

    static float BlessingTelegraphBonus()
    {
        float b = 0f;
        for (int i = 0; i < activeBlessingCount; i++)
            if (activeBlessings[i] == BlessingType.LongFuse) b += 0.5f;
        return b;
    }

    static float GunAffixDamageMult() => runGunAffix switch
    {
        GunAffixType.Steady => 1.12f,
        GunAffixType.Heavy => 1.2f,
        GunAffixType.Volatile => 0.92f,
        _ => 1f,
    };

    static float GunAffixFireRateMult() => runGunAffix switch
    {
        GunAffixType.Volatile => 1.18f,
        GunAffixType.Quickdraw => 1.1f,
        GunAffixType.Heavy => 0.88f,
        _ => 1f,
    };

    static string GunAffixName() => runGunAffix switch
    {
        GunAffixType.Volatile => "Volatile",
        GunAffixType.Steady => "Steady",
        GunAffixType.Leeching => "Leeching",
        GunAffixType.Piercing => "Piercing",
        GunAffixType.Quickdraw => "Quickdraw",
        GunAffixType.Heavy => "Heavy",
        GunAffixType.Lucky => "Lucky",
        GunAffixType.Ranger => "Ranger",
        _ => "",
    };

    static float OathRewardMult()
    {
        float m = 1f;
        if ((runOathFlags & (1 << (int)OathType.NoVerdict)) != 0) m += 0.15f;
        if ((runOathFlags & (1 << (int)OathType.NoOath)) != 0) m += 0.2f;
        if ((runOathFlags & (1 << (int)OathType.HeraldryBound)) != 0) m += 0.15f;
        if ((runOathFlags & (1 << (int)OathType.PureNightmare)) != 0 && runDifficulty == Difficulty.FableNightmare) m += 0.25f;
        for (int i = 0; i < activeBlessingCount; i++)
            if (activeBlessings[i] == BlessingType.SiegeRations) m += 0.1f;
        return m;
    }

    static bool OathBlocksVerdict() => (runOathFlags & (1 << (int)OathType.NoVerdict)) != 0;
    static bool OathBlocksOath() => (runOathFlags & (1 << (int)OathType.NoOath)) != 0;

    static void OfferBlessings()
    {
        if (waveNumber % 5 != 0 || waveNumber <= 0 || activeBlessingCount >= activeBlessings.Length) return;
        var pool = new List<BlessingType>();
        for (int i = 1; i <= (int)BlessingType.WindBlessing; i++) pool.Add((BlessingType)i);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        for (int i = 0; i < blessingChoices.Length; i++) blessingChoices[i] = pool[i];
        blessingPickActive = true;
        state = GameState.Paused;
    }

    static void RollSiegeObjective()
    {
        siegeObjective = SiegeObjectiveType.None;
        siegeObjectiveDone = false;
        siegeObjectiveFailed = false;
        siegeGruntsSpawned = 0;
        siegeGruntsKilled = 0;
        if (IsPracticeRun() || IsBossWave(waveNumber) || Rng.NextSingle() > 0.3f) return;
        siegeObjective = (SiegeObjectiveType)Rng.Next(1, 4);
        if (siegeObjective == SiegeObjectiveType.ProtectCorner)
        {
            siegeCornerTx = Rng.Next(2) == 0 ? 1 : GridSize - 2;
            siegeCornerTy = Rng.Next(2) == 0 ? 1 : GridSize - 2;
        }
        else if (siegeObjective == SiegeObjectiveType.ClearBreach)
        {
            siegeObjectiveTimer = 22f + waveNumber * 0.4f;
        }
        waveSubtext = SiegeObjectiveLabel();
    }

    static int ReinforceFableCost() => Math.Max(5, ReinforceFableBase + waveNumber * 2);

    static bool TryReinforceTile()
    {
        if (reinforceCooldown > 0f) return false;
        if (!TryGetTileUnder(playerPos, out int tx, out int ty)) return false;
        int cost = ReinforceFableCost();
        if (fables < cost) return false;

        int fortified = 0;
        for (int dy = -ReinforceRadius; dy <= ReinforceRadius; dy++)
        {
            for (int dx = -ReinforceRadius; dx <= ReinforceRadius; dx++)
            {
                int nx = tx + dx, ny = ty + dy;
                if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) continue;
                ref Tile t = ref tiles[nx, ny];
                if (t.State == 2) continue;
                fortified++;
            }
        }
        if (fortified == 0) return false;

        fables -= cost;
        Vector2 epicenter = TileCenter(tx, ty);
        for (int dy = -ReinforceRadius; dy <= ReinforceRadius; dy++)
        {
            for (int dx = -ReinforceRadius; dx <= ReinforceRadius; dx++)
            {
                int nx = tx + dx, ny = ty + dy;
                if (nx < 0 || nx >= GridSize || ny < 0 || ny >= GridSize) continue;
                ref Tile t = ref tiles[nx, ny];
                if (t.State == 2) continue;
                t.State = 0;
                t.Durability = MaxDurability * ReinforceFortifiedMult;
                t.Collapse = 0f;
                t.EventMarked = false;
                t.RegrowTimer = 0f;
                t.UntouchedTimer = ReinforceFreshTimer;
            }
        }

        reinforceCooldown = ReinforceCooldownTime;
        abilityIFrameTimer = Math.Max(abilityIFrameTimer, 0.35f);
        SpawnFloatingText(epicenter, "FORTIFIED", Gold, 20);
        SpawnFloatingText(epicenter + new Vector2(0, -22f), "+" + (int)(ReinforceFortifiedMult * 100f - 100f) + "% stone", UiAccent, 14);
        SpawnEventShockwave(epicenter, UiAccent, TileSize * (ReinforceRadius + 1.5f), 0.55f);
        AddFlash(UiAccent, 0.14f);
        AddTrauma(0.1f);
        return true;
    }

    static bool TryRoyalPardon()
    {
        if (royalPardonUsed || activeEvent != FloorEventType.None || verdictHaltTimer > 0f) return false;
        int cost = 200 + waveNumber * 10;
        if (fables < cost) return false;
        fables -= cost;
        royalPardonUsed = true;
        nextFloorEventTimer = 0f;
        StartFloorEvent(PickNextFloorEvent());
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 100f), "ROYAL PARDON", Gold, 22);
        return true;
    }

    static string GetBossBark(EnemyType boss)
    {
        int di = Math.Clamp((int)runDifficulty, 0, 4);
        return boss switch
        {
            EnemyType.BrambleLord => di switch { 0 => "Even a beginner smells fear.", 1 => "The thorns remember your name.", 2 => "The marshal rides for the bailey.", 3 => "Champion - the brambles hunger.", _ => "Nightmare or not, you bleed." },
            EnemyType.FoxWarden => di switch { 0 => "A fox outruns a novice.", 1 => "Scarlet crest - do not blink.", 2 => "The warden tests your nerve.", 3 => "Champion's blood paints the wall.", _ => "Run. You cannot." },
            EnemyType.GroveTitan => di switch { 0 => "The colossus barely notices you.", 1 => "Stone shakes for the titan.", 2 => "The keep groans under its tread.", 3 => "Champion - the walls split.", _ => "The nightmare walks." },
            _ => "",
        };
    }

    static void BeginReverseSiege()
    {
        if (!IsBossWave(waveNumber) || Rng.NextSingle() > 0.25f) return;
        reverseSiegeActive = true;
        reverseSiegeTimer = 8f;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                bool edge = x == 0 || y == 0 || x == GridSize - 1 || y == GridSize - 1;
                if (!edge) continue;
                ref Tile t = ref tiles[x, y];
                if (t.State == 2) { t.State = 0; t.Durability = MaxDurability * 0.7f; t.RegrowTimer = 0f; }
            }
        }
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 140f), "REVERSE SIEGE", Danger, 24);
    }

    static void UpdateReverseSiege(float dt)
    {
        if (!reverseSiegeActive) return;
        reverseSiegeTimer -= dt;
        if (reverseSiegeTimer > 0f) return;
        reverseSiegeActive = false;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                bool edge = x == 0 || y == 0 || x == GridSize - 1 || y == GridSize - 1;
                if (!edge) continue;
                ref Tile t = ref tiles[x, y];
                if (t.State != 2) CollapseTile(ref t, true, x, y);
            }
        }
    }

    static void DrawHeraldryHatch(Rectangle r, Color color, int index)
    {
        if (!heraldryPatterns) return;
        Color line = WithAlpha(Lighten(color, 0.6f), 0.55f);
        Vector2 c = new Vector2(r.X + r.Width * 0.5f, r.Y + r.Height * 0.5f);
        for (int i = -2; i <= 2; i++)
        {
            float off = i * 5f;
            Raylib.DrawLineEx(c + new Vector2(-r.Width * 0.5f, off), c + new Vector2(r.Width * 0.5f, off), 1f, line);
            Raylib.DrawLineEx(c + new Vector2(off, -r.Height * 0.5f), c + new Vector2(off, r.Height * 0.5f), 1f, line);
        }
    }

}
