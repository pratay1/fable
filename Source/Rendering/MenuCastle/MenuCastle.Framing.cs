partial class Program
{
    // -------------------------------------------------------------------------
    // Main menu castle framing
    // -------------------------------------------------------------------------

    static void DrawMenuCastleHeraldicAccents(float time, MenuCastleLayout L, MenuCastlePalette p)
    {
        float pulse = MathF.Sin(time * 1.6f) * 0.5f + 0.5f;
        var gatehouse = L.GatehouseRect;

        // Thin crimson band beneath the gatehouse battlements
        Raylib.DrawRectangle((int)(gatehouse.X + 6f), (int)(L.BattlementsY + 26f), (int)(gatehouse.Width - 12f), 3,
            WithAlpha(p.HeraldRed, 0.42f + pulse * 0.08f));
        Raylib.DrawLineEx(new Vector2(gatehouse.X + 8f, L.BattlementsY + 25f),
            new Vector2(gatehouse.X + gatehouse.Width - 8f, L.BattlementsY + 25f), 1f, WithAlpha(Gold, 0.18f));

        // Small tower pennons
        ReadOnlySpan<(float x, float phase)> pennons = [(0.06f, 0f), (0.94f, 1.4f)];
        for (int i = 0; i < pennons.Length; i++)
        {
            var (nx, phase) = pennons[i];
            float wave = MathF.Sin(time * 2.6f + phase) * 4f;
            Vector2 pole = new Vector2(WindowWidth * nx, L.ParapetY - 18f);
            Vector2 tip = pole + new Vector2(14f + wave, 10f);
            Raylib.DrawLineEx(pole, new Vector2(pole.X, pole.Y - 12f), 1.2f, WithAlpha(p.StoneLight, 0.55f));
            Raylib.DrawTriangle(pole, new Vector2(pole.X + 1f, pole.Y + 14f), tip, WithAlpha(p.HeraldRed, 0.72f));
            Raylib.DrawLineEx(pole, tip, 1f, WithAlpha(Gold, 0.22f));
        }

        // Warm torch wash on stone near the gate — ties red banners to the masonry
        var heraldWash = new Rectangle(L.GateX - 28f, L.GateY - 18f, L.GateW + 56f, L.GateH * 0.35f);
        DrawGradientWash(heraldWash, WithAlpha(p.HeraldRedLight, 0.03f + pulse * 0.015f), WithAlpha(p.StoneDeep, 0f),
            new Vector2(0.5f, 0.2f), 1.8f);
    }

    static void DrawMenuCastleAmbientMoonlight(float wallY, float wallH, MenuCastlePalette p)
    {
        Raylib.DrawRectangleGradientH(0, (int)wallY, WindowWidth, (int)wallH,
            WithAlpha(p.MoonGlow, 0f), WithAlpha(p.MoonGlow, 0.07f));
        Raylib.DrawRectangleGradientV(0, (int)wallY, WindowWidth, (int)(wallH * 0.45f),
            WithAlpha(p.MoonGlow, 0.045f), WithAlpha(p.MoonGlow, 0f));
        Raylib.DrawRectangleGradientV(0, (int)(wallY + wallH - 52f), WindowWidth, 52,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.32f));
    }

    static void DrawMenuCastleFoundationCourse(float wallY, float wallH, MenuCastlePalette p)
    {
        var plinth = new Rectangle(0f, wallY + wallH - 28f, WindowWidth, 28f);
        Raylib.DrawRectangle((int)plinth.X, (int)plinth.Y, (int)plinth.Width, (int)plinth.Height, WithAlpha(p.StoneDeep, 0.95f));
        DrawMenuMasonryUltra(plinth, p, 2, 24);
        Raylib.DrawLine(0, (int)plinth.Y, WindowWidth, (int)plinth.Y, WithAlpha(p.StoneHi, 0.14f));
        Raylib.DrawLine(0, (int)(plinth.Y + plinth.Height), WindowWidth, (int)(plinth.Y + plinth.Height), WithAlpha(ForestShadow, 0.4f));
        for (int block = 0; block < 28; block++)
        {
            float bx = block * (WindowWidth / 28f);
            if (block % 2 == 0)
            {
                Raylib.DrawRectangle((int)bx, (int)(plinth.Y + 4f), (int)(WindowWidth / 28f) - 1, (int)(plinth.Height - 8f),
                    WithAlpha(p.StoneDark, 0.35f));
            }
        }
    }

    static void DrawMenuCastleFraming(float time)
    {
        MenuCastlePalette p = MenuPalette;
        MenuCastleLayout L = GetMenuCastleLayout();
        MenuCastleLayoutCurrent = L;

        DrawMenuCastleNightSky(time);
        DrawMenuCastleMoon(time);

        EnsureMenuCastleBake(L, p);
        var src = new Rectangle(0, 0, menuCastleBake.Texture.Width, -menuCastleBake.Texture.Height);
        var dest = new Rectangle(0f, 0f, WindowWidth, WindowHeight);
        Raylib.DrawTexturePro(menuCastleBake.Texture, src, dest, Vector2.Zero, 0f, Color.White);

        DrawMenuCastleBanner(new Vector2(L.GatehouseX + 10f, L.BattlementsY + 8f), time, 0f, p);
        DrawMenuCastleBanner(new Vector2(L.GatehouseX + L.GatehouseW - 10f, L.BattlementsY + 8f), time, 1.4f, p);
        DrawMenuCastleRichPennant(new Vector2(L.CenterX, L.BattlementsY + 4f), time, p);
        DrawMenuCastleHeraldicAccents(time, L, p);

        float gateFlicker = MathF.Sin(time * 4.6f) * 0.5f + 0.5f;
        Vector2 gateGlow = new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateH * 0.55f);
        DrawEllipticalGlow(gateGlow, L.GateW * 0.62f, L.GateH * 0.48f, 0f, p.TorchWarm, 0.06f + gateFlicker * 0.05f, 5);
        DrawLightCone(gateGlow, MathF.PI * 0.5f, L.GateH * 0.55f, 0.42f, p.TorchWarm, 0.04f + gateFlicker * 0.03f);

        DrawMenuCastleRampartTorches(L, time, p);

        Vector2 torchL = new Vector2(L.GateX - 36f, L.GateY + L.GateH * 0.12f);
        Vector2 torchR = new Vector2(L.GateX + L.GateW + 36f, L.GateY + L.GateH * 0.12f);
        DrawCastleTorch(torchL, time, 0f, 1.15f, TorchMountKind.WallRight);
        DrawCastleTorch(torchR, time, 1.8f, 1.15f, TorchMountKind.WallLeft);
        DrawCastleTorch(new Vector2(L.LeftTowerOrigin.X + L.TowerW * 0.72f, L.WallY + 52f), time, 3.2f, 0.9f, TorchMountKind.WallFace);
        DrawCastleTorch(new Vector2(L.RightTowerOrigin.X + L.TowerW * 0.28f, L.WallY + 52f), time, 4.5f, 0.9f, TorchMountKind.WallFace);
        DrawWallTorchWash(torchL, new Vector2(0.2f, 0.1f), time, 0f, 1.1f);
        DrawWallTorchWash(torchR, new Vector2(-0.2f, 0.1f), time, 1.8f, 1.1f);
        DrawWallTorchWash(new Vector2(L.LeftTowerOrigin.X + L.TowerW * 0.72f, L.WallY + 52f), new Vector2(0.3f, 0.05f), time, 3.2f, 0.85f);
        DrawWallTorchWash(new Vector2(L.RightTowerOrigin.X + L.TowerW * 0.28f, L.WallY + 52f), new Vector2(-0.3f, 0.05f), time, 4.5f, 0.85f);

        var wetZone = new Rectangle(L.GateX - 48f, L.ForecourtY, L.GateW + 96f, L.ForecourtH);
        Raylib.DrawRectangleGradientV((int)wetZone.X, (int)wetZone.Y, (int)wetZone.Width, (int)wetZone.Height,
            WithAlpha(p.TorchWarm, 0.06f + gateFlicker * 0.05f), WithAlpha(p.StoneDeep, 0f));
        for (int sheen = 0; sheen < 12; sheen++)
        {
            float shx = L.GateX + Hash(sheen * 13) * L.GateW;
            Raylib.DrawLineEx(new Vector2(shx, L.ForecourtY + 2f), new Vector2(shx + 8f, L.ForecourtY + 8f), 1f, WithAlpha(p.WetSheen, 0.12f + Hash(sheen * 17) * 0.1f));
        }

        DrawMenuCastleFinishingPass(time, p, L.WallY, L.WallH, L.GateX, L.GateW, L.GateY, L.GateH);
        DrawMenuCastleRefinementPass(time, p, L.WallY, L.WallH, L.GateX, L.GateW, L.GateY, L.GateH, L.GatehouseX, L.GatehouseW, L.GatehouseTop);
        var gatehouse = L.GatehouseRect;
        DrawMenuCastleReleasePass(time, p, L.WallY, L.WallH, L.GateX, L.GateW, L.GateY, L.GateH, L.GatehouseX, L.GatehouseW, L.GatehouseTop, gatehouse);
        DrawMenuCastleUltimateBeautyPass(time, p, L.WallY, L.WallH, L.GateX, L.GateW, L.GateY, L.GateH, L.GatehouseX, L.GatehouseW, L.GatehouseTop, gatehouse);
        DrawMenuCastleOblivionEnhancements(time, L, p);
        DrawMenuCastleProductionMasterPass(MenuCastleProductionPhase.Finalize, time, p);
        DrawMenuCastleMaximalDetailOverlay(time, L, p);
    }

    static void DrawMenuCastleMaximalDetailOverlay(float time, MenuCastleLayout L, MenuCastlePalette p)
    {
        DrawMenuCastleGateWinchAndChains(L, p, time);
        DrawMenuCastleWallWalkActivity(L, p, time);
        DrawMenuCastleAdditionalDefensiveLayers(L, p, time);
        DrawMenuCastleMasonMarksAndStoneNumbers(L, p);
        DrawMenuCastleRopesAndCables(L, p, time);
        DrawMenuCastleDeepShadowOcclusion(L, p);
        DrawMenuCastleForegroundDebrisAndTools(L, p, time);
        DrawMenuCastleWeatherAnimation(time, L, p);
    }

    static void DrawMenuCastleGateWinchAndChains(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        var winchRoom = new Rectangle(L.GatehouseX + L.GatehouseW * 0.30f, L.GatehouseTop + L.GatehouseH * 0.18f,
            L.GatehouseW * 0.40f, 28f);
        Raylib.DrawRectangleRounded(winchRoom, 0.18f, 4, WithAlpha(ForestShadow, 0.76f));
        Raylib.DrawRectangleRoundedLines(winchRoom, 0.18f, 4, WithAlpha(p.StoneHi, 0.22f));

        Vector2 axle = new Vector2(winchRoom.X + winchRoom.Width / 2f, winchRoom.Y + winchRoom.Height / 2f);
        Raylib.DrawCircleV(axle, 9f, WithAlpha(p.Iron, 0.58f));
        Raylib.DrawCircleV(axle, 4f, WithAlpha(p.StoneDeep, 0.9f));
        for (int spoke = 0; spoke < 6; spoke++)
        {
            float a = time * 0.08f + spoke * MathF.PI / 3f;
            Vector2 end = axle + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 12f;
            Raylib.DrawLineEx(axle, end, 1.3f, WithAlpha(p.Iron, 0.44f));
        }

        Vector2 leftChainTop = new Vector2(winchRoom.X + 10f, winchRoom.Y + winchRoom.Height - 2f);
        Vector2 rightChainTop = new Vector2(winchRoom.X + winchRoom.Width - 10f, winchRoom.Y + winchRoom.Height - 2f);
        DrawMenuCastleHeavyChain(leftChainTop, new Vector2(L.GateX + 14f, L.GateY + L.GateH * 0.32f), p, 5);
        DrawMenuCastleHeavyChain(rightChainTop, new Vector2(L.GateX + L.GateW - 14f, L.GateY + L.GateH * 0.32f), p, 6);

        for (int pin = 0; pin < 4; pin++)
        {
            float x = winchRoom.X + 14f + pin * ((winchRoom.Width - 28f) / 3f);
            Raylib.DrawCircleV(new Vector2(x, winchRoom.Y + winchRoom.Height - 5f), 1.4f, WithAlpha(p.StoneHi, 0.22f));
        }
    }

    static void DrawMenuCastleWallWalkActivity(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        ReadOnlySpan<float> guardXs = [88f, 212f, 308f, 492f, 594f, 710f];
        for (int i = 0; i < guardXs.Length; i++)
        {
            float x = guardXs[i] + MathF.Sin(time * 0.32f + i) * 1.6f;
            if (x > L.GatehouseLeft - 20f && x < L.GatehouseRight + 20f) continue;
            float y = L.ParapetY - 12f + MathF.Sin(time * 0.5f + i * 1.7f) * 0.8f;
            Color silhouette = WithAlpha(ForestShadow, 0.58f);
            Raylib.DrawCircleV(new Vector2(x, y - 7f), 2.6f, silhouette);
            Raylib.DrawLineEx(new Vector2(x, y - 5f), new Vector2(x, y + 5f), 2.2f, silhouette);
            Raylib.DrawLineEx(new Vector2(x, y - 1f), new Vector2(x + (i % 2 == 0 ? 7f : -7f), y + 2f), 1.4f, silhouette);
            Raylib.DrawLineEx(new Vector2(x + 4f, y - 12f), new Vector2(x + 4f, y + 6f), 0.8f, WithAlpha(p.Iron, 0.4f));
            if (i % 2 == 0)
            {
                Raylib.DrawTriangle(new Vector2(x + 4f, y - 12f), new Vector2(x + 12f, y - 8f), new Vector2(x + 4f, y - 4f),
                    WithAlpha(p.HeraldRed, 0.38f));
            }
        }

        for (int crate = 0; crate < 8; crate++)
        {
            float x = 42f + crate * 96f;
            if (x > L.GatehouseLeft - 26f && x < L.GatehouseRight + 26f) continue;
            float y = L.ParapetY - 4f + Hash(crate * 19) * 5f;
            var r = new Rectangle(x, y, 12f + Hash(crate * 23) * 8f, 7f);
            Raylib.DrawRectangleRounded(r, 0.12f, 2, WithAlpha(new Color(48, 38, 30, 255), 0.42f));
            Raylib.DrawRectangleRoundedLines(r, 0.12f, 2, WithAlpha(p.Iron, 0.16f));
        }

        for (int rack = 0; rack < 4; rack++)
        {
            float x = 136f + rack * 168f;
            if (x > L.GatehouseLeft - 16f && x < L.GatehouseRight + 16f) continue;
            float y = L.ParapetY - 7f;
            Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x + 28f, y), 1.2f, WithAlpha(new Color(54, 42, 32, 255), 0.44f));
            for (int spear = 0; spear < 5; spear++)
            {
                float sx = x + 3f + spear * 5f;
                Raylib.DrawLineEx(new Vector2(sx, y + 4f), new Vector2(sx + 2f, y - 16f), 0.7f, WithAlpha(p.Iron, 0.42f));
                Raylib.DrawTriangle(new Vector2(sx + 2f, y - 19f), new Vector2(sx - 1f, y - 14f), new Vector2(sx + 5f, y - 14f),
                    WithAlpha(p.StoneHi, 0.20f));
            }
        }
    }

    static void DrawMenuCastleAdditionalDefensiveLayers(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        for (int hoarding = 0; hoarding < 7; hoarding++)
        {
            float x = 50f + hoarding * 112f;
            if (x > L.GatehouseLeft - 36f && x < L.GatehouseRight + 36f) continue;
            var box = new Rectangle(x, L.ParapetY - 24f + Hash(hoarding * 7) * 4f, 58f, 18f);
            Color wood = new Color(45, 34, 26, 255);
            Raylib.DrawRectangleRounded(box, 0.08f, 3, WithAlpha(wood, 0.44f));
            Raylib.DrawLineEx(new Vector2(box.X, box.Y + box.Height), new Vector2(box.X + box.Width, box.Y + box.Height + 2f),
                1.1f, WithAlpha(ForestShadow, 0.3f));
            for (int plank = 1; plank < 5; plank++)
            {
                float px = box.X + plank * box.Width / 5f;
                Raylib.DrawLineEx(new Vector2(px, box.Y + 2f), new Vector2(px, box.Y + box.Height - 2f),
                    0.8f, WithAlpha(ForestShadow, 0.18f));
            }
            Raylib.DrawLineEx(new Vector2(box.X + 8f, box.Y), new Vector2(box.X + 2f, box.Y + box.Height + 10f),
                1f, WithAlpha(wood, 0.4f));
            Raylib.DrawLineEx(new Vector2(box.X + box.Width - 8f, box.Y), new Vector2(box.X + box.Width - 2f, box.Y + box.Height + 10f),
                1f, WithAlpha(wood, 0.4f));
        }

        for (int shield = 0; shield < 10; shield++)
        {
            float x = 60f + shield * 74f;
            if (x > L.GatehouseLeft - 24f && x < L.GatehouseRight + 24f) continue;
            float y = L.WallY + L.WallH * (0.72f + Hash(shield * 13) * 0.10f);
            Rectangle s = new Rectangle(x - 5f, y - 8f, 10f, 16f);
            Color herald = shield % 3 == 0 ? p.HeraldRedDeep : shield % 3 == 1 ? p.StoneDark : new Color(42, 48, 42, 255);
            Raylib.DrawRectangleRounded(s, 0.32f, 4, WithAlpha(herald, 0.42f));
            Raylib.DrawRectangleRoundedLines(s, 0.32f, 4, WithAlpha(p.StoneHi, 0.16f));
            Raylib.DrawLine((int)(s.X + s.Width / 2f), (int)(s.Y + 2f), (int)(s.X + s.Width / 2f), (int)(s.Y + s.Height - 2f),
                WithAlpha(p.StoneHi, 0.10f));
        }

        for (int scar = 0; scar < 28; scar++)
        {
            int seed = 11200 + scar * 47;
            float x = 28f + Hash(seed) * (WindowWidth - 56f);
            if (x > L.GatehouseLeft - 20f && x < L.GatehouseRight + 20f) continue;
            float y = L.WallY + 28f + Hash(seed + 3) * (L.WallH - 70f);
            float len = 8f + Hash(seed + 5) * 22f;
            float angle = -0.4f + Hash(seed + 7) * 0.8f;
            Vector2 a = new Vector2(x, y);
            Vector2 b = a + new Vector2(MathF.Sin(angle) * len, MathF.Cos(angle) * len);
            Raylib.DrawLineEx(a, b, 0.8f, WithAlpha(ForestShadow, 0.18f));
            Raylib.DrawLineEx(a + new Vector2(0.7f, -0.6f), b + new Vector2(0.7f, -0.6f), 0.45f, WithAlpha(p.StoneHi, 0.045f));
        }
    }

    static void DrawMenuCastleMasonMarksAndStoneNumbers(MenuCastleLayout L, MenuCastlePalette p)
    {
        for (int mark = 0; mark < 46; mark++)
        {
            int seed = 11800 + mark * 31;
            float x = 24f + Hash(seed) * (WindowWidth - 48f);
            if (x > L.GatehouseLeft - 18f && x < L.GatehouseRight + 18f) continue;
            float y = L.WallY + 20f + Hash(seed + 5) * (L.WallH - 48f);
            float a = 0.045f + Hash(seed + 7) * 0.055f;
            int type = (int)(Hash(seed + 11) * 4f);
            Color c = WithAlpha(p.StoneHi, a);
            switch (type)
            {
                case 0:
                    Raylib.DrawLineEx(new Vector2(x - 3f, y), new Vector2(x + 3f, y), 0.6f, c);
                    Raylib.DrawLineEx(new Vector2(x, y - 3f), new Vector2(x, y + 3f), 0.6f, c);
                    break;
                case 1:
                    Raylib.DrawLineEx(new Vector2(x - 3f, y + 3f), new Vector2(x, y - 3f), 0.6f, c);
                    Raylib.DrawLineEx(new Vector2(x, y - 3f), new Vector2(x + 3f, y + 3f), 0.6f, c);
                    break;
                case 2:
                    Raylib.DrawCircleLines((int)x, (int)y, 3f, c);
                    break;
                default:
                    Raylib.DrawLineEx(new Vector2(x - 3f, y - 2f), new Vector2(x + 3f, y + 2f), 0.6f, c);
                    Raylib.DrawLineEx(new Vector2(x - 3f, y + 2f), new Vector2(x + 3f, y - 2f), 0.6f, c);
                    break;
            }
        }
    }

    static void DrawMenuCastleRopesAndCables(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        Color rope = new Color(84, 68, 48, 255);
        Vector2[] anchors =
        [
            new Vector2(L.LeftTowerOrigin.X + L.TowerW * 0.82f, L.ParapetY - 20f),
            new Vector2(L.GatehouseX + 18f, L.BattlementsY + 8f),
            new Vector2(L.GatehouseX + L.GatehouseW - 18f, L.BattlementsY + 8f),
            new Vector2(L.RightTowerOrigin.X + L.TowerW * 0.18f, L.ParapetY - 20f),
        ];
        for (int i = 0; i < anchors.Length - 1; i++)
        {
            Vector2 a = anchors[i];
            Vector2 b = anchors[i + 1];
            int segments = 14;
            Vector2 prev = a;
            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)segments;
                Vector2 pos = Vector2.Lerp(a, b, t);
                float sag = MathF.Sin(t * MathF.PI) * (12f + i * 2f);
                pos.Y += sag + MathF.Sin(time * 0.45f + i + t * 4f) * 1.2f;
                Raylib.DrawLineEx(prev, pos, 0.8f, WithAlpha(rope, 0.30f));
                prev = pos;
            }
        }

        for (int cloth = 0; cloth < 9; cloth++)
        {
            float t = (cloth + 0.5f) / 9f;
            Vector2 a = Vector2.Lerp(anchors[0], anchors[^1], t);
            a.Y += MathF.Sin(t * MathF.PI) * 13f;
            Color rag = cloth % 3 == 0 ? p.HeraldRedDeep : cloth % 3 == 1 ? p.StoneDark : new Color(60, 54, 44, 255);
            float wave = MathF.Sin(time * 1.2f + cloth) * 2f;
            Raylib.DrawTriangle(a, a + new Vector2(8f + wave, 6f), a + new Vector2(1f, 15f), WithAlpha(rag, 0.34f));
        }
    }

    static void DrawMenuCastleDeepShadowOcclusion(MenuCastleLayout L, MenuCastlePalette p)
    {
        Raylib.DrawRectangleGradientV((int)(L.GatehouseX - 18f), (int)(L.GatehouseTop + L.GatehouseH * 0.40f),
            (int)(L.GatehouseW + 36f), (int)(L.GatehouseH * 0.60f),
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.18f));
        Raylib.DrawRectangleGradientH(0, (int)L.WallY, (int)(L.TowerW * 0.9f), (int)L.WallH,
            WithAlpha(ForestShadow, 0.23f), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH((int)(WindowWidth - L.TowerW * 0.9f), (int)L.WallY, (int)(L.TowerW * 0.9f), (int)L.WallH,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.23f));

        DrawEllipticalGlow(new Vector2(L.GateX + L.GateW / 2f, L.GateY + L.GateH * 0.72f),
            L.GateW * 0.58f, L.GateH * 0.22f, 0f, ForestShadow, 0.06f, 4);
        Raylib.DrawLineEx(new Vector2(0f, L.ForecourtY - 1f), new Vector2(WindowWidth, L.ForecourtY - 1f),
            2f, WithAlpha(ForestShadow, 0.32f));
    }

    static void DrawMenuCastleForegroundDebrisAndTools(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float groundY = L.ForecourtY + L.ForecourtH - 4f;
        Color wood = new Color(52, 40, 30, 255);
        for (int plank = 0; plank < 12; plank++)
        {
            int seed = 12400 + plank * 29;
            float x = 24f + Hash(seed) * (WindowWidth - 48f);
            float y = L.ForecourtY + 12f + Hash(seed + 5) * (L.ForecourtH - 18f);
            float len = 14f + Hash(seed + 7) * 28f;
            float rot = (Hash(seed + 11) - 0.5f) * 0.6f;
            Vector2 a = new Vector2(x, y);
            Vector2 b = a + new Vector2(MathF.Cos(rot) * len, MathF.Sin(rot) * len);
            Raylib.DrawLineEx(a + new Vector2(1f, 2f), b + new Vector2(1f, 2f), 2.6f, WithAlpha(ForestShadow, 0.22f));
            Raylib.DrawLineEx(a, b, 2.2f, WithAlpha(wood, 0.42f));
            Raylib.DrawLineEx(a, b, 0.5f, WithAlpha(p.StoneHi, 0.08f));
        }

        for (int stone = 0; stone < 22; stone++)
        {
            int seed = 12900 + stone * 17;
            float x = Hash(seed) * WindowWidth;
            float y = groundY - Hash(seed + 3) * 18f;
            float r = 1.4f + Hash(seed + 5) * 3.8f;
            Color c = LerpColor(p.StoneDeep, p.StoneLight, Hash(seed + 7) * 0.45f);
            Raylib.DrawCircleV(new Vector2(x + 1f, y + 1f), r, WithAlpha(ForestShadow, 0.18f));
            Raylib.DrawCircleV(new Vector2(x, y), r, WithAlpha(c, 0.30f));
            Raylib.DrawCircleV(new Vector2(x - r * 0.25f, y - r * 0.25f), r * 0.35f, WithAlpha(p.StoneHi, 0.08f));
        }

        Vector2 anvil = new Vector2(L.GateX + L.GateW + 122f, groundY - 4f);
        Raylib.DrawRectangleRounded(new Rectangle(anvil.X - 14f, anvil.Y - 8f, 28f, 7f), 0.25f, 3, WithAlpha(p.Iron, 0.36f));
        Raylib.DrawRectangleRounded(new Rectangle(anvil.X - 5f, anvil.Y - 3f, 10f, 7f), 0.1f, 2, WithAlpha(p.Iron, 0.32f));
        float glint = MathF.Sin(time * 2.1f) * 0.5f + 0.5f;
        Raylib.DrawLineEx(new Vector2(anvil.X - 12f, anvil.Y - 8f), new Vector2(anvil.X + 12f, anvil.Y - 8f),
            0.8f, WithAlpha(p.StoneHi, 0.08f + glint * 0.08f));

        Vector2 wheel = new Vector2(L.GateX - 126f, groundY - 7f);
        Raylib.DrawCircleLines((int)wheel.X, (int)wheel.Y, 9f, WithAlpha(wood, 0.42f));
        Raylib.DrawCircleLines((int)wheel.X, (int)wheel.Y, 5f, WithAlpha(wood, 0.30f));
        for (int spoke = 0; spoke < 6; spoke++)
        {
            float a = spoke * MathF.PI / 3f;
            Raylib.DrawLineEx(wheel, wheel + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 9f, 0.7f, WithAlpha(wood, 0.35f));
        }
    }

    static void DrawMenuCastleWeatherAnimation(float time, MenuCastleLayout L, MenuCastlePalette p)
    {
        for (int drop = 0; drop < 44; drop++)
        {
            int seed = 13400 + drop * 19;
            float speed = 0.12f + Hash(seed + 3) * 0.18f;
            float t = (time * speed + Hash(seed)) % 1f;
            float x = Hash(seed + 7) * WindowWidth + MathF.Sin(time * 0.2f + drop) * 8f;
            float y = L.WallY - 40f + t * (L.ForecourtY - L.WallY + 70f);
            float len = 6f + Hash(seed + 11) * 11f;
            Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x - 2.5f, y + len), 0.55f,
                WithAlpha(p.MoonGlow, 0.035f + Hash(seed + 13) * 0.035f));
        }

        for (int mist = 0; mist < 9; mist++)
        {
            float x = WindowWidth * (mist / 8f) + MathF.Sin(time * 0.14f + mist) * 24f;
            float y = L.ForecourtY - 10f + MathF.Sin(time * 0.18f + mist) * 4f;
            DrawEllipticalGlow(new Vector2(x, y), 74f + Hash(mist * 17) * 32f, 9f + Hash(mist * 19) * 5f,
                Hash(mist * 23) * 12f, p.MoonGlow, 0.006f + Hash(mist * 29) * 0.006f, 2);
        }
    }

    static void DrawMenuCastleCurtainBattlements(MenuCastleLayout L, MenuCastlePalette p, float time)
    {
        float bh = 26f;
        float gap = 18f;
        var left = new Rectangle(0f, L.ParapetY - bh + 8f, L.GatehouseLeft - gap, bh);
        var right = new Rectangle(L.GatehouseRight + gap, L.ParapetY - bh + 8f, WindowWidth - L.GatehouseRight - gap, bh);
        if (left.Width > 40f)
            DrawBattlements(left, p.StoneDark, p.StoneLight, p.StoneHi, 1f, time, true);
        if (right.Width > 40f)
            DrawBattlements(right, p.StoneDark, p.StoneLight, p.StoneHi, 1f, time, true);
    }

    static void DrawMenuCastleReleasePass(float time, MenuCastlePalette p, float wallY, float wallH,
        float gateX, float gateW, float gateY, float gateH, float gatehouseX, float gatehouseW, float gatehouseY,
        Rectangle gatehouse)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float gateFlicker = MathF.Sin(time * 4.6f) * 0.5f + 0.5f;

        // Ground-hugging ambient occlusion where the wall meets the world
        Raylib.DrawRectangleGradientV(0, (int)(wallY - 6f), WindowWidth, 28,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.38f));
        Raylib.DrawRectangleGradientV(0, (int)(MenuCastleLayoutCurrent.ForecourtY - 4f), WindowWidth, (int)MenuCastleLayoutCurrent.ForecourtH + 4,
            WithAlpha(ForestShadow, 0.22f), WithAlpha(ForestShadow, 0f));

        // Gatehouse string courses - horizontal masonry bands for visual weight
        for (int course = 0; course < 3; course++)
        {
            float cy = gatehouse.Y + gatehouse.Height * (0.22f + course * 0.22f);
            Raylib.DrawLineEx(new Vector2(gatehouse.X + 8f, cy), new Vector2(gatehouse.X + gatehouse.Width - 8f, cy),
                1f, WithAlpha(p.StoneLight, 0.10f + Hash(course * 9) * 0.08f));
            Raylib.DrawLineEx(new Vector2(gatehouse.X + 8f, cy + 1f), new Vector2(gatehouse.X + gatehouse.Width - 8f, cy + 1f),
                1f, WithAlpha(p.StoneDeep, 0.18f));
        }

        // Quoin corner depth on gatehouse
        Raylib.DrawRectangleGradientH((int)gatehouse.X, (int)gatehouse.Y, 14, (int)gatehouse.Height,
            WithAlpha(ForestShadow, 0.22f), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH((int)(gatehouse.X + gatehouse.Width - 14f), (int)gatehouse.Y, 14, (int)gatehouse.Height,
            WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.22f));

        // Rising embers from gate torches
        Vector2[] emberSources =
        [
            new Vector2(gateX - 42f, gateY + 12f),
            new Vector2(gateX + gateW + 42f, gateY + 12f),
        ];
        for (int e = 0; e < emberSources.Length; e++)
        {
            for (int em = 0; em < 6; em++)
            {
                float life = (time * (1.6f + em * 0.3f) + e * 2f + em * 0.7f) % 1f;
                Vector2 pos = emberSources[e] + new Vector2(MathF.Sin(life * 12f + em) * 8f, -life * 42f);
                Raylib.DrawCircleV(pos, 1.2f - life * 0.6f, WithAlpha(p.TorchWarm, (1f - life) * 0.55f));
            }
        }

        // Moon-silver reflection band on wet forecourt cobbles
        float reflectX = gateX + gateW / 2f + (moon.X - (gateX + gateW / 2f)) * 0.15f;
        DrawEllipticalGlow(new Vector2(reflectX, MenuCastleLayoutCurrent.ForecourtY + 12f), gateW * 0.42f, 10f, 0f,
            p.MoonGlow, 0.016f + MathF.Sin(time * 0.35f) * 0.004f, 3);

        for (int m = 0; m < 22; m++)
        {
            float mx = 48f + m * ((WindowWidth - 96f) / 21f);
            if (mx > MenuCastleLayoutCurrent.GatehouseLeft - 8f && mx < MenuCastleLayoutCurrent.GatehouseRight + 8f) continue;
            bool lit = m % 2 == 0;
            Raylib.DrawRectangle((int)mx, (int)(MenuCastleLayoutCurrent.ParapetY - 8f), 9, lit ? 6 : 4,
                WithAlpha(lit ? p.StoneHi : ForestShadow, lit ? 0.14f : 0.28f));
        }

        // Ivy rim highlights catching moon on wall top edge
        for (int iv = 0; iv < 14; iv++)
        {
            float ix = Hash(iv * 41) * WindowWidth;
            float iy = wallY + 8f + Hash(iv * 43) * (wallH * 0.55f);
            if (Hash(iv * 47) > 0.55f)
                Raylib.DrawCircleV(new Vector2(ix, iy), 1.5f + Hash(iv * 53), WithAlpha(p.Lichen, 0.12f + Hash(iv * 59) * 0.12f));
        }

        // Faint warm pulse inside gatehouse arrow-slit windows
        float[] winCols = { gatehouseX + gatehouseW * 0.22f, gatehouseX + gatehouseW * 0.5f, gatehouseX + gatehouseW * 0.78f };
        float winRowY = gatehouse.Y + gatehouse.Height * 0.32f;
        for (int w = 0; w < winCols.Length; w++)
        {
            float pulse = MathF.Sin(time * 1.5f + w * 1.7f) * 0.5f + 0.5f;
            var win = new Rectangle(winCols[w] - 8f, winRowY, 16f, 22f);
            DrawEllipticalGlow(new Vector2(win.X + win.Width / 2f, win.Y + win.Height / 2f),
                win.Width * 0.7f, win.Height * 0.65f, 0f, p.TorchWarm, 0.012f + pulse * 0.01f, 2);
        }

        // Shield emblem polish - subtle gold edge catch
        Vector2 shieldCenter = new Vector2(WindowWidth / 2f, gatehouse.Y + gatehouse.Height * 0.12f);
        Raylib.DrawCircleLines((int)shieldCenter.X, (int)(shieldCenter.Y - 2f), 14f, WithAlpha(Gold, 0.12f + gateFlicker * 0.08f));

        Vector2 pennantAnchor = new Vector2(WindowWidth / 2f, MenuCastleLayoutCurrent.BattlementsY + 4f);
        Raylib.DrawLineEx(pennantAnchor + new Vector2(4f, 4f), pennantAnchor + new Vector2(40f, 34f), 2f, WithAlpha(ForestShadow, 0.25f));

        // Distant silhouette birds crossing the moon path
        for (int bat = 0; bat < 3; bat++)
        {
            float t = (time * 0.08f + bat * 0.31f) % 1f;
            float bx = WindowWidth * (0.1f + t * 0.8f);
            float by = WindowHeight * (0.14f + bat * 0.04f) + MathF.Sin(time * 1.1f + bat) * 8f;
            float wing = MathF.Sin(time * 7f + bat * 2f) * 3.5f;
            Raylib.DrawLineEx(new Vector2(bx - 5f, by), new Vector2(bx - wing, by - 2f), 1f, WithAlpha(p.StoneDeep, 0.4f));
            Raylib.DrawLineEx(new Vector2(bx + 5f, by), new Vector2(bx + wing, by - 2f), 1f, WithAlpha(p.StoneDeep, 0.4f));
        }

        // Tower roof edge highlights on both flanking towers
        MenuCastleLayout L = MenuCastleLayoutCurrent;
        ReadOnlySpan<(float x, float y)> towerRims =
        [
            (L.LeftTowerOrigin.X + L.TowerW * 0.5f, L.ParapetY - 28f),
            (L.RightTowerOrigin.X + L.TowerW * 0.5f, L.ParapetY - 28f),
        ];
        for (int t = 0; t < towerRims.Length; t++)
        {
            var (tx, ty) = towerRims[t];
            Raylib.DrawLineEx(new Vector2(tx - 28f, ty), new Vector2(tx + 28f, ty - 5f), 1.2f, WithAlpha(p.StoneHi, 0.16f));
            DrawEllipticalGlow(new Vector2(tx, ty - 2f), 34f, 8f, -4f, p.MoonGlow, 0.01f, 2);
        }

        // Portcullis depth shadow behind bars
        var gateDepth = new Rectangle(gateX + gateW * 0.12f, gateY + gateH * 0.18f, gateW * 0.76f, gateH * 0.68f);
        DrawGradientWash(gateDepth, WithAlpha(ForestShadow, 0.5f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0f), 1.6f);
    }

    static void DrawMenuCastleRefinementPass(float time, MenuCastlePalette p, float wallY, float wallH,
        float gateX, float gateW, float gateY, float gateH, float gatehouseX, float gatehouseW, float gatehouseY)
    {
        float moonX = WindowWidth * 0.76f;
        float gateFlicker = MathF.Sin(time * 4.6f) * 0.5f + 0.5f;
        MenuCastleLayout L = MenuCastleLayoutCurrent;

        // Chimney smoke drifting from gatehouse towers
        Vector2[] chimneys =
        {
            new Vector2(gatehouseX + 28f, L.BattlementsY + 2f),
            new Vector2(gatehouseX + gatehouseW - 28f, L.BattlementsY + 2f),
        };
        for (int c = 0; c < chimneys.Length; c++)
        {
            for (int w = 0; w < 5; w++)
            {
                float drift = time * (0.7f + c * 0.25f) + w * 0.9f;
                float wx = chimneys[c].X + MathF.Sin(drift) * (6f + w * 2.5f);
                float wy = chimneys[c].Y - 10f - w * 9f;
                DrawEllipticalGlow(new Vector2(wx, wy), 8f + w * 3f, 4f + w * 1.5f, Hash(c * 5 + w) * 18f,
                    new Color(52, 50, 48, 255), 0.011f - w * 0.0015f, 2);
            }
        }

        // Iron glints on the portcullis
        float grateTop = gateY + gateH * 0.14f;
        float grateBot = gateY + gateH * 0.92f;
        for (int b = 0; b < 9; b += 2)
        {
            float bx = gateX + 10f + b * ((gateW - 20f) / 8f);
            float gy = grateTop + Hash(b * 13) * (grateBot - grateTop);
            if (Hash(b * 17 + (int)(time * 2.4f)) > 0.82f)
                Raylib.DrawCircleV(new Vector2(bx, gy), 1.6f, WithAlpha(p.StoneHi, 0.45f + gateFlicker * 0.15f));
        }

        // Wet cobble sparkles in the torch-lit forecourt
        for (int s = 0; s < 20; s++)
        {
            float sx = gateX - 44f + Hash(s * 23) * (gateW + 88f);
            float sy = L.ForecourtY + 2f + Hash(s * 29) * (L.ForecourtH - 4f);
            float tw = MathF.Sin(time * 3.8f + s * 1.6f) * 0.5f + 0.5f;
            if (tw > 0.78f)
                Raylib.DrawCircleV(new Vector2(sx, sy), 0.8f + Hash(s * 31), WithAlpha(p.WetSheen, 0.18f + tw * 0.22f));
        }

        // Moonlit caps on the flanking towers and gatehouse
        ReadOnlySpan<Vector2> peaks =
        [
            new Vector2(L.LeftTowerOrigin.X + L.TowerW * 0.5f, L.ParapetY - 18f),
            new Vector2(L.RightTowerOrigin.X + L.TowerW * 0.5f, L.ParapetY - 18f),
            new Vector2(gatehouseX + gatehouseW / 2f, L.BattlementsY - 4f),
        ];
        for (int i = 0; i < peaks.Length; i++)
        {
            float facing = Math.Clamp(1f - MathF.Abs(peaks[i].X - moonX) / 260f, 0.15f, 1f);
            DrawEllipticalGlow(peaks[i], 22f * facing, 7f, -6f, p.MoonGlow, 0.013f * facing, 2);
            Raylib.DrawLineEx(new Vector2(peaks[i].X - 10f, peaks[i].Y + 1f), new Vector2(peaks[i].X + 10f, peaks[i].Y - 3f),
                1f, WithAlpha(p.StoneHi, 0.14f * facing));
        }

        // Keystone catch-light on the gate arch
        Vector2 keystone = new Vector2(gateX + gateW / 2f, gateY + gateH * 0.06f);
        Raylib.DrawCircleV(keystone, 3.5f, WithAlpha(p.StoneHi, 0.32f + MathF.Sin(time * 1.1f) * 0.08f));

        // Warm torch spill across the gatehouse lintel
        var lintelSpill = new Rectangle(gatehouseX + 16f, gatehouseY + 8f, gatehouseW - 32f, 28f);
        Raylib.DrawRectangleGradientV((int)lintelSpill.X, (int)lintelSpill.Y, (int)lintelSpill.Width, (int)lintelSpill.Height,
            WithAlpha(p.TorchWarm, 0f), WithAlpha(p.TorchWarm, 0.035f + gateFlicker * 0.025f));

        // Deep merlon shadow teeth along the wall walk
        for (int m = 0; m < 18; m++)
        {
            float mx = 56f + m * ((WindowWidth - 112f) / 17f);
            if (mx > L.GatehouseLeft - 10f && mx < L.GatehouseRight + 10f) continue;
            Raylib.DrawRectangle((int)mx, (int)(L.ParapetY - 5f), 7, 5, WithAlpha(ForestShadow, 0.32f));
        }

        // Drawbridge chain sway and hinge gleam
        for (int side = 0; side < 2; side++)
        {
            float cx = side == 0 ? gateX + 14f : gateX + gateW - 14f;
            float sway = MathF.Sin(time * 1.4f + side * 2.1f) * 2f;
            for (int link = 0; link < 6; link++)
            {
                float ly = gateY + gateH * 0.2f + link * 9f;
                Raylib.DrawEllipse((int)(cx + sway), (int)ly, 2.5f, 4f, WithAlpha(p.Iron, 0.42f));
            }
            Raylib.DrawCircleV(new Vector2(cx + sway, gateY + gateH * 0.18f), 2f, WithAlpha(p.StoneHi, 0.35f));
        }
    }

}
