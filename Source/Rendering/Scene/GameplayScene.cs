partial class Program
{
    static void DrawPlayScene()
    {
        if (!gfxReady)
        {
            DrawPlaySceneFallback();
            return;
        }

        Raylib.BeginTextureMode(sceneTarget);
        Raylib.ClearBackground(ForestShadow);

        Raylib.BeginMode2D(ComputeSceneCamera());
        DrawSiegeBackdrop((float)Raylib.GetTime());
        DrawArenaBase();
        DrawArenaAtmosphere();
        DrawTiles();
        DrawSafeZoneRushWorldBand();
        DrawTileContactOcclusion();
        DrawParalyzeBurst();
        DrawWindStepAura();
        DrawBannerOfStillness();
        DrawOathRescueFlash();
        DrawOathReinforcedTile();
        DrawVerdictWave();
        DrawVerdictTelegraph();
        DrawVerdictHaltOverlay();
        DrawFloorEventTileWarnings();
        DrawEnemyAttackTelegraphs();
        DrawTrails();
        DrawParticles();
        DrawEntityShadows();
        DrawEnemyShaderUnderglow();
        DrawEnemies();
        DrawProjectiles();
        DrawPlayer();
        DrawFloorSpecular(playerPos, PlayerRadius, BodyBright(), 0.1f + adrenaline * 0.06f);
        DrawFloaters();
        DrawDynamicLighting();
        DrawBloomOverlay();
        DrawEntityRimHighlights();
        Raylib.EndMode2D();

        DrawVolumetricGodRays();
        Raylib.EndTextureMode();

        DrawSceneComposite();
        DrawCinematicPostProcess();
        DrawFlash();
        DrawImpactFlash();
        DrawMarkedStrikeLasers();
        DrawEventVfx();
        DrawTopHud();
        DrawLevelUpBanner();
        DrawComboMeter();
        DrawWaveBanner();
        DrawFloorEventHud();
        DrawFloorEventOverlay();
        DrawFloorEventWarningBorder();
        DrawBossHud();
        DrawAbilityHud();
        DrawWeaponHud();
        DrawPlayControlLegend();
        DrawAiPilotBanner();
    }

    static void DrawPlaySceneFallback()
    {
        Raylib.BeginMode2D(ComputeCamera());
        DrawSiegeBackdrop((float)Raylib.GetTime());
        DrawArenaBase();
        DrawArenaAtmosphere();
        DrawTiles();
        DrawSafeZoneRushWorldBand();
        DrawParalyzeBurst();
        DrawWindStepAura();
        DrawBannerOfStillness();
        DrawOathRescueFlash();
        DrawOathReinforcedTile();
        DrawVerdictWave();
        DrawVerdictTelegraph();
        DrawVerdictHaltOverlay();
        DrawFloorEventTileWarnings();
        DrawEnemyAttackTelegraphs();
        DrawTrails();
        DrawParticles();
        DrawEntityShadows();
        DrawEnemyShaderUnderglow();
        DrawEnemies();
        DrawProjectiles();
        DrawPlayer();
        DrawFloaters();
        Raylib.EndMode2D();

        DrawFlash();
        DrawImpactFlash();
        DrawMarkedStrikeLasers();
        DrawEventVfx();
        DrawTopHud();
        DrawLevelUpBanner();
        DrawComboMeter();
        DrawWaveBanner();
        DrawFloorEventHud();
        DrawFloorEventOverlay();
        DrawFloorEventWarningBorder();
        DrawBossHud();
        DrawAbilityHud();
        DrawWeaponHud();
        DrawAiPilotBanner();
    }

    static void DrawEnemyShaderUnderglow()
    {
        if (!UhdShadersActive) return;

        float time = (float)Raylib.GetTime();
        Raylib.BeginBlendMode(BlendMode.Additive);
        foreach (Enemy e in enemies)
        {
            if (e.Spawn < 0.3f) continue;
            Color c = Lighten(GetEnemyColor(e.Type), 0.52f);
            float r = e.Radius * EaseOutBack(e.Spawn);
            float pulse = MathF.Sin(time * 4.5f + e.Wobble) * 0.5f + 0.5f;
            DrawGlowFast(e.Position, r * 2.4f, c, 0.06f + pulse * 0.04f);
        }
        Raylib.EndBlendMode();
    }

    static void DrawAiPilotBanner()
    {
        if (!aiPilotEnabled || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        int cx = WindowWidth / 2;
        float pulse = MathF.Sin(time * 4f) * 0.5f + 0.5f;
        float prominent = aiPilotBannerTimer > 0f ? MathF.Min(1f, aiPilotBannerTimer / 2f) : 0f;
        float alpha = 0.62f + pulse * 0.12f + prominent * 0.2f;

        const string title = "HUMAN SURVIVAL PILOT";
        const string warn = "SURVIVAL PRIORITY";
        int titleSize = prominent > 0.2f ? 15 : 13;
        int warnSize = 11;
        int titleW = Raylib.MeasureText(title, titleSize);
        int warnW = Raylib.MeasureText(warn, warnSize);
        float panelW = MathF.Max(titleW, warnW) + 36f;
        float panelH = prominent > 0.2f ? 46f : 34f;
        // Keep pilot status clear of the bottom weapon and ability HUD.
        var panel = new Rectangle(cx - panelW / 2f, 106f, panelW, panelH);

        DrawRichPanel(panel, WithAlpha(UiPanel, alpha * 0.92f), WithAlpha(new Color(196, 168, 108, 255), alpha), 0.22f, accentStripe: true);
        ShadowTextCentered(title, cx, (int)(panel.Y + 8f), titleSize, new Color(196, 168, 108, 255), alpha);
        if (prominent > 0.2f)
        {
            ShadowTextCentered(warn, cx, (int)(panel.Y + 26f), warnSize, WithAlpha(Color.White, 0.72f), alpha);
        }
    }

    static Color SodBase(int x, int y)
    {
        float h = Hash(x * 31 + y * 17);
        float h2 = Hash(x * 47 + y * 71);
        Color baseCol = LerpColor(SodDark, SodMid, h * 0.5f + 0.25f);
        Color warmed = LerpColor(baseCol, new Color(34, 32, 28, 255), h2 * 0.14f);
        Color grain = LerpColor(warmed, Bark, Hash(x * 13 + y * 29) * 0.09f);
        float diag = ((x + y) & 1) == 0 ? 0.02f : -0.02f;
        return Darken(grain, 0.08f + diag);
    }

    static void DrawTileSodGrain(Rectangle rect, int x, int y, Color sod)
    {
        int seed = x * 16127 + y * 31337;
        for (int i = 0; i < 2; i++)
        {
            if (Hash(seed + i * 7) < 0.32f) continue;
            float fx = rect.X + 4f + Hash(seed + i) * (rect.Width - 8f);
            float fy = rect.Y + 4f + Hash(seed + i + 3) * (rect.Height - 8f);
            float w = 3f + Hash(seed + i + 5) * 5f;
            float h = 1.5f + Hash(seed + i + 9) * 2f;
            Color g = Hash(seed + i + 11) > 0.5f ? Lighten(sod, 0.07f) : Darken(sod, 0.09f);
            Raylib.DrawRectangleRounded(new Rectangle(fx, fy, w, h), 0.5f, 2, WithAlpha(g, 0.38f));
        }
    }

    static void DrawArenaBase()
    {
        const float pad = 4f;
        float time = (float)Raylib.GetTime();
        var arena = new Rectangle(pad, pad, WindowWidth - pad * 2f, WindowHeight - pad * 2f);

        DrawArenaCourtyardCobbles(arena, time);

        Raylib.DrawRectangleRoundedLines(arena, 0.04f, 8, WithAlpha(GameplayCastlePalette.StoneLight, 0.55f));

        var inner = new Rectangle(pad + 5f, pad + 5f, arena.Width - 10f, arena.Height - 10f);
        Raylib.DrawRectangleRoundedLines(inner, 0.03f, 8, WithAlpha(GameplayCastlePalette.StoneHi, 0.22f));
        DrawGradientWash(inner,
            WithAlpha(GameplayCastlePalette.TorchWarm, 0.03f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.5f, 0.12f), 2.4f);

        MenuCastlePalette p = GameplayCastlePalette;
        Raylib.DrawCircleV(new Vector2(pad + 14f, pad + 14f), 5f, WithAlpha(p.Moss, 0.35f));
        Raylib.DrawCircleV(new Vector2(arena.Width + pad - 14f, pad + 14f), 4f, WithAlpha(p.Lichen, 0.28f));
        Raylib.DrawCircleV(new Vector2(pad + 14f, arena.Height + pad - 14f), 4f, WithAlpha(p.Moss, 0.28f));
        Raylib.DrawCircleV(new Vector2(arena.Width + pad - 14f, arena.Height + pad - 14f), 5f, WithAlpha(p.Lichen, 0.32f));
    }

    static void DrawTiles()
    {
        const float seam = 0.5f;
        float time = (float)Raylib.GetTime();

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Tile tile = tiles[x, y];
                float cx = x * TileSize + TileSize / 2f;
                float cy = y * TileSize + TileSize / 2f;
                float ox = x * TileSize + seam;
                float oy = y * TileSize + seam;
                float size = TileSize - seam * 2f;

                if (tile.State == 2)
                {
                    float e = tile.Collapse;
                    bool mergeL = IsCollapsedVoid(x - 1, y);
                    bool mergeR = IsCollapsedVoid(x + 1, y);
                    bool mergeU = IsCollapsedVoid(x, y - 1);
                    bool mergeD = IsCollapsedVoid(x, y + 1);

                    float left = x * TileSize + (mergeL ? 0f : seam);
                    float top = y * TileSize + (mergeU ? 0f : seam);
                    float right = (x + 1) * TileSize - (mergeR ? 0f : seam);
                    float bottom = (y + 1) * TileSize - (mergeD ? 0f : seam);
                    float w = right - left;
                    float h = bottom - top;
                    var pit = new Rectangle(left, top, w, h);

                    DrawVaultPit(left, top, w, h, cx, cy, x, y, time);

                    if (!mergeU && w > 4f)
                    {
                        Raylib.DrawLineEx(new Vector2(left + 2f, top + 2f), new Vector2(right - 2f, top + 2f), 1f,
                            WithAlpha(Lighten(Bark, 0.15f), 0.22f));
                    }

                    if (!mergeL)
                        Raylib.DrawLineEx(new Vector2(left, top), new Vector2(left, bottom), 1.2f, WithAlpha(Darken(Bark, 0.15f), 0.75f));
                    if (!mergeR)
                        Raylib.DrawLineEx(new Vector2(right, top), new Vector2(right, bottom), 1.2f, WithAlpha(Darken(Bark, 0.15f), 0.75f));
                    if (!mergeU)
                        Raylib.DrawLineEx(new Vector2(left, top), new Vector2(right, top), 1.2f, WithAlpha(Darken(Bark, 0.15f), 0.75f));
                    if (!mergeD)
                        Raylib.DrawLineEx(new Vector2(left, bottom), new Vector2(right, bottom), 1.2f, WithAlpha(Darken(Bark, 0.15f), 0.75f));

                    if (e < 1f)
                    {
                        int rings = 4;
                        for (int rIdx = 0; rIdx < rings; rIdx++)
                        {
                            float ringPhase = Math.Clamp(e * rings - rIdx, 0f, 1f);
                            if (ringPhase >= 1f)
                            {
                                continue;
                            }

                            float shrink = (rIdx / (float)rings) + ringPhase * (1f / rings);
                            float rsize = Math.Min(w, h) * (1f - shrink);
                            float drop = ringPhase * 10f + rIdx * 1.5f;
                            var rr = new Rectangle(cx, cy + drop, rsize, rsize);
                            Color shard = WithAlpha(
                                LerpColor(TileFill(MaxDurability * 0.3f), EarthPit, shrink + ringPhase * 0.4f),
                                (1f - ringPhase) * (1f - shrink * 0.4f));
                            Raylib.DrawRectanglePro(rr, new Vector2(rsize / 2f, rsize / 2f),
                                (rIdx + ringPhase) * 40f, shard);
                        }

                        if (e < 0.5f)
                        {
                            float dust = MathF.Sin(time * 20f + x + y) * 0.5f + 0.5f;
                            Raylib.DrawCircleV(new Vector2(cx, cy - 4f), 1.5f + dust * 1.5f,
                                WithAlpha(LeafGold, (0.5f - e) * 0.5f));
                        }
                    }

                    if (tile.RegrowTimer > 0f && tile.Collapse >= 1f)
                    {
                        float regrowPulse = MathF.Sin(time * 4f + x + y) * 0.5f + 0.5f;
                        if (!mergeL)
                            Raylib.DrawLineEx(new Vector2(left, top), new Vector2(left, bottom), 1.5f,
                                WithAlpha(UiAccent, regrowPulse * (1f - tile.RegrowTimer / TileRegrowTime) * 0.75f));
                        if (!mergeR)
                            Raylib.DrawLineEx(new Vector2(right, top), new Vector2(right, bottom), 1.5f,
                                WithAlpha(UiAccent, regrowPulse * (1f - tile.RegrowTimer / TileRegrowTime) * 0.75f));
                    }

                    continue;
                }

                Color fill = StoneTileFill(tile.Durability);
                float ratio = tile.Durability / MaxDurability;
                TileHealthVisual healthBand = GetTileHealthVisual(ratio);
                Color stoneBase = StoneBase(x, y);

                if (tile.State == 1)
                {
                    float blightPulse = MathF.Sin(time * 2.4f + (x * 0.6f + y * 0.4f)) * 0.5f + 0.5f;
                    Color rot = LerpColor(fill, ActiveTheme.BlightRot, 0.55f);
                    Color sick = LerpColor(rot, ActiveTheme.BlightSick, blightPulse);
                    fill = sick;
                    stoneBase = LerpColor(stoneBase, Darken(rot, 0.3f), 0.6f);
                }
                else if (healthBand is TileHealthVisual.Worn or TileHealthVisual.Cracked)
                {
                    stoneBase = LerpColor(stoneBase, Darken(fill, 0.12f), 0.25f);
                }
                else if (healthBand is TileHealthVisual.Fractured or TileHealthVisual.Critical)
                {
                    stoneBase = LerpColor(stoneBase, Darken(fill, 0.22f), 0.45f);
                }

                var baseRect = new Rectangle(ox, oy, size, size);
                var bodyRect = new Rectangle(ox + 1.5f, oy + 1.5f, size - 3f, size - 3f);

                Raylib.DrawRectangleRounded(baseRect, 0.1f, 6, stoneBase);
                DrawTileStonePattern(baseRect, x, y, fill, healthBand);
                Raylib.DrawRectangleRounded(bodyRect, 0.12f, 6, Darken(fill, 0.26f));
                Raylib.DrawRectangleRounded(
                    new Rectangle(bodyRect.X + 1f, bodyRect.Y + 1f, bodyRect.Width - 2f, bodyRect.Height - 2f),
                    0.1f, 6, Darken(fill, 0.08f));

                Raylib.DrawRectangleRoundedLines(
                    new Rectangle(ox + 2f, oy + 2f, size - 4f, size - 4f), 0.12f, 6,
                    WithAlpha(TileSeam, 0.72f));

                float edgeHi = 0.12f;
                float edgeLo = 0.58f;
                float sheenA = 0.02f;
                float sheenLift = 0.08f;

                Raylib.DrawLineEx(
                    new Vector2(ox + 3f, oy + size - 3f),
                    new Vector2(ox + 3f, oy + 3f),
                    1.2f, WithAlpha(Lighten(fill, edgeHi), edgeHi));
                Raylib.DrawLineEx(
                    new Vector2(ox + 3f, oy + 3f),
                    new Vector2(ox + size - 3f, oy + 3f),
                    1.2f, WithAlpha(Lighten(fill, edgeHi), edgeHi));
                Raylib.DrawLineEx(
                    new Vector2(ox + size - 3f, oy + size - 3f),
                    new Vector2(ox + size - 3f, oy + 3f),
                    1f, WithAlpha(Darken(fill, edgeLo), 0.4f));
                Raylib.DrawLineEx(
                    new Vector2(ox + 3f, oy + size - 3f),
                    new Vector2(ox + size - 3f, oy + size - 3f),
                    1f, WithAlpha(Darken(fill, edgeLo), 0.4f));

                var sheen = new Rectangle(ox + 4f, oy + 3f, size - 8f, size * 0.38f);
                float sheenBoost = healthBand == TileHealthVisual.Pristine ? 0.08f
                    : healthBand == TileHealthVisual.Sturdy ? 0.04f : 0f;
                Raylib.DrawRectangleRounded(sheen, 0.6f, 4, WithAlpha(Lighten(fill, sheenLift), sheenA + sheenBoost));

                DrawTileHealthSurface(x, y, cx, cy, bodyRect, fill, ratio, healthBand, tile, time);

                if (tile.State == 1)
                {
                    float vein = MathF.Sin(time * 1.8f + x * 0.9f + y * 0.7f) * 0.5f + 0.5f;
                    Raylib.DrawRectangleRounded(bodyRect, 0.12f, 6, WithAlpha(ActiveTheme.BlightSick, 0.08f + vein * 0.1f));
                    Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(Darken(ActiveTheme.BlightRot, 0.1f), 0.28f + vein * 0.2f));
                }

                if (floorRotTimer > 0f && IsTileInMossRotZone(x))
                {
                    float rotPulse = MathF.Sin(time * 3f + x * 0.4f + y * 0.3f) * 0.5f + 0.5f;
                    Color mossRot = new Color(54, 68, 58, 255);
                    Raylib.DrawRectangleRounded(bodyRect, 0.12f, 6, WithAlpha(mossRot, 0.2f + rotPulse * 0.16f));
                }

                if (tile.UntouchedTimer >= TileIdleRegenDelay && tile.Durability < MaxDurability)
                {
                    float regenT = Math.Clamp((tile.UntouchedTimer - TileIdleRegenDelay) / 4f, 0f, 1f);
                    float regenPulse = MathF.Sin(time * 3.5f + x + y) * 0.5f + 0.5f;
                    Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6,
                        WithAlpha(UiAccent, (0.1f + regenPulse * 0.16f) * regenT));
                }

                if (x < GridSize - 1 && !IsCollapsedVoid(x + 1, y))
                {
                    Raylib.DrawRectangle((int)(ox + size), (int)oy, 1, (int)size, TileSeam);
                    Raylib.DrawRectangle((int)(ox + size), (int)oy, 1, (int)(size * 0.35f), WithAlpha(Lighten(TileSeam, 0.15f), 0.25f));
                }
                if (y < GridSize - 1 && !IsCollapsedVoid(x, y + 1))
                {
                    Raylib.DrawRectangle((int)ox, (int)(oy + size), (int)size, 1, TileSeam);
                    Raylib.DrawRectangle((int)ox, (int)(oy + size), (int)(size * 0.35f), 1, WithAlpha(Lighten(TileSeam, 0.15f), 0.25f));
                }

                if (tile.EventMarked && tile.State != 2)
                {
                    float flash = MathF.Sin(time * 18f + x * 1.7f + y) * 0.5f + 0.5f;
                    float warn = activeEvent != FloorEventType.None && eventCountdown > 0f ? 1.15f : 1f;
                    float urgency = activeEvent != FloorEventType.None && eventStartCountdown > 0.01f
                        ? Math.Clamp(1f - eventCountdown / eventStartCountdown, 0.2f, 1f) : 0.65f;
                    DrawMasonryStressMarks(bodyRect, x, y, time, urgency * warn);
                    Raylib.DrawRectangleRounded(bodyRect, 0.12f, 6, WithAlpha(Danger, (0.28f + flash * 0.38f) * warn));
                    Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(new Color(255, 80, 60, 255), (0.45f + flash * 0.4f) * warn));
                    DrawPulseFrame(bodyRect, Danger, 0.12f, 14f, 0.18f + flash * 0.35f);

                    float cx2 = ox + size / 2f;
                    float cy2 = oy + size / 2f;
                    float xSize = size * (0.18f + flash * 0.06f);
                    Raylib.DrawLineEx(new Vector2(cx2 - xSize, cy2 - xSize), new Vector2(cx2 + xSize, cy2 + xSize), 2f, WithAlpha(Color.White, 0.35f + flash * 0.3f));
                    Raylib.DrawLineEx(new Vector2(cx2 + xSize, cy2 - xSize), new Vector2(cx2 - xSize, cy2 + xSize), 2f, WithAlpha(Color.White, 0.35f + flash * 0.3f));
                }
            }
        }
    }

    static void DrawCracks(float cx, float cy, int x, int y, float severity, int crackCount = 3)
    {
        int seed = x * 73856093 ^ y * 19349663;
        Color crack = WithAlpha(ForestShadow, severity * 0.55f);
        int count = Math.Clamp(crackCount, 1, 8);
        for (int k = 0; k < count; k++)
        {
            float a1 = Hash(seed + k) * MathF.PI;
            float a2 = a1 + Hash(seed + k * 7 + 99) * 0.9f;
            float reach = TileSize * (0.28f + Hash(seed + k * 3) * 0.18f) * (0.65f + severity * 0.55f);
            var p1 = new Vector2(cx + MathF.Cos(a1) * 3f, cy + MathF.Sin(a1) * 3f);
            var p2 = new Vector2(cx + MathF.Cos(a2) * reach, cy + MathF.Sin(a2) * reach);
            Raylib.DrawLineEx(p1, p2, 1.4f + severity * 0.5f, crack);

            if (severity > 0.45f)
            {
                var mid = Vector2.Lerp(p1, p2, 0.45f);
                float ba = a2 + Hash(seed + k * 11) * 1.4f;
                var branch = new Vector2(mid.X + MathF.Cos(ba) * reach * 0.35f, mid.Y + MathF.Sin(ba) * reach * 0.35f);
                Raylib.DrawLineEx(mid, branch, 1.1f, WithAlpha(crack, 0.85f));
            }
        }
    }

    static void DrawTrails()
    {
        Color body = BodyColor();
        Color bright = BodyBright();
        foreach (DashTrail t in trails)
        {
            float a = t.Alpha * t.Alpha;
            float rad = t.Radius * (0.35f + a);
            DrawGlow(t.Position, rad * 1.6f, body, a * 0.14f);
            Raylib.DrawCircleV(t.Position, rad, WithAlpha(body, a * 0.55f));
            Raylib.DrawCircleV(t.Position, rad * 0.5f, WithAlpha(bright, a * 0.5f));
        }
    }

    static void DrawParticles()
    {
        foreach (Particle p in particles)
        {
            if (p.Glow)
            {
                DrawGlow(p.Position, p.Size * 2.4f, p.Color, p.Alpha * 0.11f);
                Raylib.DrawCircleV(p.Position, p.Size * 0.5f, WithAlpha(Lighten(p.Color, 0.4f), p.Alpha));
            }
            else
            {
                var origin = new Vector2(p.Size / 2f, p.Size / 2f);
                Raylib.DrawRectanglePro(
                    new Rectangle(p.Position.X + 1.5f, p.Position.Y + 1.5f, p.Size, p.Size),
                    origin, p.Rotation, WithAlpha(ForestShadow, p.Alpha * 0.3f));
                Raylib.DrawRectanglePro(
                    new Rectangle(p.Position.X, p.Position.Y, p.Size, p.Size),
                    origin, p.Rotation, WithAlpha(p.Color, p.Alpha));
            }
        }
    }

    static void DrawProjectiles()
    {
        float time = (float)Raylib.GetTime();
        foreach (Projectile p in projectiles)
        {
            float lifeT = p.SpawnLife > 0.01f ? Math.Clamp(p.Life / p.SpawnLife, 0f, 1f) : 1f;
            float rot = MathF.Atan2(p.Velocity.Y, p.Velocity.X) * 180f / MathF.PI;
            Color hot = Lighten(p.Color, 0.45f);
            Color core = Lighten(p.Color, 0.7f);

            switch (p.Style)
            {
                case GunFireStyle.Laser:
                {
                    float alpha = Math.Clamp(p.Life / 0.28f, 0f, 1f);
                    DrawGlow(p.Position, p.BeamWidth * 4f, p.Color, 0.22f * alpha);
                    DrawGlow(p.BeamEnd, p.BeamWidth * 2.4f, hot, 0.14f * alpha);
                    Raylib.DrawLineEx(p.Position, p.BeamEnd, p.BeamWidth * 1.8f, WithAlpha(p.Color, 0.22f * alpha));
                    Raylib.DrawLineEx(p.Position, p.BeamEnd, p.BeamWidth, WithAlpha(p.Color, 0.42f * alpha));
                    Raylib.DrawLineEx(p.Position, p.BeamEnd, p.BeamWidth * 0.42f, WithAlpha(core, 0.92f * alpha));
                    Raylib.DrawLineEx(p.Position, p.BeamEnd, p.BeamWidth * 0.15f, WithAlpha(Color.White, 0.85f * alpha));
                    break;
                }
                case GunFireStyle.Mortar:
                {
                    float pulse = MathF.Sin(time * 14f + p.Position.X) * 0.5f + 0.5f;
                    DrawGlow(p.Position, p.Size * 2.2f, p.Color, 0.18f + pulse * 0.08f);
                    Raylib.DrawCircleV(p.Position, p.Size * 1.15f, WithAlpha(Darken(p.Color, 0.2f), 0.35f));
                    Raylib.DrawCircleV(p.Position, p.Size, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.42f, hot);
                    Raylib.DrawCircleLinesV(p.Position, p.Size + 3f + pulse * 2f, WithAlpha(hot, 0.7f));
                    break;
                }
                case GunFireStyle.Homing:
                {
                    float orbit = time * 8f + p.Position.X * 0.02f;
                    DrawGlow(p.Position, p.Size * 2.4f, p.Color, 0.2f);
                    Raylib.DrawCircleV(p.Position, p.Size, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.4f, core);
                    for (int i = 0; i < 3; i++)
                    {
                        float a = orbit + i * 2.1f;
                        Vector2 mote = p.Position + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (p.Size + 4f);
                        Raylib.DrawCircleV(mote, 2f, WithAlpha(hot, 0.75f));
                    }
                    Raylib.DrawCircleLinesV(p.Position, p.Size + 5f, WithAlpha(hot, 0.45f));
                    break;
                }
                case GunFireStyle.DriftOrb:
                {
                    float pulse = MathF.Sin(time * 6f) * 0.5f + 0.5f;
                    DrawGlow(p.Position, p.Size * 2.6f, p.Color, 0.18f + pulse * 0.1f);
                    Raylib.DrawCircleV(p.Position, p.Size, WithAlpha(p.Color, 0.85f));
                    Raylib.DrawCircleV(p.Position, p.Size * 0.55f, hot);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.22f, WithAlpha(Color.White, 0.65f));
                    Raylib.DrawRing(p.Position, p.Size + 2f, p.Size + 5f + pulse * 3f, 0f, 360f, 24, WithAlpha(hot, 0.35f));
                    break;
                }
                case GunFireStyle.Lance:
                {
                    Vector2 tail = p.Position - SafeNormalize(p.Velocity) * p.Size * 4.5f;
                    Raylib.DrawLineEx(tail, p.Position + SafeNormalize(p.Velocity) * p.Size * 1.2f, p.Size * 0.9f, WithAlpha(p.Color, 0.35f));
                    DrawGlow(p.Position, p.Size * 1.9f, p.Color, 0.18f);
                    var lance = new Rectangle(p.Position.X, p.Position.Y, p.Size * 3.2f, p.Size * 0.65f);
                    Raylib.DrawRectanglePro(lance, new Vector2(p.Size * 1.6f, p.Size * 0.32f), rot, p.Color);
                    Raylib.DrawRectanglePro(lance, new Vector2(p.Size * 1.6f, p.Size * 0.32f), rot, WithAlpha(hot, 0.55f));
                    Raylib.DrawCircleV(p.Position + SafeNormalize(p.Velocity) * p.Size * 1.1f, p.Size * 0.28f, core);
                    break;
                }
                case GunFireStyle.Sniper:
                {
                    Vector2 tail = p.Position - SafeNormalize(p.Velocity) * 42f;
                    Raylib.DrawLineEx(tail, p.Position, 2f, WithAlpha(p.Color, 0.25f * lifeT));
                    Raylib.DrawLineEx(tail, p.Position, 1f, WithAlpha(hot, 0.75f * lifeT));
                    var slug = new Rectangle(p.Position.X, p.Position.Y, p.Size * 2.8f, p.Size * 0.45f);
                    Raylib.DrawRectanglePro(slug, new Vector2(p.Size * 1.4f, p.Size * 0.22f), rot, p.Color);
                    Raylib.DrawCircleV(p.Position + SafeNormalize(p.Velocity) * p.Size * 0.8f, 2.5f, WithAlpha(Color.White, 0.8f * lifeT));
                    break;
                }
                case GunFireStyle.Buckshot:
                {
                    DrawGlow(p.Position, p.Size * 1.3f, p.Color, 0.14f);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.85f, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.35f, hot);
                    break;
                }
                case GunFireStyle.RingPulse:
                {
                    float pulse = MathF.Sin(time * 18f) * 0.5f + 0.5f;
                    DrawGlow(p.Position, p.Size * 2f, p.Color, 0.16f + pulse * 0.08f);
                    Raylib.DrawCircleV(p.Position, p.Size, p.Color);
                    Raylib.DrawRing(p.Position, p.Size * 0.5f, p.Size * 0.9f, rot, 220f, 12, WithAlpha(hot, 0.65f));
                    break;
                }
                case GunFireStyle.CrossBurst:
                {
                    float arm = p.Size * 1.1f;
                    Vector2 c = p.Position;
                    Vector2 n = SafeNormalize(p.Velocity);
                    Vector2 perp = new Vector2(-n.Y, n.X);
                    Raylib.DrawLineEx(c - n * arm, c + n * arm, p.Size * 0.55f, WithAlpha(p.Color, 0.9f));
                    Raylib.DrawLineEx(c - perp * arm, c + perp * arm, p.Size * 0.55f, WithAlpha(p.Color, 0.9f));
                    DrawGlow(c, p.Size * 1.6f, hot, 0.16f);
                    Raylib.DrawCircleV(c, p.Size * 0.35f, core);
                    break;
                }
                case GunFireStyle.FlailArc:
                {
                    float spin = time * 18f + p.SpinRate * 4f;
                    DrawGlow(p.Position, p.Size * 1.8f, p.Color, 0.16f);
                    var link = new Rectangle(p.Position.X, p.Position.Y, p.Size * 1.8f, p.Size * 0.9f);
                    Raylib.DrawRectanglePro(link, new Vector2(p.Size * 0.9f, p.Size * 0.45f), spin, p.Color);
                    Raylib.DrawRectanglePro(link, new Vector2(p.Size * 0.9f, p.Size * 0.45f), spin + 90f, WithAlpha(hot, 0.5f));
                    Raylib.DrawCircleV(p.Position, p.Size * 0.4f, core);
                    break;
                }
                case GunFireStyle.ArcFan:
                {
                    DrawGlow(p.Position, p.Size * 1.6f, p.Color, 0.15f);
                    var wedge = new Rectangle(p.Position.X, p.Position.Y, p.Size * 1.9f, p.Size * 0.85f);
                    Raylib.DrawRectanglePro(wedge, new Vector2(p.Size * 0.95f, p.Size * 0.42f), rot, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.32f, hot);
                    break;
                }
                case GunFireStyle.Repeater:
                case GunFireStyle.Burst:
                case GunFireStyle.Volley:
                {
                    Vector2 tail = p.Position - SafeNormalize(p.Velocity) * 12f;
                    Raylib.DrawLineEx(tail, p.Position, p.Size * 0.35f, WithAlpha(p.Color, 0.45f));
                    DrawGlow(p.Position, p.Size * 1.5f, p.Color, 0.16f);
                    var bolt = new Rectangle(p.Position.X, p.Position.Y, p.Size * 1.7f, p.Size * 0.75f);
                    Raylib.DrawRectanglePro(bolt, new Vector2(p.Size * 0.85f, p.Size * 0.38f), rot, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.3f, hot);
                    break;
                }
                case GunFireStyle.Standard:
                {
                    Vector2 tail = p.Position - SafeNormalize(p.Velocity) * 10f;
                    Raylib.DrawLineEx(tail, p.Position, p.Size * 0.4f, WithAlpha(p.Color, 0.4f * lifeT));
                    DrawGlow(p.Position, p.Size * 1.7f, p.Color, 0.17f);
                    var bolt = new Rectangle(p.Position.X, p.Position.Y, p.Size * 1.8f, p.Size);
                    Raylib.DrawRectanglePro(bolt, new Vector2(p.Size * 0.9f, p.Size * 0.5f), rot, p.Color);
                    Raylib.DrawCircleV(p.Position, p.Size * 0.38f, hot);
                    break;
                }
                default:
                    throw new UnreachableException();
            }
        }
    }

    static void DrawEnemies()
    {
        float time = (float)Raylib.GetTime();
        foreach (Enemy e in enemies)
        {
            Color color = GetEnemyColor(e.Type);
            ref readonly EnemyDef def = ref GetDef(e.Type);
            bool frozen = e.ParalyzeTimer > 0f;
            if (frozen)
            {
                color = LerpColor(color, new Color(138, 152, 172, 255), 0.58f);
            }
            if (e.Hit > 0f) color = LerpColor(color, Color.White, e.Hit / 0.12f);
            if (UhdShadersActive) color = Lighten(color, 0.24f);
            int sides = GetEnemySides(e.Type);
            float grow = EaseOutBack(e.Spawn);
            float r = e.Radius * grow;
            Vector2 p = e.Position;

            if (def.Behavior == EnemyBehavior.Lurker)
            {
                float dist = Vector2.Distance(playerPos, p);
                float stealth = Math.Clamp((dist - 120f) / 140f, 0.25f, 1f);
                if (UhdShadersActive) stealth = MathF.Max(stealth, 0.82f);
                color = WithAlpha(color, stealth * e.Spawn);
            }

            if (e.Spawn < 1f)
            {
                float tr = e.Radius + (1f - e.Spawn) * 34f;
                Raylib.DrawRing(p, tr, tr + 3f, 0f, 360f, 36, WithAlpha(color, e.Spawn * 0.8f));
            }

            float rot = time * def.RotSpeed + e.Wobble * 30f;
            float wob = 1f + MathF.Sin(time * 6f + e.Wobble) * 0.06f;
            float hitSquash = e.Hit > 0f ? (e.Hit / 0.12f) * 0.24f : 0f;
            bool stretched = e.Spawn >= 1f && BeginStretch(p, e.Vel, 0.00135f, 0.34f, hitSquash);

            float glowStrength = UhdShadersActive ? 0.22f : 0.1f;
            if (UhdShadersActive)
            {
                Raylib.DrawPolyLinesEx(p, sides, r * wob + 3.5f, rot, 4f, WithAlpha(new Color(20, 16, 14, 255), 0.82f * e.Spawn));
            }
            DrawGlowFast(p, r * (UhdShadersActive ? 1.55f : 1.35f), color, glowStrength * e.Spawn);
            Raylib.DrawPoly(p, sides, r * wob, rot, color);
            float lineW = UhdShadersActive ? 3f : 2f;
            float lineA = UhdShadersActive ? 1f : 0.9f;
            Raylib.DrawPolyLinesEx(p, sides, r * wob, rot, lineW, WithAlpha(Lighten(color, UhdShadersActive ? 0.45f : 0.3f), lineA));
            Raylib.DrawPoly(p, sides, r * 0.45f, -rot, WithAlpha(MossLight, (UhdShadersActive ? 0.72f : 0.5f) * e.Spawn));
            if (UhdShadersActive)
            {
                Raylib.DrawPolyLinesEx(p, sides, r * wob + 1.5f, rot, 2f, WithAlpha(Lighten(color, 0.55f), 0.5f * e.Spawn));
            }
            EndStretch(stretched);

            if (frozen)
            {
                float pulse = MathF.Sin(time * 14f + e.Wobble) * 0.5f + 0.5f;
                Color bolt = new Color(196, 214, 232, 255);
                Raylib.DrawRing(p, r + 4f, r + 8f + pulse * 4f, 0f, 360f, 36, WithAlpha(bolt, 0.45f + pulse * 0.4f));
                DrawGlow(p, r * 2.4f, bolt, 0.1f + pulse * 0.12f);
                for (int b = 0; b < 4; b++)
                {
                    float a = time * 9f + e.Wobble + b * 1.57f;
                    Vector2 spark = p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (r + 7f);
                    Raylib.DrawCircleV(spark, 2.2f, WithAlpha(Color.White, 0.75f));
                }
            }

            if (e.Elite && e.Spawn >= 0.5f)
            {
                Raylib.DrawRing(p, r + 6f, r + 9f, 0f, 360f, 32, WithAlpha(Gold, 0.75f * e.Spawn));
            }

            if (e.Spawn >= 1f && e.Hp < e.MaxHp && showEnemyHealthBars)
            {
                float w = IsBoss(e.Type) ? e.Radius * 2.8f : e.Radius * 2f;
                float barH = IsBoss(e.Type) ? 5f : 3f;
                float hpf = Math.Clamp(e.Hp / e.MaxHp, 0f, 1f);
                float bx = p.X - w / 2f;
                float by = p.Y - e.Radius - (IsBoss(e.Type) ? 14f : 8f);
                Raylib.DrawRectangle((int)bx, (int)by, (int)w, (int)barH, WithAlpha(ForestShadow, 0.75f));
                Raylib.DrawRectangle((int)bx, (int)by, (int)(w * hpf), (int)barH, Lighten(color, 0.3f));
            }

            if (IsBoss(e.Type) && e.Spawn >= 0.85f)
            {
                string nm = GetEnemyName(e.Type).ToUpperInvariant();
                ShadowTextCentered(nm, (int)p.X, (int)(p.Y - e.Radius - 24f), 11, WithAlpha(Gold, e.Spawn), 1f);
            }
        }
    }

    static void DrawPlayer()
    {
        float time = (float)Raylib.GetTime();
        bool dashing = dashTimer > 0f;
        Vector2 p = playerPos;
        Color body = BodyColor();
        Color bright = BodyBright();

        float auraPulse = MathF.Sin(time * 6f) * 1.2f;
        float auraR = PlayerRadius * 1.65f + auraPulse + (dashing ? 6f : 0f);
        float bodyR = PlayerRadius + (dashing ? 3f : MathF.Sin(time * 8f) * 1.2f);

        bool stretched = BeginStretch(p, playerVel, dashing ? 0.00092f : 0.00074f, dashing ? 0.55f : 0.3f);
        var accessoryRig = BuildAccessoryRig(p, bodyR);
        DrawAccessoryLayer(accessoryRig, time, accessoryIndex, AccessoryLayer.Back);
        DrawGlow(p, auraR, body, dashing ? 0.08f : 0.045f);
        Raylib.DrawCircleV(p, bodyR, body);
        Raylib.DrawCircleLinesV(p, bodyR, WithAlpha(bright, 0.9f));
        if (heraldryPatterns) DrawHeraldryHatch(new Rectangle(p.X - bodyR, p.Y - bodyR, bodyR * 2f, bodyR * 2f), body, (int)(p.X + p.Y));

        Vector2 iris = p + accessoryRig.Forward * bodyR * 0.34f;
        Raylib.DrawCircleV(iris, bodyR * 0.36f, bright);
        Raylib.DrawCircleV(iris, bodyR * 0.16f, Color.White);

        DrawAccessoryLayer(accessoryRig, time, accessoryIndex, AccessoryLayer.Mid);
        DrawAccessoryLayer(accessoryRig, time, accessoryIndex, AccessoryLayer.Front);
        EndStretch(stretched);
        DrawEquippedWeaponOnPlayer(p, bodyR, time, dashing);

        if (!dashing && dashCooldown <= 0f)
        {
            float rp = MathF.Sin(time * 5f) * 0.5f + 0.5f;
            Raylib.DrawRing(p, bodyR + 5f, bodyR + 7f + rp * 2f, 0f, 360f, 40, WithAlpha(bright, 0.4f + rp * 0.4f));
        }
    }

    static void DrawGunArchetype(int archetype, Vector2 c, float s, float time, Color primary, Color shadow, Color hi,
        float lineMul = 1f)
    {
        float L(float w) => w * lineMul;
        float bob = MathF.Sin(time * 2.4f + archetype) * s * 0.04f;
        c.Y += bob;
        switch (archetype % 10)
        {
            case 0: // pike
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.06f, c.Y - s * 0.72f, s * 0.12f, s * 1.1f), 0.1f, 2, primary);
                Raylib.DrawTriangle(new Vector2(c.X, c.Y - s * 0.82f), new Vector2(c.X - s * 0.1f, c.Y - s * 0.55f), new Vector2(c.X + s * 0.1f, c.Y - s * 0.55f), hi);
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.14f, c.Y + s * 0.2f, s * 0.28f, s * 0.16f), 0.2f, 2, shadow);
                if (lineMul > 1.5f)
                    Raylib.DrawRectangleRoundedLines(new Rectangle(c.X - s * 0.08f, c.Y - s * 0.76f, s * 0.16f, s * 1.18f), 0.1f, 4, WithAlpha(ForestShadow, 0.85f));
                break;
            case 1: // bow
                for (int seg = 0; seg < 8; seg++)
                {
                    float t0 = seg / 8f;
                    float t1 = (seg + 1) / 8f;
                    Vector2 a = new Vector2(c.X - s * 0.42f + t0 * s * 0.84f, c.Y - s * 0.35f + MathF.Sin(t0 * MathF.PI) * s * 0.55f);
                    Vector2 b = new Vector2(c.X - s * 0.42f + t1 * s * 0.84f, c.Y - s * 0.35f + MathF.Sin(t1 * MathF.PI) * s * 0.55f);
                    Raylib.DrawLineEx(a, b, L(1.8f), primary);
                }
                Raylib.DrawLineEx(new Vector2(c.X - s * 0.38f, c.Y - s * 0.3f), new Vector2(c.X + s * 0.38f, c.Y - s * 0.3f), L(1f), hi);
                Raylib.DrawLineEx(new Vector2(c.X, c.Y - s * 0.3f), new Vector2(c.X + s * 0.35f, c.Y + s * 0.1f), L(1.2f), shadow);
                break;
            case 2: // handgun
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.34f, c.Y - s * 0.12f, s * 0.68f, s * 0.24f), 0.15f, 3, primary);
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.08f, c.Y + s * 0.1f, s * 0.16f, s * 0.34f), 0.2f, 2, shadow);
                Raylib.DrawCircleV(new Vector2(c.X + s * 0.28f, c.Y), s * 0.05f * lineMul, hi);
                if (lineMul > 1.5f)
                    Raylib.DrawRectangleRoundedLines(new Rectangle(c.X - s * 0.36f, c.Y - s * 0.14f, s * 0.72f, s * 0.28f), 0.15f, 4, WithAlpha(ForestShadow, 0.8f));
                break;
            case 3: // crossbow
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.34f, c.Y - s * 0.08f, s * 0.68f, s * 0.16f), 0.1f, 2, shadow);
                Raylib.DrawLineEx(new Vector2(c.X - s * 0.42f, c.Y - s * 0.22f), new Vector2(c.X + s * 0.42f, c.Y - s * 0.22f), L(2f), primary);
                Raylib.DrawLineEx(new Vector2(c.X, c.Y - s * 0.22f), new Vector2(c.X + s * 0.38f, c.Y + s * 0.08f), L(1.2f), hi);
                break;
            case 4: // cannon
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.48f, c.Y - s * 0.14f, s * 0.82f, s * 0.28f), 0.2f, 4, primary);
                Raylib.DrawCircleV(new Vector2(c.X + s * 0.34f, c.Y), s * 0.1f * lineMul, Darken(primary, 0.4f));
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.18f, c.Y + s * 0.12f, s * 0.12f, s * 0.22f), 0.1f, 2, shadow);
                if (lineMul > 1.5f)
                    Raylib.DrawRectangleRoundedLines(new Rectangle(c.X - s * 0.5f, c.Y - s * 0.16f, s * 0.86f, s * 0.32f), 0.2f, 4, WithAlpha(ForestShadow, 0.8f));
                break;
            case 5: // flail
                Raylib.DrawLineEx(new Vector2(c.X, c.Y + s * 0.35f), new Vector2(c.X, c.Y - s * 0.15f), L(2.5f), shadow);
                Raylib.DrawLineEx(new Vector2(c.X, c.Y - s * 0.15f), new Vector2(c.X + s * 0.28f, c.Y - s * 0.42f), L(1.2f), hi);
                Raylib.DrawCircleV(new Vector2(c.X + s * 0.32f, c.Y - s * 0.48f), s * 0.14f * lineMul, primary);
                for (int spike = 0; spike < 6; spike++)
                {
                    float ang = spike * MathF.PI / 3f;
                    Raylib.DrawLineEx(new Vector2(c.X + s * 0.32f, c.Y - s * 0.48f),
                        new Vector2(c.X + s * 0.32f + MathF.Cos(ang) * s * 0.16f, c.Y - s * 0.48f + MathF.Sin(ang) * s * 0.16f),
                        L(1f), hi);
                }
                break;
            case 6: // lance
                Raylib.DrawLineEx(new Vector2(c.X - s * 0.45f, c.Y + s * 0.25f), new Vector2(c.X + s * 0.48f, c.Y - s * 0.35f), L(3f), primary);
                Raylib.DrawTriangle(new Vector2(c.X + s * 0.52f, c.Y - s * 0.38f), new Vector2(c.X + s * 0.34f, c.Y - s * 0.28f), new Vector2(c.X + s * 0.4f, c.Y - s * 0.18f), hi);
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.52f, c.Y + s * 0.12f, s * 0.18f, s * 0.2f), 0.15f, 2, shadow);
                break;
            case 7: // rifle
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.46f, c.Y - s * 0.1f, s * 0.92f, s * 0.2f), 0.12f, 3, primary);
                Raylib.DrawRectangleRounded(new Rectangle(c.X - s * 0.12f, c.Y + s * 0.08f, s * 0.14f, s * 0.28f), 0.15f, 2, shadow);
                Raylib.DrawRectangle((int)(c.X + s * 0.18f), (int)(c.Y - s * 0.16f), (int)(s * 0.24f), (int)(s * 0.08f), hi);
                if (lineMul > 1.5f)
                    Raylib.DrawRectangleRoundedLines(new Rectangle(c.X - s * 0.48f, c.Y - s * 0.12f, s * 0.96f, s * 0.24f), 0.12f, 4, WithAlpha(ForestShadow, 0.8f));
                break;
            case 8: // charm orb
            {
                float pulse = MathF.Sin(time * 4f) * 0.5f + 0.5f;
                Raylib.DrawCircleV(c, s * 0.18f * lineMul, WithAlpha(primary, 0.95f));
                Raylib.DrawCircleLinesV(c, s * (0.24f + pulse * 0.04f) * lineMul, WithAlpha(hi, 0.95f));
                Raylib.DrawLineEx(new Vector2(c.X, c.Y - s * 0.34f), new Vector2(c.X, c.Y + s * 0.34f), L(1f), WithAlpha(hi, 0.75f));
                Raylib.DrawLineEx(new Vector2(c.X - s * 0.34f, c.Y), new Vector2(c.X + s * 0.34f, c.Y), L(1f), WithAlpha(hi, 0.75f));
                break;
            }
            default: // scatter pot
                Raylib.DrawCircleV(c, s * 0.2f * lineMul, primary);
                Raylib.DrawCircleLinesV(c, s * 0.24f * lineMul, hi);
                for (int shard = 0; shard < 5; shard++)
                {
                    float ang = shard * 1.25f + time;
                    Raylib.DrawLineEx(c, new Vector2(c.X + MathF.Cos(ang) * s * 0.34f, c.Y + MathF.Sin(ang) * s * 0.34f), L(1.2f), shadow);
                }
                break;
        }
    }

    static float GunIconForwardDeg(int index) => index switch
    {
        0 => -47f,
        2 => 0f,
        _ => 0f,
    };

    static Vector2 GunIconGripOffset(int index, float s) => index switch
    {
        0 => new Vector2(s * 0.18f, s * 0.08f),
        4 => new Vector2(s * 0.12f, 0f),
        5 => new Vector2(s * 0.08f, s * 0.18f),
        7 => new Vector2(s * 0.1f, s * 0.04f),
        _ => new Vector2(s * 0.2f, 0f),
    };

    static Vector2 GunLocalMuzzlePoint(int index, Vector2 localCenter, float s) => index switch
    {
        0 => localCenter + new Vector2(s * 0.5f, -s * 0.52f),
        1 => localCenter + new Vector2(s * 0.42f, -s * 0.08f),
        4 => localCenter + new Vector2(s * 0.48f, 0f),
        5 => localCenter + new Vector2(s * 0.32f, -s * 0.48f),
        8 => localCenter + new Vector2(s * 0.34f, 0f),
        _ => localCenter + new Vector2(s * 0.46f, -s * 0.12f),
    };

    static Vector2 ResolveWeaponAimDir()
    {
        Vector2 aim = weaponAimDir.LengthSquared() > 0.001f ? weaponAimDir : lastMoveDirection;
        if (aim.LengthSquared() < 0.001f) aim = Vector2.UnitY;
        return Vector2.Normalize(aim);
    }

    static (Vector2 grip, Vector2 hand, Vector2 rim, float rot, float scale) ComputeEquippedWeaponPose(
        Vector2 p, float bodyR, float time, bool dashing)
    {
        Vector2 aim = ResolveWeaponAimDir();
        Vector2 tangent = new Vector2(-aim.Y, aim.X);

        float aimDeg = MathF.Atan2(aim.Y, aim.X) * 180f / MathF.PI;
        float rot = aimDeg - GunIconForwardDeg(equippedGun) - weaponRecoil;
        float scale = bodyR * EquippedWeaponScaleMul;

        float bob = MathF.Sin(time * 5.2f) * (dashing ? 0.8f : 2f);
        float breathe = MathF.Sin(time * 7f + equippedGun) * 0.45f;

        // Anchor on the player rim in the aim direction, then orbit with tangential sway.
        Vector2 rim = p + aim * bodyR;
        Vector2 grip = rim + aim * (bodyR * 0.14f + breathe) + tangent * bob;
        Vector2 hand = p + aim * (bodyR * 0.82f) + tangent * bob * 0.55f;

        return (grip, hand, rim, rot, scale);
    }

    static void DrawEquippedWeaponOnPlayer(Vector2 p, float bodyR, float time, bool dashing)
    {
        ref readonly Gun g = ref Guns[equippedGun];
        Vector2 aim = ResolveWeaponAimDir();
        var (grip, hand, rim, rot, scale) = ComputeEquippedWeaponPose(p, bodyR, time, dashing);
        Vector2 muzzle = GetEquippedMuzzleWorld(grip, scale, rot, equippedGun);

        Raylib.DrawLineEx(rim, hand, LerpFloat(3.2f, 4.2f, adrenaline), WithAlpha(BodyColor(), 0.9f));
        Raylib.DrawCircleV(hand, bodyR * 0.2f, BodyColor());
        Raylib.DrawCircleLinesV(hand, bodyR * 0.2f, WithAlpha(BodyBright(), 0.85f));

        DrawGlow(grip, scale * 0.85f, g.Color, 0.12f + adrenaline * 0.05f);
        DrawGlow(muzzle, scale * 0.32f, Lighten(g.Color, 0.5f), 0.1f + MathF.Sin(time * 9f) * 0.03f);

        DrawGunIconAtPivot(equippedGun, grip + new Vector2(3f, 4f), scale * 1.04f, time, rot, 0.92f, worldHeld: true, outline: true);
        DrawGunIconAtPivot(equippedGun, grip, scale, time, rot, 1f, worldHeld: true);

        Raylib.DrawLineEx(grip + aim * scale * 0.08f, muzzle, 3.5f, WithAlpha(Lighten(g.Color, 0.55f), 0.75f));
        Raylib.DrawCircleV(muzzle, 3.5f + MathF.Sin(time * 11f) * 0.8f, WithAlpha(Color.White, 0.9f));
        Raylib.DrawCircleLinesV(muzzle, 5f + MathF.Sin(time * 8f), WithAlpha(g.Color, 0.85f));

        if (weaponRecoil > 0.5f)
        {
            DrawGlow(muzzle, scale * 0.45f, Lighten(g.Color, 0.5f), Math.Min(weaponRecoil * 0.018f, 0.28f));
        }
    }

}
