partial class Program
{
    static void DrawHudSection(Rectangle area, string label, string value, Color valueColor, Color iconColor, bool iconCircle = true)
    {
        var inner = new Rectangle(area.X + 6f, area.Y + 4f, area.Width - 12f, area.Height - 8f);
        Raylib.DrawRectangleRounded(inner, 0.25f, 4, WithAlpha(UiPanelDeep, 0.55f));
        Raylib.DrawRectangle((int)inner.X, (int)inner.Y + 6, 2, (int)inner.Height - 12, WithAlpha(iconColor, 0.75f));

        if (iconCircle)
        {
            Raylib.DrawCircleV(new Vector2(area.X + 22f, area.Y + area.Height / 2f), 5f, iconColor);
            Raylib.DrawCircleV(new Vector2(area.X + 22f, area.Y + area.Height / 2f), 2f, WithAlpha(Color.White, 0.35f));
        }

        int labelX = iconCircle ? (int)area.X + 34 : (int)area.X + 14;
        Raylib.DrawText(label, labelX, (int)area.Y + 7, 10, WithAlpha(Color.White, 0.52f));
        ShadowText(value, labelX, (int)area.Y + 19, 20, valueColor);
    }

    static void DrawLevelBar(Rectangle area, bool showLabel, bool ghost = false)
    {
        float dt = Raylib.GetFrameTime();
        int need = XpToNextLevel();
        float target = need > 0 ? playerXp / (float)need : 0f;
        levelBarVis = Approach(levelBarVis, target, 10f, dt);

        const float barH = 8f;
        const float barInset = 12f;
        float barY = area.Y + area.Height - barH - 8f;

        DrawRichPanel(area, WithAlpha(UiPanel, ghost ? 0.32f : 0.82f), ghost ? WithAlpha(UiBorder, 0.35f) : UiBorder, 0.22f);
        if (showLabel)
        {
            int textY = (int)(area.Y + 5f);
            string rank = $"RANK {playerLevel}";
            ShadowText(rank, (int)area.X + (int)barInset, textY, 15, Color.White);
            string prog = $"{playerXp} / {need} xp";
            int pw = Raylib.MeasureText(prog, 12);
            Raylib.DrawText(prog, (int)(area.X + area.Width - pw - barInset), textY + 2, 12, Color.White);
        }

        var barBg = new Rectangle(area.X + barInset, barY, area.Width - barInset * 2f, barH);
        Raylib.DrawRectangleRounded(barBg, 0.8f, 4, WithAlpha(UiPanelDeep, 0.95f));
        var barFill = new Rectangle(barBg.X, barBg.Y, MathF.Max(0f, barBg.Width * levelBarVis), barBg.Height);
        if (barFill.Width > 1f)
        {
            Raylib.DrawRectangleRounded(barFill, 0.8f, 4, Darken(UiAccent, 0.12f));
            Raylib.DrawRectangleGradientV((int)barFill.X, (int)barFill.Y, (int)barFill.Width, (int)barFill.Height,
                WithAlpha(Lighten(Gold, 0.15f), 0.45f), WithAlpha(UiAccent, 0.92f));
            Raylib.DrawRectangle((int)barFill.X, (int)barFill.Y, (int)barFill.Width, 1, WithAlpha(Color.White, 0.18f));
        }
        Raylib.DrawRectangleRoundedLines(barBg, 0.8f, 4, WithAlpha(UiBorder, 0.35f));
    }

    static void DrawLevelUpBanner()
    {
        if (!showLevelUpBanner) return;
        if (levelUpBannerTimer <= 0f) return;
        float a = Math.Clamp(levelUpBannerTimer / 2.8f, 0f, 1f);
        if (levelUpBannerTimer < 0.6f) a = levelUpBannerTimer / 0.6f;
        int cx = WindowWidth / 2;
        string txt = $"RANK {playerLevel}";
        ShadowTextCentered(txt, cx, 108, 30, Gold, a);
        ShadowTextCentered("new arms and trinkets may be ready", cx, 142, 14, WithAlpha(Color.White, 0.7f), a);
    }

    static void DrawTopHud()
    {
        if (!showTopHud) return;
        var panel = new Rectangle(14f, 10f, WindowWidth - 28f, 54f);
        DrawRichPanel(panel, UiPanel, UiBorder, 0.35f, accentStripe: true);

        float third = panel.Width / 3f;
        DrawHudSection(
            new Rectangle(panel.X, panel.Y, third, panel.Height),
            "SCORE", ((int)scoreDisplay).ToString("N0"), Color.White, UiAccent);
        DrawHudSection(
            new Rectangle(panel.X + third, panel.Y, third, panel.Height),
            waveInProgress ? "SWARM" : "WAVE",
            waveInProgress ? $"{waveSwarmIndex}/{waveSwarmTotal}" : waveNumber.ToString(),
            Color.White, MossLight);
        DrawHudSection(
            new Rectangle(panel.X + third * 2f, panel.Y, third, panel.Height),
            "FABLES", fables.ToString("N0"), Gold, Gold);

        for (int d = 1; d < 3; d++)
        {
            float dx = panel.X + third * d;
            Raylib.DrawLineEx(new Vector2(dx, panel.Y + 10f), new Vector2(dx, panel.Y + panel.Height - 10f), 1f, WithAlpha(UiBorder, 0.42f));
            Raylib.DrawLineEx(new Vector2(dx + 1f, panel.Y + 10f), new Vector2(dx + 1f, panel.Y + panel.Height - 10f), 1f, WithAlpha(UiBorderLight, 0.1f));
        }

        var rankBar = new Rectangle(panel.X + 12f, panel.Y + panel.Height + 6f, panel.Width - 24f, 36f);
        DrawLevelBar(rankBar, true);

        if (runGunAffix != GunAffixType.None && state == GameState.Playing)
        {
            string affix = GunAffixName();
            int aw = Raylib.MeasureText(affix, 11);
            ShadowText(affix, (int)(panel.X + panel.Width - aw - 14f), (int)(panel.Y + 8f), 11, MossLight, 0.85f);
        }
    }

    static void DrawComboMeter()
    {
        if (!showComboMeter) return;
        if (combo < 2 || state != GameState.Playing) return;

        float dt = Raylib.GetFrameTime();
        Color col = ComboColor(combo);
        string txt = $"COMBO  x{combo}";
        float fresh = Math.Clamp(comboTimer / ComboWindow, 0f, 1f);
        comboFillVis = Approach(comboFillVis, fresh, 14f, dt);
        float pop = 1f + (comboTimer > ComboWindow - 0.12f ? 0.25f : 0f);
        int fs = (int)(24 * pop);
        int cx = WindowWidth / 2;
        int y = 70;

        ShadowTextCentered(txt, cx, y, fs, col);

        int barW = 160;
        int barH = 6;
        var barBg = new Rectangle(cx - barW / 2f, y + fs + 6, barW, barH);
        Raylib.DrawRectangleRounded(barBg, 0.8f, 4, WithAlpha(ForestShadow, 0.5f));
        var barFill = new Rectangle(barBg.X, barBg.Y, MathF.Max(0f, barW * comboFillVis), barH);
        Raylib.DrawRectangleRounded(barFill, 0.8f, 4, col);
        if (comboFillVis > 0.02f)
        {
            DrawGlow(new Vector2(barFill.X + barFill.Width, barFill.Y + barH / 2f), 8f, col, 0.6f);
        }
    }

    static void DrawWaveBanner()
    {
        if (!showWaveBanner) return;
        if (waveBannerTimer <= 0f) return;

        float since = WaveBannerTime - waveBannerTimer;
        float a = 1f;
        if (since < 0.3f) a = since / 0.3f;
        else if (waveBannerTimer < 0.5f) a = waveBannerTimer / 0.5f;

        float slide = (1f - a) * 20f;
        string txt = $"WAVE {waveNumber}";
        int fs = 52;
        int w = Raylib.MeasureText(txt, fs);
        int cx = WindowWidth / 2;
        int y = (int)(175f - slide);

        var banner = new Rectangle(cx - w / 2f - 28f, y - 12f, w + 56f, fs + 44f);
        MenuCastlePalette p = GameplayCastlePalette;
        DrawRichPanel(banner, WithAlpha(UiPanel, a * 0.92f), WithAlpha(UiBorder, a), 0.2f, accentStripe: true);

        float grateW = banner.Width * 0.72f;
        float grateX = banner.X + (banner.Width - grateW) / 2f;
        float grateY = banner.Y + banner.Height - 10f;
        int grateBars = 9;
        for (int b = 0; b < grateBars; b++)
        {
            float bx = grateX + b * (grateW / (grateBars - 1));
            Raylib.DrawLineEx(new Vector2(bx, grateY - 14f), new Vector2(bx, grateY), 1.4f, WithAlpha(p.Iron, a * 0.55f));
        }
        Raylib.DrawLineEx(new Vector2(grateX, grateY - 6f), new Vector2(grateX + grateW, grateY - 6f), 1.2f, WithAlpha(p.StoneLight, a * 0.4f));

        SiegeBaileyLayout L = SiegeBaileyLayout.Compute();
        float gateCx = L.GateX + L.GateW / 2f;
        Raylib.DrawLineEx(new Vector2(gateCx, banner.Y + banner.Height + 4f), new Vector2(cx, banner.Y + banner.Height + 18f),
            1f, WithAlpha(p.TorchWarm, a * 0.35f));

        ShadowTextCentered(txt, cx, y, fs, Gold, a);

        string sub = waveSubtext.Length > 0 ? waveSubtext : "FOOTSTEPS ON THE STONE";
        int sw = Raylib.MeasureText(sub, 16);
        Raylib.DrawText(sub, cx - sw / 2, y + fs + 6, 16, WithAlpha(UiAccent, a * 0.85f));

        if (siegeObjective != SiegeObjectiveType.None && waveInProgress)
        {
            string obj = SiegeObjectiveLabel();
            Color objCol = siegeObjectiveFailed ? Danger : siegeObjectiveDone ? UiAccent : Gold;
            ShadowTextCentered(obj, cx, y + fs + 28, 13, WithAlpha(objCol, a * 0.9f));
        }
    }

    static void DrawFloorEventHud()
    {
        if (!showFloorEventHud) return;
        if (activeEvent == FloorEventType.None || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        int cx = WindowWidth / 2;
        string name = FloorEventNames[(int)activeEvent];
        float pulse = MathF.Sin(time * 8f) * 0.5f + 0.5f;
        float urgency = eventStartCountdown > 0.01f
            ? Math.Clamp(1f - eventCountdown / eventStartCountdown, 0.25f, 1f)
            : 0.5f;

        bool tall = activeEvent is FloorEventType.SafeZoneRush or FloorEventType.CenterSnare or FloorEventType.MossRot
            or FloorEventType.EmberGate or FloorEventType.EmberTide or FloorEventType.EmberCage
            or FloorEventType.CryptVeil or FloorEventType.CryptMist
            or FloorEventType.CrownEdict or FloorEventType.CrownThrone or FloorEventType.CrownRot;
        var panel = new Rectangle(cx - 240f, 104f, 480f, tall ? 108f : 88f);
        Color accent = EventAccentColor(activeEvent);
        DrawRichPanel(panel, WithAlpha(UiPanel, 0.92f), WithAlpha(accent, 0.45f + pulse * 0.4f * urgency), 0.22f, accentStripe: true);
        DrawPulseFrame(panel, accent, 0.18f, 10f, 0.12f + pulse * 0.2f * urgency);
        ShadowTextCentered(name, cx, 116, 24, accent, 1f);

        bool timed = activeEvent is FloorEventType.SafeZoneRush or FloorEventType.CenterSnare
            or FloorEventType.CrimsonCrumble or FloorEventType.Checkerfall or FloorEventType.RingCollapse
            or FloorEventType.StoneIslands or FloorEventType.ScatterPits or FloorEventType.MarkedStrike
            or FloorEventType.BlightStorm or FloorEventType.MossRot
            or FloorEventType.EmberRain or FloorEventType.EmberGate or FloorEventType.EmberPulse
            or FloorEventType.EmberCross or FloorEventType.EmberBridge or FloorEventType.EmberFury
            or FloorEventType.EmberSnake or FloorEventType.EmberHive or FloorEventType.EmberTide
            or FloorEventType.EmberCage or FloorEventType.EmberQuake or FloorEventType.EmberBloom
            or FloorEventType.EmberAltar
            or FloorEventType.CryptSeal or FloorEventType.CryptWail or FloorEventType.CryptTorch
            or FloorEventType.CryptChains or FloorEventType.CryptMist or FloorEventType.CryptTomb
            or FloorEventType.CryptShroud or FloorEventType.CryptGlimpse or FloorEventType.CryptRattle
            or FloorEventType.CryptEcho or FloorEventType.CryptGrave or FloorEventType.CryptLantern
            or FloorEventType.CryptVeil
            or FloorEventType.CrownTrial or FloorEventType.CrownFall or FloorEventType.CrownShard
            or FloorEventType.CrownThrone or FloorEventType.CrownEdict or FloorEventType.CrownRot
            or FloorEventType.CrownBolt or FloorEventType.CrownRing or FloorEventType.CrownIsles
            or FloorEventType.CrownStorm or FloorEventType.CrownCoronation or FloorEventType.CrownUsurpation
            or FloorEventType.CrownReckoning;

        if (timed && (eventCountdown > 0f || eventPhase == 1))
        {
            string cd = eventPhase == 1 ? "NOW!" : FormatEventCountdown(eventCountdown);
            ShadowTextCentered(cd + (eventPhase == 1 ? "" : "s"), cx, 148, 28, WithAlpha(Color.White, 0.95f), 1f);
            string action = activeEvent switch
            {
                FloorEventType.StoneIslands => "REACH AN ISLAND",
                FloorEventType.SafeZoneRush => "REACH SAFE BAND",
                FloorEventType.CenterSnare => "HUG THE WALLS",
                FloorEventType.MarkedStrike => $"STRIKE {eventStep + 1}/{markedStrikeCount}",
                FloorEventType.BlightStorm => "WEATHER THE STORM",
                FloorEventType.TideSurge => "FLEE THE SURGE",
                FloorEventType.TideRift => "LEAP THE RIFT",
                FloorEventType.TideRecede => "HUG THE RIM",
                FloorEventType.TideColumn => "DODGE THE COLUMNS",
                FloorEventType.TideEcho => "ESCAPE THE RIPPLES",
                FloorEventType.TideUndertow => "FLEE THE CENTER",
                FloorEventType.TideCrest => "RIDE THE CRESTS",
                FloorEventType.TideWall => TideWallHint(),
                FloorEventType.TideAnchor => "REACH AN ANCHOR",
                FloorEventType.TideFoam => "WEATHER THE FOAM",
                FloorEventType.TideWhirlpool => "FLEE THE VORTEX",
                FloorEventType.TideBeacon => "REACH THE BEACON",
                FloorEventType.TideStrike => $"SALVO {eventStep + 1}/{markedStrikeCount}",
                FloorEventType.MossRot => floorRotSide < 0.5f ? "FLEE LEFT ROT" : "FLEE RIGHT ROT",
                FloorEventType.EmberRain => eventSide == 0 ? "DODGE RIGHT RAIN" : "DODGE LEFT RAIN",
                FloorEventType.EmberGate => "THROUGH THE GATE",
                FloorEventType.EmberPulse => "FLEE THE PULSES",
                FloorEventType.EmberCross => "AVOID THE CROSS",
                FloorEventType.EmberBridge => "CROSS THE BRIDGES",
                FloorEventType.EmberFury => "KEEP MOVING!",
                FloorEventType.EmberSnake => "OUTRUN THE SERPENT",
                FloorEventType.EmberHive => "REACH THE HIVE",
                FloorEventType.EmberTide => eventSide == 2 ? "CLIMB ABOVE TIDE" : "BELOW THE SURGE",
                FloorEventType.EmberCage => "HOLD THE CENTER",
                FloorEventType.EmberQuake => "SURVIVE THE SHAKES",
                FloorEventType.EmberBloom => "FLEE THE BLOOMS",
                FloorEventType.EmberAltar => "SEIZE THE ALTAR",
                FloorEventType.CrownTrial => "SURVIVE THE GAUNTLET",
                FloorEventType.CrownFall => "FLEE GILDED TILES",
                FloorEventType.CrownShard => "DODGE THE SHARDS",
                FloorEventType.CrownThrone => "SEIZE THE THRONE",
                FloorEventType.CrownEdict => "OBEY THE EDICT",
                FloorEventType.CrownRot => floorRotSide < 0.5f ? "FLEE USURPER ROT" : "FLEE CROWN ROT",
                FloorEventType.CrownBolt => $"JUDGMENT {eventStep + 1}/{markedStrikeCount}",
                FloorEventType.CrownRing => "ESCAPE THE RING",
                FloorEventType.CrownIsles => "REACH A BASTION",
                FloorEventType.CrownStorm => "WEATHER THE TEMPEST",
                FloorEventType.CrownCoronation => "REACH THE DAIS",
                FloorEventType.CrownUsurpation => "FLEE THE CROSS",
                FloorEventType.CrownReckoning => "OUTRUN RECKONING",
                _ => "GET CLEAR",
            };
            ShadowTextCentered(action, cx, 176, 14, WithAlpha(Gold, 0.7f + pulse * 0.3f), 1f);

            if (urgency > 0.55f)
            {
                ShadowTextCentered("HURRY!", cx, 194, 13, WithAlpha(accent, 0.65f + pulse * 0.35f), 1f);
            }
        }

        if (activeEvent is FloorEventType.SafeZoneRush or FloorEventType.CenterSnare)
        {
            string hint = activeEvent == FloorEventType.SafeZoneRush ? SafeZoneRushHint() : "SAFE: KEEP WALL";
            Color hintColor = playerInEventSafeZone ? UiAccent : Danger;
            ShadowTextCentered(hint, cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
            if (playerInEventSafeZone)
            {
                ShadowTextCentered("IN SAFE ZONE", cx, tall ? 204 : 184, 13, WithAlpha(UiAccent, 0.85f), 1f);
            }
        }
        else if (activeEvent == FloorEventType.StoneIslands && eventCountdown > 0f)
        {
            bool onIsland = PlayerOnSafeIsland();
            Color hintColor = onIsland ? UiAccent : Danger;
            ShadowTextCentered("SAFE: STONE ISLAND", cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
            if (onIsland)
            {
                ShadowTextCentered("ON SOLID GROUND", cx, tall ? 204 : 184, 13, WithAlpha(UiAccent, 0.85f), 1f);
            }
        }
        else if (activeEvent is FloorEventType.EmberGate or FloorEventType.EmberTide or FloorEventType.EmberCage or FloorEventType.EmberAltar)
        {
            Color hintColor = playerInEventSafeZone ? UiAccent : Danger;
            string hint = activeEvent switch
            {
                FloorEventType.EmberGate => "SAFE: FIRE CORRIDOR",
                FloorEventType.EmberTide => "SAFE: DRY GROUND",
                FloorEventType.EmberCage => "SAFE: INNER CAGE",
                FloorEventType.EmberAltar => "SAFE: ALTAR ZONE",
                _ => "SAFE ZONE",
            };
            ShadowTextCentered(hint, cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent == FloorEventType.EmberHive && eventCountdown > 0f)
        {
            bool onHive = PlayerOnSafeIsland();
            Color hintColor = onHive ? UiAccent : Danger;
            ShadowTextCentered("SAFE: EMBER HIVE", cx, 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent is FloorEventType.CryptVeil or FloorEventType.CryptSeal)
        {
            bool inSafe = activeEvent == FloorEventType.CryptVeil
                ? PlayerInSafeRect(eventSafeRect)
                : PlayerInCenterSnareSafe();
            Color hintColor = inSafe ? UiAccent : Danger;
            string hint = activeEvent == FloorEventType.CryptVeil ? "SAFE: LIT QUADRANT" : "SAFE: OUTER RING";
            ShadowTextCentered(hint, cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent == FloorEventType.CryptLantern && eventCountdown > 0f)
        {
            bool onPath = TryGetTileUnder(playerPos, out int lx, out int ly) && !tiles[lx, ly].EventMarked;
            Color hintColor = onPath ? UiAccent : Danger;
            ShadowTextCentered("SAFE: LANTERN PATH", cx, 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent is FloorEventType.TideAnchor && eventCountdown > 0f)
        {
            bool onAnchor = PlayerOnSafeIsland();
            Color hintColor = onAnchor ? UiAccent : Danger;
            ShadowTextCentered("SAFE: TIDE ANCHOR", cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent is FloorEventType.TideBeacon or FloorEventType.TideWhirlpool)
        {
            string hint = activeEvent == FloorEventType.TideBeacon ? TideBeaconHint() : "SAFE: OUTSIDE VORTEX";
            Color hintColor = playerInEventSafeZone ? UiAccent : Danger;
            ShadowTextCentered(hint, cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent is FloorEventType.CrownEdict or FloorEventType.CrownThrone)
        {
            string hint = activeEvent == FloorEventType.CrownEdict ? SafeZoneRushHint() : "SAFE: THRONE DAIS";
            Color hintColor = playerInEventSafeZone ? UiAccent : Danger;
            ShadowTextCentered(hint, cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
            if (playerInEventSafeZone)
            {
                ShadowTextCentered("ROYAL SANCTUARY", cx, tall ? 204 : 184, 13, WithAlpha(UiAccent, 0.85f), 1f);
            }
        }
        else if (activeEvent == FloorEventType.CrownIsles && eventCountdown > 0f)
        {
            bool onIsland = PlayerOnSafeIsland();
            Color hintColor = onIsland ? UiAccent : Danger;
            ShadowTextCentered("SAFE: CROWN BASTION", cx, tall ? 188 : 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
        else if (activeEvent == FloorEventType.CrownCoronation && eventCountdown > 0f)
        {
            bool onDais = PlayerInCrownCenter(2);
            Color hintColor = onDais ? UiAccent : Danger;
            ShadowTextCentered("SAFE: CORONATION DAIS", cx, 168, 15, WithAlpha(hintColor, 0.95f), 1f);
        }
    }

    static void DrawFloorEventWarningBorder()
    {
        if (!showEventWarningBorder) return;
        if (activeEvent == FloorEventType.None || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 9f) * 0.5f + 0.5f;
        Color accent = EventAccentColor(activeEvent);
        float urgency = eventStartCountdown > 0.01f
            ? Math.Clamp(1f - eventCountdown / eventStartCountdown, 0.2f, 1f)
            : eventPhase == 1 ? 1f : 0.35f;
        bool subtle = activeEvent == FloorEventType.SafeZoneRush;
        int thickness = subtle ? 4 + (int)(pulse * 3f * urgency) : 10 + (int)(pulse * 18f * urgency);
        Color c = WithAlpha(accent, subtle ? 0.08f + pulse * 0.1f * urgency : 0.24f + pulse * 0.32f * urgency);

        Raylib.DrawRectangle(0, 0, WindowWidth, thickness, c);
        Raylib.DrawRectangle(0, WindowHeight - thickness, WindowWidth, thickness, c);
        Raylib.DrawRectangle(0, 0, thickness, WindowHeight, c);
        Raylib.DrawRectangle(WindowWidth - thickness, 0, thickness, WindowHeight, c);
    }

    static void DrawMarkedStrikeLasers()
    {
        if (state != GameState.Playing) return;
        if (activeEvent is not (FloorEventType.MarkedStrike or FloorEventType.CryptGrave or FloorEventType.CrownBolt)) return;
        if (eventSkyBeams.Count == 0) return;

        float time = (float)Raylib.GetTime();
        Color accent = EventAccentColor(activeEvent);
        Color hotWhite = activeEvent == FloorEventType.CryptGrave
            ? new Color(232, 228, 248, 255)
            : new Color(255, 252, 244, 255);
        Color plasma = activeEvent == FloorEventType.CryptGrave
            ? new Color(168, 148, 196, 255)
            : new Color(255, 148, 72, 255);
        Color ember = activeEvent == FloorEventType.CryptGrave
            ? new Color(88, 72, 108, 255)
            : new Color(220, 52, 28, 255);

        foreach (EventSkyBeam beam in eventSkyBeams)
        {
            DrawMarkedStrikeBeam(beam, time, accent, hotWhite, plasma, ember);
        }
    }

    static void DrawMarkedStrikeBeam(in EventSkyBeam beam, float time, Color accent, Color hotWhite, Color plasma,
        Color ember)
    {
        float lifeT = beam.MaxLife > 0.001f ? beam.Life / beam.MaxLife : 0f;
        Vector2 ground = beam.Ground;
        Vector2 top = new Vector2(ground.X, -80f);
        Vector2 dir = SafeNormalize(ground - top);
        Vector2 side = new Vector2(-dir.Y, dir.X);

        if (beam.Charging)
        {
            float charge = 1f - lifeT;
            float flicker = 0.82f + MathF.Sin(time * 48f) * 0.12f + MathF.Sin(time * 73f + ground.X) * 0.06f;
            float chargeAlpha = (0.2f + charge * 0.65f) * flicker;
            float width = beam.Width * (0.35f + charge * 0.85f);

            Raylib.DrawLineEx(top, ground, width * 2.4f, WithAlpha(ember, chargeAlpha * 0.22f));
            Raylib.DrawLineEx(top, ground, width * 1.35f, WithAlpha(plasma, chargeAlpha * 0.42f));
            Raylib.DrawLineEx(top, ground, width * 0.45f, WithAlpha(hotWhite, chargeAlpha * 0.55f));

            float ringR = 10f + charge * 22f;
            float pulse = MathF.Sin(time * 28f) * 0.5f + 0.5f;
            Raylib.DrawCircleLinesV(ground, ringR, WithAlpha(plasma, chargeAlpha * 0.75f));
            Raylib.DrawCircleLinesV(ground, ringR * 0.62f, WithAlpha(hotWhite, chargeAlpha * (0.45f + pulse * 0.35f)));
            DrawGlowFast(ground, ringR * 1.2f, plasma, chargeAlpha * 0.14f);

            for (int i = 0; i < 4; i++)
            {
                float sparkT = (time * 2.8f + i * 0.37f + ground.X * 0.01f) % 1f;
                Vector2 spark = Vector2.Lerp(ground, top, sparkT);
                Raylib.DrawCircleV(spark, 1.5f + charge * 2f, WithAlpha(hotWhite, chargeAlpha * (1f - sparkT) * 0.7f));
            }

            Raylib.DrawLineEx(ground - side * 16f, ground + side * 16f, 2f, WithAlpha(hotWhite, chargeAlpha * 0.65f));
            Raylib.DrawLineEx(ground - dir * 10f, ground + dir * 10f, 2f, WithAlpha(hotWhite, chargeAlpha * 0.65f));
            return;
        }

        float strike = lifeT;
        float impact = MathF.Pow(strike, 0.55f);
        float strikeAlpha = impact * 0.98f;
        float coreW = beam.Width * (0.18f + impact * 0.22f);
        float midW = beam.Width * (0.55f + impact * 0.35f);
        float outerW = beam.Width * (1.1f + impact * 0.65f);

        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawLineEx(top, ground, outerW, WithAlpha(ember, strikeAlpha * 0.28f));
        Raylib.DrawLineEx(top, ground, midW, WithAlpha(plasma, strikeAlpha * 0.52f));
        Raylib.DrawLineEx(top, ground, coreW * 1.8f, WithAlpha(Lighten(plasma, 0.35f), strikeAlpha * 0.72f));
        Raylib.DrawLineEx(top, ground, coreW, WithAlpha(hotWhite, strikeAlpha * 0.95f));

        for (int layer = 0; layer < 3; layer++)
        {
            float off = (layer - 1) * 3.5f;
            Vector2 o = side * off;
            Raylib.DrawLineEx(top + o, ground + o, coreW * 0.35f, WithAlpha(plasma, strikeAlpha * 0.18f));
        }
        Raylib.EndBlendMode();

        float scorchR = 18f + impact * 34f;
        DrawGlowFast(ground, scorchR * 1.2f, ember, strikeAlpha * 0.2f);
        DrawGlowFast(ground, scorchR * 0.6f, hotWhite, strikeAlpha * 0.15f);
        Raylib.DrawCircleV(ground, 5f + impact * 6f, WithAlpha(hotWhite, strikeAlpha));
        Raylib.DrawCircleLinesV(ground, scorchR, WithAlpha(plasma, strikeAlpha * 0.7f));
        Raylib.DrawCircleLinesV(ground, scorchR * 0.55f, WithAlpha(hotWhite, strikeAlpha * 0.55f));

        for (int ray = 0; ray < 6; ray++)
        {
            float ang = ray * MathF.PI / 3f + time * 2.2f;
            Vector2 rayEnd = ground + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (scorchR * 0.85f);
            Raylib.DrawLineEx(ground, rayEnd, 2.5f, WithAlpha(plasma, strikeAlpha * 0.35f));
        }

        var spill = new Rectangle(ground.X - scorchR, ground.Y - 4f, scorchR * 2f, 8f);
        Raylib.DrawRectangleGradientH((int)spill.X, (int)spill.Y, (int)spill.Width, (int)spill.Height,
            WithAlpha(ember, 0f), WithAlpha(ember, strikeAlpha * 0.35f));
        Raylib.DrawRectangleGradientH((int)spill.X, (int)spill.Y, (int)spill.Width, (int)spill.Height,
            WithAlpha(ember, strikeAlpha * 0.35f), WithAlpha(ember, 0f));
    }

    static void DrawEventVfx()
    {
        if (activeEvent == FloorEventType.None || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        Color accent = EventAccentColor(activeEvent);

        foreach (EventShockwave sw in eventShockwaves)
        {
            float t = 1f - sw.Life / sw.MaxLife;
            float alpha = (1f - t) * 0.62f;
            Raylib.DrawCircleLinesV(sw.Center, sw.Radius, WithAlpha(sw.Color, alpha));
            Raylib.DrawCircleLinesV(sw.Center, sw.Radius * 0.72f, WithAlpha(Lighten(sw.Color, 0.35f), alpha * 0.75f));
            if (t < 0.35f)
            {
                DrawGlowFast(sw.Center, sw.Radius * 0.3f, sw.Color, alpha * 0.16f);
            }
        }

        foreach (EventSkyBeam beam in eventSkyBeams)
        {
            if (activeEvent is FloorEventType.MarkedStrike or FloorEventType.CryptGrave) continue;

            float lifeT = beam.Life / beam.MaxLife;
            Vector2 top = new Vector2(beam.Ground.X, -40f);
            float alpha = beam.Charging
                ? 0.28f + (1f - lifeT) * 0.58f
                : lifeT * 0.98f;
            float width = beam.Width * (beam.Charging ? 0.5f + (1f - lifeT) * 0.8f : lifeT);

            Raylib.DrawLineEx(top, beam.Ground, width * 1.45f, WithAlpha(beam.Color, alpha * 0.28f));
            Raylib.DrawLineEx(top, beam.Ground, width, WithAlpha(Lighten(beam.Color, 0.45f), alpha * 0.82f));
            Raylib.DrawLineEx(top, beam.Ground, width * 0.3f, WithAlpha(Color.White, alpha * 0.92f));
            DrawGlowFast(beam.Ground, width * 1.4f, beam.Color, alpha * 0.18f);

            if (beam.Charging)
            {
                float pulse = MathF.Sin(time * 24f) * 0.5f + 0.5f;
                Raylib.DrawCircleLinesV(beam.Ground, 14f + pulse * 8f, WithAlpha(beam.Color, alpha));
            }
            else
            {
                Raylib.DrawCircleV(beam.Ground, 6f, WithAlpha(Color.White, alpha));
            }
        }

        if (activeEvent == FloorEventType.CrimsonCrumble && eventPhase == 0)
        {
            float pulse = MathF.Sin(time * 10f) * 0.5f + 0.5f;
            DrawGlowFast(eventEpicenter, 36f + pulse * 20f, accent, 0.1f + pulse * 0.08f);
            Raylib.DrawCircleLinesV(eventEpicenter, 22f + pulse * 16f, WithAlpha(accent, 0.58f));
        }

        if (activeEvent == FloorEventType.CenterSnare)
        {
            float cx = WindowWidth / 2f;
            float cy = WindowHeight / 2f;
            float urgency = eventStartCountdown > 0.01f
                ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
                : 0f;
            float vortexR = 60f + urgency * 120f;
            for (int i = 0; i < 4; i++)
            {
                float a = time * (2.5f + urgency * 3f) + i * 1.57f;
                Vector2 p = new Vector2(cx + MathF.Cos(a) * vortexR, cy + MathF.Sin(a) * vortexR * 0.75f);
                Raylib.DrawCircleV(p, 4.5f + urgency * 3f, WithAlpha(Danger, 0.32f + urgency * 0.24f));
            }
            DrawGlowFast(new Vector2(cx, cy), vortexR, Danger, 0.06f + urgency * 0.08f);
        }

        if (activeEvent == FloorEventType.BlightStorm || activeEvent == FloorEventType.CrownStorm)
        {
            int streaks = 5;
            for (int i = 0; i < streaks; i++)
            {
                float x = Hash(i * 97) * WindowWidth;
                float y = (time * 220f + i * 90f) % (WindowHeight + 80f) - 40f;
                Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x + 30f, y + 70f), 2f, WithAlpha(accent, 0.16f));
            }
        }

        if (activeEvent == FloorEventType.CrownThrone && eventPhase == 0)
        {
            Vector2 throne = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
            float pulse = MathF.Sin(time * 7f) * 0.5f + 0.5f;
            DrawGlowFast(throne, 40f + pulse * 24f, accent, 0.12f + pulse * 0.08f);
            Raylib.DrawCircleLinesV(throne, 28f + pulse * 14f, WithAlpha(accent, 0.55f));
        }

        if (activeEvent == FloorEventType.CrownCoronation && eventPhase == 0)
        {
            Vector2 dais = new Vector2(WindowWidth / 2f, WindowHeight / 2f);
            float pulse = MathF.Sin(time * 6f) * 0.5f + 0.5f;
            for (int ray = 0; ray < 8; ray++)
            {
                float ang = ray * MathF.PI / 4f + time * 0.8f;
                Vector2 end = dais + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (80f + pulse * 40f);
                Raylib.DrawLineEx(dais, end, 1.5f, WithAlpha(accent, 0.14f + pulse * 0.1f));
            }
        }

        if (activeEvent == FloorEventType.SafeZoneRush && eventCountdown > 0f)
        {
            Vector2 beacon = new Vector2(
                eventSafeRect.X + eventSafeRect.Width * 0.5f,
                eventSafeRect.Y + eventSafeRect.Height * 0.5f);
            DrawGlow(beacon, 36f, new Color(108, 132, 102, 255), 0.06f + MathF.Sin(time * 6f) * 0.5f * 0.02f);
        }

        if (activeEvent == FloorEventType.EmberRain)
        {
            for (int i = 0; i < 6; i++)
            {
                float x = (time * 180f + i * 73f) % (WindowWidth + 40f) - 20f;
                float y = (time * 260f + i * 41f) % (WindowHeight + 60f) - 30f;
                Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x + 8f, y + 22f), 2f, WithAlpha(accent, 0.2f));
            }
        }

        if (activeEvent == FloorEventType.EmberPulse && eventPhase == 0)
        {
            float ringR = (eventStep + MathF.Sin(time * 8f) * 0.3f) * TileSize;
            Raylib.DrawCircleLinesV(eventEpicenter, ringR, WithAlpha(accent, 0.45f));
            DrawGlowFast(eventEpicenter, ringR * 0.4f, accent, 0.08f);
        }

        if (activeEvent == FloorEventType.EmberSnake && eventStep > 0 && eventStep <= markedStrikeCount)
        {
            int idx = markedStrikeOrder[eventStep - 1];
            Vector2 head = TileCenter(idx % GridSize, idx / GridSize);
            DrawGlowFast(head, 18f, accent, 0.12f + MathF.Sin(time * 14f) * 0.04f);
        }
    }

    static void DrawOutsideSafeZoneDimming(Rectangle safe, Color dim)
    {
        if (safe.Width <= 0f || safe.Height <= 0f) return;

        if (safe.X <= 1f)
        {
            Raylib.DrawRectangleRec(new Rectangle(safe.Width, 0f, WindowWidth - safe.Width, WindowHeight), dim);
        }
        else if (safe.X + safe.Width >= WindowWidth - 1f)
        {
            Raylib.DrawRectangleRec(new Rectangle(0f, 0f, WindowWidth - safe.Width, WindowHeight), dim);
        }
        else if (safe.Y <= 1f)
        {
            Raylib.DrawRectangleRec(new Rectangle(0f, safe.Height, WindowWidth, WindowHeight - safe.Height), dim);
        }
        else
        {
            Raylib.DrawRectangleRec(new Rectangle(0f, 0f, WindowWidth, WindowHeight - safe.Height), dim);
        }
    }

    static void DrawSafeZoneRushWorldBand()
    {
        if (activeEvent != FloorEventType.SafeZoneRush || eventCountdown <= 0f || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 6f) * 0.5f + 0.5f;
        Color safeEdge = new Color(108, 132, 102, 255);
        Color safeFill = new Color(92, 112, 88, 255);

        Raylib.DrawRectangleRec(eventSafeRect, WithAlpha(safeFill, 0.04f + pulse * 0.02f));
        DrawGradientWash(eventSafeRect, WithAlpha(safeEdge, 0.055f), WithAlpha(safeFill, 0.008f),
            new Vector2(0.5f, 0.5f), 2.8f);

        float edgeThickness = 2f + pulse * 1f;
        Rectangle edge = eventSide switch
        {
            0 => new Rectangle(eventSafeRect.Width - edgeThickness * 0.5f, 0f, edgeThickness, WindowHeight),
            1 => new Rectangle(eventSafeRect.X - edgeThickness * 0.5f, 0f, edgeThickness, WindowHeight),
            2 => new Rectangle(0f, eventSafeRect.Height - edgeThickness * 0.5f, WindowWidth, edgeThickness),
            3 => new Rectangle(0f, eventSafeRect.Y - edgeThickness * 0.5f, WindowWidth, edgeThickness),
            _ => eventSafeRect,
        };
        Raylib.DrawRectangleRec(edge, WithAlpha(safeEdge, 0.22f + pulse * 0.1f));
        DrawGradientWash(edge, WithAlpha(safeEdge, 0.18f), WithAlpha(safeFill, 0f), new Vector2(0.5f, 0.5f), 1.8f);
    }

    static void DrawFloorEventOverlay()
    {
        if (activeEvent == FloorEventType.None || state != GameState.Playing) return;

        float time = (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 8f) * 0.5f + 0.5f;
        Color accent = EventAccentColor(activeEvent);
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 0f;

        if (activeEvent == FloorEventType.SafeZoneRush)
        {
            Color safeEdge = new Color(108, 132, 102, 255);
            Color dangerDim = WithAlpha(ForestShadow, 0.22f + urgency * 0.06f);
            DrawOutsideSafeZoneDimming(eventSafeRect, dangerDim);

            Vector2 beacon = new Vector2(
                eventSafeRect.X + eventSafeRect.Width * 0.5f,
                eventSafeRect.Y + eventSafeRect.Height * 0.5f);

            if (!PlayerInSafeRect(eventSafeRect))
            {
                float dist = Vector2.Distance(playerPos, beacon);
                Raylib.DrawLineEx(playerPos, beacon, 1.2f, WithAlpha(safeEdge, 0.14f + pulse * 0.08f));
                string hint = SafeZoneRushHint() + "  �  " + dist.ToString("0") + "u";
                ShadowTextCentered(hint, WindowWidth / 2, WindowHeight - 58, 13, WithAlpha(safeEdge, 0.72f), 1f);
            }
        }
        else if (activeEvent == FloorEventType.CenterSnare)
        {
            Raylib.DrawRectangleRec(eventDangerRect, WithAlpha(Danger, 0.22f + MathF.Sin(time * 11f) * 0.1f + urgency * 0.12f));
            DrawPulseFrame(eventDangerRect, Danger, 0.04f, 10f, 0.35f + urgency * 0.25f);
            DrawCenterSnareSafeBands(time);

            if (PlayerInCenterSnareSafe())
            {
                DrawGlow(playerPos, PlayerRadius * 2.2f, UiAccent, 0.1f);
            }

            ShadowTextCentered("RUN FOR THE BATTLEMENTS", WindowWidth / 2, WindowHeight - 132, 20, WithAlpha(Danger, 0.95f), 1f);
        }
        else if (activeEvent == FloorEventType.MossRot || floorRotTimer > 0f)
        {
            float half = WindowWidth / 2f;
            var rotRect = floorRotSide < 0.5f
                ? new Rectangle(0f, 0f, half + MathF.Sin(time * 3f) * 6f, WindowHeight)
                : new Rectangle(half - MathF.Sin(time * 3f) * 6f, 0f, half, WindowHeight);
            Raylib.DrawRectangleRec(rotRect, WithAlpha(new Color(72, 88, 48, 255), 0.14f + MathF.Sin(time * 5f) * 0.06f));
            DrawGradientWash(rotRect, WithAlpha(new Color(96, 108, 58, 255), 0.2f), WithAlpha(ForestShadow, 0f),
                floorRotSide < 0.5f ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f), 1.5f);

            for (int i = 0; i < 8; i++)
            {
                float px = floorRotSide < 0.5f ? Hash(i) * half : half + Hash(i) * half;
                float py = (time * 40f + i * 47f) % WindowHeight;
                Raylib.DrawCircleV(new Vector2(px, py), 2.5f + Hash(i + 5), WithAlpha(new Color(108, 128, 72, 255), 0.38f));
            }
        }
        else if (activeEvent is FloorEventType.CryptVeil or FloorEventType.CryptMist or FloorEventType.CryptWail)
        {
            if (activeEvent == FloorEventType.CryptVeil)
            {
                DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.22f + urgency * 0.08f));
                DrawPulseFrame(eventSafeRect, accent, 0.06f, 9f, 0.28f + pulse * 0.22f);
            }
            else if (activeEvent == FloorEventType.CryptMist)
            {
                float half = WindowWidth / 2f;
                var mistRect = floorRotSide < 0.5f
                    ? new Rectangle(0f, 0f, half + MathF.Sin(time * 4f) * 8f, WindowHeight)
                    : new Rectangle(half - MathF.Sin(time * 4f) * 8f, 0f, half, WindowHeight);
                Raylib.DrawRectangleRec(mistRect, WithAlpha(accent, 0.12f + MathF.Sin(time * 5f) * 0.05f));
            }
            else
            {
                float wavePos = eventSide switch
                {
                    0 => (1f - eventCountdown / Math.Max(eventStartCountdown, 0.01f)) * WindowWidth,
                    1 => eventCountdown / Math.Max(eventStartCountdown, 0.01f) * WindowWidth,
                    2 => (1f - eventCountdown / Math.Max(eventStartCountdown, 0.01f)) * WindowHeight,
                    _ => eventCountdown / Math.Max(eventStartCountdown, 0.01f) * WindowHeight,
                };
                var wailBand = eventSide < 2
                    ? new Rectangle(wavePos - 24f, 0f, 48f, WindowHeight)
                    : new Rectangle(0f, wavePos - 24f, WindowWidth, 48f);
                Raylib.DrawRectangleRec(wailBand, WithAlpha(accent, 0.14f + pulse * 0.1f));
            }
        }
        else if (activeEvent is FloorEventType.CrimsonCrumble or FloorEventType.Checkerfall
            or FloorEventType.RingCollapse or FloorEventType.StoneIslands or FloorEventType.ScatterPits
            or FloorEventType.MarkedStrike or FloorEventType.BlightStorm
            or FloorEventType.TideSurge or FloorEventType.TideRift or FloorEventType.TideRecede
            or FloorEventType.TideColumn or FloorEventType.TideEcho or FloorEventType.TideUndertow
            or FloorEventType.TideCrest or FloorEventType.TideWall or FloorEventType.TideAnchor
            or FloorEventType.TideFoam or FloorEventType.TideStrike
            or FloorEventType.EmberRain or FloorEventType.EmberPulse or FloorEventType.EmberCross
            or FloorEventType.EmberBridge or FloorEventType.EmberFury or FloorEventType.EmberSnake
            or FloorEventType.EmberHive or FloorEventType.EmberQuake or FloorEventType.EmberBloom
            or FloorEventType.CryptSeal or FloorEventType.CryptTorch or FloorEventType.CryptChains
            or FloorEventType.CryptTomb or FloorEventType.CryptShroud or FloorEventType.CryptGlimpse
            or FloorEventType.CryptRattle or FloorEventType.CryptEcho or FloorEventType.CryptLantern)
        {
            float tint = 0.06f + urgency * 0.1f;
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(accent, tint * (0.7f + MathF.Sin(time * 6f) * 0.3f)));
        }
        else if (activeEvent is FloorEventType.CrownTrial or FloorEventType.CrownFall
            or FloorEventType.CrownShard or FloorEventType.CrownRing or FloorEventType.CrownUsurpation
            or FloorEventType.CrownReckoning or FloorEventType.CrownBolt)
        {
            float tint = 0.06f + urgency * 0.1f;
            Raylib.DrawRectangle(0, 0, WindowWidth, WindowHeight, WithAlpha(accent, tint * (0.7f + MathF.Sin(time * 6f) * 0.3f)));
        }
        else if (activeEvent == FloorEventType.CrownEdict)
        {
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.22f + urgency * 0.06f));
            DrawPulseFrame(eventSafeRect, accent, 0.06f, 8f, 0.28f + pulse * 0.18f);
        }
        else if (activeEvent == FloorEventType.CrownThrone)
        {
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.2f + urgency * 0.08f));
            DrawGlow(new Vector2(WindowWidth / 2f, WindowHeight / 2f), 32f + pulse * 16f, accent, 0.1f);
            DrawPulseFrame(eventSafeRect, accent, 0.08f, 10f, 0.32f + urgency * 0.2f);
        }
        else if (activeEvent == FloorEventType.CrownRot || (floorRotTimer > 0f && activeEvent == FloorEventType.CrownRot))
        {
            float half = WindowWidth / 2f;
            var rotRect = floorRotSide < 0.5f
                ? new Rectangle(0f, 0f, half + MathF.Sin(time * 3f) * 6f, WindowHeight)
                : new Rectangle(half - MathF.Sin(time * 3f) * 6f, 0f, half, WindowHeight);
            Raylib.DrawRectangleRec(rotRect, WithAlpha(new Color(98, 118, 62, 255), 0.14f + MathF.Sin(time * 5f) * 0.06f));
        }
        else if (activeEvent == FloorEventType.EmberGate)
        {
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.2f + urgency * 0.08f));
            DrawPulseFrame(eventSafeRect, accent, 0.06f, 8f, 0.3f + pulse * 0.2f);
        }
        else if (activeEvent == FloorEventType.EmberTide)
        {
            Raylib.DrawRectangleRec(eventDangerRect, WithAlpha(accent, 0.18f + urgency * 0.14f));
            DrawPulseFrame(eventSafeRect, accent, 0.04f, 6f, 0.25f + urgency * 0.2f);
        }
        else if (activeEvent == FloorEventType.EmberCage)
        {
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(accent, 0.14f + urgency * 0.1f));
            DrawPulseFrame(eventSafeRect, accent, 0.08f, 10f, 0.35f + urgency * 0.25f);
        }
        else if (activeEvent == FloorEventType.EmberAltar)
        {
            DrawGlow(eventEpicenter, 28f + pulse * 12f, accent, 0.1f + urgency * 0.08f);
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.24f + urgency * 0.06f));
        }
        else if (activeEvent == FloorEventType.TideWall)
        {
            float half = WindowWidth / 2f;
            var wallRect = eventSide switch
            {
                0 => new Rectangle(0f, 0f, half, WindowHeight),
                1 => new Rectangle(half, 0f, half, WindowHeight),
                2 => new Rectangle(0f, 0f, WindowWidth, half),
                _ => new Rectangle(0f, half, WindowWidth, half),
            };
            Raylib.DrawRectangleRec(wallRect, WithAlpha(accent, 0.12f + pulse * 0.06f));
        }
        else if (activeEvent == FloorEventType.TideBeacon)
        {
            DrawOutsideSafeZoneDimming(eventSafeRect, WithAlpha(ForestShadow, 0.2f + urgency * 0.06f));
            DrawPulseFrame(eventSafeRect, accent, 0.06f, 8f, 0.28f + pulse * 0.18f);
        }
        else if (activeEvent == FloorEventType.TideWhirlpool)
        {
            Raylib.DrawRectangleRec(eventDangerRect, WithAlpha(accent, 0.2f + pulse * 0.12f + urgency * 0.1f));
            DrawPulseFrame(eventDangerRect, accent, 0.05f, 9f, 0.32f + urgency * 0.2f);
        }
    }

    static void DrawRushArrow(Vector2 pos, float degrees, Color color, float scale = 1f, bool lite = false)
    {
        float rad = degrees * MathF.PI / 180f;
        Vector2 dir = new Vector2(MathF.Cos(rad), MathF.Sin(rad));
        Vector2 side = new Vector2(-dir.Y, dir.X);
        var tip = pos + dir * (14f * scale);
        var left = pos - dir * (8f * scale) + side * (7f * scale);
        var right = pos - dir * (8f * scale) - side * (7f * scale);
        Raylib.DrawTriangle(tip, left, right, WithAlpha(color, 0.95f));
        if (!lite)
        {
            DrawGlow(pos, 10f * scale, color, 0.08f);
        }
        Raylib.DrawCircleV(pos, 3f * scale, WithAlpha(Color.White, 0.82f));
    }

    static void DrawFloorEventTileWarnings()
    {
        if (activeEvent == FloorEventType.None || state != GameState.Playing) return;
        if (eventCountdown <= 0f && eventPhase == 0
            && activeEvent is not (FloorEventType.MarkedStrike or FloorEventType.CryptGrave or FloorEventType.TideStrike or FloorEventType.CrownBolt)) return;

        float time = frameTime > 0f ? frameTime : (float)Raylib.GetTime();
        float pulse = MathF.Sin(time * 14f) * 0.5f + 0.5f;
        float urgency = eventStartCountdown > 0.01f
            ? 1f - Math.Clamp(eventCountdown / eventStartCountdown, 0f, 1f)
            : 0.5f;
        Color accent = EventAccentColor(activeEvent);

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (tiles[x, y].State == 2) continue;

                var r = new Rectangle(x * TileSize + 1f, y * TileSize + 1f, TileSize - 2f, TileSize - 2f);
                Vector2 center = TileCenter(x, y);

                if (tiles[x, y].EventMarked)
                {
                    float distEpic = Vector2.Distance(center, eventEpicenter);
                    float wave = activeEvent == FloorEventType.CrimsonCrumble
                        ? MathF.Sin(time * 8f - distEpic * 0.08f) * 0.5f + 0.5f
                        : pulse;

                    Color warn = activeEvent is FloorEventType.BlightStorm or FloorEventType.TideFoam
                        or FloorEventType.CrownStorm
                        ? LerpColor(accent, Danger, pulse)
                        : accent;

                    Raylib.DrawRectangleRec(r, WithAlpha(warn, 0.2f + wave * 0.36f + urgency * 0.14f));
                    DrawPulseFrameLite(r, warn, 0.1f, 14f, 0.28f + wave * 0.38f);

                    if (activeEvent == FloorEventType.Checkerfall)
                    {
                        float diag = (x + y) * 0.3f;
                        Raylib.DrawLineEx(
                            new Vector2(r.X + 4f, r.Y + 4f),
                            new Vector2(r.X + r.Width - 4f, r.Y + r.Height - 4f),
                            1.5f, WithAlpha(warn, 0.35f + MathF.Sin(time * 10f + diag) * 0.2f));
                    }

                    if (activeEvent == FloorEventType.RingCollapse)
                    {
                        int edge = TileEdgeDistance(x, y);
                        Raylib.DrawRectangleRoundedLines(r, 0.1f, 4, WithAlpha(warn, 0.3f + edge * 0.08f));
                    }

                    if (activeEvent == FloorEventType.ScatterPits)
                    {
                        Raylib.DrawCircleV(center, 4f + pulse * 3f, WithAlpha(warn, 0.4f));
                    }

                    if (activeEvent is FloorEventType.MarkedStrike or FloorEventType.CryptGrave or FloorEventType.TideStrike or FloorEventType.CrownBolt && eventStep < markedStrikeCount)
                    {
                        int idx = markedStrikeOrder[eventStep];
                        if (idx % GridSize == x && idx / GridSize == y)
                        {
                            Color hot = activeEvent switch
                            {
                                FloorEventType.CryptGrave => new Color(220, 210, 240, 255),
                                FloorEventType.CrownBolt => new Color(255, 248, 220, 255),
                                _ => Color.White,
                            };
                            DrawPulseFrame(r, hot, 0.14f, 18f, 0.5f + pulse * 0.4f);
                            Raylib.DrawCircleV(center, 6f + pulse * 4f, WithAlpha(hot, 0.55f));
                        }
                    }

                    if (activeEvent == FloorEventType.CrownFall)
                    {
                        float diag = (x + y) * 0.3f;
                        Raylib.DrawLineEx(
                            new Vector2(r.X + 4f, r.Y + 4f),
                            new Vector2(r.X + r.Width - 4f, r.Y + r.Height - 4f),
                            1.5f, WithAlpha(warn, 0.35f + MathF.Sin(time * 10f + diag) * 0.2f));
                    }

                    if (activeEvent == FloorEventType.CrownShard)
                    {
                        Raylib.DrawCircleV(center, 4f + pulse * 3f, WithAlpha(warn, 0.4f));
                    }

                    if (activeEvent == FloorEventType.CrownUsurpation && (x == GridSize / 2 || y == GridSize / 2))
                    {
                        Raylib.DrawRectangleRoundedLines(r, 0.12f, 4, WithAlpha(warn, 0.42f + pulse * 0.2f));
                    }

                    if (activeEvent == FloorEventType.CryptShroud)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(r.X + 2f, r.Y + 2f),
                            new Vector2(r.X + r.Width - 2f, r.Y + r.Height - 2f),
                            1.2f, WithAlpha(warn, 0.35f + pulse * 0.2f));
                    }

                    if (activeEvent == FloorEventType.CryptTomb)
                    {
                        Raylib.DrawCircleLinesV(center, 8f + pulse * 4f, WithAlpha(warn, 0.55f));
                    }

                    if (activeEvent == FloorEventType.TideColumn)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(center.X, r.Y + 2f),
                            new Vector2(center.X, r.Y + r.Height - 2f),
                            1.5f, WithAlpha(warn, 0.4f + pulse * 0.2f));
                    }

                    if (activeEvent == FloorEventType.TideEcho)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(r.X + 4f, r.Y + 4f),
                            new Vector2(r.X + r.Width - 4f, r.Y + r.Height - 4f),
                            1.2f, WithAlpha(warn, 0.35f + pulse * 0.15f));
                    }

                    if (activeEvent == FloorEventType.TideRift)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(r.X + r.Width * 0.5f, r.Y + 2f),
                            new Vector2(r.X + r.Width * 0.5f, r.Y + r.Height - 2f),
                            1.2f, WithAlpha(warn, 0.35f));
                        Raylib.DrawLineEx(
                            new Vector2(r.X + 2f, r.Y + r.Height * 0.5f),
                            new Vector2(r.X + r.Width - 2f, r.Y + r.Height * 0.5f),
                            1.2f, WithAlpha(warn, 0.35f));
                    }

                    if (activeEvent == FloorEventType.EmberCross)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(r.X + r.Width * 0.5f, r.Y + 2f),
                            new Vector2(r.X + r.Width * 0.5f, r.Y + r.Height - 2f),
                            1.5f, WithAlpha(warn, 0.4f + pulse * 0.2f));
                    }

                    if (activeEvent == FloorEventType.EmberFury)
                    {
                        Raylib.DrawCircleV(center, 3f + pulse * 2f, WithAlpha(warn, 0.5f));
                    }
                }
                else if (activeEvent is FloorEventType.StoneIslands or FloorEventType.TideAnchor or FloorEventType.EmberHive or FloorEventType.EmberBridge
                    or FloorEventType.CrownIsles or FloorEventType.CryptTorch or FloorEventType.CryptLantern)
                {
                    Color safeGlow = activeEvent is FloorEventType.EmberHive or FloorEventType.CryptLantern
                        ? accent
                        : UiAccent;
                    Raylib.DrawRectangleRec(r, WithAlpha(safeGlow, 0.12f + pulse * 0.16f));
                    DrawPulseFrameLite(r, safeGlow, 0.08f, 10f, 0.18f + pulse * 0.24f);
                }
                else if (activeEvent == FloorEventType.CryptGlimpse && eventCountdown > 0f)
                {
                    Raylib.DrawRectangleRec(r, WithAlpha(UiAccent, 0.08f + pulse * 0.14f));
                    DrawPulseFrameLite(r, UiAccent, 0.06f, 12f, 0.15f + pulse * 0.2f);
                }
            }
        }

        if (eventPhase == 1 && StormGlassPeekCount() > 0 && eventTileQueue.Count > 0)
        {
            int peek = 0;
            Color glass = new Color(140, 200, 255, 255);
            foreach ((int gx, int gy) in eventTileQueue)
            {
                if (peek >= StormGlassPeekCount()) break;
                var gr = new Rectangle(gx * TileSize + 1f, gy * TileSize + 1f, TileSize - 2f, TileSize - 2f);
                DrawPulseFrame(gr, glass, 0.2f, 10f, 0.45f + pulse * 0.3f);
                Raylib.DrawRectangleRoundedLines(gr, 0.15f, 4, WithAlpha(glass, 0.85f));
                peek++;
            }
        }
    }

    static string FormatEventCountdown(float seconds)
    {
        if (seconds <= 0.05f) return "0.0";
        return seconds.ToString("0.0", CultureInfo.InvariantCulture);
    }

    static (string cause, string tip) GetDeathCauseCopy()
    {
        return lastDeathCause switch
        {
            DeathCause.FellThrough => (
                "The void swallowed you.",
                "Tip: Stay on solid tiles. Wind Step can leap gaps in a pinch."),
            DeathCause.FloorGaveWay => (
                "The floor collapsed beneath you.",
                "Tip: Keep moving-worn tiles crumble. Seek fresh, sturdy ground."),
            DeathCause.EnemyGrasp => (
                string.IsNullOrEmpty(lastDeathDetail)
                    ? "A foe caught you."
                    : lastDeathDetail + " struck you down.",
                "Tip: Wind Step grants brief invulnerability-dash through danger."),
            DeathCause.SafeZoneFailed => (
                "You missed the safe zone.",
                "Tip: Watch the glowing band and reach it before the timer hits zero."),
            DeathCause.CenterSnare => (
                "The keep's heart gave way.",
                "Tip: When the rim glows safe, sprint for the outer wall."),
            DeathCause.TideDrowned => (
                "The whirlpool dragged you under.",
                "Tip: Flee the shrinking center before the tide closes."),
            DeathCause.TideBeaconLost => (
                "You never reached the tide beacon.",
                "Tip: Sprint for the glowing corner shrine before the flood."),
            _ => (
                "The stones took you.",
                "Tip: Move often, read the cracks, and save your dash for panic."),
        };
    }

    static void DrawTileAttackTelegraph(int tx, int ty, float intensity, float time, string label, Color color)
    {
        if (tx < 0 || tx >= GridSize || ty < 0 || ty >= GridSize) return;
        if (tiles[tx, ty].State == 2) return;

        var r = new Rectangle(tx * TileSize + 1f, ty * TileSize + 1f, TileSize - 2f, TileSize - 2f);
        float pulse = MathF.Sin(time * 16f) * 0.5f + 0.5f;
        float alpha = 0.28f + intensity * 0.52f + pulse * 0.22f;
        Raylib.DrawRectangleRec(r, WithAlpha(color, alpha));
        DrawPulseFrame(r, color, 0.14f, 14f, 0.3f + intensity * 0.5f);

        Vector2 a = new Vector2(r.X + 4f, r.Y + 4f);
        Vector2 b = new Vector2(r.X + r.Width - 4f, r.Y + r.Height - 4f);
        Vector2 c = new Vector2(r.X + r.Width - 4f, r.Y + 4f);
        Vector2 d = new Vector2(r.X + 4f, r.Y + r.Height - 4f);
        Raylib.DrawLineEx(a, b, 2.8f, WithAlpha(color, 0.75f + pulse * 0.2f));
        Raylib.DrawLineEx(c, d, 2.8f, WithAlpha(color, 0.75f + pulse * 0.2f));

        if (label.Length > 0)
        {
            ShadowTextCentered(label, (int)(r.X + r.Width / 2f), (int)(r.Y - 8f), 11, WithAlpha(color, 0.95f), 1f);
        }
    }

    static void DrawEnemyAttackTelegraphs()
    {
        if (!showEnemyTelegraphs) return;
        float time = (float)Raylib.GetTime();
        float touchDist = PlayerRadius;

        foreach (Enemy e in enemies)
        {
            if (e.Dead || e.Spawn < 1f) continue;
            ref readonly EnemyDef def = ref GetDef(e.Type);
            float dist = Vector2.Distance(playerPos, e.Position);
            float pulse = MathF.Sin(time * 14f + e.Wobble) * 0.5f + 0.5f;

            if (def.Behavior == EnemyBehavior.BossSmash)
            {
                DrawGroveTitanTelegraphs(e, time);
                continue;
            }

            // --- Tile crush (Rootbeast, Moss Golem) ---
            if (def.Behavior == EnemyBehavior.CrushTiles && e.AbilityStep == 1 && e.AbilityTimer > 0f)
            {
                float intensity = 1f - e.AbilityTimer / CrushTelegraphTime;
                DrawTileAttackTelegraph(e.StrikeTx, e.StrikeTy, intensity, time, "BREAK!", Danger);
                DrawGlow(TileCenter(e.StrikeTx, e.StrikeTy), 34f, Danger, 0.2f + intensity * 0.45f);
                ShadowTextCentered("STONE BREAK", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 34f), 13, WithAlpha(Danger, 0.85f + intensity * 0.15f), 1f);
                continue;
            }

            // --- Boss blight / dash ---
            if (def.Boss)
            {
                if (def.Behavior == EnemyBehavior.BossBlight && e.AbilityTimer > 0f && e.AbilityTimer <= EnemyTelegraphTime)
                {
                    float t = 1f - e.AbilityTimer / EnemyTelegraphTime;
                    Color warn = WithAlpha(Danger, 0.35f + t * 0.5f + pulse * 0.2f);
                    DrawGlow(e.Position, TileSize * 2.6f, Danger, 0.22f + t * 0.35f);
                    Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, TileSize * 2f * (0.88f + t * 0.18f), warn);
                    ShadowTextCentered("BLIGHT!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 36f), 14, warn, 1f);
                    continue;
                }

                if (def.Behavior == EnemyBehavior.BossDash && e.AbilityStep == 1 && e.AbilityTimer > 0f)
                {
                    float t = 1f - e.AbilityTimer / BossTelegraphTime;
                    Color warn = WithAlpha(Danger, 0.35f + t * 0.5f + pulse * 0.2f);
                    Vector2 toP = playerPos - e.Position;
                    if (toP.LengthSquared() > 1f)
                    {
                        Vector2 dir = Vector2.Normalize(toP);
                        float len = MathF.Min(toP.Length(), 320f);
                        Vector2 end = e.Position + dir * len;
                        Raylib.DrawLineEx(e.Position, end, 5f + t * 6f, warn);
                        DrawGlow(end, 32f, Danger, 0.3f + t * 0.4f);
                    }
                    ShadowTextCentered("DASH!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 36f), 14, warn, 1f);
                    continue;
                }
            }

            // --- Charge wind-up / active ---
            if (def.Behavior == EnemyBehavior.Charge)
            {
                if (e.AbilityStep == 2 && e.AbilityTimer > 0f)
                {
                    float t = 1f - e.AbilityTimer / ChargeTelegraphTime;
                    Color warn = WithAlpha(Danger, 0.4f + t * 0.45f + pulse * 0.2f);
                    Vector2 toP = playerPos - e.Position;
                    if (toP.LengthSquared() > 1f)
                    {
                        Vector2 dir = Vector2.Normalize(toP);
                        Vector2 end = e.Position + dir * MathF.Min(toP.Length(), 220f);
                        Raylib.DrawLineEx(e.Position, end, 4f + t * 4f, warn);
                        DrawGlow(end, 24f, Danger, 0.25f + t * 0.35f);
                    }
                    Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, e.Radius + 8f + t * 10f, warn);
                    ShadowTextCentered("CHARGE!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 30f), 12, warn, 1f);
                    continue;
                }
                if (e.AbilityStep == 3 && e.Phase > 0f)
                {
                    Color warn = WithAlpha(Danger, 0.65f + pulse * 0.25f);
                    Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, e.Radius + 6f, warn);
                    ShadowTextCentered("RUSHING", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 28f), 11, warn, 0.9f);
                    continue;
                }
            }

            // --- Hop / blink destination ---
            if (def.Behavior == EnemyBehavior.Hop && e.AbilityStep == 1)
            {
                float t = 1f - e.AbilityTimer / 0.42f;
                DrawTileAttackTelegraph(e.StrikeTx, e.StrikeTy, t, time, "LEAP!", new Color(196, 148, 88, 255));
                Vector2 dest = TileCenter(e.StrikeTx, e.StrikeTy);
                Raylib.DrawLineEx(e.Position, dest, 3f, WithAlpha(new Color(196, 148, 88, 255), 0.5f + t * 0.4f));
                DrawGlow(dest, 22f, new Color(196, 148, 88, 255), 0.3f + t * 0.35f);
                continue;
            }

            if (def.Behavior == EnemyBehavior.Phaser && e.AbilityStep == 1)
            {
                float t = 1f - e.AbilityTimer / 0.48f;
                DrawTileAttackTelegraph(e.StrikeTx, e.StrikeTy, t, time, "BLINK!", new Color(120, 188, 198, 255));
                Vector2 dest = TileCenter(e.StrikeTx, e.StrikeTy);
                DrawGlow(dest, 26f, new Color(120, 188, 198, 255), 0.35f + t * 0.4f);
                ShadowTextCentered("BLINK!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 28f), 11, WithAlpha(new Color(120, 188, 198, 255), 0.9f), 1f);
                continue;
            }

            // --- Blight pulse ---
            if ((def.Behavior == EnemyBehavior.PulseBlight || def.Behavior == EnemyBehavior.BossBlight)
                && e.AbilityTimer > 0f && e.AbilityTimer <= EnemyTelegraphTime)
            {
                float t = 1f - e.AbilityTimer / EnemyTelegraphTime;
                Color warn = WithAlpha(ActiveTheme.BlightSick, 0.35f + t * 0.45f + pulse * 0.15f);
                float rad = TileSize * (def.Boss ? 2.2f : 1.35f) * (0.85f + t * 0.2f);
                DrawGlow(e.Position, rad, ActiveTheme.BlightSick, 0.2f + t * 0.3f);
                Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, rad, warn);
                ShadowTextCentered("BLIGHT!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 28f), 11, warn, 0.95f);
                continue;
            }

            // --- Blight trail drop ---
            if (def.Behavior == EnemyBehavior.BlightTrail && e.AbilityTimer > 0f && e.AbilityTimer <= 0.32f
                && TryGetTileUnder(e.Position, out int btx, out int bty))
            {
                float t = 1f - e.AbilityTimer / 0.32f;
                DrawTileAttackTelegraph(btx, bty, t, time, "ROT", ActiveTheme.BlightSick);
                continue;
            }

            // --- Sapper aura (3x3) ---
            if (def.Behavior == EnemyBehavior.Sapper && TryGetTileUnder(e.Position, out int sx, out int sy))
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        DrawTileAttackTelegraph(sx + i, sy + j, 0.35f + pulse * 0.25f, time, i == 0 && j == 0 ? "SAP" : "", Danger);
                    }
                }
                ShadowTextCentered("SAP!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 26f), 10, WithAlpha(Danger, 0.75f), 0.9f);
                continue;
            }

            // --- Tile leech / decay aura ---
            if (def.Behavior == EnemyBehavior.TileLeech && TryGetTileUnder(e.Position, out int ltx, out int lty))
            {
                DrawTileAttackTelegraph(ltx, lty, 0.4f + pulse * 0.2f, time, "LEECH", new Color(140, 88, 108, 255));
            }

            // --- Contact threat (chase, fast, lurker close, orbit near) ---
            bool contactThreat = def.Behavior is EnemyBehavior.Chase or EnemyBehavior.FastChase
                or EnemyBehavior.Lurker or EnemyBehavior.Zigzag or EnemyBehavior.Orbit
                or EnemyBehavior.Kite;
            if (contactThreat || def.TileDecay > 0f && !def.InstaCollapse && def.Behavior != EnemyBehavior.CrushTiles)
            {
                float warnR = (touchDist + e.Radius) * ContactWarnMul;
                if (dist <= warnR)
                {
                    float t = 1f - dist / warnR;
                    Color warn = WithAlpha(Danger, 0.25f + t * 0.45f + pulse * 0.15f);
                    Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, e.Radius + 6f + t * 8f, warn);
                    if (dist <= touchDist + e.Radius + 6f)
                    {
                        ShadowTextCentered("!", (int)e.Position.X, (int)(e.Position.Y - e.Radius - 22f), 16, warn, 1f);
                    }
                }
            }
        }
    }

    static void DrawGroveTitanTelegraphs(Enemy e, float time)
    {
        if (e.AbilityStep <= 0 || e.AbilityTimer <= 0f) return;

        float intensity = 1f - e.AbilityTimer / BossCutTelegraphTime;
        float pulse = MathF.Sin(time * 14f) * 0.5f + 0.5f;
        Color warn = WithAlpha(Danger, 0.35f + intensity * 0.45f + pulse * 0.2f);

        switch (e.AbilityStep)
        {
            case 1:
                DrawBossCutLaneTelegraph(e.StrikeTx, e.StrikeTy, 0, 1, e.StrikeDepth, time, intensity);
                ShadowTextCentered("CUT BELOW", WindowWidth / 2, 52, 16, warn, 0.9f);
                break;
            case 2:
                DrawBossCutLaneTelegraph(e.StrikeTx, e.StrikeTy, 0, -1, e.StrikeDepth, time, intensity);
                ShadowTextCentered("CUT ABOVE", WindowWidth / 2, 52, 16, warn, 0.9f);
                break;
            case 3:
                DrawBossCutLaneTelegraph(e.StrikeTx, e.StrikeTy, -1, 0, e.StrikeDepth, time, intensity);
                DrawBossCutLaneTelegraph(e.StrikeTx, e.StrikeTy, 1, 0, e.StrikeDepth, time, intensity);
                ShadowTextCentered("CUT SIDES", WindowWidth / 2, 52, 16, warn, 0.9f);
                break;
            default:
                break;
        }
    }

    static string GroveTitanHudWarning(Enemy b)
    {
        if (b.AbilityStep == 1) return "CUT BELOW INCOMING";
        if (b.AbilityStep == 2) return "CUT ABOVE INCOMING";
        if (b.AbilityStep == 3) return "CUT SIDES INCOMING";
        if (b.AbilityTimer > 0f && b.AbilityTimer <= BossTelegraphTime) return "STONE CUT INCOMING";
        return "";
    }

    static void DrawBossHud()
    {
        if (!showBossHud) return;
        if (state != GameState.Playing) return;

        Enemy? bossEnemy = null;
        foreach (Enemy e in enemies)
        {
            if (!e.Dead && e.Spawn >= 0.8f && IsBoss(e.Type))
            {
                if (bossEnemy == null || e.MaxHp > bossEnemy.MaxHp) bossEnemy = e;
            }
        }

        if (bossEnemy == null) return;

        Enemy b = bossEnemy;
        int cx = WindowWidth / 2;
        float w = 420f;
        float hpf = Math.Clamp(b.Hp / b.MaxHp, 0f, 1f);
        var panel = new Rectangle(cx - w / 2f - 16f, 58f, w + 32f, 48f);
        DrawRichPanel(panel, WithAlpha(UiPanel, 0.9f), Danger, 0.2f, accentStripe: true);
        DrawPulseFrame(panel, Danger, 0.16f, 6f, 0.15f);

        string label = GetEnemyName(b.Type).ToUpperInvariant();
        ShadowTextCentered(label, cx, 66, 16, Gold, 1f);

        string warnText = GetDef(b.Type).Behavior == EnemyBehavior.BossSmash
            ? GroveTitanHudWarning(b)
            : GetDef(b.Type).Behavior == EnemyBehavior.BossDash && b.AbilityStep == 1 && b.AbilityTimer > 0f
                ? "DASH INCOMING"
            : b.AbilityTimer > 0f && b.AbilityTimer <= EnemyTelegraphTime
                ? GetDef(b.Type).Behavior switch
                {
                    EnemyBehavior.BossBlight => "BLIGHT INCOMING",
                    EnemyBehavior.BossDash => "DASH INCOMING",
                    _ => "ATTACK INCOMING",
                }
                : "";

        if (warnText.Length > 0)
        {
            float pulse = MathF.Sin((float)Raylib.GetTime() * 12f) * 0.5f + 0.5f;
            ShadowTextCentered(warnText, cx, 84, 13, WithAlpha(Danger, 0.7f + pulse * 0.3f), 1f);
        }
        else
        {
            ShadowTextCentered(GetEnemyDesc(b.Type), cx, 84, 11, WithAlpha(Color.White, 0.55f), 1f);
        }

        var barBg = new Rectangle(cx - w / 2f, 96f, w, 10f);
        Raylib.DrawRectangleRounded(barBg, 0.8f, 4, WithAlpha(ForestShadow, 0.7f));
        var barFill = new Rectangle(barBg.X, barBg.Y, w * hpf, 10f);
        if (barFill.Width > 1f)
        {
            Raylib.DrawRectangleRounded(barFill, 0.8f, 4, LerpColor(Danger, Gold, hpf));
        }
    }

    static void DrawAbilityHud()
    {
        if (!showAbilityHud) return;
        const int barW = 196;
        const int barH = 13;
        const int gap = 12;
        int totalW = barW * 2 + gap;
        int panelPad = 12;
        int barY = WindowHeight - 30;
        int labelY = barY - 26;
        int panelY = labelY - 8;
        int panelH = barY + barH + 10 - panelY;
        int panelX = WindowWidth - (totalW + panelPad * 2) - 14;
        float time = (float)Raylib.GetTime();

        var panel = new Rectangle(panelX, panelY, totalW + panelPad * 2, panelH);
        DrawRichPanel(panel, UiPanel, UiBorder, 0.32f);

        int slot1Cx = panelX + panelPad + barW / 2;
        int slot2Cx = slot1Cx + barW + gap;
        DrawEquippedAbilityBar(0, abilitySlot1, abilityKey1, slot1Cx, labelY, barY, barW, barH, time);
        DrawEquippedAbilityBar(1, abilitySlot2, abilityKey2, slot2Cx, labelY, barY, barW, barH, time);
    }

    static void DrawEquippedAbilityBar(int slot, AbilityType ability, KeyboardKey key, int cx, int labelY, int barY,
        int barW, int barH, float time)
    {
        const int iconPad = 22;
        SyncAbilitySlotHud(slot);
        ability = slot == 0 ? abilitySlot1 : abilitySlot2;

        float readiness = AbilityReadiness(ability);
        abilityFillVis[slot] = Approach(abilityFillVis[slot], readiness, 14f, Raylib.GetFrameTime());
        bool ready = readiness >= 0.995f;
        Color accent = AbilityAccent(ability);
        string name = AbilityHudName[(int)ability];
        string keyLabel = "[" + KeyName(key) + "]";
        Color? fillTint = ability == AbilityType.WindStep ? BodyColor() : null;

        Vector2 iconPos = new Vector2(cx - barW / 2f + 14f, barY + barH / 2f);
        var iconBg = new Rectangle(iconPos.X - 11f, iconPos.Y - 11f, 22f, 22f);
        Raylib.DrawRectangleRounded(iconBg, 0.35f, 4, WithAlpha(UiPanelDeep, 0.85f));
        DrawAbilityIcon(ability, iconPos, 0.72f, time, ready ? 1f : 0.72f);

        int textCx = cx + iconPad / 2;
        ShadowTextCentered(name, textCx, labelY, 10, ready ? Color.White : WithAlpha(Color.White, 0.72f), ready ? 1f : 0.9f);

        string subLabel = keyLabel;
        if (ability == AbilityType.OathOfTheBailey && oathUsedThisRun) subLabel = "SPENT";
        else if (ability == AbilityType.Verdict && verdictHaltTimer > 0f) subLabel = "HALTING " + Math.Ceiling(verdictHaltTimer).ToString("0") + "s";
        else if (ability == AbilityType.Verdict && !IsVerdictUnlocked()) subLabel = runKillCount + "/" + VerdictUnlockKills + " KILLS";
        else if (!ready) subLabel = keyLabel + "  �  COOLDOWN";

        Color subCol = ability == AbilityType.OathOfTheBailey && oathUsedThisRun
            ? WithAlpha(Color.White, 0.42f)
            : WithAlpha(accent, ready ? 0.95f : 0.62f);
        ShadowTextCentered(subLabel, textCx, labelY + 12, 8, subCol, 0.9f);

        int barX = cx - barW / 2 + iconPad;
        int innerW = barW - iconPad;
        var track = new Rectangle(barX, barY, innerW, barH);
        Raylib.DrawRectangleRounded(track, 0.5f, 6, WithAlpha(UiPanelDeep, 0.92f));

        var fill = new Rectangle(barX + 2f, barY + 2f, (innerW - 4f) * abilityFillVis[slot], barH - 4f);
        Color baseFill = fillTint ?? accent;
        Color fillColor = ready
            ? Lighten(baseFill, (MathF.Sin(time * 5f) * 0.5f + 0.5f) * 0.22f)
            : LerpColor(new Color(48, 52, 58, 255), baseFill, abilityFillVis[slot]);
        if (fill.Width > 1f) Raylib.DrawRectangleRounded(fill, 0.5f, 6, fillColor);
        if (ready) DrawPulseFrame(track, accent, 0.45f, 5f, 0.12f);
    }

    static void DrawAbilityBar(int cx, int labelY, int barY, int barW, int barH, float fillVis, bool ready,
        string label, string readyLabel, Color accent, float time, float pulseSpeed, Color? fillTint = null)
    {
        int barX = cx - barW / 2;
        var track = new Rectangle(barX, barY, barW, barH);
        Raylib.DrawRectangleRounded(track, 0.5f, 6, WithAlpha(UiPanelDeep, 0.9f));

        var fill = new Rectangle(barX + 2f, barY + 2f, (barW - 4f) * fillVis, barH - 4f);
        Color baseFill = fillTint ?? accent;
        Color fillColor = ready
            ? Lighten(baseFill, (MathF.Sin(time * pulseSpeed) * 0.5f + 0.5f) * 0.24f)
            : LerpColor(new Color(48, 52, 58, 255), baseFill, fillVis);
        if (fill.Width > 1f) Raylib.DrawRectangleRounded(fill, 0.5f, 6, fillColor);
        if (ready)
        {
            DrawPulseFrame(track, accent, 0.5f, pulseSpeed, 0.14f);
        }

        Color labelColor = ready ? Color.White : WithAlpha(Color.White, 0.65f);
        ShadowTextCentered(ready ? readyLabel : label, cx, labelY, 11, labelColor, ready ? 1f : 0.85f);
    }

    static void DrawWeaponHud()
    {
        if (!showWeaponHud) return;
        ref readonly Gun g = ref Guns[equippedGun];
        int mag = GetMagazineSize(in g);
        bool reloading = reloadTimer > 0f;
        const float panelH = 64f;
        var panel = new Rectangle(14f, WindowHeight - panelH - 14f, 278f, panelH);
        DrawRichPanel(panel, UiPanel, UiBorder, 0.28f, accentStripe: true);

        var iconBg = new Rectangle(panel.X + 10f, panel.Y + 10f, 32f, 32f);
        Raylib.DrawRectangleRounded(iconBg, 0.3f, 4, WithAlpha(UiPanelDeep, 0.8f));
        DrawGunIcon(equippedGun, new Vector2(iconBg.X + 16f, iconBg.Y + 16f), 13f, (float)Raylib.GetTime());

        Raylib.DrawText(g.Name, (int)panel.X + 50, (int)panel.Y + 8, 15, Color.White);

        string ammoText = reloading
            ? "RELOADING " + Math.Ceiling(reloadTimer).ToString("0") + "s"
            : ammoInMag + " / " + mag;
        Color ammoCol = reloading ? Gold : ammoInMag <= mag / 4 ? Danger : WithAlpha(UiAccent, 0.95f);
        Raylib.DrawText(ammoText, (int)panel.X + 50, (int)panel.Y + 26, 12, ammoCol);

        string mode = autoFire ? "AUTO-FIRE" : "LMB � RMB reload";
        Raylib.DrawText(mode, (int)panel.X + 50, (int)panel.Y + 40, 10, WithAlpha(Color.White, 0.5f));

        float ammoFill = mag > 0 ? ammoInMag / (float)mag : 0f;
        var ammoTrack = new Rectangle(panel.X + 50f, panel.Y + panelH - 12f, panel.Width - 60f, 5f);
        Raylib.DrawRectangleRounded(ammoTrack, 0.8f, 4, WithAlpha(ForestShadow, 0.75f));
        if (reloading)
        {
            float reloadTotal = GetReloadTime(in g);
            float prog = 1f - reloadTimer / reloadTotal;
            var fill = new Rectangle(ammoTrack.X, ammoTrack.Y, ammoTrack.Width * prog, ammoTrack.Height);
            if (fill.Width > 1f)
            {
                Raylib.DrawRectangleRounded(fill, 0.8f, 4, Gold);
            }
        }
        else if (ammoFill > 0f)
        {
            Color fillCol = ammoInMag <= mag / 4 ? Danger : UiAccent;
            var fill = new Rectangle(ammoTrack.X, ammoTrack.Y, ammoTrack.Width * ammoFill, ammoTrack.Height);
            Raylib.DrawRectangleRounded(fill, 0.8f, 4, fillCol);
        }
        Raylib.DrawRectangleRoundedLines(ammoTrack, 0.8f, 4, WithAlpha(UiBorder, 0.35f));
    }

}
