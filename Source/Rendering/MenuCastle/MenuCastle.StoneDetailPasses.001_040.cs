partial class Program
{
    // -------------------------------------------------------------------------
    // Supplemental procedural stone detail
    // -------------------------------------------------------------------------

    static void DrawMenuStoneDetailPass1(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 13;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 10) * region.Width;
            float py = region.Y + Hash(i * 16) * region.Height;
            float size = 1f + Hash(i * 15) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 24) * 0.15f));
            if (Hash(i * 30) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 32) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass2(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 13) * region.Width;
            float py = region.Y + Hash(i * 21) * region.Height;
            float size = 1f + Hash(i * 17) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 25) * 0.15f));
            if (Hash(i * 31) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 33) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass3(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 16) * region.Width;
            float py = region.Y + Hash(i * 26) * region.Height;
            float size = 1f + Hash(i * 19) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 26) * 0.15f));
            if (Hash(i * 32) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 34) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass4(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 16;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 19) * region.Width;
            float py = region.Y + Hash(i * 31) * region.Height;
            float size = 1f + Hash(i * 21) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 27) * 0.15f));
            if (Hash(i * 33) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 35) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass5(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 17;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 22) * region.Width;
            float py = region.Y + Hash(i * 36) * region.Height;
            float size = 1f + Hash(i * 23) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 28) * 0.15f));
            if (Hash(i * 34) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 36) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass6(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 25) * region.Width;
            float py = region.Y + Hash(i * 41) * region.Height;
            float size = 1f + Hash(i * 25) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 29) * 0.15f));
            if (Hash(i * 35) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 37) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass7(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 19;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 28) * region.Width;
            float py = region.Y + Hash(i * 46) * region.Height;
            float size = 1f + Hash(i * 27) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 30) * 0.15f));
            if (Hash(i * 36) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 38) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass8(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 31) * region.Width;
            float py = region.Y + Hash(i * 51) * region.Height;
            float size = 1f + Hash(i * 29) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 31) * 0.15f));
            if (Hash(i * 37) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 39) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass9(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 13;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 34) * region.Width;
            float py = region.Y + Hash(i * 56) * region.Height;
            float size = 1f + Hash(i * 31) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 32) * 0.15f));
            if (Hash(i * 38) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 40) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass10(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 37) * region.Width;
            float py = region.Y + Hash(i * 61) * region.Height;
            float size = 1f + Hash(i * 33) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 33) * 0.15f));
            if (Hash(i * 39) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 41) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass11(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 40) * region.Width;
            float py = region.Y + Hash(i * 66) * region.Height;
            float size = 1f + Hash(i * 35) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 34) * 0.15f));
            if (Hash(i * 40) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 42) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass12(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 16;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 43) * region.Width;
            float py = region.Y + Hash(i * 71) * region.Height;
            float size = 1f + Hash(i * 37) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 35) * 0.15f));
            if (Hash(i * 41) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 43) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass13(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 17;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 46) * region.Width;
            float py = region.Y + Hash(i * 76) * region.Height;
            float size = 1f + Hash(i * 39) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 36) * 0.15f));
            if (Hash(i * 42) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 44) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass14(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 49) * region.Width;
            float py = region.Y + Hash(i * 81) * region.Height;
            float size = 1f + Hash(i * 41) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 37) * 0.15f));
            if (Hash(i * 43) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 45) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass15(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 19;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 52) * region.Width;
            float py = region.Y + Hash(i * 86) * region.Height;
            float size = 1f + Hash(i * 43) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 38) * 0.15f));
            if (Hash(i * 44) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 46) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass16(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 55) * region.Width;
            float py = region.Y + Hash(i * 91) * region.Height;
            float size = 1f + Hash(i * 45) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 39) * 0.15f));
            if (Hash(i * 45) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 47) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass17(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 13;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 58) * region.Width;
            float py = region.Y + Hash(i * 96) * region.Height;
            float size = 1f + Hash(i * 47) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 40) * 0.15f));
            if (Hash(i * 46) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 48) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass18(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 61) * region.Width;
            float py = region.Y + Hash(i * 101) * region.Height;
            float size = 1f + Hash(i * 49) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 41) * 0.15f));
            if (Hash(i * 47) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 49) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass19(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 64) * region.Width;
            float py = region.Y + Hash(i * 106) * region.Height;
            float size = 1f + Hash(i * 51) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 42) * 0.15f));
            if (Hash(i * 48) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 50) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass20(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 16;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 67) * region.Width;
            float py = region.Y + Hash(i * 111) * region.Height;
            float size = 1f + Hash(i * 53) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 43) * 0.15f));
            if (Hash(i * 49) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 51) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass21(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 17;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 70) * region.Width;
            float py = region.Y + Hash(i * 116) * region.Height;
            float size = 1f + Hash(i * 55) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 44) * 0.15f));
            if (Hash(i * 50) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 52) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass22(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 73) * region.Width;
            float py = region.Y + Hash(i * 121) * region.Height;
            float size = 1f + Hash(i * 57) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 45) * 0.15f));
            if (Hash(i * 51) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 53) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass23(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 19;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 76) * region.Width;
            float py = region.Y + Hash(i * 126) * region.Height;
            float size = 1f + Hash(i * 59) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 46) * 0.15f));
            if (Hash(i * 52) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 54) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass24(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 79) * region.Width;
            float py = region.Y + Hash(i * 131) * region.Height;
            float size = 1f + Hash(i * 61) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 47) * 0.15f));
            if (Hash(i * 53) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 55) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass25(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 13;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 82) * region.Width;
            float py = region.Y + Hash(i * 136) * region.Height;
            float size = 1f + Hash(i * 63) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 48) * 0.15f));
            if (Hash(i * 54) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 56) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass26(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 85) * region.Width;
            float py = region.Y + Hash(i * 141) * region.Height;
            float size = 1f + Hash(i * 65) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 49) * 0.15f));
            if (Hash(i * 55) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 57) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass27(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 88) * region.Width;
            float py = region.Y + Hash(i * 146) * region.Height;
            float size = 1f + Hash(i * 67) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 50) * 0.15f));
            if (Hash(i * 56) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 58) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass28(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 16;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 91) * region.Width;
            float py = region.Y + Hash(i * 151) * region.Height;
            float size = 1f + Hash(i * 69) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 51) * 0.15f));
            if (Hash(i * 57) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 59) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass29(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 17;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 94) * region.Width;
            float py = region.Y + Hash(i * 156) * region.Height;
            float size = 1f + Hash(i * 71) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 52) * 0.15f));
            if (Hash(i * 58) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 60) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass30(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 97) * region.Width;
            float py = region.Y + Hash(i * 161) * region.Height;
            float size = 1f + Hash(i * 73) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 53) * 0.15f));
            if (Hash(i * 59) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 61) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass31(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 19;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 100) * region.Width;
            float py = region.Y + Hash(i * 166) * region.Height;
            float size = 1f + Hash(i * 75) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 54) * 0.15f));
            if (Hash(i * 60) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 62) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass32(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 103) * region.Width;
            float py = region.Y + Hash(i * 171) * region.Height;
            float size = 1f + Hash(i * 77) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 55) * 0.15f));
            if (Hash(i * 61) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 63) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass33(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 13;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 106) * region.Width;
            float py = region.Y + Hash(i * 176) * region.Height;
            float size = 1f + Hash(i * 79) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 56) * 0.15f));
            if (Hash(i * 62) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 64) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass34(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 109) * region.Width;
            float py = region.Y + Hash(i * 181) * region.Height;
            float size = 1f + Hash(i * 81) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 57) * 0.15f));
            if (Hash(i * 63) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 65) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass35(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 112) * region.Width;
            float py = region.Y + Hash(i * 186) * region.Height;
            float size = 1f + Hash(i * 83) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 58) * 0.15f));
            if (Hash(i * 64) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 66) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass36(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 16;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 115) * region.Width;
            float py = region.Y + Hash(i * 191) * region.Height;
            float size = 1f + Hash(i * 85) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 59) * 0.15f));
            if (Hash(i * 65) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 67) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass37(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 17;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 118) * region.Width;
            float py = region.Y + Hash(i * 196) * region.Height;
            float size = 1f + Hash(i * 87) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 60) * 0.15f));
            if (Hash(i * 66) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 68) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass38(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 121) * region.Width;
            float py = region.Y + Hash(i * 201) * region.Height;
            float size = 1f + Hash(i * 89) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 61) * 0.15f));
            if (Hash(i * 67) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 69) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass39(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 19;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 124) * region.Width;
            float py = region.Y + Hash(i * 206) * region.Height;
            float size = 1f + Hash(i * 91) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 62) * 0.15f));
            if (Hash(i * 68) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 70) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }

    static void DrawMenuStoneDetailPass40(Rectangle region, MenuCastlePalette palette, float time)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float px = region.X + Hash(i * 127) * region.Width;
            float py = region.Y + Hash(i * 211) * region.Height;
            float size = 1f + Hash(i * 93) * 3f;
            Color c = LerpColor(palette.StoneMid, palette.StoneLight, Hash(i * 19));
            Raylib.DrawCircleV(new Vector2(px, py), size, WithAlpha(c, 0.1f + Hash(i * 63) * 0.15f));
            if (Hash(i * 69) > 0.7f)
            {
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(px + Hash(i * 71) * 6f, py + 4f), 0.8f, WithAlpha(palette.Mortar, 0.2f));
            }
        }
    }


    static Color stoneDeep() => new Color(18, 16, 16, 255);

    static void DrawStoneMasonry(Rectangle region, Color block, Color mortar, int rows, int cols, float stagger)
    {
        float rowH = region.Height / rows;
        float colW = region.Width / cols;
        for (int row = 0; row < rows; row++)
        {
            float y = region.Y + row * rowH;
            float offset = (row % 2 == 0 ? 0f : stagger) * colW;
            for (int col = -1; col < cols + 1; col++)
            {
                float x = region.X + col * colW + offset;
                var brick = new Rectangle(x + 1.5f, y + 1.5f, colW - 3f, rowH - 3f);
                if (brick.X + brick.Width < region.X || brick.X > region.X + region.Width) continue;

                float n = Hash((int)(x * 3f) ^ (int)(y * 7f));
                Color b = LerpColor(block, Lighten(block, 0.12f), n * 0.5f + 0.25f);
                Raylib.DrawRectangleRounded(brick, 0.08f, 4, WithAlpha(b, 0.22f));
            }
        }

        for (int row = 0; row <= rows; row++)
        {
            float y = region.Y + row * rowH;
            Raylib.DrawLine((int)region.X, (int)y, (int)(region.X + region.Width), (int)y, WithAlpha(mortar, 0.42f));
        }
        for (int col = 0; col <= cols; col++)
        {
            float x = region.X + col * colW;
            Raylib.DrawLine((int)x, (int)region.Y, (int)x, (int)(region.Y + region.Height), WithAlpha(mortar, 0.28f));
        }
    }

    static void DrawQuoinStripes(Rectangle region, Color edge, Color highlight)
    {
        float stripeW = 14f;
        int quoinRows = Math.Max(5, (int)(region.Height / 26f));
        float rowStep = region.Height / quoinRows;

        for (int side = 0; side < 2; side++)
        {
            float baseX = side == 0 ? region.X : region.X + region.Width - stripeW;
            for (int q = 0; q < quoinRows; q++)
            {
                float qy = region.Y + q * rowStep + 1f;
                float qh = rowStep - 2f;
                bool proud = (q + side) % 2 == 0;
                float protrude = proud ? 3.5f : 0f;
                float qx = side == 0 ? baseX - protrude : baseX;
                var quoin = new Rectangle(qx, qy, stripeW + protrude, qh);
                Color face = proud ? LerpColor(edge, highlight, 0.28f) : edge;
                Raylib.DrawRectangleRounded(quoin, 0.1f, 3, WithAlpha(face, proud ? 0.72f : 0.48f));
                Raylib.DrawLineEx(new Vector2(quoin.X + (side == 0 ? 2f : quoin.Width - 2f), quoin.Y + 1f),
                    new Vector2(quoin.X + (side == 0 ? 2f : quoin.Width - 2f), quoin.Y + qh - 1f),
                    1.2f, WithAlpha(highlight, proud ? 0.42f : 0.22f));
                if (proud)
                {
                    float shadowX = side == 0 ? quoin.X + quoin.Width - 1f : quoin.X + 1f;
                    Raylib.DrawLineEx(new Vector2(shadowX, quoin.Y + 1f), new Vector2(shadowX, quoin.Y + qh - 1f),
                        1f, WithAlpha(ForestShadow, 0.24f));
                }
            }
        }
    }

    static void DrawButtress(Vector2 origin, float width, float height, Color stone, Color dark, Color light, bool left)
    {
        var body = new Rectangle(origin.X, origin.Y, width, height);
        Raylib.DrawRectangleRounded(body, 0.06f, 6, WithAlpha(stone, 0.82f));
        DrawMenuMasonryUltra(body, MenuPalette, 8, 3);

        float[] setoffs = { 0.18f, 0.42f, 0.68f };
        foreach (float setoff in setoffs)
        {
            float sy = origin.Y + height * setoff;
            float inset = 4f + setoff * 6f;
            Raylib.DrawLineEx(new Vector2(left ? origin.X + inset : origin.X + width - inset, sy),
                new Vector2(left ? origin.X + width - 4f : origin.X + 4f, sy),
                1.5f, WithAlpha(light, 0.22f));
            Raylib.DrawLineEx(new Vector2(left ? origin.X + inset : origin.X + width - inset, sy + 1f),
                new Vector2(left ? origin.X + width - 4f : origin.X + 4f, sy + 1f),
                1f, WithAlpha(ForestShadow, 0.25f));
        }

        float taperX = left ? origin.X + width * 0.55f : origin.X + width * 0.45f;
        Raylib.DrawTriangle(
            new Vector2(taperX, origin.Y - 22f),
            new Vector2(origin.X, origin.Y + height * 0.12f),
            new Vector2(origin.X + width, origin.Y + height * 0.12f),
            WithAlpha(dark, 0.88f));
        Raylib.DrawLineEx(new Vector2(taperX, origin.Y - 22f), new Vector2(taperX, origin.Y - 8f), 2f, WithAlpha(light, 0.3f));
        Raylib.DrawLineEx(new Vector2(origin.X + width * 0.5f, origin.Y), new Vector2(origin.X + width * 0.5f, origin.Y + height), 2f, WithAlpha(light, 0.32f));
    }

    static void DrawTowerRoof(float cx, float apexY, float baseY, float halfW, Color dark, Color light, Color hi)
    {
        Color slateDark = new Color(34, 32, 30, 255);
        Color slateMid = new Color(48, 46, 42, 255);
        Color slateHi = new Color(68, 66, 60, 255);
        var apex = new Vector2(cx, apexY);
        var left = new Vector2(cx - halfW, baseY);
        var right = new Vector2(cx + halfW, baseY);

        Raylib.DrawTriangle(apex, right, new Vector2(cx, baseY), Darken(slateDark, 0.18f));
        Raylib.DrawTriangle(apex, left, new Vector2(cx, baseY), slateMid);

        const int courses = 9;
        for (int c = 0; c < courses; c++)
        {
            float t = c / (float)courses;
            float y = baseY - (baseY - apexY) * t;
            float w = halfW * (1f - t * 0.94f);
            Color courseCol = c % 2 == 0 ? slateDark : LerpColor(slateDark, slateMid, 0.35f);
            Raylib.DrawLineEx(new Vector2(cx - w, y), new Vector2(cx + w, y), 1.4f, WithAlpha(courseCol, 0.62f));
            if (c % 3 == 1)
            {
                Raylib.DrawLineEx(new Vector2(cx - w * 0.85f, y - 2f), new Vector2(cx + w * 0.85f, y - 2f), 0.7f, WithAlpha(slateHi, 0.22f));
            }
        }

        Raylib.DrawLineEx(left, apex, 2.2f, WithAlpha(slateHi, 0.5f));
        Raylib.DrawLineEx(apex, right, 1.4f, WithAlpha(Darken(slateDark, 0.2f), 0.75f));
        Raylib.DrawLineEx(new Vector2(cx - halfW, baseY), new Vector2(cx + halfW, baseY), 2.8f, WithAlpha(ForestShadow, 0.42f));

        Raylib.DrawLineEx(apex, new Vector2(apex.X, apexY - 10f), 1.6f, WithAlpha(hi, 0.75f));
        Raylib.DrawCircleV(new Vector2(apex.X, apexY - 11f), 2.8f, WithAlpha(hi, 0.6f));
        Raylib.DrawLineEx(new Vector2(apex.X, apexY - 11f), new Vector2(apex.X + 7f, apexY - 15f), 1.2f, WithAlpha(light, 0.45f));
        Raylib.DrawLineEx(new Vector2(apex.X + 7f, apexY - 15f), new Vector2(apex.X + 7f, apexY - 9f), 1f, WithAlpha(light, 0.35f));
    }

    enum TorchMountKind
    {
        WallLeft,
        WallRight,
        WallFace,
        GroundPost,
        Rampart,
    }

    static void DrawTorchMountStand(Vector2 mount, float scale, TorchMountKind kind, float groundY = 0f)
    {
        Color iron = new Color(54, 52, 50, 255);
        Color ironHi = WithAlpha(MossLight, 0.35f);
        Color stoneBracket = new Color(68, 66, 62, 255);

        switch (kind)
        {
            case TorchMountKind.WallLeft:
            {
                var plate = new Rectangle(mount.X - 18f * scale, mount.Y + 4f * scale, 10f * scale, 14f * scale);
                Raylib.DrawRectangleRounded(plate, 0.15f, 2, WithAlpha(iron, 0.9f));
                Raylib.DrawLineEx(
                    new Vector2(plate.X + plate.Width, plate.Y + plate.Height * 0.5f),
                    new Vector2(mount.X - 5f * scale, mount.Y + 8f * scale),
                    2.5f * scale, iron);
                Raylib.DrawCircleV(new Vector2(plate.X + 5f * scale, plate.Y + 3f * scale), 1.5f * scale, ironHi);
                break;
            }
            case TorchMountKind.WallRight:
            {
                var plate = new Rectangle(mount.X + 8f * scale, mount.Y + 4f * scale, 10f * scale, 14f * scale);
                Raylib.DrawRectangleRounded(plate, 0.15f, 2, WithAlpha(iron, 0.9f));
                Raylib.DrawLineEx(
                    new Vector2(plate.X, plate.Y + plate.Height * 0.5f),
                    new Vector2(mount.X + 5f * scale, mount.Y + 8f * scale),
                    2.5f * scale, iron);
                Raylib.DrawCircleV(new Vector2(plate.X + 5f * scale, plate.Y + 3f * scale), 1.5f * scale, ironHi);
                break;
            }
            case TorchMountKind.WallFace:
            {
                var shelf = new Rectangle(mount.X - 11f * scale, mount.Y + 14f * scale, 22f * scale, 5f * scale);
                Raylib.DrawRectangleRounded(shelf, 0.2f, 2, stoneBracket);
                Raylib.DrawLineEx(
                    new Vector2(mount.X - 8f * scale, mount.Y + 14f * scale),
                    new Vector2(mount.X - 6f * scale, mount.Y + 24f * scale),
                    2f * scale, stoneBracket);
                Raylib.DrawLineEx(
                    new Vector2(mount.X + 8f * scale, mount.Y + 14f * scale),
                    new Vector2(mount.X + 6f * scale, mount.Y + 24f * scale),
                    2f * scale, stoneBracket);
                Raylib.DrawRectangleRounded(
                    new Rectangle(mount.X - 6f * scale, mount.Y + 1f * scale, 12f * scale, 3f * scale),
                    0.2f, 2, WithAlpha(iron, 0.75f));
                break;
            }
            case TorchMountKind.GroundPost:
            {
                float footY = groundY > 0f ? groundY : mount.Y + 34f * scale;
                float postTop = mount.Y + 14f * scale;
                float postH = MathF.Max(8f * scale, footY - postTop);
                Raylib.DrawRectangleRounded(
                    new Rectangle(mount.X - 3f * scale, postTop, 6f * scale, postH),
                    0.1f, 2, iron);
                Raylib.DrawRectangleRounded(
                    new Rectangle(mount.X - 7f * scale, footY - 4f * scale, 14f * scale, 5f * scale),
                    0.2f, 2, stoneBracket);
                Raylib.DrawLineEx(
                    new Vector2(mount.X - 5f * scale, footY - 1f * scale),
                    new Vector2(mount.X + 5f * scale, footY - 1f * scale),
                    1.5f * scale, WithAlpha(ironHi, 0.5f));
                break;
            }
            case TorchMountKind.Rampart:
            {
                var block = new Rectangle(mount.X - 9f * scale, mount.Y + 12f * scale, 18f * scale, 14f * scale);
                Raylib.DrawRectangleRounded(block, 0.1f, 3, stoneBracket);
                Raylib.DrawRectangleRoundedLines(block, 0.1f, 3, WithAlpha(ironHi, 0.35f));
                Raylib.DrawLine(
                    (int)(mount.X - 6f * scale), (int)(mount.Y + 26f * scale),
                    (int)(mount.X + 6f * scale), (int)(mount.Y + 26f * scale),
                    WithAlpha(Darken(stoneBracket, 0.2f), 0.6f));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unexpected torch mount kind.");
        }
    }

    static void DrawCastleTorch(Vector2 mount, float time, float phase, float scale,
        TorchMountKind mountKind = TorchMountKind.WallFace, float mountGroundY = 0f)
    {
        DrawTorchMountStand(mount, scale, mountKind, mountGroundY);

        float flicker = MathF.Sin(time * 5.8f + phase) * 0.5f + 0.5f;
        Color bracket = new Color(54, 52, 50, 255);
        Color flameCore = LerpColor(new Color(196, 192, 180, 255), MossLight, flicker * 0.6f);
        Color flameOuter = new Color(148, 144, 132, 255);

        var holder = new Rectangle(mount.X - 5f * scale, mount.Y, 10f * scale, 16f * scale);
        Raylib.DrawRectangleRounded(holder, 0.2f, 4, bracket);
        Raylib.DrawLineEx(new Vector2(mount.X - 8f * scale, mount.Y + 4f * scale), new Vector2(mount.X + 8f * scale, mount.Y + 4f * scale), 2f * scale, WithAlpha(MossLight, 0.4f));

        Vector2 flame = new Vector2(mount.X, mount.Y - 6f * scale);
        Raylib.DrawCircleV(flame, 5.5f * scale, WithAlpha(flameOuter, 0.55f + flicker * 0.15f));
        Raylib.DrawCircleV(flame, 4.5f * scale, WithAlpha(flameCore, 0.85f));
        Raylib.DrawCircleV(new Vector2(flame.X, flame.Y - 3f * scale), 2.5f * scale, WithAlpha(Color.White, 0.35f + flicker * 0.25f));
    }

    static void DrawCobbleForeground(Rectangle region, Color dark, Color seam)
    {
        Raylib.DrawRectangle((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height, dark);
        int cols = 24;
        float colW = region.Width / cols;
        for (int row = 0; row < 3; row++)
        {
            float y = region.Y + row * (region.Height / 3f);
            float offset = row % 2 == 0 ? 0f : colW * 0.5f;
            for (int col = -1; col < cols + 1; col++)
            {
                float x = region.X + col * colW + offset;
                float n = Hash(col * 17 + row * 31);
                Color c = LerpColor(dark, Lighten(dark, 0.15f), n * 0.5f + 0.2f);
                var stone = new Rectangle(x + 1f, y + 1f, colW - 2f, region.Height / 3f - 2f);
                Raylib.DrawRectangleRounded(stone, 0.15f, 3, WithAlpha(c, 0.55f));
            }
            Raylib.DrawLine((int)region.X, (int)y, (int)(region.X + region.Width), (int)y, WithAlpha(seam, 0.5f));
        }
    }

    static void DrawBattlements(Rectangle region, Color dark, Color light, Color highlight, float heightVar = 1f,
        float time = 0f, bool moonFromRight = true)
    {
        int merlons = Math.Max(8, (int)(region.Width / 30f));
        float merlonW = region.Width / merlons;
        float gapH = region.Height * 0.22f;
        float breathe = time > 0f ? 1f + MathF.Sin(time * 0.65f) * 0.012f : 1f;
        Color moonRim = MenuPalette.MoonGlow;

        Raylib.DrawRectangle((int)region.X, (int)(region.Y + gapH), (int)region.Width, (int)(region.Height - gapH), WithAlpha(dark, 0.85f));
        Raylib.DrawLine((int)region.X, (int)(region.Y + gapH), (int)(region.X + region.Width), (int)(region.Y + gapH), WithAlpha(ForestShadow, 0.32f));

        for (int i = 0; i < merlons; i++)
        {
            if (i % 2 == 1)
            {
                float gx = region.X + i * merlonW;
                var gap = new Rectangle(gx + 1f, region.Y + gapH * 0.4f, merlonW - 2f, region.Height * 0.6f);
                Raylib.DrawRectangleRounded(gap, 0.1f, 3, WithAlpha(ForestShadow, 0.72f));
                Raylib.DrawRectangle((int)gx, (int)region.Y, (int)merlonW, (int)(region.Height * 0.38f), WithAlpha(ForestShadow, 0.42f));
                continue;
            }

            float hVar = (1f + Hash(i * 13) * 0.18f * heightVar) * breathe;
            var merlon = new Rectangle(region.X + i * merlonW + 2f, region.Y, merlonW - 4f, region.Height * hVar);
            Raylib.DrawRectangleRounded(merlon, 0.1f, 4, WithAlpha(dark, 0.95f));
            Raylib.DrawRectangleRoundedLines(merlon, 0.1f, 4, WithAlpha(light, 0.6f));
            Raylib.DrawLine((int)(merlon.X + 3f), (int)merlon.Y, (int)(merlon.X + merlon.Width - 3f), (int)merlon.Y, WithAlpha(highlight, 0.35f));

            if (time > 0f)
            {
                float litX = moonFromRight ? merlon.X + merlon.Width - 2f : merlon.X + 2f;
                float shadeX = moonFromRight ? merlon.X + 2f : merlon.X + merlon.Width - 2f;
                float rimPulse = 0.12f + MathF.Sin(time * 1.05f + i * 0.55f) * 0.035f;
                Raylib.DrawLineEx(new Vector2(litX, merlon.Y + 2f), new Vector2(litX, merlon.Y + merlon.Height - 2f), 1.5f, WithAlpha(moonRim, rimPulse));
                Raylib.DrawLineEx(new Vector2(shadeX, merlon.Y + 4f), new Vector2(shadeX, merlon.Y + merlon.Height), 1.2f, WithAlpha(ForestShadow, 0.38f));
            }

            Raylib.DrawLineEx(new Vector2(merlon.X + 2f, merlon.Y + merlon.Height - 1f),
                new Vector2(merlon.X + merlon.Width - 2f, merlon.Y + merlon.Height - 1f), 1f, WithAlpha(ForestShadow, 0.28f));
        }
    }


}
