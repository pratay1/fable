partial class Program
{
    // ---------------------------------------------------------------- Progression

    static int XpToNextLevel() => 55 + playerLevel * 38 + playerLevel * playerLevel * 7;

    static void GrantXp(int amount)
    {
        if (amount <= 0) return;
        playerXp += amount;
        while (playerXp >= XpToNextLevel())
        {
            playerXp -= XpToNextLevel();
            playerLevel++;
            levelUpBannerTimer = 2.8f;
            ApplyLevelUnlocks();
            UnlockMottosForLevel();
            SpawnFloatingText(new Vector2(WindowWidth / 2f, 140f), "RANK UP", Gold, 26);
        }
        SaveGame();
    }

    static void ApplyLevelUnlocks()
    {
        for (int i = 0; i < Guns.Length; i++)
        {
            if (Guns[i].LevelReq > 0 && playerLevel >= Guns[i].LevelReq)
            {
                gunUnlocked[i] = true;
            }
        }

        for (int i = 0; i < BodyPalette.Length; i++)
        {
            if (BodyLevelReq[i] > 0 && BodyFableCost[i] == 0 && BodyWaveReq[i] == 0 && playerLevel >= BodyLevelReq[i])
            {
                bodyUnlocked[i] = true;
            }
        }

        for (int i = 0; i < AccessoryNames.Length; i++)
        {
            if (AccessoryLevelReq[i] > 0 && AccessoryFableCost[i] == 0 && AccessoryWaveReq[i] == 0
                && playerLevel >= AccessoryLevelReq[i])
            {
                accessoryUnlocked[i] = true;
            }
        }
    }

    static bool CanPurchaseGun(int i)
    {
        ref readonly Gun g = ref Guns[i];
        if (gunUnlocked[i]) return true;
        if (g.LevelReq > 0) return playerLevel >= g.LevelReq;
        if (g.WaveReq > 0 && maxWaveReached < g.WaveReq) return false;
        return g.FableCost <= 0 || fables >= g.FableCost;
    }

    static bool TryUnlockGun(int i)
    {
        if (gunUnlocked[i]) return true;
        ref readonly Gun g = ref Guns[i];
        if (g.LevelReq > 0)
        {
            if (playerLevel >= g.LevelReq) { gunUnlocked[i] = true; return true; }
            return false;
        }
        if (g.WaveReq > 0 && maxWaveReached < g.WaveReq) return false;
        if (g.FableCost > 0 && fables < g.FableCost) return false;
        if (g.FableCost > 0) fables -= g.FableCost;
        gunUnlocked[i] = true;
        return true;
    }

    static bool CanPurchaseBody(int i)
    {
        if (bodyUnlocked[i]) return true;
        if (BodyLevelReq[i] > 0 && playerLevel < BodyLevelReq[i]) return false;
        if (BodyWaveReq[i] > 0 && maxWaveReached < BodyWaveReq[i]) return false;
        return BodyFableCost[i] <= 0 || fables >= BodyFableCost[i];
    }

    static bool TryUnlockBody(int i)
    {
        if (bodyUnlocked[i]) return true;
        if (!CanPurchaseBody(i)) return false;
        if (BodyFableCost[i] > 0) fables -= BodyFableCost[i];
        bodyUnlocked[i] = true;
        return true;
    }

    static bool IsSecretAccessory(int i) => i == AccessoryCursorCrown;

    static int ArmoryAccessoryCount()
    {
        int n = AccessoryNames.Length;
        if (!accessoryUnlocked[AccessoryCursorCrown]) n--;
        return n;
    }

    static bool CanPurchaseAccessory(int i)
    {
        if (accessoryUnlocked[i]) return true;
        if (IsSecretAccessory(i)) return false;
        if (AccessoryLevelReq[i] > 0 && playerLevel < AccessoryLevelReq[i]) return false;
        if (AccessoryWaveReq[i] > 0 && maxWaveReached < AccessoryWaveReq[i]) return false;
        return AccessoryFableCost[i] <= 0 || fables >= AccessoryFableCost[i];
    }

    static bool TryUnlockAccessory(int i)
    {
        if (accessoryUnlocked[i]) return true;
        if (IsSecretAccessory(i)) return false;
        if (!CanPurchaseAccessory(i)) return false;
        if (AccessoryFableCost[i] > 0) fables -= AccessoryFableCost[i];
        accessoryUnlocked[i] = true;
        return true;
    }

    static void UnlockCursorCrownEasterEgg()
    {
        if (accessoryUnlocked[AccessoryCursorCrown]) return;
        accessoryUnlocked[AccessoryCursorCrown] = true;
        accessoryIndex = AccessoryCursorCrown;
        settingsEggBannerTimer = 5f;
        settingsTypeBuffer = "";
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 168f), "CURSOR CROWN", Gold, 28);
        SpawnFloatingText(new Vector2(WindowWidth / 2f, 198f), "A secret fit for the siege", WithAlpha(Color.White, 0.85f), 14);
        SaveGame();
    }

    static void UpdateSettingsEasterEggInput()
    {
        int ch = Raylib.GetCharPressed();
        while (ch != 0)
        {
            if (ch is >= 32 and < 127)
            {
                char c = char.ToLowerInvariant((char)ch);
                if (char.IsLetter(c))
                {
                    settingsTypeBuffer += c;
                    if (settingsTypeBuffer.Length > 32)
                    {
                        settingsTypeBuffer = settingsTypeBuffer[^32..];
                    }

                    if (settingsTypeBuffer.EndsWith(CursorCrownEasterEgg, StringComparison.Ordinal))
                    {
                        UnlockCursorCrownEasterEgg();
                    }
                }
            }

            ch = Raylib.GetCharPressed();
        }
    }

    static string GunLockLabel(int i)
    {
        ref readonly Gun g = ref Guns[i];
        if (g.LevelReq > 0) return $"RANK {g.LevelReq}";
        if (g.WaveReq > 0 && g.FableCost > 0) return $"WAVE {g.WaveReq}  �  {g.FableCost}";
        if (g.FableCost > 0) return g.FableCost.ToString();
        return "LOCKED";
    }

    static string CosmeticLockLabel(int fableCost, int levelReq, int waveReq)
    {
        if (levelReq > 0 && fableCost == 0 && waveReq == 0) return $"RANK {levelReq}";
        if (waveReq > 0 && fableCost > 0) return $"W{waveReq} � {fableCost}";
        if (fableCost > 0) return fableCost.ToString();
        if (waveReq > 0) return $"WAVE {waveReq}";
        return "";
    }

    static void ResizeUnlockArrays()
    {
        if (gunUnlocked.Length != Guns.Length)
        {
            bool[] old = gunUnlocked;
            gunUnlocked = new bool[Guns.Length];
            Array.Copy(old, gunUnlocked, Math.Min(old.Length, Guns.Length));
            gunUnlocked[0] = true;
            shopBarVis = new float[Guns.Length];
        }

        if (bodyUnlocked.Length != BodyPalette.Length)
        {
            bool[] old = bodyUnlocked;
            bodyUnlocked = new bool[BodyPalette.Length];
            Array.Copy(old, bodyUnlocked, Math.Min(old.Length, BodyPalette.Length));
            bodyUnlocked[0] = bodyUnlocked[1] = bodyUnlocked[2] = true;
        }

        if (accessoryUnlocked.Length != AccessoryNames.Length)
        {
            bool[] old = accessoryUnlocked;
            accessoryUnlocked = new bool[AccessoryNames.Length];
            Array.Copy(old, accessoryUnlocked, Math.Min(old.Length, AccessoryNames.Length));
            accessoryUnlocked[0] = true;
        }

        if (bestiaryKills == null || bestiaryKills.Length != EnemyCatalog.Length)
        {
            int[] old = bestiaryKills ?? Array.Empty<int>();
            bestiaryKills = new int[EnemyCatalog.Length];
            Array.Copy(old, bestiaryKills, Math.Min(old.Length, bestiaryKills.Length));
        }

        if (mottoUnlocked == null || mottoUnlocked.Length != MottoLines.Length)
        {
            bool[] old = mottoUnlocked ?? Array.Empty<bool>();
            mottoUnlocked = new bool[MottoLines.Length];
            Array.Copy(old, mottoUnlocked, Math.Min(old.Length, mottoUnlocked.Length));
        }

        if (difficultyRecords == null || difficultyRecords.Length != Enum.GetValues<Difficulty>().Length)
        {
            difficultyRecords = new DifficultyRecord[Enum.GetValues<Difficulty>().Length];
        }
    }

}
