partial class Program
{
    // -------------------------------------------------------------------------
    // Shaped glow system (non-circular)
    // -------------------------------------------------------------------------

    static void DrawEllipticalGlow(Vector2 center, float rx, float ry, float rotationDeg, Color color, float intensity, int layers)
    {
        if (intensity <= 0.0005f) return;
        layers = Math.Clamp(Math.Max(layers, 8), 8, 20);
        for (int i = layers - 1; i >= 0; i--)
        {
            float t0 = i / (float)layers;
            float t1 = (i + 1) / (float)layers;
            float shell = (GlowFalloff(t0) - GlowFalloff(t1)) * intensity * 0.12f;
            if (shell <= 0.0001f) continue;
            Raylib.DrawEllipse((int)center.X, (int)center.Y, rx * t1, ry * t1, WithAlpha(color, shell));
        }
    }

    static void DrawGradientWash(Rectangle area, Color hot, Color cold, Vector2 hotspotNorm, float power)
    {
        if (area.Width < 1f || area.Height < 1f) return;

        power = Math.Max(0.5f, power);
        float hotAlpha = hot.A / 255f * (0.35f + 0.15f / power);
        Color hotTint = WithAlpha(hot, hotAlpha);
        Color clear = WithAlpha(cold, 0f);

        bool vertical = hotspotNorm.Y <= 0.62f;
        if (vertical)
        {
            if (hotspotNorm.Y < 0.45f)
                Raylib.DrawRectangleGradientV((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, clear, hotTint);
            else
                Raylib.DrawRectangleGradientV((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, hotTint, clear);
        }
        else if (hotspotNorm.X < 0.45f)
        {
            Raylib.DrawRectangleGradientH((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, hotTint, clear);
        }
        else
        {
            Raylib.DrawRectangleGradientH((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, clear, hotTint);
        }

        Vector2 hotspot = new Vector2(area.X + hotspotNorm.X * area.Width, area.Y + hotspotNorm.Y * area.Height);
        DrawEllipticalGlow(hotspot, area.Width * 0.48f, area.Height * 0.42f, 0f, hot, hotAlpha * 0.22f, 8);
    }

    static void DrawLightCone(Vector2 origin, float directionRad, float length, float halfAngleRad, Color color, float intensity)
    {
        int layers = 10;
        for (int layer = layers; layer >= 1; layer--)
        {
            float t = layer / (float)layers;
            float len = length * (0.4f + 0.6f * t);
            float half = halfAngleRad * (0.5f + 0.5f * t);
            float alpha = intensity * GlowFalloff(t) * 0.38f;
            Vector2 tip = new Vector2(origin.X + MathF.Cos(directionRad) * len, origin.Y + MathF.Sin(directionRad) * len);
            Vector2 left = new Vector2(origin.X + MathF.Cos(directionRad - half) * len * 0.15f, origin.Y + MathF.Sin(directionRad - half) * len * 0.15f);
            Vector2 right = new Vector2(origin.X + MathF.Cos(directionRad + half) * len * 0.15f, origin.Y + MathF.Sin(directionRad + half) * len * 0.15f);
            Raylib.DrawTriangle(origin, left, tip, WithAlpha(color, alpha * 0.5f));
            Raylib.DrawTriangle(origin, tip, right, WithAlpha(color, alpha * 0.5f));
            Vector2 farL = new Vector2(origin.X + MathF.Cos(directionRad - half) * len, origin.Y + MathF.Sin(directionRad - half) * len);
            Vector2 farR = new Vector2(origin.X + MathF.Cos(directionRad + half) * len, origin.Y + MathF.Sin(directionRad + half) * len);
            Raylib.DrawTriangle(left, farL, tip, WithAlpha(color, alpha * 0.35f));
            Raylib.DrawTriangle(tip, farR, right, WithAlpha(color, alpha * 0.35f));
        }
    }

    static void DrawWallTorchWash(Vector2 torchPos, Vector2 wallNormal, float time, float phase, float scale)
    {
        float flicker = MathF.Sin(time * 5.4f + phase) * 0.5f + 0.5f;
        float flicker2 = MathF.Sin(time * 8.2f + phase * 1.3f) * 0.5f + 0.5f;
        Color warm = MenuPalette.TorchWarm;
        Vector2 n = Vector2.Normalize(wallNormal);
        Vector2 tangent = new Vector2(-n.Y, n.X);
        float washLen = 90f * scale;
        float washH = 48f * scale;
        Vector2 washBase = torchPos + n * (8f * scale);
        for (int strip = 0; strip < 6; strip++)
        {
            float along = strip / 5f;
            float asym = 0.35f + along * 0.65f;
            Vector2 center = washBase + tangent * ((along - 0.5f) * washLen * 1.4f) + n * (along * washH * 0.6f);
            float rx = (18f + strip * 4f) * scale * asym;
            float ry = (10f + strip * 2f) * scale;
            float rot = MathF.Atan2(tangent.Y, tangent.X) * 180f / MathF.PI;
            float inten = (0.04f + flicker * 0.05f) * (1f - strip * 0.12f);
            DrawEllipticalGlow(center, rx, ry, rot, warm, inten * (0.7f + flicker2 * 0.3f), 3);
        }
        DrawLightCone(torchPos, MathF.Atan2(n.Y, n.X), washH * 1.2f, 0.55f, warm, 0.06f + flicker * 0.05f);
    }

    static void DrawWindowInteriorGlow(Rectangle windowRect, float time, float phase)
    {
        float pulse = MathF.Sin(time * 1.5f + phase) * 0.5f + 0.5f;
        Color warm = new Color(180, 152, 108, 255);
        var inner = new Rectangle(windowRect.X + 3f, windowRect.Y + 4f, windowRect.Width - 6f, windowRect.Height - 8f);
        Raylib.DrawRectangleRounded(inner, 0.12f, 3, WithAlpha(warm, 0.10f + pulse * 0.06f));
        Vector2 center = new Vector2(inner.X + inner.Width / 2f, inner.Y + inner.Height / 2f);
        DrawEllipticalGlow(center, inner.Width * 0.55f, inner.Height * 0.5f, 0f, warm, 0.018f + pulse * 0.012f, 2);
    }

    static void DrawMoonCorona(Vector2 moonPos, float time)
    {
        float pulse = MathF.Sin(time * 0.35f) * 0.5f + 0.5f;
        Color corona = MenuPalette.MoonGlow;
        for (int i = 0; i < 5; i++)
        {
            float rx = 90f + i * 28f;
            float ry = 22f + i * 6f;
            float yOff = i * 4f - 6f;
            float rot = -8f + i * 3f;
            float inten = (0.018f - i * 0.0025f) * (0.85f + pulse * 0.15f);
            DrawEllipticalGlow(new Vector2(moonPos.X, moonPos.Y + yOff), rx, ry, rot, corona, inten, 4);
        }
    }
}
