partial class Program
{
    // ---------------------------------------------------------------- Settings

    static float SettingsScrollMax()
    {
        const float rowH = 52f;
        const float gap = 8f;
        const float sectionGap = 8f;
        const float sectionLabel = 20f;
        ReadOnlySpan<int> sectionRows = stackalloc int[] { 7, 3, 4, 11, 7, 2 };
        float contentH = 0f;
        for (int s = 0; s < sectionRows.Length; s++)
        {
            contentH += sectionLabel + gap + (rowH + gap) * sectionRows[s];
            contentH += sectionGap;
        }

        float viewportH = UiFooterTop - 88f;
        return Math.Max(0f, contentH - viewportH);
    }

    static void DrawSettingsToggleRow(ref float y, float startX, float rowW, float rowH, float gap, string title, string desc, ref bool value)
    {
        var r = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText(title, (int)r.X + 18, (int)r.Y + 10, 20, Color.White);
        DrawTextTruncated(desc, (int)r.X + 18, (int)r.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(r.X + rowW - 130f, r.Y + 12f, 114f, 32f), value ? "ON" : "OFF", 16, true, value ? UiAccent : UiBorder))
        {
            value = !value;
            SaveGame();
        }

        y += rowH + gap;
    }

    static void DrawSettingsSliderRow(ref float y, float startX, float rowW, float rowH, float gap, string title, ref float value, Color accent)
    {
        var r = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText(title, (int)r.X + 18, (int)r.Y + 10, 20, Color.White);
        Raylib.DrawText((int)(value * 100f) + "%", (int)r.X + 18, (int)r.Y + 34, 12, WithAlpha(accent, 0.9f));
        float nv = Slider(new Rectangle(r.X + 230f, r.Y + rowH / 2f - 6f, rowW - 260f, 12f), value);
        if (MathF.Abs(nv - value) > 0.001f)
        {
            value = nv;
            SaveGame();
        }

        y += rowH + gap;
    }

    static void DrawSettings()
    {
        int cx = WindowWidth / 2;
        DrawScreenBackdrop(0.66f);

        DrawUiScreenHeader("SETTINGS", settingsReturnState == GameState.Paused
            ? "Changes apply immediately to the paused run."
            : "Controls, comfort, and visual quality.");

        float rowW = 560f, rowH = 52f, gap = 8f;
        const float sectionGap = 8f;
        float startX = cx - rowW / 2f;
        float contentTop = 88f;
        float y = contentTop - settingsScroll;
        settingsScroll = Math.Clamp(settingsScroll, 0f, SettingsScrollMax());

        var clip = new Rectangle(startX - 8f, contentTop, rowW + 16f, UiFooterTop - contentTop);
        Raylib.BeginScissorMode((int)clip.X, (int)clip.Y, (int)clip.Width, (int)clip.Height);

        DrawUiSectionLabel("GAMEPLAY", startX, y, UiAccent);
        y += 20f + gap;

        var r1 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r1, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Auto-Fire", (int)r1.X + 18, (int)r1.Y + 10, 20, Color.White);
        DrawTextTruncated("Auto-aims at foes. Earns 25% fewer fables.", (int)r1.X + 18, (int)r1.Y + 34, (int)rowW - 160, 11,
            WithAlpha(autoFire ? new Color(196, 168, 108, 255) : Color.White, autoFire ? 0.9f : 0.5f));
        if (Button(new Rectangle(r1.X + rowW - 130f, r1.Y + 12f, 114f, 32f), autoFire ? "ON" : "OFF", 16, true, autoFire ? UiAccent : UiBorder))
        {
            autoFire = !autoFire;
            SaveGame();
        }

        y += rowH + gap;
        var r5 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r5, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Survival Pilot", (int)r5.X + 18, (int)r5.Y + 10, 20, Color.White);
        DrawTextTruncated("Press \\ in-game to toggle.", (int)r5.X + 18, (int)r5.Y + 34, (int)rowW - 160, 11, WithAlpha(new Color(196, 168, 108, 255), 0.85f));

        y += rowH + gap;
        var rLegend = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rLegend, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Control Hints", (int)rLegend.X + 18, (int)rLegend.Y + 10, 20, Color.White);
        DrawTextTruncated("Wave 1 overlay with move, fire, and skill keys.", (int)rLegend.X + 18, (int)rLegend.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rLegend.X + rowW - 130f, rLegend.Y + 12f, 114f, 32f), showControlLegend ? "ON" : "OFF", 16, true, showControlLegend ? UiAccent : UiBorder))
        {
            showControlLegend = !showControlLegend;
            SaveGame();
        }

        y += rowH + gap;
        var rFloat = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rFloat, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Damage Numbers", (int)rFloat.X + 18, (int)rFloat.Y + 10, 20, Color.White);
        DrawTextTruncated("Floating combat text for hits, pickups, and alerts.", (int)rFloat.X + 18, (int)rFloat.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rFloat.X + rowW - 130f, rFloat.Y + 12f, 114f, 32f), floatingTextEnabled ? "ON" : "OFF", 16, true, floatingTextEnabled ? UiAccent : UiBorder))
        {
            floatingTextEnabled = !floatingTextEnabled;
            SaveGame();
        }

        y += rowH + gap;
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Hit-Stop", "Brief freeze on heavy hits for impact feel.", ref hitStopEnabled);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Heraldry Patterns", "Diagonal hatch on body colors for colorblind clarity.", ref heraldryPatterns);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Lock Cursor", "Hide and lock the mouse while playing.", ref lockCursorInGame);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Pause When Unfocused", "Auto-pause when you alt-tab away.", ref pauseOnFocusLoss);

        y += sectionGap;
        DrawUiSectionLabel("CONTROLS", startX, y, MossLight);
        y += 20f + gap;

        var r2 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r2, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Fire Key", (int)r2.X + 18, (int)r2.Y + 10, 20, Color.White);
        DrawTextTruncated("Extra fire key. LMB fires, RMB reloads.", (int)r2.X + 18, (int)r2.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        string keyLabel = rebindTarget == RebindTarget.Fire ? "PRESS KEY" : KeyName(fireKey);
        if (Button(new Rectangle(r2.X + rowW - 130f, r2.Y + 12f, 114f, 32f), keyLabel, 16, true, rebindTarget == RebindTarget.Fire ? Gold : UiBorder))
        {
            rebindTarget = RebindTarget.Fire;
        }

        y += rowH + gap;
        var rAb1 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rAb1, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Ability 1 Key", (int)rAb1.X + 18, (int)rAb1.Y + 10, 20, Color.White);
        DrawTextTruncated("Slot 1: " + AbilityNames[(int)abilitySlot1], (int)rAb1.X + 18, (int)rAb1.Y + 34, (int)rowW - 160, 11, WithAlpha(AbilityAccent(abilitySlot1), 0.9f));
        string ab1Label = rebindTarget == RebindTarget.Ability1 ? "PRESS KEY" : KeyName(abilityKey1);
        if (Button(new Rectangle(rAb1.X + rowW - 130f, rAb1.Y + 12f, 114f, 32f), ab1Label, 16, true, rebindTarget == RebindTarget.Ability1 ? Gold : UiBorder))
        {
            rebindTarget = RebindTarget.Ability1;
        }

        y += rowH + gap;
        var rAb2 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rAb2, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Ability 2 Key", (int)rAb2.X + 18, (int)rAb2.Y + 10, 20, Color.White);
        DrawTextTruncated("Slot 2: " + AbilityNames[(int)abilitySlot2], (int)rAb2.X + 18, (int)rAb2.Y + 34, (int)rowW - 160, 11, WithAlpha(AbilityAccent(abilitySlot2), 0.9f));
        string ab2Label = rebindTarget == RebindTarget.Ability2 ? "PRESS KEY" : KeyName(abilityKey2);
        if (Button(new Rectangle(rAb2.X + rowW - 130f, rAb2.Y + 12f, 114f, 32f), ab2Label, 16, true, rebindTarget == RebindTarget.Ability2 ? Gold : UiBorder))
        {
            rebindTarget = RebindTarget.Ability2;
        }

        y += rowH + gap + sectionGap;
        DrawUiSectionLabel("COMFORT", startX, y, Gold);
        y += 20f + gap;

        var r3 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r3, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Screen Shake", (int)r3.X + 18, (int)r3.Y + 10, 20, Color.White);
        Raylib.DrawText((int)(shakeScale * 100f) + "%", (int)r3.X + 18, (int)r3.Y + 34, 12, WithAlpha(UiAccent, 0.9f));
        float ns = Slider(new Rectangle(r3.X + 230f, r3.Y + rowH / 2f - 6f, rowW - 260f, 12f), shakeScale);
        if (MathF.Abs(ns - shakeScale) > 0.001f) { shakeScale = ns; SaveGame(); }

        y += rowH + gap;
        var r4 = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(r4, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Screen Flash", (int)r4.X + 18, (int)r4.Y + 10, 20, Color.White);
        DrawTextTruncated("Impact flashes on hits and deaths.", (int)r4.X + 18, (int)r4.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(r4.X + rowW - 130f, r4.Y + 12f, 114f, 32f), flashEnabled ? "ON" : "OFF", 16, true, flashEnabled ? UiAccent : UiBorder))
        {
            flashEnabled = !flashEnabled;
            SaveGame();
        }

        y += rowH + gap;
        var rMotion = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rMotion, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Reduce Motion", (int)rMotion.X + 18, (int)rMotion.Y + 10, 20, Color.White);
        DrawTextTruncated("Less shake, hit-stop, zoom punch, and pulse effects.", (int)rMotion.X + 18, (int)rMotion.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rMotion.X + rowW - 130f, rMotion.Y + 12f, 114f, 32f), reduceMotion ? "ON" : "OFF", 16, true, reduceMotion ? UiAccent : UiBorder))
        {
            reduceMotion = !reduceMotion;
            SaveGame();
        }

        y += rowH + gap;
        var rVig = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rVig, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Vignette Intensity", (int)rVig.X + 18, (int)rVig.Y + 10, 20, Color.White);
        Raylib.DrawText((int)(vignetteScale * 100f) + "%", (int)rVig.X + 18, (int)rVig.Y + 34, 12, WithAlpha(Gold, 0.9f));
        float nv = Slider(new Rectangle(rVig.X + 230f, rVig.Y + rowH / 2f - 6f, rowW - 260f, 12f), vignetteScale);
        if (MathF.Abs(nv - vignetteScale) > 0.001f) { vignetteScale = nv; SaveGame(); }

        y += rowH + gap + sectionGap;
        DrawUiSectionLabel("HUD", startX, y, new Color(148, 196, 220, 255));
        y += 20f + gap;

        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Top Bar", "Score, wave, and fables across the top.", ref showTopHud);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Weapon Panel", "Equipped arm, ammo count, and reload bar.", ref showWeaponHud);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Ability Bar", "Skill cooldowns and readiness at the bottom.", ref showAbilityHud);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Combo Meter", "Combo multiplier banner during chains.", ref showComboMeter);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Wave Banner", "Large wave title when a new swarm begins.", ref showWaveBanner);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Event HUD", "Active floor catastrophe name and timer.", ref showFloorEventHud);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Event Border", "Pulsing screen edge during catastrophes.", ref showEventWarningBorder);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Attack Telegraphs", "Warnings before enemy tile breaks and dashes.", ref showEnemyTelegraphs);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Enemy Health Bars", "HP bars above damaged foes.", ref showEnemyHealthBars);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Boss Health Bar", "Dedicated boss HP panel at the top.", ref showBossHud);
        DrawSettingsToggleRow(ref y, startX, rowW, rowH, gap, "Level-Up Banner", "Rank-up toast when you gain a level.", ref showLevelUpBanner);

        y += sectionGap;
        DrawUiSectionLabel("VISUAL QUALITY", startX, y, UiBorderLight);
        y += 20f + gap;

        var rUhd = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rUhd, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("UHD Rendering", (int)rUhd.X + 18, (int)rUhd.Y + 10, 20, Color.White);
        DrawTextTruncated("2x supersampling for a sharper arena.", (int)rUhd.X + 18, (int)rUhd.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rUhd.X + rowW - 130f, rUhd.Y + 12f, 114f, 32f), uhdEnabled ? "ON" : "OFF", 16, true, uhdEnabled ? UiAccent : UiBorder))
        {
            uhdEnabled = !uhdEnabled;
            if (gfxReady) RecreateGfxTargets();
            SaveGame();
        }

        y += rowH + gap;
        var rSh = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rSh, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("UHD Shaders", (int)rSh.X + 18, (int)rSh.Y + 10, 20, Color.White);
        DrawTextTruncated("Bloom, sharpen, vignette, grain.", (int)rSh.X + 18, (int)rSh.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rSh.X + rowW - 130f, rSh.Y + 12f, 114f, 32f), uhdShaders ? "ON" : "OFF", 16, true, uhdShaders ? UiAccent : UiBorder))
        {
            uhdShaders = !uhdShaders;
            LoadUhdShaders();
            SaveGame();
        }

        y += rowH + gap;
        var rPart = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rPart, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Particle Effects", (int)rPart.X + 18, (int)rPart.Y + 10, 20, Color.White);
        Raylib.DrawText((int)(particleDensity * 100f) + "%", (int)rPart.X + 18, (int)rPart.Y + 34, 12, WithAlpha(UiAccent, 0.9f));
        float np = Slider(new Rectangle(rPart.X + 230f, rPart.Y + rowH / 2f - 6f, rowW - 260f, 12f), (particleDensity - 0.15f) / 0.85f);
        float newParticleDensity = 0.15f + np * 0.85f;
        if (MathF.Abs(newParticleDensity - particleDensity) > 0.001f) { particleDensity = newParticleDensity; SaveGame(); }

        y += rowH + gap;
        var rMotes = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rMotes, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Background Motes", (int)rMotes.X + 18, (int)rMotes.Y + 10, 20, Color.White);
        DrawTextTruncated("Drifting leaves and fireflies behind the arena.", (int)rMotes.X + 18, (int)rMotes.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rMotes.X + rowW - 130f, rMotes.Y + 12f, 114f, 32f), backgroundMotes ? "ON" : "OFF", 16, true, backgroundMotes ? UiAccent : UiBorder))
        {
            backgroundMotes = !backgroundMotes;
            SaveGame();
        }

        y += rowH + gap;
        var rCastle = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rCastle, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Menu Castle", (int)rCastle.X + 18, (int)rCastle.Y + 10, 20, Color.White);
        DrawTextTruncated("Detailed castle backdrop on the main menu.", (int)rCastle.X + 18, (int)rCastle.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rCastle.X + rowW - 130f, rCastle.Y + 12f, 114f, 32f), menuCastleEnabled ? "ON" : "OFF", 16, true, menuCastleEnabled ? UiAccent : UiBorder))
        {
            menuCastleEnabled = !menuCastleEnabled;
            SaveGame();
        }

        DrawSettingsSliderRow(ref y, startX, rowW, rowH, gap, "Film Grain", ref filmGrainScale, Gold);
        DrawSettingsSliderRow(ref y, startX, rowW, rowH, gap, "Bloom Glow", ref bloomScale, UiAccent);

        y += sectionGap;
        DrawUiSectionLabel("DISPLAY", startX, y, new Color(120, 220, 140, 255));
        y += 20f + gap;

        var rFps = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rFps, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("Show FPS", (int)rFps.X + 18, (int)rFps.Y + 10, 20, Color.White);
        DrawTextTruncated("Performance overlay. F3 also toggles this.", (int)rFps.X + 18, (int)rFps.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rFps.X + rowW - 130f, rFps.Y + 12f, 114f, 32f), showFps ? "ON" : "OFF", 16, true, showFps ? UiAccent : UiBorder))
        {
            showFps = !showFps;
            SaveGame();
        }

        y += rowH + gap;
        var rCap = new Rectangle(startX, y, rowW, rowH);
        DrawRichPanel(rCap, UiPanel, UiBorder, 0.18f);
        Raylib.DrawText("FPS Cap", (int)rCap.X + 18, (int)rCap.Y + 10, 20, Color.White);
        DrawTextTruncated("Limit frame rate to save power or reduce heat.", (int)rCap.X + 18, (int)rCap.Y + 34, (int)rowW - 160, 11, WithAlpha(Color.White, 0.5f));
        if (Button(new Rectangle(rCap.X + rowW - 130f, rCap.Y + 12f, 114f, 32f), FpsCapLabel(), 14, true, UiBorder))
        {
            CycleFpsCap();
        }

        Raylib.EndScissorMode();

        if (settingsEggBannerTimer > 0f)
        {
            float a = Math.Clamp(settingsEggBannerTimer / 5f, 0f, 1f);
            if (settingsEggBannerTimer < 0.8f) a = settingsEggBannerTimer / 0.8f;
            int bannerCx = WindowWidth / 2;
            var banner = new Rectangle(bannerCx - 220f, 92f, 440f, 72f);
            DrawRichPanel(banner, WithAlpha(UiPanel, a * 0.95f), WithAlpha(Gold, a), 0.2f, accentStripe: true);
            DrawPulseFrame(banner, Gold, 0.14f, 3f, 0.12f * a);
            ShadowTextCentered("CURSOR CROWN UNLOCKED", bannerCx, 108, 22, Gold, a);
            ShadowTextCentered("Equip it in the Armory � Look tab", bannerCx, 134, 12, WithAlpha(Color.White, 0.72f), a);
            var iconCenter = new Vector2(bannerCx, banner.Y + banner.Height + 38f);
            DrawAccessory(iconCenter, 28f, frameTime, AccessoryCursorCrown, AccessoryPreviewForward);
        }

        string centerHint = rebindTarget != RebindTarget.None ? "Listening..." : SettingsScrollMax() > 1f ? "Scroll for more" : "F3 toggles FPS overlay";
        DrawUiHintBar("Click to rebind keys", centerHint, settingsReturnState == GameState.Paused ? "ESC pause" : "ESC back");

        if (Button(new Rectangle(30f, UiBackButtonY, 180f, 40f), "BACK  [ESC]", 20, true, UiBorder))
        {
            SaveGame();
            state = settingsReturnState;
        }
    }

}
