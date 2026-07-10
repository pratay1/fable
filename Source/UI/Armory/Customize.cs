partial class Program
{
    // ---------------------------------------------------------------- Customize

    const float ArmoryTabY = 132f;
    const float ArmoryBannerY = 226f;
    const float ArmoryContentTop = 260f;
    const float LookPreviewX = 24f;
    const float LookPreviewW = 92f;
    const float LookPreviewH = 104f;

    static float HeraldryGridStartX() => LookPreviewX + LookPreviewW + 12f;
    static float HeraldryGridWidth() => WindowWidth - HeraldryGridStartX() - 22f;

    static void ComputeHeraldryGrid(out float padX, out float gap, out int cols, out float cell, out float rowH)
    {
        padX = HeraldryGridStartX();
        gap = 3f;
        cols = 11;
        float gridW = HeraldryGridWidth();
        cell = MathF.Min(26f, (gridW - (cols - 1) * gap) / cols);
        rowH = cell;
    }

    static float ArmoryCosmeticsPinnedHeight() => LookPreviewH + 14f;
    static float ArmoryContentBottom => UiFooterTop;
    static float ArmoryViewportHeight => ArmoryContentBottom - ArmoryContentTop;

    static string TruncateText(string text, int maxWidth, int fontSize)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0) return text;
        if (Raylib.MeasureText(text, fontSize) <= maxWidth) return text;

        const string ell = "...";
        int ellW = Raylib.MeasureText(ell, fontSize);
        for (int len = text.Length - 1; len > 0; len--)
        {
            string sub = text[..len];
            if (Raylib.MeasureText(sub, fontSize) + ellW <= maxWidth) return sub + ell;
        }

        return ell;
    }

    static void DrawTextTruncated(string text, int x, int y, int maxWidth, int fontSize, Color color)
    {
        Raylib.DrawText(TruncateText(text, maxWidth, fontSize), x, y, fontSize, color);
    }

    static float DrawWrappedText(string text, int x, int y, int maxWidth, int fontSize, int lineHeight, Color color, int maxLines = 8)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0) return 0f;

        string[] words = text.Split(' ');
        var line = new System.Text.StringBuilder();
        int lines = 0;
        float cy = y;

        foreach (string word in words)
        {
            string trial = line.Length == 0 ? word : line + " " + word;
            if (Raylib.MeasureText(trial, fontSize) <= maxWidth)
            {
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
                continue;
            }

            if (line.Length > 0)
            {
                Raylib.DrawText(line.ToString(), x, (int)cy, fontSize, color);
                cy += lineHeight;
                lines++;
                line.Clear();
                if (lines >= maxLines) return cy - y;
            }

            if (Raylib.MeasureText(word, fontSize) <= maxWidth)
            {
                line.Append(word);
            }
            else
            {
                Raylib.DrawText(TruncateText(word, maxWidth, fontSize), x, (int)cy, fontSize, color);
                cy += lineHeight;
                lines++;
                if (lines >= maxLines) return cy - y;
            }
        }

        if (line.Length > 0 && lines < maxLines)
        {
            Raylib.DrawText(line.ToString(), x, (int)cy, fontSize, color);
            cy += lineHeight;
        }

        return cy - y;
    }

    static float ArmoryScrollContentY() => ArmoryContentTop;
    static float ArmoryScrollViewportH() => ArmoryContentBottom - ArmoryContentTop;

    static float MeasureBestiaryContentHeight()
    {
        const float headerH = 82f;
        const float rowH = 76f;
        const float gap = 8f;
        return headerH + EnemyCatalog.Length * (rowH + gap) + 20f;
    }

    static float MeasureGlossaryContentHeight()
    {
        float h = 48f;
        EventFamily family = EventFamily.General;
        for (int i = 1; i < FloorEventNames.Length; i++)
        {
            FloorEventType ev = (FloorEventType)i;
            EventFamily f = GetEventFamily(ev);
            if (f != family)
            {
                family = f;
                h += 40f;
            }

            if (string.IsNullOrEmpty(FloorEventNames[i])) continue;
            h += 56f;
        }

        return h + 20f;
    }

    static float ArmoryScrollMax()
    {
        float viewport = ArmoryScrollViewportH();
        switch (customizeTab)
        {
            case CustomizeTab.Cosmetics:
            {
                ComputeTrinketGrid(out _, out _, out int acols, out _, out float rowH);
                int rows = (ArmoryAccessoryCount() + acols - 1) / acols;
                float trinketH = 38f + rows * rowH;
                float trinketViewport = ArmoryViewportHeight - ArmoryCosmeticsPinnedHeight();
                return Math.Max(0f, trinketH - trinketViewport);
            }
            case CustomizeTab.Weapons:
            {
                const int cols = 3;
                const float cardH = 118f;
                const float gy = 14f;
                int gunRows = (Guns.Length + cols - 1) / cols;
                float contentH = 36f + gunRows * (cardH + gy);
                return Math.Max(0f, contentH - viewport);
            }
            case CustomizeTab.Upgrades:
            {
                const float rowH = 72f;
                const float gap = 10f;
                float contentH = 36f + UpgradeCount * (rowH + gap);
                return Math.Max(0f, contentH - viewport);
            }
            case CustomizeTab.Abilities:
            {
                const float cardH = 196f;
                const float gap = 14f;
                float contentH = 132f + AbilityCount * (cardH + gap) + 24f;
                return Math.Max(0f, contentH - viewport);
            }
            case CustomizeTab.Rank:
                return Math.Max(0f, MeasureRankTabContentHeight() - viewport);
            case CustomizeTab.Bestiary:
                return Math.Max(0f, MeasureBestiaryContentHeight() - viewport);
            case CustomizeTab.Glossary:
                return Math.Max(0f, MeasureGlossaryContentHeight() - viewport);
            default:
                return 0f;
        }
    }

    static void DrawArmoryScrollBar(float viewportH)
    {
        float maxScroll = ArmoryScrollMax();
        if (maxScroll <= 1f) return;

        float trackX = WindowWidth - 18f;
        float trackY = ArmoryScrollContentY() + 6f;
        float trackH = viewportH - 12f;
        float thumbH = Math.Max(36f, trackH * (viewportH / (maxScroll + viewportH)));
        float thumbY = trackY + (trackH - thumbH) * (customizeScroll / maxScroll);
        var track = new Rectangle(trackX, trackY, 5f, trackH);
        var thumb = new Rectangle(trackX - 1f, thumbY, 7f, thumbH);
        Raylib.DrawRectangleRounded(track, 1f, 4, WithAlpha(UiPanelDeep, 0.65f));
        Raylib.DrawRectangleRounded(thumb, 1f, 4, WithAlpha(UiAccent, 0.55f + MathF.Sin((float)Raylib.GetTime() * 2f) * 0.08f));
    }

    static Color EventFamilyAccent(EventFamily family) => family switch
    {
        EventFamily.Tide => new Color(88, 156, 214, 255),
        EventFamily.Ember => new Color(214, 128, 72, 255),
        EventFamily.Crypt => new Color(148, 124, 196, 255),
        EventFamily.Crown => Gold,
        _ => MossLight,
    };

    static string EnemyBehaviorLabel(EnemyBehavior behavior) => behavior switch
    {
        EnemyBehavior.Chase => "Chaser",
        EnemyBehavior.FastChase => "Swift",
        EnemyBehavior.CrushTiles => "Crusher",
        EnemyBehavior.Orbit => "Orbiter",
        EnemyBehavior.Zigzag => "Zigzag",
        EnemyBehavior.Hop => "Leaper",
        EnemyBehavior.BlightTrail => "Blight trail",
        EnemyBehavior.TileLeech => "Tile leech",
        EnemyBehavior.Kite => "Kiter",
        EnemyBehavior.Charge => "Charger",
        EnemyBehavior.PulseBlight => "Pulse blight",
        EnemyBehavior.Phaser => "Phaser",
        EnemyBehavior.Rotburst => "Rotburst",
        EnemyBehavior.Splitter => "Splitter",
        EnemyBehavior.Sapper => "Sapper",
        EnemyBehavior.Lurker => "Lurker",
        EnemyBehavior.BossBlight => "Boss � Blight",
        EnemyBehavior.BossDash => "Boss � Dash",
        EnemyBehavior.BossSmash => "Boss � Smash",
        _ => behavior.ToString(),
    };

    static void DrawBestiaryPortrait(Vector2 center, float scale, ref readonly EnemyDef def, bool known, float time)
    {
        if (!known)
        {
            Raylib.DrawCircleV(center, 18f * scale, WithAlpha(UiPanelDeep, 0.9f));
            Raylib.DrawCircleLinesV(center, 18f * scale, WithAlpha(UiBorder, 0.45f));
            ShadowText("?", (int)center.X - 4, (int)center.Y - 10, 18, WithAlpha(Color.White, 0.28f));
            return;
        }

        float r = def.Radius * scale;
        float rot = time * def.RotSpeed * 0.14f;
        if (def.Boss) DrawGlow(center, r * 1.8f, Danger, 0.07f);
        else DrawGlow(center, r * 1.4f, def.Color, 0.06f);
        Raylib.DrawPoly(center, def.Sides, r, rot, def.Color);
        Raylib.DrawPolyLinesEx(center, def.Sides, r, rot, 2f, WithAlpha(Lighten(def.Color, 0.35f), 0.9f));
        Raylib.DrawPoly(center, def.Sides, r * 0.42f, -rot, WithAlpha(MossLight, 0.48f));
    }

    static void DrawArmoryHeader(int cx)
    {
        var header = new Rectangle(18f, 12f, WindowWidth - 36f, 66f);
        DrawRichPanel(header, WithAlpha(UiPanel, 0.94f), UiBorder, 0.22f, accentStripe: true);
        ShadowText("ARMORY", 30, 20, 28, UiAccent);
        DrawTextTruncated("Heraldry, arms, perks, and siege lore.", 30, 50, (int)header.Width / 2, 12, WithAlpha(Color.White, 0.58f));

        float chipW = 108f;
        float chipH = 40f;
        float chipY = header.Y + 14f;
        float chipX = header.X + header.Width - chipW * 2f - 18f;
        DrawUiStatChip(new Rectangle(chipX, chipY, chipW, chipH), "RANK", playerLevel.ToString(), UiAccent);
        DrawUiStatChip(new Rectangle(chipX + chipW + 10f, chipY, chipW, chipH), "FABLES", fables.ToString("N0"), Gold);

        DrawLevelBar(new Rectangle(header.X + 10f, header.Y + header.Height + 8f, header.Width - 20f, 40f), true);
    }

    static bool DrawArmoryTab(Rectangle r, string label, string key, bool active, bool lore = false)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
        Color accent = lore ? MossLight : UiAccent;

        Color fill = active
            ? LerpColor(UiPanel, accent, lore ? 0.22f : 0.3f)
            : hover ? WithAlpha(accent, 0.12f) : WithAlpha(UiPanel, 0.88f);
        DrawRichPanel(r, fill, active ? accent : WithAlpha(UiBorder, hover ? 0.55f : 0.38f), 0.26f, accentStripe: active);

        if (active)
        {
            DrawPulseFrame(r, accent, 0.24f, lore ? 3.2f : 4f, 0.22f);
            Raylib.DrawRectangle((int)r.X + 5, (int)(r.Y + r.Height - 3), (int)r.Width - 10, 3, accent);
        }

        int labelW = Raylib.MeasureText(label, 15);
        ShadowText(label, (int)(r.X + r.Width / 2 - labelW / 2), (int)(r.Y + 9), 15, active ? Color.White : new Color(220, 230, 220, 255), active ? 1f : 0.78f);
        string hint = "[" + key + "]";
        int hintW = Raylib.MeasureText(hint, 10);
        Raylib.DrawText(hint, (int)(r.X + r.Width / 2 - hintW / 2), (int)(r.Y + r.Height - 16), 10, WithAlpha(accent, active ? 0.75f : 0.42f));
        return clicked;
    }

    static void DrawArmoryTabBar(int cx)
    {
        const float tw = 104f;
        const float th = 40f;
        const float gap = 6f;
        float row1W = tw * 5 + gap * 4;
        float startX = cx - row1W / 2f;
        float y1 = ArmoryTabY;

        if (DrawArmoryTab(new Rectangle(startX, y1, tw, th), "LOOK", "1", customizeTab == CustomizeTab.Cosmetics))
        { customizeTab = CustomizeTab.Cosmetics; customizeScroll = 0f; }
        if (DrawArmoryTab(new Rectangle(startX + (tw + gap), y1, tw, th), "ARMS", "2", customizeTab == CustomizeTab.Weapons))
        { customizeTab = CustomizeTab.Weapons; customizeScroll = 0f; }
        if (DrawArmoryTab(new Rectangle(startX + (tw + gap) * 2, y1, tw, th), "PERKS", "3", customizeTab == CustomizeTab.Upgrades))
        { customizeTab = CustomizeTab.Upgrades; customizeScroll = 0f; }
        if (DrawArmoryTab(new Rectangle(startX + (tw + gap) * 3, y1, tw, th), "SKILLS", "4", customizeTab == CustomizeTab.Abilities))
        { customizeTab = CustomizeTab.Abilities; customizeScroll = 0f; }
        if (DrawArmoryTab(new Rectangle(startX + (tw + gap) * 4, y1, tw, th), "RANK", "5", customizeTab == CustomizeTab.Rank))
        { customizeTab = CustomizeTab.Rank; customizeScroll = 0f; }

        float row2W = tw * 2 + gap;
        float row2X = cx - row2W / 2f;
        float y2 = y1 + th + 8f;
        if (DrawArmoryTab(new Rectangle(row2X, y2, tw, th), "BESTIARY", "6", customizeTab == CustomizeTab.Bestiary, lore: true))
        { customizeTab = CustomizeTab.Bestiary; customizeScroll = 0f; }
        if (DrawArmoryTab(new Rectangle(row2X + tw + gap, y2, tw, th), "GLOSSARY", "7", customizeTab == CustomizeTab.Glossary, lore: true))
        { customizeTab = CustomizeTab.Glossary; customizeScroll = 0f; }
    }

    static void DrawArmoryContextStrip()
    {
        var strip = new Rectangle(18f, ArmoryBannerY, WindowWidth - 36f, 30f);
        DrawRichPanel(strip, WithAlpha(UiPanelDeep, 0.88f), WithAlpha(UiAccent, 0.35f), 0.3f);
        ShadowText(CustomizeTabTitle().ToUpperInvariant(), (int)strip.X + 12, (int)strip.Y + 4, 13, UiAccent, 0.92f);
        DrawTextTruncated(CustomizeTabSubtitle(), (int)strip.X + 12, (int)strip.Y + 16, (int)strip.Width - 24, 10, WithAlpha(Color.White, 0.55f));
    }

    static float DrawBestiarySummary(float y)
    {
        int known = 0;
        int totalKills = 0;
        for (int i = 0; i < EnemyCatalog.Length; i++)
        {
            if (bestiaryKills[i] > 0) known++;
            totalKills += bestiaryKills[i];
        }

        var bar = new Rectangle(24f, y, WindowWidth - 48f, 48f);
        DrawRichPanel(bar, WithAlpha(UiPanel, 0.9f), UiBorder, 0.18f, accentStripe: true);
        float chipW = (bar.Width - 32f) / 3f;
        DrawUiStatChip(new Rectangle(bar.X + 8f, bar.Y + 6f, chipW, 36f), "DISCOVERED", known + " / " + EnemyCatalog.Length, UiAccent);
        DrawUiStatChip(new Rectangle(bar.X + 14f + chipW, bar.Y + 6f, chipW, 36f), "TOTAL SLAIN", totalKills.ToString("N0"), MossLight);
        DrawUiStatChip(new Rectangle(bar.X + 20f + chipW * 2f, bar.Y + 6f, chipW, 36f), "BOSSES", CountKnownBosses().ToString(), Danger);
        return bar.Height + 10f;
    }

    static int CountKnownBosses()
    {
        int n = 0;
        for (int i = 0; i < EnemyCatalog.Length; i++)
        {
            if (EnemyCatalog[i].Boss && bestiaryKills[i] > 0) n++;
        }

        return n;
    }

    static float DrawBestiaryEntry(float y, int index, float time)
    {
        const float rowH = 76f;
        ref readonly EnemyDef def = ref EnemyCatalog[index];
        int kills = bestiaryKills[index];
        bool known = kills > 0;
        Color accent = def.Boss ? Danger : known ? UiAccent : UiBorder;
        var row = new Rectangle(24f, y, WindowWidth - 48f, rowH);
        DrawRichPanel(row, WithAlpha(UiPanel, known ? 0.92f : 0.62f), accent, 0.16f, accentStripe: known && def.Boss);

        var iconFrame = new Rectangle(row.X + 10f, row.Y + 10f, 56f, 56f);
        DrawRichPanel(iconFrame, WithAlpha(UiPanelDeep, 0.82f), WithAlpha(accent, 0.4f), 0.2f);
        DrawBestiaryPortrait(new Vector2(iconFrame.X + iconFrame.Width / 2f, iconFrame.Y + iconFrame.Height / 2f + 2f), 0.95f, in def, known, time);

        float textX = iconFrame.X + iconFrame.Width + 12f;
        int textW = (int)(row.Width - (textX - row.X) - 88f);
        string title = known ? def.Name : "Unknown Foe";
        ShadowText(title, (int)textX, (int)row.Y + 12, 16, known ? Color.White : WithAlpha(Color.White, 0.42f));

        if (def.Boss)
        {
            var pill = new Rectangle(textX, row.Y + 34f, 52f, 16f);
            DrawRichPanel(pill, WithAlpha(Danger, 0.18f), WithAlpha(Danger, 0.55f), 0.35f);
            Raylib.DrawText("BOSS", (int)pill.X + 10, (int)pill.Y + 3, 9, Danger);
        }
        else if (known)
        {
            string tag = EnemyBehaviorLabel(def.Behavior);
            Raylib.DrawText(tag, (int)textX, (int)row.Y + 34, 10, WithAlpha(MossLight, 0.72f));
        }

        if (known)
        {
            DrawTextTruncated(def.Desc, (int)textX, (int)row.Y + 50, textW, 10, WithAlpha(Color.White, 0.52f));
            var killChip = new Rectangle(row.X + row.Width - 78f, row.Y + 24f, 66f, 28f);
            DrawRichPanel(killChip, WithAlpha(UiPanelDeep, 0.85f), UiAccent, 0.28f);
            ShadowText(kills.ToString("N0"), (int)killChip.X + 8, (int)killChip.Y + 4, 14, UiAccent);
            Raylib.DrawText("slain", (int)killChip.X + 8, (int)killChip.Y + 18, 9, WithAlpha(Color.White, 0.45f));
        }
        else
        {
            Raylib.DrawText("Slay this foe to reveal its lore.", (int)textX, (int)row.Y + 50, 10, WithAlpha(Color.White, 0.32f));
        }

        return rowH + 8f;
    }

    static float DrawGlossaryFamilyHeader(float y, EventFamily family)
    {
        Color accent = EventFamilyAccent(family);
        var header = new Rectangle(24f, y, WindowWidth - 48f, 32f);
        DrawRichPanel(header, WithAlpha(UiPanelDeep, 0.9f), WithAlpha(accent, 0.5f), 0.24f, accentStripe: true);
        Raylib.DrawRectangle((int)header.X, (int)header.Y + 6, 4, (int)header.Height - 12, accent);
        ShadowText(family.ToString().ToUpperInvariant(), (int)header.X + 14, (int)header.Y + 8, 14, accent);
        return header.Height + 8f;
    }

    static float DrawGlossaryEntry(float y, FloorEventType ev, string name, string tip, Color accent)
    {
        const float rowH = 48f;
        var row = new Rectangle(32f, y, WindowWidth - 64f, rowH);
        DrawRichPanel(row, WithAlpha(UiPanel, 0.88f), WithAlpha(accent, 0.28f), 0.14f);
        Raylib.DrawRectangle((int)row.X, (int)row.Y + 8, 3, (int)row.Height - 16, WithAlpha(accent, 0.65f));
        ShadowText(name, (int)row.X + 12, (int)row.Y + 8, 13, Color.White, 0.9f);
        DrawTextTruncated(tip, (int)row.X + 12, (int)row.Y + 26, (int)row.Width - 20, 10, WithAlpha(MossLight, 0.72f));
        return rowH + 8f;
    }

    static void DrawCustomize()
    {
        int cx = WindowWidth / 2;
        DrawScreenBackdrop(0.72f, 0.34f);

        DrawArmoryHeader(cx);
        DrawArmoryTabBar(cx);
        DrawArmoryContextStrip();

        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());
        float viewportH = ArmoryScrollViewportH();
        var clip = new Rectangle(18f, ArmoryScrollContentY(), WindowWidth - 36f, viewportH);
        Raylib.BeginScissorMode((int)clip.X, (int)clip.Y, (int)clip.Width, (int)clip.Height);

        switch (customizeTab)
        {
            case CustomizeTab.Cosmetics: DrawCosmeticsTab(); break;
            case CustomizeTab.Weapons: DrawWeaponsTab(); break;
            case CustomizeTab.Upgrades: DrawUpgradesTab(); break;
            case CustomizeTab.Abilities: DrawAbilitiesTab(); break;
            case CustomizeTab.Rank: DrawRankTab(); break;
            case CustomizeTab.Bestiary: DrawBestiaryTab(); break;
            case CustomizeTab.Glossary: DrawGlossaryTab(); break;
            default: throw new UnreachableException();
        }

        Raylib.EndScissorMode();
        DrawArmoryScrollBar(viewportH);

        string scrollHint = ArmoryScrollMax() > 1f ? "Scroll wheel � more below" : "Click to equip or unlock";
        DrawUiHintBar(scrollHint, CustomizeTabTitle(), "ESC back");

        if (Button(new Rectangle(30f, UiBackButtonY, 180f, 40f), "BACK  [ESC]", 20, true, UiBorder))
        {
            SaveGame();
            state = GameState.MainMenu;
        }
    }

    static void DrawBestiaryTab()
    {
        float time = (float)Raylib.GetTime();
        float y = ArmoryScrollContentY() - customizeScroll;
        DrawUiSectionLabel("BESTIARY", 28f, y, UiAccent);
        y += 24f;
        y += DrawBestiarySummary(y);
        for (int i = 0; i < EnemyCatalog.Length; i++)
        {
            y += DrawBestiaryEntry(y, i, time);
        }
    }

    static void DrawGlossaryTab()
    {
        float y = ArmoryScrollContentY() - customizeScroll;
        DrawUiSectionLabel("EVENT GLOSSARY", 28f, y, MossLight);
        y += 28f;
        EventFamily family = EventFamily.General;
        for (int i = 1; i < FloorEventNames.Length; i++)
        {
            FloorEventType ev = (FloorEventType)i;
            EventFamily f = GetEventFamily(ev);
            if (f != family)
            {
                family = f;
                y += DrawGlossaryFamilyHeader(y, family);
            }

            string name = FloorEventNames[i];
            if (string.IsNullOrEmpty(name)) continue;
            y += DrawGlossaryEntry(y, ev, name, GetEventGlossaryTip(ev), EventFamilyAccent(family));
        }
    }

    static void DrawCosmeticsTab()
    {
        float time = (float)Raylib.GetTime();
        float y = ArmoryContentTop;
        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());
        float pinnedHeaderH = ArmoryCosmeticsPinnedHeight();

        var preview = new Rectangle(LookPreviewX, y, LookPreviewW, LookPreviewH);
        DrawRichPanel(preview, WithAlpha(UiPanel, 0.94f), UiAccent, 0.24f, accentStripe: true);
        DrawGlow(new Vector2(preview.X + preview.Width / 2f, preview.Y + preview.Height / 2f), 40f, BodyColor(), 0.16f);
        Vector2 pv = new Vector2(preview.X + preview.Width / 2f, preview.Y + preview.Height / 2f + 4f);
        Raylib.DrawCircleV(pv, 26f, BodyColor());
        Raylib.DrawCircleLinesV(pv, 26f, WithAlpha(BodyBright(), 0.95f));
        Vector2 iris = pv + new Vector2(0, 1) * 8f;
        Raylib.DrawCircleV(iris, 9f, BodyBright());
        Raylib.DrawCircleV(iris, 4f, Color.White);
        DrawAccessory(pv, 26f, time, accessoryIndex, AccessoryPreviewForward);
        ShadowText("PREVIEW", (int)preview.X + 8, (int)preview.Y + 6, 9, WithAlpha(Color.White, 0.5f));
        DrawTextTruncated(BodyNames[bodyColorIndex], (int)preview.X + 8, (int)(preview.Y + preview.Height - 28), (int)preview.Width - 14, 10, UiAccent);
        DrawTextTruncated(AccessoryNames[accessoryIndex], (int)preview.X + 8, (int)(preview.Y + preview.Height - 14), (int)preview.Width - 14, 9, WithAlpha(Gold, 0.9f));

        ComputeHeraldryGrid(out float heraldryPadX, out float hgap, out int hcols, out float hcell, out float hrowH);
        float heraldryY = y + 4f;
        Raylib.DrawText("COLORS", (int)heraldryPadX, (int)heraldryY, 10, WithAlpha(Color.White, 0.42f));
        float gridY = heraldryY + 14f;
        int heraldryHover = -1;
        for (int i = 0; i < BodyPalette.Length; i++)
        {
            int col = i % hcols;
            int row = i / hcols;
            var r = new Rectangle(heraldryPadX + col * (hcell + hgap), gridY + row * (hrowH + hgap), hcell, hcell);
            bool unlocked = bodyUnlocked[i];
            bool sel = bodyColorIndex == i;
            string lockText = CosmeticLockLabel(BodyFableCost[i], BodyLevelReq[i], BodyWaveReq[i]);
            bool ready = CanPurchaseBody(i);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r)) heraldryHover = i;
            if (DrawCompactHeraldrySwatch(r, BodyPalette[i], sel, unlocked, lockText, ready))
            {
                if (runLockedBodyIndex >= 0 && state == GameState.Playing) { }
                else if (unlocked) { bodyColorIndex = i; SaveGame(); }
                else if (TryUnlockBody(i)) { bodyColorIndex = i; SaveGame(); }
            }
        }

        if (heraldryHover >= 0)
        {
            string colorLabel = BodyNames[heraldryHover];
            Raylib.DrawText(colorLabel, (int)(heraldryPadX + 52f), (int)heraldryY, 10, WithAlpha(Color.White, 0.72f));
        }
        else
        {
            Raylib.DrawText(BodyNames[bodyColorIndex], (int)(heraldryPadX + 52f), (int)heraldryY, 10, WithAlpha(UiAccent, 0.85f));
        }

        float dividerY = y + pinnedHeaderH - 2f;
        Raylib.DrawLineEx(new Vector2(18f, dividerY), new Vector2(WindowWidth - 18f, dividerY), 1f, WithAlpha(UiBorder, 0.45f));

        float trinketClipTop = y + pinnedHeaderH;
        float trinketClipH = Math.Max(0f, ArmoryContentBottom - trinketClipTop);
        var trinketClip = new Rectangle(14f, trinketClipTop, WindowWidth - 28f, trinketClipH);
        var trinketBg = new Rectangle(trinketClip.X, trinketClip.Y, trinketClip.Width, trinketClip.Height);
        DrawRichPanel(trinketBg, WithAlpha(UiPanelDeep, 0.42f), WithAlpha(UiBorder, 0.28f), 0.2f);
        Raylib.BeginScissorMode((int)trinketClip.X, (int)trinketClip.Y, (int)trinketClip.Width, (int)trinketClip.Height);

        ComputeTrinketGrid(out float padX, out float tgap, out int acols, out float cell, out float rowH);
        float gridW = WindowWidth - padX * 2f;

        float trinketY = trinketClipTop + 8f - customizeScroll;
        DrawUiSectionLabel("TRINKETS", padX, trinketY, MossLight);
        Raylib.DrawText("Tap to equip or unlock", (int)(padX + 92f), (int)(trinketY + 1f), 10, WithAlpha(Color.White, 0.38f));
        Raylib.DrawLine((int)padX, (int)(trinketY + 22f), (int)(padX + gridW), (int)(trinketY + 22f), WithAlpha(MossLight, 0.22f));

        float gridStartY = trinketY + 30f;
        int display = 0;
        for (int i = 0; i < AccessoryNames.Length; i++)
        {
            if (IsSecretAccessory(i) && !accessoryUnlocked[i]) continue;

            int col = display % acols;
            int row = display / acols;
            display++;
            float tx = padX + col * (cell + tgap);
            float ty = gridStartY + row * rowH;
            var r = new Rectangle(tx, ty, cell, cell);

            bool unlocked = accessoryUnlocked[i];
            bool sel = accessoryIndex == i;
            string lockText = CosmeticLockLabel(AccessoryFableCost[i], AccessoryLevelReq[i], AccessoryWaveReq[i]);
            bool ready = CanPurchaseAccessory(i);
            bool trinketVisible = r.Y + r.Height + 20f >= trinketClipTop && r.Y <= trinketClipTop + trinketClipH;
            if (DrawTrinketTile(r, time, i, sel, unlocked, lockText, ready) && trinketVisible)
            {
                if (unlocked) { accessoryIndex = i; SaveGame(); }
                else if (TryUnlockAccessory(i)) { accessoryIndex = i; SaveGame(); }
            }

            DrawTextTruncated(AccessoryNames[i], (int)tx, (int)(ty + cell + 5f), (int)cell, 11,
                WithAlpha(Color.White, unlocked ? 0.88f : 0.48f));
            if (i == AccessoryKeepBanner && accessoryIndex == AccessoryKeepBanner && upgradeLevels[UpLockstep] > 0)
                ShadowText("SYNERGY", (int)tx, (int)(ty - 1), 8, Gold);
            if (i == AccessoryStormGlass && accessoryIndex == AccessoryStormGlass && upgradeLevels[UpPierce] > 0)
                ShadowText("SYNERGY", (int)tx, (int)(ty - 1), 8, Gold);
        }

        Raylib.EndScissorMode();
    }

    static bool DrawCompactHeraldrySwatch(Rectangle r, Color color, bool selected, bool unlocked, string lockText, bool readyToBuy)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color border = selected ? Gold : hover ? UiAccent : WithAlpha(UiBorder, 0.5f);
        Color fill = unlocked ? color : Darken(color, 0.58f);
        Raylib.DrawRectangleRounded(r, 0.32f, 4, fill);
        Raylib.DrawRectangleRoundedLines(r, 0.32f, 4, border);

        if (!unlocked)
        {
            Raylib.DrawRectangleRounded(r, 0.32f, 4, WithAlpha(ForestShadow, 0.52f));
            if (readyToBuy)
            {
                DrawPulseFrame(new Rectangle(r.X, r.Y, r.Width, r.Height), Gold, 0.18f, 5f, 0.14f);
            }
            else
            {
                Raylib.DrawCircleV(new Vector2(r.X + r.Width / 2f, r.Y + r.Height / 2f), r.Width * 0.12f, WithAlpha(Color.White, 0.55f));
            }
        }
        else if (selected)
        {
            Raylib.DrawRectangle((int)(r.X + 2f), (int)(r.Y + r.Height - 3f), (int)r.Width - 4, 2, Color.White);
        }

        if (unlocked) DrawHeraldryHatch(r, color, (int)(r.X + r.Y));
        if (hover)
        {
            DrawGlow(new Vector2(r.X + r.Width / 2f, r.Y + r.Height / 2f), r.Width * 0.9f, selected ? Gold : UiAccent, 0.035f);
        }

        return clicked;
    }

    static bool UiClickAllowed() => uiInputBlockFrames <= 0;

    static void OpenArmory()
    {
        customizeTab = CustomizeTab.Cosmetics;
        customizeScroll = 0f;
        uiInputBlockFrames = 3;
        state = GameState.Customize;
    }

    static void ComputeMainMenuButtonLayout(int cx, int yStart, out float playY, out float armoryY, out float settingsY, out float btnW, out float primaryH, out float secondaryH)
    {
        const float menuButtonDrop = 22f;
        const float btnGap = 10f;
        btnW = 276f;
        primaryH = 46f;
        secondaryH = 40f;
        playY = yStart + 4f + menuButtonDrop;
        armoryY = playY + primaryH + btnGap;
        settingsY = armoryY + secondaryH + btnGap;
    }

    static void ComputeTrinketGrid(out float padX, out float gap, out int cols, out float cell, out float rowH)
    {
        padX = 18f;
        gap = 12f;
        const float labelGap = 20f;
        float gridW = WindowWidth - padX * 2f;
        const float minCell = 96f;
        cols = Math.Clamp((int)MathF.Floor((gridW + gap) / (minCell + gap)), 4, 7);
        cell = (gridW - (cols - 1) * gap) / cols;
        rowH = cell + labelGap;
    }

    static void DrawLockBadge(Vector2 center, float size, Color color)
    {
        float sh = size * 0.52f;
        float sw = size * 0.72f;
        var shackle = new Rectangle(center.X - sw * 0.5f, center.Y - size * 0.42f, sw, sh);
        Raylib.DrawRectangleRounded(shackle, 0.55f, 4, WithAlpha(color, 0.55f));
        Raylib.DrawRectangleRoundedLines(shackle, 0.55f, 4, WithAlpha(color, 0.85f));
        var body = new Rectangle(center.X - sw * 0.62f, center.Y - size * 0.06f, sw * 1.24f, size * 0.62f);
        Raylib.DrawRectangleRounded(body, 0.28f, 4, WithAlpha(color, 0.9f));
        Raylib.DrawRectangleRoundedLines(body, 0.28f, 4, WithAlpha(Lighten(color, 0.25f), 0.95f));
        Raylib.DrawCircleV(new Vector2(center.X, center.Y + size * 0.14f), size * 0.08f, WithAlpha(ForestShadow, 0.75f));
    }

    static bool DrawTrinketTile(Rectangle r, float time, int idx, bool selected, bool unlocked, string lockText, bool readyToBuy)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);
        bool clicked = UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color border = selected ? UiBorderLight : hover ? UiAccent : UiBorder;
        DrawRichPanel(r, WithAlpha(UiPanel, 0.97f), border, 0.26f, accentStripe: selected || readyToBuy);

        var iconArea = new Rectangle(r.X + 8f, r.Y + 8f, r.Width - 16f, r.Height - 16f);
        DrawRichPanel(iconArea, WithAlpha(UiPanelDeep, 0.78f), WithAlpha(UiBorder, 0.5f), 0.18f);

        float iconR = MathF.Min(iconArea.Width, iconArea.Height) * 0.34f;
        Vector2 iconCenter = new Vector2(iconArea.X + iconArea.Width / 2f, iconArea.Y + iconArea.Height * 0.46f);
        DrawAccessory(iconCenter, iconR, time, idx, AccessoryPreviewForward);

        if (!unlocked)
        {
            Raylib.DrawRectangleRounded(iconArea, 0.18f, 4, WithAlpha(ForestShadow, 0.48f));
            Color lockCol = readyToBuy ? Gold : WithAlpha(Color.White, 0.72f);
            DrawLockBadge(new Vector2(iconCenter.X, iconCenter.Y - iconR * 0.08f), iconR * 0.95f, lockCol);

            if (!string.IsNullOrEmpty(lockText))
            {
                int fs = lockText.Length > 12 ? 8 : lockText.Length > 9 ? 9 : 10;
                string fit = TruncateText(lockText, (int)iconArea.Width - 8, fs);
                int tw = Raylib.MeasureText(fit, fs);
                ShadowText(fit,
                    (int)(iconArea.X + iconArea.Width / 2f - tw / 2f),
                    (int)(iconArea.Y + iconArea.Height - fs - 6f),
                    fs, readyToBuy ? Gold : WithAlpha(Color.White, 0.58f));
            }

            if (readyToBuy)
            {
                DrawPulseFrame(new Rectangle(r.X + 2f, r.Y + 2f, r.Width - 4f, r.Height - 4f), Gold, 0.22f, 5f, 0.2f);
            }
        }
        else if (selected)
        {
            ShadowText("\u2713", (int)(r.X + r.Width - 18f), (int)(r.Y + 6f), 16, UiAccent);
            DrawGlow(iconCenter, iconR * 1.6f, UiAccent, 0.045f);
        }

        if (hover && unlocked)
        {
            DrawGlow(iconCenter, iconR * 1.4f, UiAccent, 0.03f);
        }

        return clicked;
    }

    static void DrawWeaponsTab()
    {
        const int cols = 3;
        const float cardW = 236f;
        const float cardH = 118f;
        const float gx = 12f;
        const float gy = 14f;
        float totalW = cols * cardW + (cols - 1) * gx;
        float startX = WindowWidth / 2f - totalW / 2f;
        float startY = ArmoryScrollContentY() - customizeScroll;
        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());
        float dt = Raylib.GetFrameTime();
        float time = (float)Raylib.GetTime();
        int textW = (int)cardW - 24;

        DrawUiSectionLabel("SIEGE ARMS", startX, startY, UiAccent);
        startY += 28f;

        for (int i = 0; i < Guns.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float y = startY + row * (cardH + gy);
            var r = new Rectangle(startX + col * (cardW + gx), y, cardW, cardH);

            bool unlocked = gunUnlocked[i];
            bool equipped = equippedGun == i;
            Vector2 m = Raylib.GetMousePosition();
            bool hover = Raylib.CheckCollisionPointRec(m, r);

            Color border = equipped ? UiAccent : unlocked ? UiBorder : Darken(UiBorder, 0.4f);
            Color fill = hover ? WithAlpha(Guns[i].Color, 0.12f) : WithAlpha(UiPanel, 0.92f);
            DrawRichPanel(r, fill, border, 0.18f, accentStripe: equipped);

            DrawGunIcon(i, new Vector2(r.X + 20f, r.Y + 22f), 11f, time, unlocked ? 1f : 0.45f);
            DrawTextTruncated(Guns[i].Name, (int)r.X + 36, (int)r.Y + 8, textW - 24, 14, unlocked ? Color.White : WithAlpha(Color.White, 0.5f));

            string stats = $"DMG {Guns[i].Damage:0}  x{Guns[i].Count}  P{Guns[i].Pierce}" + (Guns[i].Homing > 0 ? "  SEEK" : "");
            DrawTextTruncated(stats, (int)r.X + 12, (int)r.Y + 30, textW, 10, WithAlpha(UiAccent, 0.9f));
            DrawTextTruncated(Guns[i].Desc, (int)r.X + 12, (int)r.Y + 46, textW, 10, WithAlpha(Color.White, 0.5f));

            if (equipped)
            {
                Raylib.DrawText("EQUIPPED", (int)r.X + 12, (int)r.Y + 84, 11, UiAccent);
            }
            else if (unlocked)
            {
                Raylib.DrawText("click to equip", (int)r.X + 12, (int)r.Y + 84, 10, WithAlpha(Color.White, 0.5f));
            }
            else
            {
                string lockLabel = GunLockLabel(i);
                bool ready = CanPurchaseGun(i);
                bool fableGun = Guns[i].FableCost > 0;
                if (fableGun)
                {
                    float target = Math.Clamp(fables / (float)Math.Max(1, Guns[i].FableCost), 0f, 1f);
                    shopBarVis[i] = Approach(shopBarVis[i], target, 12f, dt);
                    var barBg = new Rectangle(r.X + 12f, r.Y + 78f, cardW - 24f, 5f);
                    Raylib.DrawRectangleRounded(barBg, 0.8f, 4, WithAlpha(UiPanelDeep, 0.9f));
                    var barFill = new Rectangle(barBg.X, barBg.Y, MathF.Max(0f, barBg.Width * shopBarVis[i]), barBg.Height);
                    Raylib.DrawRectangleRounded(barFill, 0.8f, 4, ready ? Gold : LerpColor(Danger, Gold, shopBarVis[i]));
                }

                DrawTextTruncated(lockLabel, (int)r.X + 12, (int)r.Y + 86, textW, 10, ready ? Gold : WithAlpha(Color.White, 0.45f));
                if (ready && fableGun) DrawPulseFrame(r, Gold, 0.18f, 4.5f, 0.16f);
            }

            if (UiClickAllowed() && hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (unlocked) { equippedGun = i; SaveGame(); }
                else if (TryUnlockGun(i)) { equippedGun = i; SaveGame(); }
                else AddFlash(Danger, 0.2f);
            }
        }
    }

    readonly struct RankUnlockLine
    {
        public readonly string Kind;
        public readonly string Name;
        public readonly int GunIndex;

        public RankUnlockLine(string kind, string name, int gunIndex = -1)
        {
            Kind = kind;
            Name = name;
            GunIndex = gunIndex;
        }
    }

    static int CountRankUnlocks(int level)
    {
        int count = 0;
        for (int i = 0; i < Guns.Length; i++)
        {
            if (Guns[i].LevelReq == level) count++;
        }

        for (int i = 0; i < BodyPalette.Length; i++)
        {
            if (BodyLevelReq[i] == level && BodyFableCost[i] == 0 && BodyWaveReq[i] == 0) count++;
        }

        for (int i = 0; i < AccessoryNames.Length; i++)
        {
            if (AccessoryLevelReq[i] == level && AccessoryFableCost[i] == 0 && AccessoryWaveReq[i] == 0) count++;
        }

        return count;
    }

    static void CollectRankUnlocks(int level, List<RankUnlockLine> dest)
    {
        for (int i = 0; i < Guns.Length; i++)
        {
            if (Guns[i].LevelReq == level)
            {
                dest.Add(new RankUnlockLine("Arm", Guns[i].Name, i));
            }
        }

        for (int i = 0; i < BodyPalette.Length; i++)
        {
            if (BodyLevelReq[i] == level && BodyFableCost[i] == 0 && BodyWaveReq[i] == 0)
            {
                dest.Add(new RankUnlockLine("Heraldry", BodyNames[i]));
            }
        }

        for (int i = 0; i < AccessoryNames.Length; i++)
        {
            if (AccessoryLevelReq[i] == level && AccessoryFableCost[i] == 0 && AccessoryWaveReq[i] == 0)
            {
                dest.Add(new RankUnlockLine("Trinket", AccessoryNames[i]));
            }
        }
    }

    static float RankMilestoneRowHeight(int level)
    {
        int unlocks = CountRankUnlocks(level);
        if (unlocks == 0) return level == playerLevel ? 58f : 44f;
        return 52f + unlocks * 20f;
    }

    static float MeasureRankTabContentHeight()
    {
        float h = 122f;
        h += 26f;

        int rows = 0;
        if (playerLevel > 1)
        {
            h += RankMilestoneRowHeight(playerLevel - 1) + 8f;
            rows++;
        }

        for (int lv = playerLevel; lv <= playerLevel + 18 && rows < 14; lv++)
        {
            if (lv == playerLevel || CountRankUnlocks(lv) > 0)
            {
                h += RankMilestoneRowHeight(lv) + 8f;
                rows++;
            }
        }

        h += 18f;
        h += 40f;

        for (int i = 0; i < Guns.Length; i++)
        {
            ref readonly Gun g = ref Guns[i];
            if (g.WaveReq > 0 && g.LevelReq == 0) h += 56f;
        }

        return h + 16f;
    }

    static float DrawRankSummaryCard(float y)
    {
        const float cardH = 112f;
        float padX = 24f;
        var card = new Rectangle(padX, y, WindowWidth - padX * 2f, cardH);
        DrawRichPanel(card, WithAlpha(UiPanel, 0.92f), UiAccent, 0.22f, accentStripe: true);
        DrawLevelBar(new Rectangle(card.X + 10f, card.Y + 8f, card.Width - 20f, 46f), true);

        float chipW = (card.Width - 36f) / 3f;
        float chipY = card.Y + 62f;
        DrawUiStatChip(new Rectangle(card.X + 10f, chipY, chipW, 42f), "RANK", playerLevel.ToString(), UiAccent);
        DrawUiStatChip(new Rectangle(card.X + 16f + chipW, chipY, chipW, 42f), "BEST WAVE", maxWaveReached.ToString(), MossLight);
        DrawUiStatChip(new Rectangle(card.X + 22f + chipW * 2f, chipY, chipW, 42f), "FABLES", fables.ToString("N0"), Gold);

        string motto = GetUnlockedMotto();
        if (!string.IsNullOrEmpty(motto))
        {
            DrawTextTruncated("\"" + motto + "\"", (int)card.X + 12, (int)(chipY + 48f), (int)card.Width - 24, 11, WithAlpha(Gold, 0.8f));
        }

        int di = (int)runDifficulty;
        if (di >= 0 && di < difficultyRecords.Length)
        {
            ref DifficultyRecord rec = ref difficultyRecords[di];
            string recs = activeDifficulty.Title + ": wave " + rec.BestWave + " � " + rec.BestKills + " kills";
            DrawTextTruncated(recs, (int)card.X + 12, (int)(chipY + (string.IsNullOrEmpty(motto) ? 48f : 64f)), (int)card.Width - 24, 10, WithAlpha(MossLight, 0.72f));
        }

        return cardH + 10f + (string.IsNullOrEmpty(motto) ? 0f : 16f) + 16f;
    }

    static float DrawRankMilestoneRow(float y, int level, bool drawLineBelow)
    {
        float padX = 24f;
        float rowH = RankMilestoneRowHeight(level);
        float timelineX = padX + 16f;
        bool isCurrent = level == playerLevel;
        bool isPast = level < playerLevel;

        Color dot = isCurrent ? Gold : isPast ? UiAccent : WithAlpha(Color.White, 0.45f);
        Raylib.DrawCircleV(new Vector2(timelineX, y + 14f), isCurrent ? 7f : 5f, dot);
        if (isCurrent)
        {
            Raylib.DrawCircleLinesV(new Vector2(timelineX, y + 14f), 10f, WithAlpha(Gold, 0.55f));
        }

        if (drawLineBelow)
        {
            Raylib.DrawLineEx(new Vector2(timelineX, y + 22f), new Vector2(timelineX, y + rowH + 6f), 2f, WithAlpha(UiBorder, 0.35f));
        }

        var card = new Rectangle(padX + 34f, y, WindowWidth - padX * 2f - 34f, rowH);
        Color border = isCurrent ? Gold : isPast ? WithAlpha(UiBorder, 0.55f) : UiBorder;
        DrawRichPanel(card, WithAlpha(UiPanel, isPast ? 0.72f : 0.9f), border, 0.18f, accentStripe: isCurrent);

        string title = isCurrent ? $"RANK {level}  �  YOU ARE HERE" : isPast ? $"RANK {level}  �  COMPLETE" : $"RANK {level}  �  NEXT";
        Color titleCol = isCurrent ? Gold : isPast ? WithAlpha(UiAccent, 0.85f) : WithAlpha(Color.White, 0.82f);
        Raylib.DrawText(title, (int)card.X + 12, (int)card.Y + 8, isCurrent ? 14 : 13, titleCol);

        var unlocks = new List<RankUnlockLine>();
        CollectRankUnlocks(level, unlocks);
        float lineY = card.Y + 30f;
        float time = (float)Raylib.GetTime();

        if (unlocks.Count == 0)
        {
            string empty = isCurrent
                ? "Keep earning XP in runs to reach the next reward."
                : isPast ? "Rank reached." : "No automatic unlocks at this rank.";
            DrawTextTruncated(empty, (int)card.X + 12, (int)lineY, (int)card.Width - 24, 11, WithAlpha(Color.White, 0.48f));
        }
        else
        {
            for (int u = 0; u < unlocks.Count; u++)
            {
                RankUnlockLine entry = unlocks[u];
                Color kindCol = entry.Kind switch
                {
                    "Arm" => UiAccent,
                    "Heraldry" => MossLight,
                    "Trinket" => Gold,
                    _ => throw new UnreachableException(),
                };
                bool earned = isPast || isCurrent;
                Raylib.DrawText(entry.Kind.ToUpperInvariant(), (int)card.X + 12, (int)lineY, 10, WithAlpha(kindCol, earned ? 0.9f : 0.55f));
                if (entry.GunIndex >= 0)
                {
                    DrawGunIcon(entry.GunIndex, new Vector2(card.X + 62f, lineY + 8f), 8f, time, earned ? 1f : 0.45f);
                }

                int nameX = entry.GunIndex >= 0 ? 82 : 72;
                DrawTextTruncated(entry.Name, (int)card.X + nameX, (int)lineY, (int)card.Width - nameX - 56, 12,
                    WithAlpha(Color.White, earned ? 0.88f : 0.58f));
                if (earned)
                {
                    ShadowText("\u2713", (int)(card.X + card.Width - 22f), (int)lineY - 1, 14, UiAccent);
                }

                lineY += 20f;
            }
        }

        return rowH + 8f;
    }

    static void DrawRankStatusPill(Rectangle row, string text, Color accent)
    {
        int fs = 10;
        int tw = Raylib.MeasureText(text, fs);
        var pill = new Rectangle(row.X + row.Width - tw - 22f, row.Y + 10f, tw + 16f, 22f);
        DrawRichPanel(pill, WithAlpha(accent, 0.16f), WithAlpha(accent, 0.55f), 0.35f);
        Raylib.DrawText(text, (int)(pill.X + 8f), (int)(pill.Y + 5f), fs, WithAlpha(Color.White, 0.92f));
    }

    static float DrawRankSiegeSection(float y)
    {
        float padX = 24f;
        DrawUiSectionLabel("SIEGE ARMS", padX, y, Gold);
        y += 24f;
        DrawTextTruncated("Clear waves in runs, then buy these in the ARMS tab with fables.", (int)padX, (int)y,
            WindowWidth - (int)padX * 2, 11, WithAlpha(Color.White, 0.5f));
        y += 20f;

        float time = (float)Raylib.GetTime();
        int textW = (int)(WindowWidth - padX * 2f - 120f);

        for (int i = 0; i < Guns.Length; i++)
        {
            ref readonly Gun g = ref Guns[i];
            if (g.WaveReq <= 0 || g.LevelReq > 0) continue;

            var row = new Rectangle(padX, y, WindowWidth - padX * 2f, 50f);
            bool owned = gunUnlocked[i];
            bool waveMet = maxWaveReached >= g.WaveReq;
            bool canBuy = CanPurchaseGun(i);
            Color border = owned ? UiAccent : canBuy ? Gold : WithAlpha(UiBorder, 0.55f);
            DrawRichPanel(row, WithAlpha(UiPanel, 0.88f), border, 0.16f, accentStripe: owned || canBuy);

            DrawGunIcon(i, new Vector2(row.X + 34f, row.Y + 25f), 10f, time, owned ? 1f : waveMet ? 0.7f : 0.4f);
            DrawTextTruncated(g.Name, (int)row.X + 56, (int)row.Y + 8, textW, 14, owned ? UiAccent : Color.White);
            string req = $"Requires wave {g.WaveReq} cleared  �  {g.FableCost:N0} fables";
            DrawTextTruncated(req, (int)row.X + 56, (int)row.Y + 28, textW, 11,
                WithAlpha(Gold, waveMet ? 0.9f : 0.45f));

            if (owned)
            {
                DrawRankStatusPill(row, "OWNED", UiAccent);
            }
            else if (canBuy)
            {
                DrawRankStatusPill(row, "READY", Gold);
            }
            else if (!waveMet)
            {
                DrawRankStatusPill(row, $"WAVE {g.WaveReq}", Danger);
            }
            else
            {
                DrawRankStatusPill(row, "NEED FABLES", WithAlpha(Gold, 0.75f));
            }

            y += 56f;
        }

        return y;
    }

    static void DrawRankTab()
    {
        float y = ArmoryScrollContentY() - customizeScroll;
        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());

        DrawUiSectionLabel("SIEGE RANK", 24f, y, Gold);
        y += 24f;
        y += DrawRankSummaryCard(y);
        DrawUiSectionLabel("RANK ROADMAP", 24f, y, UiAccent);
        y += 26f;

        var milestoneLevels = new List<int>();
        if (playerLevel > 1) milestoneLevels.Add(playerLevel - 1);
        for (int lv = playerLevel; lv <= playerLevel + 18 && milestoneLevels.Count < 14; lv++)
        {
            if (lv == playerLevel || CountRankUnlocks(lv) > 0)
            {
                milestoneLevels.Add(lv);
            }
        }

        for (int i = 0; i < milestoneLevels.Count; i++)
        {
            y += DrawRankMilestoneRow(y, milestoneLevels[i], drawLineBelow: i < milestoneLevels.Count - 1);
        }

        y += 10f;
        DrawRankSiegeSection(y);
    }

    static void DrawUpgradesTab()
    {
        const float rowH = 72f;
        const float gap = 10f;
        float rowW = WindowWidth - 48f;
        float startX = 24f;
        float startY = ArmoryScrollContentY() - customizeScroll;
        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());
        int descW = (int)rowW - 300;

        DrawUiSectionLabel("PERMANENT PERKS", startX, startY, UiAccent);
        startY += 28f;

        for (int i = 0; i < UpgradeCount; i++)
        {
            var r = new Rectangle(startX, startY + i * (rowH + gap), rowW, rowH);
            bool maxed = upgradeLevels[i] >= UpgradeMax;
            int cost = UpgradeCost(i);
            bool afford = !maxed && fables >= cost;

            DrawRichPanel(r, UiPanel, UiBorder, 0.15f);
            if (afford)
            {
                DrawPulseFrame(r, UiAccent, 0.15f, 4.5f, 0.16f);
            }

            DrawTextTruncated(UpgradeNames[i], (int)r.X + 14, (int)r.Y + 10, descW, 17, Color.White);
            DrawTextTruncated(UpgradeDesc[i], (int)r.X + 14, (int)r.Y + 34, descW, 11, WithAlpha(Color.White, 0.55f));

            float px = r.X + rowW - 168f - UpgradeMax * 22f;
            for (int l = 0; l < UpgradeMax; l++)
            {
                Color pc = l < upgradeLevels[i] ? UiAccent : WithAlpha(Color.White, 0.15f);
                Raylib.DrawRectangle((int)(px + l * 22f), (int)(r.Y + rowH / 2f - 5f), 16, 10, pc);
            }

            var btn = new Rectangle(r.X + rowW - 148f, r.Y + 14f, 132f, 36f);
            if (maxed)
            {
                DrawPanel(btn, Darken(UiPanel, 0.2f), Darken(UiBorder, 0.3f), 0.3f);
                int mw = Raylib.MeasureText("MAX", 16);
                Raylib.DrawText("MAX", (int)(btn.X + btn.Width / 2 - mw / 2), (int)(btn.Y + 10), 16, WithAlpha(Gold, 0.8f));
            }
            else
            {
                string label = cost + " fables";
                if (Button(btn, label, 14, afford, afford ? UiAccent : Darken(Danger, 0.1f)) && afford)
                {
                    fables -= cost;
                    upgradeLevels[i]++;
                    SaveGame();
                    AddFlash(UiAccent, 0.15f);
                }
            }
        }
    }

    static void DrawAbilityIcon(AbilityType ability, Vector2 center, float scale, float time, float alpha = 1f)
    {
        Color accent = WithAlpha(AbilityAccent(ability), alpha);
        Color hi = WithAlpha(Color.White, 0.75f * alpha);
        float pulse = MathF.Sin(time * 4f) * 0.5f + 0.5f;

        switch (ability)
        {
            case AbilityType.Paralyze:
                Raylib.DrawCircleLinesV(center, 15f * scale, WithAlpha(new Color(168, 188, 212, 255), 0.55f * alpha));
                for (int i = 0; i < 6; i++)
                {
                    float a = time * 5f + i * 1.047f;
                    Vector2 p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (11f + pulse * 2f) * scale;
                    Raylib.DrawLineEx(center, p, 2.2f * scale, accent);
                    Raylib.DrawCircleV(p, 2.5f * scale, hi);
                }
                Raylib.DrawCircleV(center, 5f * scale, hi);
                DrawGlow(center, 18f * scale, accent, 0.05f * alpha);
                break;
            case AbilityType.WindStep:
                Vector2 dir = new Vector2(1f, -0.25f);
                dir = SafeNormalize(dir);
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = dir * (-12f + i * 5f) * scale + perp * (i - 1.5f) * 4f * scale;
                    Raylib.DrawCircleV(center + off, (5f - i * 0.6f) * scale, WithAlpha(accent, (0.5f - i * 0.08f) * alpha));
                }
                Raylib.DrawLineEx(center + dir * (-14f * scale), center + dir * (14f * scale), 3f * scale, accent);
                Raylib.DrawLineEx(center + perp * (-8f * scale), center + perp * (8f * scale), 1.5f * scale, hi);
                break;
            case AbilityType.OathOfTheBailey:
                Raylib.DrawRectangle((int)(center.X - 11f * scale), (int)(center.Y - 4f * scale), (int)(22f * scale), (int)(14f * scale), accent);
                Raylib.DrawRectangleLines((int)(center.X - 11f * scale), (int)(center.Y - 4f * scale), (int)(22f * scale), (int)(14f * scale), hi);
                Raylib.DrawTriangle(
                    center + new Vector2(0, -15f * scale),
                    center + new Vector2(-13f * scale, -2f * scale),
                    center + new Vector2(13f * scale, -2f * scale),
                    WithAlpha(new Color(196, 176, 120, 255), 0.65f * alpha));
                Raylib.DrawCircleV(center + new Vector2(0, 2f * scale), 3f * scale, hi);
                break;
            case AbilityType.Verdict:
                Raylib.DrawCircleLinesV(center, 14f * scale, accent);
                Raylib.DrawLineEx(center + new Vector2(0, -13f * scale), center + new Vector2(0, 13f * scale), 2.5f * scale, accent);
                Raylib.DrawLineEx(center + new Vector2(-13f * scale, 0), center + new Vector2(13f * scale, 0), 2.5f * scale, accent);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * (MathF.PI / 2f) + time * 2f;
                    Vector2 p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 9f * scale;
                    Raylib.DrawCircleV(p, 2f * scale, hi);
                }
                Raylib.DrawCircleV(center, 5f * scale, hi);
                break;
            case AbilityType.BannerOfStillness:
                Raylib.DrawLineEx(center + new Vector2(0, 11f * scale), center + new Vector2(0, -17f * scale), 3f * scale, new Color(92, 78, 58, (int)(255 * alpha)));
                Raylib.DrawTriangle(
                    center + new Vector2(0, -17f * scale),
                    center + new Vector2(15f * scale, -3f * scale),
                    center + new Vector2(0, 9f * scale),
                    accent);
                Raylib.DrawCircleLinesV(center, 17f * scale, WithAlpha(accent, 0.5f * alpha));
                Raylib.DrawCircleV(center + new Vector2(6f * scale, -4f * scale), 2.5f * scale, hi);
                break;
            default:
                throw new UnreachableException();
        }
    }

    static void DrawAbilitySlotPanel(int slot, AbilityType ability, KeyboardKey key, Rectangle r, float time)
    {
        Color accent = AbilityAccent(ability);
        DrawRichPanel(r, WithAlpha(UiPanel, 0.9f), accent, 0.2f, accentStripe: true);
        DrawAbilityIcon(ability, new Vector2(r.X + 34f, r.Y + r.Height / 2f), 1f, time);
        DrawGlow(new Vector2(r.X + 34f, r.Y + r.Height / 2f), 30f, accent, 0.05f);
        Raylib.DrawText("SLOT " + (slot + 1), (int)r.X + 58, (int)r.Y + 10, 11, WithAlpha(Color.White, 0.55f));
        Raylib.DrawText(AbilityNames[(int)ability], (int)r.X + 58, (int)r.Y + 26, 15, Color.White);
        Raylib.DrawText("[" + KeyName(key) + "]", (int)r.X + 58, (int)r.Y + 46, 12, accent);
        Raylib.DrawText(AbilityTagline[(int)ability], (int)r.X + 58, (int)r.Y + 62, 10, WithAlpha(accent, 0.75f));
    }

    static void DrawAbilitiesTab()
    {
        const float cardH = 196f;
        const float gap = 14f;
        float cardW = WindowWidth - 48f;
        float startX = 24f;
        float startY = ArmoryScrollContentY() - customizeScroll;
        customizeScroll = Math.Clamp(customizeScroll, 0f, ArmoryScrollMax());
        float time = (float)Raylib.GetTime();
        int descW = (int)cardW - 210;
        int descX = 196;

        DrawUiSectionLabel("BATTLE SKILLS", startX, startY, UiAccent);
        startY += 24f;
        Raylib.DrawText("EQUIPPED", (int)startX, (int)startY, 12, WithAlpha(Color.White, 0.55f));
        startY += 18f;

        float slotW = (cardW - 12f) / 2f;
        var slot1Panel = new Rectangle(startX, startY, slotW, 82f);
        var slot2Panel = new Rectangle(startX + slotW + 12f, startY, slotW, 82f);
        DrawAbilitySlotPanel(0, abilitySlot1, abilityKey1, slot1Panel, time);
        DrawAbilitySlotPanel(1, abilitySlot2, abilityKey2, slot2Panel, time);
        startY += 92f;

        Raylib.DrawText("UNLOCK & ASSIGN", (int)startX, (int)startY, 12, WithAlpha(Color.White, 0.55f));
        startY += 20f;

        for (int i = 0; i < AbilityCount; i++)
        {
            var ability = (AbilityType)i;
            var r = new Rectangle(startX, startY + i * (cardH + gap), cardW, cardH);
            bool unlocked = abilityUnlocked[i];
            bool inSlot1 = abilitySlot1 == ability;
            bool inSlot2 = abilitySlot2 == ability;
            Color accent = AbilityAccent(ability);
            Vector2 m = Raylib.GetMousePosition();
            bool hover = Raylib.CheckCollisionPointRec(m, r);

            Color border = inSlot1 || inSlot2 ? UiAccent : unlocked ? accent : Darken(UiBorder, 0.35f);
            DrawRichPanel(r, hover ? WithAlpha(accent, 0.09f) : WithAlpha(UiPanel, 0.94f), border, 0.2f, accentStripe: inSlot1 || inSlot2);

            var iconFrame = new Rectangle(r.X + 14f, r.Y + 14f, 72f, cardH - 28f);
            DrawRichPanel(iconFrame, WithAlpha(UiPanelDeep, 0.75f), WithAlpha(accent, 0.35f), 0.25f);
            DrawAbilityIcon(ability, new Vector2(iconFrame.X + iconFrame.Width / 2f, iconFrame.Y + iconFrame.Height / 2f), 1.35f, time, unlocked ? 1f : 0.38f);
            DrawGlow(new Vector2(iconFrame.X + iconFrame.Width / 2f, iconFrame.Y + iconFrame.Height / 2f), 42f, accent, unlocked ? 0.07f : 0.02f);

            Raylib.DrawText(AbilityNames[i], descX, (int)r.Y + 14, 18, unlocked ? Color.White : WithAlpha(Color.White, 0.55f));
            Raylib.DrawText(AbilityTagline[i], descX, (int)r.Y + 36, 11, WithAlpha(accent, 0.92f));
            DrawWrappedText(AbilityDesc[i], descX, (int)r.Y + 54, descW, 11, 14, WithAlpha(Color.White, 0.58f), 4);

            if (inSlot1 || inSlot2)
            {
                string badge = inSlot1 && inSlot2 ? "BOTH SLOTS" : inSlot1 ? "IN SLOT 1" : "IN SLOT 2";
                Raylib.DrawText(badge, descX, (int)(r.Y + cardH - 52f), 11, UiAccent);
            }

            if (unlocked)
            {
                var slot1Btn = new Rectangle(r.X + cardW - 252f, r.Y + cardH - 44f, 116f, 34f);
                var slot2Btn = new Rectangle(r.X + cardW - 126f, r.Y + cardH - 44f, 116f, 34f);
                if (Button(slot1Btn, inSlot1 ? "SLOT 1 ?" : "SLOT 1", 13, true, inSlot1 ? UiAccent : UiBorder))
                {
                    AssignAbilityToSlot(0, ability);
                }
                if (Button(slot2Btn, inSlot2 ? "SLOT 2 ?" : "SLOT 2", 13, true, inSlot2 ? UiAccent : UiBorder))
                {
                    AssignAbilityToSlot(1, ability);
                }
            }
            else
            {
                int cost = AbilityFableCost[i];
                if (cost < 0)
                {
                    string lockLabel = "Unlock: " + VerdictUnlockKills + " kills in one run (persists forever)";
                    DrawWrappedText(lockLabel, descX, (int)(r.Y + cardH - 48f), descW, 11, 13, Gold, 2);
                    var buyBtn = new Rectangle(r.X + cardW - 148f, r.Y + cardH - 44f, 132f, 34f);
                    DrawPanel(buyBtn, Darken(UiPanel, 0.15f), UiBorder, 0.25f);
                    Raylib.DrawText("IN RUN", (int)(buyBtn.X + 34), (int)(buyBtn.Y + 10), 12, WithAlpha(Gold, 0.7f));
                }
                else
                {
                    bool ready = cost == 0 || fables >= cost;
                    string lockLabel = cost == 0 ? "Included with every run" : cost.ToString("N0") + " fables to unlock";
                    DrawWrappedText(lockLabel, descX, (int)(r.Y + cardH - 48f), descW, 11, 13, ready ? Gold : WithAlpha(Color.White, 0.45f), 2);
                    if (ready && cost > 0) DrawPulseFrame(r, Gold, 0.16f, 4.5f, 0.14f);

                    var buyBtn = new Rectangle(r.X + cardW - 148f, r.Y + cardH - 44f, 132f, 34f);
                    if (cost == 0)
                    {
                        DrawPanel(buyBtn, Darken(UiPanel, 0.15f), UiBorder, 0.25f);
                        Raylib.DrawText("STANDARD", (int)(buyBtn.X + 20), (int)(buyBtn.Y + 10), 12, WithAlpha(Color.White, 0.5f));
                    }
                    else if (Button(buyBtn, "UNLOCK", 14, ready, ready ? Gold : Darken(Danger, 0.1f)) && ready)
                    {
                        TryUnlockAbility(ability);
                    }
                }
            }
        }
    }

}
