partial class Program
{
    // -------------------------------------------------------------------------
    // Realistic moon
    // -------------------------------------------------------------------------

    static Vector2 MenuCastleMoonPosition => new Vector2(WindowWidth * 0.76f, 64f);

    static void DrawMenuCastleMoon(float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float pulse = MathF.Sin(time * 0.35f) * 0.5f + 0.5f;
        Color lit = MenuPalette.MoonGlow;
        Color darkSide = new Color(18, 18, 18, 255);
        Color maria = new Color(52, 50, 48, 255);

        DrawMoonCorona(moon, time);

        // Earthshine on dark hemisphere
        DrawEllipticalGlow(new Vector2(moon.X + 6f, moon.Y + 2f), 22f, 20f, -12f, darkSide, 0.06f + pulse * 0.02f, 3);

        // Main disc
        Raylib.DrawCircleV(moon, 22f, WithAlpha(lit, 0.22f + pulse * 0.06f));
        Raylib.DrawCircleV(moon, 18f, WithAlpha(Color.White, 0.12f + pulse * 0.05f));

        // Gibbous terminator via overlapping circles
        Vector2 shadowCenter = new Vector2(moon.X + 10f, moon.Y - 1f);
        Raylib.DrawCircleV(shadowCenter, 19f, WithAlpha(darkSide, 0.55f));
        Raylib.DrawCircleV(new Vector2(moon.X - 2f, moon.Y), 20f, WithAlpha(lit, 0.35f));

        // Maria patches
        for (int m = 0; m < 6; m++)
        {
            float mx = moon.X + (Hash(m * 13) - 0.5f) * 24f;
            float my = moon.Y + (Hash(m * 17) - 0.5f) * 20f;
            float mrx = 4f + Hash(m * 19) * 5f;
            float mry = 3f + Hash(m * 23) * 4f;
            Raylib.DrawEllipse((int)mx, (int)my, mrx, mry, WithAlpha(maria, 0.2f + Hash(m * 29) * 0.15f));
        }

        // Crater rims and shadows
        for (int cr = 0; cr < 14; cr++)
        {
            float cx = moon.X + (Hash(cr * 37) - 0.5f) * 30f;
            float cy = moon.Y + (Hash(cr * 41) - 0.5f) * 26f;
            float rad = 1.2f + Hash(cr * 43) * 2.8f;
            Raylib.DrawCircleLines((int)cx, (int)cy, rad, WithAlpha(Darken(lit, 0.35f), 0.25f));
            Raylib.DrawCircleV(new Vector2(cx + 0.6f, cy + 0.6f), rad * 0.7f, WithAlpha(darkSide, 0.18f));
        }

        // Limb highlight
        for (int h = 0; h < 8; h++)
        {
            float ang = MathF.PI * 0.15f + h * 0.08f;
            Vector2 lp = new Vector2(moon.X + MathF.Cos(ang) * 19f, moon.Y + MathF.Sin(ang) * 17f);
            Raylib.DrawCircleV(lp, 1.2f, WithAlpha(Color.White, 0.2f));
        }

        // Soft landscape earth-glow beneath moon
        DrawEllipticalGlow(new Vector2(moon.X, moon.Y + 120f), 160f, 28f, 0f, lit, 0.022f + pulse * 0.008f, 4);

        for (int ring = 0; ring < 3; ring++)
        {
            float rr = 30f + ring * 10f + pulse * 5f;
            Raylib.DrawCircleLinesV(moon, rr, WithAlpha(lit, 0.07f - ring * 0.018f));
        }

        DrawEllipticalGlow(moon, 48f + pulse * 8f, 48f + pulse * 8f, 0f, Color.White, 0.028f + pulse * 0.012f, 3);
    }


}
