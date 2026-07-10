partial class Program
{
    // ---------------------------------------------------------------- Update

    static void Update()
    {
        float dt = Raylib.GetFrameTime();
        frameTime = (float)Raylib.GetTime();
        menuTime += dt;

        UpdateCursorLock();
        UpdateWindowChrome();

        if (backgroundMotes) UpdateMotes(dt);
        scoreDisplay += (score - scoreDisplay) * (1f - MathF.Exp(-10f * dt));
        flash *= MathF.Exp(-6f * dt);
        impactFlash *= MathF.Exp(-32f * dt);
        zoomPunch *= MathF.Exp(-9f * dt);
        UpdateGfx(dt);
        if (levelUpBannerTimer > 0f) levelUpBannerTimer -= dt;
        if (settingsEggBannerTimer > 0f) settingsEggBannerTimer -= dt;
        if (uiInputBlockFrames > 0) uiInputBlockFrames--;

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
        {
            showFps = !showFps;
            SaveGame();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backslash))
        {
            aiPilotEnabled = !aiPilotEnabled;
            aiGameOverTimer = 0f;
            if (aiPilotEnabled)
            {
                aiPilotBannerTimer = 8f;
                ResetAiHumanSurvivalDirector();
            }

            if (aiPilotEnabled && state == GameState.MainMenu)
            {
                ResetGame();
            }
            else if (state == GameState.Playing)
            {
                SpawnFloatingText(
                    playerPos + new Vector2(0, -28f),
                    aiPilotEnabled ? "HUMAN SURVIVAL PILOT ON" : "AUTO-PILOT OFF",
                    aiPilotEnabled ? UiAccent : WithAlpha(Color.White, 0.7f),
                    16);
                if (aiPilotEnabled)
                {
                    SpawnFloatingText(
                        playerPos + new Vector2(0, -50f),
                        "Survival planner engaged",
                        new Color(196, 168, 108, 255),
                        13);
                }
            }
        }

        switch (state)
        {
            case GameState.MainMenu:
                if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
                {
                    difficultyMenuIndex = (int)runDifficulty;
                    difficultySelectAnim = difficultyMenuIndex;
                    state = GameState.DifficultySelect;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.C))
                {
                    OpenArmory();
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.S))
                {
                    OpenSettings(GameState.MainMenu);
                }
                break;

            case GameState.DifficultySelect:
                UpdateDifficultySelect(dt);
                break;

            case GameState.Playing:
                if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    state = GameState.Paused;
                    autoPausedForFocus = false;
                    break;
                }
                if (pauseOnFocusLoss && !Raylib.IsWindowFocused())
                {
                    state = GameState.Paused;
                    autoPausedForFocus = true;
                    break;
                }
                PollAbilityHotkeys();
                if (ProcessHitStop(dt))
                {
                    break;
                }

                trauma = Math.Max(0f, trauma - TraumaDecay * dt);
                UpdateParticles(dt);
                UpdateTrails(dt);
                UpdateFloaters(dt);
                UpdateTiles(dt);

                if (comboTimer > 0f)
                {
                    comboTimer -= dt;
                    if (comboTimer <= 0f) combo = 0;
                }
                if (waveBannerTimer > 0f) waveBannerTimer -= dt;
                if (aiPilotEnabled && aiPilotBannerTimer > 0f) aiPilotBannerTimer -= dt;

                if (aiPilotEnabled)
                {
                    UpdateAiPilot(dt);
                }

                UpdateAbilities(dt);
                UpdatePlayer(dt);
                UpdateFloorEvents(dt);
                UpdateBlight(dt);
                UpdateWaves(dt);
                UpdateEnemies(dt);
                UpdateWeapon(dt);
                UpdateProjectiles(dt);
                UpdateNearDeathPulse(dt);
                UpdateSiegeGraphics(dt);
                UpdateReverseSiege(dt);
                if (reinforceCooldown > 0f) reinforceCooldown -= dt;
                if (Raylib.IsKeyDown(KeyboardKey.R)) TryReinforceTile();
                if (Raylib.IsKeyPressed(KeyboardKey.P)) TryRoyalPardon();
                RecordPlayerTrailTile();
                ReapEnemies();
                break;

            case GameState.Paused:
                if (blessingPickActive)
                {
                    for (int bi = 0; bi < blessingChoices.Length; bi++)
                    {
                        if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + bi)))
                        {
                            if (activeBlessingCount < activeBlessings.Length)
                            {
                                activeBlessings[activeBlessingCount++] = blessingChoices[bi];
                            }
                            blessingPickActive = false;
                            state = GameState.Playing;
                        }
                    }
                }
                else if (autoPausedForFocus && pauseOnFocusLoss && Raylib.IsWindowFocused())
                {
                    state = GameState.Playing;
                    autoPausedForFocus = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.S))
                {
                    OpenSettings(GameState.Paused);
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    state = GameState.Playing;
                    autoPausedForFocus = false;
                }
                break;

            case GameState.GameOver:
                trauma = Math.Max(0f, trauma - TraumaDecay * dt);
                UpdateParticles(dt);
                UpdateTrails(dt);
                UpdateFloaters(dt);
                UpdateProjectiles(dt);
                UpdateTiles(dt);
                if (aiPilotEnabled)
                {
                    aiGameOverTimer += dt;
                    if (aiGameOverTimer >= 0.6f)
                    {
                        ResetGame();
                    }
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.R)) ResetGame();
                else if (Raylib.IsKeyPressed(KeyboardKey.Escape)) state = GameState.MainMenu;
                break;

            case GameState.Customize:
                if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { SaveGame(); state = GameState.MainMenu; }
                else if (Raylib.IsKeyPressed(KeyboardKey.One)) customizeTab = CustomizeTab.Cosmetics;
                else if (Raylib.IsKeyPressed(KeyboardKey.Two)) customizeTab = CustomizeTab.Weapons;
                else if (Raylib.IsKeyPressed(KeyboardKey.Three)) customizeTab = CustomizeTab.Upgrades;
                else if (Raylib.IsKeyPressed(KeyboardKey.Four)) customizeTab = CustomizeTab.Abilities;
                else if (Raylib.IsKeyPressed(KeyboardKey.Five)) customizeTab = CustomizeTab.Rank;
                else if (Raylib.IsKeyPressed(KeyboardKey.Six)) customizeTab = CustomizeTab.Bestiary;
                else if (Raylib.IsKeyPressed(KeyboardKey.Seven)) customizeTab = CustomizeTab.Glossary;
                if (state == GameState.Customize)
                {
                    float wheel = Raylib.GetMouseWheelMove();
                    if (wheel != 0f)
                    {
                        customizeScroll = Math.Clamp(customizeScroll - wheel * 42f, 0f, ArmoryScrollMax());
                    }
                }
                break;

            case GameState.Settings:
                if (rebindTarget != RebindTarget.None)
                {
                    int k = Raylib.GetKeyPressed();
                    if (k != 0)
                    {
                        switch (rebindTarget)
                        {
                            case RebindTarget.Fire: fireKey = (KeyboardKey)k; break;
                            case RebindTarget.Ability1: abilityKey1 = (KeyboardKey)k; break;
                            case RebindTarget.Ability2: abilityKey2 = (KeyboardKey)k; break;
                            default: throw new UnreachableException();
                        }
                        rebindTarget = RebindTarget.None;
                        SaveGame();
                    }
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    settingsTypeBuffer = "";
                    SaveGame();
                    state = settingsReturnState;
                }
                else
                {
                    UpdateSettingsEasterEggInput();
                    float wheel = Raylib.GetMouseWheelMove();
                    if (wheel != 0f)
                    {
                        settingsScroll = Math.Clamp(settingsScroll - wheel * 42f, 0f, SettingsScrollMax());
                    }
                }
                break;

            default:
                throw new UnreachableException();
        }

        UpdateCursorLock();
    }

    static void UpdateMotes(float dt)
    {
        if (!backgroundMotes) return;
        float time = frameTime > 0f ? frameTime : (float)Raylib.GetTime();
        for (int i = 0; i < motes.Length; i++)
        {
            Mote m = motes[i];
            if (m.IsLeaf)
            {
                m.Position.Y += m.Speed * dt;
                m.Position.X += MathF.Sin(time * 0.8f + m.Phase) * 18f * dt;
                m.Spin += 40f * dt;
            }
            else
            {
                m.Position.Y -= m.Speed * dt * 0.4f;
                m.Position.X += MathF.Sin(time * 1.4f + m.Phase) * 12f * dt;
            }

            if (m.Position.Y < -8f)
            {
                m.Position.Y = WindowHeight + 8f;
                m.Position.X = Rng.Next(0, WindowWidth);
            }
            else if (m.Position.Y > WindowHeight + 8f)
            {
                m.Position.Y = -8f;
                m.Position.X = Rng.Next(0, WindowWidth);
            }

            motes[i] = m;
        }
    }

    static bool IsTileInMossRotZone(int x)
    {
        bool left = x < GridSize / 2;
        return floorRotSide < 0.5f ? left : !left;
    }

    static bool IsCollapsedVoid(int x, int y)
    {
        if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return false;
        return tiles[x, y].State == 2 && tiles[x, y].Collapse >= 1f;
    }

    static bool IsTileInBannerZone(int x, int y) =>
        bannerActive && bannerTimer > 0f && IsInBannerZone(TileCenter(x, y));

    static void UpdateTiles(float dt)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref Tile tile = ref tiles[x, y];

                if (tile.State == 2)
                {
                    if (tile.Collapse < 1f)
                    {
                        tile.Collapse = Math.Min(1f, tile.Collapse + dt / CollapseTime);
                    }

                    if (tile.RegrowTimer > 0f)
                    {
                        tile.RegrowTimer -= dt;
                        if (tile.RegrowTimer <= 0f)
                        {
                            tile.State = 0;
                            tile.Durability = MaxDurability;
                            tile.Collapse = 0f;
                            tile.UntouchedTimer = 0f;
                            tile.EventMarked = false;
                            SpawnRegrowBurst(TileCenter(x, y));
                        }
                    }

                    continue;
                }

                tile.UntouchedTimer += dt;
                if (tile.UntouchedTimer >= TileIdleRegenDelay && tile.Durability < MaxDurability)
                {
                    tile.Durability = Math.Min(MaxDurability, tile.Durability + TileIdleRegenRate * dt);
                    if (tile.Durability >= MaxDurability * 0.6f) tile.State = 0;
                }

                if (floorRotTimer > 0f && !EventsHaltedByVerdict() && IsTileInMossRotZone(x))
                {
                    if (!IsTileInBannerZone(x, y))
                    {
                        ApplyTileDamage(ref tile, MossRotAreaDecayRate, dt, out bool rotCollapsed, markTouched: false);
                    }
                }
            }
        }
    }

    static Vector2 TileCenter(int x, int y) =>
        new Vector2(x * TileSize + TileSize / 2f, y * TileSize + TileSize / 2f);

    static void SpawnRegrowBurst(Vector2 pos)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * MathF.PI * 2f / 8f;
            AddParticle(new Particle
            {
                Position = pos,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 90f,
                Color = MossLight,
                Alpha = 0.9f,
                Fade = 1f / 0.35f,
                Size = 3f + Rng.NextSingle() * 2f,
                Drag = 3f,
                Glow = true,
            });
        }
    }

    static void CollapseTile(ref Tile tile, bool regrow = true, int tileX = -1, int tileY = -1)
    {
        if (tile.State == 2) return;
        tile.Durability = 0f;
        tile.State = 2;
        tile.Collapse = 0f;
        tile.EventMarked = false;
        tile.RegrowTimer = regrow ? TileRegrowTime : 0f;
        aiIntelTimer = 0f;
        if (tileX >= 0 && tileY >= 0) ApplyCollapseProximityShake(tileX, tileY);
    }

    static float TileDecayMultiplier(int x, int y)
    {
        if (floorRotTimer <= 0f) return 1f;
        bool left = x < GridSize / 2;
        bool affected = floorRotSide < 0.5f ? left : !left;
        return affected ? 2.8f : 1f;
    }

    static void UpdatePlayer(float dt)
    {
        bool dashing = dashTimer > 0f;

        if (dashing)
        {
            dashTimer -= dt;
            playerPos += lastMoveDirection * DashSpeed * dt;
            playerVel = lastMoveDirection * DashSpeed;
            trails.Add(new DashTrail { Position = playerPos, Alpha = 0.75f, Radius = PlayerRadius });
            if (dashTimer <= 0f) dashCooldown = EffDashCooldown();
        }
        else
        {
            dashCooldown = Math.Max(0f, dashCooldown - dt);

            Vector2 move = Vector2.Zero;
            if (aiPilotEnabled && state == GameState.Playing)
            {
                move = aiMove;
            }
            else
            {
                if (Raylib.IsKeyDown(KeyboardKey.W)) move.Y -= 1f;
                if (Raylib.IsKeyDown(KeyboardKey.S)) move.Y += 1f;
                if (Raylib.IsKeyDown(KeyboardKey.A)) move.X -= 1f;
                if (Raylib.IsKeyDown(KeyboardKey.D)) move.X += 1f;
            }

            if (move != Vector2.Zero)
            {
                move = Vector2.Normalize(move);
                lastMoveDirection = move;
            }

            Vector2 targetVel = move * EffMoveSpeed();
            playerVel = Vector2.Lerp(playerVel, targetVel, 1f - MathF.Exp(-PlayerAccel * dt));
            playerPos += playerVel * dt;

            SpawnWalkDust(dt);
        }

        playerPos.X = Math.Clamp(playerPos.X, PlayerRadius, WindowWidth - PlayerRadius);
        playerPos.Y = Math.Clamp(playerPos.Y, PlayerRadius, WindowHeight - PlayerRadius);

        if (dashing || abilityIFrameTimer > 0f)
        {
            return; // i-frames - leap over the earth pit or oath ward
        }

        if (TryGetTileUnder(playerPos, out int tileX, out int tileY))
        {
            ref Tile tile = ref tiles[tileX, tileY];
            if (tile.State == 2)
            {
                TriggerGameOver(DeathCause.FellThrough);
                return;
            }

            if (ShouldSkipWarningTileDamage(tileX, tileY)) return;
            if (IsTileInBannerZone(tileX, tileY)) return;

            ApplyTileDamage(ref tile, EffPlayerDecay() * TileDecayMultiplier(tileX, tileY), dt, out bool collapsed);
            if (collapsed) TriggerGameOver(DeathCause.FloorGaveWay);
        }
    }

    static bool ShouldSkipWarningTileDamage(int tileX, int tileY)
    {
        if ((activeEvent != FloorEventType.StoneIslands && activeEvent != FloorEventType.TideAnchor
                && activeEvent != FloorEventType.CrownIsles) || eventCountdown <= 0f) return false;
        return tiles[tileX, tileY].EventMarked;
    }

    static void UpdateWeaponAim(float dt)
    {
        if (state == GameState.Playing && cursorLocked)
        {
            Vector2 delta = Raylib.GetMouseDelta();
            bool manualAim = !autoFire && !aiPilotEnabled;
            if (manualAim && delta.LengthSquared() > 0.05f)
            {
                Vector2 aim = weaponAimDir.LengthSquared() > 0.01f ? weaponAimDir : lastMoveDirection;
                if (aim.LengthSquared() < 0.01f) aim = Vector2.UnitY;
                float angle = MathF.Atan2(aim.Y, aim.X) + delta.X * 0.024f;
                weaponAimDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                weaponAimTarget = playerPos + weaponAimDir * 120f;
                weaponRecoil = MathF.Max(0f, weaponRecoil - 52f * dt);
                return;
            }
        }

        Vector2 desired;
        int target = aiPilotEnabled && aiFocusEnemyIndex >= 0 ? aiFocusEnemyIndex : NearestEnemyIndex(playerPos);
        if (pendingBurstShots > 0)
        {
            desired = SafeNormalize(pendingBurstTarget - playerPos);
        }
        else if (target >= 0)
        {
            desired = SafeNormalize(enemies[target].Position - playerPos);
            weaponAimTarget = enemies[target].Position;
        }
        else if (lastMoveDirection.LengthSquared() > 0.01f)
        {
            desired = SafeNormalize(lastMoveDirection);
            weaponAimTarget = playerPos + desired * 120f;
        }
        else if (weaponAimDir.LengthSquared() > 0.01f)
        {
            desired = weaponAimDir;
            weaponAimTarget = playerPos + desired * 120f;
        }
        else
        {
            desired = Vector2.UnitY;
            weaponAimTarget = playerPos + desired * 120f;
        }

        float snap = 1f - MathF.Exp(-24f * dt);
        if (weaponAimDir.LengthSquared() < 0.01f)
        {
            weaponAimDir = desired;
        }
        else
        {
            weaponAimDir = SafeNormalize(Vector2.Lerp(weaponAimDir, desired, snap));
        }

        weaponRecoil = MathF.Max(0f, weaponRecoil - 52f * dt);
    }

    static Vector2 GetPlayerMuzzlePos()
    {
        float bodyR = PlayerRadius + (dashTimer > 0f ? 3f : 0f);
        var (grip, _, _, rot, scale) = ComputeEquippedWeaponPose(
            playerPos, bodyR, (float)Raylib.GetTime(), dashTimer > 0f);
        return GetEquippedMuzzleWorld(grip, scale, rot, equippedGun);
    }

    static void KickWeaponRecoil(in Gun g, float strength = 1f)
    {
        float kick = g.Style switch
        {
            GunFireStyle.Laser => 5.5f,
            GunFireStyle.Mortar => 17f,
            GunFireStyle.Sniper => 13f,
            GunFireStyle.Buckshot => 10f,
            GunFireStyle.FlailArc => 8f,
            GunFireStyle.Lance => 9f,
            GunFireStyle.Volley => 7f,
            GunFireStyle.Burst => 6f,
            GunFireStyle.RingPulse => 11f,
            GunFireStyle.CrossBurst => 11f,
            _ => 6.5f,
        };
        weaponRecoil = Math.Min(weaponRecoil + kick * strength, 26f);
    }

    static void SnapWeaponAim(Vector2 targetPos)
    {
        Vector2 delta = targetPos - playerPos;
        if (delta.LengthSquared() > 1f)
        {
            weaponAimDir = SafeNormalize(delta);
            weaponAimTarget = targetPos;
        }
    }

    static int GetMagazineSize(in Gun g)
    {
        int size = g.Style switch
        {
            GunFireStyle.Laser => 11,
            GunFireStyle.Sniper => 14,
            GunFireStyle.Mortar => 16,
            GunFireStyle.Buckshot => 19,
            GunFireStyle.Repeater => 54,
            GunFireStyle.Volley => 27,
            GunFireStyle.Burst => 38,
            GunFireStyle.Lance => 22,
            GunFireStyle.RingPulse => 30,
            GunFireStyle.CrossBurst => 30,
            GunFireStyle.Homing => 24,
            GunFireStyle.FlailArc => 32,
            GunFireStyle.DriftOrb => 27,
            GunFireStyle.ArcFan => 34,
            _ => 34,
        };
        if (g.FireCooldown < 0.2f) size = (int)(size * 1.15f);
        else if (g.FireCooldown > 0.55f) size = Math.Max(8, size - 2);
        float mult = 1.35f + upgradeLevels[UpMagazine] * 0.15f;
        if (runDifficulty == Difficulty.FableNightmare) mult *= 1.3f;
        size = (int)MathF.Round(size * mult);
        return Math.Clamp(size, 8, 72);
    }

    static float GetReloadTime(in Gun g)
    {
        float time = g.Style switch
        {
            GunFireStyle.Laser => 4.1f,
            GunFireStyle.Sniper => 3.8f,
            GunFireStyle.Mortar => 3.9f,
            GunFireStyle.Buckshot => 3.3f,
            GunFireStyle.Repeater => 2.6f,
            GunFireStyle.Volley => 2.9f,
            GunFireStyle.Burst => 2.8f,
            GunFireStyle.Lance => 3.5f,
            GunFireStyle.RingPulse => 3f,
            GunFireStyle.CrossBurst => 3f,
            GunFireStyle.Homing => 3.1f,
            GunFireStyle.FlailArc => 3f,
            GunFireStyle.DriftOrb => 3.2f,
            GunFireStyle.ArcFan => 2.9f,
            _ => 2.9f,
        };
        time += Math.Clamp(g.FireCooldown - 0.3f, 0f, 0.32f);
        float mult = 1f - upgradeLevels[UpReload] * 0.10f;
        if (runDifficulty == Difficulty.FableNightmare) mult *= 0.78f;
        return Math.Clamp(time * mult, 1.8f, 4.2f);
    }

    static void UpdateCursorLock()
    {
        bool shouldLock = state == GameState.Playing && lockCursorInGame;
        if (shouldLock == cursorLocked) return;
        cursorLocked = shouldLock;
        if (shouldLock) Raylib.DisableCursor();
        else Raylib.EnableCursor();
    }

    static void TryStartReload()
    {
        if (reloadTimer > 0f || pendingBurstShots > 0) return;
        ref readonly Gun g = ref Guns[equippedGun];
        if (ammoInMag >= GetMagazineSize(in g)) return;
        reloadTimer = GetReloadTime(in g);
    }

    static void UpdateReload(float dt)
    {
        if (reloadTimer <= 0f) return;
        reloadTimer -= dt;
        if (reloadTimer > 0f) return;
        reloadTimer = 0f;
        ammoInMag = GetMagazineSize(in Guns[equippedGun]);
    }

    static bool TryConsumeAmmoForShot()
    {
        if (reloadTimer > 0f) return false;
        if (ammoInMag <= 0)
        {
            TryStartReload();
            return false;
        }
        ammoInMag--;
        if (ammoInMag <= 0) TryStartReload();
        return true;
    }

    static void UpdateWeapon(float dt)
    {
        UpdateWeaponAim(dt);
        UpdateReload(dt);

        if (state == GameState.Playing && Raylib.IsMouseButtonPressed(MouseButton.Right))
        {
            TryStartReload();
        }

        if (pendingBurstShots > 0)
        {
            pendingBurstTimer -= dt;
            if (pendingBurstTimer <= 0f)
            {
                ref readonly Gun burstGun = ref Guns[pendingBurstGun];
                FireWeaponShot(in burstGun, pendingBurstTarget, pendingBurstShots == 1);
                pendingBurstShots--;
                pendingBurstTimer = burstGun.BurstGap > 0f ? burstGun.BurstGap : 0.07f;
                if (pendingBurstShots <= 0)
                {
                    fireTimer = EffFireCooldown(in burstGun);
                }
            }
            return;
        }

        fireTimer -= dt;
        if (fireTimer < -0.5f) fireTimer = -0.5f;

        if (reloadTimer > 0f) return;

        bool wantFire = (state == GameState.Playing && Raylib.IsMouseButtonDown(MouseButton.Left))
            || autoFire
            || Raylib.IsKeyDown(fireKey)
            || (aiPilotEnabled && state == GameState.Playing);
        if (!wantFire || fireTimer > 0f)
        {
            return;
        }

        if (ammoInMag <= 0)
        {
            TryStartReload();
            return;
        }

        int target = aiPilotEnabled && aiFocusEnemyIndex >= 0 ? aiFocusEnemyIndex : NearestEnemyIndex(playerPos);
        if (target < 0 && Guns[equippedGun].Style != GunFireStyle.RingPulse && Guns[equippedGun].Style != GunFireStyle.CrossBurst)
        {
            return;
        }

        Vector2 aim = target >= 0 ? enemies[target].Position : playerPos + lastMoveDirection * 120f;
        ref readonly Gun g = ref Guns[equippedGun];
        FireWeapon(in g, aim, equippedGun);
    }

    static void FireWeapon(in Gun g, Vector2 targetPos, int gunIndex)
    {
        if (!TryConsumeAmmoForShot()) return;

        SnapWeaponAim(targetPos);

        switch (g.Style)
        {
            case GunFireStyle.Burst:
            case GunFireStyle.Volley:
                pendingBurstShots = Math.Max(1, g.BurstCount);
                pendingBurstGun = gunIndex;
                pendingBurstTarget = targetPos;
                pendingBurstTimer = 0f;
                FireWeaponShot(in g, targetPos, pendingBurstShots == 1);
                pendingBurstShots--;
                pendingBurstTimer = g.BurstGap > 0f ? g.BurstGap : 0.07f;
                break;
            case GunFireStyle.Laser:
                FireLaserBeam(in g, targetPos);
                fireTimer = EffFireCooldown(in g);
                break;
            default:
                FireWeaponPattern(in g, targetPos);
                fireTimer = EffFireCooldown(in g);
                break;
        }
    }

    static void FireWeaponPattern(in Gun g, Vector2 targetPos)
    {
        switch (g.Style)
        {
            case GunFireStyle.ArcFan:
            {
                int shots = Math.Max(3, g.BurstCount);
                float spread = 70f * MathF.PI / 180f;
                Vector2 dir = SafeNormalize(targetPos - playerPos);
                float baseAngle = MathF.Atan2(dir.Y, dir.X);
                for (int i = 0; i < shots; i++)
                {
                    float frac = shots == 1 ? 0.5f : i / (float)(shots - 1);
                    float angle = baseAngle + (frac - 0.5f) * spread;
                    SpawnProjectile(in g, angle, EffDamage(in g), EffPierce(in g), g.Speed, 4f, g.Homing, GunFireStyle.ArcFan);
                }
                break;
            }
            case GunFireStyle.Buckshot:
            {
                int pellets = Math.Max(4, EffCount(in g));
                float spread = (g.Spread + upgradeLevels[UpSplit] * 4f + 18f) * MathF.PI / 180f;
                float baseAngle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                for (int i = 0; i < pellets; i++)
                {
                    float frac = pellets == 1 ? 0.5f : i / (float)(pellets - 1);
                    float angle = baseAngle + (frac - 0.5f) * spread;
                    SpawnProjectile(in g, angle, EffDamage(in g) * 0.75f, 1, g.Speed * 0.82f, 3.2f, 0f, GunFireStyle.Buckshot);
                }
                break;
            }
            case GunFireStyle.Homing:
                FireWeaponShot(in g, targetPos, true);
                break;
            case GunFireStyle.Lance:
            {
                float angle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                SpawnProjectile(in g, angle, EffDamage(in g) * 1.35f, EffPierce(in g) + 2, g.Speed * 1.25f, 6.5f, 0f, GunFireStyle.Lance);
                break;
            }
            case GunFireStyle.Repeater:
            {
                float angle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                SpawnProjectile(in g, angle, EffDamage(in g) * 0.85f, EffPierce(in g), g.Speed * 1.1f, 3f, 0f, GunFireStyle.Repeater);
                break;
            }
            case GunFireStyle.Sniper:
            {
                float angle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                SpawnProjectile(in g, angle, EffDamage(in g) * 2.4f, Math.Max(2, EffPierce(in g)), g.Speed * 1.55f, 3.5f, 0f, GunFireStyle.Sniper);
                break;
            }
            case GunFireStyle.RingPulse:
            {
                int rays = 8;
                for (int i = 0; i < rays; i++)
                {
                    float angle = i * MathF.PI * 2f / rays;
                    SpawnProjectile(in g, angle, EffDamage(in g) * 0.9f, 1, g.Speed * 0.95f, 4f, 0f, GunFireStyle.RingPulse);
                }
                break;
            }
            case GunFireStyle.CrossBurst:
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * MathF.PI * 0.5f;
                    SpawnProjectile(in g, angle, EffDamage(in g), EffPierce(in g), g.Speed, 4.2f, 0f, GunFireStyle.CrossBurst);
                }
                break;
            }
            case GunFireStyle.Mortar:
            {
                float angle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                SpawnProjectile(in g, angle, EffDamage(in g) * 1.8f, 0, g.Speed * 0.55f, 8f, 0f, GunFireStyle.Mortar);
                break;
            }
            case GunFireStyle.DriftOrb:
            {
                float angle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                SpawnProjectile(in g, angle, EffDamage(in g), EffPierce(in g), g.Speed * 0.65f, 5.5f, Math.Max(1.8f, g.Homing), GunFireStyle.DriftOrb);
                break;
            }
            case GunFireStyle.FlailArc:
            {
                float baseAngle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
                for (int i = -1; i <= 1; i++)
                {
                    float angle = baseAngle + i * 0.42f;
                    SpawnProjectile(in g, angle, EffDamage(in g) * 1.1f, EffPierce(in g), g.Speed * 0.9f, 5f, 0f, GunFireStyle.FlailArc, spinRate: 6f + i * 2f);
                }
                break;
            }
            default:
                FireWeaponShot(in g, targetPos, true);
                break;
        }

        Vector2 muzzleDir = SafeNormalize(targetPos - playerPos);
        Vector2 muzzlePos = GetPlayerMuzzlePos();
        SpawnWeaponFireFx(g.Style, muzzlePos, muzzleDir, g.Color, g.Name);
        AddTrauma(GunStyleTrauma(g.Style));
        KickWeaponRecoil(in g, 0.7f);
    }

    static void FireWeaponShot(in Gun g, Vector2 targetPos, bool finalBurstShot)
    {
        SnapWeaponAim(targetPos);
        float baseAngle = MathF.Atan2(targetPos.Y - playerPos.Y, targetPos.X - playerPos.X);
        float spread = (g.Spread + upgradeLevels[UpSplit] * 4f) * MathF.PI / 180f;
        int count = g.Style is GunFireStyle.Burst or GunFireStyle.Volley ? 1 : EffCount(in g);

        for (int i = 0; i < count; i++)
        {
            float frac = count == 1 ? 0.5f : i / (float)(count - 1);
            float angle = baseAngle + (frac - 0.5f) * spread;
            float homing = g.Style == GunFireStyle.Homing ? Math.Max(2.5f, g.Homing) : g.Homing;
            float size = g.Style == GunFireStyle.Homing ? 6f : g.Style == GunFireStyle.Repeater ? 3f : 4.5f;
            SpawnProjectile(in g, angle, EffDamage(in g), EffPierce(in g), g.Speed, size, homing, g.Style);
        }

        if (finalBurstShot)
        {
            Vector2 muzzleDir = new Vector2(MathF.Cos(baseAngle), MathF.Sin(baseAngle));
            Vector2 muzzlePos = GetPlayerMuzzlePos();
            SpawnWeaponFireFx(g.Style, muzzlePos, muzzleDir, g.Color, g.Name);
            AddTrauma(GunStyleTrauma(g.Style) * 0.65f);
        }

        KickWeaponRecoil(in g, finalBurstShot ? 1f : 0.42f);
    }

    static void SpawnProjectile(in Gun g, float angle, float damage, int pierce, float speed, float size,
        float homing, GunFireStyle style, float spinRate = 0f)
    {
        var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
        float life = style == GunFireStyle.Mortar ? 2.2f : style == GunFireStyle.DriftOrb ? 2.6f : 1.4f;
        projectiles.Add(new Projectile
        {
            Position = playerPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (PlayerRadius + 4f),
            Velocity = vel,
            Color = g.Color,
            Life = life,
            SpawnLife = life,
            Size = size,
            Damage = damage,
            Pierce = pierce,
            Homing = homing,
            Style = style,
            SpinRate = spinRate,
        });
        SpawnProjectileBirthFx(style, playerPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (PlayerRadius + 4f), vel, g.Color);
    }

    static void FireLaserBeam(in Gun g, Vector2 targetPos)
    {
        SnapWeaponAim(targetPos);
        Vector2 dir = SafeNormalize(targetPos - playerPos);
        Vector2 start = GetPlayerMuzzlePos();
        Vector2 end = start + dir * 920f;
        float damage = EffDamage(in g) * 1.6f;
        DamageBeam(start, end, 7f, damage, g.Color);

        projectiles.Add(new Projectile
        {
            Position = start,
            Velocity = Vector2.Zero,
            Color = g.Color,
            Life = 0.28f,
            SpawnLife = 0.28f,
            Size = 7f,
            Damage = 0f,
            Pierce = 0,
            Homing = 0f,
            Style = GunFireStyle.Laser,
            BeamEnd = end,
            BeamWidth = 9f,
        });

        SpawnWeaponFireFx(GunFireStyle.Laser, start, dir, g.Color, g.Name);
        SpawnLaserBeamFx(start, end, g.Color);
        AddTrauma(0.32f);
        TriggerImpact(ImpactTier.Medium, Lighten(g.Color, 0.45f));
        AddFlash(Lighten(g.Color, 0.3f), 0.14f);
        KickWeaponRecoil(in g, 0.85f);
    }

    static void DamageBeam(Vector2 start, Vector2 end, float width, float damage, Color color)
    {
        Vector2 seg = end - start;
        float lenSq = seg.LengthSquared();
        if (lenSq < 1f) return;

        int beamHits = 0;
        for (int e = 0; e < enemies.Count; e++)
        {
            Enemy en = enemies[e];
            if (en.Dead || en.Spawn < 0.5f) continue;

            float t = Math.Clamp(Vector2.Dot(en.Position - start, seg) / lenSq, 0f, 1f);
            Vector2 closest = start + seg * t;
            float dist = Vector2.Distance(en.Position, closest);
            if (dist > width + en.Radius) continue;

            en.Hp -= damage;
            en.Hit = 0.12f;
            if (en.Hp <= 0f) en.Dead = true;
            enemies[e] = en;
            SpawnHitSpark(closest, color);
            beamHits++;
        }

        if (beamHits >= 3)
        {
            TriggerImpact(ImpactTier.Medium, color);
        }
        else if (beamHits > 0)
        {
            TriggerImpact(ImpactTier.Light, color);
        }
    }

    static void UpdateProjectiles(float dt)
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            Projectile p = projectiles[i];
            p.Life -= dt;

            if (p.Style == GunFireStyle.Laser)
            {
                if (p.Life <= 0f) projectiles.RemoveAt(i);
                else projectiles[i] = p;
                continue;
            }

            if (p.SpinRate != 0f)
            {
                float ang = MathF.Atan2(p.Velocity.Y, p.Velocity.X) + p.SpinRate * dt;
                float spd = p.Velocity.Length();
                p.Velocity = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd;
            }

            if (p.Homing > 0f)
            {
                int t = NearestEnemyIndex(p.Position);
                if (t >= 0)
                {
                    Vector2 desired = Vector2.Normalize(enemies[t].Position - p.Position) * p.Velocity.Length();
                    p.Velocity = Vector2.Lerp(p.Velocity, desired, 1f - MathF.Exp(-p.Homing * dt));
                }
            }

            if (p.Style == GunFireStyle.DriftOrb)
            {
                p.Size = Math.Min(10f, p.Size + dt * 2.2f);
            }

            p.Position += p.Velocity * dt;

            if (Rng.NextSingle() < ProjectileTrailChance(p.Style) && p.Style != GunFireStyle.Mortar)
            {
                SpawnProjectileTrailFx(p);
            }

            bool dead = p.Life <= 0f
                || p.Position.X < -20f || p.Position.X > WindowWidth + 20f
                || p.Position.Y < -20f || p.Position.Y > WindowHeight + 20f;

            if (!dead)
            {
                for (int e = 0; e < enemies.Count; e++)
                {
                    Enemy en = enemies[e];
                    if (en.Dead || en.Spawn < 0.5f) continue;

                    float rr = p.Size + en.Radius;
                    if (Vector2.DistanceSquared(p.Position, en.Position) > rr * rr) continue;

                    float dmg = p.Damage;
                    if (p.Style == GunFireStyle.Mortar)
                    {
                        dmg *= 1.35f;
                        SpawnExplosion(p.Position, p.Color, 42);
                    }

                    en.Hp -= dmg;
                    en.Hit = 0.12f;
                    enemies[e] = en;
                    SpawnProjectileHitFx(p.Style, p.Position, p.Velocity, p.Color, dmg);

                    if (!en.Dead)
                    {
                        RegisterWeaponImpact(p.Position, p.Color, dmg, p.Style);
                    }

                    if (en.Hp <= 0f)
                    {
                        en.Dead = true;
                        enemies[e] = en;
                    }

                    p.Pierce--;
                    if (p.Pierce < 0 || p.Style == GunFireStyle.Mortar)
                    {
                        dead = true;
                    }
                    break;
                }
            }

            if (dead) projectiles.RemoveAt(i);
            else projectiles[i] = p;
        }
    }

    static int NearestEnemyIndex(Vector2 from)
    {
        int best = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (e.Dead || e.Spawn < 0.5f) continue;
            float d = Vector2.DistanceSquared(from, e.Position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

}
