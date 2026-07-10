partial class Program
{
    // ---------------------------------------------------------------- Cinematic GFX ("RTX" lighting stack)

    static string GfxShaderPath(string fileName)
        => Path.Combine(AppContext.BaseDirectory, "assets", "shaders", fileName);

    static void InitGfx()
    {
        RecreateGfxTargets();
        LoadUhdShaders();
        gfxReady = true;
    }

    static void RecreateGfxTargets()
    {
        if (gfxReady)
        {
            Raylib.UnloadRenderTexture(sceneTarget);
        }

        sceneTarget = Raylib.LoadRenderTexture(GfxRenderWidth, GfxRenderHeight);
    }

    static void LoadUhdShaders()
    {
        uhdShaderReady = false;
        if (!uhdShaders) return;

        string compositePath = GfxShaderPath("uhd_composite.fs");
        if (!File.Exists(compositePath)) return;

        if (uhdCompositeShader.Id != 0)
        {
            Raylib.UnloadShader(uhdCompositeShader);
            uhdCompositeShader = default;
        }

        uhdCompositeShader = Raylib.LoadShader(null, compositePath);
        if (uhdCompositeShader.Id == 0) return;

        uhdLocResolution = Raylib.GetShaderLocation(uhdCompositeShader, "resolution");
        uhdLocTime = Raylib.GetShaderLocation(uhdCompositeShader, "time");
        uhdLocSharpen = Raylib.GetShaderLocation(uhdCompositeShader, "sharpen");
        uhdLocVignette = Raylib.GetShaderLocation(uhdCompositeShader, "vignette");
        uhdLocGrain = Raylib.GetShaderLocation(uhdCompositeShader, "grain");
        uhdLocExposure = Raylib.GetShaderLocation(uhdCompositeShader, "exposure");
        uhdLocWarmth = Raylib.GetShaderLocation(uhdCompositeShader, "warmth");
        uhdLocAdrenaline = Raylib.GetShaderLocation(uhdCompositeShader, "adrenaline");
        uhdShaderReady = true;
    }

    static void UnloadGfx()
    {
        if (!gfxReady) return;
        Raylib.UnloadRenderTexture(sceneTarget);
        UnloadMenuCastleBake();
        if (uhdCompositeShader.Id != 0) Raylib.UnloadShader(uhdCompositeShader);
        gfxReady = false;
        uhdShaderReady = false;
    }

    static void ApplyUhdShaderUniforms()
    {
        if (!uhdShaderReady) return;

        float time = (float)Raylib.GetTime();
        float[] resolution = { GfxRenderWidth, GfxRenderHeight };
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocResolution, resolution, ShaderUniformDataType.Vec2);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocTime, time, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocSharpen, uhdEnabled ? 0.55f : 0.35f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocVignette, (0.42f + adrenaline * 0.22f + nearDeathPulse * 0.35f) * vignetteScale, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocGrain, (0.028f + adrenaline * 0.02f) * filmGrainScale, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocExposure, 1.08f + adrenaline * 0.12f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocWarmth, 0.18f + adrenaline * 0.14f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(uhdCompositeShader, uhdLocAdrenaline, adrenaline, ShaderUniformDataType.Float);
    }

    static void DrawTexturedRectWithOptionalShader(Texture2D texture, Rectangle src, Rectangle dest, Color tint)
    {
        if (uhdShaderReady && uhdShaders)
        {
            ApplyUhdShaderUniforms();
            Raylib.BeginShaderMode(uhdCompositeShader);
            Raylib.DrawTexturePro(texture, src, dest, Vector2.Zero, 0f, tint);
            Raylib.EndShaderMode();
        }
        else
        {
            Raylib.DrawTexturePro(texture, src, dest, Vector2.Zero, 0f, tint);
        }
    }

    static void UpdateGfx(float dt)
    {
        if (state != GameState.Playing && state != GameState.GameOver)
        {
            adrenaline = Approach(adrenaline, 0f, 3f, dt);
        }
        else
        {
            float eventBoost = activeEvent != FloorEventType.None && eventCountdown > 0f ? 0.62f : 0f;
            float comboBoost = Math.Clamp(combo / 6f, 0f, 0.55f);
            float target = Math.Clamp(
                trauma * 0.72f + comboBoost + eventBoost + zoomPunch * 2.2f + hitstop * 4.5f,
                0f, 1f);
            adrenaline = Approach(adrenaline, target, 6f, dt);
        }

        glowPulse = MathF.Sin((float)Raylib.GetTime() * (7f + adrenaline * 8f)) * 0.5f + 0.5f;

        float dashLeanTarget = dashTimer > 0f ? 1f : 0f;
        cameraDashLean = Approach(cameraDashLean, dashLeanTarget, dashTimer > 0f ? 20f : 11f, dt);
        if (dashTimer > 0f && lastMoveDirection.LengthSquared() > 0.01f)
        {
            cameraDashLeanDir = SafeNormalize(lastMoveDirection);
        }
        else if (cameraDashLean < 0.02f)
        {
            cameraDashLeanDir = Vector2.Zero;
        }

        for (int i = gfxLightPulses.Count - 1; i >= 0; i--)
        {
            GfxLightPulse lp = gfxLightPulses[i];
            lp.Life -= dt;
            if (lp.Life <= 0f) gfxLightPulses.RemoveAt(i);
            else gfxLightPulses[i] = lp;
        }
    }

    static void SpawnGfxLightPulse(Vector2 pos, Color color, float radius, float intensity, float life)
    {
        gfxLightPulses.Add(new GfxLightPulse
        {
            Position = pos,
            Color = color,
            Radius = radius,
            Intensity = intensity,
            Life = life,
            MaxLife = life,
        });
    }

    static Rectangle SceneSourceRect()
        => new(0, 0, sceneTarget.Texture.Width, -sceneTarget.Texture.Height);

    static float GlowFalloff(float normalizedRadius)
    {
        float t = Math.Clamp(normalizedRadius, 0f, 1f);
        return MathF.Exp(-4.8f * t * t);
    }

    static void DrawSmoothGlowCore(Vector2 center, float radius, Color color, float intensity, float alphaScale = 1f,
        int steps = 32)
    {
        if (intensity <= 0.0005f || radius < 0.5f) return;

        steps = Math.Clamp(steps, 10, 36);
        if (radius < 24f) steps = Math.Min(steps, 16);
        else if (radius < 80f) steps = Math.Min(steps, 22);
        int ringSteps = Math.Max(6, steps / 2);
        for (int i = steps - 1; i >= 0; i--)
        {
            float t0 = i / (float)steps;
            float t1 = (i + 1) / (float)steps;
            float r0 = radius * t0;
            float r1 = radius * t1;
            float shell = (GlowFalloff(t0) - GlowFalloff(t1)) * intensity * alphaScale;
            if (shell <= 0.0001f) continue;
            Raylib.DrawRing(center, r0, r1, 0f, 360f, ringSteps, WithAlpha(color, shell));
        }
    }

    static void DrawRadialLight(Vector2 pos, float radius, Color color, float intensity)
        => DrawSmoothGlowCore(pos, radius, color, intensity, 0.065f, steps: 18);

    static void DrawArenaAtmosphere()
    {
        float time = (float)Raylib.GetTime();
        float pulse = 0.5f + MathF.Sin(time * 1.4f) * 0.5f;
        var arena = new Rectangle(0f, 0f, WindowWidth, WindowHeight);
        MenuCastlePalette p = GameplayCastlePalette;

        DrawGradientWash(arena,
            WithAlpha(new Color(168, 162, 152, 255), 0.06f + adrenaline * 0.05f + pulse * 0.02f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.5f, 0.08f), 2.8f);

        DrawGradientWash(arena,
            WithAlpha(ForestShadow, 0f),
            WithAlpha(ForestShadow, 0.44f + adrenaline * 0.18f),
            new Vector2(0.5f, 0.88f), 2.4f);

        Color wallTint = SiegeWallTorchTint(p.TorchWarm);
        DrawGradientWash(arena,
            WithAlpha(wallTint, 0.08f + pulse * 0.04f + siegeEventDangerLight * 0.06f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.02f, 0.5f), 2f);
        DrawGradientWash(arena,
            WithAlpha(ForestShadow, 0f),
            WithAlpha(wallTint, 0.08f + pulse * 0.04f + siegeEventDangerLight * 0.06f),
            new Vector2(0.98f, 0.5f), 2f);

        DrawGradientWash(arena,
            WithAlpha(ForestShadow, 0.28f + adrenaline * 0.1f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.02f, 0.5f), 2f);
        DrawGradientWash(arena,
            WithAlpha(ForestShadow, 0f),
            WithAlpha(ForestShadow, 0.28f + adrenaline * 0.1f),
            new Vector2(0.98f, 0.5f), 2f);

        SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
        DrawGradientWash(
            new Rectangle(L.GateX - 20f, L.GateY, L.GateW + 40f, WindowHeight - L.GateY),
            WithAlpha(wallTint, 0.12f + pulse * 0.05f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.5f, 0.15f), 2.3f);

        Vector2 keyLight = new(WindowWidth * 0.28f, WindowHeight * 0.16f);
        DrawRadialLight(keyLight, 560f, new Color(196, 188, 172, 255), 0.09f + adrenaline * 0.06f);
        DrawRadialLight(new Vector2(WindowWidth * 0.72f, WindowHeight * 0.22f), 420f, new Color(148, 146, 140, 255), 0.04f + adrenaline * 0.03f);
        DrawRadialLight(new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateH * 0.4f), 220f, wallTint, 0.06f + pulse * 0.04f);

        Color tod = GetTimeOfDayTint();
        float todStr = GetTimeOfDayStrength();
        DrawGradientWash(arena, WithAlpha(tod, todStr), WithAlpha(tod, 0f), new Vector2(0.5f, 0.2f), 2.2f);

        int motes = reduceMotion ? 10 : 18 + (int)(adrenaline * 10f);
        for (int i = 0; i < motes; i++)
        {
            float h = Hash(i * 131 + 7);
            float vx = h * WindowWidth;
            float vy = (time * (28f + h * 40f) + i * 53f) % (WindowHeight + 40f) - 20f;
            float a = 0.04f + Hash(i + 19) * 0.08f + pulse * 0.03f;
            Raylib.DrawCircleV(new Vector2(vx, vy), 1.2f + Hash(i + 3) * 1.8f, WithAlpha(new Color(196, 188, 172, 255), a));
        }

        for (int ember = 0; ember < 6; ember++)
        {
            float ex = 24f + Hash(ember * 19) * (WindowWidth - 48f);
            float ey = WindowHeight - 40f - Hash(ember * 23) * 80f;
            if (MathF.Sin(time * 3f + ember) > 0.2f)
            {
                Raylib.DrawCircleV(new Vector2(ex, ey), 1f, WithAlpha(p.TorchWarm, 0.12f + pulse * 0.08f));
            }
        }
    }

    static void DrawTileContactOcclusion()
    {
        const float seam = 0.5f;
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;

                float ox = x * TileSize + seam;
                float oy = y * TileSize + seam;
                float size = TileSize - seam * 2f;
                float ao = 0f;
                if (IsCollapsedVoid(x - 1, y)) ao += 0.22f;
                if (IsCollapsedVoid(x + 1, y)) ao += 0.22f;
                if (IsCollapsedVoid(x, y - 1)) ao += 0.22f;
                if (IsCollapsedVoid(x, y + 1)) ao += 0.22f;
                if (ao <= 0.01f) continue;

                ao = Math.Min(ao, 0.55f);
                var body = new Rectangle(ox + 1.5f, oy + 1.5f, size - 3f, size - 3f);
                Raylib.DrawRectangleRounded(body, 0.12f, 6, WithAlpha(ForestShadow, ao * 0.55f));

                if (IsCollapsedVoid(x - 1, y))
                {
                    var edge = new Rectangle(ox, oy, 5f, size);
                    DrawGradientWash(edge, WithAlpha(ForestShadow, ao * 0.7f), WithAlpha(ForestShadow, 0f), new Vector2(0f, 0.5f), 1.2f);
                }
                if (IsCollapsedVoid(x + 1, y))
                {
                    var edge = new Rectangle(ox + size - 5f, oy, 5f, size);
                    DrawGradientWash(edge, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, ao * 0.7f), new Vector2(1f, 0.5f), 1.2f);
                }
                if (IsCollapsedVoid(x, y - 1))
                {
                    var edge = new Rectangle(ox, oy, size, 5f);
                    DrawGradientWash(edge, WithAlpha(ForestShadow, ao * 0.7f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0f), 1.2f);
                }
                if (IsCollapsedVoid(x, y + 1))
                {
                    var edge = new Rectangle(ox, oy + size - 5f, size, 5f);
                    DrawGradientWash(edge, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, ao * 0.7f), new Vector2(0.5f, 1f), 1.2f);
                }
            }
        }
    }

    static void DrawDynamicLighting()
    {
        if (bloomScale <= 0.01f) return;
        Raylib.BeginBlendMode(BlendMode.Additive);

        DrawRadialLight(playerPos, PlayerRadius * 5f, BodyBright(), (0.11f + adrenaline * 0.07f + glowPulse * 0.025f) * bloomScale);

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.4f) continue;
            Color c = GetEnemyColor(e.Type);
            float grow = EaseOutBack(e.Spawn);
            bool boss = IsBoss(e.Type);
            float intensity = boss ? 0.12f : 0.05f;
            if (e.Hit > 0f) intensity += (e.Hit / 0.12f) * 0.35f;
            DrawRadialLight(e.Position, e.Radius * grow * (boss ? 3.6f : 2.4f), Lighten(c, 0.25f), intensity * 0.75f * bloomScale);
        }

        foreach (Projectile p in projectiles)
        {
            float glow = p.Style switch
            {
                GunFireStyle.Laser => 0.95f,
                GunFireStyle.Mortar => 0.72f,
                GunFireStyle.Sniper => 0.58f,
                GunFireStyle.Lance => 0.62f,
                GunFireStyle.DriftOrb => 0.55f,
                GunFireStyle.Homing => 0.5f,
                GunFireStyle.RingPulse => 0.48f,
                _ => 0.38f,
            };
            float radiusMul = p.Style switch
            {
                GunFireStyle.Laser => 8f,
                GunFireStyle.Mortar => 7f,
                GunFireStyle.Lance => 5.5f,
                _ => 4.8f,
            };
            DrawRadialLight(p.Position, p.Size * radiusMul, Lighten(p.Color, 0.2f), glow * (0.38f + adrenaline * 0.12f));
        }

        foreach (Particle p in particles)
        {
            if (!p.Glow || p.Alpha < 0.2f) continue;
            DrawRadialLight(p.Position, p.Size * 4f, p.Color, p.Alpha * 0.14f);
        }

        foreach (GfxLightPulse lp in gfxLightPulses)
        {
            float lifeT = lp.MaxLife > 0.001f ? lp.Life / lp.MaxLife : 0f;
            float burst = MathF.Sin(lifeT * MathF.PI);
            DrawRadialLight(lp.Position, lp.Radius * (0.6f + (1f - lifeT) * 0.8f), lp.Color, lp.Intensity * burst);
        }

        if (activeEvent != FloorEventType.None && eventCountdown > 0f)
        {
            Color warn = activeEvent == FloorEventType.SafeZoneRush
                ? new Color(108, 132, 102, 255)
                : new Color(220, 72, 52, 255);
            if (activeEvent == FloorEventType.SafeZoneRush)
            {
                Vector2 beacon = new Vector2(
                    eventSafeRect.X + eventSafeRect.Width * 0.5f,
                    eventSafeRect.Y + eventSafeRect.Height * 0.5f);
                DrawRadialLight(beacon, 72f, warn, 0.035f + glowPulse * 0.02f);
            }
            else
            {
                float eventGlow = 0.1f + glowPulse * 0.06f;
                DrawRadialLight(new Vector2(WindowWidth * 0.5f, WindowHeight * 0.5f), 280f, warn, eventGlow * adrenaline * 0.55f);
            }

            SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
            MenuCastlePalette pal = GameplayCastlePalette;
            float time = (float)Raylib.GetTime();
            float flicker = MathF.Sin(time * 6.5f) * 0.5f + 0.5f;
            float brazierIntensity = (0.14f + siegeEventTorchPulse * 0.12f + adrenaline * 0.08f) * bloomScale;

            Color brazierHot = SiegeWallTorchTint(pal.TorchWarm);
            Color brazierCool = activeEvent == FloorEventType.SafeZoneRush
                ? LerpColor(brazierHot, warn, 0.55f)
                : LerpColor(brazierHot, warn, 0.72f);

            for (int side = 0; side < 2; side++)
            {
                float wallX = side == 0 ? L.SideWallW * 0.55f : WindowWidth - L.SideWallW * 0.55f;
                for (int brazier = 0; brazier < 4; brazier++)
                {
                    float frac = (brazier + 0.5f) / 4f;
                    float by = WindowHeight * (0.22f + frac * 0.58f);
                    float pulse = MathF.Sin(time * 7f + brazier * 1.7f + side * 2.3f) * 0.5f + 0.5f;
                    Vector2 pos = new Vector2(wallX, by);
                    DrawRadialLight(pos, 95f + pulse * 28f, brazierCool, brazierIntensity * (0.75f + pulse * 0.35f));
                    DrawRadialLight(pos + new Vector2(0f, 18f), 42f, brazierHot, brazierIntensity * 0.45f * (0.6f + flicker * 0.4f));
                }
            }

            Vector2 gateCenter = new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateH * 0.35f);
            DrawRadialLight(gateCenter, 120f, brazierCool, brazierIntensity * 0.55f);
        }

        Raylib.EndBlendMode();
    }

    static void DrawEntityRimHighlights()
    {
        Vector2 keyDir = Vector2.Normalize(new Vector2(-0.55f, -0.84f));

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.5f) continue;
            Color c = Lighten(GetEnemyColor(e.Type), UhdShadersActive ? 0.62f : 0.45f);
            float r = e.Radius * EaseOutBack(e.Spawn);
            Vector2 rim = e.Position - keyDir * (r + 2f);
            if (UhdShadersActive)
            {
                Raylib.DrawCircleV(e.Position, r + 4f, WithAlpha(new Color(22, 18, 16, 255), 0.42f * e.Spawn));
                Raylib.DrawCircleV(rim, r * 0.34f, WithAlpha(c, 0.62f + adrenaline * 0.22f));
                Raylib.DrawCircleLinesV(e.Position, r + 2.5f, WithAlpha(c, 0.48f + adrenaline * 0.18f));
            }
            else
            {
                Raylib.DrawCircleV(rim, r * 0.22f, WithAlpha(c, 0.35f + adrenaline * 0.2f));
                Raylib.DrawCircleLinesV(e.Position, r + 1.5f, WithAlpha(c, 0.12f + adrenaline * 0.1f));
            }
        }

        Vector2 pRim = playerPos - keyDir * (PlayerRadius + 2f);
        Raylib.DrawCircleV(pRim, PlayerRadius * 0.34f, WithAlpha(BodyBright(), 0.62f + adrenaline * 0.3f));
        Raylib.DrawCircleLinesV(playerPos, PlayerRadius + 2.5f, WithAlpha(BodyBright(), 0.18f + adrenaline * 0.14f));
    }

    static void DrawFloorSpecular(Vector2 anchor, float radius, Color color, float strength)
    {
        Vector2 spec = anchor + new Vector2(radius * 0.08f, radius * 0.55f);
        DrawRadialLight(spec, radius * 1.1f, Lighten(color, 0.6f), strength);
    }

    static void DrawVolumetricGodRays()
    {
        float time = (float)Raylib.GetTime();
        SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
        Vector2 gateOrigin = new Vector2(L.GateX + L.GateW / 2f, L.GateY - 20f);
        Vector2 moonOrigin = MenuCastleMoonPosition + SiegeParallaxOffset() * 0.25f;
        Vector2 origin = Vector2.Lerp(moonOrigin, gateOrigin, 0.35f);
        origin.Y = GfxS(-40f);
        int beams = activeEvent != FloorEventType.None ? 10 : 15;
        float spread = 0.48f + adrenaline * 0.22f;
        Color rayHot = SiegeWallTorchTint(new Color(220, 206, 176, 255));
        Color rayCold = new Color(148, 146, 140, 255);

        for (int i = 0; i < beams; i++)
        {
            float frac = i / (float)(beams - 1);
            float ang = (-spread + frac * spread * 2f) + MathF.Sin(time * 0.35f + i * 0.7f) * 0.04f;
            float len = GfxS(WindowHeight * (1.2f + adrenaline * 0.25f));
            float halfW = 0.1f + (1f - MathF.Abs(frac - 0.5f) * 2f) * 0.06f;
            Color beam = LerpColor(rayCold, rayHot, 1f - MathF.Abs(frac - 0.5f) * 1.6f);
            float intensity = (0.08f + adrenaline * 0.12f) * (0.65f + glowPulse * 0.35f);
            DrawLightCone(new Vector2(GfxS(origin.X), origin.Y), ang + MathF.PI * 0.5f, len, halfW, beam, intensity);
        }

        DrawRadialLight(new Vector2(GfxS(gateOrigin.X), GfxS(gateOrigin.Y + 40f)), GfxS(180f), rayHot, 0.05f + adrenaline * 0.04f);
        DrawRadialLight(new Vector2(GfxS(origin.X), GfxS(80f)), GfxS(300f), rayHot, 0.06f + adrenaline * 0.05f);
    }

    static void DrawSceneComposite()
    {
        Rectangle src = SceneSourceRect();
        var dest = new Rectangle(0, 0, WindowWidth, WindowHeight);
        Color tint = LerpColor(Color.White, new Color(255, 244, 228, 255), 0.12f + adrenaline * 0.16f);
        DrawTexturedRectWithOptionalShader(sceneTarget.Texture, src, dest, tint);

        if (!uhdShaderReady || !uhdShaders)
        {
            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawTexturePro(sceneTarget.Texture, src, dest, Vector2.Zero, 0f, WithAlpha(new Color(255, 220, 180, 255), 0.04f + adrenaline * 0.05f));
            Raylib.EndBlendMode();
        }
    }

    static void DrawBloomOverlay()
    {
        if (bloomScale <= 0.01f) return;
        Raylib.BeginBlendMode(BlendMode.Additive);

        foreach (Particle p in particles)
        {
            if (!p.Glow || p.Alpha < 0.15f) continue;
            DrawRadialLight(p.Position, p.Size * 7f, p.Color, p.Alpha * 0.1f * bloomScale);
        }

        foreach (Projectile p in projectiles)
        {
            if (p.Style == GunFireStyle.Laser) continue;
            DrawRadialLight(p.Position, p.Size * 9f, p.Color, (0.07f + adrenaline * 0.05f) * bloomScale);
        }

        Raylib.EndBlendMode();
    }

    static void DrawScreenVignette()
    {
        float strength = (0.22f + adrenaline * 0.18f) * vignetteScale;
        int band = WindowWidth / 3;
        Raylib.DrawRectangleGradientH(0, 0, band, WindowHeight, WithAlpha(ForestShadow, strength), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH(WindowWidth - band, 0, band, WindowHeight, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, strength));
        int vband = WindowHeight / 3;
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, vband, WithAlpha(ForestShadow, strength * 0.85f), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientV(0, WindowHeight - vband, WindowWidth, vband, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, strength * 0.85f));
    }

    static void DrawCinematicPostProcess()
    {
        float ca = Math.Clamp(impactFlash * 0.95f + adrenaline * 0.28f, 0f, 1f);
        if (ca > 0.02f)
        {
            Rectangle src = SceneSourceRect();
            float off = 2f + ca * 10f;
            var destL = new Rectangle(-off, 0f, WindowWidth, WindowHeight);
            var destR = new Rectangle(off, 0f, WindowWidth, WindowHeight);
            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawTexturePro(sceneTarget.Texture, src, destL, Vector2.Zero, 0f, new Color(90, 20, 20, (int)(ca * 44f)));
            Raylib.DrawTexturePro(sceneTarget.Texture, src, destR, Vector2.Zero, 0f, new Color(20, 30, 90, (int)(ca * 44f)));
            Raylib.EndBlendMode();
        }

        Color warm = WithAlpha(new Color(196, 168, 132, 255), 0.04f + adrenaline * 0.07f);
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, WindowHeight / 2 + 40,
            warm, WithAlpha(warm, 0f));

        Color lowerWash = WithAlpha(new Color(8, 8, 8, 255), 0.035f + adrenaline * 0.04f);
        Raylib.DrawRectangleGradientV(0, WindowHeight / 2 - 20, WindowWidth, WindowHeight / 2 + 20,
            WithAlpha(lowerWash, 0f), lowerWash);

        Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight,
            WithAlpha(ForestShadow, 0.05f + adrenaline * 0.09f));

        if (!uhdShaderReady || !uhdShaders)
        {
            DrawScreenVignette();
            DrawFilmGrain(0.32f + adrenaline * 0.48f + impactFlash * 0.28f);
        }
        else
        {
            DrawFilmGrain((0.08f + impactFlash * 0.12f) * 0.35f);
        }
    }

    static void DrawFilmGrain(float intensity)
    {
        intensity *= filmGrainScale;
        if (intensity <= 0.02f) return;
        if (reduceMotion) intensity *= 0.45f;
        if (UhdShadersActive && uhdShaders) intensity *= 0.35f;
        float eventMul = activeEvent != FloorEventType.None ? 0.5f : 1f;
        int grains = Math.Min(140, (int)(650f * intensity * eventMul));
        float time = frameTime > 0f ? frameTime : (float)Raylib.GetTime();
        int seedBase = (int)(time * 60f);
        for (int i = 0; i < grains; i++)
        {
            int h = seedBase + i * 7919;
            int px = (int)(Hash(h) * WindowWidth);
            int py = (int)(Hash(h + 17) * WindowHeight);
            float a = Hash(h + 33) * intensity * 0.35f;
            bool bright = Hash(h + 51) > 0.5f;
            Raylib.DrawPixel(px, py, WithAlpha(bright ? Color.White : ForestShadow, a));
        }
    }

}
