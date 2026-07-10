partial class Program
{
    static void DrawFloaters()
    {
        if (!floatingTextEnabled) return;
        foreach (FloatingText f in floaters)
        {
            float age = f.MaxLife - f.Life;
            float scale = age < 0.12f ? 0.4f + (age / 0.12f) * 0.7f : 1.1f - Math.Clamp((age - 0.12f) / 0.12f, 0f, 1f) * 0.1f;
            float alpha = Math.Min(1f, f.Life / (f.MaxLife * 0.6f));
            int fs = Math.Max(1, (int)(f.Size * scale));
            int w = Raylib.MeasureText(f.Text, fs);
            int tx = (int)(f.Position.X - w / 2f);
            int ty = (int)(f.Position.Y);
            ShadowText(f.Text, tx, ty, fs, f.Color, alpha);
        }
    }

    static void DrawFlash()
    {
        if (flashEnabled && flash > 0.01f)
        {
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(flashColor, Math.Min(flash, 1f) * 0.45f));
        }
    }

    static void DrawImpactFlash()
    {
        if (!flashEnabled || impactFlash <= 0.008f) return;

        float a = Math.Clamp(impactFlash, 0f, 1f);
        if (impactFlashSharp)
        {
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(Color.White, a * 0.82f));
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(impactFlashColor, a * 0.4f));
            int crush = (int)(16f * a);
            if (crush > 0)
            {
                Color crushCol = WithAlpha(ForestShadow, a * 0.5f);
                Raylib.DrawRectangle(0, 0, WindowWidth, crush, crushCol);
                Raylib.DrawRectangle(0, WindowHeight - crush, WindowWidth, crush, crushCol);
                Raylib.DrawRectangle(0, 0, crush, WindowHeight, crushCol);
                Raylib.DrawRectangle(WindowWidth - crush, 0, crush, WindowHeight, crushCol);
            }
        }
        else
        {
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(impactFlashColor, a * 0.42f));
        }
    }

    static void DrawEntityShadows()
    {
        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 0.15f) continue;
            float grow = EaseOutBack(e.Spawn);
            DrawEntityAoShadow(e.Position, e.Radius * grow, e.Vel);
        }

        if (state == GameState.Playing || state == GameState.GameOver)
        {
            DrawEntityAoShadow(playerPos, PlayerRadius, playerVel);
        }
    }

    static void DrawEntityAoShadow(Vector2 anchor, float radius, Vector2 vel)
    {
        Vector2 baseOffset = new Vector2(radius * 0.12f, radius * 0.42f);
        float speed = vel.Length();
        Vector2 velOffset = speed > 4f
            ? Vector2.Normalize(vel) * Math.Min(speed * 0.009f, radius * 0.24f)
            : Vector2.Zero;
        Vector2 center = anchor + baseOffset + velOffset;
        float speedTilt = Math.Clamp(speed * 0.0016f, 0f, 0.22f);

        int layers = reduceMotion ? 2 : 4;
        for (int i = layers - 1; i >= 0; i--)
        {
            float t = (i + 1) / (float)layers;
            float rx = radius * (0.42f + t * 0.62f + speedTilt * 0.35f);
            float ry = radius * (0.2f + t * 0.24f);
            float alpha = (0.05f + t * 0.11f) * (1f - (1f - t) * 0.35f);
            Raylib.DrawEllipse((int)center.X, (int)center.Y, rx, ry, WithAlpha(ForestShadow, alpha));
        }
    }

    static void DrawVignette()
    {
        float vigMult = vignetteScale * (reduceMotion ? 0.72f : 1f);
        if (state == GameState.MainMenu)
        {
            int side = 110;
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(new Color(2, 2, 4, 255), 0.28f * vigMult));
            Color edge = WithAlpha(ForestShadow, 0.28f * vigMult);
            Raylib.DrawRectangleGradientV(0, WindowHeight - side, WindowWidth, side, WithAlpha(ForestShadow, 0f), edge);
            Raylib.DrawRectangleGradientH(0, 0, side, WindowHeight, WithAlpha(ForestShadow, 0.22f * vigMult), WithAlpha(ForestShadow, 0f));
            Raylib.DrawRectangleGradientH(WindowWidth - side, 0, side, WindowHeight, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, 0.22f * vigMult));
            return;
        }

        int t = 130;
        float vigStrength = (0.55f + adrenaline * 0.22f + impactFlash * 0.15f) * vigMult;
        Color edgeFull = WithAlpha(ForestShadow, vigStrength);
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, t, edgeFull, WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientV(0, WindowHeight - t, WindowWidth, t, WithAlpha(ForestShadow, 0f), edgeFull);
        Raylib.DrawRectangleGradientH(0, 0, t, WindowHeight, WithAlpha(ForestShadow, vigStrength * 0.9f), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH(WindowWidth - t, 0, t, WindowHeight, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, vigStrength * 0.9f));

        if (!reduceMotion && adrenaline > 0.08f)
        {
            Color pulse = WithAlpha(new Color(180, 48, 38, 255), adrenaline * glowPulse * 0.12f * vigMult);
            Raylib.DrawRectangleGradientV(0, 0, WindowWidth, 90, pulse, WithAlpha(pulse, 0f));
            Raylib.DrawRectangleGradientV(0, WindowHeight - 90, WindowWidth, 90, WithAlpha(pulse, 0f), pulse);
        }
    }

    static void DrawGlow(Vector2 center, float radius, Color color, float intensity)
        => DrawSmoothGlowCore(center, radius, color, intensity, 0.11f, steps: 24);

    static void DrawGlowFast(Vector2 center, float radius, Color color, float intensity)
        => DrawSmoothGlowCore(center, radius, color, intensity, 0.11f, steps: 16);

    static float Approach(float current, float target, float rate, float dt)
        => current + (target - current) * (1f - MathF.Exp(-rate * dt));

    static void ShadowText(string text, int x, int y, int size, Color color, float alpha = 1f, int off = 2)
    {
        Raylib.DrawText(text, x + off, y + off, size, WithAlpha(ForestShadow, alpha * 0.7f));
        Raylib.DrawText(text, x, y, size, WithAlpha(color, alpha));
    }

    static void ShadowTextCentered(string text, int cx, int y, int size, Color color, float alpha = 1f, int off = 2)
    {
        int w = Raylib.MeasureText(text, size);
        ShadowText(text, cx - w / 2, y, size, color, alpha, off);
    }

    static float AngleFromVector(Vector2 primary, Vector2 fallback)
    {
        Vector2 v = primary.LengthSquared() > 1f ? primary : fallback;
        if (v.LengthSquared() < 0.001f) v = Vector2.UnitY;
        return MathF.Atan2(v.Y, v.X) * 180f / MathF.PI;
    }

    static bool BeginStretch(Vector2 center, Vector2 vel, float intensity, float cap, float hitSquash = 0f)
    {
        float speed = vel.Length();
        float stretch = Math.Clamp(speed * intensity + hitSquash, 0f, cap + hitSquash * 0.4f);
        if (stretch < 0.0035f)
        {
            return false;
        }

        float angle = speed > 1.5f
            ? MathF.Atan2(vel.Y, vel.X) * 180f / MathF.PI
            : hitSquash > 0.01f
                ? AngleFromVector(center - playerPos, lastMoveDirection)
                : AngleFromVector(lastMoveDirection, Vector2.UnitY);
        float squashX = 1f + stretch;
        float squashY = 1f - stretch * 0.68f;
        Rlgl.PushMatrix();
        Rlgl.Translatef(center.X, center.Y, 0f);
        Rlgl.Rotatef(angle, 0f, 0f, 1f);
        Rlgl.Scalef(squashX, squashY, 1f);
        Rlgl.Rotatef(-angle, 0f, 0f, 1f);
        Rlgl.Translatef(-center.X, -center.Y, 0f);
        return true;
    }

    static void EndStretch(bool active)
    {
        if (active)
        {
            Rlgl.PopMatrix();
        }
    }

    static void DrawPulseFrame(Rectangle rect, Color color, float roundness, float speed, float baseAlpha)
    {
        float pulse = MathF.Sin((float)Raylib.GetTime() * speed) * 0.5f + 0.5f;
        var outer = new Rectangle(rect.X - 3f, rect.Y - 3f, rect.Width + 6f, rect.Height + 6f);
        Raylib.DrawRectangleRoundedLines(outer, roundness, 6, WithAlpha(color, baseAlpha + pulse * 0.5f));
        Raylib.DrawRectangleRoundedLines(rect, roundness, 6, WithAlpha(color, baseAlpha * 0.6f + pulse * 0.3f));
    }

    static void DrawPulseFrameLite(Rectangle rect, Color color, float roundness, float speed, float baseAlpha)
    {
        float pulse = MathF.Sin((float)Raylib.GetTime() * speed) * 0.5f + 0.5f;
        Raylib.DrawRectangleRoundedLines(rect, roundness, 4, WithAlpha(color, baseAlpha + pulse * 0.55f));
    }

    static void DrawPanel(Rectangle rect, Color fill, Color border, float roundness)
    {
        var outer = new Rectangle(rect.X - 2f, rect.Y - 2f, rect.Width + 4f, rect.Height + 4f);
        Raylib.DrawRectangleRounded(outer, roundness, 6, border);
        Raylib.DrawRectangleRounded(rect, roundness, 6, fill);
    }

    static void DrawScreenVignette(float strength)
    {
        int edge = (int)(WindowWidth * 0.16f);
        Color vignette = WithAlpha(ForestShadow, strength * 0.52f);
        Raylib.DrawRectangleGradientH(0, 0, edge, WindowHeight, vignette, WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientH(WindowWidth - edge, 0, edge, WindowHeight, WithAlpha(ForestShadow, 0f), vignette);
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, edge, WithAlpha(ForestShadow, strength * 0.42f), WithAlpha(ForestShadow, 0f));
        Raylib.DrawRectangleGradientV(0, WindowHeight - edge, WindowWidth, edge, WithAlpha(ForestShadow, 0f), WithAlpha(ForestShadow, strength * 0.42f));
    }

    static void DrawMedievalDivider(float cx, float y, float width)
    {
        float half = width / 2f;
        Raylib.DrawLineEx(new Vector2(cx - half, y), new Vector2(cx - 14f, y), 1.5f, WithAlpha(UiBorder, 0.55f));
        Raylib.DrawLineEx(new Vector2(cx + 14f, y), new Vector2(cx + half, y), 1.5f, WithAlpha(UiBorder, 0.55f));
        Raylib.DrawCircleV(new Vector2(cx, y), 3.2f, WithAlpha(UiBorder, 0.65f));
        Raylib.DrawCircleV(new Vector2(cx, y), 1.6f, WithAlpha(Gold, 0.8f));
    }

    static void DrawRichPanel(Rectangle rect, Color fill, Color border, float roundness, bool accentStripe = false)
    {
        var shadow = new Rectangle(rect.X + 2f, rect.Y + 3f, rect.Width, rect.Height);
        Raylib.DrawRectangleRounded(shadow, roundness, 8, WithAlpha(ForestShadow, 0.46f));
        var outerBevel = new Rectangle(rect.X - 1f, rect.Y - 1f, rect.Width + 2f, rect.Height + 2f);
        Raylib.DrawRectangleRounded(outerBevel, roundness, 8, WithAlpha(Darken(border, 0.3f), 0.55f));
        DrawPanel(rect, fill, border, roundness);

        var innerGrad = new Rectangle(rect.X + 2f, rect.Y + 2f, rect.Width - 4f, rect.Height - 4f);
        if (innerGrad.Width > 2f && innerGrad.Height > 2f)
        {
            Raylib.DrawRectangleGradientV((int)innerGrad.X, (int)innerGrad.Y, (int)innerGrad.Width, (int)innerGrad.Height,
                WithAlpha(Lighten(fill, 0.07f), 0.055f), WithAlpha(Darken(fill, 0.12f), 0.035f));
        }

        var highlight = new Rectangle(rect.X + 4f, rect.Y + 4f, rect.Width - 8f, MathF.Max(8f, rect.Height * 0.38f));
        Raylib.DrawRectangleRounded(highlight, roundness, 4, WithAlpha(Color.White, 0.05f));

        Raylib.DrawRectangleRoundedLines(
            new Rectangle(rect.X + 3f, rect.Y + 3f, rect.Width - 6f, rect.Height - 6f),
            roundness * 0.92f, 6, WithAlpha(UiBorderLight, 0.16f));

        if (accentStripe)
        {
            var stripe = new Rectangle(rect.X + 6f, rect.Y + 5f, rect.Width - 12f, 3f);
            Raylib.DrawRectangleGradientH((int)stripe.X, (int)stripe.Y, (int)stripe.Width, (int)stripe.Height,
                WithAlpha(Darken(border, 0.25f), 0.3f), WithAlpha(UiAccent, 0.68f));
            Raylib.DrawRectangle((int)(stripe.X + stripe.Width / 2f - 1f), (int)stripe.Y, 2, (int)stripe.Height, WithAlpha(Gold, 0.5f));
        }

        if (rect.Width > 120f && rect.Height > 70f)
        {
            Color bossHi = WithAlpha(UiBorderLight, 0.5f);
            Color bossLo = WithAlpha(Darken(border, 0.2f), 0.42f);
            Raylib.DrawCircleV(new Vector2(rect.X + 8f, rect.Y + 8f), 2.2f, bossHi);
            Raylib.DrawCircleV(new Vector2(rect.X + rect.Width - 8f, rect.Y + 8f), 2.2f, bossHi);
            Raylib.DrawCircleV(new Vector2(rect.X + 8f, rect.Y + rect.Height - 8f), 2.2f, bossLo);
            Raylib.DrawCircleV(new Vector2(rect.X + rect.Width - 8f, rect.Y + rect.Height - 8f), 2.2f, bossLo);
        }
    }

}
