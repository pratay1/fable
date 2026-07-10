partial class Program
{
    // ---------------------------------------------------------------- Save / Load

    static void EnsureSaveStorage()
    {
        Directory.CreateDirectory(SaveDirectory);
        MigrateLegacySaveFile("fable_save.txt", SavePath);
        MigrateLegacySaveFile("fable_progress_v2.flag", ProgressResetFlagPath);
    }

    static void MigrateLegacySaveFile(string fileName, string destination)
    {
        if (File.Exists(destination)) return;

        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string[] candidates =
        {
            Path.Combine(Environment.CurrentDirectory, fileName),
            Path.Combine(projectRoot, fileName),
            Path.Combine(AppContext.BaseDirectory, fileName),
        };

        foreach (string candidate in candidates)
        {
            if (!File.Exists(candidate)) continue;
            File.Copy(candidate, destination, overwrite: false);
            return;
        }
    }

    static bool ShouldResetProgressOnce()
    {
        try
        {
            EnsureSaveStorage();
            if (File.Exists(ProgressResetFlagPath)) return false;
            File.WriteAllText(ProgressResetFlagPath, "1");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
        return true;
    }

    static void ResetProgress()
    {
        fables = 0;
        highScore = 0;
        playerLevel = 1;
        playerXp = 0;
        maxWaveReached = 0;
        equippedGun = 0;
        bodyColorIndex = 0;
        accessoryIndex = 0;
        runDifficulty = Difficulty.Knight;
        difficultyMenuIndex = (int)Difficulty.Knight;
        shakeScale = 1f;
        flashEnabled = true;
        uhdEnabled = true;
        uhdShaders = true;
        autoFire = true;
        fireKey = KeyboardKey.J;
        abilityKey1 = KeyboardKey.Q;
        abilityKey2 = KeyboardKey.Space;
        abilitySlot1 = AbilityType.Paralyze;
        abilitySlot2 = AbilityType.WindStep;
        showControlLegend = true;
        floatingTextEnabled = true;
        reduceMotion = false;
        vignetteScale = 1f;
        particleDensity = 1f;
        backgroundMotes = true;
        menuCastleEnabled = true;
        showFps = false;
        fpsCap = 0;
        showTopHud = true;
        showWeaponHud = true;
        showAbilityHud = true;
        showComboMeter = true;
        showWaveBanner = true;
        showFloorEventHud = true;
        showEventWarningBorder = true;
        showEnemyTelegraphs = true;
        showEnemyHealthBars = true;
        showBossHud = true;
        showLevelUpBanner = true;
        lockCursorInGame = true;
        hitStopEnabled = true;
        pauseOnFocusLoss = false;
        filmGrainScale = 1f;
        bloomScale = 1f;
        Array.Fill(abilityUnlocked, false);
        abilityUnlocked[(int)AbilityType.Paralyze] = true;
        abilityUnlocked[(int)AbilityType.WindStep] = true;

        Array.Fill(gunUnlocked, false);
        gunUnlocked[0] = true;
        Array.Fill(upgradeLevels, 0);
        Array.Fill(bodyUnlocked, false);
        bodyUnlocked[0] = bodyUnlocked[1] = bodyUnlocked[2] = true;
        Array.Fill(accessoryUnlocked, false);
        accessoryUnlocked[0] = true;

        SaveGame();
    }

    static void SaveGame()
    {
        try
        {
            EnsureSaveStorage();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("fables=" + fables);
            sb.AppendLine("high=" + highScore);
            sb.AppendLine("level=" + playerLevel);
            sb.AppendLine("xp=" + playerXp);
            sb.AppendLine("maxwave=" + maxWaveReached);
            sb.AppendLine("gun=" + equippedGun);
            sb.AppendLine("body=" + bodyColorIndex);
            sb.AppendLine("acc=" + accessoryIndex);
            sb.AppendLine("gunU=" + JoinBools(gunUnlocked));
            sb.AppendLine("bodyU=" + JoinBools(bodyUnlocked));
            sb.AppendLine("accU=" + JoinBools(accessoryUnlocked));
            sb.AppendLine("upg=" + JoinInts(upgradeLevels));
            sb.AppendLine("shake=" + shakeScale.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("flash=" + (flashEnabled ? 1 : 0));
            sb.AppendLine("uhd=" + (uhdEnabled ? 1 : 0));
            sb.AppendLine("uhdsh=" + (uhdShaders ? 1 : 0));
            sb.AppendLine("autofire=" + (autoFire ? 1 : 0));
            sb.AppendLine("firekey=" + (int)fireKey);
            sb.AppendLine("abkey1=" + (int)abilityKey1);
            sb.AppendLine("abkey2=" + (int)abilityKey2);
            sb.AppendLine("abslot1=" + (int)abilitySlot1);
            sb.AppendLine("abslot2=" + (int)abilitySlot2);
            sb.AppendLine("abU=" + JoinBools(abilityUnlocked));
            sb.AppendLine("ctrllegend=" + (showControlLegend ? 1 : 0));
            sb.AppendLine("floattext=" + (floatingTextEnabled ? 1 : 0));
            sb.AppendLine("reducemotion=" + (reduceMotion ? 1 : 0));
            sb.AppendLine("vignette=" + vignetteScale.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("partdens=" + particleDensity.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("motes=" + (backgroundMotes ? 1 : 0));
            sb.AppendLine("menucastle=" + (menuCastleEnabled ? 1 : 0));
            sb.AppendLine("showfps=" + (showFps ? 1 : 0));
            sb.AppendLine("fpscap=" + fpsCap);
            sb.AppendLine("hudtop=" + (showTopHud ? 1 : 0));
            sb.AppendLine("hudwpn=" + (showWeaponHud ? 1 : 0));
            sb.AppendLine("hudab=" + (showAbilityHud ? 1 : 0));
            sb.AppendLine("hudcombo=" + (showComboMeter ? 1 : 0));
            sb.AppendLine("hudwave=" + (showWaveBanner ? 1 : 0));
            sb.AppendLine("hudevent=" + (showFloorEventHud ? 1 : 0));
            sb.AppendLine("hudevborder=" + (showEventWarningBorder ? 1 : 0));
            sb.AppendLine("hudtel=" + (showEnemyTelegraphs ? 1 : 0));
            sb.AppendLine("hudhp=" + (showEnemyHealthBars ? 1 : 0));
            sb.AppendLine("hudboss=" + (showBossHud ? 1 : 0));
            sb.AppendLine("hudlvlup=" + (showLevelUpBanner ? 1 : 0));
            sb.AppendLine("lockcur=" + (lockCursorInGame ? 1 : 0));
            sb.AppendLine("hitstop=" + (hitStopEnabled ? 1 : 0));
            sb.AppendLine("pausefocus=" + (pauseOnFocusLoss ? 1 : 0));
            sb.AppendLine("filmgrain=" + filmGrainScale.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("bloom=" + bloomScale.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("difficulty=" + (int)runDifficulty);
            sb.AppendLine("bestiary=" + JoinInts(bestiaryKills));
            sb.AppendLine("mottos=" + JoinBools(mottoUnlocked));
            sb.AppendLine("heraldrypat=" + (heraldryPatterns ? 1 : 0));
            for (int i = 0; i < difficultyRecords.Length; i++)
            {
                ref DifficultyRecord r = ref difficultyRecords[i];
                sb.AppendLine($"rec{i}={r.BestWave},{r.BestScore},{r.BestKills}");
            }
            for (int i = 0; i < ChronicleBuffer.Length; i++)
            {
                if (!string.IsNullOrEmpty(ChronicleBuffer[i]))
                    sb.AppendLine("chron" + i + "=" + ChronicleBuffer[i]);
            }
            File.WriteAllText(SavePath, sb.ToString());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Saving is best-effort; ignore disk failures.
        }
    }

    static void LoadGame()
    {
        try
        {
            EnsureSaveStorage();
            if (!File.Exists(SavePath))
            {
                return;
            }

            foreach (string raw in File.ReadAllLines(SavePath))
            {
                int eq = raw.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                string key = raw.Substring(0, eq);
                string val = raw.Substring(eq + 1);

                switch (key)
                {
                    case "fables": int.TryParse(val, out fables); break;
                    case "high": int.TryParse(val, out highScore); break;
                    case "level": int.TryParse(val, out playerLevel); break;
                    case "xp": int.TryParse(val, out playerXp); break;
                    case "maxwave": int.TryParse(val, out maxWaveReached); break;
                    case "gun": int.TryParse(val, out equippedGun); break;
                    case "body": int.TryParse(val, out bodyColorIndex); break;
                    case "acc": int.TryParse(val, out accessoryIndex); break;
                    case "gunU": ReadBools(val, gunUnlocked); break;
                    case "bodyU": ReadBools(val, bodyUnlocked); break;
                    case "accU": ReadBools(val, accessoryUnlocked); break;
                    case "upg": ReadInts(val, upgradeLevels); break;
                    case "shake": float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out shakeScale); break;
                    case "flash": flashEnabled = val.Trim() == "1"; break;
                    case "uhd": uhdEnabled = val.Trim() == "1"; break;
                    case "uhdsh": uhdShaders = val.Trim() == "1"; break;
                    case "autofire": autoFire = val.Trim() == "1"; break;
                    case "firekey": if (int.TryParse(val, out int fk)) fireKey = (KeyboardKey)fk; break;
                    case "abkey1": if (int.TryParse(val, out int ak1)) abilityKey1 = (KeyboardKey)ak1; break;
                    case "abkey2": if (int.TryParse(val, out int ak2)) abilityKey2 = (KeyboardKey)ak2; break;
                    case "abslot1": if (int.TryParse(val, out int as1)) abilitySlot1 = (AbilityType)Math.Clamp(as1, 0, AbilityCount - 1); break;
                    case "abslot2": if (int.TryParse(val, out int as2)) abilitySlot2 = (AbilityType)Math.Clamp(as2, 0, AbilityCount - 1); break;
                    case "abU": ReadBools(val, abilityUnlocked); break;
                    case "ctrllegend": showControlLegend = val.Trim() == "1"; break;
                    case "floattext": floatingTextEnabled = val.Trim() == "1"; break;
                    case "reducemotion": reduceMotion = val.Trim() == "1"; break;
                    case "vignette": float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out vignetteScale); break;
                    case "partdens": float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out particleDensity); break;
                    case "motes": backgroundMotes = val.Trim() == "1"; break;
                    case "menucastle": menuCastleEnabled = val.Trim() == "1"; break;
                    case "showfps": showFps = val.Trim() == "1"; break;
                    case "fpscap": int.TryParse(val, out fpsCap); break;
                    case "hudtop": showTopHud = val.Trim() == "1"; break;
                    case "hudwpn": showWeaponHud = val.Trim() == "1"; break;
                    case "hudab": showAbilityHud = val.Trim() == "1"; break;
                    case "hudcombo": showComboMeter = val.Trim() == "1"; break;
                    case "hudwave": showWaveBanner = val.Trim() == "1"; break;
                    case "hudevent": showFloorEventHud = val.Trim() == "1"; break;
                    case "hudevborder": showEventWarningBorder = val.Trim() == "1"; break;
                    case "hudtel": showEnemyTelegraphs = val.Trim() == "1"; break;
                    case "hudhp": showEnemyHealthBars = val.Trim() == "1"; break;
                    case "hudboss": showBossHud = val.Trim() == "1"; break;
                    case "hudlvlup": showLevelUpBanner = val.Trim() == "1"; break;
                    case "lockcur": lockCursorInGame = val.Trim() == "1"; break;
                    case "hitstop": hitStopEnabled = val.Trim() == "1"; break;
                    case "pausefocus": pauseOnFocusLoss = val.Trim() == "1"; break;
                    case "filmgrain": float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out filmGrainScale); break;
                    case "bloom": float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out bloomScale); break;
                    case "difficulty":
                        if (int.TryParse(val, out int diffRaw))
                        {
                            runDifficulty = (Difficulty)Math.Clamp(diffRaw, 0, DifficultyProfiles.Length - 1);
                            difficultyMenuIndex = (int)runDifficulty;
                        }
                        break;
                    case "bestiary": ReadInts(val, bestiaryKills); break;
                    case "mottos": ReadBools(val, mottoUnlocked); break;
                    case "heraldrypat": heraldryPatterns = val.Trim() == "1"; break;
                    default:
                        if (key.StartsWith("rec", StringComparison.Ordinal) && int.TryParse(key.AsSpan(3), out int rdi)
                            && rdi >= 0 && rdi < difficultyRecords.Length)
                        {
                            string[] parts = val.Split(',');
                            if (parts.Length >= 3)
                            {
                                int.TryParse(parts[0], out difficultyRecords[rdi].BestWave);
                                int.TryParse(parts[1], out difficultyRecords[rdi].BestScore);
                                int.TryParse(parts[2], out difficultyRecords[rdi].BestKills);
                            }
                        }
                        else if (key.StartsWith("chron", StringComparison.Ordinal) && int.TryParse(key.AsSpan(5), out int ci)
                            && ci >= 0 && ci < ChronicleBuffer.Length)
                        {
                            ChronicleBuffer[ci] = val;
                            chronicleCount = Math.Max(chronicleCount, ci + 1);
                        }
                        break;
                }
            }

            equippedGun = Math.Clamp(equippedGun, 0, Guns.Length - 1);
            bodyColorIndex = Math.Clamp(bodyColorIndex, 0, BodyPalette.Length - 1);
            accessoryIndex = Math.Clamp(accessoryIndex, 0, AccessoryNames.Length - 1);
            gunUnlocked[0] = true;
            bodyUnlocked[0] = true;
            accessoryUnlocked[0] = true;
            abilityUnlocked[(int)AbilityType.Paralyze] = true;
            abilityUnlocked[(int)AbilityType.WindStep] = true;
            abilitySlot1 = SanitizeAbilitySlot(abilitySlot1);
            abilitySlot2 = SanitizeAbilitySlot(abilitySlot2);
            abilitySlotTracked[0] = abilitySlot1;
            abilitySlotTracked[1] = abilitySlot2;
            abilityFillVis[0] = AbilityReadiness(abilitySlot1);
            abilityFillVis[1] = AbilityReadiness(abilitySlot2);
            playerLevel = Math.Max(1, playerLevel);
            shakeScale = Math.Clamp(shakeScale, 0f, 1f);
            vignetteScale = Math.Clamp(vignetteScale, 0f, 1f);
            particleDensity = Math.Clamp(particleDensity, 0.15f, 1f);
            filmGrainScale = Math.Clamp(filmGrainScale, 0f, 1f);
            bloomScale = Math.Clamp(bloomScale, 0f, 1f);
            fpsCap = fpsCap switch { 60 or 120 or 144 => fpsCap, _ => 0 };
            ApplyFpsCap();
            UnlockMottosForLevel();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable save; fall back to defaults already set.
        }
    }

    static void ApplyFpsCap() => Raylib.SetTargetFPS(fpsCap <= 0 ? TargetFps : fpsCap);

    static int ParticleCap => Math.Max(80, (int)(MaxParticles * particleDensity));

    static void AddParticle(Particle p)
    {
        if (particleDensity <= 0.01f) return;
        if (particleDensity < 0.999f && Rng.NextSingle() > particleDensity) return;
        particles.Add(p);
    }

    static string FpsCapLabel() => fpsCap switch
    {
        60 => "60 FPS",
        120 => "120 FPS",
        144 => "144 FPS",
        _ => "UNLIMITED",
    };

    static void CycleFpsCap()
    {
        fpsCap = fpsCap switch
        {
            0 => 60,
            60 => 120,
            120 => 144,
            _ => 0,
        };
        ApplyFpsCap();
        SaveGame();
    }

    static string JoinBools(bool[] a)
    {
        var parts = new string[a.Length];
        for (int i = 0; i < a.Length; i++) parts[i] = a[i] ? "1" : "0";
        return string.Join(',', parts);
    }

    static string JoinInts(int[] a) => string.Join(',', a);

    static void ReadBools(string s, bool[] target)
    {
        string[] parts = s.Split(',');
        for (int i = 0; i < target.Length && i < parts.Length; i++)
        {
            target[i] = parts[i].Trim() == "1";
        }
    }

    static void ReadInts(string s, int[] target)
    {
        string[] parts = s.Split(',');
        for (int i = 0; i < target.Length && i < parts.Length; i++)
        {
            int.TryParse(parts[i], out target[i]);
        }
    }

}
