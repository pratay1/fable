partial class Program
{
    // -------------------------------------------------------------------------
    // Menu castle - production master suite (backdrop / architecture / finalize)
    // -------------------------------------------------------------------------

    enum MenuCastleProductionPhase { Backdrop, Architecture, Finalize }

    static void DrawMenuCastleProductionMasterPass(MenuCastleProductionPhase phase, float time, MenuCastlePalette p)
    {
        MenuCastleLayout L = MenuCastleLayoutCurrent;
        switch (phase)
        {
            case MenuCastleProductionPhase.Backdrop:
                DrawMenuCastleApproachCurtainWalls(L, p, time);
                DrawMenuCastleOuterPalisadeHints(L, p, time);
                break;
            case MenuCastleProductionPhase.Architecture:
                DrawMenuCastleTowerBartizans(L, p, time);
                DrawMenuCastleWallHoardings(L, p, time);
                DrawMenuCastleInnerKeepDepth(L.GateX, L.GateW, L.GateY, L.GateH, p, time);
                DrawMenuCastleInnerCurtainWings(L, p, time);
                DrawMenuCastlePortcullisOverlay(new Vector2(L.GateX, L.GateY), L.GateW, L.GateH, p, time);
                DrawMenuCastleExtraChimneyStacks(L, p, time);
                DrawMenuCastleParapetCauldrons(L, time, p);
                DrawMenuCastleSentrySilhouettes(L, time, p);
                DrawMenuCastleForecourtProps(L, p, time);
                DrawMenuCastleBannerRigging(L, time, p);
                DrawMenuCastleRavensAndOwls(time, L.WallY);
                DrawMenuCastleGateMurderHoleDrips(L.GateX, L.GateW, L.GateY, L.GateH, p, time);
                break;
            case MenuCastleProductionPhase.Finalize:
                DrawMenuCastleGlobalWeatheringNetwork(new Rectangle(0f, L.ParapetY - 54f, WindowWidth, L.WallH + 54f), p, time);
                DrawMenuCastleWetSpecularHighlights(L, p, time);
                DrawMenuCastleMoonLensFlare(time);
                DrawMenuCastleProductionLighting(L, p, time);
                DrawMenuCastleProductionColorGrade(time, p);
                DrawMenuCastleProductionFilmGrain(time);
                break;
            default:
                throw new UnreachableException();
        }
    }

    static void DrawMenuCastleConstellationMap(float time)
    {
        ReadOnlySpan<(float x, float y, float r)> anchors =
        [
            (0.14f, 0.11f, 38f),
            (0.38f, 0.07f, 32f),
            (0.62f, 0.13f, 36f),
            (0.84f, 0.09f, 28f),
        ];
        for (int a = 0; a < anchors.Length; a++)
        {
            var (xf, yf, rad) = anchors[a];
            Vector2 center = new Vector2(WindowWidth * xf, WindowHeight * yf);
            int nodes = 5 + (int)(Hash(a * 61) * 4f);
            Vector2[] pts = new Vector2[nodes];
            for (int n = 0; n < nodes; n++)
            {
                float ang = Hash(a * 71 + n * 17) * MathF.PI * 2f;
                float dist = Hash(a * 73 + n * 19) * rad;
                pts[n] = center + new Vector2(MathF.Cos(ang) * dist, MathF.Sin(ang) * dist);
                float tw = MathF.Sin(time * 0.7f + a + n) * 0.5f + 0.5f;
                Raylib.DrawCircleV(pts[n], 0.9f + Hash(a + n * 3), WithAlpha(new Color(210, 216, 232, 255), 0.08f + tw * 0.14f));
            }
            for (int e = 0; e < nodes - 1; e++)
            {
                if (Hash(a * 79 + e * 11) > 0.42f)
                    Raylib.DrawLineEx(pts[e], pts[e + 1], 0.6f, WithAlpha(new Color(160, 168, 188, 255), 0.06f));
            }
            if (nodes > 3 && Hash(a * 83) > 0.5f)
                Raylib.DrawLineEx(pts[0], pts[nodes - 1], 0.55f, WithAlpha(new Color(160, 168, 188, 255), 0.05f));
        }
    }

    static void DrawMenuCastleApproachCurtainWalls(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float approachH = L.WallH * 0.18f;
        float approachY = L.ForecourtY - approachH;
        float pathHalf = L.GateW * 0.72f;
        float pathCenter = L.CenterX;

        for (int side = 0; side < 2; side++)
        {
            float wx = side == 0 ? 0f : pathCenter + pathHalf + 8f;
            float ww = side == 0 ? pathCenter - pathHalf - 8f : WindowWidth - wx;
            if (ww < 40f) continue;
            var wall = new Rectangle(wx, approachY, ww, approachH);
            Raylib.DrawRectangleRounded(wall, 0.03f, 4, WithAlpha(p.StoneDeep, 0.52f));
            DrawMenuMasonryUltra(wall, p, 2, 18 + side * 7);
            DrawBattlements(new Rectangle(wall.X - 2f, wall.Y - 10f, wall.Width + 4f, 10f),
                p.StoneDark, p.StoneLight, p.StoneHi, 0.7f, time, side == 0);
            float tx = side == 0 ? wall.X + wall.Width - 18f : wall.X + 10f;
            var turret = new Rectangle(tx - 10f, wall.Y - 16f, 20f, approachH + 12f);
            DrawMenuCastleMiniTowerSeparationShadow(turret, 20f, p);
            Raylib.DrawRectangleRounded(turret, 0.1f, 3, WithAlpha(p.StoneMid, 0.58f));
            DrawTowerRoof(tx, turret.Y - 14f, turret.Y + 2f, 10f, p.StoneDark, p.StoneLight, p.StoneHi);
        }
        Raylib.DrawRectangleGradientV(0, (int)(approachY - 6f), WindowWidth, 12, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.18f));
    }

    static void DrawMenuCastleOuterPalisadeHints(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float baseY = L.MoatY + 6f;
        for (int stake = 0; stake < 48; stake++)
        {
            float sx = stake * (WindowWidth / 47f) + MathF.Sin(time * 0.05f + stake * 0.2f);
            float sh = 8f + Hash(stake * 29) * 7f;
            Color wood = new Color(34, 30, 26, 255);
            Raylib.DrawLineEx(new Vector2(sx, baseY), new Vector2(sx + (Hash(stake * 31) - 0.5f) * 2f, baseY - sh), 1.2f, WithAlpha(wood, 0.24f + Hash(stake * 37) * 0.1f));
        }
    }

    static void DrawMenuCastleTowerBartizans(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        ReadOnlySpan<(Vector2 origin, bool left)> towers = [(L.LeftTowerOrigin, true), (L.RightTowerOrigin, false)];
        for (int t = 0; t < towers.Length; t++)
        {
            var (origin, left) = towers[t];
            float ty = origin.Y + L.TowerH * 0.16f;
            float bx = left ? origin.X + L.TowerW - 24f : origin.X + 4f;
            var bart = new Rectangle(bx, ty, 26f, 32f);
            DrawMenuCastleMiniTowerSeparationShadow(bart, 18f, p);
            Raylib.DrawRectangleRounded(bart, 0.12f, 3, WithAlpha(p.StoneMid, 0.9f));
            DrawMenuMasonryUltra(bart, p, 2, 40 + t * 11);
            DrawBattlements(new Rectangle(bart.X - 2f, bart.Y - 8f, bart.Width + 4f, 8f),
                p.StoneDark, p.StoneLight, p.StoneHi, 0.75f, time, left);
            DrawMenuArrowSlit(new Vector2(bart.X + bart.Width * 0.5f - 3f, bart.Y + 10f), 6f, 16f, p);
            DrawMenuCastleSculptedGargoyle(new Vector2(bart.X + (left ? bart.Width + 2f : -2f), bart.Y + 8f), !left, p, time);
        }
    }

    static void DrawMenuCastleWallHoardings(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        Color timber = new Color(42, 36, 30, 255);
        Color timberHi = new Color(58, 50, 42, 255);
        float plankY = L.ParapetY - 12f;
        int spans = 12;
        for (int span = 0; span < spans; span++)
        {
            float sx = 32f + span * ((WindowWidth - 64f) / spans);
            float sw = (WindowWidth - 64f) / spans - 2f;
            if (sx + sw > L.GatehouseLeft - 8f && sx < L.GatehouseRight + 8f) continue;
            float sway = MathF.Sin(time * 0.35f + span * 0.5f) * 0.8f;
            var plank = new Rectangle(sx, plankY + sway, sw, 12f);
            Raylib.DrawRectangleRounded(plank, 0.06f, 2, WithAlpha(LerpColor(timber, timberHi, Hash(span * 7) * 0.4f), 0.62f));
            Raylib.DrawLineEx(new Vector2(plank.X, plank.Y + plank.Height), new Vector2(plank.X + plank.Width, plank.Y + plank.Height + 3f),
                1f, WithAlpha(ForestShadow, 0.35f));
        }
    }

    static void DrawMenuCastleInnerKeepDepth(float gateX, float gateW, float gateY, float gateH, MenuCastlePalette p, float time)
    {
        var keep = new Rectangle(gateX + gateW * 0.08f, gateY + gateH * 0.12f, gateW * 0.84f, gateH * 0.72f);
        Raylib.DrawRectangleRounded(keep, 0.04f, 4, WithAlpha(p.StoneDeep, 0.82f));
        DrawMenuMasonryUltra(keep, p, 2, 55);
        DrawBattlements(new Rectangle(keep.X - 3f, keep.Y - 10f, keep.Width + 6f, 10f), p.StoneDark, p.StoneLight, p.StoneHi, 0.8f, time, true);
        float pulse = MathF.Sin(time * 1.5f) * 0.5f + 0.5f;
        for (int win = 0; win < 3; win++)
        {
            float wx = keep.X + keep.Width * (0.2f + win * 0.3f);
            var window = new Rectangle(wx - 5f, keep.Y + keep.Height * 0.28f, 10f, 16f);
            DrawWindowInteriorGlow(window, time, win * 2.1f);
            Raylib.DrawRectangleRounded(window, 0.2f, 2, WithAlpha(p.StoneDeep, 0.9f));
            DrawEllipticalGlow(new Vector2(wx, window.Y + window.Height * 0.5f), 8f, 12f, 0f, p.TorchWarm, 0.008f + pulse * 0.006f, 2);
        }
        Raylib.DrawLineEx(new Vector2(keep.X + keep.Width * 0.5f, keep.Y + keep.Height * 0.55f),
            new Vector2(keep.X + keep.Width * 0.5f, keep.Y + keep.Height * 0.92f), 2f, WithAlpha(p.StoneHi, 0.15f));
    }

    static void DrawMenuCastlePortcullisOverlay(Vector2 gateOrigin, float width, float height, MenuCastlePalette p, float time)
    {
        float grateTop = gateOrigin.Y + height * 0.16f;
        float grateBot = gateOrigin.Y + height * 0.88f;
        float lift = MathF.Sin(time * 0.35f) * 1.5f;
        int bars = 11;
        for (int b = 0; b < bars; b++)
        {
            float bx = gateOrigin.X + 8f + b * ((width - 16f) / (bars - 1));
            Raylib.DrawLineEx(new Vector2(bx, grateTop + lift), new Vector2(bx, grateBot + lift), 2.8f, WithAlpha(p.Iron, 0.72f));
            Raylib.DrawLineEx(new Vector2(bx + 0.8f, grateTop + lift), new Vector2(bx + 0.8f, grateBot + lift), 0.8f, WithAlpha(p.StoneHi, 0.22f));
        }
        for (int r = 0; r < 6; r++)
        {
            float gy = grateTop + lift + r * ((grateBot - grateTop) / 5f);
            Raylib.DrawLineEx(new Vector2(gateOrigin.X + 6f, gy), new Vector2(gateOrigin.X + width - 6f, gy), 1.6f, WithAlpha(p.Iron, 0.55f));
        }
        float glint = MathF.Sin(time * 3.2f) * 0.5f + 0.5f;
        for (int g = 0; g < 5; g++)
        {
            if (Hash(g * 19 + (int)(time * 2f)) > 0.78f)
            {
                float gx = gateOrigin.X + 12f + g * ((width - 24f) / 4f);
                float gy = grateTop + lift + Hash(g * 23) * (grateBot - grateTop);
                Raylib.DrawCircleV(new Vector2(gx, gy), 1.4f, WithAlpha(p.StoneHi, 0.35f + glint * 0.25f));
            }
        }
    }

    static void DrawMenuCastleExtraChimneyStacks(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        ReadOnlySpan<float> cols = [0.16f, 0.84f];
        for (int c = 0; c < cols.Length; c++)
        {
            float cx = L.GatehouseX + L.GatehouseW * cols[c];
            float baseY = L.BattlementsY + 4f;
            float chH = 18f + Hash(c * 19) * 6f;
            Raylib.DrawRectangleRounded(new Rectangle(cx - 5f, baseY - chH, 10f, chH), 0.12f, 2, WithAlpha(p.StoneDark, 0.85f));
            Raylib.DrawRectangle((int)(cx - 6f), (int)(baseY - chH - 4f), 12, 4, WithAlpha(p.StoneMid, 0.8f));
            for (int w = 0; w < 4; w++)
            {
                float drift = time * (0.6f + c * 0.15f) + w * 0.8f;
                Vector2 smoke = new Vector2(cx + MathF.Sin(drift) * (4f + w * 2f), baseY - chH - 6f - w * 7f);
                DrawEllipticalGlow(smoke, 6f + w * 2f, 3f + w, Hash(c + w) * 12f, new Color(48, 46, 44, 255), 0.009f - w * 0.001f, 2);
            }
        }
    }

    static void DrawMenuCastleForecourtProps(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float groundY = L.ForecourtY + L.ForecourtH - 4f;
        Color barrel = new Color(46, 38, 32, 255);
        for (int b = 0; b < 3; b++)
        {
            float bx = L.GateX - 52f + b * 16f;
            Raylib.DrawRectangleRounded(new Rectangle(bx, groundY - 14f, 12f, 14f), 0.25f, 3, WithAlpha(barrel, 0.75f));
        }
        float cartX = L.GateX + L.GateW + 28f;
        Raylib.DrawRectangleRounded(new Rectangle(cartX, groundY - 10f, 26f, 10f), 0.1f, 2, WithAlpha(barrel, 0.65f));
        Raylib.DrawCircle((int)(cartX + 4f), (int)(groundY + 1f), 4, WithAlpha(p.Iron, 0.55f));
        Raylib.DrawCircle((int)(cartX + 20f), (int)(groundY + 1f), 4, WithAlpha(p.Iron, 0.55f));
        float wellX = L.GateX - 72f;
        Raylib.DrawCircleV(new Vector2(wellX, groundY - 4f), 10f, WithAlpha(p.StoneDark, 0.7f));
        Raylib.DrawCircleLines((int)wellX, (int)(groundY - 4f), 10, WithAlpha(p.StoneHi, 0.25f));
        Raylib.DrawLineEx(new Vector2(wellX, groundY - 14f), new Vector2(wellX, groundY - 24f + MathF.Sin(time * 0.4f) * 2f), 1.2f, WithAlpha(p.StoneLight, 0.35f));
    }

    static void DrawMenuCastleRavensAndOwls(float time, float wallY)
    {
        ReadOnlySpan<(float x, float y, float phase, bool owl)> birds =
        [
            (0.12f, 0.18f, 0f, false),
            (0.22f, 0.14f, 1.2f, false),
            (0.78f, 0.16f, 2.4f, true),
            (0.88f, 0.12f, 3.6f, false),
        ];
        for (int b = 0; b < birds.Length; b++)
        {
            var (xf, yf, phase, owl) = birds[b];
            float flap = MathF.Sin(time * (owl ? 1.2f : 4.5f) + phase) * (owl ? 2f : 5f);
            Vector2 pos = new Vector2(WindowWidth * xf + MathF.Sin(time * 0.3f + phase) * 20f, WindowHeight * yf + MathF.Cos(time * 0.25f + phase) * 8f);
            Color body = owl ? new Color(62, 58, 52, 255) : new Color(28, 26, 24, 255);
            Raylib.DrawCircleV(pos, owl ? 3.5f : 2.5f, WithAlpha(body, 0.65f));
            Raylib.DrawLineEx(pos + new Vector2(-7f, -1f), pos + new Vector2(-2f - flap * 0.3f, -4f - flap * 0.2f), 1.2f, WithAlpha(body, 0.7f));
            Raylib.DrawLineEx(pos + new Vector2(7f, -1f), pos + new Vector2(2f + flap * 0.3f, -4f - flap * 0.2f), 1.2f, WithAlpha(body, 0.7f));
            if (owl)
            {
                Raylib.DrawCircleV(pos + new Vector2(-1.5f, 0f), 1f, WithAlpha(Gold, 0.45f));
                Raylib.DrawCircleV(pos + new Vector2(1.5f, 0f), 1f, WithAlpha(Gold, 0.45f));
            }
        }
        if (Hash((int)(time * 8f)) > 0.7f)
        {
            Vector2 raven = new Vector2(WindowWidth * 0.5f + MathF.Sin(time) * 120f, wallY - 80f + MathF.Cos(time * 0.7f) * 20f);
            float wing = MathF.Sin(time * 5f) * 6f;
            Raylib.DrawLineEx(raven + new Vector2(-8f, 0f), raven + new Vector2(-wing, -5f), 1.3f, WithAlpha(ForestShadow, 0.55f));
            Raylib.DrawLineEx(raven + new Vector2(8f, 0f), raven + new Vector2(wing, -5f), 1.3f, WithAlpha(ForestShadow, 0.55f));
            Raylib.DrawCircleV(raven, 2f, WithAlpha(ForestShadow, 0.6f));
        }
    }

    static void DrawMenuCastleGateMurderHoleDrips(float gateX, float gateW, float gateY, float gateH, MenuCastlePalette p, float time)
    {
        for (int mh = 0; mh < 4; mh++)
        {
            float mx = gateX + gateW * (0.22f + mh * 0.18f);
            float my = gateY + gateH * 0.06f;
            if (Hash(mh * 31 + (int)(time * 4f)) > 0.62f)
            {
                float drip = (time * 2f + mh) % 1f;
                Raylib.DrawLineEx(new Vector2(mx, my + 8f), new Vector2(mx + MathF.Sin(mh) * 2f, my + 8f + drip * 18f), 0.8f, WithAlpha(p.WetSheen, 0.15f * (1f - drip)));
            }
        }
    }

    static void DrawMenuCastleGlobalWeatheringNetwork(Rectangle region, MenuCastlePalette p, float time)
    {
        for (int crack = 0; crack < 32; crack++)
        {
            int seed = crack * 173 + 4400;
            float px = region.X + Hash(seed) * region.Width;
            float py = region.Y + Hash(seed + 5) * region.Height;
            Vector2 cursor = new Vector2(px, py);
            int segments = 3 + (int)(Hash(seed + 9) * 5f);
            for (int s = 0; s < segments; s++)
            {
                float n = Hash(seed + s * 13);
                float n2 = Hash(seed + s * 17);
                Vector2 next = cursor + new Vector2((n - 0.5f) * 28f, 4f + n2 * 20f);
                Raylib.DrawLineEx(cursor, next, 0.55f + n * 0.35f, WithAlpha(p.StoneDeep, 0.08f + Hash(seed + s) * 0.1f));
                cursor = next;
            }
        }
        for (int pat = 0; pat < 40; pat++)
        {
            float px = region.X + Hash(pat * 59) * region.Width;
            float py = region.Y + Hash(pat * 61) * region.Height;
            DrawGradientWash(new Rectangle(px, py, 4f + Hash(pat * 67) * 8f, 2f), WithAlpha(p.StoneDeep, 0.1f), WithAlpha(p.StoneMid, 0f), new Vector2(0.5f, 0.5f), 1.1f);
        }
    }

    static void DrawMenuCastleProductionLighting(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float key = Math.Clamp(1f - MathF.Abs(L.CenterX - moon.X) / (WindowWidth * 0.55f), 0.25f, 1f);
        Raylib.DrawRectangleGradientH(0, (int)L.WallY, WindowWidth, (int)(L.WallH * 0.5f),
            WithAlpha(p.MoonGlow, 0f), WithAlpha(p.MoonGlow, 0.025f * key));
        var gatePool = new Rectangle(L.GateX - 16f, L.GateY + 8f, L.GateW + 32f, L.GateH * 0.4f);
        float flicker = MathF.Sin(time * 4.2f) * 0.5f + 0.5f;
        DrawGradientWash(gatePool, WithAlpha(p.TorchWarm, 0.04f + flicker * 0.03f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0.3f), 2f);
        for (int rim = 0; rim < 8; rim++)
        {
            float rx = WindowWidth * (rim / 7f);
            if (rx > L.GatehouseLeft - 20f && rx < L.GatehouseRight + 20f) continue;
            DrawEllipticalGlow(new Vector2(rx, L.ParapetY + 2f), 34f, 4f, -3f, p.MoonGlow, 0.006f * key, 2);
        }
    }

    static void DrawMenuCastleProductionColorGrade(float time, MenuCastlePalette p)
    {
        float pulse = 0.94f + MathF.Sin(time * 0.12f) * 0.03f;
        MenuCastleLayout L = MenuCastleLayoutCurrent;
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, (int)(WindowHeight * 0.18f),
            WithAlpha(new Color(12, 10, 14, 255), 0.18f * pulse), WithAlpha(new Color(12, 10, 14, 255), 0f));
        Raylib.DrawRectangleGradientV(0, (int)(WindowHeight * 0.82f), WindowWidth, (int)(WindowHeight * 0.18f),
            WithAlpha(new Color(14, 12, 10, 255), 0f), WithAlpha(new Color(18, 14, 10, 255), 0.14f * pulse));
        Raylib.DrawRectangleGradientH(0, 0, (int)(WindowWidth * 0.08f), WindowHeight,
            WithAlpha(new Color(8, 8, 10, 255), 0.12f), WithAlpha(new Color(8, 8, 10, 255), 0f));
        Raylib.DrawRectangleGradientH((int)(WindowWidth * 0.92f), 0, (int)(WindowWidth * 0.08f), WindowHeight,
            WithAlpha(new Color(8, 8, 10, 255), 0f), WithAlpha(new Color(8, 8, 10, 255), 0.12f));
        Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(new Color(58, 52, 46, 255), 0.018f));
        Raylib.DrawRectangleGradientV((int)L.WallY, (int)L.WallY, WindowWidth, (int)(L.WallH * 0.72f),
            WithAlpha(p.HeraldRedDeep, 0f), WithAlpha(p.HeraldRedDeep, 0.028f * pulse));
    }

    static void DrawMenuCastleInnerCurtainWings(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float wingW = L.GateW * 0.14f;
        float wingH = L.GateH * 0.72f;
        float wingY = L.GateY + L.GateH * 0.12f;
        for (int side = 0; side < 2; side++)
        {
            float wx = side == 0 ? L.GateX + L.GateW * 0.04f : L.GateX + L.GateW * 0.82f;
            var wing = new Rectangle(wx, wingY, wingW, wingH);
            Raylib.DrawRectangleRounded(wing, 0.06f, 3, WithAlpha(p.StoneDeep, 0.72f));
            DrawMenuMasonryUltra(wing, p, 2, 90 + side * 13);
            DrawMenuArrowSlit(new Vector2(wing.X + wing.Width * 0.5f - 3f, wing.Y + wing.Height * 0.42f), 5f, 14f, p);
        }
    }

    static void DrawMenuCastleParapetCauldrons(MenuCastleLayout L, float time, MenuCastlePalette p)
    {
        ReadOnlySpan<float> positions = [0.12f, 0.30f, 0.70f, 0.88f];
        for (int i = 0; i < positions.Length; i++)
        {
            float x = WindowWidth * positions[i];
            if (x > L.GatehouseLeft - 24f && x < L.GatehouseRight + 24f) continue;
            Vector2 pos = new Vector2(x, L.ParapetY - 16f);
            Raylib.DrawCircleV(pos + new Vector2(0f, 4f), 7f, WithAlpha(p.StoneDark, 0.75f));
            Raylib.DrawCircleV(pos, 5f, WithAlpha(p.Iron, 0.6f));
            float flicker = MathF.Sin(time * 3.5f + i * 1.7f) * 0.5f + 0.5f;
            DrawEllipticalGlow(pos + new Vector2(0f, -2f), 10f, 14f, 0f, p.TorchWarm, 0.012f + flicker * 0.01f, 2);
        }
    }

    static void DrawMenuCastleSentrySilhouettes(MenuCastleLayout L, float time, MenuCastlePalette p)
    {
        ReadOnlySpan<float> positions = [0.18f, 0.82f];
        for (int s = 0; s < positions.Length; s++)
        {
            float sway = MathF.Sin(time * 0.45f + s * 1.6f) * 1.5f;
            Vector2 basePos = new Vector2(WindowWidth * positions[s], L.ParapetY - 6f + sway);
            Raylib.DrawCircleV(basePos + new Vector2(0f, -10f), 3f, WithAlpha(ForestShadow, 0.55f));
            Raylib.DrawLineEx(basePos + new Vector2(0f, -7f), basePos + new Vector2(0f, 4f), 2f, WithAlpha(ForestShadow, 0.5f));
            Raylib.DrawLineEx(basePos + new Vector2(0f, -2f), basePos + new Vector2(-5f, 3f), 1.5f, WithAlpha(ForestShadow, 0.45f));
        }
    }

    static void DrawMenuCastleBannerRigging(MenuCastleLayout L, float time, MenuCastlePalette p)
    {
        float poleX = L.CenterX;
        float poleTop = L.BattlementsY + 2f;
        float wave = MathF.Sin(time * 1.8f) * 3f;
        Raylib.DrawLineEx(new Vector2(poleX, poleTop + 36f), new Vector2(poleX + 26f + wave, poleTop + 8f), 0.8f, WithAlpha(p.StoneLight, 0.35f));
        Raylib.DrawLineEx(new Vector2(poleX, poleTop + 40f), new Vector2(poleX - 20f - wave * 0.7f, poleTop + 16f), 0.7f, WithAlpha(p.StoneMid, 0.3f));
    }

    static void DrawMenuCastleWetSpecularHighlights(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        for (int streak = 0; streak < 28; streak++)
        {
            float sx = Hash(streak * 47) * WindowWidth;
            float sy = L.WallY + Hash(streak * 53) * L.WallH;
            float face = Math.Clamp(1f - MathF.Abs(sx - moon.X) / 320f, 0.08f, 1f);
            float tw = MathF.Sin(time * 2.2f + streak) * 0.5f + 0.5f;
            if (tw > 0.72f)
                Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + 6f, sy - 3f), 0.9f, WithAlpha(p.WetSheen, 0.1f * face));
        }
        for (int cob = 0; cob < 18; cob++)
        {
            float cx = L.GateX - 36f + Hash(cob * 61) * (L.GateW + 72f);
            float cy = L.ForecourtY + 2f + Hash(cob * 67) * (L.ForecourtH - 4f);
            if (MathF.Sin(time * 3.1f + cob) > 0.65f)
                Raylib.DrawCircleV(new Vector2(cx, cy), 0.9f, WithAlpha(p.WetSheen, 0.16f));
        }
    }

    static void DrawMenuCastleMoonLensFlare(float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float pulse = 0.85f + MathF.Sin(time * 0.22f) * 0.15f;
        ReadOnlySpan<float> offsets = [-180f, -95f, -42f, 38f, 112f, 210f];
        for (int i = 0; i < offsets.Length; i++)
        {
            float ox = offsets[i];
            float size = 8f + MathF.Abs(ox) * 0.04f;
            Color flare = LerpColor(MenuPalette.MoonGlow, new Color(255, 248, 220, 255), i / (float)(offsets.Length - 1));
            DrawEllipticalGlow(new Vector2(moon.X + ox, moon.Y + ox * 0.02f), size, size * 0.35f, 0f, flare, 0.006f * pulse / (1f + i * 0.15f), 2);
        }
        DrawEllipticalGlow(moon, 42f, 42f, 0f, MenuPalette.MoonGlow, 0.035f * pulse, 4);
    }

    static void DrawMenuCastleProductionFilmGrain(float time)
    {
        for (int grain = 0; grain < 55; grain++)
        {
            if (Hash(grain + (int)(time * 24f)) > 0.78f)
            {
                float gx = Hash(grain * 3 + 1) * WindowWidth;
                float gy = Hash(grain * 5 + 2) * WindowHeight;
                Color tone = Hash(grain * 7) > 0.5f ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 255);
                Raylib.DrawPixel((int)gx, (int)gy, WithAlpha(tone, 0.018f));
            }
        }
    }

}
