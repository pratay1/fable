partial class Program
{
    // ---------------------------------------------------------------- UI widgets

    static void DrawScreenBackdrop(float dim, float vignetteStrength = 0.32f)
    {
        Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(ForestShadow, dim));
        DrawScreenVignette(vignetteStrength);
    }

    static void DrawUiScreenHeader(string title, string subtitle, string rightTag = "")
    {
        float headerH = string.IsNullOrEmpty(subtitle) ? 48f : 60f;
        var header = new Rectangle(20f, 14f, WindowWidth - 40f, headerH);
        DrawRichPanel(header, UiPanel, UiBorder, 0.2f, accentStripe: true);
        ShadowText(title, 34, 24, 26, UiAccent);
        if (!string.IsNullOrEmpty(subtitle))
        {
            DrawTextTruncated(subtitle, 34, 50, (int)header.Width - 68, 12, WithAlpha(Color.White, 0.55f));
        }

        if (!string.IsNullOrEmpty(rightTag))
        {
            string fit = TruncateText(rightTag, 180, 14);
            int tw = Raylib.MeasureText(fit, 14);
            Raylib.DrawText(fit, (int)(header.X + header.Width - tw - 14), 26, 14, Gold);
        }
    }

    static void DrawUiSectionLabel(string text, float x, float y, Color accent)
    {
        Raylib.DrawText(text, (int)x, (int)y, 13, WithAlpha(accent, 0.92f));
        Raylib.DrawLineEx(new Vector2(x, y + 18f), new Vector2(x + Raylib.MeasureText(text, 13), y + 18f), 1f, WithAlpha(accent, 0.35f));
    }

    static void DrawUiStatChip(Rectangle r, string label, string value, Color accent, bool ghost = false)
    {
        Color fill = ghost ? WithAlpha(UiPanel, 0.34f) : WithAlpha(UiPanel, 0.9f);
        Color border = ghost ? WithAlpha(UiBorder, 0.38f) : UiBorder;
        DrawRichPanel(r, fill, border, 0.22f);
        Raylib.DrawRectangle((int)r.X, (int)r.Y + 4, 3, (int)r.Height - 8, WithAlpha(accent, ghost ? 0.65f : 0.85f));
        DrawTextTruncated(label, (int)r.X + 12, (int)r.Y + 6, (int)r.Width - 16, 10, WithAlpha(Color.White, ghost ? 0.48f : 0.52f));
        DrawTextTruncated(value, (int)r.X + 12, (int)r.Y + 20, (int)r.Width - 16, 15, WithAlpha(Color.White, ghost ? 0.88f : 1f));
    }

    static void DrawUiContextBanner(float y, string text, Color accent)
    {
        var banner = new Rectangle(20f, y, WindowWidth - 40f, 24f);
        DrawRichPanel(banner, WithAlpha(UiPanelDeep, 0.82f), WithAlpha(accent, 0.45f), 0.35f);
        DrawTextTruncated(text, (int)banner.X + 12, (int)banner.Y + 6, (int)banner.Width - 24, 11, WithAlpha(Color.White, 0.78f));
    }

    static void DrawUiHintBar(string left, string center, string right)
    {
        var bar = new Rectangle(16f, UiHintBarY, WindowWidth - 32f, UiHintBarHeight);
        DrawRichPanel(bar, WithAlpha(UiPanelDeep, 0.78f), WithAlpha(UiBorder, 0.4f), 0.25f);
        float colW = (bar.Width - 24f) / 3f;
        DrawTextTruncated(left, (int)bar.X + 12, (int)bar.Y + 9, (int)colW, 11, WithAlpha(Color.White, 0.45f));
        if (!string.IsNullOrEmpty(center))
        {
            string fit = TruncateText(center, (int)colW, 11);
            int cw = Raylib.MeasureText(fit, 11);
            Raylib.DrawText(fit, (int)(bar.X + bar.Width / 2 - cw / 2), (int)bar.Y + 9, 11, WithAlpha(UiAccent, 0.82f));
        }

        if (!string.IsNullOrEmpty(right))
        {
            string fit = TruncateText(right, (int)colW, 11);
            int rw = Raylib.MeasureText(fit, 11);
            Raylib.DrawText(fit, (int)(bar.X + bar.Width - rw - 12), (int)bar.Y + 9, 11, WithAlpha(Color.White, 0.45f));
        }
    }

    static string CustomizeTabTitle() => customizeTab switch
    {
        CustomizeTab.Cosmetics => "Look",
        CustomizeTab.Weapons => "Arms",
        CustomizeTab.Upgrades => "Perks",
        CustomizeTab.Abilities => "Skills",
        CustomizeTab.Rank => "Rank",
        CustomizeTab.Bestiary => "Bestiary",
        CustomizeTab.Glossary => "Glossary",
        _ => throw new UnreachableException(),
    };

    static string CustomizeTabShortHint() => customizeTab switch
    {
        CustomizeTab.Cosmetics => "Trinkets are the main pick — scroll the grid. Tap a color chip to re-tint your knight.",
        CustomizeTab.Weapons => "Click an arm to equip or purchase it.",
        CustomizeTab.Upgrades => "Spend fables on permanent stat boosts.",
        CustomizeTab.Abilities => "Click a card to assign battle skills.",
        CustomizeTab.Rank => "Earn XP in runs to rank up. Roadmap shows auto-unlocks; siege arms need waves + fables.",
        CustomizeTab.Bestiary => "Enemy lore unlocks as you slay each foe type.",
        CustomizeTab.Glossary => "Quick reference for every floor catastrophe family.",
        _ => throw new UnreachableException(),
    };

    static string CustomizeTabSubtitle() => customizeTab switch
    {
        CustomizeTab.Cosmetics => "Pick trinkets below. Color chips beside the preview are optional heraldry.",
        CustomizeTab.Weapons => "Click an unlocked arm to equip it. Spend fables on locked siege weapons.",
        CustomizeTab.Upgrades => "Permanent stat boosts bought with fables between runs.",
        CustomizeTab.Abilities => "Choose two skills for your battle loadout. Click a card to assign a slot.",
        CustomizeTab.Rank => "Your rank, upcoming unlocks, and every siege arm with clear status pills.",
        CustomizeTab.Bestiary => "Kill counts and lore for every enemy in the siege.",
        CustomizeTab.Glossary => "Grouped tips for reading floor events during a run.",
        _ => throw new UnreachableException(),
    };

    static void DrawPlayControlLegend()
    {
        if (!showControlLegend) return;
        if (state != GameState.Playing) return;
        if (waveNumber > 1) return;
        var panel = new Rectangle(WindowWidth - 196f, 118f, 182f, 72f);
        DrawRichPanel(panel, WithAlpha(UiPanelDeep, 0.72f), WithAlpha(UiBorder, 0.35f), 0.22f);
        Raylib.DrawText("CONTROLS", (int)panel.X + 10, (int)panel.Y + 6, 9, WithAlpha(UiAccent, 0.85f));
        Raylib.DrawText("WASD  move", (int)panel.X + 10, (int)panel.Y + 20, 10, WithAlpha(Color.White, 0.55f));
        Raylib.DrawText("LMB   fire", (int)panel.X + 10, (int)panel.Y + 34, 10, WithAlpha(Color.White, 0.55f));
        Raylib.DrawText("RMB   reload", (int)panel.X + 10, (int)panel.Y + 48, 10, WithAlpha(Color.White, 0.55f));
        Raylib.DrawText(KeyName(abilityKey1) + " / " + KeyName(abilityKey2), (int)panel.X + 96, (int)panel.Y + 34, 10, WithAlpha(Gold, 0.75f));
        Raylib.DrawText("skills", (int)panel.X + 96, (int)panel.Y + 48, 9, WithAlpha(Color.White, 0.42f));
    }

    static bool Button(Rectangle r, string label, int fontSize, bool enabled, Color accent)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = enabled && Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        if (enabled)
        {
            var shadow = new Rectangle(r.X + 1f, r.Y + 3f, r.Width, r.Height);
            Raylib.DrawRectangleRounded(shadow, 0.32f, 6, WithAlpha(ForestShadow, hover ? 0.5f : 0.38f));
        }

        Color fill = !enabled ? Darken(UiPanel, 0.2f) : hover ? LerpColor(UiPanel, accent, 0.22f) : UiPanel;
        Color border = enabled ? (hover ? UiBorderLight : accent) : Darken(UiBorder, 0.4f);
        DrawRichPanel(r, fill, border, 0.32f, accentStripe: enabled && hover);
        if (hover && enabled)
        {
            DrawGlow(new Vector2(r.X + r.Width / 2f, r.Y + r.Height / 2f), MathF.Min(r.Width, r.Height) * 0.55f, accent, 0.035f);
        }

        Color textColor = enabled ? Color.White : WithAlpha(Color.White, 0.4f);
        int w = Raylib.MeasureText(label, fontSize);
        Raylib.DrawText(label, (int)(r.X + r.Width / 2 - w / 2), (int)(r.Y + r.Height / 2 - fontSize / 2), fontSize, textColor);
        return clicked;
    }

    static bool TabButton(Rectangle r, string label, bool active)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color fill = active ? LerpColor(UiPanel, UiAccent, 0.28f) : hover ? WithAlpha(UiAccent, 0.14f) : UiPanel;
        DrawRichPanel(r, fill, active ? UiAccent : UiBorder, 0.28f, accentStripe: active);

        if (active)
        {
            DrawPulseFrame(r, UiAccent, 0.28f, 4f, 0.25f);
            Raylib.DrawRectangle((int)r.X + 4, (int)(r.Y + r.Height - 4), (int)r.Width - 8, 3, UiAccent);
        }
        else if (hover)
        {
            Raylib.DrawRectangleLinesEx(new Rectangle(r.X + 2f, r.Y + 2f, r.Width - 4f, r.Height - 4f), 1f, WithAlpha(UiAccent, 0.35f));
        }

        int w = Raylib.MeasureText(label, 18);
        ShadowText(label, (int)(r.X + r.Width / 2 - w / 2), (int)(r.Y + r.Height / 2 - 9), 18, active ? Color.White : new Color(220, 230, 220, 255), active ? 1f : 0.7f);
        return clicked;
    }

    static bool Swatch(Rectangle r, Color color, bool selected, bool unlocked, string lockText, bool readyToBuy)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color border = selected ? UiBorderLight : hover ? UiAccent : UiBorder;
        if (unlocked && color.A > 200)
        {
            DrawRichPanel(r, color, border, 0.22f, accentStripe: selected);
        }
        else
        {
            DrawRichPanel(r, unlocked ? color : Darken(color, 0.6f), border, 0.22f, accentStripe: selected);
        }

        if (!unlocked)
        {
            Raylib.DrawRectangleRounded(r, 0.25f, 4, WithAlpha(ForestShadow, 0.55f));
            if (!string.IsNullOrEmpty(lockText))
            {
                int fs = lockText.Length > 10 ? 8 : lockText.Length > 7 ? 9 : 10;
                int maxW = (int)r.Width - 6;
                string fit = TruncateText(lockText, maxW, fs);
                int w = Raylib.MeasureText(fit, fs);
                ShadowText(fit, (int)(r.X + r.Width / 2 - w / 2), (int)(r.Y + r.Height / 2 - fs / 2), fs, readyToBuy ? Gold : Danger);
            }
            if (readyToBuy)
            {
                DrawPulseFrame(new Rectangle(r.X + 2f, r.Y + 2f, r.Width - 4f, r.Height - 4f), Gold, 0.22f, 5f, 0.18f);
            }
        }
        else if (selected)
        {
            ShadowText("\u2713", (int)(r.X + r.Width / 2 - 6), (int)(r.Y + r.Height / 2 - 8), 18, Color.White);
        }

        if (unlocked) DrawHeraldryHatch(r, color, (int)(r.X + r.Y));
        return clicked;
    }

    static float Slider(Rectangle r, float value)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, new Rectangle(r.X - 6f, r.Y - 10f, r.Width + 12f, r.Height + 20f));
        if (hover && Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            value = Math.Clamp((m.X - r.X) / r.Width, 0f, 1f);
        }

        Raylib.DrawRectangleRounded(r, 0.8f, 6, WithAlpha(UiPanelDeep, 0.85f));
        var fill = new Rectangle(r.X + 1f, r.Y + 1f, MathF.Max(0f, r.Width * value - 2f), r.Height - 2f);
        if (fill.Width > 0f) Raylib.DrawRectangleRounded(fill, 0.8f, 6, UiAccent);
        Raylib.DrawRectangleRoundedLines(r, 0.8f, 6, WithAlpha(UiBorder, 0.5f));
        Raylib.DrawCircleV(new Vector2(r.X + r.Width * value, r.Y + r.Height / 2f), 8f, UiBorderLight);
        Raylib.DrawCircleV(new Vector2(r.X + r.Width * value, r.Y + r.Height / 2f), 5f, Color.White);
        return value;
    }

    static float EaseOutBack(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    }

    static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
}
