partial class Program
{
    // -------------------------------------------------------------------------
    // Gameplay siege castle (arena bailey — never drawn on main menu)
    // -------------------------------------------------------------------------

    struct SiegeBaileyLayout
    {
        public float ArenaPad;
        public float SideWallW;
        public float BottomWallH;
        public float TopParapetH;
        public float GateW;
        public float GateH;
        public float GateX;
        public float GateY;

        public static SiegeBaileyLayout Compute()
        {
            const float pad = 4f;
            const float sideWallW = 76f;
            const float bottomWallH = 124f;
            const float topParapetH = 46f;
            const float gateW = 116f;
            const float gateH = 102f;
            float gateX = WindowWidth / 2f - gateW / 2f;
            float gateY = WindowHeight - pad - bottomWallH + 10f;
            return new SiegeBaileyLayout
            {
                ArenaPad = pad,
                SideWallW = sideWallW,
                BottomWallH = bottomWallH,
                TopParapetH = topParapetH,
                GateW = gateW,
                GateH = gateH,
                GateX = gateX,
                GateY = gateY,
            };
        }
    }

    static MenuCastlePalette GameplayCastlePalette => MenuCastlePalette.Default;

    static void UpdateSiegeGraphics(float dt)
    {
        if (state != GameState.Playing && state != GameState.Paused && state != GameState.GameOver) return;

        if (siegeGateOpenTimer > 0f) siegeGateOpenTimer = Math.Max(0f, siegeGateOpenTimer - dt);

        float targetDanger = 0f;
        float targetSafe = 0f;
        if (activeEvent != FloorEventType.None && eventCountdown > 0f)
        {
            if (activeEvent == FloorEventType.SafeZoneRush)
            {
                targetSafe = 0.72f + adrenaline * 0.28f;
            }
            else
            {
                targetDanger = 0.68f + adrenaline * 0.32f;
            }

            siegeEventTorchPulse = MathF.Sin((float)Raylib.GetTime() * 7f) * 0.5f + 0.5f;
        }

        siegeEventDangerLight = Approach(siegeEventDangerLight, targetDanger, 4.5f, dt);
        siegeEventSafeLight = Approach(siegeEventSafeLight, targetSafe, 4.5f, dt);
    }

    static Color SiegeWallTorchTint(Color warmBase)
    {
        if (siegeEventDangerLight > 0.02f)
        {
            Color danger = new Color(220, 72, 52, 255);
            return LerpColor(warmBase, danger, siegeEventDangerLight * (0.55f + siegeEventTorchPulse * 0.45f));
        }

        if (siegeEventSafeLight > 0.02f)
        {
            Color safe = new Color(108, 132, 102, 255);
            return LerpColor(warmBase, safe, siegeEventSafeLight * (0.5f + siegeEventTorchPulse * 0.35f));
        }

        return warmBase;
    }

    static Vector2 SiegeParallaxOffset()
    {
        Camera2D cam = ComputeCamera();
        float ox = cam.Offset.X - WindowWidth / 2f;
        float oy = cam.Offset.Y - WindowHeight / 2f;
        return new Vector2(ox * 0.06f, oy * 0.06f);
    }

    static void DrawSiegeBackdrop(float time)
    {
        MenuCastlePalette p = GameplayCastlePalette;
        Vector2 parallax = SiegeParallaxOffset();
        float sway = MathF.Sin(time * 0.22f) * 3f;

        Vector2 moon = MenuCastleMoonPosition + parallax * 0.35f;
        moon.X += sway * 0.15f;
        DrawEllipticalGlow(moon, 52f, 52f, 0f, p.MoonGlow, 0.045f, 3);
        Raylib.DrawCircleV(moon, 16f, WithAlpha(p.MoonGlow, 0.82f));
        Raylib.DrawCircleV(moon + new Vector2(-4f, -3f), 12f, WithAlpha(Color.White, 0.22f));

        float horizonY = WindowHeight * 0.14f + parallax.Y;
        var skyBand = new Rectangle(-20f, -20f, WindowWidth + 40f, horizonY + 40f);
        DrawGradientWash(skyBand, WithAlpha(new Color(12, 11, 10, 255), 0.55f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 1f), 2.4f);

        float wallBaseY = WindowHeight * 0.11f + parallax.Y * 0.5f;
        float wallH = WindowHeight * 0.2f;
        var distantWall = new Rectangle(-30f + parallax.X * 0.4f, wallBaseY, WindowWidth + 60f, wallH);
        Raylib.DrawRectangleRounded(distantWall, 0.02f, 8, WithAlpha(p.StoneDeep, 0.42f));
        DrawStoneMasonry(distantWall, WithAlpha(p.StoneDark, 0.35f), WithAlpha(p.Mortar, 0.28f), 5, 18, 0.38f);
        DrawBattlements(new Rectangle(distantWall.X, distantWall.Y - 10f, distantWall.Width, 14f),
            WithAlpha(p.StoneDeep, 0.38f), WithAlpha(p.StoneMid, 0.32f), WithAlpha(p.StoneHi, 0.28f), 0.85f, time, true);

        for (int tower = 0; tower < 5; tower++)
        {
            float tx = distantWall.X + distantWall.Width * (0.08f + tower * 0.21f) + sway * (tower % 2 == 0 ? 1f : -1f);
            float tw = 28f + Hash(tower * 17) * 14f;
            float th = wallH * (0.85f + Hash(tower * 23) * 0.35f);
            var towerRect = new Rectangle(tx, distantWall.Y + wallH - th, tw, th);
            Raylib.DrawRectangleRounded(towerRect, 0.05f, 6, WithAlpha(p.StoneDark, 0.4f));
            DrawBattlements(new Rectangle(towerRect.X - 2f, towerRect.Y - 8f, towerRect.Width + 4f, 10f),
                WithAlpha(p.StoneDeep, 0.35f), WithAlpha(p.StoneLight, 0.3f), WithAlpha(p.StoneHi, 0.25f), 0.8f, time, tower % 2 == 0);
        }

        var mist = new Rectangle(0f, wallBaseY + wallH * 0.55f, WindowWidth, WindowHeight * 0.35f);
        float mistPulse = MathF.Sin(time * 0.55f) * 0.5f + 0.5f;
        DrawGradientWash(mist, WithAlpha(p.MoonGlow, 0f), WithAlpha(p.StoneDeep, 0.18f + mistPulse * 0.06f), new Vector2(0.5f, 0.35f), 2.6f);

        for (int firefly = 0; firefly < 14; firefly++)
        {
            float life = (time * (0.28f + Hash(firefly * 5) * 0.4f) + firefly * 1.4f) % 1f;
            float fx = Hash(firefly * 11) * WindowWidth + MathF.Sin(time * 0.5f + firefly) * 18f;
            float fy = wallBaseY + wallH + 20f + life * (WindowHeight * 0.45f);
            float glow = MathF.Sin(life * MathF.PI);
            Raylib.DrawCircleV(new Vector2(fx, fy), 1.1f, WithAlpha(p.TorchWarm, glow * 0.35f));
        }
    }

    static void DrawArenaCourtyardCobbles(Rectangle arena, float time)
    {
        MenuCastlePalette p = GameplayCastlePalette;
        Raylib.DrawRectangleRounded(arena, 0.04f, 8, p.StoneDeep);

        int cols = 28;
        float colW = arena.Width / cols;
        int rows = Math.Max(18, (int)(arena.Height / (colW * 0.82f)));
        float rowH = arena.Height / rows;

        for (int row = 0; row < rows; row++)
        {
            float y = arena.Y + row * rowH;
            float offset = row % 2 == 0 ? 0f : colW * 0.5f;
            for (int col = -1; col < cols + 1; col++)
            {
                float x = arena.X + col * colW + offset;
                int seed = row * 197 + col * 89;
                float n = Hash(seed);
                float n2 = Hash(seed + 13);
                float inset = 0.7f + n * 1.4f;
                var stone = new Rectangle(x + inset, y + inset, colW - inset * 2f, rowH - inset * 2f);
                if (stone.X + stone.Width < arena.X || stone.X > arena.X + arena.Width) continue;

                float cx = stone.X + stone.Width / 2f;
                float cy = stone.Y + stone.Height / 2f;
                float dist = Vector2.Distance(new Vector2(cx, cy), new Vector2(arena.X + arena.Width / 2f, arena.Y + arena.Height / 2f));
                float maxDist = MathF.Min(arena.Width, arena.Height) * 0.52f;
                float edgeDark = Math.Clamp(dist / maxDist, 0f, 1f);

                Color face = LerpColor(p.StoneMid, p.StoneLight, n * 0.45f + 0.12f);
                face = LerpColor(face, p.StoneDark, edgeDark * 0.38f);
                face = LerpColor(face, p.WetSheen, n2 * 0.12f);
                Raylib.DrawRectangleRounded(stone, 0.1f + n * 0.06f, 3, WithAlpha(face, 0.78f + n2 * 0.16f));
                Raylib.DrawLine((int)stone.X, (int)stone.Y, (int)(stone.X + stone.Width * 0.55f), (int)stone.Y, WithAlpha(p.StoneHi, 0.08f + n * 0.06f));
                if (n > 0.82f)
                {
                    Raylib.DrawCircleV(new Vector2(stone.X + stone.Width * 0.5f, stone.Y + stone.Height * 0.5f), 0.9f, WithAlpha(p.Mortar, 0.3f));
                }
            }
        }

        DrawStoneMasonry(arena, WithAlpha(p.StoneDark, 0.08f), WithAlpha(p.Mortar, 0.14f), rows / 3, cols / 4, 0.42f);

        float centerX = arena.X + arena.Width / 2f;
        float centerY = arena.Y + arena.Height / 2f;
        float ringR = MathF.Min(arena.Width, arena.Height) * 0.19f;
        Color herald = WithAlpha(p.HeraldRed, 0.07f + MathF.Sin(time * 0.8f) * 0.02f);
        Raylib.DrawCircleLines((int)centerX, (int)centerY, ringR, herald);
        Raylib.DrawCircleLines((int)centerX, (int)centerY, ringR - 5f, WithAlpha(p.HeraldRedLight, 0.05f));
        for (int spoke = 0; spoke < 8; spoke++)
        {
            float ang = spoke * MathF.PI / 4f + time * 0.04f;
            Vector2 inner = new Vector2(centerX + MathF.Cos(ang) * (ringR * 0.35f), centerY + MathF.Sin(ang) * (ringR * 0.35f));
            Vector2 outer = new Vector2(centerX + MathF.Cos(ang) * (ringR * 0.92f), centerY + MathF.Sin(ang) * (ringR * 0.92f));
            Raylib.DrawLineEx(inner, outer, 1f, WithAlpha(p.HeraldRedDeep, 0.06f));
        }

        DrawGradientWash(arena,
            WithAlpha(p.TorchWarm, 0.04f + MathF.Sin(time * 3.5f) * 0.015f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.5f, 0.88f), 2.5f);
        DrawGradientWash(arena,
            WithAlpha(ForestShadow, 0.12f),
            WithAlpha(ForestShadow, 0f),
            new Vector2(0.5f, 0.08f), 2.2f);

        for (int sheen = 0; sheen < 22; sheen++)
        {
            if (MathF.Sin(time * 2.4f + sheen * 1.1f) < 0.55f) continue;
            float sx = arena.X + Hash(sheen * 29) * arena.Width;
            float sy = arena.Y + Hash(sheen * 31) * arena.Height;
            Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + 12f, sy - 2f), 0.85f, WithAlpha(p.WetSheen, 0.12f));
        }
    }

    static void DrawSiegePortcullis(Vector2 origin, float width, float height, MenuCastlePalette p, float openT, float time)
    {
        openT = Math.Clamp(openT, 0f, 1f);
        float lift = openT * height * 0.82f;
        float grateTop = origin.Y + height * 0.12f - lift;
        float grateBot = origin.Y + height * 0.94f - lift * 0.35f;
        int bars = 11;

        for (int b = 0; b < bars; b++)
        {
            float bx = origin.X + 8f + b * ((width - 16f) / (bars - 1));
            Raylib.DrawLineEx(new Vector2(bx, grateTop), new Vector2(bx, grateBot), 2.6f, WithAlpha(p.Iron, 0.72f));
            Raylib.DrawLineEx(new Vector2(bx + 0.8f, grateTop), new Vector2(bx + 0.8f, grateBot), 1f, WithAlpha(p.StoneHi, 0.22f));
            for (int rivet = 0; rivet < 6; rivet++)
            {
                float rivetY = grateTop + rivet * ((grateBot - grateTop) / 5f);
                Raylib.DrawCircleV(new Vector2(bx, rivetY), 1.3f, WithAlpha(p.Iron, 0.55f));
            }
        }

        for (int r = 0; r < 6; r++)
        {
            float gy = grateTop + r * ((grateBot - grateTop) / 5f);
            Raylib.DrawLineEx(new Vector2(origin.X + 6f, gy), new Vector2(origin.X + width - 6f, gy), 1.6f, WithAlpha(p.StoneLight, 0.38f));
        }

        if (openT < 0.98f)
        {
            float spark = MathF.Sin(time * 18f) * 0.5f + 0.5f;
            Raylib.DrawLineEx(new Vector2(origin.X + 4f, grateTop - 2f), new Vector2(origin.X + width - 4f, grateTop - 2f),
                2f, WithAlpha(p.TorchWarm, (1f - openT) * (0.25f + spark * 0.15f)));
        }
    }

    static void DrawGameplayGatehouse(float time, float alpha)
    {
        SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
        MenuCastlePalette p = GameplayCastlePalette;
        var gateOrigin = new Vector2(L.GateX, L.GateY);

        float openT = 1f - Math.Clamp(siegeGateOpenTimer / SiegeGateOpenDuration, 0f, 1f);
        if (waveBannerTimer > 0f)
        {
            float bannerT = Math.Clamp(waveBannerTimer / WaveBannerTime, 0f, 1f);
            openT = Math.Max(openT, 1f - bannerT * 0.85f);
        }

        var gatehouse = new Rectangle(L.GateX - 18f, L.GateY - 28f, L.GateW + 36f, L.GateH + 36f);
        Raylib.DrawRectangleRounded(gatehouse, 0.06f, 8, WithAlpha(p.StoneMid, alpha * 0.92f));
        DrawStoneMasonry(gatehouse, WithAlpha(p.Stone, alpha * 0.85f), WithAlpha(p.Mortar, alpha * 0.7f), 7, 5, 0.4f);
        DrawQuoinStripes(gatehouse, WithAlpha(p.StoneLight, alpha), WithAlpha(p.StoneHi, alpha));
        DrawBattlements(new Rectangle(gatehouse.X - 4f, gatehouse.Y - 16f, gatehouse.Width + 8f, 18f),
            WithAlpha(p.StoneDark, alpha), WithAlpha(p.StoneLight, alpha), WithAlpha(p.StoneHi, alpha), 1f, time, true);

        DrawMenuCastleGate(gateOrigin, L.GateW, L.GateH, p, time);
        DrawSiegePortcullis(gateOrigin, L.GateW, L.GateH, p, openT, time);

        if (waveBannerTimer > 0f && showWaveBanner)
        {
            float bannerA = Math.Clamp(waveBannerTimer / WaveBannerTime, 0f, 1f);
            float bannerY = gatehouse.Y - 38f;
            var pennant = new Rectangle(L.GateX + L.GateW / 2f - 22f, bannerY, 44f, 30f);
            Color bannerCol = IsBossWave(waveNumber) ? p.HeraldRedLight : Gold;
            Raylib.DrawTriangle(
                new Vector2(pennant.X, pennant.Y),
                new Vector2(pennant.X + pennant.Width, pennant.Y),
                new Vector2(pennant.X + pennant.Width / 2f, pennant.Y + pennant.Height),
                WithAlpha(bannerCol, bannerA * 0.85f));
            string waveTag = $"W{waveNumber}";
            int tw = Raylib.MeasureText(waveTag, 11);
            Raylib.DrawText(waveTag, (int)(pennant.X + pennant.Width / 2f - tw / 2f), (int)(pennant.Y + 6f), 11, WithAlpha(Color.White, bannerA));
        }

        float torchFlicker = MathF.Sin(time * 5.2f) * 0.5f + 0.5f;
        Color gateGlow = SiegeWallTorchTint(p.TorchWarm);
        DrawEllipticalGlow(new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateH * 0.5f), 90f, 50f, 0f, gateGlow, 0.02f + torchFlicker * 0.015f, 2);
    }

    static void DrawSiegeBaileyPerimeter(float time, float alpha)
    {
        SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
        MenuCastlePalette p = GameplayCastlePalette;
        Color stone = p.Stone;
        Color stoneMid = p.StoneMid;
        Color stoneDark = p.StoneDark;
        Color stoneLight = p.StoneLight;
        Color stoneHi = p.StoneHi;
        Color mortar = p.Mortar;

        DrawBattlements(new Rectangle(0f, 0f, WindowWidth, L.TopParapetH),
            WithAlpha(stoneDark, alpha), WithAlpha(stoneMid, alpha), WithAlpha(stoneLight, alpha), 1f, time, true);

        var topWall = new Rectangle(L.ArenaPad, L.TopParapetH - 8f, WindowWidth - L.ArenaPad * 2f, 14f);
        Raylib.DrawRectangleRounded(topWall, 0.03f, 6, WithAlpha(stoneMid, alpha * 0.75f));
        DrawStoneMasonry(topWall, WithAlpha(stone, alpha * 0.55f), WithAlpha(mortar, alpha * 0.45f), 2, 24, 0.35f);

        float wallY = WindowHeight - L.BottomWallH;
        var leftWall = new Rectangle(0f, wallY, L.SideWallW, L.BottomWallH);
        var rightWall = new Rectangle(WindowWidth - L.SideWallW, wallY, L.SideWallW, L.BottomWallH);
        Raylib.DrawRectangleRounded(leftWall, 0.04f, 6, WithAlpha(stoneMid, alpha));
        Raylib.DrawRectangleRounded(rightWall, 0.04f, 6, WithAlpha(stoneMid, alpha));
        DrawStoneMasonry(leftWall, WithAlpha(stone, alpha), WithAlpha(mortar, alpha), 8, 3, 0.4f);
        DrawStoneMasonry(rightWall, WithAlpha(stone, alpha), WithAlpha(mortar, alpha), 8, 3, 0.4f);
        DrawQuoinStripes(leftWall, WithAlpha(stoneLight, alpha), WithAlpha(stoneHi, alpha));
        DrawQuoinStripes(rightWall, WithAlpha(stoneLight, alpha), WithAlpha(stoneHi, alpha));

        DrawTowerRoof(L.SideWallW / 2f, wallY - 30f, wallY + 6f, L.SideWallW * 0.5f,
            WithAlpha(stoneDark, alpha), WithAlpha(stoneLight, alpha), WithAlpha(stoneHi, alpha));
        DrawTowerRoof(WindowWidth - L.SideWallW / 2f, wallY - 30f, wallY + 6f, L.SideWallW * 0.5f,
            WithAlpha(stoneDark, alpha), WithAlpha(stoneLight, alpha), WithAlpha(stoneHi, alpha));

        var bottomSpine = new Rectangle(L.SideWallW, WindowHeight - 38f, WindowWidth - L.SideWallW * 2f, 34f);
        Raylib.DrawRectangleRounded(bottomSpine, 0.02f, 6, WithAlpha(stoneDark, alpha * 0.88f));
        DrawStoneMasonry(bottomSpine, WithAlpha(stone, alpha * 0.7f), WithAlpha(mortar, alpha * 0.55f), 2, 22, 0.38f);

        int merlonCount = 20;
        for (int m = 0; m < merlonCount; m++)
        {
            float mx = L.SideWallW + m * ((WindowWidth - L.SideWallW * 2f) / (merlonCount - 1));
            if (mx > L.GateX - 14f && mx < L.GateX + L.GateW + 14f) continue;
            if (m % 2 != 0) continue;
            Raylib.DrawRectangle((int)mx, (int)(WindowHeight - 44f), 9, 11, WithAlpha(stoneMid, alpha * 0.8f));
        }

        float[] torchYs = { wallY + 48f, wallY + 88f, L.TopParapetH + 18f, WindowHeight - 62f };
        float[] torchPhases = { 0f, 1.7f, 3.1f, 4.8f, 2.2f, 5.5f, 0.9f, 3.9f };
        Vector2[] torchPos =
        {
            new(28f, torchYs[0]), new(WindowWidth - 28f, torchYs[0]),
            new(28f, torchYs[1]), new(WindowWidth - 28f, torchYs[1]),
            new(28f, torchYs[2]), new(WindowWidth - 28f, torchYs[2]),
            new(28f, torchYs[3]), new(WindowWidth - 28f, torchYs[3]),
        };
        for (int i = 0; i < torchPos.Length; i++)
        {
            DrawCastleTorch(torchPos[i], time, torchPhases[i], 0.82f, TorchMountKind.WallFace);
            Color glow = SiegeWallTorchTint(p.TorchWarm);
            float flicker = MathF.Sin(time * 5.8f + torchPhases[i]) * 0.5f + 0.5f;
            DrawEllipticalGlow(torchPos[i] + new Vector2(0f, -8f), 34f, 24f, 0f, glow, 0.012f + flicker * 0.01f, 2);
        }

        DrawGameplayGatehouse(time, alpha);
    }

    static void DrawMasonryStressMarks(Rectangle bodyRect, int x, int y, float time, float urgency)
    {
        MenuCastlePalette p = GameplayCastlePalette;
        int seed = x * 92821 ^ y * 68917;
        Color stress = WithAlpha(p.HeraldRedLight, 0.35f + urgency * 0.35f);
        for (int i = 0; i < 4; i++)
        {
            float fx = bodyRect.X + 4f + Hash(seed + i * 3) * (bodyRect.Width - 8f);
            float fy = bodyRect.Y + 4f + Hash(seed + i * 3 + 1) * (bodyRect.Height - 8f);
            float len = 6f + Hash(seed + i) * 10f;
            float ang = Hash(seed + i * 5) * MathF.PI;
            Vector2 end = new Vector2(fx + MathF.Cos(ang) * len, fy + MathF.Sin(ang) * len);
            Raylib.DrawLineEx(new Vector2(fx, fy), end, 1.2f, stress);
        }

        float pulse = MathF.Sin(time * 14f + x + y) * 0.5f + 0.5f;
        Raylib.DrawRectangleRoundedLines(bodyRect, 0.12f, 6, WithAlpha(p.HeraldRed, (0.35f + pulse * 0.4f) * urgency));
    }

    static void DrawVaultPit(float left, float top, float w, float h, float cx, float cy, int x, int y, float time)
    {
        MenuCastlePalette p = GameplayCastlePalette;
        Raylib.DrawRectangle((int)left, (int)top, (int)w, (int)h, LerpColor(p.StoneDeep, Color.Black, 0.55f));

        if (w > 8f && h > 8f)
        {
            var inner = new Rectangle(left + 4f, top + 4f, w - 8f, h - 8f);
            Raylib.DrawRectangleGradientV((int)inner.X, (int)inner.Y, (int)inner.Width, (int)inner.Height,
                WithAlpha(p.StoneDark, 0.35f), WithAlpha(Color.Black, 0.55f));

            int beams = Math.Max(2, (int)(w / TileSize));
            for (int b = 0; b < beams; b++)
            {
                float bx = left + 6f + b * ((w - 12f) / Math.Max(1, beams - 1));
                Raylib.DrawLineEx(new Vector2(bx, top + 3f), new Vector2(bx, top + h - 3f), 2f, WithAlpha(p.StoneLight, 0.28f));
                Raylib.DrawLineEx(new Vector2(bx + 1f, top + 3f), new Vector2(bx + 1f, top + h - 3f), 1f, WithAlpha(ForestShadow, 0.45f));
            }

            float lintelY = top + h * 0.22f;
            Raylib.DrawLineEx(new Vector2(left + 3f, lintelY), new Vector2(left + w - 3f, lintelY), 2.5f, WithAlpha(p.StoneHi, 0.32f));
        }

        float dust = MathF.Sin(time * 8f + x + y) * 0.5f + 0.5f;
        Raylib.DrawCircleV(new Vector2(cx, cy - 2f), 1.5f + dust, WithAlpha(p.Mortar, 0.25f + dust * 0.2f));
    }

    static Color StoneBase(int x, int y)
    {
        MenuCastlePalette p = GameplayCastlePalette;
        float h = Hash(x * 31 + y * 17);
        float h2 = Hash(x * 47 + y * 71);
        Color baseCol = LerpColor(p.StoneDark, p.StoneMid, h * 0.5f + 0.22f);
        Color warmed = LerpColor(baseCol, p.StoneDeep, h2 * 0.18f);
        float diag = ((x + y) & 1) == 0 ? 0.03f : -0.02f;
        return Darken(warmed, 0.06f + diag);
    }

    static Color StoneTileFill(float durability)
    {
        float t = Math.Clamp(durability / MaxDurability, 0f, 1f);
        MenuCastlePalette p = GameplayCastlePalette;
        return GetTileHealthVisual(t) switch
        {
            TileHealthVisual.Pristine => LerpColor(p.StoneLight, p.StoneHi, Math.Clamp((t - 0.92f) / 0.08f, 0f, 1f)),
            TileHealthVisual.Sturdy => LerpColor(p.StoneMid, p.StoneLight, (t - 0.7f) / 0.22f),
            TileHealthVisual.Worn => LerpColor(p.Stone, p.StoneMid, (t - 0.5f) / 0.2f),
            TileHealthVisual.Cracked => LerpColor(p.StoneDark, p.Stone, (t - 0.35f) / 0.15f),
            TileHealthVisual.Fractured => LerpColor(p.StoneDeep, p.StoneDark, (t - 0.2f) / 0.15f),
            TileHealthVisual.Critical => LerpColor(new Color(28, 26, 24, 255), p.StoneDeep, t / 0.2f),
            _ => Danger,
        };
    }

    static void DrawEdgeCastle(float time, float alpha)
    {
        DrawSiegeBaileyPerimeter(time, alpha);
    }

    static void DrawEdgeForest(float time, float alpha)
    {
        float swayL = MathF.Sin(time * 0.4f) * 2f;
        float swayR = MathF.Sin(time * 0.45f + 1.2f) * 2f;

        DrawOrganicTree(new Vector2(28f + swayL, WindowHeight - 10f), 1.15f, -0.04f, alpha * 0.5f);
        DrawOrganicTree(new Vector2(62f + swayL * 0.5f, WindowHeight - 30f), 0.85f, 0.03f, alpha * 0.4f);
        DrawOrganicTree(new Vector2(WindowWidth - 34f + swayR, WindowHeight - 12f), 1.1f, 0.05f, alpha * 0.5f);
        DrawOrganicTree(new Vector2(WindowWidth - 68f + swayR * 0.5f, WindowHeight - 28f), 0.8f, -0.03f, alpha * 0.38f);
    }

    static void DrawOrganicTree(Vector2 root, float scale, float lean, float alpha)
    {
        float h = 88f * scale;
        Color trunk = WithAlpha(new Color(72, 54, 38, 255), alpha);
        Color trunkDark = WithAlpha(new Color(48, 36, 26, 255), alpha);
        Color leafA = WithAlpha(new Color(48, 82, 52, 255), alpha);
        Color leafB = WithAlpha(new Color(62, 108, 62, 255), alpha * 0.9f);
        Color leafC = WithAlpha(new Color(78, 128, 72, 255), alpha * 0.75f);

        Vector2 top = new Vector2(root.X + lean * h, root.Y - h);
        Vector2 mid = new Vector2(root.X + lean * h * 0.45f, root.Y - h * 0.55f);
        float trunkW = 5f * scale;

        Raylib.DrawLineEx(root, mid, trunkW + 1f, trunkDark);
        Raylib.DrawLineEx(mid, top, trunkW, trunk);
        Raylib.DrawLineEx(
            new Vector2(root.X - trunkW, root.Y),
            new Vector2(top.X - trunkW * 0.3f, top.Y + 8f * scale),
            1.5f, WithAlpha(trunkDark, alpha * 0.5f));

        Vector2 crown = new Vector2(top.X, top.Y - 6f * scale);
        Raylib.DrawCircleV(new Vector2(crown.X - 14f * scale, crown.Y + 8f * scale), 22f * scale, leafA);
        Raylib.DrawCircleV(new Vector2(crown.X + 16f * scale, crown.Y + 10f * scale), 20f * scale, leafB);
        Raylib.DrawCircleV(crown, 24f * scale, leafC);
        Raylib.DrawCircleV(new Vector2(crown.X, crown.Y - 10f * scale), 18f * scale, leafB);

        DrawGlow(crown, 28f * scale, leafC, alpha * 0.12f);
    }

    static void DrawMotes(float alphaMult = 1f) 
    {
        if (!backgroundMotes) return;
        float time = frameTime > 0f ? frameTime : (float)Raylib.GetTime();
        foreach (Mote m in motes)
        {
            if (m.IsLeaf)
            {
                float sway = MathF.Sin(time * 1.2f + m.Phase) * 0.3f;
                var rect = new Rectangle(m.Position.X, m.Position.Y, m.Radius * 2.2f, m.Radius * 1.4f);
                Raylib.DrawRectanglePro(rect, new Vector2(m.Radius, m.Radius * 0.7f), m.Spin + sway * 30f, WithAlpha(m.Color, 0.55f * alphaMult));
            }
            else
            {
                float twinkle = 0.3f + (MathF.Sin(time * 2.5f + m.Phase) * 0.5f + 0.5f) * 0.5f;
                if (alphaMult >= 0.99f)
                {
                    DrawGlowFast(m.Position, m.Radius * 2.5f, m.Color, twinkle * 0.25f);
                }
                else
                {
                    Raylib.DrawCircleV(m.Position, m.Radius * 1.4f, WithAlpha(m.Color, twinkle * 0.35f * alphaMult));
                }

                Raylib.DrawCircleV(m.Position, m.Radius, WithAlpha(m.Color, twinkle * 0.85f * alphaMult));
            }
        }
    }

    static Camera2D ComputeCamera()
    {
        float clamped = Math.Clamp(trauma, 0f, 1f);
        float s = clamped * clamped * clamped * shakeScale * (reduceMotion ? 0.35f : 1f);
        float t = frameTime > 0f ? frameTime : (float)Raylib.GetTime();

        float hf = MathF.Sin(t * 61f) * 0.65f + MathF.Sin(t * 89f + 2.1f) * 0.35f;
        float hf2 = MathF.Sin(t * 71f + 1.3f) * 0.65f + MathF.Sin(t * 103f + 4.7f) * 0.35f;
        float ox = MaxShakeOffset * s * hf;
        float oy = MaxShakeOffset * s * hf2;
        float rot = MaxShakeRotation * s * MathF.Sin(t * 47f + 3.1f);

        if (cameraDashLean > 0.001f && cameraDashLeanDir.LengthSquared() > 0.001f)
        {
            Vector2 d = cameraDashLeanDir;
            float lean = DashCameraLeanDeg * cameraDashLean;
            rot += lean * d.X;
            rot -= lean * d.Y * 0.5f;
        }

        float zoom = 1f + (reduceMotion ? zoomPunch * 0.35f : zoomPunch) + s * 0.065f
            + (reduceMotion ? 0f : adrenaline * (0.022f + glowPulse * 0.014f));

        return new Camera2D
        {
            Offset = new Vector2(WindowWidth / 2f + ox, WindowHeight / 2f + oy),
            Target = new Vector2(WindowWidth / 2f, WindowHeight / 2f),
            Rotation = rot,
            Zoom = zoom,
        };
    }

    static int GfxRenderScale => uhdEnabled ? UhdRenderScale : 1;
    static int GfxRenderWidth => WindowWidth * GfxRenderScale;
    static int GfxRenderHeight => WindowHeight * GfxRenderScale;

    static float GfxS(float logical) => logical * GfxRenderScale;

    static Camera2D ComputeSceneCamera()
    {
        Camera2D cam = ComputeCamera();
        int scale = GfxRenderScale;
        if (scale <= 1) return cam;

        float ox = cam.Offset.X - WindowWidth / 2f;
        float oy = cam.Offset.Y - WindowHeight / 2f;
        cam.Offset = new Vector2(GfxRenderWidth / 2f + ox * scale, GfxRenderHeight / 2f + oy * scale);
        cam.Zoom *= scale;
        return cam;
    }

}
