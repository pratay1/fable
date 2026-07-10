partial class Program
{
    // -------------------------------------------------------------------------
    // Menu castle palette
    // -------------------------------------------------------------------------

    readonly struct MenuCastlePalette
    {
        public Color Stone { get; init; }
        public Color StoneMid { get; init; }
        public Color StoneDark { get; init; }
        public Color StoneDeep { get; init; }
        public Color StoneLight { get; init; }
        public Color StoneHi { get; init; }
        public Color Mortar { get; init; }
        public Color TorchWarm { get; init; }
        public Color MoonGlow { get; init; }
        public Color Iron { get; init; }
        public Color Moss { get; init; }
        public Color Lichen { get; init; }
        public Color WetSheen { get; init; }
        public Color HeraldRed { get; init; }
        public Color HeraldRedLight { get; init; }
        public Color HeraldRedDeep { get; init; }

        public static MenuCastlePalette Default => new()
        {
            Stone = new Color(68, 64, 58, 255),
            StoneMid = new Color(54, 50, 46, 255),
            StoneDark = new Color(36, 34, 32, 255),
            StoneDeep = new Color(22, 20, 19, 255),
            StoneLight = new Color(102, 98, 90, 255),
            StoneHi = new Color(138, 132, 122, 255),
            Mortar = new Color(26, 24, 23, 255),
            TorchWarm = new Color(210, 168, 108, 255),
            MoonGlow = new Color(184, 182, 176, 255),
            Iron = new Color(48, 46, 44, 255),
            Moss = new Color(54, 58, 48, 255),
            Lichen = new Color(68, 74, 58, 255),
            WetSheen = new Color(82, 80, 74, 255),
            HeraldRed = new Color(118, 36, 34, 255),
            HeraldRedLight = new Color(148, 52, 44, 255),
            HeraldRedDeep = new Color(78, 24, 26, 255),
        };
    }

    static MenuCastlePalette MenuPalette => MenuCastlePalette.Default;

    readonly struct MenuCastleLayout
    {
        public float WallY { get; init; }
        public float WallH { get; init; }
        public float ParapetY { get; init; }
        public float ForecourtY { get; init; }
        public float ForecourtH { get; init; }
        public float GatehouseW { get; init; }
        public float GatehouseH { get; init; }
        public float GatehouseX { get; init; }
        public float GatehouseTop { get; init; }
        public float GatehouseBottom { get; init; }
        public float BattlementsY { get; init; }
        public Rectangle GatehouseRect { get; init; }
        public float GateW { get; init; }
        public float GateH { get; init; }
        public float GateX { get; init; }
        public float GateY { get; init; }
        public Rectangle GateRect { get; init; }
        public Vector2 LeftTowerOrigin { get; init; }
        public Vector2 RightTowerOrigin { get; init; }
        public float TowerW { get; init; }
        public float TowerH { get; init; }
        public float BarbicanX { get; init; }
        public float BarbicanY { get; init; }
        public float BarbicanW { get; init; }
        public float BarbicanH { get; init; }
        public float MoatY { get; init; }
        public float CenterX => WindowWidth / 2f;
        public float GatehouseLeft => GatehouseX;
        public float GatehouseRight => GatehouseX + GatehouseW;

        public static MenuCastleLayout Compute()
        {
            const float forecourtH = 40f;
            float forecourtY = WindowHeight - forecourtH;
            float wallH = (WindowHeight - forecourtH) * 0.50f;
            float wallY = forecourtY - wallH;
            float parapetY = wallY - 10f;

            float gatehouseW = MathF.Min(208f, WindowWidth * 0.26f);
            float gatehouseTop = wallY + wallH * 0.05f;
            float gatehouseBottom = forecourtY;
            float gatehouseH = gatehouseBottom - gatehouseTop;
            float gatehouseX = WindowWidth / 2f - gatehouseW / 2f;
            float battlementsY = gatehouseTop - 30f;

            float gateW = gatehouseW * 0.52f;
            float gateH = gatehouseH * 0.60f;
            float gateX = WindowWidth / 2f - gateW / 2f;
            float gateY = gatehouseBottom - gateH;

            float towerW = MathF.Min(172f, WindowWidth * 0.215f);
            float towerH = wallH + 58f;
            float towerY = wallY - 42f;

            float barbW = gateW + 76f;
            float barbH = gateH * 0.32f;
            float barbX = gateX - (barbW - gateW) / 2f;
            float barbY = gateY - barbH + 6f;

            return new MenuCastleLayout
            {
                WallY = wallY,
                WallH = wallH,
                ParapetY = parapetY,
                ForecourtY = forecourtY,
                ForecourtH = forecourtH,
                GatehouseW = gatehouseW,
                GatehouseH = gatehouseH,
                GatehouseX = gatehouseX,
                GatehouseTop = gatehouseTop,
                GatehouseBottom = gatehouseBottom,
                BattlementsY = battlementsY,
                GatehouseRect = new Rectangle(gatehouseX, gatehouseTop, gatehouseW, gatehouseH),
                GateW = gateW,
                GateH = gateH,
                GateX = gateX,
                GateY = gateY,
                GateRect = new Rectangle(gateX, gateY, gateW, gateH),
                LeftTowerOrigin = new Vector2(-2f, towerY),
                RightTowerOrigin = new Vector2(WindowWidth - towerW + 2f, towerY),
                TowerW = towerW,
                TowerH = towerH,
                BarbicanX = barbX,
                BarbicanY = barbY,
                BarbicanW = barbW,
                BarbicanH = barbH,
                MoatY = forecourtY - 16f,
            };
        }
    }

    static MenuCastleLayout MenuCastleLayoutCurrent;

    static MenuCastleLayout GetMenuCastleLayout()
    {
        if (!menuCastleLayoutValid)
        {
            menuCastleLayoutCached = MenuCastleLayout.Compute();
            menuCastleLayoutValid = true;
        }

        return menuCastleLayoutCached;
    }

    static void InvalidateMenuCastleLayout() => menuCastleLayoutValid = false;

    static void UnloadMenuCastleBake()
    {
        if (!menuCastleBakeReady) return;
        Raylib.UnloadRenderTexture(menuCastleBake);
        menuCastleBakeReady = false;
        menuCastleBakeW = 0;
        menuCastleBakeH = 0;
    }

    static void EnsureMenuCastleBake(MenuCastleLayout L, MenuCastlePalette p)
    {
        if (menuCastleBakeReady && menuCastleBakeW == WindowWidth && menuCastleBakeH == WindowHeight) return;

        UnloadMenuCastleBake();
        menuCastleBake = Raylib.LoadRenderTexture(WindowWidth, WindowHeight);
        menuCastleBakeW = WindowWidth;
        menuCastleBakeH = WindowHeight;

        Raylib.BeginTextureMode(menuCastleBake);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));
        DrawMenuCastleStructuralPass(0f, L, p);
        Raylib.EndTextureMode();
        menuCastleBakeReady = true;
    }

    static void DrawMenuCastleStructuralPass(float time, MenuCastleLayout L, MenuCastlePalette p)
    {
        DrawMenuCastleHorizonMist(L.WallY, p, time);
        DrawMenuCastleForestTreeline(L.WallY, p, time);
        DrawMenuCastleSilhouetteLayer(L.WallY, L.WallH, p, time);
        DrawMenuCastleMoatReflection(L.WallY, L.WallH, p, time);
        DrawMenuCastleProductionMasterPass(MenuCastleProductionPhase.Backdrop, time, p);

        Raylib.DrawRectangleGradientV(0, (int)(L.WallY - 110f), WindowWidth, 90,
            WithAlpha(p.StoneDeep, 0f), WithAlpha(new Color(18, 17, 16, 255), 0.28f));
        Raylib.DrawRectangleGradientV(0, (int)(L.WallY - 90f), WindowWidth, 120,
            WithAlpha(p.StoneDeep, 0f), WithAlpha(p.StoneDeep, 0.42f));

        var curtain = new Rectangle(40f, L.WallY - 18f, WindowWidth - 80f, L.WallH + 18f);
        Raylib.DrawRectangleRounded(curtain, 0.02f, 6, WithAlpha(p.StoneDeep, 0.55f));
        Raylib.DrawRectangleRoundedLines(curtain, 0.02f, 6, WithAlpha(p.StoneLight, 0.12f));

        Raylib.DrawRectangle(0, (int)L.WallY, WindowWidth, (int)L.WallH, p.StoneDark);
        Raylib.DrawRectangleGradientV(0, (int)L.WallY, WindowWidth, (int)(L.WallH * 0.35f),
            WithAlpha(p.StoneMid, 0.35f), WithAlpha(p.StoneDark, 0f));
        Raylib.DrawRectangleGradientV(0, (int)(L.WallY + L.WallH * 0.55f), WindowWidth, (int)(L.WallH * 0.45f),
            WithAlpha(p.StoneDark, 0f), WithAlpha(p.StoneDeep, 0.85f));

        DrawMenuMasonryUltra(new Rectangle(0f, L.WallY, WindowWidth, L.WallH), p, 11, 20);
        DrawMenuCastleCurtainWallRelief(L.WallY, L.WallH, p, time);
        DrawMenuCastleRealisticStonework(L, p, time);
        DrawMenuCastleIvyPatches(L.WallY, L.WallH, time, p);
        DrawMenuCastleTowerBasePlinths(L.WallY, L.WallH, p);
        DrawMenuCastleFoundationCourse(L.WallY, L.WallH, p);
        DrawMenuCastleAmbientMoonlight(L.WallY, L.WallH, p);
        DrawMenuCastleCurtainBattlements(L, p, time);

        DrawButtress(new Vector2(0f, L.WallY), 62f, L.WallH, p.Stone, p.StoneDark, p.StoneLight, true);
        DrawButtress(new Vector2(WindowWidth - 62f, L.WallY), 62f, L.WallH, p.Stone, p.StoneDark, p.StoneLight, false);

        DrawMenuCastleTower(L.LeftTowerOrigin, L.TowerW, L.TowerH, p, time, true, 0f);
        DrawMenuCastleTower(L.RightTowerOrigin, L.TowerW, L.TowerH, p, time, false, 0.9f);
        DrawMenuCastleIntervalTowers(L.WallY, L.WallH, L.GatehouseX, L.GatehouseW, p, time);
        DrawMenuCastleChapelSpire(L.GatehouseX, L.GatehouseW, L.GatehouseTop, time, p);
        DrawMenuCastleLayeredInnerKeepComplex(L, p, time);

        var gatehouse = L.GatehouseRect;
        Raylib.DrawRectangleRounded(gatehouse, 0.04f, 8, p.StoneMid);
        Raylib.DrawRectangleRoundedLines(gatehouse, 0.04f, 8, WithAlpha(p.StoneLight, 0.7f));
        DrawMenuMasonryUltra(gatehouse, p, 8, 7);
        DrawQuoinStripes(gatehouse, p.StoneLight, p.StoneHi);
        DrawMenuCastleShieldEmblem(new Vector2(L.CenterX, L.GatehouseTop + L.GatehouseH * 0.12f), time, p);
        DrawMenuCastleGatehouseWindows(gatehouse, p, time);
        DrawMenuCastleLintelShadows(gatehouse, p);
        DrawMenuCastleIronworkDetail(gatehouse, p, 42);
        DrawMenuCastleGatehouseRealism(L, p, time);

        var cornice = new Rectangle(L.GatehouseX - 4f, L.BattlementsY + 22f, L.GatehouseW + 8f, 10f);
        Raylib.DrawRectangleRounded(cornice, 0.08f, 3, WithAlpha(p.StoneDark, 0.9f));
        Raylib.DrawLineEx(new Vector2(cornice.X + 4f, cornice.Y + 2f), new Vector2(cornice.X + cornice.Width - 4f, cornice.Y + 2f),
            1.5f, WithAlpha(p.StoneHi, 0.22f));

        DrawBattlements(new Rectangle(L.GatehouseX - 8f, L.BattlementsY, L.GatehouseW + 16f, 30f),
            p.StoneDark, p.StoneLight, p.StoneHi, 1.1f, time, true);

        float turretH = MathF.Min(112f, L.GatehouseH * 0.34f);
        float turretW = 42f;
        DrawMenuCastleTurret(new Vector2(L.GatehouseX - turretW + 6f, L.GatehouseTop + L.GatehouseH * 0.42f), turretW, turretH, p, time, 1.3f);
        DrawMenuCastleTurret(new Vector2(L.GatehouseX + L.GatehouseW - 6f, L.GatehouseTop + L.GatehouseH * 0.42f), turretW, turretH, p, time, 2.1f);

        DrawMenuCastleBarbican(L, time, p);
        DrawMenuCastleCourtyardDepth(L.GateX, L.GateW, L.GateY, L.GateH, p, time);
        DrawMenuCastleGate(new Vector2(L.GateX, L.GateY), L.GateW, L.GateH, p, time);
        DrawMenuCastlePortcullisShadow(new Vector2(L.GateX, L.GateY), L.GateW, L.GateH, p);

        float bridgeSpan = L.ForecourtY - (L.GateY + L.GateH);
        DrawMenuCastleDrawbridge(new Vector2(L.GateX, L.GateY + L.GateH), L.GateW, bridgeSpan, time, p);
        DrawMenuCastleDrawbridgeShadow(L.GateX, L.GateW, L.GateY, L.GateH, p);

        DrawMenuCastleWallWalkway(L, p);
        DrawMenuCastleWalkwayParapetDetail(L, p, time);
        DrawMenuCastleHumanScaleDetails(L, p, time);
        DrawMenuCastleProductionMasterPass(MenuCastleProductionPhase.Architecture, time, p);

        Raylib.DrawRectangle(0, (int)(L.ParapetY + 6f), WindowWidth, 4, WithAlpha(p.StoneDeep, 0.9f));
        Raylib.DrawLine(0, (int)(L.ParapetY + 6f), WindowWidth, (int)(L.ParapetY + 6f), WithAlpha(p.StoneHi, 0.25f));
        Raylib.DrawLine(0, (int)(L.ParapetY + 5f), WindowWidth, (int)(L.ParapetY + 5f), WithAlpha(p.StoneLight, 0.08f));

        DrawMenuCurtainWallArrowSlits(L, p);
        DrawMenuCastleForecourtRealism(L, p, time);
        DrawMenuCastleEpicCobbles(new Rectangle(0f, L.ForecourtY, WindowWidth, L.ForecourtH), p, time, L.CenterX, L.GateW);
    }

    static void DrawMenuCastleLayeredInnerKeepComplex(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float center = L.CenterX;
        float rearY = L.GatehouseTop - 92f;
        float rearH = 126f;
        float rearW = 326f;
        var rearKeep = new Rectangle(center - rearW / 2f, rearY, rearW, rearH);

        DrawEllipticalGlow(new Vector2(center, rearY + 16f), rearW * 0.46f, 16f, 0f, p.MoonGlow, 0.012f, 2);
        Raylib.DrawRectangleRounded(rearKeep, 0.04f, 6, WithAlpha(p.StoneDeep, 0.66f));
        Raylib.DrawRectangleGradientV((int)rearKeep.X, (int)rearKeep.Y, (int)rearKeep.Width, (int)rearKeep.Height,
            WithAlpha(p.StoneMid, 0.18f), WithAlpha(p.StoneDeep, 0.16f));
        DrawMenuCastleAshlarMicroBlocks(rearKeep, p, 7, 18, 14000);
        Raylib.DrawRectangleRoundedLines(rearKeep, 0.04f, 6, WithAlpha(p.StoneHi, 0.11f));

        DrawBattlements(new Rectangle(rearKeep.X - 4f, rearKeep.Y - 18f, rearKeep.Width + 8f, 18f),
            p.StoneDark, p.StoneLight, p.StoneHi, 0.78f, time, true);
        DrawMenuCastleBrokenMerlons(new Rectangle(rearKeep.X - 4f, rearKeep.Y - 18f, rearKeep.Width + 8f, 18f), p, 15040);

        DrawMenuCastleGabledRoof(new Rectangle(rearKeep.X + 42f, rearKeep.Y - 34f, 94f, 40f), p, time, 0.2f);
        DrawMenuCastleGabledRoof(new Rectangle(rearKeep.X + rearKeep.Width - 136f, rearKeep.Y - 30f, 98f, 36f), p, time, 1.1f);

        ReadOnlySpan<float> towerOffsets = [-0.42f, -0.25f, 0.25f, 0.42f];
        for (int i = 0; i < towerOffsets.Length; i++)
        {
            float tw = i is 1 or 2 ? 34f : 42f;
            float th = i is 1 or 2 ? 100f : 118f;
            float tx = center + towerOffsets[i] * rearW - tw / 2f;
            float ty = rearY - th * 0.32f + Hash(14100 + i * 17) * 5f;
            var tower = new Rectangle(tx, ty, tw, th);
            Raylib.DrawRectangleRounded(tower, 0.08f, 5, WithAlpha(p.StoneDark, 0.78f));
            DrawMenuCastleAshlarMicroBlocks(tower, p, 7, 3, 14200 + i * 43);
            Raylib.DrawRectangleRoundedLines(tower, 0.08f, 5, WithAlpha(p.StoneHi, 0.16f));
            DrawTowerRoof(tx + tw / 2f, ty - 24f, ty - 2f, tw * 0.58f, p.StoneDeep, p.StoneLight, p.StoneHi);
            DrawMenuCastleRoofTileDetail(tx + tw / 2f, ty - 24f, ty - 2f, tw * 0.58f, p, i);
            DrawMenuArrowSlit(new Vector2(tx + tw / 2f - 3f, ty + th * 0.34f), 6f, 18f, p);
            DrawMenuArrowSlit(new Vector2(tx + tw / 2f - 3f, ty + th * 0.58f), 6f, 18f, p);
        }

        var gallery = new Rectangle(rearKeep.X + 28f, rearKeep.Y + rearKeep.Height * 0.38f, rearKeep.Width - 56f, 24f);
        Raylib.DrawRectangleRounded(gallery, 0.08f, 3, WithAlpha(ForestShadow, 0.42f));
        Raylib.DrawRectangleRoundedLines(gallery, 0.08f, 3, WithAlpha(p.StoneHi, 0.13f));
        int arches = 9;
        for (int a = 0; a < arches; a++)
        {
            float ax = gallery.X + 9f + a * ((gallery.Width - 18f) / (arches - 1));
            var arch = new Rectangle(ax - 6f, gallery.Y + 4f, 12f, 16f);
            Raylib.DrawRectangleRounded(arch, 0.45f, 5, WithAlpha(p.StoneDeep, 0.74f));
            Raylib.DrawRectangleRoundedLines(arch, 0.45f, 5, WithAlpha(p.StoneHi, 0.11f));
            if (a % 3 == 1)
                DrawEllipticalGlow(new Vector2(ax, arch.Y + arch.Height * 0.62f), 8f, 10f, 0f, p.TorchWarm, 0.004f, 2);
        }

        for (int win = 0; win < 10; win++)
        {
            float x = rearKeep.X + 24f + win * ((rearKeep.Width - 48f) / 9f);
            if (MathF.Abs(x - center) < L.GatehouseW * 0.34f) continue;
            float y = rearKeep.Y + rearKeep.Height * (0.18f + Hash(14500 + win * 11) * 0.12f);
            var w = new Rectangle(x - 4f, y, 8f, 16f);
            Raylib.DrawRectangleRounded(w, 0.35f, 3, WithAlpha(ForestShadow, 0.8f));
            if (win % 4 == 0)
                DrawEllipticalGlow(new Vector2(x, y + 9f), 7f, 10f, 0f, p.TorchWarm, 0.005f, 2);
            Raylib.DrawLine((int)w.X, (int)w.Y, (int)(w.X + w.Width), (int)w.Y, WithAlpha(p.StoneHi, 0.12f));
        }

        DrawMenuCastleRearFlyingButtresses(rearKeep, L, p);
        DrawMenuCastleRearRoofClutter(rearKeep, p, time);

        Raylib.DrawRectangleGradientV((int)(rearKeep.X - 18f), (int)(rearKeep.Y + rearKeep.Height - 28f),
            (int)(rearKeep.Width + 36f), 54, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.42f));
    }

    static void DrawMenuCastleGabledRoof(Rectangle baseRect, MenuCastlePalette p, float time, float phase)
    {
        Vector2 left = new Vector2(baseRect.X, baseRect.Y + baseRect.Height);
        Vector2 right = new Vector2(baseRect.X + baseRect.Width, baseRect.Y + baseRect.Height);
        Vector2 peak = new Vector2(baseRect.X + baseRect.Width / 2f, baseRect.Y);
        Raylib.DrawTriangle(left, peak, right, WithAlpha(p.StoneDeep, 0.84f));
        Raylib.DrawLineEx(left, peak, 1.2f, WithAlpha(p.MoonGlow, 0.11f));
        Raylib.DrawLineEx(peak, right, 1.2f, WithAlpha(ForestShadow, 0.22f));
        int courses = 6;
        for (int c = 1; c <= courses; c++)
        {
            float t = c / (float)courses;
            float y = baseRect.Y + baseRect.Height * t;
            float half = baseRect.Width * 0.5f * t;
            Raylib.DrawLineEx(new Vector2(baseRect.X + baseRect.Width / 2f - half, y),
                new Vector2(baseRect.X + baseRect.Width / 2f + half, y), 0.8f, WithAlpha(p.StoneHi, 0.06f));
            for (int tile = 0; tile < 5 + c; tile++)
            {
                float x = baseRect.X + baseRect.Width / 2f - half + tile * ((half * 2f) / Math.Max(1, 4 + c));
                Raylib.DrawLineEx(new Vector2(x, y - 4f), new Vector2(x + (Hash((int)(phase * 100f) + c * 17 + tile) - 0.5f) * 3f, y),
                    0.45f, WithAlpha(ForestShadow, 0.14f));
            }
        }
        Raylib.DrawCircleV(peak + new Vector2(0f, -1f), 1.7f, WithAlpha(p.StoneHi, 0.20f + MathF.Sin(time + phase) * 0.04f));
    }

    static void DrawMenuCastleRearFlyingButtresses(Rectangle rearKeep, MenuCastleLayout L, MenuCastlePalette p)
    {
        ReadOnlySpan<float> xs = [0.18f, 0.32f, 0.68f, 0.82f];
        for (int i = 0; i < xs.Length; i++)
        {
            float x = rearKeep.X + rearKeep.Width * xs[i];
            float targetX = x + (x < L.CenterX ? -48f : 48f);
            Vector2 high = new Vector2(x, rearKeep.Y + rearKeep.Height * 0.34f);
            Vector2 low = new Vector2(targetX, L.WallY + 24f);
            Raylib.DrawLineEx(high + new Vector2(2f, 3f), low + new Vector2(2f, 3f), 5.8f, WithAlpha(ForestShadow, 0.24f));
            Raylib.DrawLineEx(high, low, 4.5f, WithAlpha(p.StoneDark, 0.46f));
            Raylib.DrawLineEx(high + new Vector2(0f, -1f), low + new Vector2(0f, -1f), 1f, WithAlpha(p.StoneHi, 0.09f));
        }
    }

    static void DrawMenuCastleRearRoofClutter(Rectangle rearKeep, MenuCastlePalette p, float time)
    {
        for (int chimney = 0; chimney < 7; chimney++)
        {
            float x = rearKeep.X + rearKeep.Width * (0.12f + chimney * 0.13f) + Hash(14700 + chimney * 17) * 8f;
            float h = 14f + Hash(14720 + chimney * 19) * 12f;
            float y = rearKeep.Y - 14f - Hash(14740 + chimney * 23) * 18f;
            Raylib.DrawRectangleRounded(new Rectangle(x - 4f, y - h, 8f, h), 0.12f, 2, WithAlpha(p.StoneDark, 0.70f));
            Raylib.DrawRectangle((int)(x - 5f), (int)(y - h - 3f), 10, 3, WithAlpha(p.StoneMid, 0.55f));
            for (int puff = 0; puff < 3; puff++)
            {
                float drift = time * 0.28f + chimney * 0.6f + puff * 0.7f;
                Vector2 smoke = new Vector2(x + MathF.Sin(drift) * (5f + puff * 4f), y - h - 8f - puff * 8f);
                DrawEllipticalGlow(smoke, 7f + puff * 5f, 3f + puff * 2f, Hash(chimney * 11 + puff) * 20f,
                    new Color(46, 44, 42, 255), 0.006f - puff * 0.001f, 2);
            }
        }

        for (int spike = 0; spike < 11; spike++)
        {
            float x = rearKeep.X + 16f + spike * ((rearKeep.Width - 32f) / 10f);
            float y = rearKeep.Y - 20f + Hash(14800 + spike * 5) * 4f;
            Raylib.DrawLineEx(new Vector2(x, y + 10f), new Vector2(x, y - 8f), 0.7f, WithAlpha(p.Iron, 0.28f));
            Raylib.DrawCircleV(new Vector2(x, y - 8f), 1f, WithAlpha(p.StoneHi, 0.12f));
        }
    }

    static void DrawMenuCastleRealisticStonework(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        var wall = new Rectangle(0f, L.WallY, WindowWidth, L.WallH);
        DrawMenuCastleAshlarMicroBlocks(wall, p, 18, 34, 7300);

        for (int course = 0; course < 7; course++)
        {
            float y = L.WallY + 24f + course * (L.WallH - 54f) / 6f;
            Raylib.DrawLineEx(new Vector2(0f, y), new Vector2(WindowWidth, y), 1.3f, WithAlpha(p.StoneHi, 0.09f));
            Raylib.DrawLineEx(new Vector2(0f, y + 1.5f), new Vector2(WindowWidth, y + 1.5f), 1f, WithAlpha(p.StoneDeep, 0.18f));
        }

        ReadOnlySpan<float> buttressXs = [74f, 172f, 276f, 524f, 628f, 726f];
        for (int i = 0; i < buttressXs.Length; i++)
        {
            float x = buttressXs[i];
            if (x > L.GatehouseLeft - 26f && x < L.GatehouseRight + 26f) continue;
            DrawMenuCastleButtressPier(new Rectangle(x - 12f, L.WallY + 10f, 24f, L.WallH - 36f), p, i);
        }

        for (int patch = 0; patch < 18; patch++)
        {
            int seed = 8100 + patch * 43;
            float px = 32f + Hash(seed) * (WindowWidth - 64f);
            if (px > L.GatehouseLeft - 34f && px < L.GatehouseRight + 34f) continue;
            float py = L.WallY + 26f + Hash(seed + 3) * (L.WallH - 78f);
            var repair = new Rectangle(px, py, 22f + Hash(seed + 7) * 42f, 12f + Hash(seed + 11) * 20f);
            DrawMenuCastleRepairPatch(repair, p, seed);
        }

        for (int hole = 0; hole < 54; hole++)
        {
            int seed = 9100 + hole * 17;
            float hx = 20f + (hole % 18) * ((WindowWidth - 40f) / 17f);
            float hy = L.WallY + 44f + (hole / 18) * 54f + Hash(seed) * 8f;
            if (hx > L.GatehouseLeft - 18f && hx < L.GatehouseRight + 18f) continue;
            var slot = new Rectangle(hx - 2.5f, hy - 1.5f, 5f, 3f);
            Raylib.DrawRectangleRounded(slot, 0.35f, 2, WithAlpha(ForestShadow, 0.48f));
            Raylib.DrawLine((int)slot.X, (int)slot.Y, (int)(slot.X + slot.Width), (int)slot.Y, WithAlpha(p.StoneHi, 0.09f));
        }

        DrawMenuCastleWallDrainage(L, p, time);
    }

    static void DrawMenuCastleAshlarMicroBlocks(Rectangle region, MenuCastlePalette p, int rows, int cols, int seedBase)
    {
        float rowH = region.Height / rows;
        float colW = region.Width / cols;
        for (int row = 0; row < rows; row++)
        {
            float y = region.Y + row * rowH;
            float offset = row % 2 == 0 ? 0f : colW * 0.42f;
            for (int col = -1; col <= cols; col++)
            {
                int seed = seedBase + row * 181 + col * 67;
                float x = region.X + col * colW + offset + (Hash(seed) - 0.5f) * 2.2f;
                float w = colW * (0.86f + Hash(seed + 5) * 0.28f);
                float h = rowH * (0.76f + Hash(seed + 9) * 0.22f);
                var block = new Rectangle(x + 1.5f, y + 1.5f, w - 3f, h - 3f);
                if (block.X > region.X + region.Width || block.X + block.Width < region.X) continue;

                float n = Hash(seed + 13);
                float raised = Hash(seed + 17);
                Color edge = LerpColor(p.StoneDark, p.StoneLight, n * 0.42f);
                Raylib.DrawRectangleRoundedLines(block, 0.08f, 3, WithAlpha(edge, 0.045f + raised * 0.075f));

                if (Hash(seed + 23) > 0.72f)
                {
                    Vector2 a = new Vector2(block.X + block.Width * Hash(seed + 29), block.Y + block.Height * 0.18f);
                    Vector2 b = a + new Vector2((Hash(seed + 31) - 0.5f) * 12f, block.Height * (0.18f + Hash(seed + 37) * 0.38f));
                    Vector2 c = b + new Vector2((Hash(seed + 41) - 0.5f) * 10f, block.Height * (0.12f + Hash(seed + 43) * 0.24f));
                    Raylib.DrawLineEx(a, b, 0.6f, WithAlpha(p.StoneDeep, 0.16f));
                    Raylib.DrawLineEx(b, c, 0.55f, WithAlpha(p.StoneDeep, 0.12f));
                    Raylib.DrawLineEx(a + new Vector2(0f, -0.8f), b + new Vector2(0f, -0.8f), 0.45f, WithAlpha(p.StoneHi, 0.05f));
                }

                if (Hash(seed + 47) > 0.8f)
                {
                    Vector2 chip = new Vector2(block.X + Hash(seed + 53) * block.Width, block.Y + Hash(seed + 59) * block.Height);
                    Raylib.DrawTriangle(chip, chip + new Vector2(4f, 1f), chip + new Vector2(1f, 4f), WithAlpha(p.StoneDeep, 0.18f));
                    Raylib.DrawCircleV(chip + new Vector2(1f, 1f), 0.8f, WithAlpha(p.StoneHi, 0.08f));
                }
            }
        }
    }

    static void DrawMenuCastleButtressPier(Rectangle pier, MenuCastlePalette p, int seed)
    {
        Raylib.DrawRectangleGradientH((int)(pier.X - 5f), (int)pier.Y, 5, (int)pier.Height,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.28f));
        Raylib.DrawRectangleGradientH((int)(pier.X + pier.Width), (int)pier.Y, 6, (int)pier.Height,
            WithAlpha(ForestShadow, 0.25f), WithAlpha(ForestShadow, 0f));

        Raylib.DrawRectangleRounded(pier, 0.08f, 4, WithAlpha(p.StoneMid, 0.86f));
        DrawMenuMasonryUltra(pier, p, 7, 2);
        Raylib.DrawRectangleRoundedLines(pier, 0.08f, 4, WithAlpha(p.StoneHi, 0.24f));

        var cap = new Rectangle(pier.X - 4f, pier.Y - 8f, pier.Width + 8f, 10f);
        Raylib.DrawRectangleRounded(cap, 0.12f, 3, WithAlpha(p.StoneDark, 0.92f));
        Raylib.DrawLine((int)cap.X, (int)cap.Y, (int)(cap.X + cap.Width), (int)cap.Y, WithAlpha(p.StoneHi, 0.24f));

        for (int step = 0; step < 3; step++)
        {
            float sy = pier.Y + pier.Height * (0.30f + step * 0.22f);
            float inset = 2f + step * 1.2f;
            Raylib.DrawRectangleRounded(new Rectangle(pier.X + inset, sy, pier.Width - inset * 2f, 4f), 0.2f, 2,
                WithAlpha(step % 2 == 0 ? p.StoneDark : p.Stone, 0.5f));
        }

        if (Hash(seed * 31) > 0.45f)
            DrawMenuArrowSlit(new Vector2(pier.X + pier.Width / 2f - 3f, pier.Y + pier.Height * 0.48f), 6f, 18f, p);
    }

    static void DrawMenuCastleRepairPatch(Rectangle patch, MenuCastlePalette p, int seed)
    {
        Color tint = LerpColor(p.StoneDark, p.StoneLight, Hash(seed + 13) * 0.34f);
        Raylib.DrawRectangleRounded(patch, 0.06f, 3, WithAlpha(tint, 0.20f));
        Raylib.DrawRectangleRoundedLines(patch, 0.06f, 3, WithAlpha(p.Mortar, 0.32f));
        int rows = Math.Max(2, (int)(patch.Height / 8f));
        int cols = Math.Max(3, (int)(patch.Width / 12f));
        for (int row = 1; row < rows; row++)
        {
            float y = patch.Y + row * patch.Height / rows;
            Raylib.DrawLine((int)patch.X, (int)y, (int)(patch.X + patch.Width), (int)y, WithAlpha(p.Mortar, 0.28f));
        }
        for (int col = 1; col < cols; col++)
        {
            float x = patch.X + col * patch.Width / cols + (Hash(seed + col * 7) - 0.5f) * 2f;
            Raylib.DrawLine((int)x, (int)patch.Y, (int)x, (int)(patch.Y + patch.Height), WithAlpha(p.Mortar, 0.22f));
        }
    }

    static void DrawMenuCastleWallDrainage(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        ReadOnlySpan<float> drainXs = [122f, 248f, 552f, 682f];
        for (int i = 0; i < drainXs.Length; i++)
        {
            float x = drainXs[i];
            if (x > L.GatehouseLeft - 24f && x < L.GatehouseRight + 24f) continue;
            float y = L.WallY + L.WallH * (0.42f + Hash(i * 19) * 0.18f);
            var mouth = new Rectangle(x - 5f, y - 3f, 10f, 6f);
            Raylib.DrawRectangleRounded(mouth, 0.25f, 3, WithAlpha(p.StoneDeep, 0.82f));
            Raylib.DrawRectangleRoundedLines(mouth, 0.25f, 3, WithAlpha(p.StoneHi, 0.18f));
            float drip = (time * 0.5f + i * 0.31f) % 1f;
            Raylib.DrawLineEx(new Vector2(x, y + 3f), new Vector2(x + MathF.Sin(i) * 2f, y + 28f), 0.8f, WithAlpha(p.WetSheen, 0.09f));
            Raylib.DrawCircleV(new Vector2(x + MathF.Sin(i) * 2f, y + 6f + drip * 22f), 0.9f, WithAlpha(p.WetSheen, 0.13f * (1f - drip)));
        }
    }

    static void DrawMenuCastleGatehouseRealism(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        Rectangle gatehouse = L.GatehouseRect;
        DrawMenuCastleArchVoussoirs(L, p);

        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? L.GateX - 13f : L.GateX + L.GateW + 5f;
            for (int stone = 0; stone < 8; stone++)
            {
                float y = L.GateY + stone * (L.GateH / 8f);
                var jamb = new Rectangle(x, y + 2f, 8f, L.GateH / 8f - 3f);
                Raylib.DrawRectangleRounded(jamb, 0.08f, 2, WithAlpha(stone % 2 == 0 ? p.StoneLight : p.StoneDark, 0.42f));
                Raylib.DrawLine((int)jamb.X, (int)jamb.Y, (int)(jamb.X + jamb.Width), (int)jamb.Y, WithAlpha(p.StoneHi, 0.12f));
            }
        }

        var machicolationBand = new Rectangle(gatehouse.X + 16f, L.GateY - 24f, gatehouse.Width - 32f, 18f);
        Raylib.DrawRectangleRounded(machicolationBand, 0.1f, 3, WithAlpha(p.StoneDark, 0.82f));
        int murderHoles = 7;
        for (int i = 0; i < murderHoles; i++)
        {
            float x = machicolationBand.X + 10f + i * ((machicolationBand.Width - 20f) / (murderHoles - 1));
            var hole = new Rectangle(x - 4f, machicolationBand.Y + 5f, 8f, 8f);
            Raylib.DrawRectangleRounded(hole, 0.32f, 3, WithAlpha(ForestShadow, 0.88f));
            Raylib.DrawLine((int)hole.X, (int)hole.Y, (int)(hole.X + hole.Width), (int)hole.Y, WithAlpha(p.StoneHi, 0.18f));
        }

        for (int floor = 0; floor < 3; floor++)
        {
            float y = gatehouse.Y + gatehouse.Height * (0.22f + floor * 0.22f);
            Raylib.DrawLineEx(new Vector2(gatehouse.X + 12f, y), new Vector2(gatehouse.X + gatehouse.Width - 12f, y),
                2f, WithAlpha(p.StoneDeep, 0.22f));
            Raylib.DrawLineEx(new Vector2(gatehouse.X + 14f, y - 1f), new Vector2(gatehouse.X + gatehouse.Width - 14f, y - 1f),
                0.8f, WithAlpha(p.StoneHi, 0.13f));
        }

        DrawMenuCastleShutteredWindow(new Rectangle(gatehouse.X + 24f, gatehouse.Y + gatehouse.Height * 0.51f, 24f, 32f), p, time, 0.3f);
        DrawMenuCastleShutteredWindow(new Rectangle(gatehouse.X + gatehouse.Width - 48f, gatehouse.Y + gatehouse.Height * 0.51f, 24f, 32f), p, time, 1.6f);

        for (int stud = 0; stud < 28; stud++)
        {
            float sx = L.GateX + 8f + (stud % 7) * ((L.GateW - 16f) / 6f);
            float sy = L.GateY + L.GateH * 0.23f + (stud / 7) * (L.GateH * 0.14f);
            Raylib.DrawCircleV(new Vector2(sx, sy), 1.25f, WithAlpha(p.Iron, 0.62f));
            Raylib.DrawCircleV(new Vector2(sx - 0.4f, sy - 0.4f), 0.45f, WithAlpha(p.StoneHi, 0.22f));
        }

        Vector2 chainL = new Vector2(L.GateX + 12f, L.GateY + L.GateH * 0.18f);
        Vector2 chainR = new Vector2(L.GateX + L.GateW - 12f, L.GateY + L.GateH * 0.18f);
        DrawMenuCastleHeavyChain(chainL, new Vector2(L.GateX + 20f, L.GateY + L.GateH + 24f), p, 0);
        DrawMenuCastleHeavyChain(chainR, new Vector2(L.GateX + L.GateW - 20f, L.GateY + L.GateH + 24f), p, 1);
    }

    static void DrawMenuCastleArchVoussoirs(MenuCastleLayout L, MenuCastlePalette p)
    {
        Vector2 center = new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateW * 0.46f);
        float innerR = L.GateW * 0.48f;
        float outerR = innerR + 16f;
        int stones = 17;
        for (int i = 0; i < stones; i++)
        {
            float t = i / (float)(stones - 1);
            float a = MathF.PI + t * MathF.PI;
            Vector2 p0 = center + new Vector2(MathF.Cos(a) * innerR, MathF.Sin(a) * innerR);
            Vector2 p1 = center + new Vector2(MathF.Cos(a) * outerR, MathF.Sin(a) * outerR);
            Color stone = LerpColor(p.StoneDark, p.StoneLight, Hash(9400 + i * 13) * 0.45f + 0.18f);
            Raylib.DrawLineEx(p0, p1, 7f, WithAlpha(stone, 0.78f));
            Raylib.DrawLineEx(p0 + new Vector2(0f, -1f), p1 + new Vector2(0f, -1f), 1f, WithAlpha(p.StoneHi, 0.14f));
        }

        var keystone = new Rectangle(center.X - 7f, center.Y - outerR - 4f, 14f, 22f);
        Raylib.DrawRectangleRounded(keystone, 0.18f, 3, WithAlpha(p.StoneLight, 0.72f));
        Raylib.DrawRectangleRoundedLines(keystone, 0.18f, 3, WithAlpha(p.StoneHi, 0.26f));
    }

    static void DrawMenuCastleShutteredWindow(Rectangle r, MenuCastlePalette p, float time, float phase)
    {
        DrawWindowInteriorGlow(r, time, phase);
        Raylib.DrawRectangleRounded(r, 0.18f, 4, WithAlpha(ForestShadow, 0.9f));
        Raylib.DrawRectangleRoundedLines(r, 0.18f, 4, WithAlpha(p.StoneHi, 0.32f));

        Color wood = new Color(48, 38, 30, 255);
        float swing = MathF.Sin(time * 0.55f + phase) * 1.2f;
        var left = new Rectangle(r.X - 8f + swing, r.Y + 3f, 8f, r.Height - 6f);
        var right = new Rectangle(r.X + r.Width, r.Y + 3f, 8f, r.Height - 6f);
        Raylib.DrawRectangleRounded(left, 0.1f, 2, WithAlpha(wood, 0.72f));
        Raylib.DrawRectangleRounded(right, 0.1f, 2, WithAlpha(wood, 0.72f));
        Raylib.DrawLine((int)left.X, (int)(left.Y + left.Height * 0.5f), (int)(left.X + left.Width), (int)(left.Y + left.Height * 0.5f), WithAlpha(p.Iron, 0.32f));
        Raylib.DrawLine((int)right.X, (int)(right.Y + right.Height * 0.5f), (int)(right.X + right.Width), (int)(right.Y + right.Height * 0.5f), WithAlpha(p.Iron, 0.32f));
    }

    static void DrawMenuCastleHeavyChain(Vector2 a, Vector2 b, MenuCastlePalette p, int seed)
    {
        int links = 10;
        for (int i = 0; i < links; i++)
        {
            float t = i / (float)(links - 1);
            Vector2 pos = Vector2.Lerp(a, b, t);
            pos.X += MathF.Sin(t * MathF.PI + seed) * 4f;
            float rx = i % 2 == 0 ? 3.5f : 2.2f;
            float ry = i % 2 == 0 ? 2.2f : 3.5f;
            Raylib.DrawEllipse((int)pos.X, (int)pos.Y, rx, ry, WithAlpha(p.Iron, 0.55f));
            Raylib.DrawEllipseLines((int)pos.X, (int)pos.Y, rx, ry, WithAlpha(p.StoneHi, 0.13f));
        }
    }

    static void DrawMenuCastleHumanScaleDetails(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        DrawMenuCastleScaffoldBay(new Rectangle(86f, L.WallY + L.WallH * 0.26f, 78f, 74f), p, time, 0);
        DrawMenuCastleScaffoldBay(new Rectangle(WindowWidth - 164f, L.WallY + L.WallH * 0.30f, 76f, 68f), p, time, 1);

        ReadOnlySpan<(float x, float y)> lamps =
        [
            (L.GatehouseX - 30f, L.GateY - 20f),
            (L.GatehouseX + L.GatehouseW + 30f, L.GateY - 20f),
            (L.LeftTowerOrigin.X + L.TowerW * 0.40f, L.WallY + 104f),
            (L.RightTowerOrigin.X + L.TowerW * 0.60f, L.WallY + 104f),
        ];
        for (int i = 0; i < lamps.Length; i++)
        {
            Vector2 pos = new Vector2(lamps[i].x, lamps[i].y);
            Raylib.DrawLineEx(pos + new Vector2(-4f, -7f), pos + new Vector2(4f, -7f), 1.2f, WithAlpha(p.Iron, 0.48f));
            Raylib.DrawRectangleRounded(new Rectangle(pos.X - 4f, pos.Y - 6f, 8f, 12f), 0.22f, 3, WithAlpha(p.Iron, 0.42f));
            float glow = MathF.Sin(time * 3f + i * 1.7f) * 0.5f + 0.5f;
            DrawEllipticalGlow(pos, 14f, 18f, 0f, p.TorchWarm, 0.014f + glow * 0.01f, 2);
        }

        for (int slit = 0; slit < 10; slit++)
        {
            float x = 70f + slit * 72f;
            if (x > L.GatehouseLeft - 12f && x < L.GatehouseRight + 12f) continue;
            DrawMenuArrowSlit(new Vector2(x, L.WallY + L.WallH * (0.58f + Hash(slit * 17) * 0.15f)), 7f, 24f, p);
        }
    }

    static void DrawMenuCastleScaffoldBay(Rectangle bay, MenuCastlePalette p, float time, int seed)
    {
        Color wood = new Color(54, 42, 32, 255);
        Color rope = new Color(76, 62, 46, 255);
        float sway = MathF.Sin(time * 0.35f + seed) * 0.8f;

        for (int post = 0; post < 3; post++)
        {
            float x = bay.X + post * (bay.Width / 2f) + sway;
            Raylib.DrawLineEx(new Vector2(x, bay.Y), new Vector2(x, bay.Y + bay.Height), 2f, WithAlpha(wood, 0.52f));
        }
        for (int level = 0; level < 3; level++)
        {
            float y = bay.Y + level * (bay.Height / 2f);
            Raylib.DrawLineEx(new Vector2(bay.X - 4f, y), new Vector2(bay.X + bay.Width + 4f, y + sway * 0.4f), 2.2f, WithAlpha(wood, 0.58f));
        }
        Raylib.DrawLineEx(new Vector2(bay.X, bay.Y + bay.Height), new Vector2(bay.X + bay.Width, bay.Y), 1.1f, WithAlpha(rope, 0.36f));
        Raylib.DrawLineEx(new Vector2(bay.X + bay.Width, bay.Y + bay.Height), new Vector2(bay.X, bay.Y), 1.1f, WithAlpha(rope, 0.28f));
        Raylib.DrawRectangleRounded(new Rectangle(bay.X + bay.Width * 0.18f, bay.Y + bay.Height * 0.52f, bay.Width * 0.58f, 5f),
            0.14f, 2, WithAlpha(wood, 0.48f));
    }

    static void DrawMenuCastleForecourtRealism(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float groundY = L.ForecourtY + L.ForecourtH - 5f;
        for (int rut = 0; rut < 4; rut++)
        {
            float x0 = L.GateX + L.GateW * (0.18f + rut * 0.18f);
            float sway = MathF.Sin(rut * 1.7f) * 8f;
            Raylib.DrawLineEx(new Vector2(x0, L.ForecourtY + 3f), new Vector2(x0 + sway, groundY), 1.1f,
                WithAlpha(ForestShadow, 0.16f + Hash(rut * 19) * 0.08f));
            Raylib.DrawLineEx(new Vector2(x0 + 1f, L.ForecourtY + 3f), new Vector2(x0 + sway + 1f, groundY), 0.6f,
                WithAlpha(p.WetSheen, 0.08f));
        }

        for (int puddle = 0; puddle < 7; puddle++)
        {
            int seed = 10300 + puddle * 31;
            float px = 38f + Hash(seed) * (WindowWidth - 76f);
            float py = L.ForecourtY + 8f + Hash(seed + 5) * (L.ForecourtH - 16f);
            float rx = 10f + Hash(seed + 9) * 22f;
            float ry = 2.5f + Hash(seed + 13) * 5f;
            DrawEllipticalGlow(new Vector2(px, py), rx, ry, 0f, p.MoonGlow, 0.01f + Hash(seed + 17) * 0.006f, 2);
            Raylib.DrawEllipse((int)px, (int)py, rx, ry, WithAlpha(p.WetSheen, 0.045f));
        }

        DrawMenuCastleCrateStack(new Vector2(L.GateX - 96f, groundY), p, 0);
        DrawMenuCastleCrateStack(new Vector2(L.GateX + L.GateW + 82f, groundY), p, 1);

        for (int weed = 0; weed < 24; weed++)
        {
            int seed = 10600 + weed * 23;
            float x = Hash(seed) * WindowWidth;
            float y = L.ForecourtY + L.ForecourtH - 2f;
            float h = 4f + Hash(seed + 5) * 8f;
            Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x + (Hash(seed + 7) - 0.5f) * 4f, y - h),
                0.8f, WithAlpha(p.Moss, 0.22f + Hash(seed + 11) * 0.2f));
        }
    }

    static void DrawMenuCastleCrateStack(Vector2 foot, MenuCastlePalette p, int seed)
    {
        Color wood = new Color(50, 38, 28, 255);
        for (int box = 0; box < 4; box++)
        {
            float x = foot.X + (box % 2) * 16f + Hash(seed * 29 + box) * 3f;
            float y = foot.Y - 12f - (box / 2) * 13f;
            var r = new Rectangle(x, y, 16f, 12f);
            Raylib.DrawRectangleRounded(r, 0.1f, 2, WithAlpha(wood, 0.68f));
            Raylib.DrawRectangleRoundedLines(r, 0.1f, 2, WithAlpha(p.Iron, 0.24f));
            Raylib.DrawLine((int)r.X, (int)(r.Y + r.Height * 0.5f), (int)(r.X + r.Width), (int)(r.Y + r.Height * 0.5f), WithAlpha(p.Iron, 0.18f));
        }
    }

    static void DrawMenuCastleOblivionEnhancements(float time, MenuCastleLayout L, MenuCastlePalette p)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float pulse = MathF.Sin(time * 0.55f) * 0.5f + 0.5f;

        for (int beam = 0; beam < 5; beam++)
        {
            float bx = 80f + beam * ((WindowWidth - 160f) / 4f);
            float sway = MathF.Sin(time * 0.22f + beam * 1.4f) * 18f;
            Vector2 top = moon + new Vector2((bx - moon.X) * 0.08f, 40f);
            Vector2 foot = new Vector2(bx + sway, L.ForecourtY + 6f);
            Raylib.DrawLineEx(top, foot, 1.2f + beam * 0.15f, WithAlpha(p.MoonGlow, 0.018f + pulse * 0.012f));
        }

        for (int fly = 0; fly < 28; fly++)
        {
            float life = (time * (0.35f + Hash(fly * 11) * 0.25f) + fly * 0.37f) % 1f;
            float fx = Hash(fly * 19) * WindowWidth;
            float fy = L.WallY + 20f + life * (L.ForecourtY - L.WallY - 30f);
            fx += MathF.Sin(time * 2.4f + fly) * 14f;
            Color flyCol = Hash(fly * 7) > 0.5f ? p.TorchWarm : p.MoonGlow;
            Raylib.DrawCircleV(new Vector2(fx, fy), 1.1f + (1f - life) * 0.8f, WithAlpha(flyCol, (1f - life) * 0.42f));
        }

        for (int frost = 0; frost < 16; frost++)
        {
            float fx = 36f + frost * ((WindowWidth - 72f) / 15f);
            if (fx > L.GatehouseLeft - 12f && fx < L.GatehouseRight + 12f) continue;
            float fy = L.ParapetY - 4f + Hash(frost * 23) * 8f;
            Raylib.DrawCircleV(new Vector2(fx, fy), 1.2f, WithAlpha(p.MoonGlow, 0.12f + MathF.Sin(time * 3f + frost) * 0.06f));
        }

        DrawEllipticalGlow(moon, 92f + pulse * 16f, 92f + pulse * 16f, 0f, p.MoonGlow, 0.045f + pulse * 0.02f, 4);
        DrawEllipticalGlow(new Vector2(L.CenterX, L.GateY + L.GateH * 0.35f), L.GateW * 0.75f, L.GateH * 0.55f, 0f,
            p.TorchWarm, 0.035f + MathF.Sin(time * 3.1f) * 0.015f, 4);
    }

}
