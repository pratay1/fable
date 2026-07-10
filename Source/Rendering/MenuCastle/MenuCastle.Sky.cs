partial class Program
{
    // -------------------------------------------------------------------------
    // Night sky
    // -------------------------------------------------------------------------

    static void DrawMenuCastleNightSky(float time)
    {
        int skyH = (int)(WindowHeight * 0.58f);

        // Obsidian night: true black zenith, charcoal horizon - zero blue cast.
        Color zenith = new Color(0, 0, 0, 255);
        Color upperDeep = new Color(2, 2, 2, 255);
        Color upperMid = new Color(4, 3, 3, 255);
        Color midSky = new Color(6, 5, 5, 255);
        Color lowerSky = new Color(8, 7, 7, 255);
        Color horizon = new Color(10, 9, 8, 255);
        Color groundHaze = new Color(7, 6, 6, 255);

        Raylib.DrawRectangle(0, 0, WindowWidth, skyH, zenith);
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, skyH / 4, zenith, upperDeep);
        Raylib.DrawRectangleGradientV(0, skyH / 4, WindowWidth, skyH / 4, upperDeep, upperMid);
        Raylib.DrawRectangleGradientV(0, skyH / 2, WindowWidth, skyH / 4, upperMid, midSky);
        Raylib.DrawRectangleGradientV(0, skyH * 3 / 4, WindowWidth, skyH / 4, midSky, lowerSky);
        Raylib.DrawRectangleGradientV(0, skyH - 140, WindowWidth, 100, WithAlpha(lowerSky, 0f), WithAlpha(horizon, 0.92f));
        Raylib.DrawRectangleGradientV(0, skyH - 48, WindowWidth, 48, WithAlpha(horizon, 0f), WithAlpha(groundHaze, 0.88f));

        DrawMenuCastleSkyAtmosphereLayers(time, skyH);
        DrawMenuCastleSkyNebula(time, skyH);
        DrawMenuCastleMilkyWay(time);

        DrawMenuCastleStarField(time);
        DrawMenuCastleMoonSkyScatter(time);
        DrawMenuCastleShootingStar(time);

        DrawMenuCastleSkyCirrus(time, skyH);
        DrawMenuCastleDistantMountains(time);
        DrawMenuCastleCloudWisps(time);
        DrawMenuCastleNightSkyExtras(time);
        DrawMenuCastleAuroraVeil(time);
        DrawMenuCastleGodRays(time);
        DrawMenuCastleSkyHorizonGlow(time, skyH);
    }

    static void DrawMenuCastleSkyAtmosphereLayers(float time, int skyH)
    {
        // Zenith pole darkening - real night skies are darkest straight overhead.
        int zenithR = (int)(MathF.Min(WindowWidth, skyH) * 0.72f);
        Vector2 zenith = new Vector2(WindowWidth * 0.5f, skyH * 0.12f);
        for (int ring = 8; ring >= 1; ring--)
        {
            float t = ring / 8f;
            float rx = zenithR * (0.35f + t * 0.65f);
            float ry = zenithR * (0.28f + t * 0.52f);
            DrawEllipticalGlow(zenith, rx, ry, -6f, new Color(0, 0, 0, 255), 0.045f * t, 3);
        }

        // High-altitude neutral haze bands (subtle, not blue).
        for (int band = 0; band < 5; band++)
        {
            float by = skyH * (0.08f + band * 0.07f);
            float drift = MathF.Sin(time * 0.04f + band * 1.1f) * 6f;
            Color haze = new Color(12, 11, 10, 255);
            Raylib.DrawRectangleGradientV(0, (int)(by - 8f), WindowWidth, 16,
                WithAlpha(haze, 0f), WithAlpha(haze, 0.06f + band * 0.012f));
            DrawEllipticalGlow(new Vector2(WindowWidth * 0.5f + drift, by), WindowWidth * 0.55f, 10f, 0f,
                haze, 0.004f + band * 0.0015f, 2);
        }

        // Aerial perspective - stars and sky fade toward the horizon.
        Raylib.DrawRectangleGradientV(0, (int)(skyH * 0.34f), WindowWidth, (int)(skyH * 0.28f),
            WithAlpha(new Color(0, 0, 0, 255), 0f), WithAlpha(new Color(0, 0, 0, 255), 0.22f));
        Raylib.DrawRectangleGradientV(0, (int)(skyH * 0.52f), WindowWidth, (int)(skyH * 0.18f),
            WithAlpha(new Color(8, 7, 6, 255), 0f), WithAlpha(new Color(8, 7, 6, 255), 0.14f));
    }

    static void DrawMenuCastleSkyHorizonGlow(float time, int skyH)
    {
        float pulse = 0.92f + MathF.Sin(time * 0.08f) * 0.08f;
        int bandY = (int)(skyH * 0.44f);
        Color warmDust = new Color(14, 12, 10, 255);
        Color coldVoid = new Color(4, 4, 4, 255);
        Raylib.DrawRectangleGradientV(0, bandY - 24, WindowWidth, 48,
            WithAlpha(coldVoid, 0f), WithAlpha(warmDust, 0.18f * pulse));
        Raylib.DrawRectangleGradientV(0, bandY + 8, WindowWidth, 36,
            WithAlpha(warmDust, 0.12f * pulse), WithAlpha(new Color(6, 5, 5, 255), 0.28f));
        for (int glow = 0; glow < 4; glow++)
        {
            float gx = WindowWidth * (0.15f + glow * 0.22f) + MathF.Sin(time * 0.06f + glow) * 20f;
            DrawEllipticalGlow(new Vector2(gx, bandY + 10f), 120f + glow * 30f, 14f, glow * 5f,
                new Color(18, 16, 14, 255), 0.006f * pulse, 2);
        }
    }

    static void DrawMenuCastleSkyCirrus(float time, int skyH)
    {
        Color cirrus = new Color(22, 21, 20, 255);
        Color cirrusHi = new Color(32, 30, 28, 255);
        for (int cloud = 0; cloud < 7; cloud++)
        {
            float drift = time * (2.5f + cloud * 0.6f);
            float cx = (Hash(cloud * 91) * WindowWidth + drift) % (WindowWidth + 220f) - 110f;
            float cy = skyH * (0.06f + Hash(cloud * 73) * 0.22f) + MathF.Sin(time * 0.11f + cloud) * 4f;
            for (int w = 0; w < 6; w++)
            {
                float wx = cx + (w - 2.5f) * 38f;
                float wy = cy + Hash(cloud * 17 + w) * 6f - 3f;
                float rx = 52f + w * 14f + cloud * 4f;
                float ry = 5f + w * 1.2f;
                float rot = Hash(cloud + w * 3) * 10f - 5f;
                Color c = LerpColor(cirrus, cirrusHi, Hash(cloud * 29 + w) * 0.45f);
                DrawEllipticalGlow(new Vector2(wx, wy), rx, ry, rot, c, 0.007f + Hash(cloud * 31 + w) * 0.006f, 2);
            }
        }
    }

    static void DrawMenuCastleSkyNebula(float time, int skyH)
    {
        float pulse = 0.88f + MathF.Sin(time * 0.16f) * 0.12f;
        DrawEllipticalGlow(new Vector2(WindowWidth * 0.32f, skyH * 0.16f), WindowWidth * 0.34f, 72f, -18f,
            new Color(14, 12, 12, 255), 0.012f * pulse, 4);
        DrawEllipticalGlow(new Vector2(WindowWidth * 0.58f, skyH * 0.24f), WindowWidth * 0.26f, 48f, 10f,
            new Color(10, 10, 9, 255), 0.009f * pulse, 3);
        DrawEllipticalGlow(new Vector2(WindowWidth * 0.12f, skyH * 0.30f), 90f, 36f, -32f,
            new Color(8, 8, 7, 255), 0.007f * pulse, 2);
        DrawEllipticalGlow(new Vector2(WindowWidth * 0.78f, skyH * 0.11f), 110f, 28f, 14f,
            new Color(12, 11, 10, 255), 0.008f * pulse, 2);
    }

    static void DrawMenuCastleMoonSkyScatter(float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        float pulse = MathF.Sin(time * 0.35f) * 0.5f + 0.5f;
        Color silver = new Color(156, 154, 148, 255);
        Color ash = new Color(72, 70, 66, 255);
        DrawEllipticalGlow(new Vector2(moon.X, moon.Y + 36f), 130f, 82f, 0f,
            silver, 0.022f + pulse * 0.006f, 5);
        DrawEllipticalGlow(new Vector2(moon.X - 55f, moon.Y + 72f), 200f, 44f, -10f,
            ash, 0.009f, 3);
    }

    static void DrawMenuCastleStarField(float time)
    {
        float horizonFadeStart = WindowHeight * 0.34f;
        float horizonFadeEnd = WindowHeight * 0.50f;

        // Far field - tiny, dense, slow twinkle
        const int farCount = 320;
        for (int i = 0; i < farCount; i++)
        {
            float sx = Hash(i * 41 + 7) * WindowWidth;
            float sy = Hash(i * 19 + 3) * WindowHeight * 0.50f;
            float horizonMul = sy < horizonFadeStart ? 1f
                : sy > horizonFadeEnd ? 0.08f
                : 1f - (sy - horizonFadeStart) / (horizonFadeEnd - horizonFadeStart) * 0.92f;
            float twinkle = MathF.Sin(time * (0.35f + Hash(i * 5) * 0.9f) + i * 0.9f) * 0.5f + 0.5f;
            float alpha = (0.025f + twinkle * (0.05f + Hash(i * 31) * 0.08f)) * horizonMul;
            Color star = LerpColor(new Color(180, 178, 174, 255), new Color(220, 218, 212, 255), Hash(i * 67));
            Raylib.DrawCircleV(new Vector2(sx, sy), 0.4f + Hash(i * 11) * 0.5f, WithAlpha(star, alpha));
        }

        // Mid field - varied color temperature (warm/neutral, no blue bias)
        const int midCount = 96;
        for (int i = 0; i < midCount; i++)
        {
            float sx = Hash(i * 53 + 11) * WindowWidth;
            float sy = Hash(i * 29 + 5) * WindowHeight * 0.48f;
            float horizonMul = sy < horizonFadeStart ? 1f
                : sy > horizonFadeEnd ? 0.06f
                : 1f - (sy - horizonFadeStart) / (horizonFadeEnd - horizonFadeStart) * 0.94f;
            float temp = Hash(i * 37);
            Color starCol = temp < 0.12f
                ? new Color(255, 210, 180, 255)
                : temp < 0.28f
                    ? new Color(210, 208, 202, 255)
                    : temp < 0.72f
                        ? new Color(228, 226, 220, 255)
                        : new Color(255, 248, 232, 255);
            float twinkle = MathF.Sin(time * (0.85f + Hash(i * 7) * 2f) + i * 1.1f) * 0.5f + 0.5f;
            float r = 0.6f + Hash(i * 13) * 1.1f;
            float alpha = (0.07f + twinkle * (0.12f + Hash(i * 43) * 0.16f)) * horizonMul;
            Raylib.DrawCircleV(new Vector2(sx, sy), r, WithAlpha(starCol, alpha));
            if (Hash(i * 71) > 0.90f && twinkle > 0.74f && horizonMul > 0.35f)
            {
                float spark = 1.2f + Hash(i * 73) * 1.8f;
                Raylib.DrawLine((int)sx - (int)spark, (int)sy, (int)sx + (int)spark, (int)sy, WithAlpha(starCol, alpha * 0.55f));
                Raylib.DrawLine((int)sx, (int)sy - (int)spark, (int)sx, (int)sy + (int)spark, WithAlpha(starCol, alpha * 0.55f));
            }
        }

        // Bright anchors - prominent stars with soft neutral halos
        ReadOnlySpan<(float x, float y, float size, float phase)> anchors =
        [
            (0.14f, 0.08f, 1.8f, 0.0f),
            (0.31f, 0.18f, 1.5f, 1.2f),
            (0.48f, 0.06f, 2.0f, 2.4f),
            (0.67f, 0.14f, 1.6f, 0.8f),
            (0.84f, 0.22f, 1.4f, 3.1f),
            (0.22f, 0.28f, 1.3f, 1.9f),
            (0.91f, 0.09f, 1.7f, 2.7f),
            (0.55f, 0.11f, 1.2f, 4.2f),
            (0.08f, 0.19f, 1.1f, 1.4f),
        ];
        for (int a = 0; a < anchors.Length; a++)
        {
            var (nx, ny, size, phase) = anchors[a];
            Vector2 pos = new Vector2(WindowWidth * nx, WindowHeight * ny);
            float tw = MathF.Sin(time * 1.4f + phase) * 0.5f + 0.5f;
            Color col = new Color(236, 234, 228, 255);
            Raylib.DrawCircleV(pos, size, WithAlpha(col, 0.20f + tw * 0.16f));
            DrawEllipticalGlow(pos, size * 3.2f, size * 2.8f, 0f, col, 0.009f + tw * 0.006f, 2);
        }
    }

    static void DrawMenuCastleMilkyWay(float time)
    {
        float pulse = 0.90f + MathF.Sin(time * 0.12f) * 0.10f;
        const int segments = 20;
        for (int s = 0; s < segments; s++)
        {
            float t = s / (float)(segments - 1);
            float cx = WindowWidth * (0.04f + t * 0.82f);
            float cy = WindowHeight * (0.04f + t * 0.26f) + MathF.Sin(t * MathF.PI) * 18f;
            float rx = 92f + Hash(s * 53) * 52f;
            float ry = 14f + Hash(s * 59) * 10f;
            float rot = -32f + t * 12f;
            float envelope = 0.50f + MathF.Sin(t * MathF.PI) * 0.50f;
            float inten = (0.006f + Hash(s * 61) * 0.009f) * pulse * envelope;
            Color band = LerpColor(new Color(88, 86, 82, 255), new Color(132, 130, 124, 255), Hash(s * 67));
            DrawEllipticalGlow(new Vector2(cx, cy), rx, ry, rot, band, inten, 4);
        }
    }

    static void DrawMenuCastleShootingStar(float time)
    {
        // One slow cycle - a brief streak every ~14 seconds
        float cycle = (time * 0.07f) % 1f;
        if (cycle > 0.12f) return;

        float t = cycle / 0.12f;
        float startX = WindowWidth * (0.55f + Hash(3) * 0.25f);
        float startY = WindowHeight * (0.04f + Hash(7) * 0.10f);
        float len = 90f + Hash(11) * 60f;
        float ang = MathF.PI * 0.72f + Hash(13) * 0.18f;
        Vector2 head = new Vector2(startX + MathF.Cos(ang) * len * t, startY + MathF.Sin(ang) * len * t);
        Vector2 tail = new Vector2(head.X - MathF.Cos(ang) * len * 0.35f, head.Y - MathF.Sin(ang) * len * 0.35f);
        Color streak = new Color(228, 226, 220, 255);
        float fade = 1f - t;
        Raylib.DrawLineEx(tail, head, 1.2f, WithAlpha(streak, fade * 0.55f));
        Raylib.DrawCircleV(head, 1.4f, WithAlpha(streak, fade * 0.75f));
        DrawEllipticalGlow(head, 8f, 4f, ang * 180f / MathF.PI, streak, fade * 0.04f, 2);
    }

    static void DrawMenuCastleDistantMountains(float time)
    {
        Color ridge = new Color(4, 4, 4, 255);
        Color ridgeHi = new Color(8, 7, 7, 255);
        Color ridgeMoon = new Color(14, 13, 12, 255);
        int baseY = (int)(WindowHeight * 0.48f);
        float moonX = WindowWidth * 0.76f;
        for (int m = 0; m < 9; m++)
        {
            float x0 = WindowWidth * (m / 8f) - 40f;
            float x1 = WindowWidth * ((m + 1) / 8f) + 40f;
            float peakY = baseY - 30f - Hash(m * 17) * 55f - MathF.Sin(time * 0.05f + m) * 2f;
            Vector2 p0 = new Vector2(x0, baseY + 20f);
            Vector2 p1 = new Vector2((x0 + x1) * 0.5f, peakY);
            Vector2 p2 = new Vector2(x1, baseY + 16f);
            float moonFacing = Math.Clamp(1f - MathF.Abs(p1.X - moonX) / (WindowWidth * 0.45f), 0f, 1f);
            Color c = LerpColor(LerpColor(ridge, ridgeHi, Hash(m * 29) * 0.35f), ridgeMoon, moonFacing * 0.22f);
            Raylib.DrawTriangle(p0, p2, p1, WithAlpha(c, 0.75f));
            if (moonFacing > 0.35f)
                Raylib.DrawLineEx(p1, p2, 1f, WithAlpha(ridgeHi, 0.10f + moonFacing * 0.12f));
            else
                Raylib.DrawLineEx(p1, p2, 1f, WithAlpha(ridgeHi, 0.15f));
        }

        // Soft ground haze where mountains meet the sky
        Raylib.DrawRectangleGradientV(0, baseY - 8, WindowWidth, 48,
            WithAlpha(new Color(6, 5, 5, 255), 0f), WithAlpha(new Color(6, 5, 5, 255), 0.32f));
    }

    static void DrawMenuCastleCloudWisps(float time)
    {
        Vector2 moon = MenuCastleMoonPosition;
        Color silver = new Color(28, 27, 26, 255);
        Color shadow = new Color(8, 7, 7, 255);
        Color rim = new Color(42, 40, 38, 255);
        for (int c = 0; c < 10; c++)
        {
            float drift = time * (4f + c * 1.8f);
            float cx = (Hash(c * 97) * WindowWidth + drift) % (WindowWidth + 180f) - 90f;
            float cy = 32f + c * 18f + MathF.Sin(time * 0.16f + c * 1.2f) * 10f;
            float dx = cx - moon.X;
            float moonLit = Math.Clamp(1f - MathF.Abs(dx) / 240f, 0.15f, 1f);
            for (int w = 0; w < 5; w++)
            {
                float wx = cx + (w - 2f) * 24f;
                float wy = cy + Hash(c * 11 + w) * 8f - 4f;
                float rx = 34f + w * 10f + c * 3f;
                float ry = 9f + w * 2.5f;
                float rot = Hash(c + w) * 14f - 7f;
                DrawEllipticalGlow(new Vector2(wx, wy + 2f), rx * 1.05f, ry * 1.1f, rot, shadow, 0.010f * moonLit, 2);
                DrawEllipticalGlow(new Vector2(wx, wy), rx, ry, rot, silver, 0.012f * moonLit, 3);
                if (moonLit > 0.55f)
                    DrawEllipticalGlow(new Vector2(wx, wy - 1f), rx * 0.72f, ry * 0.55f, rot, rim, 0.006f * moonLit, 2);
            }
        }
    }
}
