partial class Program
{
    // ---------------------------------------------------------------- Menu

    static void UpdateDifficultySelect(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            state = GameState.MainMenu;
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
        {
            difficultyMenuIndex = Math.Max(0, difficultyMenuIndex - 1);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
        {
            difficultyMenuIndex = Math.Min(DifficultyProfiles.Length - 1, difficultyMenuIndex + 1);
        }

        for (int i = 0; i < DifficultyProfiles.Length; i++)
        {
            if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i)))
            {
                difficultyMenuIndex = i;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q)) runOathFlags ^= 1 << (int)OathType.NoVerdict;
        if (Raylib.IsKeyPressed(KeyboardKey.E)) runOathFlags ^= 1 << (int)OathType.NoOath;
        if (Raylib.IsKeyPressed(KeyboardKey.H)) runOathFlags ^= 1 << (int)OathType.HeraldryBound;
        if (Raylib.IsKeyPressed(KeyboardKey.N) && difficultyMenuIndex == (int)Difficulty.FableNightmare)
            runOathFlags ^= 1 << (int)OathType.PureNightmare;

        runDifficulty = (Difficulty)difficultyMenuIndex;
        difficultySelectAnim = Approach(difficultySelectAnim, difficultyMenuIndex, 14f, dt);

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            SaveGame();
            ResetGame();
        }
    }

    static void DifficultyMeterValues(in DifficultyProfile p, out float foes, out float events, out float pace, out float chaos)
    {
        foes = Math.Clamp(p.EnemyHpMult * 0.34f + p.GruntCountMult * 0.33f + p.SwarmCountMult * 0.33f, 0.08f, 1f) / 1.65f;
        pace = Math.Clamp(1.05f / p.WavePauseMult, 0.08f, 1f);
        events = Math.Clamp((18f / Math.Max(6f, p.FirstEventCooldown)) * 0.42f + (12f / Math.Max(1, p.MinWaveForEvents)) * 0.28f
            + p.EventIntensityMult * 0.3f, 0.08f, 1f);
        chaos = Math.Clamp(p.EventStackChance * 1.8f + p.EventSurgeChance * 1.4f + (p.EventIntensityMult - 0.5f) * 0.32f, 0f, 1f);
    }

    static string DifficultyMeterLabel(float t) => t switch
    {
        < 0.22f => "GENTLE",
        < 0.42f => "LOW",
        < 0.58f => "STEADY",
        < 0.76f => "HIGH",
        < 0.92f => "BRUTAL",
        _ => "LETHAL",
    };

    static void DrawDifficultyStatMeter(Rectangle area, string label, float value, Color accent, bool highlight)
    {
        float t = Math.Clamp(value, 0f, 1f);
        DrawRichPanel(area, WithAlpha(UiPanelDeep, 0.72f), WithAlpha(accent, highlight ? 0.42f : 0.22f), 0.22f);
        Raylib.DrawText(label, (int)area.X + 10, (int)area.Y + 5, 10, WithAlpha(Color.White, 0.5f));
        string val = DifficultyMeterLabel(t);
        int vw = Raylib.MeasureText(val, 10);
        Raylib.DrawText(val, (int)(area.X + area.Width - vw - 10), (int)area.Y + 5, 10, WithAlpha(accent, highlight ? 0.95f : 0.72f));

        var track = new Rectangle(area.X + 10f, area.Y + 22f, area.Width - 20f, 8f);
        Raylib.DrawRectangleRounded(track, 1f, 4, WithAlpha(ForestShadow, 0.85f));
        if (t > 0.02f)
        {
            var fill = new Rectangle(track.X, track.Y, track.Width * t, track.Height);
            Raylib.DrawRectangleRounded(fill, 1f, 4, WithAlpha(accent, 0.88f));
            Raylib.DrawRectangleGradientV((int)fill.X, (int)fill.Y, (int)fill.Width, (int)fill.Height,
                WithAlpha(Lighten(accent, 0.35f), 0.55f), WithAlpha(Darken(accent, 0.15f), 0.95f));
            Raylib.DrawRectangle((int)fill.X, (int)fill.Y, (int)fill.Width, 1, WithAlpha(Color.White, 0.22f));
        }

        Raylib.DrawRectangleRoundedLines(track, 1f, 4, WithAlpha(accent, 0.25f));
    }

    static void DrawDifficultySigil(Vector2 c, float r, int index, Color accent, Color accentHi, float time, bool nightmare)
    {
        float pulse = MathF.Sin(time * (nightmare ? 4f : 2.4f) + index) * 0.5f + 0.5f;
        DrawGlow(c, r * 2.8f, accent, nightmare ? 0.08f + pulse * 0.06f : 0.04f + pulse * 0.03f);
        Raylib.DrawCircleV(c, r + 2f, WithAlpha(Darken(accent, 0.45f), 0.85f));
        Raylib.DrawCircleV(c, r, WithAlpha(accent, 0.92f));
        Raylib.DrawCircleV(c - new Vector2(r * 0.28f, r * 0.32f), r * 0.34f, WithAlpha(accentHi, 0.55f + pulse * 0.25f));

        switch (index)
        {
            case 0:
                Raylib.DrawCircleLinesV(c, r * 0.55f, WithAlpha(accentHi, 0.75f));
                Raylib.DrawLineEx(c + new Vector2(0, -r * 0.35f), c + new Vector2(0, r * 0.35f), 1.5f, accentHi);
                break;
            case 1:
                Raylib.DrawLineEx(c + new Vector2(-r * 0.35f, r * 0.2f), c + new Vector2(r * 0.35f, -r * 0.2f), 1.5f, accentHi);
                break;
            case 2:
                Raylib.DrawLineEx(c + new Vector2(-r * 0.38f, 0), c + new Vector2(r * 0.38f, 0), 2f, accentHi);
                Raylib.DrawLineEx(c + new Vector2(0, -r * 0.38f), c + new Vector2(0, r * 0.38f), 2f, WithAlpha(accentHi, 0.65f));
                break;
            case 3:
                for (int sp = 0; sp < 3; sp++)
                {
                    float fx = -r * 0.34f + sp * r * 0.34f;
                    Raylib.DrawTriangle(c + new Vector2(fx, -r * 0.42f), c + new Vector2(fx - r * 0.12f, -r * 0.08f),
                        c + new Vector2(fx + r * 0.12f, -r * 0.08f), accentHi);
                }
                break;
            default:
                Raylib.DrawCircleV(c + new Vector2(0, -r * 0.08f), r * 0.16f, WithAlpha(accentHi, 0.9f));
                Raylib.DrawLineEx(c + new Vector2(-r * 0.22f, r * 0.18f), c + new Vector2(r * 0.22f, r * 0.18f), 1.5f, accentHi);
                for (int drip = 0; drip < 3; drip++)
                {
                    float dx = -r * 0.18f + drip * r * 0.18f;
                    Raylib.DrawLineEx(c + new Vector2(dx, r * 0.18f), c + new Vector2(dx, r * 0.42f + pulse * 3f), 1.2f,
                        WithAlpha(accentHi, 0.55f));
                }
                break;
        }
    }

    static void DrawDifficultyScreenFrame(Rectangle frame, Color accent, float time)
    {
        Color stoneMid = new Color(42, 40, 38, 255);
        Color stoneLight = new Color(96, 92, 86, 255);
        Color mortar = new Color(24, 22, 22, 255);
        float pulse = MathF.Sin(time * 1.4f) * 0.5f + 0.5f;

        DrawRichPanel(frame, WithAlpha(stoneMid, 0.78f), WithAlpha(stoneLight, 0.32f), 0.14f, accentStripe: true);
        DrawStoneMasonry(frame, stoneMid, mortar, 8, 14, 0.42f);
        DrawQuoinStripes(frame, stoneLight, MossLight);

        var inner = new Rectangle(frame.X + 12f, frame.Y + 10f, frame.Width - 24f, frame.Height - 20f);
        Raylib.DrawRectangleLinesEx(inner, 1f, WithAlpha(accent, 0.18f + pulse * 0.08f));
        Raylib.DrawRectangleLinesEx(frame, 1.5f, WithAlpha(stoneLight, 0.28f));

        float inset = 16f;
        Color corner = WithAlpha(accent, 0.45f);
        Raylib.DrawLineEx(new Vector2(frame.X + inset, frame.Y + inset), new Vector2(frame.X + inset + 26f, frame.Y + inset), 1.5f, corner);
        Raylib.DrawLineEx(new Vector2(frame.X + inset, frame.Y + inset), new Vector2(frame.X + inset, frame.Y + inset + 26f), 1.5f, corner);
        Raylib.DrawLineEx(new Vector2(frame.X + frame.Width - inset, frame.Y + inset), new Vector2(frame.X + frame.Width - inset - 26f, frame.Y + inset), 1.5f, corner);
        Raylib.DrawLineEx(new Vector2(frame.X + frame.Width - inset, frame.Y + inset), new Vector2(frame.X + frame.Width - inset, frame.Y + inset + 26f), 1.5f, corner);
        Raylib.DrawLineEx(new Vector2(frame.X + inset, frame.Y + frame.Height - inset), new Vector2(frame.X + inset + 26f, frame.Y + frame.Height - inset), 1.5f, WithAlpha(stoneLight, 0.35f));
        Raylib.DrawLineEx(new Vector2(frame.X + frame.Width - inset, frame.Y + frame.Height - inset), new Vector2(frame.X + frame.Width - inset - 26f, frame.Y + frame.Height - inset), 1.5f, WithAlpha(stoneLight, 0.35f));
    }

    static void DrawDifficultyHeaderPlaque(int cx, float time, in DifficultyProfile chosen, bool nightmare)
    {
        var plaque = new Rectangle(cx - 290f, 46f, 580f, 88f);
        Color stoneMid = new Color(44, 42, 40, 255);
        Color stoneLight = new Color(98, 94, 88, 255);
        Color mortar = new Color(26, 24, 24, 255);
        float pulse = MathF.Sin(time * 1.6f) * 0.5f + 0.5f;

        DrawRichPanel(plaque, WithAlpha(stoneMid, 0.82f), WithAlpha(chosen.Accent, nightmare ? 0.42f : 0.24f), 0.12f, accentStripe: true);
        DrawStoneMasonry(plaque, stoneMid, mortar, 3, 12, 0.4f);
        DrawQuoinStripes(plaque, stoneLight, nightmare ? chosen.AccentHi : MossLight);

        ShadowTextCentered("CHOOSE YOUR TRIAL", cx, (int)plaque.Y + 16, 30, nightmare ? chosen.AccentHi : new Color(196, 188, 172, 255));
        string lead = "Swear your oath before the stones begin to fall.";
        ShadowTextCentered(lead, cx, (int)plaque.Y + 50, 13, WithAlpha(Color.White, 0.58f));

        Vector2 gem = new Vector2(plaque.X + plaque.Width - 28f, plaque.Y + plaque.Height / 2f);
        DrawTrinketGem(gem, 9f, chosen.Accent, time, nightmare ? 1.2f : 0.85f);
        Raylib.DrawCircleV(gem, 12f + pulse * 2f, WithAlpha(chosen.Accent, nightmare ? 0.08f : 0.04f));
    }

    static void DrawDifficultyTimeline(float x, float y, float height, float cardH, float gap, float animIndex, float time)
    {
        float nodeStep = cardH + gap;
        float markerY = y + animIndex * nodeStep + cardH * 0.5f;
        Color rail = WithAlpha(new Color(88, 84, 78, 255), 0.55f);
        Raylib.DrawLineEx(new Vector2(x, y + 8f), new Vector2(x, y + height - 8f), 2f, rail);

        for (int i = 0; i < DifficultyProfiles.Length; i++)
        {
            float ny = y + i * nodeStep + cardH * 0.5f;
            Color nodeCol = DifficultyProfiles[i].Accent;
            float size = i == difficultyMenuIndex ? 6f : 4f;
            Raylib.DrawCircleV(new Vector2(x, ny), size + 1.5f, WithAlpha(Darken(nodeCol, 0.4f), 0.8f));
            Raylib.DrawCircleV(new Vector2(x, ny), size, WithAlpha(nodeCol, i == difficultyMenuIndex ? 0.95f : 0.45f));
        }

        DrawGlow(new Vector2(x, markerY), 28f, DifficultyProfiles[difficultyMenuIndex].Accent, 0.05f + MathF.Sin(time * 3f) * 0.02f);
        Raylib.DrawRing(new Vector2(x, markerY), 7f, 10f, 0f, 360f, 24, WithAlpha(DifficultyProfiles[difficultyMenuIndex].AccentHi, 0.75f));
    }

    static bool DrawDifficultyPreviewPanel(Rectangle panel, in DifficultyProfile profile, bool nightmare, float time)
    {
        float pulse = MathF.Sin(time * (nightmare ? 3.6f : 2f)) * 0.5f + 0.5f;
        Color fill = nightmare
            ? LerpColor(new Color(24, 8, 12, 255), new Color(46, 10, 18, 255), 0.35f + pulse * 0.25f)
            : WithAlpha(UiPanel, 0.9f);
        DrawRichPanel(panel, fill, WithAlpha(profile.Accent, nightmare ? 0.62f : 0.38f), 0.18f, accentStripe: true);

        if (nightmare)
        {
            Raylib.DrawRectangleGradientV((int)panel.X, (int)panel.Y, (int)panel.Width, (int)(panel.Height * 0.35f),
                WithAlpha(new Color(80, 8, 16, 255), 0.22f + pulse * 0.1f), WithAlpha(new Color(80, 8, 16, 255), 0f));
        }

        Vector2 sigilCenter = new Vector2(panel.X + panel.Width / 2f, panel.Y + 44f);
        DrawDifficultySigil(sigilCenter, 20f, difficultyMenuIndex, profile.Accent, profile.AccentHi, time, nightmare);

        ShadowTextCentered(profile.Title, (int)(panel.X + panel.Width / 2f), (int)(panel.Y + 76f), nightmare ? 17 : 16, profile.AccentHi);
        DrawTextTruncated(profile.Tagline, (int)(panel.X + panel.Width / 2f - (panel.Width - 28f) / 2f), (int)(panel.Y + 98f),
            (int)(panel.Width - 28f), 11, WithAlpha(Color.White, 0.62f));

        DifficultyMeterValues(profile, out float foes, out float events, out float pace, out _);
        float meterH = 28f;
        float meterGap = 5f;
        float meterY = panel.Y + 118f;
        float meterW = panel.Width - 24f;
        DrawDifficultyStatMeter(new Rectangle(panel.X + 12f, meterY, meterW, meterH), "FOES", foes, profile.Accent, true);
        DrawDifficultyStatMeter(new Rectangle(panel.X + 12f, meterY + meterH + meterGap, meterW, meterH), "EVENTS", events, profile.Accent, true);
        DrawDifficultyStatMeter(new Rectangle(panel.X + 12f, meterY + (meterH + meterGap) * 2, meterW, meterH), "PACE", pace, profile.Accent, false);

        float recordsY = meterY + (meterH + meterGap) * 3 + 10f;
        int rdi = difficultyMenuIndex;
        if (rdi >= 0 && rdi < difficultyRecords.Length)
        {
            ref DifficultyRecord rec = ref difficultyRecords[rdi];
            string recLine = $"Your best - wave {rec.BestWave} � {rec.BestScore:N0} pts";
            DrawTextTruncated(recLine, (int)(panel.X + 12f), (int)recordsY, (int)meterW, 10, WithAlpha(profile.Accent, 0.72f));
        }

        const float btnH = 44f;
        const float oathH = 108f;
        const float bottomPad = 10f;
        float btnY = panel.Y + panel.Height - bottomPad - btnH;
        float oathY = btnY - 10f - oathH;

        var oathArea = new Rectangle(panel.X + 10f, oathY, panel.Width - 20f, oathH);
        DrawDifficultyOathChips(oathArea, profile, time);

        return Button(new Rectangle(panel.X + 10f, btnY, panel.Width - 20f, btnH), "BEGIN TRIAL  [ENTER]", 22, true, profile.Accent);
    }

    static void DrawDifficultyOathChips(Rectangle area, in DifficultyProfile profile, float time)
    {
        DrawRichPanel(area, WithAlpha(UiPanelDeep, 0.55f), WithAlpha(profile.Accent, 0.22f), 0.14f);
        DrawUiSectionLabel("SWORN OATHS", area.X + 8f, area.Y + 6f, profile.Accent);

        float gridY = area.Y + 26f;
        float gap = 6f;
        float chipW = (area.Width - 16f - gap) / 2f;
        float chipH = 34f;

        (OathType type, string key, string title, string bonus)[] oaths =
        {
            (OathType.NoVerdict, "Q", "No Verdict", "+15% rewards"),
            (OathType.NoOath, "E", "No Oath", "+20% rewards"),
            (OathType.HeraldryBound, "H", "Heraldry", "Lock colors"),
            (OathType.PureNightmare, "N", "Pure Nightmare", "+25% Nightmare"),
        };

        Vector2 mouse = Raylib.GetMousePosition();
        for (int i = 0; i < oaths.Length; i++)
        {
            int col = i % 2;
            int row = i / 2;
            var chip = new Rectangle(area.X + 8f + col * (chipW + gap), gridY + row * (chipH + gap), chipW, chipH);
            (OathType type, string key, string title, string bonus) = oaths[i];
            bool on = (runOathFlags & (1 << (int)type)) != 0;
            bool disabled = type == OathType.PureNightmare && difficultyMenuIndex != (int)Difficulty.FableNightmare;
            bool hover = !disabled && Raylib.CheckCollisionPointRec(mouse, chip);
            float pulse = MathF.Sin(time * 3.2f + i) * 0.5f + 0.5f;

            Color fill = disabled
                ? WithAlpha(UiPanelDeep, 0.5f)
                : on
                    ? LerpColor(UiPanel, Gold, 0.22f + pulse * 0.08f)
                    : hover
                        ? WithAlpha(UiPanel, 0.95f)
                        : WithAlpha(UiPanelDeep, 0.72f);
            Color border = disabled
                ? WithAlpha(UiBorder, 0.35f)
                : on
                    ? LerpColor(Gold, profile.Accent, 0.35f)
                    : hover
                        ? WithAlpha(profile.Accent, 0.65f)
                        : WithAlpha(profile.Accent, 0.28f);

            DrawRichPanel(chip, fill, border, 0.18f, accentStripe: on);
            if (on) DrawPulseFrame(chip, Gold, 0.14f, 3.5f, 0.2f + pulse * 0.15f);

            var keyBadge = new Rectangle(chip.X + 6f, chip.Y + 7f, 20f, 18f);
            DrawRichPanel(keyBadge, WithAlpha(profile.Accent, disabled ? 0.25f : 0.55f), WithAlpha(profile.AccentHi, 0.4f), 0.2f);
            ShadowText(key, (int)(keyBadge.X + 5f), (int)(keyBadge.Y + 2f), 11, disabled ? WithAlpha(Color.White, 0.35f) : Color.White);

            Color titleCol = disabled ? WithAlpha(Color.White, 0.32f) : on ? Gold : Color.White;
            DrawTextTruncated(title, (int)(chip.X + 30f), (int)(chip.Y + 7f), (int)(chip.Width - 36f), 11, titleCol);
            DrawTextTruncated(bonus, (int)(chip.X + 30f), (int)(chip.Y + 20f), (int)(chip.Width - 36f), 8,
                WithAlpha(on ? Gold : MossLight, disabled ? 0.28f : 0.65f));

            if (on)
            {
                ShadowText("\u2713", (int)(chip.X + chip.Width - 16f), (int)(chip.Y + 9f), 12, Gold);
            }

            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                runOathFlags ^= 1 << (int)type;
            }
        }
    }

    static (string name, string desc, Color accent) GetBlessingInfo(BlessingType b) => b switch
    {
        BlessingType.SwiftMarch => ("Swift March", "+5% movement speed for the rest of the run.", MossLight),
        BlessingType.DeepPockets => ("Deep Pockets", "+12% fables from every source.", Gold),
        BlessingType.Stonecraft => ("Stonecraft", "Tiles underfoot wear down more slowly.", new Color(148, 142, 132, 255)),
        BlessingType.LongFuse => ("Long Fuse", "+0.5s on event telegraphs - read the floor.", new Color(120, 170, 220, 255)),
        BlessingType.ThornVolley => ("Thorn Volley", "Extra projectile splinter on each shot.", Danger),
        BlessingType.LuckySigil => ("Lucky Sigil", "+8% fables and brighter fortune.", new Color(220, 190, 90, 255)),
        BlessingType.IronSoles => ("Iron Soles", "Reduced durability loss while moving.", UiBorderLight),
        BlessingType.KeenEye => ("Keen Eye", "Tighter spread and cleaner aim.", UiAccent),
        BlessingType.BannerWard => ("Banner Ward", "Banner of Stillness lasts longer.", AbilityAccent(AbilityType.BannerOfStillness)),
        BlessingType.SiegeRations => ("Siege Rations", "+10% end-of-run reward multiplier.", new Color(196, 128, 72, 255)),
        BlessingType.LastLight => ("Last Light", "Stronger near-death vignette warning.", new Color(240, 230, 210, 255)),
        BlessingType.WindBlessing => ("Wind Blessing", "Shorter dash cooldown between leaps.", AbilityAccent(AbilityType.WindStep)),
        _ => ("Unknown", "A mystery boon from the crown.", Color.White),
    };

    static bool DrawBlessingCard(Rectangle card, BlessingType blessing, int slot, float time, bool selected)
    {
        (string name, string desc, Color accent) = GetBlessingInfo(blessing);
        Vector2 mouse = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(mouse, card);
        float pulse = MathF.Sin(time * 4f + slot * 1.7f) * 0.5f + 0.5f;
        float lift = hover ? 6f : 0f;
        var drawCard = new Rectangle(card.X, card.Y - lift, card.Width, card.Height);

        Color fill = LerpColor(new Color(28, 26, 24, 255), UiPanel, 0.35f + pulse * 0.15f);
        if (hover || selected) fill = LerpColor(fill, accent, 0.12f + pulse * 0.08f);
        DrawRichPanel(drawCard, WithAlpha(fill, 0.96f), WithAlpha(accent, hover ? 0.75f : 0.38f), 0.2f, accentStripe: true);

        if (hover)
        {
            DrawGlow(new Vector2(drawCard.X + drawCard.Width / 2f, drawCard.Y + drawCard.Height / 2f),
                drawCard.Width * 0.55f, accent, 0.06f + pulse * 0.04f);
            DrawPulseFrame(drawCard, accent, 0.16f, 4f, 0.28f + pulse * 0.2f);
        }

        Raylib.DrawRectangleGradientV((int)drawCard.X, (int)drawCard.Y, (int)drawCard.Width, (int)(drawCard.Height * 0.42f),
            WithAlpha(accent, 0.14f + pulse * 0.1f), WithAlpha(accent, 0f));

        Vector2 gemCenter = new Vector2(drawCard.X + drawCard.Width / 2f, drawCard.Y + 44f);
        DrawTrinketGem(gemCenter, 16f, accent, time, hover ? 1.35f : 1f);
        Raylib.DrawCircleLinesV(gemCenter, 24f + pulse * 4f, WithAlpha(accent, 0.35f + pulse * 0.25f));

        var keyBadge = new Rectangle(drawCard.X + drawCard.Width / 2f - 14f, drawCard.Y + 10f, 28f, 24f);
        DrawRichPanel(keyBadge, WithAlpha(Gold, 0.85f), WithAlpha(Gold, 0.4f), 0.22f);
        ShadowTextCentered(slot.ToString(), (int)(keyBadge.X + keyBadge.Width / 2f), (int)(keyBadge.Y + 4f), 16, new Color(40, 32, 20, 255));

        ShadowTextCentered(name.ToUpperInvariant(), (int)(drawCard.X + drawCard.Width / 2f), (int)(drawCard.Y + 78f), 17, accent);
        DrawMedievalDivider(drawCard.X + drawCard.Width / 2f, drawCard.Y + 100f, drawCard.Width - 36f);

        DrawTextTruncated(desc, (int)(drawCard.X + 16f), (int)(drawCard.Y + 112f), (int)(drawCard.Width - 32f), 12,
            WithAlpha(Color.White, hover ? 0.82f : 0.62f));

        string hint = hover ? "CLICK TO CLAIM" : "KEY " + slot;
        ShadowTextCentered(hint, (int)(drawCard.X + drawCard.Width / 2f), (int)(drawCard.Y + drawCard.Height - 22f), 11,
            WithAlpha(hover ? Color.White : accent, hover ? 0.9f : 0.55f));

        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    static void DrawBlessingPickerScreen()
    {
        int cx = WindowWidth / 2;
        int cy = WindowHeight / 2;
        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 2f) * 0.5f + 0.5f;

        DrawScreenBackdrop(0.88f, 0.48f);
        Raylib.DrawRectangleGradientV(0, 0, WindowWidth, WindowHeight,
            WithAlpha(new Color(48, 38, 18, 255), 0.22f + pulse * 0.08f), WithAlpha(new Color(12, 14, 18, 255), 0.55f));

        for (int ray = 0; ray < 8; ray++)
        {
            float ang = time * 0.35f + ray * MathF.PI / 4f;
            Vector2 origin = new Vector2(cx, cy - 40f);
            Vector2 tip = origin + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 420f;
            Raylib.DrawLineEx(origin, tip, 2f, WithAlpha(Gold, 0.04f + pulse * 0.03f));
        }

        var frame = new Rectangle(cx - 380f, cy - 230f, 760f, 460f);
        DrawRichPanel(frame, WithAlpha(UiPanelDeep, 0.94f), WithAlpha(Gold, 0.45f + pulse * 0.2f), 0.15f, accentStripe: true);
        DrawPulseFrame(frame, Gold, 0.1f, 2.5f, 0.15f + pulse * 0.12f);

        ShadowTextCentered("A BLESSING DESCENDS", cx, (int)(frame.Y + 28f), 32, Gold);
        string sub = $"Wave {waveNumber} cleared  �  choose one boon for the siege ahead";
        ShadowTextCentered(sub, cx, (int)(frame.Y + 66f), 13, WithAlpha(Color.White, 0.62f));
        DrawMedievalDivider(cx, frame.Y + 88f, 420f);

        if (activeBlessingCount > 0)
        {
            string stacked = activeBlessingCount + " blessing" + (activeBlessingCount == 1 ? "" : "s") + " already sworn";
            ShadowTextCentered(stacked, cx, (int)(frame.Y + 98f), 11, WithAlpha(MossLight, 0.75f));
        }

        const float cardW = 220f;
        const float cardH = 300f;
        const float cardGap = 18f;
        float cardsTotal = cardW * 3f + cardGap * 2f;
        float cardX = cx - cardsTotal / 2f;
        float cardY = frame.Y + 118f;

        for (int i = 0; i < blessingChoices.Length; i++)
        {
            var card = new Rectangle(cardX + i * (cardW + cardGap), cardY, cardW, cardH);
            if (DrawBlessingCard(card, blessingChoices[i], i + 1, time, false))
            {
                if (activeBlessingCount < activeBlessings.Length)
                    activeBlessings[activeBlessingCount++] = blessingChoices[i];
                blessingPickActive = false;
                state = GameState.Playing;
                AddFlash(Gold, 0.18f);
                SpawnFloatingText(playerPos + new Vector2(0, -50f), GetBlessingInfo(blessingChoices[i]).name.ToUpperInvariant(), Gold, 22);
            }
        }

        DrawUiHintBar("1 � 2 � 3 quick pick", "Click a card to claim", "Blessings stack up to 6");
    }

    static void DrawDifficultySelect()
    {
        int cx = WindowWidth / 2;
        float time = (float)Raylib.GetTime();
        DifficultyProfile chosen = DifficultyProfiles[difficultyMenuIndex];
        bool nightmare = difficultyMenuIndex == (int)Difficulty.FableNightmare;

        DrawScreenBackdrop(0.86f, nightmare ? 0.52f : 0.38f);
        if (nightmare)
        {
            float pulse = MathF.Sin(time * 2.2f) * 0.5f + 0.5f;
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(new Color(48, 4, 10, 255), 0.14f + pulse * 0.07f));
            Raylib.DrawRectangleGradientV(0, 0, WindowWidth, WindowHeight / 2,
                WithAlpha(new Color(72, 8, 16, 255), 0.12f + pulse * 0.06f), WithAlpha(new Color(72, 8, 16, 255), 0f));
        }

        DrawDifficultyHeaderPlaque(cx, time, chosen, nightmare);

        var frame = new Rectangle(20f, 142f, WindowWidth - 40f, 598f);
        DrawDifficultyScreenFrame(frame, chosen.Accent, time);

        float cardH = 76f;
        float gap = 5f;
        float listX = frame.X + 36f;
        float listY = frame.Y + 24f;
        float cardW = 396f;
        float timelineX = listX - 18f;
        float listHeight = DifficultyProfiles.Length * cardH + (DifficultyProfiles.Length - 1) * gap;
        DrawDifficultyTimeline(timelineX, listY, listHeight, cardH, gap, difficultySelectAnim, time);

        for (int i = 0; i < DifficultyProfiles.Length; i++)
        {
            DifficultyProfile profile = DifficultyProfiles[i];
            bool selected = i == difficultyMenuIndex;
            bool cardNightmare = i == (int)Difficulty.FableNightmare;
            var card = new Rectangle(listX, listY + i * (cardH + gap), cardW, cardH);
            if (DrawDifficultyCard(card, profile, selected, cardNightmare, time, i + 1))
            {
                difficultyMenuIndex = i;
                runDifficulty = (Difficulty)i;
                SaveGame();
                ResetGame();
            }
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), card))
            {
                difficultyMenuIndex = i;
                runDifficulty = (Difficulty)i;
            }
        }

        var preview = new Rectangle(frame.X + 452f, frame.Y + 24f, frame.Width - 476f, listHeight);
        if (DrawDifficultyPreviewPanel(preview, chosen, nightmare, time))
        {
            SaveGame();
            ResetGame();
        }

        if (Button(new Rectangle(30f, UiBackButtonY, 180f, 40f), "BACK  [ESC]", 20, true, UiBorder))
        {
            state = GameState.MainMenu;
        }

        DrawUiHintBar("Click a trial � ?? move", "Q E H N oaths � click chips", "ENTER begin � ESC back");
    }

    static bool DrawDifficultyCard(Rectangle card, in DifficultyProfile profile, bool selected, bool nightmare, float time, int rank)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(mouse, card);
        float pulse = MathF.Sin(time * (nightmare ? 3.4f : 2.1f) + rank) * 0.5f + 0.5f;
        float lift = selected ? 1f : hover ? 0.55f : 0f;

        Color fill = nightmare
            ? LerpColor(new Color(26, 8, 12, 255), new Color(50, 12, 20, 255), 0.35f + pulse * 0.28f)
            : LerpColor(new Color(32, 30, 28, 255), UiPanel, 0.25f + lift * 0.35f);
        Color border = selected
            ? LerpColor(profile.Accent, profile.AccentHi, 0.35f + pulse * 0.4f)
            : hover
                ? WithAlpha(profile.Accent, nightmare ? 0.58f : 0.42f)
                : WithAlpha(profile.Accent, nightmare ? 0.26f : 0.16f);

        if (selected || hover)
        {
            DrawGlow(new Vector2(card.X + card.Width / 2f, card.Y + card.Height / 2f),
                nightmare ? 110f : 72f, profile.Accent, (nightmare ? 0.06f : 0.03f) + lift * 0.03f);
        }

        DrawRichPanel(card, WithAlpha(fill, 0.94f), border, 0.2f, accentStripe: selected || nightmare);

        var rail = new Rectangle(card.X + 4f, card.Y + 6f, 5f, card.Height - 12f);
        Raylib.DrawRectangleRounded(rail, 1f, 4, WithAlpha(Darken(profile.Accent, 0.35f), 0.9f));
        Raylib.DrawRectangleRounded(new Rectangle(rail.X, rail.Y, rail.Width, rail.Height * (0.45f + lift * 0.35f)), 1f, 4,
            WithAlpha(profile.Accent, 0.85f));
        Raylib.DrawRectangle((int)(rail.X + 1f), (int)(rail.Y + 1f), (int)(rail.Width - 2f), 1, WithAlpha(profile.AccentHi, 0.45f));

        if (nightmare)
        {
            Raylib.DrawRectangleGradientH((int)card.X, (int)card.Y, (int)card.Width, (int)(card.Height * 0.35f),
                WithAlpha(new Color(88, 10, 20, 255), 0.16f + pulse * 0.08f), WithAlpha(new Color(88, 10, 20, 255), 0f));
        }

        DrawDifficultySigil(new Vector2(card.X + 46f, card.Y + card.Height / 2f), 14f, rank - 1, profile.Accent, profile.AccentHi, time, nightmare);

        int titleFs = nightmare ? 16 : 15;
        ShadowText(profile.Title, (int)(card.X + 72f), (int)(card.Y + 14f), titleFs, profile.AccentHi);
        DrawTextTruncated(profile.Tagline, (int)(card.X + 72f), (int)(card.Y + 36f), (int)(card.Width - 118f), 11,
            WithAlpha(Color.White, selected ? 0.78f : 0.55f));

        DifficultyMeterValues(profile, out float foes, out _, out _, out float chaos);
        float pipY = card.Y + card.Height - 16f;
        for (int pip = 0; pip < 5; pip++)
        {
            float t = (pip + 1) / 5f;
            bool on = pip < (int)MathF.Round(foes * 5f);
            Color pipCol = on ? profile.Accent : WithAlpha(ForestShadow, 0.75f);
            if (nightmare && on && pip >= 3)
            {
                pipCol = LerpColor(profile.Accent, profile.AccentHi, pulse);
            }

            Raylib.DrawCircleV(new Vector2(card.X + 72f + pip * 10f, pipY), 2.6f, pipCol);
        }

        if (chaos > 0.12f)
        {
            ShadowText("STACK", (int)(card.X + card.Width - 58f), (int)(card.Y + card.Height - 18f), 9,
                WithAlpha(profile.AccentHi, selected ? 0.85f : 0.55f));
        }

        ShadowText(rank.ToString(), (int)(card.X + card.Width - 24f), (int)(card.Y + 10f), 14,
            WithAlpha(profile.AccentHi, selected ? 0.9f : 0.45f));

        if (selected)
        {
            DrawPulseFrame(new Rectangle(card.X + 2f, card.Y + 2f, card.Width - 4f, card.Height - 4f),
                profile.Accent, 0.22f, nightmare ? 4.2f : 3.2f, nightmare ? 0.3f : 0.18f);
            Raylib.DrawTriangle(new Vector2(card.X + card.Width - 16f, card.Y + card.Height / 2f - 6f),
                new Vector2(card.X + card.Width - 6f, card.Y + card.Height / 2f),
                new Vector2(card.X + card.Width - 16f, card.Y + card.Height / 2f + 6f), profile.AccentHi);
        }

        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    // Perfectly centers all text and buttons, removes messy rule lines.
    static void DrawMainMenu()
    {
        int cx = WindowWidth / 2;
        float time = (float)Raylib.GetTime();

        int yStart = 118;
        int gap = 14;

        yStart += DrawMedievalMenuTitle(cx, yStart, time) + 8;

        string subtitle = "Stand the stones. Outlast the siege.";
        int fsSubtitle = 15;
        int twSubtitle = Raylib.MeasureText(subtitle, fsSubtitle);
        ShadowText(subtitle, cx - twSubtitle / 2, yStart, fsSubtitle, WithAlpha(new Color(148, 144, 138, 255), 0.62f));
        yStart += fsSubtitle + gap;

        DrawMedievalMenuDivider(cx, yStart);
        yStart += gap + 2;

        float chipW = 108f;
        float chipH = 44f;
        float chipGap = 10f;
        float chipsTotal = chipW * 3f + chipGap * 2f;
        float chipX = cx - chipsTotal / 2f;
        DrawUiStatChip(new Rectangle(chipX, yStart, chipW, chipH), "RANK", playerLevel.ToString(), UiAccent, ghost: true);
        DrawUiStatChip(new Rectangle(chipX + chipW + chipGap, yStart, chipW, chipH), "FABLES", fables.ToString("N0"), Gold, ghost: true);
        string bestVal = highScore > 0 ? highScore.ToString("N0") : "-";
        DrawUiStatChip(new Rectangle(chipX + (chipW + chipGap) * 2f, yStart, chipW, chipH), "BEST", bestVal, MossLight, ghost: true);
        yStart += (int)chipH + gap;

        var levelRect = new Rectangle(cx - 176f, yStart, 352f, 38f);
        DrawLevelBar(levelRect, true, ghost: true);
        yStart += 44;

        ComputeMainMenuButtonLayout(cx, yStart, out float playY, out float armoryY, out float settingsY, out float btnW, out float primaryH, out float secondaryH);
        bool playClick = MenuTextButton(new Rectangle(cx - btnW / 2f, playY, btnW, primaryH), "Play", "enter", true, primary: true);
        bool armoryClick = MenuTextButton(new Rectangle(cx - btnW / 2f, armoryY, btnW, secondaryH), "Armory", "c", true);
        bool settingsClick = MenuTextButton(new Rectangle(cx - btnW / 2f, settingsY, btnW, secondaryH), "Settings", "s", true);

        if (playClick)
        {
            difficultyMenuIndex = (int)runDifficulty;
            difficultySelectAnim = difficultyMenuIndex;
            uiInputBlockFrames = 2;
            state = GameState.DifficultySelect;
        }
        else if (armoryClick)
        {
            OpenArmory();
        }
        else if (settingsClick)
        {
            OpenSettings(GameState.MainMenu);
        }

        string diffLabel = "Last run: " + GetDifficultyProfile(runDifficulty).Title;
        ShadowTextCentered(diffLabel, cx, (int)(settingsY + secondaryH + 14f), 11, WithAlpha(MossLight, 0.62f));

        DrawUiHintBar("WASD move", "LMB fire � RMB reload", "F3 fps");
    }

    static int DrawMedievalMenuTitle(int cx, int y, float time)
    {
        const string title = "FABLE";
        const int fs = 82;
        MenuCastlePalette p = MenuPalette;
        int tw = Raylib.MeasureText(title, fs);
        int x = cx - tw / 2;

        Color stoneDeep = p.StoneDeep;
        Color stoneMid = p.StoneMid;
        Color stoneDark = p.StoneDark;
        Color stoneHi = p.StoneHi;
        Color moon = p.MoonGlow;
        Color mortar = p.Mortar;
        Color crimson = p.HeraldRed;
        Color crimsonDeep = p.HeraldRedDeep;

        float pulse = MathF.Sin(time * 1.15f) * 0.5f + 0.5f;
        float shimmer = MathF.Sin(time * 2.7f) * 0.5f + 0.5f;

        const float framePadX = 58f;
        const float framePadTop = 26f;
        const float framePadBot = 34f;
        var cartouche = new Rectangle(cx - tw / 2f - framePadX, y - framePadTop, tw + framePadX * 2f, fs + framePadTop + framePadBot);

        DrawEllipticalGlow(new Vector2(cx, cartouche.Y + cartouche.Height * 0.48f), cartouche.Width * 0.44f, cartouche.Height * 0.4f, 0f,
            crimsonDeep, 0.005f + pulse * 0.003f, 2);
        DrawEllipticalGlow(new Vector2(cx, cartouche.Y + cartouche.Height * 0.42f), cartouche.Width * 0.52f, cartouche.Height * 0.48f, 0f,
            moon, 0.004f + shimmer * 0.002f, 2);

        DrawRichPanel(cartouche, WithAlpha(stoneDark, 0.48f), WithAlpha(stoneHi, 0.18f + pulse * 0.05f), 0.14f, accentStripe: true);
        DrawStoneMasonry(cartouche, stoneDeep, mortar, 3, 15, 0.36f);
        DrawQuoinStripes(cartouche, stoneHi, crimsonDeep);

        int merlons = 11;
        float battW = cartouche.Width * 0.9f;
        float battX = cx - battW / 2f;
        float battY = cartouche.Y - 6f;
        float merlonW = battW / (merlons * 2 - 1);
        for (int m = 0; m < merlons; m++)
        {
            float mx = battX + m * merlonW * 2f;
            Raylib.DrawRectangle((int)mx, (int)battY, (int)merlonW, 9, WithAlpha(stoneDark, 0.82f));
            Raylib.DrawLine((int)mx, (int)battY, (int)(mx + merlonW), (int)battY, WithAlpha(moon, 0.12f + pulse * 0.05f));
            Raylib.DrawLine((int)mx, (int)(battY + 8f), (int)(mx + merlonW), (int)(battY + 8f), WithAlpha(stoneDeep, 0.45f));
        }

        for (int side = 0; side < 2; side++)
        {
            float px = side == 0 ? cartouche.X + 10f : cartouche.X + cartouche.Width - 10f;
            float wave = MathF.Sin(time * 2.5f + side * 1.8f) * 3.5f;
            Vector2 tip = new Vector2(px + (side == 0 ? -20f : 20f) + wave, cartouche.Y + 30f);
            Raylib.DrawTriangle(new Vector2(px, cartouche.Y + 8f), new Vector2(px, cartouche.Y + 34f), tip, WithAlpha(crimson, 0.58f));
            Raylib.DrawLineEx(new Vector2(px, cartouche.Y + 8f), tip, 1f, WithAlpha(stoneHi, 0.18f));
            Raylib.DrawCircleV(new Vector2(px, cartouche.Y + 4f), 2f, WithAlpha(moon, 0.22f + pulse * 0.08f));
        }

        var inner = new Rectangle(cartouche.X + 14f, cartouche.Y + 12f, cartouche.Width - 28f, cartouche.Height - 22f);
        Raylib.DrawRectangleLinesEx(inner, 1f, WithAlpha(stoneHi, 0.14f + pulse * 0.05f));
        Raylib.DrawRectangleLinesEx(new Rectangle(inner.X + 3f, inner.Y + 3f, inner.Width - 6f, inner.Height - 6f),
            1f, WithAlpha(crimsonDeep, 0.1f + pulse * 0.04f));

        ReadOnlySpan<Vector2> rivets =
        [
            new(cartouche.X + 18f, cartouche.Y + 18f),
            new(cartouche.X + cartouche.Width - 18f, cartouche.Y + 18f),
            new(cartouche.X + 18f, cartouche.Y + cartouche.Height - 14f),
            new(cartouche.X + cartouche.Width - 18f, cartouche.Y + cartouche.Height - 14f),
        ];
        for (int r = 0; r < rivets.Length; r++)
        {
            Raylib.DrawCircleV(rivets[r], 2.5f, WithAlpha(stoneMid, 0.55f));
            Raylib.DrawCircleV(rivets[r], 1.2f, WithAlpha(moon, 0.12f + pulse * 0.08f));
        }

        Raylib.DrawText(title, x + 5, y + 6, fs, WithAlpha(stoneDeep, 0.98f));
        Raylib.DrawText(title, x + 3, y + 4, fs, WithAlpha(new Color(18, 16, 14, 255), 0.94f));
        for (int ox = -3; ox <= 3; ox++)
        {
            for (int oy = -3; oy <= 3; oy++)
            {
                if (ox == 0 && oy == 0) continue;
                float dist = MathF.Sqrt(ox * ox + oy * oy);
                if (dist > 3.1f) continue;
                Raylib.DrawText(title, x + ox, y + oy, fs, WithAlpha(stoneDeep, 0.14f / Math.Max(dist, 0.8f)));
            }
        }

        Raylib.DrawText(title, x - 1, y + 1, fs, WithAlpha(crimsonDeep, 0.07f + pulse * 0.03f));
        Color face = LerpColor(stoneDark, stoneMid, 0.42f + pulse * 0.1f);
        Raylib.DrawText(title, x, y, fs, face);
        Raylib.DrawText(title, x - 1, y - 2, fs, WithAlpha(moon, 0.1f + shimmer * 0.05f));
        Raylib.DrawText(title, x, y - 1, fs, WithAlpha(stoneHi, 0.08f + pulse * 0.04f));

        float sweepX = cx - tw / 2f - 12f + (tw + 48f) * ((time * 0.16f) % 1f);
        Raylib.DrawLineEx(new Vector2(sweepX, y - 2f), new Vector2(sweepX + 22f, y + fs + 6f), 1.6f, WithAlpha(moon, 0.035f * shimmer));

        float ruleY = y + fs + 16f;
        float wing = tw * 0.55f + 58f;
        Raylib.DrawLineEx(new Vector2(cx - wing, ruleY), new Vector2(cx - 22f, ruleY), 2f, WithAlpha(stoneHi, 0.18f));
        Raylib.DrawLineEx(new Vector2(cx + 22f, ruleY), new Vector2(cx + wing, ruleY), 2f, WithAlpha(stoneHi, 0.18f));
        Raylib.DrawLineEx(new Vector2(cx - wing, ruleY + 1f), new Vector2(cx - 22f, ruleY + 1f), 1f, WithAlpha(stoneDeep, 0.45f));
        Raylib.DrawLineEx(new Vector2(cx + 22f, ruleY + 1f), new Vector2(cx + wing, ruleY + 1f), 1f, WithAlpha(stoneDeep, 0.45f));
        Raylib.DrawLineEx(new Vector2(cx - wing, ruleY - 2f), new Vector2(cx + wing, ruleY - 2f), 0.8f, WithAlpha(crimsonDeep, 0.12f));

        Vector2 gemTop = new Vector2(cx, ruleY - 9f);
        Vector2 gemLeft = new Vector2(cx - 8f, ruleY + 2f);
        Vector2 gemRight = new Vector2(cx + 8f, ruleY + 2f);
        Raylib.DrawTriangle(gemTop, gemLeft, gemRight, WithAlpha(crimsonDeep, 0.62f + pulse * 0.08f));
        Raylib.DrawLineEx(gemTop, gemLeft, 1f, WithAlpha(stoneHi, 0.22f));
        Raylib.DrawLineEx(gemTop, gemRight, 1f, WithAlpha(stoneHi, 0.22f));
        Raylib.DrawLineEx(gemLeft, gemRight, 1f, WithAlpha(stoneDeep, 0.55f));

        for (int i = 0; i < 7; i++)
        {
            float tx = cx - wing * 0.62f + i * (wing * 1.24f / 6f);
            float tickH = 4f + Hash(i * 11) * 4f;
            Raylib.DrawLineEx(new Vector2(tx, ruleY + 3f), new Vector2(tx, ruleY + 3f + tickH), 1.2f,
                WithAlpha(i % 3 == 0 ? moon : stoneHi, 0.1f + Hash(i * 9) * 0.1f));
        }

        return (int)(ruleY + 14f - y);
    }

    static void DrawMedievalMenuPlaque(int cx, float time)
    {
        var plaque = new Rectangle(cx - 268f, 110f, 536f, 132f);
        Color stoneMid = new Color(48, 46, 44, 255);
        Color stoneLight = new Color(98, 96, 92, 255);
        Color mortar = new Color(28, 26, 26, 255);
        DrawRichPanel(plaque, WithAlpha(stoneMid, 0.52f), WithAlpha(stoneLight, 0.28f), 0.12f);
        DrawStoneMasonry(plaque, stoneMid, mortar, 4, 10, 0.45f);
        DrawQuoinStripes(plaque, stoneLight, MossLight);

        float pulse = MathF.Sin(time * 1.5f) * 0.5f + 0.5f;
        Raylib.DrawRectangleLinesEx(plaque, 1f, WithAlpha(stoneLight, 0.22f));
        var innerFrame = new Rectangle(plaque.X + 10f, plaque.Y + 8f, plaque.Width - 20f, plaque.Height - 16f);
        Raylib.DrawRectangleLinesEx(innerFrame, 1f, WithAlpha(MossLight, 0.14f + pulse * 0.06f));

        // Corner flourishes
        float inset = 12f;
        Raylib.DrawLineEx(new Vector2(plaque.X + inset, plaque.Y + inset), new Vector2(plaque.X + inset + 22f, plaque.Y + inset), 1.5f, WithAlpha(MossLight, 0.4f));
        Raylib.DrawLineEx(new Vector2(plaque.X + inset, plaque.Y + inset), new Vector2(plaque.X + inset, plaque.Y + inset + 22f), 1.5f, WithAlpha(MossLight, 0.4f));
        Raylib.DrawLineEx(new Vector2(plaque.X + plaque.Width - inset, plaque.Y + inset), new Vector2(plaque.X + plaque.Width - inset - 22f, plaque.Y + inset), 1.5f, WithAlpha(MossLight, 0.4f));
        Raylib.DrawLineEx(new Vector2(plaque.X + plaque.Width - inset, plaque.Y + inset), new Vector2(plaque.X + plaque.Width - inset, plaque.Y + inset + 22f), 1.5f, WithAlpha(MossLight, 0.4f));
    }

    static void DrawMedievalMenuDivider(int cx, int y)
    {
        Color hi = new Color(88, 86, 82, 255);
        Raylib.DrawLine(cx - 120, y, cx - 24, y, WithAlpha(hi, 0.22f));
        Raylib.DrawLine(cx + 24, y, cx + 120, y, WithAlpha(hi, 0.22f));
        Raylib.DrawCircleV(new Vector2(cx, y), 2.5f, WithAlpha(MenuPalette.StoneHi, 0.35f));
    }

    static bool MenuTextButton(int cx, float y, string label, string key, bool medieval = false, bool primary = false)
    {
        int size = primary ? 26 : 22;
        string hint = "[" + key + "]";
        int labelW = Raylib.MeasureText(label, size);
        int hintW = Raylib.MeasureText(hint, 14);
        int totalW = labelW + 12 + hintW;
        int btnW = Math.Max(totalW + 40, primary ? 260 : 220);
        int btnH = primary ? 44 : 40;
        return MenuTextButton(new Rectangle(cx - btnW / 2f, y, btnW, btnH), label, key, medieval, primary);
    }

    static bool MenuTextButton(Rectangle hit, string label, string key, bool medieval = false, bool primary = false)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        int size = primary ? 26 : 22;
        string hint = "[" + key + "]";
        int labelW = Raylib.MeasureText(label, size);
        int hintW = Raylib.MeasureText(hint, 14);
        int hintGap = 12;
        int totalW = labelW + hintGap + hintW;
        float cx = hit.X + hit.Width / 2f;
        int x = (int)(cx - totalW / 2f);
        bool hover = Raylib.CheckCollisionPointRec(mouse, hit);
        bool click = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color text = hover
            ? new Color(220, 218, 212, 255)
            : medieval ? new Color(156, 152, 146, 255) : new Color(210, 220, 205, 255);
        Color hintColor = WithAlpha(primary ? Gold : Color.White, hover ? 0.65f : medieval ? 0.32f : 0.42f);
        Color panelFill = primary
            ? (hover ? LerpColor(MenuPalette.StoneDeep, UiAccent, medieval ? 0.1f : 0.18f) : WithAlpha(MenuPalette.StoneDeep, medieval ? 0.32f : 1f))
            : (hover ? WithAlpha(MenuPalette.StoneDark, medieval ? 0.34f : 0.55f) : WithAlpha(MenuPalette.StoneDeep, medieval ? 0.2f : 0.35f));
        Color panelBorder = primary
            ? (hover ? WithAlpha(MenuPalette.StoneHi, medieval ? 0.28f : 0.55f) : WithAlpha(MenuPalette.StoneHi, medieval ? 0.16f : 0.55f))
            : (hover ? WithAlpha(MenuPalette.StoneHi, medieval ? 0.2f : 0.35f) : WithAlpha(MenuPalette.StoneMid, medieval ? 0.14f : 0.25f));

        DrawRichPanel(hit, panelFill, panelBorder, 0.28f, accentStripe: primary || hover);
        if (hover && !medieval)
        {
            DrawGlow(new Vector2(cx, hit.Y + hit.Height / 2f), primary ? 64f : 48f, primary ? UiAccent : MossLight, primary ? 0.04f : 0.03f);
        }
        else if (hover && medieval)
        {
            DrawGlow(new Vector2(cx, hit.Y + hit.Height / 2f), primary ? 52f : 40f, MenuPalette.MoonGlow, primary ? 0.018f : 0.012f);
        }

        int textY = (int)(hit.Y + (hit.Height - size) / 2f);
        ShadowText(label, x, textY, size, text);
        Raylib.DrawText(hint, x + labelW + hintGap, textY + 4, 14, hintColor);

        return click;
    }

    static void DrawPauseMenu()
    {
        int cx = WindowWidth / 2;
        DrawScreenBackdrop(0.74f, 0.38f);

        if (blessingPickActive)
        {
            DrawBlessingPickerScreen();
            return;
        }

        var card = new Rectangle(cx - 270f, 190f, 540f, 440f);
        DrawRichPanel(card, UiPanel, UiBorder, 0.15f, accentStripe: true);
        ShadowTextCentered("PAUSED", cx, 222, 38, UiAccent);
        DrawMedievalDivider(cx, 268f, 300f);
        ShadowTextCentered("Run in progress - resume when ready.", cx, 282, 12, WithAlpha(Color.White, 0.58f));

        float sx = cx - 204f;
        float sy = 312f;
        const float cw = 196f;
        const float ch = 48f;
        const float cg = 16f;
        DrawUiStatChip(new Rectangle(sx, sy, cw, ch), "WAVE", waveNumber.ToString(), MossLight);
        DrawUiStatChip(new Rectangle(sx + cw + cg, sy, cw, ch), "SCORE", ((int)scoreDisplay).ToString("N0"), Color.White);
        DrawUiStatChip(new Rectangle(sx, sy + ch + 12f, cw, ch), "KILLS", runKillCount.ToString(), UiAccent);
        DrawUiStatChip(new Rectangle(sx + cw + cg, sy + ch + 12f, cw, ch), "FABLES", fables.ToString("N0"), Gold);

        float infoY = sy + ch * 2f + 28f;
        if (!IsVerdictUnlocked())
        {
            string verdictProgress = "Verdict unlock: " + runKillCount + " / " + VerdictUnlockKills + " kills";
            ShadowTextCentered(verdictProgress, cx, (int)infoY, 12, WithAlpha(AbilityAccent(AbilityType.Verdict), 0.9f));
            infoY += 20f;
        }

        if (!string.IsNullOrEmpty(GunAffixName()))
        {
            ShadowTextCentered("Affix: " + GunAffixName(), cx, (int)infoY, 12, MossLight);
            infoY += 18f;
        }

        if (verdictHaltTimer > 0f)
        {
            string halt = "Events halted: " + Math.Ceiling(verdictHaltTimer).ToString("0") + "s";
            ShadowTextCentered(halt, cx, (int)infoY, 12, Gold);
        }

        float bw = 280f;
        if (Button(new Rectangle(cx - bw / 2f, 476f, bw, 48f), "RESUME  [ESC]", 22, true, UiAccent))
        {
            state = GameState.Playing;
            autoPausedForFocus = false;
        }
        if (Button(new Rectangle(cx - bw / 2f, 532f, bw, 42f), "SETTINGS  [S]", 18, true, UiBorder))
        {
            OpenSettings(GameState.Paused);
        }
        if (Button(new Rectangle(cx - bw / 2f, 582f, bw, 42f), "MAIN MENU", 18, true, UiBorder))
        {
            SaveGame();
            state = GameState.MainMenu;
        }

        DrawUiHintBar("ESC resume", "Settings apply now", "");
    }

    static void DrawGameOverScreen()
    {
        float time = (float)Raylib.GetTime();
        int cx = WindowWidth / 2;
        DrawScreenBackdrop(0.82f, 0.42f);

        var card = new Rectangle(cx - 290f, 156f, 580f, 460f);
        DrawRichPanel(card, UiPanel, UiBorder, 0.15f, accentStripe: true);

        int titleSize = 36;
        ShadowTextCentered("THE KEEP CLAIMS YOU", cx, 186, titleSize, Danger);

        (string cause, string tip) = GetDeathCauseCopy();
        ShadowTextCentered(cause, cx, 236, 18, Color.White, 1f);
        ShadowTextCentered(tip, cx, 262, 13, WithAlpha(UiAccent, 0.9f), 1f);

        float sx = cx - 204f;
        float sy = 298f;
        const float cw = 196f;
        const float ch = 48f;
        const float cg = 16f;
        DrawUiStatChip(new Rectangle(sx, sy, cw, ch), "SCORE", ((int)scoreDisplay).ToString("N0"), Color.White);
        DrawUiStatChip(new Rectangle(sx + cw + cg, sy, cw, ch), "WAVE", waveNumber.ToString(), MossLight);
        DrawUiStatChip(new Rectangle(sx, sy + ch + 12f, cw, ch), "KILLS", runKillCount.ToString(), UiAccent);
        DrawUiStatChip(new Rectangle(sx + cw + cg, sy + ch + 12f, cw, ch), "EARNED", "+" + runFablesEarned, Gold);

        float buttonY = 452f;
        if (lastRunNewBest && score > 0)
        {
            float pop = MathF.Sin(time * 6f) * 0.5f + 0.5f;
            DrawUiContextBanner(sy + ch * 2f + 16f, "NEW PERSONAL BEST!", WithAlpha(Gold, 0.65f + pop * 0.35f));
            buttonY = 468f;
        }

        float bw = 280f;
        if (Button(new Rectangle(cx - bw / 2f, buttonY, bw, 48f), "REBIRTH  [R]", 22, true, UiAccent)) ResetGame();
        if (Button(new Rectangle(cx - bw / 2f, buttonY + 54f, bw, 42f), "MAIN MENU  [ESC]", 18, true, UiBorder)) state = GameState.MainMenu;

        DrawUiHintBar("R rebirth", "Visit Armory first", "ESC menu");
    }

}
