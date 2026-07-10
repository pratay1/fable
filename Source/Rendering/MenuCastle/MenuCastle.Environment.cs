partial class Program
{
    // -------------------------------------------------------------------------
    // Additional horizon and wall surface detail
    // -------------------------------------------------------------------------

    static void DrawMenuCastleHorizonMist(float wallY, MenuCastlePalette p, float time)
    {
        for (int layer = 0; layer < 5; layer++)
        {
            float y = wallY - 70f + layer * 14f;
            float alpha = 0.08f + layer * 0.04f;
            float drift = MathF.Sin(time * 0.15f + layer) * 12f;
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wx = WindowWidth * (wisp / 5f) + drift + Hash(layer * 11 + wisp) * 40f;
                DrawEllipticalGlow(new Vector2(wx, y), 60f + layer * 10f, 14f, layer * 4f, p.MoonGlow, alpha, 2);
            }
        }
    }

    static void DrawMenuCastleSilhouetteLayer(float wallY, float wallH, MenuCastlePalette p, float time)
    {
        float sway = MathF.Sin(time * 0.22f) * 1.2f;
        float fogPulse = 0.26f + MathF.Sin(time * 0.38f) * 0.04f;

        var farKeep = new Rectangle(WindowWidth * 0.30f + sway, wallY - 82f, WindowWidth * 0.40f, 58f);
        Raylib.DrawRectangleRounded(farKeep, 0.04f, 4, WithAlpha(ForestShadow, 0.5f));
        DrawBattlements(new Rectangle(farKeep.X - 5f, farKeep.Y - 11f, farKeep.Width + 10f, 11f),
            p.StoneDeep, p.StoneMid, p.StoneHi, 0.65f, time, true);
        // Twin flanking towers on the distant keep
        Raylib.DrawRectangleRounded(new Rectangle(farKeep.X + 8f, farKeep.Y - 38f, 34f, 42f), 0.08f, 4, WithAlpha(ForestShadow, 0.42f));
        Raylib.DrawRectangleRounded(new Rectangle(farKeep.X + farKeep.Width - 42f, farKeep.Y - 38f, 34f, 42f), 0.08f, 4, WithAlpha(ForestShadow, 0.42f));
        DrawTowerRoof(farKeep.X + 25f, farKeep.Y - 48f, farKeep.Y - 4f, 16f, p.StoneDeep, p.StoneMid, p.StoneHi);
        DrawTowerRoof(farKeep.X + farKeep.Width - 25f, farKeep.Y - 48f, farKeep.Y - 4f, 16f, p.StoneDeep, p.StoneMid, p.StoneHi);

        Raylib.DrawRectangleRounded(new Rectangle(-14f, wallY - 42f, 152f, wallH + 74f), 0.05f, 6, WithAlpha(ForestShadow, 0.32f));
        Raylib.DrawRectangleRounded(new Rectangle(WindowWidth - 140f, wallY - 42f, 152f, wallH + 74f), 0.05f, 6, WithAlpha(ForestShadow, 0.32f));

        Raylib.DrawRectangleGradientV(0, (int)(wallY - 32f), WindowWidth, 64,
            WithAlpha(p.StoneDeep, 0f), WithAlpha(ForestShadow, fogPulse));

        for (int spire = 0; spire < 3; spire++)
        {
            float sx = WindowWidth * (0.18f + spire * 0.32f) + MathF.Sin(time * 0.3f + spire) * 4f;
            float sy = wallY - 18f - spire * 6f;
            Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx, sy - 28f - Hash(spire * 7) * 14f), 2f, WithAlpha(ForestShadow, 0.38f));
            Raylib.DrawCircleV(new Vector2(sx, sy - 30f), 2.5f, WithAlpha(p.StoneDeep, 0.45f));
        }

        // Distant village embers below the treeline
        for (int hut = 0; hut < 12; hut++)
        {
            float hx = WindowWidth * (0.05f + hut * 0.08f) + Hash(hut * 29) * 20f;
            float hy = wallY - 4f + Hash(hut * 31) * 6f;
            Raylib.DrawRectangle((int)hx, (int)(hy - 6f), 6, 5, WithAlpha(ForestShadow, 0.45f));
            float tw = MathF.Sin(time * 2.2f + hut * 1.4f) * 0.5f + 0.5f;
            Raylib.DrawCircleV(new Vector2(hx + 3f, hy - 8f), 1.2f, WithAlpha(p.TorchWarm, 0.12f + tw * 0.18f));
        }
    }

    static void DrawMenuCastleWallWalkway(MenuCastleLayout L, MenuCastlePalette p)
    {
        float walkY = L.ParapetY + 2f;
        float walkH = 8f;
        DrawMenuCastleWallWalkwaySegment(new Rectangle(0f, walkY, L.GatehouseLeft - 14f, walkH), p);
        DrawMenuCastleWallWalkwaySegment(new Rectangle(L.GatehouseRight + 14f, walkY, WindowWidth - L.GatehouseRight - 14f, walkH), p);
    }

    static void DrawMenuCastleWallWalkwaySegment(Rectangle segment, MenuCastlePalette p)
    {
        if (segment.Width < 8f) return;
        Raylib.DrawRectangle((int)segment.X, (int)segment.Y, (int)segment.Width, (int)segment.Height, WithAlpha(p.StoneDark, 0.75f));
        int pavers = Math.Max(4, (int)(segment.Width / 20f));
        for (int paver = 0; paver < pavers; paver++)
        {
            float px = segment.X + paver * (segment.Width / pavers);
            float n = Hash(paver * 19);
            Color c = LerpColor(p.StoneDark, p.StoneMid, n * 0.4f);
            Raylib.DrawRectangle((int)px, (int)(segment.Y + 1f), (int)(segment.Width / pavers) - 1, (int)segment.Height - 2, WithAlpha(c, 0.5f));
        }
        Raylib.DrawLine((int)segment.X, (int)segment.Y, (int)(segment.X + segment.Width), (int)segment.Y, WithAlpha(p.StoneHi, 0.15f));
    }

    static void DrawMenuCurtainWallArrowSlits(MenuCastleLayout L, MenuCastlePalette p)
    {
        float gateLeft = L.GatehouseLeft - 20f;
        float gateRight = L.GatehouseRight + 20f;
        float[] rows = { L.WallY + L.WallH * 0.14f, L.WallY + L.WallH * 0.26f };
        float inset = MathF.Max(120f, L.TowerW + 24f);

        foreach (float row in rows)
        {
            for (int i = 0; i < 3; i++)
            {
                float leftCol = inset + i * 48f;
                if (leftCol < gateLeft)
                    DrawMenuArrowSlit(new Vector2(leftCol, row), 8f, 30f, p);

                float rightCol = WindowWidth - inset - i * 48f;
                if (rightCol > gateRight)
                    DrawMenuArrowSlit(new Vector2(rightCol, row), 8f, 30f, p);
            }
        }
    }

    static void DrawMenuCastleGatehouseWindows(Rectangle gatehouse, MenuCastlePalette p, float time)
    {
        float rowY = gatehouse.Y + gatehouse.Height * 0.36f;
        float[] cols = { gatehouse.X + gatehouse.Width * 0.22f, gatehouse.X + gatehouse.Width * 0.5f, gatehouse.X + gatehouse.Width * 0.78f };
        for (int w = 0; w < cols.Length; w++)
        {
            var win = new Rectangle(cols[w] - 8f, rowY, 16f, 22f);
            DrawWindowInteriorGlow(win, time, w * 1.7f);
            if (w == 1) DrawMenuCastleStainedGlass(win, time, w * 1.7f);
            Raylib.DrawRectangleRounded(win, 0.2f, 4, WithAlpha(p.StoneDeep, 0.92f));
            Raylib.DrawRectangleRoundedLines(win, 0.2f, 4, WithAlpha(p.StoneHi, 0.4f));
            Raylib.DrawLine((int)(win.X + win.Width / 2f), (int)win.Y, (int)(win.X + win.Width / 2f), (int)(win.Y + win.Height), WithAlpha(p.StoneDeep, 0.5f));
        }
    }

    static void DrawMenuCastleCurtainWallRelief(float wallY, float wallH, MenuCastlePalette p, float time)
    {
        for (int course = 0; course < 4; course++)
        {
            float cy = wallY + wallH * (0.2f + course * 0.18f);
            Raylib.DrawLine(0, (int)cy, WindowWidth, (int)cy, WithAlpha(p.StoneLight, 0.06f + Hash(course * 7) * 0.06f));
        }

        var wallRegion = new Rectangle(0f, wallY, WindowWidth, wallH);
        DrawMenuCastleStoneDetailOrchestrator(wallRegion, p, time);
    }

    static void DrawMenuCastleTowerBasePlinths(float wallY, float wallH, MenuCastlePalette p)
    {
        DrawMenuTowerCorbelTable(-2f, wallY + wallH - 12f, 180f, p);
        DrawMenuTowerCorbelTable(WindowWidth - 178f, wallY + wallH - 12f, 180f, p);
        for (int block = 0; block < 8; block++)
        {
            float bx = block < 4 ? block * 42f : WindowWidth - (block - 3) * 42f;
            Raylib.DrawRectangleRounded(new Rectangle(bx, wallY + wallH - 16f, 38f, 14f), 0.1f, 3, WithAlpha(p.StoneDark, 0.8f));
            Raylib.DrawLine((int)bx, (int)(wallY + wallH - 16f), (int)(bx + 38f), (int)(wallY + wallH - 16f), WithAlpha(p.StoneHi, 0.2f));
        }
    }

    static void DrawMenuCastleStarCluster(Vector2 center, int seed, float time, float radius)
    {
        int count = 6 + (int)(Hash(seed) * 8f);
        for (int i = 0; i < count; i++)
        {
            float ang = Hash(seed + i * 13) * MathF.PI * 2f;
            float dist = Hash(seed + i * 17) * radius;
            Vector2 pos = new Vector2(center.X + MathF.Cos(ang) * dist, center.Y + MathF.Sin(ang) * dist);
            float tw = MathF.Sin(time * 2f + i + seed) * 0.5f + 0.5f;
            Color sc = LerpColor(new Color(196, 194, 188, 255), new Color(255, 236, 210, 255), Hash(seed + i * 23));
            Raylib.DrawCircleV(pos, 0.8f + Hash(seed + i * 29), WithAlpha(sc, 0.1f + tw * 0.2f));
        }
    }

    static void DrawMenuCastleNightSkyExtras(float time)
    {
        Vector2[] clusters =
        {
            new Vector2(WindowWidth * 0.18f, WindowHeight * 0.12f),
            new Vector2(WindowWidth * 0.42f, WindowHeight * 0.08f),
            new Vector2(WindowWidth * 0.58f, WindowHeight * 0.15f),
            new Vector2(WindowWidth * 0.88f, WindowHeight * 0.1f),
        };
        for (int c = 0; c < clusters.Length; c++)
            DrawMenuCastleStarCluster(clusters[c], c * 97 + 13, time, 28f + c * 6f);

        DrawMenuCastleConstellationMap(time);

        // Faint atmospheric band just above the horizon - warm charcoal, not blue.
        int bandY = (int)(WindowHeight * 0.46f);
        Raylib.DrawRectangleGradientV(0, bandY - 20, WindowWidth, 40,
            WithAlpha(new Color(6, 5, 5, 255), 0f), WithAlpha(new Color(10, 9, 8, 255), 0.22f));
    }

    static void DrawMenuCastleDrawbridgeShadow(float gateX, float gateW, float gateY, float gateH, MenuCastlePalette p)
    {
        var shadow = new Rectangle(gateX - 12f, gateY + gateH - 6f, gateW + 24f, 14f);
        DrawGradientWash(shadow, WithAlpha(p.StoneDeep, 0.45f), WithAlpha(p.StoneDeep, 0f), new Vector2(0.5f, 0f), 1.4f);
    }

    static void DrawMenuCastlePortcullisShadow(Vector2 gateOrigin, float width, float height, MenuCastlePalette p)
    {
        for (int bar = 0; bar < 9; bar++)
        {
            float bx = gateOrigin.X + 10f + bar * ((width - 20f) / 8f);
            var barShadow = new Rectangle(bx - 2f, gateOrigin.Y + height * 0.2f, 4f, height * 0.72f);
            DrawGradientWash(barShadow, WithAlpha(p.StoneDeep, 0.25f), WithAlpha(p.StoneDeep, 0f), new Vector2(0.5f, 0f), 1.2f);
        }
    }

    static void DrawMenuCastleRampartTorches(MenuCastleLayout L, float time, MenuCastlePalette p)
    {
        ReadOnlySpan<float> positions = [0.10f, 0.24f, 0.76f, 0.90f];
        for (int i = 0; i < positions.Length; i++)
        {
            float x = WindowWidth * positions[i];
            if (x > L.GatehouseLeft - 30f && x < L.GatehouseRight + 30f) continue;
            Vector2 pos = new Vector2(x, L.ParapetY - 18f);
            DrawCastleTorch(pos, time, i * 2.1f, 0.75f, TorchMountKind.Rampart);
            DrawWallTorchWash(pos, new Vector2(0f, 1f), time, i * 2.1f, 0.75f);
        }
    }

    static void DrawMenuCastleIronworkDetail(Rectangle region, MenuCastlePalette p, int seed)
    {
        for (int rivet = 0; rivet < 16; rivet++)
        {
            float rx = region.X + Hash(seed + rivet * 3) * region.Width;
            float ry = region.Y + Hash(seed + rivet * 5) * region.Height;
            Raylib.DrawCircleV(new Vector2(rx, ry), 1.1f + Hash(seed + rivet * 7) * 0.6f, WithAlpha(p.Iron, 0.45f + Hash(seed + rivet * 11) * 0.2f));
            if (Hash(seed + rivet * 13) > 0.65f)
            {
                Raylib.DrawLineEx(new Vector2(rx - 2f, ry), new Vector2(rx + 2f, ry), 0.6f, WithAlpha(p.StoneHi, 0.25f));
                Raylib.DrawLineEx(new Vector2(rx, ry - 2f), new Vector2(rx, ry + 2f), 0.6f, WithAlpha(Darken(p.Iron, 0.2f), 0.2f));
            }
        }
    }

    static void DrawMenuCastleErosionStreaks(Rectangle region, MenuCastlePalette p, float time)
    {
        for (int streak = 0; streak < 20; streak++)
        {
            float sx = region.X + Hash(streak * 41 + 2) * region.Width;
            float sy = region.Y + Hash(streak * 43 + 4) * region.Height * 0.4f;
            float length = 16f + Hash(streak * 47) * 48f;
            float drip = MathF.Sin(time * 0.2f + streak) * 2f;
            Color streakCol = LerpColor(p.WetSheen, p.StoneDeep, Hash(streak * 53) * 0.5f);
            for (int seg = 0; seg < 5; seg++)
            {
                float t0 = seg / 5f;
                float t1 = (seg + 1) / 5f;
                Vector2 a = new Vector2(sx + drip * t0, sy + length * t0);
                Vector2 b = new Vector2(sx + drip * t1 + Hash(streak + seg) * 2f, sy + length * t1);
                Raylib.DrawLineEx(a, b, 0.8f, WithAlpha(streakCol, 0.12f - seg * 0.015f));
            }
        }
    }

    static void DrawMenuCastleLintelShadows(Rectangle gatehouse, MenuCastlePalette p)
    {
        for (int lintel = 0; lintel < 5; lintel++)
        {
            float ly = gatehouse.Y + gatehouse.Height * (0.18f + lintel * 0.14f);
            var shadow = new Rectangle(gatehouse.X + 4f, ly, gatehouse.Width - 8f, 6f);
            DrawGradientWash(shadow, WithAlpha(p.StoneDeep, 0.35f), WithAlpha(p.StoneDeep, 0f), new Vector2(0.5f, 0f), 1.3f);
            Raylib.DrawLine((int)gatehouse.X, (int)ly, (int)(gatehouse.X + gatehouse.Width), (int)ly, WithAlpha(p.StoneHi, 0.1f));
        }
    }

    static void DrawMenuCastleCourtyardDepth(float gateX, float gateW, float gateY, float gateH, MenuCastlePalette p, float time)
    {
        var court = new Rectangle(gateX + gateW * 0.2f, gateY + gateH * 0.25f, gateW * 0.6f, gateH * 0.55f);
        float pulse = MathF.Sin(time * 2.8f) * 0.5f + 0.5f;
        DrawGradientWash(court, WithAlpha(p.TorchWarm, 0.1f + pulse * 0.08f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0.85f), 2.4f);
        for (int slab = 0; slab < 6; slab++)
        {
            float sy = court.Y + slab * (court.Height / 6f);
            Raylib.DrawLine((int)court.X, (int)sy, (int)(court.X + court.Width), (int)sy, WithAlpha(p.StoneDeep, 0.15f));
        }
    }

}
