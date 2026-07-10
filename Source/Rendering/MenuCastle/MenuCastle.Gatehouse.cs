partial class Program
{
    // -------------------------------------------------------------------------
    // Gatehouse ornament
    // -------------------------------------------------------------------------

    static void DrawMenuCastleShieldEmblem(Vector2 center, float time, MenuCastlePalette p)
    {
        float pulse = MathF.Sin(time * 1.8f) * 0.5f + 0.5f;
        var shield = new Rectangle(center.X - 18f, center.Y - 22f, 36f, 40f);
        Raylib.DrawRectangleRounded(shield, 0.35f, 6, WithAlpha(p.HeraldRedDeep, 0.72f));
        Raylib.DrawRectangleRounded(new Rectangle(shield.X + 2f, shield.Y + 2f, shield.Width - 4f, shield.Height * 0.42f),
            0.32f, 4, WithAlpha(p.HeraldRed, 0.58f));
        Raylib.DrawRectangleRoundedLines(shield, 0.35f, 6, WithAlpha(Gold, 0.35f + pulse * 0.15f));
        Raylib.DrawRectangleRoundedLines(new Rectangle(shield.X + 3f, shield.Y + 3f, shield.Width - 6f, shield.Height - 6f),
            0.32f, 4, WithAlpha(p.StoneHi, 0.25f));
        for (int i = 0; i < 4; i++)
        {
            float ry = center.Y - 10f + i * 5f;
            Raylib.DrawLineEx(new Vector2(center.X - 12f + i * 1.5f, ry), new Vector2(center.X + 12f - i * 1.5f, ry), 1.2f, WithAlpha(p.StoneHi, 0.3f));
        }
        Raylib.DrawLineEx(new Vector2(center.X, center.Y - 16f), new Vector2(center.X, center.Y + 12f), 2.2f, WithAlpha(Gold, 0.45f));
        Raylib.DrawLineEx(new Vector2(center.X - 8f, center.Y - 4f), new Vector2(center.X + 8f, center.Y - 4f), 1.8f, WithAlpha(Gold, 0.35f));
        Vector2 lionHead = new Vector2(center.X, center.Y - 6f);
        Raylib.DrawCircleV(lionHead, 5f, WithAlpha(p.StoneLight, 0.55f));
        Raylib.DrawCircleV(new Vector2(lionHead.X - 2f, lionHead.Y - 1f), 1.2f, WithAlpha(p.StoneDeep, 0.6f));
        Raylib.DrawCircleV(new Vector2(lionHead.X + 2f, lionHead.Y - 1f), 1.2f, WithAlpha(p.StoneDeep, 0.6f));
        Raylib.DrawTriangle(new Vector2(lionHead.X, lionHead.Y + 2f), new Vector2(lionHead.X - 3f, lionHead.Y + 6f), new Vector2(lionHead.X + 3f, lionHead.Y + 6f), WithAlpha(p.StoneMid, 0.5f));
        DrawEllipticalGlow(center, 22f, 26f, 0f, p.HeraldRedLight, 0.008f + pulse * 0.005f, 2);
    }

    static void DrawMenuCastleBanner(Vector2 top, float time, float phase, MenuCastlePalette p)
    {
        float wave = MathF.Sin(time * 2.4f + phase) * 5f;
        float fold = MathF.Sin(time * 3.3f + phase * 1.2f) * 0.5f + 0.5f;
        var banner = new Rectangle(top.X - 7f, top.Y, 14f, 32f + wave * 0.15f);
        Raylib.DrawRectangleRounded(banner, 0.2f, 3, WithAlpha(p.HeraldRedDeep, 0.82f));
        Raylib.DrawRectangleRounded(banner, 0.2f, 3, WithAlpha(LerpColor(p.HeraldRed, p.HeraldRedLight, fold), 0.78f));
        for (int foldIdx = 0; foldIdx < 4; foldIdx++)
        {
            float fx = top.X - 5f + foldIdx * 2.8f;
            Color foldCol = LerpColor(p.HeraldRedDeep, p.HeraldRedLight, foldIdx / 3f);
            Raylib.DrawLineEx(new Vector2(fx, top.Y), new Vector2(fx + wave * 0.05f + fold * 0.5f, top.Y + banner.Height),
                1f, WithAlpha(foldCol, 0.22f + fold * 0.12f));
        }
        Raylib.DrawRectangleRoundedLines(banner, 0.2f, 3, WithAlpha(Gold, 0.28f + fold * 0.1f));
        Raylib.DrawLineEx(top, new Vector2(top.X, top.Y - 16f), 1.5f, WithAlpha(p.StoneLight, 0.7f));
        Raylib.DrawCircleV(new Vector2(top.X, top.Y - 16f), 2f, WithAlpha(Gold, 0.42f));
    }

    static void DrawMenuCastlePennant(Vector2 anchor, float time, MenuCastlePalette p)
    {
        float wave = MathF.Sin(time * 2.2f) * 6f;
        float fold = MathF.Sin(time * 3.1f + 0.8f) * 0.5f + 0.5f;
        Vector2 tip = new Vector2(anchor.X + 36f + wave, anchor.Y + 28f + fold * 3f);
        Raylib.DrawTriangle(anchor, new Vector2(anchor.X + 1f, anchor.Y + 39f), tip + new Vector2(2f, 1f), WithAlpha(p.StoneDark, 0.45f));
        Raylib.DrawTriangle(anchor, new Vector2(anchor.X, anchor.Y + 38f), tip, WithAlpha(p.StoneLight, 0.78f));
        Raylib.DrawLineEx(anchor, new Vector2(anchor.X, anchor.Y + 42f), 2f, WithAlpha(p.StoneHi, 0.85f));
        Raylib.DrawLineEx(anchor, tip, 1.5f, WithAlpha(p.StoneHi, 0.6f));
        Raylib.DrawLineEx(new Vector2(anchor.X + 8f, anchor.Y + 18f), tip, 1f, WithAlpha(p.StoneHi, 0.22f + fold * 0.12f));
        for (int tassel = 0; tassel < 3; tassel++)
            Raylib.DrawCircleV(new Vector2(tip.X - tassel * 4f, tip.Y + tassel * 2f), 1.5f, WithAlpha(p.StoneHi, 0.45f));
    }

    static void DrawMenuCastleMiniTowerSeparationShadow(Rectangle body, float headroom, MenuCastlePalette p)
    {
        const float outerPad = 7f;
        const float rimPad = 3f;
        float rise = headroom + outerPad;
        var outer = new Rectangle(body.X - outerPad, body.Y - rise, body.Width + outerPad * 2f, body.Height + rise + outerPad + 5f);
        Raylib.DrawRectangleRounded(outer, 0.14f, 8, WithAlpha(ForestShadow, 0.46f));
        Raylib.DrawRectangleRounded(new Rectangle(outer.X + 2f, outer.Y + 2f, outer.Width - 4f, outer.Height - 4f),
            0.12f, 7, WithAlpha(ForestShadow, 0.26f));

        var drop = new Rectangle(body.X + 4f, body.Y + 5f, body.Width, body.Height + headroom);
        Raylib.DrawRectangleRounded(drop, 0.1f, 6, WithAlpha(ForestShadow, 0.22f));

        var rim = new Rectangle(body.X - rimPad, body.Y - headroom - rimPad, body.Width + rimPad * 2f, body.Height + headroom + rimPad + 6f);
        Raylib.DrawRectangleRoundedLines(rim, 0.1f, 6, WithAlpha(p.StoneDeep, 0.72f));
        Raylib.DrawRectangleRoundedLines(new Rectangle(rim.X + 1.5f, rim.Y + 1.5f, rim.Width - 3f, rim.Height - 3f),
            0.09f, 5, WithAlpha(ForestShadow, 0.38f));

        var contact = new Rectangle(body.X - 5f, body.Y + body.Height - 2f, body.Width + 10f, 12f);
        DrawGradientWash(contact, WithAlpha(ForestShadow, 0.58f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0f), 1.35f);
    }

    static void DrawMenuCastleTurret(Vector2 origin, float width, float height, MenuCastlePalette p, float time, float phase)
    {
        var body = new Rectangle(origin.X, origin.Y, width, height);
        DrawMenuCastleMiniTowerSeparationShadow(body, 30f, p);
        Raylib.DrawRectangleRounded(body, 0.1f, 6, WithAlpha(p.Stone, 0.88f));
        DrawMenuMasonryUltra(body, p, 6, 3);
        DrawTowerRoof(origin.X + width / 2f, origin.Y - 22f, origin.Y + 4f, width * 0.55f, p.StoneDark, p.StoneLight, p.StoneHi);
        DrawBattlements(new Rectangle(origin.X - 2f, origin.Y - 18f, width + 4f, 18f), p.StoneDark, p.StoneLight, p.StoneHi, 0.85f, time, true);
        DrawMenuTowerMoonRimLight(body, false, p);
        DrawMenuArrowSlit(new Vector2(origin.X + width / 2f, origin.Y + height * 0.35f), 6f, 20f, p);
        DrawCastleTorch(new Vector2(origin.X + width / 2f, origin.Y + height * 0.35f), time, phase, 0.75f, TorchMountKind.WallFace);
        DrawMenuTowerIronRing(new Vector2(origin.X + width * 0.5f, origin.Y + height * 0.15f), 3.5f, p);
    }

    static void DrawMenuCastleGate(Vector2 origin, float width, float height, MenuCastlePalette p, float time)
    {
        var arch = new Rectangle(origin.X, origin.Y, width, height);
        Raylib.DrawRectangleRounded(arch, 0.48f, 10, WithAlpha(p.StoneDeep, 0.98f));
        Raylib.DrawRectangleRoundedLines(arch, 0.48f, 10, WithAlpha(p.StoneLight, 0.8f));
        int segments = 15;
        float archCx = origin.X + width / 2f;
        float archCy = origin.Y + height * 0.08f;
        float archRx = width * 0.46f;
        float archRy = height * 0.22f;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            float tNext = (i + 1) / (float)(segments - 1);
            float a0 = MathF.PI + t * MathF.PI;
            float a1 = MathF.PI + tNext * MathF.PI;
            Vector2 inner0 = new Vector2(archCx + MathF.Cos(a0) * archRx * 0.88f, archCy + MathF.Sin(a0) * archRy * 0.88f);
            Vector2 inner1 = new Vector2(archCx + MathF.Cos(a1) * archRx * 0.88f, archCy + MathF.Sin(a1) * archRy * 0.88f);
            Vector2 outer0 = new Vector2(archCx + MathF.Cos(a0) * (archRx + 5f), archCy + MathF.Sin(a0) * (archRy + 3f));
            Vector2 outer1 = new Vector2(archCx + MathF.Cos(a1) * (archRx + 5f), archCy + MathF.Sin(a1) * (archRy + 3f));
            Color vCol = LerpColor(p.StoneDark, p.StoneLight, t);
            Raylib.DrawTriangle(inner0, outer0, outer1, WithAlpha(vCol, 0.92f));
            Raylib.DrawTriangle(inner0, outer1, inner1, WithAlpha(Darken(vCol, 0.08f), 0.9f));
            Raylib.DrawLineEx(outer0, outer1, 1f, WithAlpha(p.StoneHi, 0.18f + Hash(i * 11) * 0.12f));
        }
        Raylib.DrawCircleV(new Vector2(archCx, archCy - archRy * 0.55f), 5f, WithAlpha(p.StoneLight, 0.95f));
        Raylib.DrawCircleV(new Vector2(archCx, archCy - archRy * 0.55f), 3f, WithAlpha(p.StoneHi, 0.35f));
        float grateTop = origin.Y + height * 0.14f;
        float grateBot = origin.Y + height * 0.92f;
        int bars = 9;
        for (int b = 0; b < bars; b++)
        {
            float bx = origin.X + 10f + b * ((width - 20f) / (bars - 1));
            Raylib.DrawLineEx(new Vector2(bx, grateTop), new Vector2(bx, grateBot), 2.4f, WithAlpha(p.StoneHi, 0.55f));
            for (int rivet = 0; rivet < 5; rivet++)
            {
                float rivetY = grateTop + rivet * ((grateBot - grateTop) / 4f);
                Raylib.DrawCircleV(new Vector2(bx, rivetY), 1.2f, WithAlpha(p.Iron, 0.5f));
            }
        }
        for (int r = 0; r < 5; r++)
        {
            float gy = grateTop + r * ((grateBot - grateTop) / 4f);
            Raylib.DrawLineEx(new Vector2(origin.X + 8f, gy), new Vector2(origin.X + width - 8f, gy), 1.8f, WithAlpha(p.StoneLight, 0.4f));
        }
        // Chains
        for (int side = -1; side <= 1; side += 2)
        {
            float chainX = origin.X + (side < 0 ? 16f : width - 16f);
            for (int link = 0; link < 8; link++)
            {
                float ly = grateTop + link * 10f;
                Raylib.DrawEllipse((int)chainX, (int)ly, 3f, 5f, WithAlpha(p.Iron, 0.45f));
            }
        }
        // Hinge plates
        for (int h = 0; h < 3; h++)
        {
            float hy = origin.Y + height * (0.25f + h * 0.22f);
            Raylib.DrawRectangleRounded(new Rectangle(origin.X + 4f, hy, 10f, 14f), 0.15f, 2, WithAlpha(p.Iron, 0.55f));
            Raylib.DrawRectangleRounded(new Rectangle(origin.X + width - 14f, hy, 10f, 14f), 0.15f, 2, WithAlpha(p.Iron, 0.55f));
        }
        // Murder holes
        for (int mh = 0; mh < 4; mh++)
        {
            float mx = origin.X + width * (0.25f + mh * 0.16f);
            var hole = new Rectangle(mx - 4f, origin.Y + height * 0.05f, 8f, 10f);
            Raylib.DrawRectangleRounded(hole, 0.3f, 2, WithAlpha(ForestShadow, 0.9f));
            Raylib.DrawRectangleRoundedLines(hole, 0.3f, 2, WithAlpha(p.StoneHi, 0.25f));
        }
        var recess = new Rectangle(origin.X + width * 0.16f, origin.Y + height * 0.1f, width * 0.68f, height * 0.78f);
        Raylib.DrawRectangleRounded(recess, 0.38f, 8, WithAlpha(ForestShadow, 0.92f));
        float innerPulse = MathF.Sin(time * 3.2f) * 0.5f + 0.5f;
        DrawGradientWash(recess, WithAlpha(p.TorchWarm, 0.15f + innerPulse * 0.12f), WithAlpha(ForestShadow, 0f), new Vector2(0.5f, 0.7f), 2f);
        Raylib.DrawCircleV(new Vector2(origin.X + width / 2f, origin.Y + height * 0.06f), 7f, WithAlpha(p.StoneHi, 0.75f));
        float colW = 14f;
        Raylib.DrawRectangle((int)origin.X, (int)origin.Y, (int)colW, (int)height, WithAlpha(p.StoneDark, 0.9f));
        Raylib.DrawRectangle((int)(origin.X + width - colW), (int)origin.Y, (int)colW, (int)height, WithAlpha(p.StoneDark, 0.9f));
        Raylib.DrawLineEx(new Vector2(origin.X + 3f, origin.Y), new Vector2(origin.X + 3f, origin.Y + height), 1.5f, WithAlpha(p.StoneHi, 0.35f));
        Raylib.DrawLineEx(new Vector2(origin.X + width - 3f, origin.Y), new Vector2(origin.X + width - 3f, origin.Y + height), 1.5f, WithAlpha(p.StoneHi, 0.35f));
    }

    static void DrawMenuCastleDrawbridge(Vector2 topLeft, float width, float spanHeight, float time, MenuCastlePalette p)
    {
        float drop = MathF.Max(8f, spanHeight) + MathF.Sin(time * 0.5f) * 1.2f;
        var bridge = new Rectangle(topLeft.X + 8f, topLeft.Y, width - 16f, drop);
        Raylib.DrawRectangleRounded(bridge, 0.08f, 4, WithAlpha(p.StoneDark, 0.88f));
        Raylib.DrawRectangleGradientV((int)bridge.X, (int)bridge.Y, (int)bridge.Width, (int)bridge.Height,
            WithAlpha(p.StoneMid, 0.35f), WithAlpha(p.StoneDeep, 0f));
        int planks = 11;
        for (int plank = 0; plank < planks; plank++)
        {
            float px = bridge.X + 4f + plank * ((bridge.Width - 8f) / (planks - 1));
            Raylib.DrawLineEx(new Vector2(px, bridge.Y + 2f), new Vector2(px, bridge.Y + bridge.Height - 2f), 1.5f, WithAlpha(p.StoneLight, 0.28f));
            if (Hash(plank * 7) > 0.4f)
                Raylib.DrawCircleV(new Vector2(px + 2f, bridge.Y + bridge.Height * 0.5f), 1.2f, WithAlpha(p.Iron, 0.45f));
        }
        for (int row = 0; row < 3; row++)
        {
            float py = bridge.Y + 3f + row * (bridge.Height / 3f);
            Raylib.DrawLineEx(new Vector2(bridge.X + 2f, py), new Vector2(bridge.X + bridge.Width - 2f, py), 1f, WithAlpha(Darken(p.StoneLight, 0.2f), 0.25f));
        }
        for (int side = 0; side < 2; side++)
        {
            float cx = side == 0 ? topLeft.X + 14f : topLeft.X + width - 14f;
            Raylib.DrawLineEx(new Vector2(cx, topLeft.Y - 8f), new Vector2(cx, topLeft.Y + drop), 1.2f, WithAlpha(p.StoneLight, 0.35f));
            for (int link = 0; link < 4; link++)
            {
                float ly = topLeft.Y - 4f + link * 4f;
                Raylib.DrawEllipse((int)cx, (int)ly, 2f, 3.5f, WithAlpha(p.Iron, 0.4f));
            }
        }
    }

    static void DrawMenuCastleIvyPatches(float wallY, float wallH, float time, MenuCastlePalette p)
    {
        float gateSkip = MenuCastleLayoutCurrent.GatehouseW * 0.55f + 20f;
        for (int i = 0; i < 10; i++)
        {
            float x = WindowWidth * (0.06f + i * 0.095f);
            if (MathF.Abs(x - WindowWidth / 2f) < gateSkip) continue;
            float sway = MathF.Sin(time * 0.6f + i) * 3f;
            for (int v = 0; v < 6; v++)
            {
                float vy = wallY + wallH * (0.1f + v * 0.14f);
                float r = 3.5f - v * 0.35f + Hash(i * 11 + v) * 1.5f;
                Raylib.DrawCircleV(new Vector2(x + sway, vy), r, WithAlpha(p.Moss, 0.3f - v * 0.04f));
            }
        }
    }

    static void DrawMenuCastleFinishingPass(float time, MenuCastlePalette p, float wallY, float wallH,
        float gateX, float gateW, float gateY, float gateH)
    {
        // Weather streaks on stone
        DrawMenuCastleErosionStreaks(new Rectangle(0f, wallY, WindowWidth, wallH), p, time);
        for (int s = 0; s < 24; s++)
        {
            float sx = Hash(s * 37) * WindowWidth;
            float sy = wallY + Hash(s * 41) * wallH;
            float len = 20f + Hash(s * 43) * 40f;
            Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + Hash(s * 47) * 6f, sy + len), 1f, WithAlpha(p.WetSheen, 0.08f + Hash(s * 51) * 0.1f));
        }
        // Ambient occlusion in recesses
        var gateRecess = new Rectangle(gateX + gateW * 0.1f, gateY + gateH * 0.15f, gateW * 0.8f, gateH * 0.7f);
        DrawGradientWash(gateRecess, WithAlpha(p.StoneDeep, 0.35f), WithAlpha(p.StoneDeep, 0f), new Vector2(0.5f, 0f), 1.5f);
        Raylib.DrawRectangle(0, (int)(wallY - 6f), WindowWidth, 6, WithAlpha(p.StoneDeep, 0.5f));
        // Optional bat silhouettes
        if (Hash((int)(time * 10f)) > 0.3f)
        {
            for (int bat = 0; bat < 4; bat++)
            {
                float bx = WindowWidth * (0.15f + bat * 0.22f) + MathF.Sin(time * 1.2f + bat) * 30f;
                float by = WindowHeight * 0.22f + MathF.Sin(time * 0.8f + bat * 2f) * 20f;
                float wing = MathF.Sin(time * 6f + bat) * 4f;
                Raylib.DrawLineEx(new Vector2(bx - 6f, by), new Vector2(bx - wing, by - 3f), 1.2f, WithAlpha(p.StoneDeep, 0.5f));
                Raylib.DrawLineEx(new Vector2(bx + 6f, by), new Vector2(bx + wing, by - 3f), 1.2f, WithAlpha(p.StoneDeep, 0.5f));
                Raylib.DrawCircleV(new Vector2(bx, by), 1.5f, WithAlpha(p.StoneDeep, 0.55f));
            }
        }
        // Distant keep rim on horizon
        float keepY = wallY - 24f;
        var keep = new Rectangle(WindowWidth * 0.38f, keepY, WindowWidth * 0.24f, 48f);
        Raylib.DrawRectangleRounded(keep, 0.05f, 4, WithAlpha(p.StoneDeep, 0.65f));
        DrawBattlements(new Rectangle(keep.X - 4f, keep.Y - 14f, keep.Width + 8f, 14f), p.StoneDeep, p.StoneLight, p.StoneHi, 0.9f, time, true);
        DrawEllipticalGlow(new Vector2(keep.X + keep.Width / 2f, keep.Y), keep.Width * 0.6f, 8f, 0f, p.MoonGlow, 0.02f, 2);
        DrawMenuCastleMoonlitWallRim(wallY, wallH, p, time);
    }

}
