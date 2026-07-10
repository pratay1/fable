partial class Program
{
    // -------------------------------------------------------------------------
    // Menu castle - ultimate beauty suite
    // -------------------------------------------------------------------------

    static void DrawMenuCastleGodRays(float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float pulse = 0.82f + MathF.Sin(time * 0.28f) * 0.18f;
        ReadOnlySpan<(float angDeg, float len, float width)> rays =
        [
            (-38f, WindowHeight * 0.72f, 0.22f),
            (-28f, WindowHeight * 0.78f, 0.28f),
            (-18f, WindowHeight * 0.82f, 0.32f),
            (-8f, WindowHeight * 0.76f, 0.24f),
            (2f, WindowHeight * 0.68f, 0.18f),
        ];
        for (int r = 0; r < rays.Length; r++)
        {
            var (angDeg, len, halfAng) = rays[r];
            float ang = angDeg * MathF.PI / 180f;
            float drift = MathF.Sin(time * 0.12f + r * 0.9f) * 0.015f;
            DrawLightCone(moon, ang + drift, len, halfAng, MenuPalette.MoonGlow, (0.018f + r * 0.004f) * pulse);
        }
    }

    static void DrawMenuCastleAuroraVeil(float time)
    {
        int skyH = (int)(WindowHeight * 0.38f);
        float wave = MathF.Sin(time * 0.14f) * 14f;
        for (int band = 0; band < 4; band++)
        {
            float t = band / 3f;
            float cx = WindowWidth * (0.10f + t * 0.58f) + wave;
            float cy = skyH * (0.14f + t * 0.22f);
            Color veil = LerpColor(new Color(10, 12, 10, 255), new Color(14, 12, 11, 255), Hash(band * 17));
            DrawEllipticalGlow(new Vector2(cx, cy), 120f + band * 18f, 14f + band * 3f, -20f + band * 5f,
                veil, 0.004f + Hash(band * 23) * 0.004f, 2);
        }
    }

    static void DrawMenuCastleForestTreeline(float wallY, MenuCastlePalette p, float time)
    {
        Color pine = new Color(8, 12, 9, 255);
        Color pineHi = new Color(14, 20, 14, 255);
        float baseY = wallY - 8f;
        for (int tree = 0; tree < 38; tree++)
        {
            float tx = tree * (WindowWidth / 37f) + MathF.Sin(time * 0.08f + tree * 0.4f) * 2f;
            float h = 28f + Hash(tree * 31) * 42f;
            float w = 10f + Hash(tree * 37) * 14f;
            Raylib.DrawTriangle(
                new Vector2(tx, baseY - h),
                new Vector2(tx - w, baseY + 4f),
                new Vector2(tx + w, baseY + 4f),
                WithAlpha(LerpColor(pine, pineHi, Hash(tree * 41) * 0.35f), 0.55f + Hash(tree * 43) * 0.25f));
            Raylib.DrawRectangle((int)(tx - 2f), (int)(baseY - 2f), 4, (int)(h * 0.35f), WithAlpha(Darken(pine, 0.2f), 0.5f));
        }
        Raylib.DrawRectangleGradientV(0, (int)(baseY - 6f), WindowWidth, 28, WithAlpha(pine, 0f), WithAlpha(pine, 0.35f));
    }

    static void DrawMenuCastleMoatReflection(float wallY, float wallH, MenuCastlePalette p, float time)
    {
        var moat = new Rectangle(0f, wallY + wallH - 6f, WindowWidth, 14f);
        Raylib.DrawRectangleGradientV((int)moat.X, (int)moat.Y, (int)moat.Width, (int)moat.Height,
            WithAlpha(new Color(4, 4, 4, 255), 0.62f), WithAlpha(new Color(2, 2, 2, 255), 0.92f));
        Vector2 moon = MenuCastleMoonPosition;
        float ripple = MathF.Sin(time * 1.2f) * 2f;
        for (int r = 0; r < 8; r++)
        {
            float rx = WindowWidth * (0.12f + r * 0.11f) + ripple * r * 0.3f;
            float ry = moat.Y + moat.Height * 0.45f + MathF.Sin(time * 0.9f + r) * 1.5f;
            DrawEllipticalGlow(new Vector2(rx, ry), 18f + r * 2f, 3f, 0f, p.MoonGlow, 0.006f, 2);
        }
        float reflectX = moon.X * 0.35f + WindowWidth * 0.5f * 0.65f;
        DrawEllipticalGlow(new Vector2(reflectX, moat.Y + 5f), 48f, 6f, 0f, p.MoonGlow, 0.014f + MathF.Sin(time * 0.4f) * 0.004f, 3);
        for (int w = 0; w < 16; w++)
        {
            float wx = Hash(w * 19) * WindowWidth;
            float wy = moat.Y + 3f + Hash(w * 23) * 8f;
            Raylib.DrawLineEx(new Vector2(wx, wy), new Vector2(wx + 12f + MathF.Sin(time + w) * 4f, wy), 0.8f, WithAlpha(p.WetSheen, 0.08f));
        }
    }

    static void DrawMenuCastleIntervalTowers(float wallY, float wallH, float gatehouseX, float gatehouseW, MenuCastlePalette p, float time)
    {
        float gateLeft = gatehouseX - 30f;
        float gateRight = gatehouseX + gatehouseW + 30f;
        ReadOnlySpan<float> offsets = [0.14f, 0.28f, 0.72f, 0.86f];
        for (int i = 0; i < offsets.Length; i++)
        {
            float ox = offsets[i];
            float tx = WindowWidth * ox;
            if (tx > gateLeft - 40f && tx < gateRight + 40f) continue;
            float tw = 36f + Hash(i * 13) * 10f;
            float th = wallH * (0.38f + Hash(i * 17) * 0.12f);
            float ty = wallY + wallH - th - 8f;
            var tower = new Rectangle(tx - tw / 2f, ty, tw, th);
            DrawMenuCastleMiniTowerSeparationShadow(tower, 38f, p);
            Raylib.DrawRectangleRounded(tower, 0.08f, 4, WithAlpha(p.StoneMid, 0.88f));
            DrawMenuMasonryUltra(tower, p, 5, 3);
            DrawBattlements(new Rectangle(tower.X - 2f, tower.Y - 14f, tower.Width + 4f, 14f),
                p.StoneDark, p.StoneLight, p.StoneHi, 0.85f, time, true);
            DrawTowerRoof(tower.X + tower.Width / 2f, tower.Y - 24f, tower.Y + 2f, tw * 0.48f, p.StoneDark, p.StoneLight, p.StoneHi);
            DrawMenuArrowSlit(new Vector2(tower.X + tower.Width * 0.5f - 4f, tower.Y + th * 0.35f), 7f, 22f, p);
        }
    }

    static void DrawMenuCastleChapelSpire(float gatehouseX, float gatehouseW, float gatehouseTop, float time, MenuCastlePalette p)
    {
        float cx = gatehouseX + gatehouseW * 0.78f;
        float baseY = gatehouseTop + 8f;
        var chapel = new Rectangle(cx - 24f, baseY - 52f, 48f, 52f);
        DrawMenuCastleMiniTowerSeparationShadow(chapel, 28f, p);
        Raylib.DrawRectangleRounded(chapel, 0.06f, 4, WithAlpha(p.StoneDeep, 0.72f));
        DrawMenuMasonryUltra(chapel, p, 4, 3);
        DrawTowerRoof(cx, baseY - 72f, baseY - 2f, 18f, p.StoneDark, p.StoneLight, p.StoneHi);
        Raylib.DrawLineEx(new Vector2(cx, baseY - 72f), new Vector2(cx, baseY - 86f), 1.5f, WithAlpha(p.StoneHi, 0.45f));
        Raylib.DrawCircleV(new Vector2(cx, baseY - 88f), 3f, WithAlpha(Gold, 0.35f + MathF.Sin(time * 1.4f) * 0.1f));
        var rose = new Rectangle(cx - 8f, baseY - 32f, 16f, 20f);
        Raylib.DrawCircleV(new Vector2(rose.X + rose.Width / 2f, rose.Y + rose.Height / 2f), 9f, WithAlpha(new Color(48, 28, 32, 255), 0.55f));
        for (int petal = 0; petal < 8; petal++)
        {
            float ang = petal * MathF.PI / 4f;
            Vector2 pc = new Vector2(rose.X + rose.Width / 2f + MathF.Cos(ang) * 7f, rose.Y + rose.Height / 2f + MathF.Sin(ang) * 7f);
            Raylib.DrawCircleV(pc, 2.5f, WithAlpha(new Color(72, 38, 42, 255), 0.45f));
        }
        DrawEllipticalGlow(new Vector2(cx, baseY - 40f), 24f, 32f, 0f, p.TorchWarm, 0.012f + MathF.Sin(time * 1.8f) * 0.006f, 2);
    }

    static void DrawMenuCastleBarbican(MenuCastleLayout L, float time, MenuCastlePalette p)
    {
        var barbican = new Rectangle(L.BarbicanX, L.BarbicanY, L.BarbicanW, L.BarbicanH);
        Raylib.DrawRectangleRounded(barbican, 0.05f, 6, WithAlpha(p.StoneDark, 0.88f));
        DrawMenuMasonryUltra(barbican, p, 3, 9);
        DrawQuoinStripes(barbican, p.StoneLight, p.StoneHi);
        DrawBattlements(new Rectangle(L.BarbicanX - 4f, L.BarbicanY - 12f, L.BarbicanW + 8f, 12f),
            p.StoneDark, p.StoneLight, p.StoneHi, 0.9f, time, true);
        for (int side = 0; side < 2; side++)
        {
            float tx = side == 0 ? L.BarbicanX + 8f : L.BarbicanX + L.BarbicanW - 28f;
            DrawMenuCastleTurret(new Vector2(tx, L.BarbicanY + 6f), 24f, L.BarbicanH - 8f, p, time, side * 2.1f);
        }
        var arch = new Rectangle(L.GateX + L.GateW * 0.22f, L.BarbicanY + L.BarbicanH * 0.35f, L.GateW * 0.56f, L.BarbicanH * 0.55f);
        Raylib.DrawRectangleRounded(arch, 0.45f, 6, WithAlpha(ForestShadow, 0.75f));
        Raylib.DrawRectangleRoundedLines(arch, 0.45f, 6, WithAlpha(p.StoneHi, 0.28f));
    }

    static void DrawMenuCastleSculptedGargoyle(Vector2 pos, bool facingLeft, MenuCastlePalette p, float time)
    {
        float dir = facingLeft ? -1f : 1f;
        float drip = time > 0f ? MathF.Sin(time * 2.4f) * 0.5f : 0f;
        Raylib.DrawCircleV(pos, 5f, WithAlpha(p.StoneDark, 0.92f));
        Raylib.DrawCircleV(new Vector2(pos.X - dir * 2f, pos.Y - 2f), 2f, WithAlpha(p.StoneHi, 0.35f));
        Vector2 brow = new Vector2(pos.X + dir * 2f, pos.Y - 4f);
        Raylib.DrawLineEx(brow + new Vector2(-dir * 4f, 0f), brow + new Vector2(dir * 5f, -1f), 1.5f, WithAlpha(p.StoneDeep, 0.7f));
        Vector2 snout = new Vector2(pos.X + dir * 11f, pos.Y + 3f);
        Raylib.DrawTriangle(pos, new Vector2(pos.X + dir * 4f, pos.Y - 2f), snout, WithAlpha(p.StoneMid, 0.9f));
        Raylib.DrawTriangle(snout, new Vector2(snout.X + dir * 4f, snout.Y + 2f), new Vector2(snout.X, snout.Y + 5f), WithAlpha(p.StoneDeep, 0.85f));
        Raylib.DrawLineEx(new Vector2(pos.X - dir * 3f, pos.Y + 1f), new Vector2(pos.X - dir * 9f, pos.Y + 8f), 1.2f, WithAlpha(p.StoneDark, 0.8f));
        Raylib.DrawLineEx(new Vector2(pos.X + dir, pos.Y + 2f), new Vector2(pos.X + dir * 7f, pos.Y + 9f), 1.2f, WithAlpha(p.StoneDark, 0.8f));
        Raylib.DrawLineEx(new Vector2(pos.X, pos.Y + 5f), new Vector2(pos.X + dir * 2f, pos.Y + 14f), 1.5f, WithAlpha(p.StoneDeep, 0.75f));
        for (int wing = 0; wing < 3; wing++)
        {
            float wy = pos.Y - 1f + wing * 3f;
            Raylib.DrawLineEx(new Vector2(pos.X - dir * 2f, wy), new Vector2(pos.X - dir * (10f + wing * 2f), wy - 4f + wing), 1f, WithAlpha(p.StoneDark, 0.7f));
        }
        if (time > 0f && Hash((int)(time * 3f)) > 0.55f)
        {
            Raylib.DrawCircleV(new Vector2(snout.X + dir * 2f, snout.Y + 10f + drip * 4f), 1f, WithAlpha(p.WetSheen, 0.25f));
        }
    }

    static void DrawMenuCastleStainedGlass(Rectangle window, float time, float phase)
    {
        float pulse = MathF.Sin(time * 1.6f + phase) * 0.5f + 0.5f;
        Color[] panes =
        [
            new Color(92, 38, 48, 255),
            new Color(48, 62, 92, 255),
            new Color(78, 72, 38, 255),
            new Color(52, 82, 58, 255),
        ];
        for (int py = 0; py < 2; py++)
        {
            for (int px = 0; px < 2; px++)
            {
                var pane = new Rectangle(
                    window.X + 2f + px * (window.Width - 4f) / 2f,
                    window.Y + 2f + py * (window.Height - 4f) / 2f,
                    (window.Width - 4f) / 2f - 1f,
                    (window.Height - 4f) / 2f - 1f);
                Color c = panes[(px + py * 2) % panes.Length];
                Raylib.DrawRectangleRounded(pane, 0.1f, 2, WithAlpha(c, 0.22f + pulse * 0.12f));
            }
        }
        DrawEllipticalGlow(new Vector2(window.X + window.Width / 2f, window.Y + window.Height / 2f),
            window.Width * 0.55f, window.Height * 0.5f, 0f, new Color(180, 152, 108, 255), 0.015f + pulse * 0.01f, 2);
    }

    static void DrawMenuCastleRichPennant(Vector2 anchor, float time, MenuCastlePalette p)
    {
        float wave = MathF.Sin(time * 2.1f) * 8f;
        float fold = MathF.Sin(time * 3.4f + 0.6f) * 0.5f + 0.5f;
        Vector2 tip = new Vector2(anchor.X + 52f + wave, anchor.Y + 34f + fold * 4f);
        Vector2 mid = new Vector2(anchor.X + 28f + wave * 0.5f, anchor.Y + 42f);
        Raylib.DrawTriangle(anchor, new Vector2(anchor.X + 2f, anchor.Y + 48f), tip + new Vector2(3f, 2f), WithAlpha(p.HeraldRedDeep, 0.62f));
        Raylib.DrawTriangle(anchor, mid, tip, WithAlpha(p.HeraldRed, 0.86f));
        Raylib.DrawTriangle(anchor, new Vector2(anchor.X, anchor.Y + 46f), mid, WithAlpha(p.HeraldRedLight, 0.8f));
        for (int stripe = 0; stripe < 5; stripe++)
        {
            float sy = anchor.Y + 6f + stripe * 7f;
            Raylib.DrawLineEx(new Vector2(anchor.X + 4f + wave * 0.1f * stripe, sy),
                new Vector2(tip.X - 8f + stripe * 3f, sy + 8f + stripe * 2f), 1.2f,
                WithAlpha(Gold, 0.22f + fold * 0.14f));
        }
        Raylib.DrawLineEx(anchor, new Vector2(anchor.X, anchor.Y + 52f), 2.5f, WithAlpha(p.StoneHi, 0.85f));
        Raylib.DrawCircleV(new Vector2(anchor.X, anchor.Y - 4f), 3f, WithAlpha(Gold, 0.55f));
        for (int tassel = 0; tassel < 4; tassel++)
            Raylib.DrawCircleV(new Vector2(tip.X - tassel * 5f, tip.Y + tassel * 2.5f), 1.8f, WithAlpha(Gold, 0.5f));
        DrawEllipticalGlow(new Vector2(anchor.X + 24f, anchor.Y + 24f), 28f, 18f, -8f, p.HeraldRedLight, 0.006f + fold * 0.004f, 2);
    }

    static void DrawMenuCastleEpicCobbles(Rectangle region, MenuCastlePalette p, float time, float gateCenterX, float gateW)
    {
        Raylib.DrawRectangle((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height, p.StoneDeep);
        int cols = 32;
        float colW = region.Width / cols;
        for (int row = 0; row < 4; row++)
        {
            float y = region.Y + row * (region.Height / 4f);
            float offset = row % 2 == 0 ? 0f : colW * 0.5f;
            for (int col = -1; col < cols + 1; col++)
            {
                float x = region.X + col * colW + offset;
                int seed = row * 131 + col * 97;
                float n = Hash(seed);
                float n2 = Hash(seed + 11);
                float inset = 0.8f + n * 1.6f;
                var stone = new Rectangle(x + inset, y + inset, colW - inset * 2f, region.Height / 4f - inset * 2f);
                Color face = LerpColor(p.StoneDark, p.StoneMid, n * 0.5f + 0.15f);
                face = LerpColor(face, p.StoneLight, n2 * 0.2f);
                Raylib.DrawRectangleRounded(stone, 0.12f + n * 0.08f, 3, WithAlpha(face, 0.72f + n2 * 0.18f));
                Raylib.DrawLine((int)stone.X, (int)stone.Y, (int)(stone.X + stone.Width * 0.6f), (int)stone.Y, WithAlpha(p.StoneHi, 0.1f));
                if (n > 0.78f)
                    Raylib.DrawCircleV(new Vector2(stone.X + stone.Width * 0.5f, stone.Y + stone.Height * 0.5f), 1f, WithAlpha(p.Mortar, 0.35f));
            }
        }
        float torchProx = MathF.Abs(gateCenterX - region.X) / region.Width;
        Raylib.DrawRectangleGradientH((int)(gateCenterX - gateW * 0.6f), (int)region.Y, (int)(gateW * 1.2f), (int)region.Height,
            WithAlpha(p.TorchWarm, 0f), WithAlpha(p.TorchWarm, 0.04f + MathF.Sin(time * 4f) * 0.02f));
        for (int sheen = 0; sheen < 18; sheen++)
        {
            float sx = region.X + Hash(sheen * 29) * region.Width;
            float sy = region.Y + 4f + Hash(sheen * 31) * (region.Height - 8f);
            if (MathF.Sin(time * 2.8f + sheen) > 0.65f)
                Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + 10f, sy - 3f), 0.9f, WithAlpha(p.WetSheen, 0.14f));
        }
    }

    static void DrawMenuCastleWalkwayParapetDetail(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        int merlons = 24;
        for (int merlon = 0; merlon < merlons; merlon++)
        {
            float mx = 32f + merlon * ((WindowWidth - 64f) / (merlons - 1));
            if (mx > L.GatehouseLeft - 12f && mx < L.GatehouseRight + 12f) continue;
            if (merlon % 2 != 0) continue;
            Raylib.DrawRectangle((int)mx, (int)(L.ParapetY - 10f), 10, 10, WithAlpha(p.StoneMid, 0.65f));
            Raylib.DrawLine((int)mx, (int)(L.ParapetY - 10f), (int)(mx + 10), (int)(L.ParapetY - 10f), WithAlpha(p.StoneHi, 0.2f));
        }
    }

    static void DrawMenuCastleFireflies(float wallY, float time)
    {
        for (int f = 0; f < 22; f++)
        {
            float life = (time * (0.35f + Hash(f * 7) * 0.5f) + f * 1.7f) % 1f;
            float fx = Hash(f * 13) * WindowWidth + MathF.Sin(time * 0.6f + f) * 30f;
            float fy = wallY - 40f - life * 120f + Hash(f * 17) * 30f;
            float glow = MathF.Sin(life * MathF.PI);
            Color fire = LerpColor(new Color(196, 220, 96, 255), new Color(148, 196, 72, 255), Hash(f * 23));
            Raylib.DrawCircleV(new Vector2(fx, fy), 1.2f + glow, WithAlpha(fire, glow * 0.55f));
            DrawEllipticalGlow(new Vector2(fx, fy), 6f, 4f, 0f, fire, glow * 0.025f, 2);
        }
    }

    static void DrawMenuCastleCinematicVignette(float time)
    {
        float pulse = 0.92f + MathF.Sin(time * 0.15f) * 0.04f;
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, (int)(WindowHeight * 0.22f),
            WithAlpha(ForestShadow, 0.38f * pulse), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientV(0, (int)(WindowHeight * 0.78f), WindowWidth, (int)(WindowHeight * 0.22f),
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.42f * pulse));
        Raylib.DrawRectangleGradientH(0, 0, (int)(WindowWidth * 0.14f), WindowHeight,
            WithAlpha(ForestShadow, 0.32f * pulse), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH((int)(WindowWidth * 0.86f), 0, (int)(WindowWidth * 0.14f), WindowHeight,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.32f * pulse));
    }

    static void DrawMenuCastleInnerBaileyGlow(float gateX, float gateW, float gateY, float gateH, MenuCastlePalette p, float time)
    {
        var bailey = new Rectangle(gateX + gateW * 0.15f, gateY + gateH * 0.2f, gateW * 0.7f, gateH * 0.65f);
        float pulse = MathF.Sin(time * 1.2f) * 0.5f + 0.5f;
        DrawGradientWash(bailey, WithAlpha(p.TorchWarm, 0.08f + pulse * 0.06f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0.2f), 2.2f);
        for (int torch = 0; torch < 3; torch++)
        {
            float tx = bailey.X + bailey.Width * (0.2f + torch * 0.3f);
            DrawEllipticalGlow(new Vector2(tx, bailey.Y + bailey.Height * 0.4f), 14f, 20f, 0f, p.TorchWarm, 0.012f + pulse * 0.008f, 2);
        }
        Raylib.DrawLineEx(new Vector2(bailey.X + 8f, bailey.Y + bailey.Height * 0.7f),
            new Vector2(bailey.X + bailey.Width - 8f, bailey.Y + bailey.Height * 0.7f), 1f, WithAlpha(p.StoneHi, 0.12f));
    }

    static void DrawMenuCastleUltimateBeautyPass(float time, MenuCastlePalette p, float wallY, float wallH,
        float gateX, float gateW, float gateY, float gateH, float gatehouseX, float gatehouseW, float gatehouseY,
        Rectangle gatehouse)
    {
        DrawMenuCastleInnerBaileyGlow(gateX, gateW, gateY, gateH, p, time);
        DrawMenuCastleFireflies(wallY, time);
        DrawMenuCastleVolumetricFog(wallY, time, p);

        Vector2 moon = MenuCastleMoonPosition;
        for (int halo = 0; halo < 6; halo++)
        {
            float hx = gatehouseX + gatehouseW * (0.15f + halo * 0.14f);
            float hy = gatehouse.Y - 12f - halo * 4f;
            float face = Math.Clamp(1f - MathF.Abs(hx - moon.X) / 280f, 0.1f, 1f);
            DrawEllipticalGlow(new Vector2(hx, hy), 18f, 6f, -5f, p.MoonGlow, 0.008f * face, 2);
        }

        for (int lantern = 0; lantern < 5; lantern++)
        {
            float lx = gatehouseX + gatehouseW * (0.1f + lantern * 0.2f);
            float ly = gatehouse.Y + gatehouse.Height * (0.55f + Hash(lantern * 9) * 0.2f);
            float lp = MathF.Sin(time * 2f + lantern * 1.3f) * 0.5f + 0.5f;
            DrawEllipticalGlow(new Vector2(lx, ly), 8f, 10f, 0f, p.TorchWarm, 0.01f + lp * 0.008f, 2);
        }

        DrawMenuCastleCinematicVignette(time);

        for (int grain = 0; grain < 40; grain++)
        {
            if (Hash(grain + (int)(time * 20f)) > 0.82f)
            {
                float gx = Hash(grain * 3) * WindowWidth;
                float gy = Hash(grain * 5) * WindowHeight;
                Raylib.DrawPixel((int)gx, (int)gy, WithAlpha(Color.White, 0.025f));
            }
        }
    }

    static void DrawMenuCastleVolumetricFog(float wallY, float time, MenuCastlePalette p)
    {
        for (int layer = 0; layer < 7; layer++)
        {
            float y = wallY - 20f + layer * 10f;
            float drift = MathF.Sin(time * 0.11f + layer * 0.7f) * 18f;
            for (int wisp = 0; wisp < 8; wisp++)
            {
                float wx = WindowWidth * (wisp / 7f) + drift + Hash(layer * 13 + wisp) * 50f;
                DrawEllipticalGlow(new Vector2(wx, y), 70f + layer * 8f, 12f + layer * 2f, layer * 3f,
                    p.MoonGlow, 0.006f + layer * 0.002f, 2);
            }
        }
    }

    static void DrawMenuCastleMoonlitWallRim(float wallY, float wallH, MenuCastlePalette p, float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float facing = Math.Clamp(1f - MathF.Abs(WindowWidth / 2f - moon.X) / (WindowWidth * 0.5f), 0.2f, 1f);
        var moonWash = new Rectangle(0f, wallY + 6f, WindowWidth, wallH * 0.38f);
        Raylib.DrawRectangleGradientH((int)moonWash.X, (int)moonWash.Y, (int)moonWash.Width, (int)moonWash.Height,
            WithAlpha(p.MoonGlow, 0f), WithAlpha(p.MoonGlow, 0.035f * facing));

        for (int crest = 0; crest < 12; crest++)
        {
            float cx = WindowWidth * (crest / 11f);
            float rimY = wallY - 1f;
            float face = Math.Clamp(1f - MathF.Abs(cx - moon.X) / (WindowWidth * 0.48f), 0.12f, 1f);
            DrawEllipticalGlow(new Vector2(cx, rimY), 26f, 5f, -4f, p.MoonGlow, 0.01f * face, 2);
        }
    }

}
