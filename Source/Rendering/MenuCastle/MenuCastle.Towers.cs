partial class Program
{
    // -------------------------------------------------------------------------
    // Tower detail helpers
    // -------------------------------------------------------------------------

    static void DrawMenuTowerBellOpening(Vector2 center, float width, float height, MenuCastlePalette p, float time)
    {
float sway = MathF.Sin(time * 0.9f) * 0.5f;
        var arch = new Rectangle(center.X - width / 2f, center.Y - height, width, height);
        Raylib.DrawRectangleRounded(arch, 0.45f, 6, WithAlpha(p.StoneDeep, 0.92f));
        Raylib.DrawRectangleRoundedLines(arch, 0.45f, 6, WithAlpha(p.StoneHi, 0.4f));
        Raylib.DrawLineEx(new Vector2(center.X, arch.Y), new Vector2(center.X, arch.Y + height), 1.5f, WithAlpha(p.Iron, 0.35f));
        Raylib.DrawCircleV(new Vector2(center.X + sway, center.Y - height * 0.35f), 3f, WithAlpha(p.Iron, 0.5f));
    }

    static void DrawMenuTowerMachicolations(Rectangle top, MenuCastlePalette p)
    {
int slots = Math.Max(5, (int)(top.Width / 18f));
        float slotW = top.Width / slots;
        for (int i = 0; i < slots; i++)
        {
            if (i % 2 == 0) continue;
            var slot = new Rectangle(top.X + i * slotW + 2f, top.Y + top.Height * 0.35f, slotW - 4f, top.Height * 0.55f);
            Raylib.DrawRectangleRounded(slot, 0.15f, 3, WithAlpha(p.StoneDeep, 0.85f));
            Raylib.DrawLine((int)slot.X, (int)slot.Y, (int)(slot.X + slot.Width), (int)slot.Y, WithAlpha(p.StoneHi, 0.2f));
        }
    }

    static void DrawMenuTowerCorbelTable(float x, float y, float width, MenuCastlePalette p)
    {
int corbels = Math.Max(6, (int)(width / 12f));
        for (int i = 0; i < corbels; i++)
        {
            float cx = x + i * (width / (corbels - 1));
            float protrude = 3f + Hash(i * 7) * 3f;
            Raylib.DrawTriangle(
                new Vector2(cx - 4f, y),
                new Vector2(cx + 4f, y),
                new Vector2(cx, y - protrude),
                WithAlpha(p.StoneDark, 0.8f));
            Raylib.DrawLineEx(new Vector2(cx, y - protrude), new Vector2(cx, y - protrude - 2f), 1f, WithAlpha(p.StoneHi, 0.25f));
        }
    }

    static void DrawMenuTowerGargoyle(Vector2 pos, bool facingLeft, MenuCastlePalette p)
    {
        DrawMenuCastleSculptedGargoyle(pos, facingLeft, p, 0f);
    }

    static void DrawMenuTowerIronRing(Vector2 pos, float radius, MenuCastlePalette p)
    {
Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, radius, WithAlpha(p.Iron, 0.55f));
        Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, radius - 1.5f, WithAlpha(Darken(p.Iron, 0.3f), 0.35f));
        Raylib.DrawCircleV(new Vector2(pos.X + radius * 0.6f, pos.Y), 1.2f, WithAlpha(p.StoneHi, 0.3f));
    }

    static void DrawMenuTowerDrainSpout(Vector2 pos, bool left, MenuCastlePalette p)
    {
float dir = left ? -1f : 1f;
        Raylib.DrawRectangleRounded(new Rectangle(pos.X, pos.Y, 8f * dir, 5f), 0.2f, 2, WithAlpha(p.StoneDark, 0.8f));
        Raylib.DrawLineEx(pos, new Vector2(pos.X + dir * 14f, pos.Y + 10f), 1.5f, WithAlpha(p.StoneDeep, 0.6f));
        for (int drip = 0; drip < 3; drip++)
            Raylib.DrawCircleV(new Vector2(pos.X + dir * (10f + drip * 2f), pos.Y + 12f + drip * 4f), 0.8f, WithAlpha(p.WetSheen, 0.15f));
    }

    static void DrawMenuTowerMoonRimLight(Rectangle body, bool moonSideLeft, MenuCastlePalette p)
    {
float edgeX = moonSideLeft ? body.X + body.Width - 2f : body.X + 2f;
        Raylib.DrawLineEx(new Vector2(edgeX, body.Y + 8f), new Vector2(edgeX, body.Y + body.Height - 8f), 2f, WithAlpha(p.MoonGlow, 0.12f));
        for (int i = 0; i < 6; i++)
        {
            float ty = body.Y + body.Height * (0.15f + i * 0.14f);
            Raylib.DrawCircleV(new Vector2(edgeX + (moonSideLeft ? -1f : 1f), ty), 1.5f, WithAlpha(p.StoneHi, 0.18f));
        }
    }

    static void DrawMenuTowerJointMoss(Rectangle region, MenuCastlePalette p, int density)
    {
for (int i = 0; i < density; i++)
        {
            float mx = region.X + Hash(i * 13 + 1) * region.Width;
            float my = region.Y + Hash(i * 17 + 3) * region.Height;
            float r = 1.5f + Hash(i * 19) * 3f;
            Raylib.DrawCircleV(new Vector2(mx, my), r, WithAlpha(p.Moss, 0.15f + Hash(i * 23) * 0.2f));
            if (Hash(i * 29) > 0.6f)
                Raylib.DrawCircleV(new Vector2(mx + 2f, my - 1f), r * 0.6f, WithAlpha(p.Lichen, 0.12f));
        }
    }

    static void DrawMenuCastleTower(Vector2 origin, float width, float height, MenuCastlePalette p,
        float time, bool left, float phase)
    {
        // Slight batter - walls widen toward the base like real keep towers
        var batter = new Rectangle(origin.X - 4f, origin.Y + height * 0.72f, width + 8f, height * 0.28f);
        Raylib.DrawRectangleRounded(batter, 0.04f, 6, WithAlpha(p.StoneDark, 0.92f));
        Raylib.DrawLineEx(new Vector2(batter.X + 2f, batter.Y), new Vector2(batter.X + 2f, batter.Y + batter.Height), 1.5f, WithAlpha(p.StoneHi, 0.12f));

        var body = new Rectangle(origin.X, origin.Y, width, height);
        Raylib.DrawRectangleRounded(body, 0.06f, 10, p.StoneMid);
        Raylib.DrawRectangleRoundedLines(body, 0.06f, 10, WithAlpha(p.StoneLight, 0.75f));
        float shadeX = left ? body.X + body.Width - 10f : body.X;
        Raylib.DrawRectangleGradientH((int)shadeX, (int)body.Y, 10, (int)body.Height,
            WithAlpha(ForestShadow, left ? 0.28f : 0f), WithAlpha(ForestShadow, left ? 0f : 0.28f));
        Raylib.DrawLineEx(new Vector2(left ? body.X + 2f : body.X + body.Width - 2f, body.Y + 6f),
            new Vector2(left ? body.X + 2f : body.X + body.Width - 2f, body.Y + body.Height - 6f), 1.5f,
            WithAlpha(p.MoonGlow, 0.08f + MathF.Sin(time * 0.9f + phase) * 0.03f));
        DrawMenuMasonryUltra(body, p, 12, 6);
        DrawQuoinStripes(body, p.StoneLight, p.StoneHi);
        for (int band = 1; band < 5; band++)
        {
            float by = origin.Y + height * (band * 0.18f);
            Raylib.DrawLine((int)origin.X, (int)by, (int)(origin.X + width), (int)by, WithAlpha(p.StoneLight, 0.18f));
        }
        float roofBase = origin.Y - 8f;
        DrawTowerRoof(origin.X + width / 2f, roofBase - 38f, roofBase, width * 0.52f, p.StoneDark, p.StoneLight, p.StoneHi);
        DrawMenuCastleRoofTileDetail(origin.X + width / 2f, roofBase - 38f, roofBase, width * 0.52f, p, phase);
        var battleRect = new Rectangle(origin.X - 4f, origin.Y - 30f, width + 8f, 30f);
        DrawBattlements(battleRect, p.StoneDark, p.StoneLight, p.StoneHi, 1f, time, !left);
        DrawMenuTowerMachicolations(battleRect, p);
        DrawMenuTowerCorbelTable(origin.X + 6f, origin.Y - 32f, width - 12f, p);
        DrawMenuCastleBrokenMerlons(battleRect, p, (int)(phase * 100f) + (left ? 11 : 23));
        DrawMenuTowerBellOpening(new Vector2(origin.X + width / 2f, origin.Y - 18f), 18f, 22f, p, time);
        float slitX = left ? origin.X + width * 0.72f : origin.X + width * 0.16f;
        DrawMenuArrowSlit(new Vector2(slitX, origin.Y + height * 0.30f), 9f, 32f, p);
        DrawMenuArrowSlit(new Vector2(slitX, origin.Y + height * 0.46f), 9f, 32f, p);

        float winX = origin.X + width * 0.5f - 11f;
        var window = new Rectangle(winX, origin.Y + height * 0.64f, 22f, 30f);
        DrawWindowInteriorGlow(window, time, phase);
        Raylib.DrawRectangleRounded(window, 0.2f, 4, WithAlpha(p.StoneDeep, 0.95f));
        Raylib.DrawRectangleRoundedLines(window, 0.2f, 4, WithAlpha(p.StoneHi, 0.5f));
        var winInner = new Rectangle(window.X + 3f, window.Y + 4f, window.Width - 6f, window.Height - 8f);
        DrawMenuCastleStainedGlass(winInner, time, phase);
        Raylib.DrawLine((int)(window.X + window.Width / 2f), (int)window.Y, (int)(window.X + window.Width / 2f), (int)(window.Y + window.Height), WithAlpha(p.StoneDeep, 0.6f));
        Raylib.DrawLine((int)window.X, (int)(window.Y + window.Height * 0.55f), (int)(window.X + window.Width), (int)(window.Y + window.Height * 0.55f), WithAlpha(p.StoneDeep, 0.6f));
        Vector2 gargoylePos = new Vector2(left ? origin.X + width + 4f : origin.X - 4f, origin.Y + height * 0.22f);
        DrawMenuCastleSculptedGargoyle(gargoylePos, !left, p, time);
        DrawMenuTowerIronRing(new Vector2(origin.X + width * 0.5f, origin.Y + height * 0.08f), 5f, p);
        DrawMenuTowerDrainSpout(new Vector2(left ? origin.X + width - 10f : origin.X + 2f, origin.Y + height * 0.78f), left, p);
        DrawMenuTowerJointMoss(body, p, 18);
        DrawMenuTowerMoonRimLight(body, !left, p);
        DrawMenuCastleTowerServiceDetails(body, p, time, left, phase);
        Vector2 torchMount = new Vector2(left ? origin.X + width - 22f : origin.X + 22f, origin.Y + height * 0.14f);
        DrawCastleTorch(torchMount, time, phase, 1f, left ? TorchMountKind.WallLeft : TorchMountKind.WallRight);
        Vector2 wallN = new Vector2(left ? 1f : -1f, 0.15f);
        DrawWallTorchWash(torchMount, wallN, time, phase, 1f);
    }

    static void DrawMenuCastleRoofTileDetail(float cx, float apexY, float baseY, float halfW, MenuCastlePalette p, float phase)
    {
        int rows = 9;
        for (int row = 0; row < rows; row++)
        {
            float t0 = row / (float)rows;
            float t1 = (row + 1f) / rows;
            float y = apexY + (baseY - apexY) * t1;
            float rowHalfW = halfW * t1;
            float nextHalfW = halfW * t0;
            int tiles = Math.Max(3, (int)(rowHalfW / 7f));
            for (int tile = 0; tile < tiles; tile++)
            {
                float u = (tile + 0.5f) / tiles;
                float x = cx - rowHalfW + u * rowHalfW * 2f;
                float w = Math.Max(4f, rowHalfW * 2f / tiles - 1f);
                float h = (baseY - apexY) / rows + 1f;
                float n = Hash((int)(phase * 97f) + row * 41 + tile * 13);
                Color slate = LerpColor(p.StoneDeep, p.StoneDark, 0.35f + n * 0.32f);
                Raylib.DrawRectangleRounded(new Rectangle(x - w / 2f, y - h, w, h), 0.16f, 2, WithAlpha(slate, 0.52f));
                Raylib.DrawLineEx(new Vector2(x - w / 2f, y - h), new Vector2(x + w / 2f, y - h + 0.5f),
                    0.65f, WithAlpha(p.MoonGlow, 0.06f + n * 0.04f));
                if (row > 2 && tile % 4 == 0)
                {
                    float crackLen = 3f + n * 4f;
                    Raylib.DrawLineEx(new Vector2(x, y - h * 0.72f), new Vector2(x + (n - 0.5f) * 4f, y - h * 0.72f + crackLen),
                        0.5f, WithAlpha(ForestShadow, 0.22f));
                }
            }

            Raylib.DrawLineEx(new Vector2(cx - rowHalfW, y), new Vector2(cx + rowHalfW, y), 0.7f, WithAlpha(p.StoneHi, 0.08f));
            if (row > 0)
            {
                Raylib.DrawLineEx(new Vector2(cx - nextHalfW, y - 2f), new Vector2(cx + nextHalfW, y - 2f),
                    0.55f, WithAlpha(ForestShadow, 0.22f));
            }
        }

        Raylib.DrawLineEx(new Vector2(cx, apexY + 4f), new Vector2(cx, baseY - 2f), 1.4f, WithAlpha(p.StoneHi, 0.12f));
        Raylib.DrawCircleV(new Vector2(cx, apexY + 2f), 2.4f, WithAlpha(p.StoneHi, 0.32f));
    }

    static void DrawMenuCastleBrokenMerlons(Rectangle battleRect, MenuCastlePalette p, int seed)
    {
        int merlons = Math.Max(5, (int)(battleRect.Width / 22f));
        for (int i = 0; i < merlons; i++)
        {
            if (Hash(seed + i * 17) < 0.72f) continue;
            float x = battleRect.X + i * (battleRect.Width / merlons) + 4f;
            float y = battleRect.Y + 3f;
            float chip = 3f + Hash(seed + i * 23) * 6f;
            Raylib.DrawTriangle(new Vector2(x, y), new Vector2(x + chip, y), new Vector2(x + chip * 0.35f, y + chip),
                WithAlpha(ForestShadow, 0.38f));
            Raylib.DrawLineEx(new Vector2(x + 1f, y + chip * 0.4f), new Vector2(x + chip, y + 1f),
                0.7f, WithAlpha(p.StoneHi, 0.10f));
        }
    }

    static void DrawMenuCastleTowerServiceDetails(Rectangle body, MenuCastlePalette p, float time, bool left, float phase)
    {
        float sideX = left ? body.X + body.Width - 11f : body.X + 7f;
        float ladderY = body.Y + body.Height * 0.38f;
        float ladderH = body.Height * 0.34f;
        Color wood = new Color(48, 38, 30, 255);
        Raylib.DrawLineEx(new Vector2(sideX, ladderY), new Vector2(sideX, ladderY + ladderH), 1.4f, WithAlpha(wood, 0.45f));
        Raylib.DrawLineEx(new Vector2(sideX + 7f, ladderY + 3f), new Vector2(sideX + 7f, ladderY + ladderH + 3f), 1.4f, WithAlpha(wood, 0.45f));
        for (int rung = 0; rung < 7; rung++)
        {
            float y = ladderY + rung * (ladderH / 6f);
            Raylib.DrawLineEx(new Vector2(sideX - 1f, y), new Vector2(sideX + 8f, y + 1f), 1f, WithAlpha(wood, 0.5f));
        }

        Vector2 hoistBase = new Vector2(left ? body.X + body.Width - 18f : body.X + 18f, body.Y + body.Height * 0.18f);
        float dir = left ? 1f : -1f;
        Raylib.DrawLineEx(hoistBase, hoistBase + new Vector2(dir * 24f, -8f), 2f, WithAlpha(wood, 0.54f));
        Raylib.DrawLineEx(hoistBase + new Vector2(dir * 24f, -8f), hoistBase + new Vector2(dir * 24f, 22f), 0.9f, WithAlpha(p.Iron, 0.48f));
        float bucketY = hoistBase.Y + 14f + MathF.Sin(time * 0.7f + phase) * 2f;
        var bucket = new Rectangle(hoistBase.X + dir * 20f - 4f, bucketY, 8f, 7f);
        Raylib.DrawRectangleRounded(bucket, 0.18f, 2, WithAlpha(new Color(44, 34, 28, 255), 0.62f));
        Raylib.DrawLine((int)bucket.X, (int)bucket.Y, (int)(bucket.X + bucket.Width), (int)bucket.Y, WithAlpha(p.Iron, 0.25f));

        for (int nest = 0; nest < 5; nest++)
        {
            float nx = body.X + body.Width * (0.2f + nest * 0.15f);
            float ny = body.Y + body.Height * (0.78f + Hash(nest * 7 + (int)phase) * 0.12f);
            Raylib.DrawCircleV(new Vector2(nx, ny), 1f + Hash(nest * 11), WithAlpha(p.Lichen, 0.12f));
        }
    }

    static void DrawMenuArrowSlit(Vector2 pos, float w, float h, MenuCastlePalette p)
    {
        var outer = new Rectangle(pos.X, pos.Y, w, h);
        Raylib.DrawRectangleRounded(outer, 0.15f, 4, WithAlpha(p.StoneDeep, 0.9f));
        Raylib.DrawRectangleRoundedLines(outer, 0.15f, 4, WithAlpha(p.StoneLight, 0.45f));
        var inner = new Rectangle(pos.X + 2f, pos.Y + 4f, w - 4f, h - 8f);
        Raylib.DrawRectangle((int)inner.X, (int)inner.Y, (int)inner.Width, (int)inner.Height, WithAlpha(ForestShadow, 0.95f));
        var cross = new Rectangle(pos.X - 1f, pos.Y + h * 0.42f, w + 2f, h * 0.18f);
        Raylib.DrawRectangleRounded(cross, 0.2f, 3, WithAlpha(p.StoneDeep, 0.95f));
        Raylib.DrawLine((int)pos.X, (int)(pos.Y + 2f), (int)(pos.X + w), (int)(pos.Y + 2f), WithAlpha(p.StoneHi, 0.12f));
    }

}
